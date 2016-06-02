//----------------------------------------------------------------------------- 
// <copyright file=UserAbortedProcessingException">
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
using System.Threading.Tasks;
using Eclair.Commands;

namespace Eclair.Exceptions
{

    /// <summary>
    /// The Exception that is thrown when the end-user aborts the processing of a command.
    /// </summary>
    public class UserAbortedProcessingException : CommandExecutionException
    {

        /// <summary>
        /// Initializes a new instance of the UserAbortedProcessingException class with the command where the error originated.
        /// </summary>
        /// <param name="command"> the command instance that was being executed when the error occurred.</param>
        public UserAbortedProcessingException(ICommand command)
            : base(command, "The command was aborted by the end-user.")
        {          
        }

        /// <summary>
        /// Initializes a new instance of the UserAbortedProcessingException class with the command where the error originated,
        /// and a specified error message.
        /// </summary>
        /// <param name="command"> the command instance that was being executed when the error occurred.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public UserAbortedProcessingException(ICommand command, string message)
            : base(command,message, null)
        {          
        }

        /// <summary>
        /// Initializes a new instance of the UserAbortedProcessingException class with the command where the error originated,
        /// a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="command">The command instance that was being executed when the error occurred.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. </param>
        public UserAbortedProcessingException(ICommand command, string message, Exception innerException)
            : base(command, message, innerException)
        {         
        }
    
        /// <summary>
        /// Initializes a new instance of the UserAbortedProcessingException class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected UserAbortedProcessingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
