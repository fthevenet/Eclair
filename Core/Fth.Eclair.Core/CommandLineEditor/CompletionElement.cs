//----------------------------------------------------------------------------- 
// <copyright file=CompletionElement">
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

namespace Eclair.CommandLineEditor
{
    /// <summary>
    /// Defines the properties of the textual element displayed during line completion.
    /// </summary>
    internal class CompletionElement
    {
        /// <summary>
        /// Gets or sets the color of the completion text.
        /// </summary>
        public ConsoleColor DisplayColor { get; set; }

        /// <summary>
        /// Gets or sets the value of the completion text.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the prefix for the completion text.
        /// </summary>
        public string DisplayPrefix { get; set; }

        /// <summary>
        /// Gets or sets the suffix for the completion text.
        /// </summary>
        public string DisplaySufix { get; set; }


        /// <summary>
        /// Initializes a new instance of the CompletionElement class.
        /// </summary>
        /// <param name="value">The value of the completion text.</param>
        public CompletionElement(string value)
            : this(value, Console.ForegroundColor, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CompletionElement class.
        /// </summary>
        /// <param name="value">The value of the completion text.</param>
        /// <param name="displayColor">The color of the completion text.</param>
        public CompletionElement(string value, ConsoleColor displayColor)
            : this(value, displayColor, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CompletionElement class.
        /// </summary>
        /// <param name="value">The value of the completion text.</param>
        /// <param name="displayColor">The color of the completion text.</param>
        /// <param name="prefix">The prefix of the completion text.</param>
        /// <param name="sufix">The suffix of the completion text.</param>
        public CompletionElement(string value, ConsoleColor displayColor, string prefix, string sufix)
        {

            this.DisplayColor = displayColor;
            this.Value = value;
            this.DisplayPrefix = prefix;
            this.DisplaySufix = sufix;
        }
    }
}
