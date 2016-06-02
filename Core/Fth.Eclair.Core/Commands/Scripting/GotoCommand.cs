//----------------------------------------------------------------------------- 
// <copyright file=GotoCommand">
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
using Eclair.Exceptions;
using Eclair.Tools;

namespace Eclair.Commands.Scripting
{
    /// <summary>
    /// Represents a command that 
    /// </summary>
    /// <remarks>
    /// This command can only be used in a script and has no effect when typed in an interactive shell console.
    /// </remarks>
    [CommandInfo(
          "*",
        "goto",
        "Go to the specified script line",
        "goto 5",
         "[lineNumber]: Line to go to in script file",
        true)]
    class GotoCommand : ScriptCommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor when the command is executed from a script.
        /// </summary>
        /// <param name="fc">The execution context of the script executed and the current CommandProcessor instance.</param>
        public override void FlowControlExecute(ScriptCommandContext fc)
        {    
            long lnb;
            if (long.TryParse(fc.CommandLineArguments[0], out lnb))
            {
                fc.Script.LineEnumerator.Reset();
               for (int i = 0; i < lnb - 1; i++) fc.Script.LineEnumerator.MoveNext();

            }
        }
    }
}
