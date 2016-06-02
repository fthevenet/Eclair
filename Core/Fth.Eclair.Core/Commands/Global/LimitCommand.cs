//----------------------------------------------------------------------------- 
// <copyright file=LimitCommand">
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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Eclair.Exceptions;

namespace Eclair.Commands.Global
{
    /// <summary>
    /// Represents a command that limits the number of elements returned by a command.
    /// </summary>
    [CommandInfo(
      "*",
      "limit",
      "limit the number of elements returned by a command",
      "ls | limit 100",
      "The number of element to limit the list to",
      true)]
    public class LimitCommand : CommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="c">The execution context of the current CommandProcessor instance.</param>
        public override void ExecuteCommand(CommandContext c)
        {
            if (c.CommandLineArguments.Count == 0)
                throw new ArgumentException("This command doesn't take zero argument");            

            int limit = 0;
            if (!int.TryParse(c.CommandLineArguments[0], out limit))
                throw  new ArgumentException("Argument must be a valid integer value");

            c.CommandLineArguments.RemoveAt(0);

            for (int i = 0; i < Math.Min(c.CommandLineArguments.Count, limit); i++)
            {
                if (c.Environment.CancelPending)
                    return;

                this.OutputInfo(c.CommandLineArguments[i]);
            }           
        }
    }
}