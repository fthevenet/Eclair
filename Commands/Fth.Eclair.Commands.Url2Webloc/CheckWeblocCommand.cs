//----------------------------------------------------------------------------- 
// <copyright file=CheckWeblocCommand">
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
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml;
using System.Threading;
using Eclair.Commands.FileSystem;
using Eclair.Exceptions;

namespace Eclair.Commands.Url2Webloc
{
    /// <summary>
    /// Defines a command that checks that the ".webloc" and ".url" files with the same name in the same folder contains the same URLs.
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
       Keyword = "chk",
       Description = "Checks that the URLs contained in the .webloc and .url files matches.",
       Example = "chk \"c:\\temp\\myfolder\" -e=100",
       Parameters = new string[] {  
            @"x:\xxx or \\xxx: The target path in which the files will be checked.",
            @"(Optional) -e=X: The maximum number of non-fatal error tolerated (default=100).",
            @"(Optional)   -v: Display verbose messages.",
            @"(Optional)   -s: Force single-threaded processing."
            },
        ServerNotRequired = true
       )]
    public class CheckWeblocCommand : FileSystemScanCommandBase
    {
        private Regex urlRegEx = new Regex(@"URL\=(?<url>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the CheckWeblocCommand class.
        /// </summary>
        public CheckWeblocCommand()
        {
            this.CanDoParallelProcessing = true;
            this.CanProcessFolders = true;
            this.CanProcessFiles = true;
            this.MaxErrorNumber = 0;
            this.DefaultSearchPattern = "*.url";
        }

        /// <summary>
        /// Checks that the ".webloc" and ".url" files with the same name in the same folder contains the same URLs.
        /// </summary>
        /// <param name="sourcePath">The path of the file to check.</param>
        /// <returns>Returns 1 if a webloc file with the same name as the provided file name exists, 0 otherwise.</returns>
        protected override int ProcessFile(string sourcePath)
        {
            var weblocPath = this.IOHelper.ChangeExtension(sourcePath, "webloc");

            if (this.IsVerbose)
                this.OutputInfo("Checking: {0}", weblocPath);

            if (!this.IOHelper.FileExists(weblocPath))
            {
                this.OutputError("No webloc file found for file {0}.", sourcePath);
            }
            else
            {
                var urlFileUri = extractUriFromUrlShortcut(sourcePath);
                var weblocFileUri = extractUriFromWeblocShortcut(weblocPath);

                if (Uri.Compare(urlFileUri, weblocFileUri, UriComponents.AbsoluteUri, UriFormat.UriEscaped, StringComparison.CurrentCultureIgnoreCase) != 0)
                    this.OutputError("The in {0} and {1} do not match.",
                        weblocPath,
                        sourcePath);
            }
            return 1;
        }

        /// <summary>
        /// Returns the URL contained in the provided webloc file.
        /// </summary>
        /// <param name="weblocPath">The path to the webloc file to extract url from.</param>
        /// <returns>The URL contained in the provided webloc file, as a UrI object.</returns>
        private Uri extractUriFromWeblocShortcut(string weblocPath)
        {
            if (string.IsNullOrEmpty(weblocPath))
                throw new ArgumentNullException("weblocPath");

            using (var fs = this.IOHelper.FileOpen(weblocPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                XDocument webloc = XDocument.Load(fs);
                var urlString = webloc.Element("plist").Element("dict").Element("string").Value;

                return new Uri(urlString, UriKind.Absolute);
            }
        }

        /// <summary>
        /// Returns the URL contained in the provided url shortcut file.
        /// </summary>
        /// <param name="urlPath">The path to the url shortcut file to extract url from.</param>
        /// <returns>The URL contained in the provided url shortcut file, as a UrI object.</returns>
        private Uri extractUriFromUrlShortcut(string urlPath)
        {
            if (string.IsNullOrEmpty(urlPath))
                throw new ArgumentNullException("urlFile");

            using (var fs = this.IOHelper.FileOpen(urlPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                using (var sr = new System.IO.StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        var url = urlRegEx.Match(sr.ReadLine()).Groups["url"];
                        if (url.Success)
                            return new Uri(url.Value, UriKind.Absolute);
                    }
                }
            }
            throw new CommandExecutionException(this, String.Format("Failed to extract url from file {0}", urlPath));
        }       
    }
}
