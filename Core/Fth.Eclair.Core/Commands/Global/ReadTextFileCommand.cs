//----------------------------------------------------------------------------- 
// <copyright file=ReadTextFileCommand">
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
using System.Threading.Tasks;
using Eclair.Tools;
using System.IO;


namespace Eclair.Commands.Global
{
    [CommandInfo(
           Category = "*",
           Keyword = "rdtxt",
           Description = "Display the content of file",
           Example = "rdtxt c:\readme.txt -latin9",
           Parameters = new string[] { 
                "x:\\xxx:   Local or UNC path  of the file to read.",
                "-utf8:    Use the UTF-8 encoding to read the file (default)",
                "-utf16:   Use the UTF-16 encoding.",
                "-latin9:  Use the Latin 9 (ISO) codepage.", 
                "-win1252: Use the Western European (Windows) codepage.",
                "-usascii: Use the US-ASCII codepage.",
                "-enc=xxx: Specify the encoding to use."
                },
           ServerNotRequired = true
           )]
    class ReadTextFileCommand : CommandBase
    {
        public override void ExecuteCommand(CommandContext context)
        {

            var encoding = getEncoding(context.CommandLineArguments);

            foreach (var path in context.CommandLineArguments.GetAllPathFromArguments())
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException("Cannot find file: " + path);

                using (var r = new StreamReader(path, encoding))
                {
                    while (!r.EndOfStream)
                    {
                        if (context.Environment.CancelPending)
                            return;
                        this.OutputInfo(r.ReadLine());
                    }
                }
            }
        }

        private Encoding getEncoding(List<string> arguments)
        {
          
            if (arguments.ArgumentExists("-utf16"))
                return Encoding.Unicode;

            if (arguments.ArgumentExists("-latin"))
                return Encoding.GetEncoding("iso-8859-15");

            if (arguments.ArgumentExists("-win1252"))
                return Encoding.GetEncoding("Windows-1252");

            if (arguments.ArgumentExists("-usascii"))
                return Encoding.GetEncoding("us-ascii");

            try
            {
                return Encoding.GetEncoding(arguments.GetFirstArgument("-enc", ".*", "utf-8"));
            }
            catch (ArgumentException)
            {
                this.OutputWarning("The provided name is not a valid code page name or the code page indicated by name is not supported by the underlying platform. Default encoder will be used.");
            }

            return Encoding.UTF8;
        }
    }
}
