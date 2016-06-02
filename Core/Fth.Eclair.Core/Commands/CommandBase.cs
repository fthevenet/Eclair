//----------------------------------------------------------------------------- 
// <copyright file=CommandContextCommandBase">
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
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using log4net;
using Eclair.Tools;
using Eclair.Exceptions;
using System.IO;

namespace Eclair.Commands
{
    /// <summary>
    /// Provides a base implementation for the ICommand interface, and provides utilities to interact with the shell environment.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        private ILog logger;
        private delegate void outputErrorDelegate(string message, params object[] args);
        private delegate void outputWarnDelegate(string message, params object[] args);
        private delegate void outputInfoDelegate(string message, params object[] args);
        private outputErrorDelegate doOutputError;
        private outputWarnDelegate doOutputWarn;
        private outputInfoDelegate doOutputInfo;
        private List<string> commandOutput;
        private string outputFile;

        /// <summary>
        /// Initializes a new instance of the CommandBase class.
        /// </summary>
        protected CommandBase()
        {
            this.doOutputError = this.displayOutputError;
            this.doOutputInfo = this.displayOutputInfo;
            this.doOutputWarn = this.displayOutputWarn;

            this.logger = LogManager.GetLogger(this.GetType());
            this.commandOutput = new List<string>();

            object[] cmdInfoAttributes = this.GetType().GetCustomAttributes(typeof(CommandInfoAttribute), true);
            if (cmdInfoAttributes.Length == 0)
                throw new CommandExecutionException(this, "Failed to retrieve command info from attributes");

            this.CommandInfo = (CommandInfoAttribute)cmdInfoAttributes[0];
        }

        /// <summary>
        /// Writes a message to the error channel of the CommandProcessor.
        /// </summary>
        /// <param name="message">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void OutputError(string message, params object[] args)
        {
            this.doOutputError("ERROR - " + message, args);
        }

        /// <summary>
        /// Writes a message to the warning channel of the CommandProcessor.
        /// </summary>
        /// <param name="message">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void OutputWarning(string message, params object[] args)
        {
            this.doOutputWarn("WARNING - " + message, args);
        }

        /// <summary>
        /// Writes a message to the info channel of the CommandProcessor.
        /// </summary>
        /// <param name="message">The message to write.</param>
        protected void OutputInfo(string message)
        {
            this.doOutputInfo(message);
        }

        /// <summary>
        /// Writes a message to the info channel of the CommandProcessor.
        /// </summary>
        /// <param name="message">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void OutputInfo(string message, params object[] args)
        {
            this.doOutputInfo(message, args);
        }

        /// <summary>
        /// Writes a message to the debug channel of the CommandProcessor.
        /// </summary>
        /// <param name="message">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void OutputDebug(string message, params object[] args)
        {
            if (logger.IsDebugEnabled)
                this.logger.Debug(formatMessage(message, args));
        }

        private void displayOutputError(string message, params object[] args)
        {
            CommandProcessor.TerminalOutput.Error(formatMessage(message, args));
        }
        private void displayOutputWarn(string message, params object[] args)
        {
            CommandProcessor.TerminalOutput.Warn(formatMessage(message, args));
        }
        private void displayOutputInfo(string message, params object[] args)
        {
            CommandProcessor.TerminalOutput.Info(formatMessage(message, args));
        }

        private void pushOutputToList(string message, params object[] args)
        {
            this.commandOutput.Add(formatMessage(message, args));
        }

        private void pushOutputToFile(string message, params object[] args)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(this.outputFile, true))
                    sw.WriteLine(formatMessage(message, args));

            }
            catch (Exception ex)
            {
                if (this.outputFile == null)
                    throw;

                throw new OutputRedirectionException("Failed to redirect output to file " + this.outputFile, ex);
            }
        }

        private string formatMessage(string message, params object[] args)
        {
            try
            {
                return string.Format(message, args);
            }
            catch (FormatException)
            {
                return message + args.Aggregate("", (s, o) => s + " " + o);
            }
        }

        #region ICommand Members

        /// <summary>
        /// Executes the logic implemented for the specified command. 
        /// </summary>
        /// <param name="context">A CommandContext object that provides the execution context under which the command logic is running.</param>
        /// <param name="outputFile">The path to a file into which the output of the command will be redirected.</param>
        public void Execute(CommandContext context, string outputFile)
        {
            this.outputFile = outputFile;

            this.doOutputError = this.pushOutputToFile;
            this.doOutputInfo = this.pushOutputToFile;
            this.doOutputWarn = this.pushOutputToFile;

            this.ExecuteCommand(context);
        }

        /// <summary>
        /// Executes the logic implemented for the specified command.
        /// </summary>
        /// <param name="context">A CommandContext object that provides the execution context under which the command logic is running.</param>
        /// <param name="results">A list of string into which the output of the command will be stored.</param>
        public void Execute(CommandContext context, out List<string> results)
        {
            this.doOutputError = this.pushOutputToList;
            this.doOutputInfo = this.pushOutputToList;
            this.doOutputWarn = this.pushOutputToList;

            this.commandOutput.Clear();

            this.ExecuteCommand(context);

            results = this.commandOutput;
        }

        /// <summary>
        /// Executes the logic implemented for the specified command.
        /// </summary>
        /// <param name="context">A CommandContext object that provides the execution context under which the command logic is running.</param>
        public void Execute(CommandContext context)
        {

            this.doOutputError = this.displayOutputError;
            this.doOutputInfo = this.displayOutputInfo;
            this.doOutputWarn = this.displayOutputWarn;

            this.ExecuteCommand(context);
        }

        #endregion

        /// <summary>
        /// Implements the core logic for the command.
        /// </summary>
        /// <remarks>
        /// Classes that inherits from CommandBase must implement that methods.
        /// </remarks>
        /// <param name="context">A CommandContext object that provides the execution context under which the command logic is running.</param>
        public abstract void ExecuteCommand(CommandContext context);

        /// <summary>
        /// Gets the properties of the command.
        /// </summary>
        public CommandInfoAttribute CommandInfo { get; private set; }

        #region Members of IDisposable


        private bool disposed;

        /// <summary>
        /// Disposes the CommandBase instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the CommandBase instance.
        /// </summary>
        /// <param name="disposing">True if managed resources must be released, false otherwise.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.	

                }
                // Release unmanaged resources. 

            }
            disposed = true;
        }

        #endregion

    }
}
