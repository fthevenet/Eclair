//----------------------------------------------------------------------------- 
// <copyright file=SetVarCommand">
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
using System.Text.RegularExpressions;
using Eclair.Exceptions;
using Eclair.Tools;

namespace Eclair.Commands.Global
{
    /// <summary>
    /// Represents a command that sets a variable's value.
    /// </summary>
    [CommandInfo(
          "*",
        "set",
        "Set a variable value",
        "set VarName= value",
         "",
        true)]
    public class SetVarCommand : CommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="c">The execution context of the current CommandProcessor instance.</param>
         public override void ExecuteCommand(CommandContext c)
        {                   
            if (c.CommandLineArguments.Count == 0)
            {
                var variables =
                    from d in c.Environment.Variables
                    let s =string.Format("{0}={1}", d.Key, d.Value)
                    select s;

                foreach (string s in variables)
                    OutputInfo(s);
            }
            else
            {
                string cmdName = c.CommandLineArguments[0].Replace("=", "");
                c.CommandLineArguments.RemoveAt(0);

                if (!Regex.IsMatch(cmdName, @"[\w\d\-_]+"))
                    throw new CommandExecutionException(this, "A Variable name can only contain letters, numbers, dashes and underscores");

                string varValue = "";
                if (c.CommandLineArguments.Count > 0)
                    varValue = c.CommandLineArguments.Aggregate((p, s) => p + s);

                if (c.Environment.Variables.Keys.Contains(cmdName))
                    c.Environment.Variables[cmdName] = varValue;
                else
                    c.Environment.Variables.Add(cmdName, varValue);
            }
        }       
    }
}
