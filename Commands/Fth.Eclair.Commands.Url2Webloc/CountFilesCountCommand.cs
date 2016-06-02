//----------------------------------------------------------------------------- 
// <copyright file=CountFilesCountCommand">
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
    /// Defines a command that returns the number of ".webloc" or ".url" files recursively in the provided folder and its subfolders.
    /// </summary>
    /// <remarks>
    /// This command:
    /// <para>
    /// - Does not support parallel processing.
    /// </para>
    /// <para>
    /// - Does not accepts single files as arguments.
    /// </para>
    /// <para>
    /// - Accepts single folders as arguments.
    /// </para>
    /// </remarks>    
    [CommandInfo(
       Category = "Webloc",
       Keyword = "count",
       Description = "Count the number of \"*.url\" or \"*.webloc\" files in the provided path",
       Example = "count [URL|WEBLOC] \"c:\\temp\\myfolder\"",
       Parameters = new string[] {
  
            @"x:\xxx or \\xxx: The target path in which the files will be counted.",
            "      URL|WEBLOC: Specify the type of files to count.",  
            @"(Optional) -e=X: The maximum number of non-fatal error tolerated (default=100).",
            @"(Optional)   -v: Display verbose messages.",
            @"(Optional)   -s: Force single-threaded processing."         
            },
        ServerNotRequired = true
       )]
    public class CountFilesCountCommand : FileSystemScanCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the CountFilesCountCommand class.
        /// </summary>
        public CountFilesCountCommand()
        {

            this.CanDoParallelProcessing = true;
            this.CanProcessFolders = true;
            this.CanProcessFiles = false;
            this.MaxErrorNumber = 0;
            this.DefaultSearchPattern = string.Empty;
        }

        /// <summary>
        ///  Parses the command line and sets the SearchPattern property.
        /// </summary>
        /// <param name="context">The execution context of the current CommandProcessor instance.</param>
        protected override void ParseExtraParameters(CommandContext context)
        {
            this.DefaultSearchPattern = null;

            if (context.CommandLineArguments.ArgumentExists("URL"))
                this.DefaultSearchPattern = "*.url";

            if (context.CommandLineArguments.ArgumentExists("WEBLOC"))
                this.DefaultSearchPattern = "*.webloc";

            if (String.IsNullOrEmpty(DefaultSearchPattern))
                throw new ArgumentException("The search pattern for this command cannot be null or empty.");
            
            base.ParseExtraParameters(context);
        }

        /// <summary>
        /// Returns 1 if a file exists at the provided path, 0 otherwise.
        /// </summary>
        /// <param name="sourcePath">The path of the file to count.</param>
        /// <returns>Returns 1 if a file exists at the provided path, 0 otherwise.</returns>
        protected override int ProcessFile(string sourcePath)
        {
            if (this.IsVerbose)
                this.OutputInfo("Found: {0}", sourcePath);

            if (this.IOHelper.FileExists(sourcePath))
                return 1;

            return 0;
        }
    }
}