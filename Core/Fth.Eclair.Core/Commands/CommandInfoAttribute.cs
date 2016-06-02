//----------------------------------------------------------------------------- 
// <copyright file=CommandInfoAttribute">
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
using System.Text;
using Eclair.Commands;

namespace Eclair.Commands
{
   /// <summary>
   /// Specifies the properties and usage of a ECLAIR command. 
   /// </summary>
   /// <remarks>
    /// In order to be registered as an executable command by the <see cref="CommandFactory"/> instance a class must both implement 
    /// the <see cref="ICommand"/> interface and be decorated by a <see cref="CommandInfoAttribute"/>.
   /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CommandInfoAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfoAttribute"/> class.
        /// </summary>
        public CommandInfoAttribute() 
        {
          this.Parameters = new string[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfoAttribute"/> class.
        /// </summary>
        /// <param name="category">The category for the command.</param>
        /// <param name="commandKeyword">The keyword used by the interpreter to identify the command.</param>
        /// <param name="commandDescription">The description of the command.</param>
        /// <param name="commandExample">A usage example for the command.</param>
        /// <param name="commandParameters">The description of the parameters for the command.</param>
        public CommandInfoAttribute(string category, string commandKeyword, string commandDescription, string commandExample, string commandParameters)
            : this(category, commandKeyword, commandDescription, commandExample, new string[] { commandParameters }, false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfoAttribute"/> class.
        /// </summary>
        /// <param name="category">The category for the command.</param>
        /// <param name="commandKeyword">The keyword used by the interpreter to identify the command.</param>
        /// <param name="commandDescription">The description of the command.</param>
        /// <param name="commandExample">A usage example for the command.</param>
        /// <param name="commandParameters">The description of the parameters for the command.</param>
        public CommandInfoAttribute(string category, string commandKeyword, string commandDescription, string commandExample, string[] commandParameters)
            : this(category, commandKeyword, commandDescription, commandExample, commandParameters, false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfoAttribute"/> class.
        /// </summary>
        /// <param name="category">The category for the command.</param>
        /// <param name="commandKeyword">The keyword used by the interpreter to identify the command.</param>
        /// <param name="commandDescription">The description of the command.</param>
        /// <param name="commandExample">A usage example for the command.</param>
        /// <param name="commandParameters">The description of the parameters for the command.</param>
        /// <param name="serverNotRequired">True if the command can be executed without a connexion to a server, False otherwise.</param>
        public CommandInfoAttribute(string category, string commandKeyword, string commandDescription, string commandExample, string commandParameters, bool serverNotRequired)
            : this(category, commandKeyword, commandDescription, commandExample, new string[] { commandParameters }, serverNotRequired) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfoAttribute"/> class.
        /// </summary>
        /// <param name="category">The category for the command.</param>
        /// <param name="commandKeyword">The keyword used by the interpreter to identify the command.</param>
        /// <param name="commandDescription">The description of the command.</param>
        /// <param name="commandExample">A usage example for the command.</param>
        /// <param name="commandParameters">The description of the parameters for the command.</param>
        /// <param name="serverNotRequired">True if the command can be executed without a connexion to a server, False otherwise.</param>
        public CommandInfoAttribute(string category, string commandKeyword, string commandDescription, string commandExample, string[] commandParameters, bool serverNotRequired)
        {
            this.Category = category;
            this.Keyword = commandKeyword;
            this.Parameters = commandParameters;
            this.Description = commandDescription;
            this.Example = commandExample;
            this.ServerNotRequired = serverNotRequired;
        }

        /// <summary>
        /// Gets or sets the description of the parameters for the command.
        /// </summary>
        /// <remarks>
        /// The content of this property is used to build the in-lined help.
        /// </remarks>
        public string[] Parameters { get;  set; }

        /// <summary>
        /// Gets or sets the category for the command.
        /// </summary>
        public string Category { get;  set; }

        /// <summary>
        /// Gets or sets True if the command can be executed without a connexion to a server, False otherwise.
        /// </summary>
        public bool ServerNotRequired { get;  set; }
             
        /// <summary>
        /// Gets or sets the keyword used by the interpreter to identify the command.
        /// </summary>
        public string Keyword { get;  set; }       

        /// <summary>
        /// Gets or sets the description of the command.
        /// </summary>
        /// <remarks>
        /// The content of this property is used to build the in-line help.
        /// </remarks>
        public string Description { get;  set; }              

        /// <summary>
        /// Gets or sets a usage example for the command.
        /// </summary>
        /// <remarks>
        /// The content of this property is used to build the in-lined help.
        /// </remarks>
        public string Example { get; set; }       

        /// <summary>
        /// Gets or sets the Type of the command class.
        /// </summary>
        public Type CommandType { get; set; }

        /// <summary>
        ///  Gets or sets True if the command should not appears when in-line help is invoked. 
        /// </summary>
        public bool NotBrowsable { get; set; }
    }

}
