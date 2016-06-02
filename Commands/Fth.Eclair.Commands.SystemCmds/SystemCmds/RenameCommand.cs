//----------------------------------------------------------------------------- 
// <copyright file=RenameCommand">
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
using System.IO;
using Eclair.Tools;

namespace Eclair.Commands.SystemCmds
{
      [CommandInfo(
        "System",
        "ren",
        "Rename files",
        "ren []",
        "",
        true)]
    public class RenameCommand:CommandBase
      {
        public override void ExecuteCommand(CommandContext c)
        {
            SearchOption so = SearchOption.TopDirectoryOnly;
            if (c.CommandLineArguments.ArgumentExists("-r"))
                so = SearchOption.AllDirectories;

            if (c.CommandLineArguments.Count < 2)
                throw new ArgumentException("This command take 2 arguments");

            string dirName = Path.GetDirectoryName(c.CommandLineArguments[0]);
            string filter = Path.GetFileName(c.CommandLineArguments[0]);
            string targetName = c.CommandLineArguments[1];
            string[] files =  Directory.GetFiles(dirName, filter,so);
            for (int i=0; i<files.Length;i++)
            {
                File.Move(files[i], string.Format("{0}\\{1}{2}.{3}", dirName, targetName, i, Path.GetExtension(files[i])));
            }
        }
    }
}
