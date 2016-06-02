//----------------------------------------------------------------------------- 
// <copyright file=FileSystemScanCommandBase">
//   Copyright (c) Frederic Thevenet. All Rights Reserved
// </copyright>
// <author>Frederic Thevenet</author>
// <license>
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// </license>
//----------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Eclair.Exceptions;
using Eclair.Tools;
using System.Timers;
using System.ComponentModel;

namespace Eclair.Commands.FileSystem
{
    /// <summary>
    /// Provides a base implementation for commands whose execution revolves around scanning through a file system
    /// and apply the same operations to all of the retrieved files. This is an abstract class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Commands based on this class take one or more paths (local or UNC) as arguments. If more than one path is provided, 
    /// each of them is processed sequentially.
    /// </para>
    /// <para>
    /// By default the paths provided to a command may point to either a file or a folder. A deriving command can alter this behaviour
    /// by setting the CanProcessFolders and CanProcessFiles properties.
    /// In case a folder is provided, the command will processed all files matching the search pattern in that folder and
    /// its sub folder, recursively. 
    /// </para>
    /// <para>
    /// Commands based on this class can provide better performances by doing the work required for the target files in parallel, 
    /// on multiple threads. 
    /// It is up to the deriving class to ensure that the implementation of the ProcessFile method is thread safe.
    /// To allow parallel processing, a deriving command must set the CanDoParallelProcessing property to true.
    /// If CanDoParallelProcessing is set to true, the IsParallelEnabled property can be used to enable or disable
    /// parallel processing for each execution of the command.
    /// </para>
    /// <para>
    /// Non fatal exceptions that are thrown during the processing of scanned files can be handled inside the loop and be prevented 
    /// from interrupting the command. Through the use of a command line switch, the end-user may specify the maximum number of
    /// non fatal exception that the command will tolerate before stopping its execution.
    /// </para>
    /// </remarks>
    public abstract class FileSystemScanCommandBase : CommandBase
    {
        #region Private fields

        private bool parallel = false;
        private CommandContext context;
        private int currentErrorNumber;
        private bool canDoParallelProcessing = true;
        private bool canProcessFolders = true;
        private bool canProcessFiles = true;
        private int maxErrorNumber = 0;
        private string defaultSearchPattern = "*";
        private int progressTimerInterval = 10000;
        private int scannedFilesNumber = 0;
        private int lastScannedFileNumber = 0;
        private int processedFilesNumber = 0;
        private int progressElapsedMs = 0;
        private BlockingCollection<string> workItemQueue;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        #endregion

        #region Properties

        /// <summary>
        /// An object implementing IIOHelper, that defines methods to manipulate files and directories.
        /// </summary>
        /// <remarks>
        ///  Access to files and directory should be made through this object and not directly via System.IO static methods,
        ///  in order to ensure the support for path longer than 260 if required by the end user ("-longPath" switch).
        /// </remarks>
        protected IIOHelper IOHelper { get; private set; }

        /// <summary>
        /// The number of worker threads spawned to process files.
        /// </summary>
        protected  int WorkerCount { get; set; }
       
        /// <summary>
        /// The capacity of the concurrent queue used to synchronize consumer and producer tasks.
        /// </summary>
        protected int QueueMaxCapacity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command supports processing its load on multiple threads.
        /// </summary>
        /// <remarks>
        /// If this is set to false, multi-threading will always be disabled, regardless of the value of IsParallelEnabled.
        /// </remarks>
        protected bool CanDoParallelProcessing
        { 
            get { return canDoParallelProcessing; }
            set { canDoParallelProcessing = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command accepts a folder to be passed as an argument. 
        /// </summary>
        /// <remarks>
        /// If a folder is passed as an argument, the command will process all files matching the SearchPattern in that folder
        /// and its sub folder, recursively.
        /// </remarks>
        protected bool CanProcessFolders
        {
            get { return canProcessFolders; }
            set { canProcessFolders = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command accepts a single file to be passed as an argument. 
        /// </summary>
        protected bool CanProcessFiles
        {
            get { return canProcessFiles; }
            set { canProcessFiles = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of non fatal exceptions that the command will accept before it stops.
        /// </summary>
        /// <remarks>
        /// Settings this property to a negative number is equivalent to setting it to Int.MaxValue.
        /// </remarks>
        protected int MaxErrorNumber
        {
            get { return maxErrorNumber; }
            set { maxErrorNumber = value; }
        }

        /// <summary>
        /// Gets or sets the search string to match against the names of files in the provided path.
        /// </summary>
        /// <remarks>
        /// This parameter can contain a combination of valid literal path and wildcard (* and ?) characters.
        /// </remarks>
        protected string DefaultSearchPattern
        {
            get { return defaultSearchPattern; }
            set { defaultSearchPattern = value; }
        }

        /// <summary>
        /// Gets or sets the list of paths provided as arguments to the command.
        /// </summary>
        protected IEnumerable<string> PathList { get; set; }
      
        /// <summary>
        /// Gets or sets a value indicating whether parallel processing on multiple threads is enabled for the 
        /// current execution of the command.
        /// </summary>
        protected bool IsParallelEnabled
        {
            get { return CanDoParallelProcessing && parallel; }
            set { parallel = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether detailed messages should be logged.
        /// </summary>        
        protected bool IsVerbose { get; set; }

        #endregion

        #region Public and protected methods 

        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="context">The execution context of the current CommandProcessor instance.</param>
        /// <exception cref="MaxErrorReachedException">
        /// The maximum number of non fatal exception allowed has been reached.
        /// </exception>
        public sealed override void ExecuteCommand(CommandContext context)
        {
            int fileProcessed = 0;                   
            this.context = context;
            this.currentErrorNumber = 0;
            this.initializeCommonParameters(context);
            this.ParseExtraParameters(context);
                     
            if (this.PathList == null || this.PathList.Count() == 0)
                throw new ArgumentException("You must specify at least one valid path.");

            foreach (var sourcePath in this.PathList)
            {
                if (this.context.Environment.CancelPending)
                    break;

                try
                {
                    this.scannedFilesNumber = 0;
                    var elapsed = new PerfMonitorResult();
                    using (PerfMonitor.Create(elapsed))
                    {
                        if (this.context.Environment.CancelPending)
                            return;

                        if (String.IsNullOrEmpty(sourcePath))
                            throw new DirectoryNotFoundException("Invalid path.");

                        if (this.IOHelper.DirectoryExists(sourcePath))
                        {
                            if (!this.CanProcessFolders)
                                throw new CommandExecutionException(this, "This command cannot process a folder");

                            fileProcessed = processFiles(sourcePath, DefaultSearchPattern);
                        }
                        else
                        {
                            if (!((sourcePath.Contains('?') || sourcePath.Contains('*')) || this.IOHelper.FileExists(sourcePath)))
                                throw new FileNotFoundException("Invalid path.");

                            if (!this.CanProcessFiles)
                                throw new CommandExecutionException(this, "This command can only process a single folder.");

                            if (!FailSafeFileBrowser.PathMatchesSearchPattern(sourcePath, this.defaultSearchPattern, false))
                                throw new CommandExecutionException(this, "The provided path does not match the search pattern for the current command: " + defaultSearchPattern);

                            string rootDir = this.IOHelper.GetDirectoryName(sourcePath);
                            string fileName = this.IOHelper.GetFileName(sourcePath);

                            fileProcessed = processFiles(rootDir, fileName);
                        }
                    }
                    double filePerSeconds = Double.PositiveInfinity;
                    if (elapsed.ElapsedTime.TotalSeconds > 0)
                        filePerSeconds = this.scannedFilesNumber / elapsed.ElapsedTime.TotalSeconds;

                    this.OutputInfo("Scanned {0} file(s) in {1}s ({2} files/s)",
                        scannedFilesNumber,
                        elapsed.ElapsedTime.TotalSeconds.ToString("F2"),
                        filePerSeconds.ToString("F2"));

                    this.OutputInfo("Processed {0} file(s) in path \"{1}\" in {2}s{3}",
                        fileProcessed,
                        sourcePath,
                        elapsed.ElapsedTime.TotalSeconds.ToString("F2"),
                        IsParallelEnabled ? " [multi-threading enabled]" : string.Empty);
                
                    if (currentErrorNumber > 0)
                        this.OutputWarning("{0} non fatal errors occurred during processing.", currentErrorNumber);
                }
                catch (MaxErrorReachedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    handleNonFatalException(ex, "Error while processing root folder");
                }
            }
        }

        /// <summary>
        /// Classes deriving from FileSystemScanCommandBase can override this method to parse extra arguments.
        /// </summary>
        /// <param name="context">The execution context of the current CommandProcessor instance.</param>
        protected virtual void ParseExtraParameters(CommandContext context)
        {
        }

        /// <summary>
        /// Implements the core logic for processing a single file. Deriving classes must provided their own implementation.
        /// </summary>
        /// <param name="sourcePath">The path to the file to process.</param>
        /// <returns>The number of file successfully processed.</returns>
        protected abstract int ProcessFile(string sourcePath);

        #endregion

        #region Private methods

        /// <summary>
        /// Parses and initializes the value of the parameters common to all commands.
        /// </summary>
        /// <param name="context">The execution context of the current CommandProcessor instance.</param>
        private void initializeCommonParameters(CommandContext context)
        {
            if (context.CommandLineArguments.ArgumentExists("-longPath"))
                this.IOHelper = new LongPathIOHelper();
            else
                this.IOHelper = new StandardIOHelper();

            this.WorkerCount = context.CommandLineArguments.GetFirstArgument<int>("-w", @"\d*", Environment.ProcessorCount * 2);
            if (this.WorkerCount <= 0)
                this.WorkerCount = 1;

            this.QueueMaxCapacity = context.CommandLineArguments.GetFirstArgument<int>("-q", @"-?\d*", Environment.ProcessorCount * 100);
            if (this.QueueMaxCapacity <= 0)
                this.QueueMaxCapacity = int.MaxValue;

            this.WorkerCount = Math.Min(this.WorkerCount, this.QueueMaxCapacity);

            this.progressTimerInterval = context.CommandLineArguments.GetFirstArgument<int>("-i", @"-?\d*", 10) * 1000;
            this.MaxErrorNumber = context.CommandLineArguments.GetFirstArgument<int>("-e", @"-?\d*", 100);
            if (this.maxErrorNumber < 0)
                this.maxErrorNumber = int.MaxValue;

            this.IsParallelEnabled = !(context.CommandLineArguments.ArgumentExists("-s"));
            this.PathList = this.IOHelper.GetAllPathFromArguments(context.CommandLineArguments);
            this.IsVerbose = context.CommandLineArguments.ArgumentExists("-v");
        }

        /// <summary>
        /// Monitors and logs the time elapsed for processing a file. 
        /// </summary>
        /// <param name="sourcePath">The path to the file to process.</param>
        /// <returns>The number of file successfully processed.</returns>
        private int processFile(string sourcePath)
        {
            using (PerfMonitor.Create(string.Format("Processing single file {0}", sourcePath)))
            {
                return ProcessFile(sourcePath);
            }
        }

        /// <summary>
        /// Loops through all the files in the specified path that matches the search pattern and process them sequentially on the main thread.
        /// </summary>
        /// <param name="rootPath">The path to the folder to process.</param>
        /// <param name="searchPattern">The search pattern used to identify the files to enumerate.</param>
        private void singleThreadProcessFiles(string rootPath, string searchPattern)
        {
            foreach (var sourcePath in this.IOHelper.EnumerateFiles(rootPath, searchPattern, new HandleNonFatalException(handleNonFatalException)))
            {
                if (this.context.Environment.CancelPending)
                    break;
                try
                {
                    this.scannedFilesNumber++;
                    processedFilesNumber += processFile(sourcePath);
                }
                catch (MaxErrorReachedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    handleNonFatalException(ex, "An error occurred while processing file: " + sourcePath);
                }
            }
        }

        /// <summary>
        /// Loops through all the files in the specified path that matches the search pattern, and processes them in parallel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Implements a producer/consumer pattern. The role of the producer consists in queuing file paths enumerated from the main thread
        /// (to avoid concurrency issues) on to a BlockingCollection, while a fixed number of workers are spawned to act as consumers, 
        /// dequeuing file paths from the BlockingCollection concurrently.
        /// </para>
        /// <para>
        /// The max capacity of the BlockingCollection as well as the number of worker assigned to it can be specified for performance tuning.
        /// </para>
        /// </remarks>
        /// <param name="rootPath">The path to the folder to process.</param>
        /// <param name="searchPattern">The search pattern used to identify the files to enumerate.</param>
        private void parallelProcessFiles(string rootPath, string searchPattern)
        {
            this.OutputDebug("Starting parallel file processing");
            this.OutputDebug("Queue Length set to {0}", this.QueueMaxCapacity);

            // initializes the blocking collection
            this.workItemQueue = new BlockingCollection<string>(this.QueueMaxCapacity);

            // Starts consumer tasks
            var consumers = new List<Task>();
            for (int i = 0; i < this.WorkerCount; i++)
            {
                consumers.Add(Task.Factory.StartNew(() =>
                    {
                        foreach (var path in this.workItemQueue.GetConsumingEnumerable())
                        {
                            try
                            {
                                Interlocked.Increment(ref this.scannedFilesNumber);
                                var nbFiles = processFile(path);
                                Interlocked.Add(ref this.processedFilesNumber, nbFiles);
                            }
                            catch (MaxErrorReachedException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                handleNonFatalException(ex, "An error occurred while processing file: " + path);
                            }
                        }
                    },
                    cancellationTokenSource.Token,
                    TaskCreationOptions.None,
                    TaskScheduler.Default));
            }

            // The producer is run on the main thread.
            foreach (var sourcePath in this.IOHelper.EnumerateFiles(rootPath, searchPattern, new HandleNonFatalException(handleNonFatalException)))
            {
                if (this.context.Environment.CancelPending)
                    break;

                try
                {
                    // If the upper bound of the BlockingCollection has been reached, the call to Add will block until some items have been consumed.
                    this.workItemQueue.TryAdd(sourcePath, -1, this.cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            // Alert the consumers that producer has finished its job.
            this.workItemQueue.CompleteAdding();

            try
            {
                // Wait for all consumers to complete their task.
                Task.WaitAll(consumers.ToArray<Task>());
            }
            catch (AggregateException ae)
            {
                // if a MaxErrorReachedException was thrown during the processing of the tasks, re throw it to interrupt the command.
                // Everything else has already been handled.
                foreach (var e in ae.InnerExceptions)
                    if (e is MaxErrorReachedException)
                        throw e;
            }
        }

        /// <summary>
        /// Scans through all the files in the current folder and its subfolders, and invoke the command specific implementation
        /// of ProcessFile for each one of them.
        /// </summary>
        /// <param name="rootPath">The path to the folder to process.</param>
        /// <param name="searchPattern">The search pattern used to identify the files to enumerate.</param>
        /// <returns>The total number of files successfully processed in the folder and its subfolders.</returns>
        /// <exception cref="MaxErrorReachedException">
        /// The maximum number of non fatal exception allowed has been reached.
        /// </exception>
        private int processFiles(string rootPath, string searchPattern)
        {
            if (String.IsNullOrEmpty(rootPath))
                throw new ArgumentNullException(rootPath);

            if (!this.IOHelper.DirectoryExists(rootPath))
                throw new DirectoryNotFoundException("The path specified is not valid");

            System.Timers.Timer progressTimer = new System.Timers.Timer(progressTimerInterval);
            progressTimer.Elapsed += progressTimer_Elapsed;
            processedFilesNumber = 0;
            this.OutputInfo("Processing file(s) \"{0}\\{1}\" ...", rootPath, searchPattern);

            try
            {
                this.progressElapsedMs = 0;
                progressTimer.Start();
                if (IsParallelEnabled)
                    parallelProcessFiles(rootPath, searchPattern);
                else
                    singleThreadProcessFiles(rootPath, searchPattern);
            }
            finally
            {
                progressTimer.Stop();
            }
            return processedFilesNumber;
        }

        /// <summary>
        /// The callback invoked on the timer elapsed event. Displays the number of files processed so far.
        /// </summary>
        /// <param name="sender">The object on which the event originated.</param>
        /// <param name="e">The event's arguments.</param>
        private void progressTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Interlocked.Add(ref this.progressElapsedMs, this.progressTimerInterval);
            var nbFilesSinceLastTick = this.scannedFilesNumber - Interlocked.Exchange(ref this.lastScannedFileNumber, this.scannedFilesNumber);

            double avgFilePerSeconds = double.PositiveInfinity;
            if (this.progressElapsedMs > 0)
                avgFilePerSeconds = this.scannedFilesNumber / (this.progressElapsedMs / 1000);

            double instantFilePerSeconds = double.PositiveInfinity;
            if (this.progressTimerInterval > 0)
                instantFilePerSeconds = nbFilesSinceLastTick / (this.progressTimerInterval/1000);

            if (this.context.Environment.CancelPending)
                this.OutputWarning("Cancel pending...");

            this.OutputInfo("Scanned {0} file(s) in {1}s (inst: {4} f/s, avg: {2} f/s){3}",
                        scannedFilesNumber,
                        (this.progressElapsedMs / 1000).ToString("D"),
                        avgFilePerSeconds.ToString("F0"),
                        this.workItemQueue != null ?
                                String.Format(" [Queued: {0} | Workers: {1}]", this.workItemQueue.Count, this.WorkerCount) : "",
                        instantFilePerSeconds.ToString("F0"));
        }

        /// <summary>
        /// Handles non-fatal exceptions thrown during folders and files enumeration and processing.
        /// </summary>
        /// <param name="ex">The exception to handle.</param>
        /// <param name="errorMessage">The message to display.</param>
        /// <exception cref="MaxErrorReachedException">
        /// The maximum number of non fatal exception allowed has been reached.
        /// </exception>
        private void handleNonFatalException(Exception ex, string errorMessage)
        {
            this.OutputWarning("{0}: {1} - {2}", errorMessage, ex.GetType().Name, ex.Message);
            this.OutputDebug(ex.ToString());

            if (Interlocked.Increment(ref this.currentErrorNumber) > MaxErrorNumber)
            {
                this.cancellationTokenSource.Cancel();
                throw new MaxErrorReachedException();
            }
        }

        #endregion
    }
}
