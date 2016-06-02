//----------------------------------------------------------------------------- 
// <copyright file=LoadCommand">
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
using System.Text;
using System.IO;
using Eclair.Exceptions;
using Eclair.Tools;

namespace Eclair.Commands.Global
{
    /// <summary>
    /// Represents a command that loads a command library and register all available commands.
    /// </summary>
    [CommandInfo(
      "*",
      "load",
      "Load a command library and register available commands",
      "load c:\\myLibPath\\myLib.dll",
      new string[] { "Library path" },
      true)]
    public class LoadCommand : CommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="c">The execution context of the current CommandProcessor instance.</param>
        public override void ExecuteCommand(CommandContext c)
        {
            if (c.CommandLineArguments.Count == 0)
                throw new ArgumentException("Library path not specified", "LibPath");

            FileInfo fi = new FileInfo(c.CommandLineArguments[0]);

            int nbCmd = CommandFactory.Instance.LoadCommandLibrary(fi.FullName);

            if (nbCmd == 0)
                this.OutputInfo("No commands found in {0}", fi.Name);
            else
                this.OutputInfo("Successfully registered {0} command(s)", nbCmd);
        }       
    }
}
