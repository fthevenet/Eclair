//----------------------------------------------------------------------------- 
// <copyright file=HashCommand">
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
    [CommandInfo(
       "System",
       "hash",
       "Generate a hash for the provided file",
       "hash <-md5|-sha1> [FilePath]",
       "",
       true)]
    public class HashCommand : CommandBase
    {
        public override void ExecuteCommand(CommandContext c)
        {
            if (c.CommandLineArguments.Count == 0)
                throw new ArgumentException("HashCommand does not take 0 parameters");

            string filePath = c.CommandLineArguments.FirstOrDefault(s => File.Exists(s));

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("You must provide a valid path");

            if (c.CommandLineArguments.Contains("-md5", new CiEqualityComparer()))
            {
                var md5 = MD5.Create();
                using (FileStream fs = File.OpenRead(filePath))
                {
                    this.OutputInfo("MD5: {0}", Convert.ToBase64String(md5.ComputeHash(fs)));
                }
            }

            if (c.CommandLineArguments.Contains("-sha1", new CiEqualityComparer()))
            {
                var sha1 = SHA1Managed.Create();
                using (FileStream fs = File.OpenRead(filePath))
                {
                    this.OutputInfo("SHA1: {0}", Convert.ToBase64String(sha1.ComputeHash(fs)));
                }
            }
        }
    }
}
