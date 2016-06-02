//----------------------------------------------------------------------------- 
// <copyright file=ScriptCommandBase">
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
    /// Provides the base class for commands that can only by executed from a scripts.
    /// </summary>
    /// <remarks>
    /// Commands inheriting this base class can only be used in a script and have no effect when entered from an interactive shell console.
    /// </remarks>
    public abstract class ScriptCommandBase : CommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor when the command is executed from an interactive shell console.
        /// </summary>
        /// <param name="c">The execution context of the current CommandProcessor instance.</param>
        /// <remarks>
        /// If this method is called as the result of the end user entering the command keyword from an interactive shell console,
        /// the inline help for the command is displayed.
        /// </remarks>
        public sealed override void ExecuteCommand(CommandContext c)
        {
            ScriptCommandContext sc = c as ScriptCommandContext;
            if ((sc != null) && (sc.Script != null))
                this.FlowControlExecute(sc);
            else
                c.Environment.EmitCommand("Help " + this.CommandInfo.Keyword);
        }

        /// <summary>
        /// The method invoked by the CommandProcessor when the command is executed from a script.
        /// </summary>
        /// <param name="fc">The execution context of the script executed and the current CommandProcessor instance.</param>
        public abstract void FlowControlExecute(ScriptCommandContext fc);

    }
}
