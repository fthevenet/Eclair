//----------------------------------------------------------------------------- 
// <copyright file=ChangeCategoryCommand">
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

using Eclair.Exceptions;

namespace Eclair.Commands.Global
{
    /// <summary>
    /// Represents a command that changes the current category.
    /// </summary>
    [CommandInfo(
      "*",
      "cc",
      "Change category",
      "cc [CategoryName]",
      new string[] { "[CategoryName] = Name of category" },
      true)]
    public class ChangeCategoryCommand : CommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="context">The execution context of the current CommandProcessor instance.</param>
        public override void ExecuteCommand(CommandContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (context.CommandLineArguments == null || context.CommandLineArguments.Count < 1)
                throw new ArgumentException("The CommandLineArguments property of the provided CommandContext instance is null or empty.");          

            string category = context.CommandLineArguments[0];

            if (category == "\\" || category == "..")
                category = "";
            context.Environment.CurrentCategory = category;
        }
    }

    /// <summary>
    /// Defines an alias to the "Change category" command.
    /// </summary>
    [CommandInfo(Category = "*", Keyword = "cd", Description = "Alias to cc", NotBrowsable = true, ServerNotRequired = true)]
    public class ChangeCategoryAlias : ChangeCategoryCommand
    {

    }
}
