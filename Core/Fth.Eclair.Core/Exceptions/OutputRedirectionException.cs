//----------------------------------------------------------------------------- 
// <copyright file=OutputRedirectionException">
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
using Eclair.Commands;


namespace Eclair.Exceptions
{
	/// <summary>
	/// The exception that is thrown when an error occurs while redirecting the output of a command.
	/// </summary>
	[Serializable]
	public class OutputRedirectionException : CommandLineInterpretationException
	{
		/// <summary>
		/// Initializes a new instance of the OutputRedirectionException class.
		/// </summary>
		public OutputRedirectionException()
			: this("Failed to redirect output", null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the OutputRedirectionException class.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		public OutputRedirectionException(string message)
			: this(message, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the OutputRedirectionException class.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception. </param>
		public OutputRedirectionException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the OutputRedirectionException class with serialized data.
		/// </summary>
		/// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
		protected OutputRedirectionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
	}

}

