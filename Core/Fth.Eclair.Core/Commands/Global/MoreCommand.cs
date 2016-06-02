//----------------------------------------------------------------------------- 
// <copyright file=MoreCommand">
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
    /// Represents a command that pages through the output of a piped command.
    /// </summary>
    [CommandInfo(
       "*",
     "more",
     "Pages through the output of a piped command",
     "help | more",
     "none",
     true
     )]
    public class MoreCommand : CommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="c">The execution context of the current CommandProcessor instance.</param>
        public override void ExecuteCommand(CommandContext c)
        {

            if (c.CommandLineArguments.Count == 1 && File.Exists(c.CommandLineArguments[0]))
                using (StreamReader r = File.OpenText(c.CommandLineArguments[0]))
                {
                    c.CommandLineArguments.Clear();
                    while (!r.EndOfStream)
                        c.CommandLineArguments.Add(r.ReadLine());
                }

            if (c.Environment.RunningMode == CommandProcessorRunningMode.Batch || !c.Environment.RunInConsole)
            {
                c.CommandLineArguments.ForEach(s => this.OutputInfo(s));
                return;
            }

            int displayed = 2;

            IEnumerator<string> le = c.CommandLineArguments.GetEnumerator();
            bool nxt = le.MoveNext();
            while (nxt)
            {
                if (c.Environment.CancelPending)
                    return;

                if (displayed < Console.WindowHeight)
                {
                    CommandProcessor.TerminalOutput.Info(le.Current);
                    if (le.Current.Length > 0)
                        displayed += (int)System.Math.Truncate((decimal)le.Current.Length / Console.WindowWidth);
                    displayed++;
                    nxt = le.MoveNext();
                }
                else
                {
                    Console.Write("-- More --");
                    var cki = Console.ReadKey(true);

                    if (cki.Modifiers == ConsoleModifiers.Control && cki.Key == ConsoleKey.C)
                        return;

                    switch (cki.Key)
                    {
                        case ConsoleKey.Enter:
                            displayed--;
                            break;

                        case ConsoleKey.Spacebar:
                            displayed -= Console.WindowHeight - 1;
                            break;

                        case ConsoleKey.Escape:
                            nxt = false;
                            break;

                        default:
                            break;
                    }
                    Console.CursorLeft = 0;
                    Console.Write(new string(' ', 10));
                    Console.CursorLeft = 0;
                }
            }
        }
    }
}
