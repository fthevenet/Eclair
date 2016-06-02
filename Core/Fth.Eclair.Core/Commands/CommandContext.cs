//----------------------------------------------------------------------------- 
// <copyright file=CommandContext">
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
    /// Represents the execution context of a ECLAIR command.
    /// </summary>
    public class CommandContext
    {
        /// <summary>
        /// Initializes a new instance of the CommandContext class.
        /// </summary>
        /// <param name="environment">The CommandProcessor environment.</param>
        /// <param name="clientProxy">The proxy to the current client connection.</param>
        /// <param name="commandLineArguments">The command line arguments passed by the user.</param>
        public CommandContext(CommandProcessorEnvironment environment, IClientProxy clientProxy, List<string> commandLineArguments)
        {
            this.Environment = environment;
            this.ClientProxy = clientProxy;
            this.CommandLineArguments = commandLineArguments;

        }

        /// <summary>
        /// Gets the underlying CommandProcessor environment.
        /// </summary>
        public CommandProcessorEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the proxy to the current client connection.
        /// </summary>
        public IClientProxy ClientProxy { get; private set; }

        /// <summary>
        /// Gets the command line arguments passed by the user.
        /// </summary>
        public List<string> CommandLineArguments { get; private set; }
    }
}
