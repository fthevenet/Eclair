//----------------------------------------------------------------------------- 
// <copyright file=scriptLine">
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

namespace Eclair.Commands.Scripting
{
    /// <summary>
    /// A structure that represents a line of script.
    /// </summary>
    public struct ScriptLine
    {
        /// <summary>
        /// The current position in the underlying stream.
        /// </summary>
        public long StreamPosistion;

        /// <summary>
        /// The line number in the script.
        /// </summary>
        public long Number;

        /// <summary>
        /// The label associated with the line of script.
        /// </summary>
        public string Label;

        /// <summary>
        /// The content of the line of script.
        /// </summary>
        public string Value;
    }

}
