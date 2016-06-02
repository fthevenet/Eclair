//----------------------------------------------------------------------------- 
// <copyright file=CommandProcessor">
//   Copyright (c) Frederic Thevenet. All Rights Reserved.
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
using System.Reflection;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using Eclair.Commands;
using Eclair.Commands.Scripting;
using Eclair.Exceptions;
using Eclair.CommandLineEditor;
using Eclair.Tools;
using log4net;
using log4net.Config;
using System.Text.RegularExpressions;
using System.Threading;

namespace Eclair.Commands
{
    /// <summary>
    /// Defines an ECLAIR processor that can be used to interpret and execute commands loaded from command libraries.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the class responsible for parsing command lines entered from the shell or read for from a script, 
    /// and then triggers the execution of each command’s payload.
    /// </para>
    /// <para>
    /// From the raw command line, the command processor extracts commands keywords and arguments, as well as 
    /// flow control keywords (pipe, redirection, etc…). 
    /// </para>
    /// <para>
    /// The parsed command data is then sent to the <see cref=" CommandFactory"/> instance which in turn provides the processor 
    /// with instances of the actual classes registered to the keywords in the input command line.
    /// </para>
    /// <para>
    /// Finally the command processor invokes the method that implements the commands payload and provides
    /// it with both the arguments from the command line and the execution context and connection object to the server.
    /// </para>
    /// <para>
    /// An instance of <see cref="CommandProcessor"/> can be used to process commands in an interactive fashion, as they
    /// are entered by an end user, or as a batch when extracted from a script file.
    /// </para>
    /// </remarks>
    public class CommandProcessor : IDisposable
    {
        /// <summary>
        /// Occurs when the commandProcessor is started.
        /// </summary>
        public event EventHandler<CommandProcessorEventArgs> OnStarted;

        /// <summary>
        /// Occurs when the commandProcessor has stopped.
        /// </summary>
        public event EventHandler<CommandProcessorEventArgs> OnStopped;

        /// <summary>
        /// Occurs when the commandProcessor is instructed to stop.
        /// </summary>
        public event EventHandler<CommandProcessorEventArgs> OnStopping;


        /// <summary>
        /// Gets a ShellEnvironment object for the command processor.
        /// </summary>
        public CommandProcessorEnvironment Environment
        {
            get;
            private set;
        }

        private static readonly ILog logger = LogManager.GetLogger(typeof(CommandProcessor));
        private static readonly ILog terminalOutput = LogManager.GetLogger("ECLAIR.Terminal.Output");
        private static readonly ILog terminalInput = LogManager.GetLogger("ECLAIR.Terminal.Input");       

        private IClientProxy clientProxy;

        #region Public static members

        /// <summary>
        /// Gets the output logger for the command processor.
        /// </summary>
        public static ILog TerminalOutput
        {
            get { return terminalOutput; }
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of disposable temp folders for the processor.
        /// </summary>
        public DisposableTempFolderCollection TempFolders {  get; private set; }

        /// <summary>
        /// Gets or sets the list of arguments for the command processor.
        /// </summary>
        public List<string> ArgumentList {get; private set;}

        /// <summary>
        /// Returns true if the argument list contains registered commands.
        /// </summary>
        public bool IsArgsContainCommands
        {
            get { return this.ArgumentList.Any(s => (CommandFactory.Instance.CommandExists(this.Environment.CurrentCategory, s))); }
        }

        /// <summary>
        ///  Returns true if the command processor is forced in interactive mode, False otherwise.
        /// </summary>
        public bool ForceInteractiveMode { get; private set; }

        /// <summary>
        /// Returns true if the command processor is configured to ignore the startup script, False otherwise.
        /// </summary>
        public bool DoNotRunStartUpScript { get; private set; }
   
        /// <summary>
        /// Specifies is the application I/O are being redirected.
        /// </summary>
        /// <remarks>
        /// When running a console application whose input and output are redirected, 
        /// some methods or properties such as Console.Read(true) or Console.Width will fail and throw an exception.
        /// Check the value of this property at runtime to prevent calling to such methods.
        /// </remarks>
        public bool RunInConsole { get; private set; }

        /// <summary>
        /// Returns the connected user.
        /// </summary>
        public string ConnectedUser
        {
            get
            {
                return this.clientProxy.ConnectedUser;
            }
        }

        /// <summary>
        /// Returns the list of connected servers. 
        /// </summary>
        public string Servers
        {
            get
            {
                return this.clientProxy.ServerList;
            }
        }

        #endregion

        #region Ctors

        /// <summary>
        /// Create a new instance of the CommandProcessor class.
        /// </summary>
        /// <param name="runInConsole">Set to True if the command processor runs in the Windows console, False otherwise.</param>
        /// <returns>The newly created CommandProcessor instance.</returns>
        static public CommandProcessor Create(bool runInConsole)
        {
            return new CommandProcessor(runInConsole, new List<string>());
        }

        /// <summary>
        /// Create a new instance of the CommandProcessor class.
        /// </summary>
        /// <param name="runInConsole">Set to True if the command processor runs in the Windows console, False otherwise.</param>
        /// <param name="args">Arguments for the command processor, specified as a list of strings.</param>
        /// <returns>The newly created CommandProcessor instance.</returns>
        static public CommandProcessor Create(bool runInConsole, List<string> args)
        {
            return new CommandProcessor(runInConsole, args);
        }
     
        private CommandProcessor(bool runInConsole, List<string> args)
        {
            // Initialize Log4net if it had not been done be the calling assembly            
            XmlConfigurator.Configure();

            if (args == null)
                throw new ArgumentNullException("args");

            this.ArgumentList = args;
            this.RunInConsole = runInConsole;                     
            this.Environment = new CommandProcessorEnvironment(this);
            this.clientProxy = CommandFactory.Instance.ClientProxyFactory.CreateProxy();

            if (this.ArgumentList.Contains("-forceInteractive", new CiEqualityComparer()))
            {
                this.ForceInteractiveMode = true;
                this.ArgumentList.RemoveAll(s => String.Compare("-forceInteractive", s, true) == 0);

                if (logger.IsDebugEnabled)
                    logger.Debug("-forceInteractive flag is on");
            }

            if (this.ArgumentList.Contains("-noStartUpScript", new CiEqualityComparer()))
            {
                this.DoNotRunStartUpScript = true;
                this.ArgumentList.RemoveAll(s => String.Compare("-noStartUpScript", s, true) == 0);
               
                if (logger.IsDebugEnabled)
                    logger.Debug("-noStartUpScript flag is on");
            }

            if (this.ArgumentList.Contains("-nologo", new CiEqualityComparer()))
            {
                this.ArgumentList.RemoveAll(s => String.Compare(s, "-noLogo", true) == 0);

                if (logger.IsDebugEnabled)
                    logger.Debug("-noLogo flag is on");
            }
            else
            {
                AssemblyInfoRetriever asmInfo = new AssemblyInfoRetriever(Assembly.GetEntryAssembly());
                CommandProcessor.TerminalOutput.InfoFormat("Expandable Command Line Applications Interactive Runtime [Version {0}]", asmInfo.FileVersion);
                CommandProcessor.TerminalOutput.InfoFormat("{0}\n",asmInfo.Copyright);              
            }

            this.setEnvVariables();
            this.TempFolders = new DisposableTempFolderCollection();
          
            try
            {
                if (this.ArgumentList.Exists(s => s.StartsWith("-login:", StringComparison.CurrentCultureIgnoreCase)))
                    this.clientProxy.Connect(this.ArgumentList);
            }
            catch (ServerConnectionException iaex)
            {
                StringBuilder sb = new StringBuilder(string.Format("Login failed: {0}", iaex.Message));
                CommandProcessor.TerminalOutput.Error((formatExceptionMsg(sb, iaex)).ToString());
                logger.Debug(iaex);
            }
            catch (CommandLineInterpretationException cmdLineEx)
            {
                string msg = string.Format("An exception occurred while processing command line");
                CommandProcessor.TerminalOutput.Error((formatExceptionMsg(new StringBuilder(msg), cmdLineEx)).ToString());
                logger.Debug(cmdLineEx);
            }
        }

        #endregion
        
        #region Public methods

        /// <summary>
        /// Starts the command processor in interactive mode.
        /// </summary>
        /// <remarks>
        /// In interactive mode, a the CommandProcessor execute commands that are entered by a user via a command prompt.
        /// This methods only exits when user invokes the "exit" command.
        /// </remarks>
        public void Start()
        {
            try
            {
                this.Environment.RunningMode = CommandProcessorRunningMode.Interactive;
                this.fireOnStartedEvent();
                this.executeStartupScript();
                LineEditor le = null;
                if (this.RunInConsole)
                {
                    le = new LineEditor(this.GetType().Name);
                    le.AutoCompleteEvent = this.completionHandler;
                    le.TabAtStartCompletes = true;
                }

                while (!this.Environment.ExitPending)
                {				   
                    string prompt = (string.Format("{0}@{1}\\{2}>",
                        this.ConnectedUser,
                        this.Servers,
                        this.Environment.CurrentCategory));
                    string line;
                    if (this.RunInConsole)
                        line = le.Edit(prompt, "");
                    else
                    {
                        Console.Write(prompt);
                        line = Console.ReadLine();
                    }
                    CommandProcessor.terminalInput.Info(prompt + line);
                    this.InputCommandLine(line);
                    Console.WriteLine();
                }
                this.fireOnStoppingEvent();
                // Disconnect before exit
                if (this.clientProxy.IsLoggedIn)
                    this.clientProxy.Disconnect();
            }
            finally
            {
                this.fireOnStoppedEvent();
            }
        }

        /// <summary>
        /// Parses a command line specified as a string into a list of arguments.
        /// </summary>
        /// <param name="commandLine">The command line to parse, as a string.</param>
        /// <returns>The parsed command line, a list of strings.</returns>
        public List<string> ParseCommandLine(string commandLine)
        {
            if (string.IsNullOrEmpty(commandLine))
                return new List<string>();

            // Evaluate variables
            commandLine = Regex.Replace(commandLine, @"\%[\w\d\-_]+\%",
                (m) =>
                {
                    string varName = m.Value.Replace("%", "");
                    if (this.Environment.Variables.Keys.Contains(varName))
                        return this.Environment.Variables[varName];
                    return m.Value;
                });

            // Parse line
            string pattern = "([^\\s\\\"]+)|(\\\"[^\\\"]*\\\"?)";
            List<string> argList = Regex.Split(commandLine, pattern)
                                            .Select(s => s.Trim().Replace("\"", ""))
                                            .Where(s => s.Length > 0)
                                            .ToList();			
            if (logger.IsDebugEnabled)
            {
                StringBuilder sb = new StringBuilder("Parsed command line: ");
                argList.ForEach(s => sb.AppendFormat("[{0}] ", s));
                logger.Debug(sb.ToString());
            }
            return argList;
        }

        /// <summary>
        /// Starts the command processor in interpreter mode.
        /// </summary>
        /// <remarks>
        /// In interpreter mode, the command processor executes the commands that are specified as arguments.
        /// The methods returns once the commands are executed.
        /// </remarks>
        public void Run()
        {
            try
            {
                if (ForceInteractiveMode)
                    this.Environment.RunningMode = CommandProcessorRunningMode.Interactive;
                else
                    this.Environment.RunningMode = CommandProcessorRunningMode.Batch;

                this.fireOnStartedEvent();

                this.executeStartupScript();

                using (PerfMonitor.Create("Total execution time"))
                {               
                    this.InterpretScriptLine(ArgumentList, null);
                }
            }
            catch (UnknownCommandException uknEx)
            {
                CommandProcessor.TerminalOutput.Error("Unknown command");

                if (logger.IsDebugEnabled)
                    logger.Debug(uknEx);
            }
            catch (ServerConnectionException iaex)
            {
                CommandProcessor.TerminalOutput.Error((formatExceptionMsg(new StringBuilder("An exception occurred while connecting to IA server"), iaex)).ToString());

                if (logger.IsDebugEnabled)
                    logger.Debug(iaex);
            }
            catch (OutputRedirectionException redirEx)
            {
                string msg = string.Format("An exception occurred while redirecting command output");
                CommandProcessor.TerminalOutput.Error((formatExceptionMsg(new StringBuilder(msg), redirEx)).ToString());

                if (logger.IsDebugEnabled)
                    logger.Debug(redirEx);
            }
            catch (CommandExecutionException ex1)
            {
                string msg = string.Format("An exception occurred while processing command \"{0}\"", ex1.Command != null ? ex1.Command.GetType().Name : "???");
                CommandProcessor.TerminalOutput.Error((formatExceptionMsg(new StringBuilder(msg), ex1)).ToString());

                if (logger.IsDebugEnabled)
                    logger.Debug(ex1);
            }
            finally
            {
                this.fireOnStoppingEvent();
                this.fireOnStoppedEvent();
            }
        }

        /// <summary>
        /// Interprets and executes  a single command line.
        /// </summary>
        /// <param name="line">The command line to interpret, specified as a string.</param>
        public void InterpretCommandLine(string line)
        {
            this.InterpretScriptLine(line, null);
        } 
        
        /// <summary>
        /// Interprets and executes a single command line.
        /// </summary>
        /// <param name="args">The command line to interpret, specified as a list of arguments.</param>
        public void InterpretCommandLine(List<string> args)
        {      
            this.InterpretScriptLine(args, null);
        }

        /// <summary>
        /// Interprets and executes a single script line.
        /// </summary>
        /// <param name="line">The script line to interpret, specified as a string.</param>
        /// <param name="script">Info on the script.</param>
        public void InterpretScriptLine(string line, ScriptInfo script)
        {
            if (string.IsNullOrEmpty(line))
                return;

            this.InterpretScriptLine(this.ParseCommandLine(line), script);
        }

        /// <summary>
        /// Interprets and executes a single script line.
        /// </summary>
        /// <param name="args">The script line to interpret, specified as a list of arguments.</param>
        /// <param name="script">Info on the script.</param>
        public void InterpretScriptLine(List<string> args, ScriptInfo script)
        {
            if (args.Count == 0)
                return;

            this.interpretLine(args, script);
        }

        /// <summary>
        /// Processes an input command line. 
        /// </summary>
        /// <param name="line">The input line to process, specified as a string.</param>
        public void InputCommandLine(string line)
        {
            this.InputCommandLine(this.ParseCommandLine(line));
        }

        /// <summary>
        ///  Processes an input command line. 
        /// </summary>
        /// <param name="args">The input line to process, specified as a list of arguments.</param>
        public void InputCommandLine(List<string> args)
        {
            if (args.Count == 0)
                return;

            try
            {
                this.interpretLine(args, null);
            }
            catch (UnknownCommandException unkEx)
            {
                CommandProcessor.TerminalOutput.Error("Unknown command");

                if (logger.IsDebugEnabled)
                    logger.Debug(unkEx);
            }
            catch (ServerConnectionException iaex)
            {
                CommandProcessor.TerminalOutput.Error((formatExceptionMsg(new StringBuilder("An exception occurred while connecting to IA server"), iaex)).ToString());

                if (logger.IsDebugEnabled)
                    logger.Debug(iaex);
            }
            catch (OutputRedirectionException redirEx)
            {
                string msg = "An exception occurred while redirecting command output";
                CommandProcessor.TerminalOutput.Error((formatExceptionMsg(new StringBuilder(msg), redirEx)).ToString());

                if (logger.IsDebugEnabled)
                    logger.Debug(redirEx);
            }
            catch (CommandExecutionException ex1)
            {
                string msg = string.Format("An exception occurred while processing command \"{0}\"", ex1.Command != null ? ex1.Command.GetType().Name : "???");
                CommandProcessor.TerminalOutput.Error((formatExceptionMsg(new StringBuilder(msg), ex1)).ToString());

                if (logger.IsDebugEnabled)
                    logger.Debug(ex1);
            }
            catch (CommandLineInterpretationException cmdLineEx)
            {
                string msg = string.Format("An exception occurred while processing command line");
                CommandProcessor.TerminalOutput.Error((formatExceptionMsg(new StringBuilder(msg), cmdLineEx)).ToString());

                if (logger.IsDebugEnabled)
                    logger.Debug(cmdLineEx);
            }

            // Any other type of exception will go through and be considered fatal,
            // hence interrupting the execution of the CommandProcessor instance.      
        }
    
        #endregion

        #region Private methods

        private void executeStartupScript()
        {
            if (this.DoNotRunStartUpScript)
                return;

            try
            {
                using (PerfMonitor.Create("Execution of startup script"))
                {
                    string startup = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Eclair.Startup.eclair");

                    if (File.Exists(startup))
                    {
                        this.InputCommandLine(string.Format("Run \"{0}\"", startup));
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "An exception occurred while executing startup script";
                CommandProcessor.TerminalOutput.Error((formatExceptionMsg(new StringBuilder(msg), ex)).ToString());

                if (logger.IsDebugEnabled)
                    logger.Debug(ex);
            }
        }

        private void setEnvVariables()
        {
            if (!this.Environment.Variables.ContainsKey("LIBDIR"))
                this.Environment.Variables.Add("LIBDIR",Path.Combine(Assembly.GetEntryAssembly().Location, "CommandLibraries\\"));

            if (!this.Environment.Variables.ContainsKey("LIBS"))
                this.Environment.Variables.Add("LIBS", string.Empty);
            List<AssemblyInfoRetriever> info = new List<AssemblyInfoRetriever>();
            info.Add(new AssemblyInfoRetriever(Assembly.GetEntryAssembly()));
            info.AddRange(CommandFactory.Instance.RegisteredLibraries);
            info.ForEach(i => this.Environment.Variables["LIBS"] += string.Format("\"{0}, {1}\";", i.Title, i.FileVersion));

            if (!this.Environment.Variables.ContainsKey("QSDIR"))
                this.Environment.Variables.Add("QSDIR", System.Environment.GetEnvironmentVariable("QSDIR"));
       
            if (!this.Environment.Variables.ContainsKey("TEMP"))
                this.Environment.Variables.Add("TEMP", Path.GetTempPath());

            if (!this.Environment.Variables.ContainsKey("CD"))
                this.Environment.Variables.Add("CD", System.Environment.CurrentDirectory);
        }

        private void interpretLine(List<string> argList, ScriptInfo script)
        {
            if (argList == null)
                throw new ArgumentNullException("argList");

            if (argList.Count == 0)
                return;

            int currentArg = 0;
            int flowCtrlIdx = 0;
            int nbArgs = 0;
            string flowCtrlChar = "x";
            string nextFlowCtrlChar = "";
            List<string> cmdOut = null;

            using (ExecutionContextFactory.CancelableExecutionContext.Create())
            {
                try
                {
                    bool append = false; 
                    string redirPath = null;
                    for (int i = 0; i < argList.Count - 1; i++)
                    {
                        append = (argList[i] == ">>");
                        if (append || argList[i] == ">")
                        {
                            redirPath = argList[i + 1];
                            argList.RemoveRange(i, argList.Count - i);
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(redirPath) && !append && File.Exists(redirPath))
                        File.Delete(redirPath);
                    do
                    {
                        if (ExecutionContextFactory.Instance.CancelPending)
                            return;

                        bool isLastBlock = false;                      
                        flowCtrlIdx = argList.FindIndex(currentArg, s => s.Equals("|", StringComparison.OrdinalIgnoreCase));
                        if (flowCtrlIdx < 0)
                        {
                            flowCtrlIdx = argList.Count - 1;
                            nbArgs = argList.Count - currentArg;
                            nextFlowCtrlChar = "";
                            isLastBlock = true;
                        }
                        else
                        {
                            nbArgs = flowCtrlIdx - currentArg;
                            nextFlowCtrlChar = argList[flowCtrlIdx];
                        }

                        switch (flowCtrlChar)
                        {
                            case "x": // x: Command is executed
                                cmdOut = executeCommand(argList.GetRange(currentArg, nbArgs), script, !isLastBlock, redirPath);
                                break;                          

                            case "|": // |: Output of a previous command is piped as an input to the current command
                                List<string> args = argList.GetRange(currentArg, nbArgs);
                                args.AddRange(cmdOut);
                                cmdOut = executeCommand(args, script, !isLastBlock, redirPath);
                                break;

                            default:
                                break;
                        }

                        flowCtrlChar = nextFlowCtrlChar;
                        currentArg = flowCtrlIdx + 1;
                    }
                    while (flowCtrlIdx < argList.Count - 1);

                }
                catch (CommandLineInterpretationException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new CommandLineInterpretationException("Error interpreting command line " + argList.Aggregate("",(s, n) => n + " " + s), ex);
                }
            }
        }        

        private  List<string> executeCommand(List<string> args, ScriptInfo script, bool differReturn, string redirPath)
        {
            using (ICommand myCommand = CommandFactory.Instance.CreateCommand(this.Environment.CurrentCategory, args))
            {
                try
                {
                    using (PerfMonitor.Create(
                        String.Format(
                            "Command \"{0}\\{1} {2}\" execution time",
                            myCommand.CommandInfo.Category,
                            myCommand.CommandInfo.Keyword,
                            args.Aggregate("",(s, n) => n + " " + s))))
                    {
                        if (!myCommand.CommandInfo.ServerNotRequired & !this.clientProxy.IsLoggedIn)
                            throw new CommandExecutionException(myCommand, "A connection to a server is required to process this command");

                        CommandContext cmdArgs = new CommandContext(this.Environment, clientProxy, args);
                        if (myCommand is RunCommand)
                            cmdArgs = new RunCommandContext(this, true, cmdArgs);

                        if (myCommand is ScriptCommandBase)
                            cmdArgs = new ScriptCommandContext(script, cmdArgs);

                        List<string> cmdOut = null;
                        if (differReturn)
                            myCommand.Execute(cmdArgs, out cmdOut);
                        else
                            if (redirPath == null)
                                myCommand.Execute(cmdArgs);
                            else
                                myCommand.Execute(cmdArgs, redirPath);

                        return cmdOut;
                    }
                }
                catch (CommandLineInterpretationException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    if (myCommand != null)
                        throw new CommandExecutionException(
                            myCommand,
                            string.Format("Failed to process command {0}\\{1}", myCommand.CommandInfo.Category, myCommand.CommandInfo.Keyword),
                            ex);

                    throw new UnknownCommandException();
                }               
            }
        }

        private CompletionResults completionHandler(string text, int pos)
        {
            try
            {
                string parsedText = text;
                if (text.Length > 0)
                    parsedText = this.ParseCommandLine(text).Last();

                CompletionResults completionCandidates = new CompletionResults();

                Match m = Regex.Match(parsedText, @"(?i)(\w\:\\)([\w\d \(\)_\-\+\[\]\{\}]*\\)*");
                if (m.Success)  //List directories and files 
                {
                    completionCandidates.Prefix = parsedText;

                    if (!Directory.Exists(m.Value))
                        return CompletionResults.Empty;

                    string[] dirList = Directory.GetDirectories(m.Value);
                    if (dirList.Length > 0)
                    {
                        var directories =
                            from s in dirList
                            where s.StartsWith(parsedText, StringComparison.CurrentCultureIgnoreCase)
                            orderby s
                            let t = s.Substring(parsedText.Length)
                            select new CompletionElement(t, ConsoleColor.DarkGreen, "[", "]");

                        completionCandidates.Result.AddRange(directories);
                    }

                    string[] fileList = Directory.GetFiles(m.Value);
                    if (fileList.Length > 0)
                    {
                        var files =
                            from s in fileList
                            where s.StartsWith(parsedText, StringComparison.CurrentCultureIgnoreCase)
                            orderby s
                            let t = s.Substring(parsedText.Length)
                            select new CompletionElement(t, ConsoleColor.DarkYellow);

                        completionCandidates.Result.AddRange(files);
                    }
                }
                else  // Display commands & categories
                {
                    string catText;
                    string cmdText;
                    string[] a = parsedText.Split('\\');
                    bool fullCmdNameUsed = false;
                    if (a.Length == 2)
                    {
                        catText = a[0];
                        cmdText = a[1];
                        fullCmdNameUsed = true;
                    }
                    else
                    {
                        catText = this.Environment.CurrentCategory;
                        cmdText = parsedText;
                    }

                    completionCandidates.Prefix = cmdText;

                    if (!fullCmdNameUsed)
                    {
                        if (catText.Length > 0 && parsedText.Length == 0)
                            completionCandidates.Result.Add(new CompletionElement("..", ConsoleColor.DarkMagenta, "[", "]"));

                        var categories =
                              from s in CommandFactory.Instance.GetCategories()
                              where s.Length > 0 && s != "*" && s.StartsWith(cmdText, StringComparison.CurrentCultureIgnoreCase)
                              orderby s
                              let t = s.Substring(cmdText.Length)
                              select new CompletionElement(t, ConsoleColor.DarkMagenta, "[", "]");

                        completionCandidates.Result.AddRange(categories);
                    }

                    var localCmds =
                        from c in CommandFactory.Instance.GetCommands(catText)
                        where !c.CommandType.IsSubclassOf(typeof(ScriptCommandBase)) && c.Keyword.StartsWith(cmdText, StringComparison.CurrentCultureIgnoreCase)
                        orderby c.Keyword
                        select new CompletionElement(c.Keyword.Substring(cmdText.Length), ConsoleColor.Cyan);

                    completionCandidates.Result.AddRange(localCmds);   

                    if (!fullCmdNameUsed)
                    {
                        var gblCmds =
                            from c in CommandFactory.Instance.GetCommands("*")
                            where !c.CommandType.IsSubclassOf(typeof(ScriptCommandBase)) &&  c.Keyword.StartsWith(cmdText, StringComparison.CurrentCultureIgnoreCase)
                            orderby c.Keyword
                            select new CompletionElement(c.Keyword.Substring(cmdText.Length), ConsoleColor.DarkCyan);
                        
                        completionCandidates.Result.AddRange(gblCmds);                       
                    }                  
                }
                return completionCandidates;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                if (logger.IsDebugEnabled)
                    logger.Debug("Exception in command line completion handler", ex);
            }
            return CompletionResults.Empty;
        }

        private StringBuilder formatExceptionMsg(StringBuilder sb, Exception ex)
        {
            sb.AppendFormat("\n   => {0}: {1}", ex.GetType().Name, ex.Message);
            if (ex.InnerException != null)
                formatExceptionMsg(sb, ex.InnerException);

            return sb;
        }
       
        private void fireOnStoppingEvent()
        {
            if (this.OnStopping != null)
                this.OnStopping(this, new CommandProcessorEventArgs(this.Environment));
        }

        private void fireOnStoppedEvent()
        {         
            if (this.OnStopped != null)
                this.OnStopped(this, new CommandProcessorEventArgs(this.Environment));
        }

        private void fireOnStartedEvent()
        {
            if (this.OnStarted != null)
                this.OnStarted(this, new CommandProcessorEventArgs(this.Environment));
        }

        #endregion

        #region IDisposable Members

        private bool disposed;

        /// <summary>
        /// Disposes the command processor.
        /// </summary>
        /// <remarks>
        /// If a connection to the server is established, it will be gracefully closed when the command processor is disposed.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the command processor.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed, false otherwise.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Flush persistent lists to disk
                    PersistentListManager.Instance.Flush();

                     // Dispose managed resources.	

                    if (this.TempFolders != null)
                        this.TempFolders.Dispose();

                    if (this.clientProxy != null)
                    {
                        try
                        {
                            if (this.clientProxy.IsLoggedIn)
                                this.clientProxy.Disconnect();
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error logging out", ex);
                        }
                        this.clientProxy.Dispose();
                    }
                }
                // Release unmanaged resources..
            }
            disposed = true;
        }

        /// <summary>
        /// Destroys the current CommandProcessor instance.
        /// </summary>
        ~CommandProcessor()
        {
            Dispose(false);
        }
        #endregion
    }
}
                

