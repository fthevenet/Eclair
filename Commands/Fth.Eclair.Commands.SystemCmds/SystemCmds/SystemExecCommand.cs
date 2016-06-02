//----------------------------------------------------------------------------- 
// <copyright file=SystemExecCommand">
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
        "exec",
        "Start a system process",
        "exec [-w] [Path]",
        new string[]{"Path= Path the file to be launched. If the path points to a file that is not executable, it will be opened with the application registered by Windows for its extention.",
                      "-w: Wait for spawned process to terminate."},
        true)]
    public class SystemExecCommand : CommandBase
    {
        protected Process process;

        public override void ExecuteCommand(CommandContext c)
        {
            if (c.CommandLineArguments.Count < 1)
                throw new ArgumentNullException();
            bool wait = c.CommandLineArguments.ArgumentExists("-w");
            string procPath = c.CommandLineArguments[0];
            c.CommandLineArguments.RemoveAt(0);
            ProcessStartInfo si = new ProcessStartInfo(procPath);
            if (c.CommandLineArguments.Count > 0)
                si.Arguments = c.CommandLineArguments.Aggregate("",(s, p) => p + " " + s);
            this.process = Process.Start(si);
            if (wait)
                this.process.WaitForExit();
        }
    }
}              
