﻿//----------------------------------------------------------------------------- 
// <copyright file=ServiceCommandBase">
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
using Eclair.Tools;
using System.Reflection;
using System.ServiceProcess;

namespace Eclair.Commands.SystemCmds
{
   
    public abstract class ServiceCommandBase : CommandBase
    {


        public override void ExecuteCommand(CommandContext c)
        {

            if (c.CommandLineArguments.Count == 0)
                throw new ArgumentNullException();

            string machine = c.CommandLineArguments.FirstOrDefault(s => s.StartsWith("\\\\", StringComparison.InvariantCultureIgnoreCase));
            string svcName = c.CommandLineArguments.FirstOrDefault(s => !s.StartsWith("\\\\", StringComparison.InvariantCultureIgnoreCase));

            ServiceController svcCtrl = new ServiceController(svcName);
            if (!string.IsNullOrEmpty(machine))
                svcCtrl.MachineName = machine.Replace("\\\\", "");

            this.ControlService(svcCtrl);

        }

        public abstract void ControlService(ServiceController svcCtrl);
    }
}              
