//----------------------------------------------------------------------------- 
// <copyright file=GenerateWeblocCommand">
//   Copyright (c) Frederic Thevenet. All Rights Reserved
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
using Eclair.Tools;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml;
using System.Threading;
using Eclair.Commands.FileSystem;
using Eclair.Exceptions;

namespace Eclair.Commands.Url2Webloc
{
    /// <summary>
    /// Defines a command that generates an XML based MacOS compatible url shortcut file (".webloc") for each Windows url shortcut file
    /// it encounters in the provided paths.
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
       Category = "Webloc",
       Keyword = "gen",
       Description = "Generates a \".webloc\" file the provided path. If the path points to a folder, the command will generate webloc for all url files in the folder and its subfolders.",
       Example = "gen \"c:\\temp\\myfolder\" -e=100 -t -o",
       Parameters = new string[] {  
            @"x:\xxx or \\xxx: The target path in which the webloc files will be generated.",
            @"(Optional) -e=X: The maximum number of non-fatal error tolerated (default=100).",
            @"(Optional)   -v: Display verbose messages.",                       
            @"(Optional)   -o: Overwrite existing webloc files.",
            @"(Optional)   -s: Force single-threaded processing."
            },
        ServerNotRequired = true
       )]
    public class GenerateWeblocCommand : FileSystemScanCommandBase
    {
        private Regex urlRegEx = new Regex(@"URL\=(?<url>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private bool overwrite;

        /// <summary>
        /// Initializes a new instance of the GenerateWeblocCommand class.
        /// </summary>
        public GenerateWeblocCommand()
        {
            this.CanDoParallelProcessing = true;
            this.CanProcessFolders = true;
            this.CanProcessFiles = true;
            this.MaxErrorNumber = 0;
            this.DefaultSearchPattern = "*.url";
        }

        /// <summary>
        /// Parses the command line and sets the "overwrite" flag.
        /// </summary>
        /// <param name="context">The execution context of the current CommandProcessor instance.</param>
        protected override void ParseExtraParameters(CommandContext context)
        {
            this.overwrite = context.CommandLineArguments.ArgumentExists("-o");

            base.ParseExtraParameters(context);
        }

        /// <summary>
        /// Generates a ".webloc" file equivalent to the provided ".url" file.
        /// </summary>
        /// <param name="sourcePath">The path to the source ".url" file.</param>
        /// <returns>Returns 1 if the file was generated (i.e. did not already exist or overwrite is true), 0 otherwise.</returns>
        protected override int ProcessFile(string sourcePath)
        {
            var targetPath = this.IOHelper.ChangeExtension(sourcePath, "webloc");
            if (overwrite || !this.IOHelper.FileExists(targetPath))
            {
                if (this.IsVerbose)
                    this.OutputInfo("Generating: {0}", targetPath);

                generateWeblocFile(targetPath, extractUrlFromShortcutFile(sourcePath));
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Extracts the URL from a Windows url shortcut file.
        /// </summary>
        /// <param name="sourcePath">The path to the source ".url" file.</param>
        /// <returns>The extracted URL, as an escaped string.</returns>
        private string extractUrlFromShortcutFile(string sourcePath)
        {
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentNullException("urlFile");

            using (var fs = this.IOHelper.FileOpen(sourcePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        var url = urlRegEx.Match(sr.ReadLine()).Groups["url"];
                        if (url.Success)
                            return url.Value;
                    }
                }
            }
            throw new CommandExecutionException(this, String.Format("Failed to extract url data from file {0}", sourcePath));
        }

        /// <summary>
        /// Creates an XML based MacOS compatible url shortcut file for the provided URL.
        /// </summary>
        /// <param name="targetPath">A string that contains the path of the file to create.</param>
        /// <param name="targetUrl">The URL to include in the shortcut.</param>
        private void generateWeblocFile(string targetPath, string targetUrl)
        {
            if (targetPath == null)
                throw new ArgumentNullException("targetPath");

            if (targetUrl == null)
                throw new ArgumentNullException("targetUri");

            if (!Uri.IsWellFormedUriString(targetUrl, UriKind.Absolute))
                targetUrl = Uri.EscapeUriString(targetUrl);

            XDocument webloc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XDocumentType("plist", "-//Apple//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null),
                new XElement("plist",
                    new XAttribute("version", "1.0"),
                    new XElement("dict",
                        new XElement("key", "URL"),
                        new XElement("string", targetUrl))));

            using (var fs = this.IOHelper.FileOpen(targetPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                webloc.Save(fs);
            }
        }
    } 
}
