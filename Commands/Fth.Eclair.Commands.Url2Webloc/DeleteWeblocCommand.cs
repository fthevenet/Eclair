//----------------------------------------------------------------------------- 
// <copyright file=DeleteWeblocCommand">
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
    /// Defines a command that deletes all \".webloc\" files recursively in the provided path.
    /// </summary>
    /// <remarks>
    /// This command:
    /// <para>
    /// - Does not support parallel processing.
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
       Keyword = "del",
       Description = "Delete all \".webloc\" files recursively in the provided path.",
       Example = "del \"c:\\temp\\myfolder\" -e=100",
       Parameters = new string[] {
  
            @"x:\xxx or \\xxx: The target path in which the webloc files will be deleted.",
            @"(Optional) -e=X: The maximum number of non-fatal error tolerated (default=100).",
            @"(Optional)   -v: Display verbose messages.",
            @"(Optional)   -s: Force single-threaded processing."
            },
        ServerNotRequired = true
       )]
    public class DeleteWeblocCommand : FileSystemScanCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the DeleteWeblocCommand class.
        /// </summary>
        public DeleteWeblocCommand()
        {
            this.CanDoParallelProcessing = true;
            this.CanProcessFolders = true;
            this.CanProcessFiles = true;
            this.MaxErrorNumber = 0;
            this.DefaultSearchPattern = "*.webloc";
        }

        /// <summary>
        /// Deletes the file at the specified path.
        /// </summary>
        /// <param name="sourcePath">The path of the file to delete</param>
        /// <returns>Returns 1 if the file exists, 0 otherwise.</returns>
        protected override int ProcessFile(string sourcePath)
        {
            if (this.IsVerbose)
                this.OutputInfo("Deleting: {0}", sourcePath);

            if (this.IOHelper.FileExists(sourcePath))
            {
                this.IOHelper.FileDelete(sourcePath);
                return 1;
            }
            return 0;
        }
    }
}
