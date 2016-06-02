//----------------------------------------------------------------------------- 
// <copyright file=GrepCommand">
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
using System.Text.RegularExpressions;

namespace Eclair.Commands.Global
{
    /// <summary>
    /// Represents a command that sorts a command output according to a regular expression.
    /// </summary>
    [CommandInfo(
        "*",
      "grep",
      "Sort a command output according to a regular expression",
      "grep [pattern] [data]",
      "none",
      true
      )]
    public class GrepCommand : CommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="c">The execution context of the current CommandProcessor instance.</param>
        public override void ExecuteCommand(CommandContext c)
        {
            if (c.CommandLineArguments.Count == 0)
                throw new ArgumentException("The provided argument list must contain at least one element.");

            string pattern = c.CommandLineArguments[0];
            c.CommandLineArguments.RemoveAt(0);

            foreach(string s in c.CommandLineArguments)
            {
                if (c.Environment.CancelPending)
                    return;

                Match m = Regex.Match(s,pattern, RegexOptions.None);
                if (m.Success)
                    this.OutputInfo(m.Value);
            }
   

        }
    }


  
}
