//----------------------------------------------------------------------------- 
// <copyright file=PersistentListManagerICommand">
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

namespace Eclair.Commands
{  
	/// <summary>
	/// Defines methods and properties that a class representing a ECLAIR command must implement.
	/// </summary>
	public interface ICommand : IDisposable
	{
		/// <summary>
		/// Execute the logic implemented for the specified ECLAIR command. 
		/// </summary>
		/// <param name="context">A CommandContext object that provides the execution context under which the command logic is running.</param>
		/// <param name="outputFile">The path to a file into which the output of the command will be redirected.</param>
		void Execute(CommandContext context, string outputFile);

		/// <summary>
		/// Execute the logic implemented for the specified ECLAIR command.
		/// </summary>
		/// <param name="context">A CommandContext object that provides the execution context under which the command logic is running.</param>
		/// <param name="results">A list of string into which the output of the command will be stored.</param>
		void Execute(CommandContext context, out List<string> results);


		/// <summary>
		/// Execute the logic implemented for the specified ECLAIR command.
		/// </summary>
		/// <param name="context">A CommandContext object that provides the execution context under which the command logic is running.</param>
		void Execute(CommandContext context);

		/// <summary>
		/// Gets the properties of the command.
		/// </summary>
		CommandInfoAttribute CommandInfo { get; }    
	}
}
