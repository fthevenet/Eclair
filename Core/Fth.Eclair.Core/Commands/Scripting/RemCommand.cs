//----------------------------------------------------------------------------- 
// <copyright file=RemCommand">
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
    /// Represents a command that comments a line in a script.
    /// </summary>
    /// <remarks>
    /// This command can only be used in a script and has no effect when typed in an interactive shell console.
    /// </remarks>
    [CommandInfo(
        "*",
      "rem",
      "Comment a line in a script",
      "rem",
      "none",
      true)]
    public class RemCommand : ScriptCommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor when the command is executed from a script.
        /// </summary>
        /// <param name="fc">The execution context of the script executed and the current CommandProcessor instance.</param>
        public override void FlowControlExecute(ScriptCommandContext fc)
        {
            this.OutputDebug("Line {0} \"{1}\" commented out",fc.Script.LineEnumerator.Current.Number, fc.CommandLineArguments.Aggregate("",(s, p) => p + " " + s));
        }
    }
}
