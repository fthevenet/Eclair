//----------------------------------------------------------------------------- 
// <copyright file=CommandProcessorRunningMode">
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
    /// Defines the modes in which a CommandProcessor instance may be running under.
    /// </summary>
    public enum CommandProcessorRunningMode
    {
        /// <summary>
        /// Indicates that the command processor is running batches of commands, passed through command line arguments of a via a script.
        /// In this mode, command that wait for an interaction by the end user should be disabled.
        /// </summary>
        Batch,
        /// <summary>
        /// Indicates that the command processor is running interactively, executing commands as they are entered by the end user.
        /// </summary>
        Interactive
    }
}
