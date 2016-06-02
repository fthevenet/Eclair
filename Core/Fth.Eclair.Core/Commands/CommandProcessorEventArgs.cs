//----------------------------------------------------------------------------- 
// <copyright file=CommandProcessorEventArgs">
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

namespace Eclair.Commands
{
    /// <summary>
    /// Represents the argument that is passed to the CommandProcessor events.
    /// </summary>
    public class CommandProcessorEventArgs : EventArgs
    {
        /// <summary>
        ///  Gets the processor's current environment.
        /// </summary>
        public CommandProcessorEnvironment Environment { get; private set; }

        /// <summary>
        /// Initializes a new instance of the CommandProcessorEventArgs class.
        /// </summary>
        /// <param name="environment">The processor's current environment.</param>
        public CommandProcessorEventArgs(CommandProcessorEnvironment environment)
        {
            this.Environment = environment;
        }
    }
}
