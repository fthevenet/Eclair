//----------------------------------------------------------------------------- 
// <copyright file=ScriptInfo">
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
using System.IO;

namespace Eclair.Commands.Scripting
{

    /// <summary>
    /// Defines and holds the properties of a script being executed by the SHELL\RUN command.
    /// </summary>
    public class ScriptInfo
    {
        /// <summary>
        /// Initializes a new instance of the ScriptInfo class.
        /// </summary>
        /// <param name="file">The FileInfo associated with the script.</param>
        /// <param name="b">The BreakOnError for the script.</param>
        /// <param name="e">The enumerator used to navigate the script lines.</param>
        public ScriptInfo(FileInfo file, bool b, IEnumerator<ScriptLine> e)
        {
            this.BreakOnError = b;
            this.LineEnumerator = e;
            this.FileInfo = file;
        }

        /// <summary>
        /// Gets or sets the FileInfo associated with the script.
        /// </summary>
        public FileInfo FileInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the BreakOnError for the script.
        /// </summary>
        public bool BreakOnError
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or set the enumerator used to navigate the script lines.
        /// </summary>
        public IEnumerator<ScriptLine> LineEnumerator
        {
            get;
            set;
        }

    }
    
}
