//----------------------------------------------------------------------------- 
// <copyright file=PauseCommand">
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
using System.IO;
using System.Collections.Generic;
using System.Text;
using Eclair.Exceptions;
using Eclair.Tools;

namespace Eclair.Commands.Scripting
{

    /// <summary>
    /// Represents a command that pauses the execution of a script until the end user presses a key.
    /// </summary>
    /// <remarks>
    /// This command has no effect when the commandProcessor is not running in interactive mode.
    /// </remarks>
    [CommandInfo(
        "*",
      "pause",
      "Pause a script",
      "pause",
      "none",
      true)]
    public class PauseCommand : CommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="c">The execution context of the current CommandProcessor instance.</param>
        public override void ExecuteCommand(CommandContext c)
        {
           
            if (c.Environment.RunningMode == CommandProcessorRunningMode.Interactive)
            {
                this.OutputInfo("Press any key to continue...");
                Console.ReadKey(true);
            }
        }
    }
}
