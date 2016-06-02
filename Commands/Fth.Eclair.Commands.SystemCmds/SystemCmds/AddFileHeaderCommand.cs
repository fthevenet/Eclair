//----------------------------------------------------------------------------- 
// <copyright file=AddFileHeaderCommand">
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
using Eclair.Commands.FileSystem;
using Eclair.Exceptions;
using Eclair.Tools;
using System.IO;

namespace Eclair.Commands.SystemCmds
{
    /// <summary>
    /// Defines a command that adds a copyright header to a given C# file or all files in a folder and its subfolders.
    /// </summary>
    /// <remarks>
    /// This command:
    /// <para>
    /// - Supports parallel processing.
    /// </para>
    /// <para>
    /// - Accepts single files as arguments.
    /// </para>
    /// <para>
    /// - Accepts single folders as arguments.
    /// </para>
    /// </remarks>
    [CommandInfo(
       Category = "System",
       Keyword = "header",
       Description = "Add a copyright header to a given C# file or all files in a folder and its subfolders.",
       Example = "head \"c:\\temp\\myfolder\" -e=100 enc=utf-8",
       Parameters = new string[] {  
            @"x:\xxx or \\xxx: The target path in which the files will be modified.",
            @"(Optional) -e=X: The maximum number of non-fatal error tolerated (default=0).",                          
            @"(Optional)   -s: Force single-threaded processing.",
            @"(Optional)  -nb: Don't backup original file (*.bak).",
            @"'Optional)  enc: Specify the encoding."
            },
        ServerNotRequired = true
       )]
    public class AddFileHeaderCommand : FileSystemScanCommandBase
    {
        private bool doBackup = true;
        private Encoding encoding = Encoding.UTF8;
        public AddFileHeaderCommand()
        {
            this.CanDoParallelProcessing = true;
            this.CanProcessFolders = true;
            this.CanProcessFiles = true;
            this.MaxErrorNumber = 0;
            this.DefaultSearchPattern = "*.cs";
        }

        protected override void ParseExtraParameters(CommandContext context)
        {
            this.doBackup = !context.CommandLineArguments.ArgumentExists("-nb");
            try
            {
                this.encoding = Encoding.GetEncoding(context.CommandLineArguments.GetFirstArgument("enc", ".*", "utf-8"));
            }
            catch (ArgumentException)
            {
                this.OutputWarning("The provided name is not a valid code page name or the code page indicated by name is not supported by the underlying platform. Default encoder will be used.");
            }
            base.ParseExtraParameters(context);
        }

        protected override int ProcessFile(string sourcePath)
        {
            this.OutputInfo("Adding header to: {0}", sourcePath);
            var newPath = sourcePath + ".tmp";

            if (File.Exists(newPath))
                File.Delete(newPath);

            using (var targetWriter = new StreamWriter(newPath, false, this.encoding))
            {
                foreach (var line in Properties.Settings.Default.Header)
                {
                    var headerLine = line;
                    headerLine = headerLine.Replace("$safeitemrootname$", Path.GetFileNameWithoutExtension(sourcePath));
                    targetWriter.WriteLine(headerLine);
                }
                targetWriter.WriteLine();
                int bufferSize = 4096;
                using (var sourceStream = new StreamReader(sourcePath, encoding, true, bufferSize * sizeof(char)))
                {
                    var buffer = new char[8192];
                    var charRead = 0;
                    while ((charRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        targetWriter.Write(buffer, 0, charRead);
                }
            }
            if (doBackup)
            {
                var backPath = sourcePath + ".bak";
                if (File.Exists(backPath))
                    File.Delete(backPath);
                File.Move(sourcePath, backPath);
            }
            else
            {
                File.Delete(sourcePath);
            }
            File.Move(newPath, sourcePath);
            return 1;
        }
    }
}
