//----------------------------------------------------------------------------- 
// <copyright file=CommandProcessorEnvironment">
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
using System.Linq;
using System.Text;
using Eclair.Tools;

namespace Eclair.Commands
{
    /// <summary>
    /// Provides information about, and means to manipulate, the current environment of a CommandProcessor instance.
    /// </summary>
    public class CommandProcessorEnvironment
    {
        private CommandProcessor cmdProc;

        /// <summary>
        /// Initializes a new instance of the ShellEnvironment class.
        /// </summary>
        /// <param name="cmdProc">The CommandProcessor instance to which this environment applies to.</param>
        public CommandProcessorEnvironment(CommandProcessor cmdProc)
            : this(cmdProc, string.Empty, CommandProcessorRunningMode.Batch)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ShellEnvironment class.
        /// </summary>
        /// <param name="cmdProc">The commandProcessor instance to which this environment applies to.</param>
        /// <param name="currentCategory">The current command category.</param>
        /// <param name="runningMode">The running mode of the CommandProcessor instance</param>
        public CommandProcessorEnvironment(CommandProcessor cmdProc, string currentCategory, CommandProcessorRunningMode runningMode)
        {
            this.ExitPending = false;
            this.cmdProc = cmdProc;
            this.currentCategory = currentCategory;
            this.RunningMode = runningMode;
            this.Variables = new Dictionary<string, string>(new CiEqualityComparer());
        }

        /// <summary>
        /// Gets a dictionary of the variables defined in the CommandProcessor.
        /// </summary>
        public Dictionary<string, string> Variables
        {
            get;
            private set;
        }

        private string currentCategory = string.Empty;
        /// <summary>
        /// Gets or sets the current command category of the CommandProcessor.
        /// </summary>
        public string CurrentCategory
        {
            get { return currentCategory; }
            set
            {
                if (!CommandFactory.Instance.CategoryExists(value, false))
                    throw new ArgumentOutOfRangeException("Category", string.Format("Command category {0} does not exist", value));

                currentCategory = value;
            }
        }

        /// <summary>
        /// Returns True if the CommandProcessor instance runs in the Console environment,
        /// False if its I/Os are redirected to another application.
        /// </summary>
        public bool RunInConsole
        {
            get { return this.cmdProc.RunInConsole; }
        }

        /// <summary>
        /// Returns True if the CommandProcessor instance is exiting, False otherwise.
        /// </summary>
        public bool ExitPending
        {
            get;
            internal set;
        }

        /// <summary>
        /// Returns True if the cancellation of a command has been required by an end user pressing Ctrl+C, False otherwise.
        /// </summary>
        public bool CancelPending
        {
            get { return ExecutionContextFactory.Instance.CancelPending; }
        }

        /// <summary>
        /// Gets the running mode the CommandProcessor instance was started in.
        /// </summary>
        public CommandProcessorRunningMode RunningMode
        {
            get;
            set;
        }

        /// <summary>
        /// A method that can be used to specify a command line that should be interpreted and executed by the current CommandProcessor instance.
        /// </summary>
        /// <param name="commandLine"></param>
        public void EmitCommand(string commandLine)
        {
            this.cmdProc.InputCommandLine(commandLine);
        }

        /// <summary>
        /// Returns a new disposable temporary folder.
        /// </summary>
        /// <remark>
        /// A disposable temporary folder can be use as a temporary location to store work data. The folder and its content is deleted 
        /// from the disk when the object is disposed.
        /// </remark>
        /// <returns>A new disposable temporary folder.</returns>
        public DisposableTempFolder GetDisposableTempFolder()
        {
            return this.cmdProc.TempFolders.CreateOne(this.Variables["TEMP"]);
        }
    }
}
