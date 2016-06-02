//----------------------------------------------------------------------------- 
// <copyright file=IClientProxy">
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
    /// Defines connection management methods and properties a client proxy class must implement to be used by ECLAIR.
    /// </summary>
    public interface IClientProxy : IDisposable
    {

        /// <summary>
        /// Gets the list of server the client is connected to.
        /// </summary>
        string ServerList { get; }

        /// <summary>
        /// Returns true if the client is logged into a server, false otherwise.
        /// </summary>
        bool IsLoggedIn { get; }

        /// <summary>
        /// Gets the name of the user logged into the server.
        /// </summary>
        string ConnectedUser { get; }

        /// <summary>
        /// Establishes the connection to a server.
        /// </summary>
        /// <param name="args">The arguments that specifies the connection.</param>
        void Connect(List<string> args);

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Gets the actual instance of the client object wrapped by the proxy.
        /// </summary>
        /// <remarks>
        /// In order to use methods specifics to the client object, a command must first cast this property as the actual type 
        /// for the client object.
        /// </remarks>      
        object ClientInstance { get; }

        /// <summary>
        /// Returns true if login was established using integrated Windows authentication, false otherwise.
        /// </summary>
        bool LoggedWithWindowsSession { get; }   
    }

}
