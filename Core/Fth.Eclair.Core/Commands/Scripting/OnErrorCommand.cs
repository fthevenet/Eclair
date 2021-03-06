﻿//----------------------------------------------------------------------------- 
// <copyright file=OnErrorCommand">
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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Eclair.Exceptions;
using Eclair.Tools;

namespace Eclair.Commands.Scripting
{
    /// <summary>
    /// Represents a command that defines the behaviour of the script in case of an error is encountered
    /// </summary>
    /// <remarks>
    /// This command can only be used in a script and has no effect when typed in an interactive shell console.
    /// </remarks>
    [CommandInfo(
        "*",
      "onerror",
      "Define the behaviour of the script in case of an error is encountered",
      "onerror [break|continue]",
      "none",
      true)]
    public class OnErrorCommand : ScriptCommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor when the command is executed from a script.
        /// </summary>
        /// <param name="fc">The execution context of the script executed and the current CommandProcessor instance.</param>
        public override void FlowControlExecute(ScriptCommandContext fc)
        {
            if (fc.CommandLineArguments.Count == 0)
                throw new ArgumentException("This command does not take 0 arguments");

            if (fc.CommandLineArguments.Contains("break", new CiEqualityComparer()))
            {
                fc.Script.BreakOnError = true;
                this.OutputDebug("BreakSciptOnError set to true");
                return;
            }

            if (fc.CommandLineArguments.Contains("continue", new CiEqualityComparer()))
            {
                fc.Script.BreakOnError = false;
                this.OutputDebug("BreakSciptOnError set to false");
                return;
            }

            throw new ArgumentOutOfRangeException("BreakOrContinue","Invalid value");
            
        }
    }


  
}
