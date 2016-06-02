//----------------------------------------------------------------------------- 
// <copyright file=FromBase64Command">
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
using System.Security.Cryptography;

namespace Eclair.Commands.SystemCmds
{
    /// <summary>
    /// defines a command that decodes a base64 encoded string.
    /// </summary>
    [CommandInfo(
       Category = "*",
       Keyword = "decb64",
       Description = "Decode a base64 encoded string.",
       Example = "decb64 \"base64 data\"",
       Parameters = new string[] { "Base64 data to decode." },
       NotBrowsable = false,
       ServerNotRequired = true)]
    public class FromBase64Command : CommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="c">The execution context of the current CommandProcessor instance.</param>
        public override void ExecuteCommand(CommandContext c)
        {
            if (c.CommandLineArguments.Count == 0)
                throw new ArgumentException("FromBase64Command does not take 0 parameters");

            this.OutputInfo(Encoding.UTF8.GetString(Convert.FromBase64String(c.CommandLineArguments.First())));
        }
    }
}
