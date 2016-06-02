//----------------------------------------------------------------------------- 
// <copyright file=GetVarCommand">
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
using System.IO;
using Eclair.Tools;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace Eclair.Commands.SystemCmds
{
    [CommandInfo(
        "System",
        "getvar",
        "Retrieve the value of an OS environment variable",
        "getvar [VariableName]",
        "VariableName= Name of the environment variable to retrieve",
        true)]
    public class GetVarCommand : CommandBase
    {
        public override void ExecuteCommand(CommandContext c)
        {
            if (c.CommandLineArguments.Count == 0)
                throw new ArgumentNullException("VariableName", "You must provide a valid variable name");
      
            this.OutputInfo(Environment.GetEnvironmentVariable(c.CommandLineArguments[0]));
        }
    }
}              
