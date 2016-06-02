//----------------------------------------------------------------------------- 
// <copyright file=CleanWeblocCommand">
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
    /// Defines a command that deletes all ".webloc" files for which a corresponding ".url" file cannot be found.
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
       Keyword = "clean",
       Description = "Delete \".webloc\" files that are not associated to a \".url\" file in the provided path.",
       Example = "clean \"c:\\temp\\myfolder\" -e=100",
       Parameters = new string[] {  
            @"x:\xxx or \\xxx: The target path in which the webloc files will be cleaned.",
            @"(Optional) -e=X: The maximum number of non-fatal error tolerated (default=100).",
            @"(Optional)   -v: Display verbose messages.",
            @"(Optional)   -s: Force single-threaded processing."
        },
        ServerNotRequired = true
       )]
    public class CleanWeblocCommand : FileSystemScanCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the CleanWeblocCommand class.
        /// </summary>
        public CleanWeblocCommand()
        {
            this.CanDoParallelProcessing = true;
            this.CanProcessFolders = true;
            this.CanProcessFiles = false;
            this.MaxErrorNumber = 0;
            this.DefaultSearchPattern = "*.webloc";
        }

        /// <summary>
        /// Deletes the file at the provided path if there doesn't exists a file with the same path save for a ".url" extention.
        /// </summary>
        /// <param name="sourcePath">The path of the file to clean.</param>
        /// <returns>Returns 1 if the file was deleted, 0 otherwise.</returns>
        protected override int ProcessFile(string sourcePath)
        {
            if (!this.IOHelper.FileExists(this.IOHelper.ChangeExtension(sourcePath, "url")))
            {
                if (this.IsVerbose)
                    this.OutputInfo("Cleaning: {0}", sourcePath);

                this.IOHelper.FileDelete(sourcePath);
                return 1;
            }
            return 0;
        }
    }
}
