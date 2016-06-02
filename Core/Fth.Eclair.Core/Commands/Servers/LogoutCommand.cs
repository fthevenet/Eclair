//----------------------------------------------------------------------------- 
// <copyright file=LogoutCommand">
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
using Eclair.Exceptions;
using Eclair.Tools;

namespace Eclair.Commands.Servers
{

    /// <summary>
    ///  Represents a command that closes an existing connection to a server.
    /// </summary>
      [CommandInfo(
          "Servers",
        "logout",
        "Logout",
        "logout",
        new string[] {  "none" },
        false)]
    public class logoutCommand : CommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="c">The execution context of the current CommandProcessor instance.</param>
          public override void ExecuteCommand(CommandContext c)
          {
              c.ClientProxy.Disconnect();
              this.OutputInfo("Client logged off");
          }      
    }
}
