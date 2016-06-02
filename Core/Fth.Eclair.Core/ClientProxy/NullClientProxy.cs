//----------------------------------------------------------------------------- 
// <copyright file=NullClientProxy">
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
    /// Represents a dummy client proxy, that doesn't connect to a server.
    /// </summary>
    /// <remarks>
    /// It designed to be used as a placeholder so that ECLAIR can be used in a context where no connection to a server is needed. 
    /// </remarks>
    public class NullClientProxy: IClientProxy
    {
        /// <summary>
        /// Always returns an empty string.
        /// </summary>
        public string ServerList
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Always returns false.
        /// </summary>
        public bool IsLoggedIn
        {
            get { return false; }
        }

        /// <summary>
        ///  Always returns an empty string.
        /// </summary>
        public string ConnectedUser
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Throws a NotImplementedException.
        /// </summary>
        /// <param name="args">The arguments that specifies the connection.</param>
        public void Connect(List<string> args)
        {
            throw new NotImplementedException("No client proxy factory was provided: the Connect method is not available.");
        }

        /// <summary>
        /// Throws a NotImplementedException.
        /// </summary>
        public void Disconnect()
        {
            throw new NotImplementedException("No client proxy factory was provided: the Disconnect method is not available.");
        }

        /// <summary>
        /// Always returns null.
        /// </summary>
        public object ClientInstance
        {
            get { return null; }
        }

        /// <summary>
        /// Always returns false. 
        /// </summary>
        public bool LoggedWithWindowsSession
        {
            get { return false; }
        }

        /// <summary>
        /// Disposes the current instance.
        /// </summary>
        public void Dispose()
        {
            
        }
    }
}
