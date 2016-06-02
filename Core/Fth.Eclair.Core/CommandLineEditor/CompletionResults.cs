//----------------------------------------------------------------------------- 
// <copyright file=CompletionResults">
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
    /// The handler for auto completion.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="pos">The position where text should be displayed.</param>
    /// <returns>The list of CompletionElement objects.</returns>
    internal delegate CompletionResults AutoCompleteHandler(string text, int pos);

    /// <summary>
    /// Provides a collection of CompletionElement objects that matches the criterion for completing a line.
    /// </summary>
    internal class CompletionResults
    {
        /// <summary>
        /// Gets a newly initialized instance of the CompletionResults class.
        /// </summary>
        public static CompletionResults Empty
        {
            get { return new CompletionResults(); }
        }

        /// <summary>
        /// Returns true if at least one result matched, false otherwise.
        /// </summary>
        public bool IsResultFound
        {
            get { return (this.Result.Count > 0); }
        }

        /// <summary>
        /// Gets or sets the list of matching CompletionElement objects.
        /// </summary>
        public List<CompletionElement> Result { get; private set; }

        /// <summary>
        /// Gets or sets the prefix for the results.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Initializes a new instance of the CompletionResults class.
        /// </summary>
        public CompletionResults()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CompletionResults class.
        /// </summary>
        /// <param name="prefix">The prefix for the results.</param>
        /// <param name="result">Existing results to add to the list.</param>
        public CompletionResults(string prefix, IEnumerable<CompletionElement> result)
            : this(prefix)
        {
            Result.AddRange(result);
        }

        /// <summary>
        /// Initializes a new instance of the CompletionResults class.
        /// </summary>
        /// <param name="prefix">The prefix for the results.</param>
        public CompletionResults(string prefix)
        {
            this.Prefix = prefix;
            this.Result = new List<CompletionElement>();
        }
    }
}
