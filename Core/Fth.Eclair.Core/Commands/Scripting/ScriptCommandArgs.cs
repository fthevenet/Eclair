//----------------------------------------------------------------------------- 
// <copyright file=ScriptCommandArgs">
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

namespace Eclair.Commands.Scripting
{
    /// <summary>
    /// Represents a command execution context for commands that affects the control flow of a script.
    /// </summary>
    public class ScriptCommandContext : CommandContext
    {
        /// <summary>
        /// Initializes a new instance of the ScriptCommandContext class.
        /// </summary>
        /// <param name="scriptContext">The contextual info on the script being run.</param>
        /// <param name="c">The base command context.</param>
        public ScriptCommandContext( ScriptInfo scriptContext, CommandContext c)
            : base(c.Environment, c.ClientProxy, c.CommandLineArguments)
        {
            this.Script = scriptContext;
        }

        /// <summary>
        /// Gets the contextual info on the script being run.
        /// </summary>
        public ScriptInfo Script
        {
            get;
            set;
        }
    }
}
