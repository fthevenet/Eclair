//----------------------------------------------------------------------------- 
// <copyright file=RunCommandContext">
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

namespace Eclair.Commands
{
    /// <summary>
    /// Represents a specific command execution context for the SHELL\RUN command.
    /// </summary>
    public class RunCommandContext : CommandContext
    {

        /// <summary>
        /// Initializes a new instance of the RunCommandContext class.
        /// </summary>
        /// <param name="commandProcessor">The current CommandProcessor instance.</param>
        /// <param name="isOutputDiffered">True if output should be differed, False otherwise.</param>
        /// <param name="context">The parent command's execution context. </param>
        public RunCommandContext(CommandProcessor commandProcessor, bool isOutputDiffered, CommandContext context)//y clientProxy, List<string> commandLineArguments)
            : base(commandProcessor.Environment, context.ClientProxy, context.CommandLineArguments)
        {
            this.CommandProcessor = commandProcessor;
            this.IsOutputDiffered = isOutputDiffered;
        }

        /// <summary>
        /// Returns True is output are differed, False is output are sent to the console.
        /// </summary>
        public bool IsOutputDiffered
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current CommandProcessor instance.
        /// </summary>
        public CommandProcessor CommandProcessor
        {
            get;
            private set;
        }      
    }
}
