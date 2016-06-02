﻿//----------------------------------------------------------------------------- 
// <copyright file=CommandExecutionException">
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
using System.Runtime.Serialization;
using Eclair.Commands;



namespace Eclair.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an error occurs during the execution of a command
    /// </summary>
    [Serializable]
    public class CommandExecutionException : CommandLineInterpretationException
    {
        /// <summary>
        /// The instance of ICommand in which the error originated.
        /// </summary>
        protected ICommand command;

        /// <summary>
        /// Gets the command instance that was being executed when the error occurred.
        /// </summary>
        public ICommand Command
        {
            get { return this.command; }
        }

        /// <summary>
        /// Initializes a new instance of the CommandExecutionException class with the command where the error originated.
        /// </summary>
        /// <param name="command"> the command instance that was being executed when the error occurred.</param>
        public CommandExecutionException(ICommand command)
            : this(command, "An error occurred while executing the command.")
        {          
        }

        /// <summary>
        /// Initializes a new instance of the CommandExecutionException class with the command where the error originated,
        /// and a specified error message.
        /// </summary>
        /// <param name="command"> the command instance that was being executed when the error occurred.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public CommandExecutionException(ICommand command, string message)
            : this(command,message, null)
        {          
        }

        /// <summary>
        /// Initializes a new instance of the CommandExecutionException class with the command where the error originated,
        /// a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="command">The command instance that was being executed when the error occurred.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. </param>
        public CommandExecutionException(ICommand command, string message, Exception innerException)
            : base(message, innerException)
        {
            this.command = command;
        }
    
        /// <summary>
        /// Initializes a new instance of the CommandExecutionException class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected CommandExecutionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {

        }
    }

}

