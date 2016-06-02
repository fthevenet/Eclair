//----------------------------------------------------------------------------- 
// <copyright file=HelpCommand">
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
using Eclair.Tools;
using Eclair.Exceptions;

namespace Eclair.Commands.Global
{
    /// <summary>
    /// Represents a command that shows in-line help.
    /// </summary>
    [CommandInfo(
      "*",
      "help",
      "Show help",
      "help [CommandName]",
      new string[] { "[CommandName] = Name of the command to get help for" },
      true)]
    public class HelpCommand : CommandBase
    {

        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="c">The execution context of the current CommandProcessor instance.</param>
        public override void ExecuteCommand(CommandContext c)
        {
            bool showDetails = c.CommandLineArguments.ArgumentExists("-d");
            if (c.CommandLineArguments.Count > 0)
            {
                string hlpTxt = c.CommandLineArguments[0];
                CommandInfoAttribute cmdInfo = null;

                if (CommandFactory.Instance.TryGetCommandInfo(c.Environment.CurrentCategory, hlpTxt, out cmdInfo))
                {
                    this.displayHelp(cmdInfo); // show help for command
                    return;
                }

                if (CommandFactory.Instance.GetCategories().Contains(hlpTxt, new CiEqualityComparer()))
                {
                    this.displayHelp(hlpTxt, showDetails); // Show help for category
                    return;
                }
            }
            if (c.Environment.CurrentCategory.Length == 0)
                this.displayHelp(showDetails); // Show global help
            else
                this.displayHelp(c.Environment.CurrentCategory, showDetails);
        }

        private void displayHelp(bool detailed)
        {
            this.OutputInfo("Commands:\n");
            foreach (var c in CommandFactory.Instance.GetCategories().OrderBy(s => s))
            {
                this.displayHelp(c, detailed);
            }
        }

        private void displayHelp(string category, bool detailed)
        {
            this.OutputInfo("Commands:\n");

            this.OutputInfo(@"@\{0}\>", category);
            foreach (CommandInfoAttribute cmdInf in CommandFactory.Instance.GetCommands(category))
            {
                if (detailed)
                {
                    this.displayHelp(cmdInf);
                }
                else
                {
                    string pad = new string(' ', Math.Max(1, 15 - cmdInf.Keyword.Length));
                    this.OutputInfo("{0}:{1}{2}", cmdInf.Keyword, pad, cmdInf.Description);
                }
            }
            this.OutputInfo(string.Empty);

        }

        private void displayHelp(CommandInfoAttribute cmdInf)
        {
            string pad = new string('_', Math.Max(1, 50 - (cmdInf.Keyword.Length + cmdInf.Category.Length)));
            this.OutputInfo("\n _[{0}\\{1}]{2}\n", cmdInf.Category, cmdInf.Keyword, pad);
            this.OutputInfo("  Description:.. " + cmdInf.Description);
            string p = "  Parameters:... ";
            for (int i = 0; i < cmdInf.Parameters.Length; i++)
            {
                this.OutputInfo(p + cmdInf.Parameters[i]);
                p = "                 ";
            }
            this.OutputInfo("  Example:...... " + cmdInf.Example);
            this.OutputInfo(string.Empty);
        }
    }
}
