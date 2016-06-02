//----------------------------------------------------------------------------- 
// <copyright file=FilterByDateCommand">
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
using Eclair.Exceptions;
using Eclair.Tools;
using System.Text.RegularExpressions;


namespace Eclair.Commands.Global
{
    /// <summary>
    /// Represents a command that filters the input based on a date.
    /// </summary>
    [CommandInfo(
        Category="*",
        Keyword="fdate",
        Parameters= new string[] {
            "d=dd/mm/yyyy: The date value to which the filter is applied.",
            "min=dd/mm/yyyy: The date to which the filtered dates should be superior or equal.",
            "max=dd/mm/yyyy: The date to which the filtered dates should be inferior or equal."
        },
        Description= "Filter the input based on a date",
        Example = "fdate d=01/01/2012 [d=02/01/2012 d=...] [min=10/12/2011] [max=10/01/2012]",
        ServerNotRequired = true)]
    public class FilterByDateCommand : CommandBase
    {      
        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="c">The execution context of the current CommandProcessor instance.</param>
        public override void ExecuteCommand(CommandContext c)
        {
            bool checkLower;
            bool checkHigher;

            DateTime minDate =
               (from s in c.CommandLineArguments
                let m = Regex.Match(s, @"(?i)min=(?<val>[0-9]{1,4}[\/\-\.][0-9]{1,4}[\/\-\.][0-9]{1,4})")
                where m.Groups["val"].Success
                let d = CommandArgsExtension.TryParse<DateTime>(m.Groups["val"].Value)
                where d.Key
                select d.Value).FirstOrDefault();

            checkLower = (minDate > DateTime.MinValue);

            DateTime maxDate =
             (from s in c.CommandLineArguments
              let m = Regex.Match(s, @"(?i)max=(?<val>[0-9]{1,4}[\/\-\.][0-9]{1,4}[\/\-\.][0-9]{1,4})")
              where m.Groups["val"].Success
              let d = CommandArgsExtension.TryParse<DateTime>(m.Groups["val"].Value)
              where d.Key
              select d.Value).FirstOrDefault();

            checkHigher = (maxDate > DateTime.MinValue);

            var dateList =
               from s in c.CommandLineArguments
               let m = Regex.Match(s, @"(?i)d=(?<val>[0-9]{1,4}[\/\-\.][0-9]{1,4}[\/\-\.][0-9]{1,4})")
               where m.Groups["val"].Success
               let d = CommandArgsExtension.TryParse<DateTime>(m.Groups["val"].Value)
               where d.Key && (checkHigher ? d.Value <= maxDate : true) && (checkLower ? d.Value >= minDate : true)
               select s;

            foreach (var s in dateList)
            {
                if (c.Environment.CancelPending)
                    return;

                this.OutputInfo(s);
            }
        }
    }
}
