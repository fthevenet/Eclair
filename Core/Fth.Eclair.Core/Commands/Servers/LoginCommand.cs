//----------------------------------------------------------------------------- 
// <copyright file=LoginCommand">
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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Eclair.Exceptions;
using Eclair.Tools;

namespace Eclair.Commands.Servers
{
    /// <summary>
    /// Represents a command that attempts to establish a connection to a server.
    /// </summary>
    [CommandInfo(
      "Servers",
      "login",
      "Log on to server",
      "login [*|domain\\username,password] [Server]<;Server2;Server3>",
      "",
      true)]
    public class loginCommand : CommandBase
    {
        /// <summary>
        /// The method invoked by the CommandProcessor to start the execution of the command.
        /// </summary>
        /// <param name="c">The execution context of the current CommandProcessor instance.</param>
        public override void ExecuteCommand(CommandContext c)
        {
            if (c.ClientProxy.IsLoggedIn)
            {
                c.ClientProxy.Disconnect();
                this.OutputDebug("Client logged off");
            }

            c.ClientProxy.Connect(c.CommandLineArguments);

            this.OutputDebug("Client logged in");
            this.OutputDebug(c.CommandLineArguments.Aggregate("", (s, p) => p + ", " + s));

            //string cnxStr = string.Empty;
            //string server = string.Empty;
            //string pwd = string.Empty;
            //string user = string.Empty;

            //if (c.CommandLineArguments.Count == 1)
            //    server = c.CommandLineArguments[0];

            //if (c.CommandLineArguments.Count == 2)
            //{
            //    user = c.CommandLineArguments[0];
            //    server = c.CommandLineArguments[1];
            //}

            //if (c.CommandLineArguments.Count >= 3)
            //    throw new ArgumentException("login command does not take 3 or more arguments");

            //if (String.IsNullOrEmpty(server))
            //{
            //    Console.Write("Server: ");
            //    server = Console.ReadLine();
            //}
            //if (String.IsNullOrEmpty(user))
            //{
            //    Console.Write("User Name: ");
            //    user = Console.ReadLine();
            //}

            //if (user != "*" && !user.Contains(','))
            //{
            //    pwd = ",";
            //    Console.Write("Password:");
            //    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            //    while (keyInfo.Key != ConsoleKey.Enter)
            //    {
            //        if (c.Environment.CancelPending)
            //            return;

            //        pwd += keyInfo.KeyChar;

            //        keyInfo = Console.ReadKey(true);
            //    }
            //}
            //Console.WriteLine();

            //cnxStr = String.Format("-login:{0}{1}@{2}", user, pwd, server);
            //c.ClientProxy.Connect(new List<string>() { cnxStr });

            //this.OutputDebug("Client logged in");
            //if (c.CommandLineArguments.Count > 0)
            //    this.OutputDebug(c.CommandLineArguments.Aggregate((s, p) => p + ", " + s));

        }
    }
}
