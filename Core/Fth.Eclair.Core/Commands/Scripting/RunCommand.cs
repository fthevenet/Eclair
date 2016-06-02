//----------------------------------------------------------------------------- 
// <copyright file=RunCommand">
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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Eclair.Exceptions;
using Eclair.Tools;

namespace Eclair.Commands.Scripting
{
   
    /// <summary>
    /// Represents a command that runs a script file.
    /// </summary>
    [CommandInfo(
        "*",
      "run",
      "Run a script file ",
      "run scriptFilePath",
      new string[] { "[scriptFilePath] = Path of the script file" },
      true)]
    public class RunCommand : CommandBase
    {
        private ScriptInfo context;
        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="args">The execution context of the current CommandProcessor instance.</param>
        public override void ExecuteCommand(CommandContext args)
        {
          
            RunCommandContext rc = args as RunCommandContext;

            FileInfo script;
            string path = rc.CommandLineArguments.Find((s) => File.Exists(s));

            if (string.IsNullOrEmpty(path))
                throw new CommandExecutionException(this, "Cannot find script file");

            script = new FileInfo(path);

            using (StreamReader sr = script.OpenText())
            {

                List<ScriptLine> lines = new List<ScriptLine>();
                int ln = 1;
                while (!sr.EndOfStream)
                {
                    ScriptLine sl = new ScriptLine();
                    sl.StreamPosistion = sr.BaseStream.Position;
                    sl.Value = sr.ReadLine();
                    sl.Number = ln++;
                    if (sl.Value.Trim().Length > 0)
                        lines.Add(sl);
                }

                context = new ScriptInfo(script, true, lines.GetEnumerator());
                ScriptLine line;

                while (context.LineEnumerator.MoveNext())
                {
                    line = context.LineEnumerator.Current;
                   
                    this.OutputDebug("Running script line: {0} - {1}", line.Number, line.Value);

                    try
                    {
                        if (line.Value.StartsWith("rem ", StringComparison.InvariantCultureIgnoreCase))
                            this.OutputDebug("Line {0} \"{1}\" commented out", line.Number, line.Value);                     
                        else
                            rc.CommandProcessor.InterpretScriptLine(line.Value, this.context);                                       
                    }
                    catch (Exception ex)
                    {                     
                        if (context.BreakOnError)
                            throw new CommandExecutionException(this, string.Format("Error in line {0} in script {1}: \"{2}\"", line.Number, script.FullName, line.Value), ex);
                                                
                        this.OutputError("Error in script {1}, line {0}: {2}", line.Number, script.FullName, ex.Message);
                        this.OutputDebug("Error in script {1}, line {0}\n{2}", line.Number, script.FullName, ex);

                    }                  
                }
            }
        }
    }
}
