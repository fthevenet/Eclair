//----------------------------------------------------------------------------- 
// <copyright file=VersionCommand">
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
using Eclair.Tools;
using System.Reflection;

namespace Eclair.Commands.Global
{

    /// <summary>
    /// Represents a command that displays version information for all the loaded modules.
    /// </summary>
    [CommandInfo(
        "*",
        "ver",
        "Display version information",
        "ver",
        "none",
        true)]
    public class VersionCommand : CommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="c">The execution context of the current CommandProcessor instance.</param>
        public override void ExecuteCommand(CommandContext c)
        {
            List<AssemblyInfoRetriever> info = new List<AssemblyInfoRetriever>();

            info.Add(new AssemblyInfoRetriever(Assembly.GetEntryAssembly()));
            info.AddRange(CommandFactory.Instance.RegisteredLibraries);
            info.ForEach(i => this.OutputInfo("{0} - {1}", i.Title, i.FileVersion));
        }
    }
}
