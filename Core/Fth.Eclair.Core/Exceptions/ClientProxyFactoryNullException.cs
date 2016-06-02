//----------------------------------------------------------------------------- 
// <copyright file=ClientProxyFactoryNullException">
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
using System.Runtime.Serialization;

namespace Eclair.Exceptions
{
    /// <summary>
    /// The exception that is thrown when no client proxy factory is available to the CommandProcessor.
    /// </summary>
    [Serializable]
    public class ClientProxyFactoryNullException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ClientProxyFactoryNullException class.
        /// </summary>
        public ClientProxyFactoryNullException()
            : this("No client proxy factory available: a command library containing a class based on ClientProxyFactoryBase must be loaded", null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ClientProxyFactoryNullException class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ClientProxyFactoryNullException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ClientProxyFactoryNullException class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. </param>
        public ClientProxyFactoryNullException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ClientProxyFactoryNullException class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected ClientProxyFactoryNullException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
