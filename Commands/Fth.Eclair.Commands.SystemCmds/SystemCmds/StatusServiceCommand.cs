//----------------------------------------------------------------------------- 
// <copyright file=StatusServiceCommand">
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
    [CommandInfo(
        "System",
        "statussvc",
        "Display the status of a Windows service",
        "statussvc <\\\\ServiceHost> [serviceName]",
        "none",
        true)]
    public class StatusServiceCommand : ServiceCommandBase
    {

        public override void ControlService(ServiceController svcCtrl)
        {            
            this.OutputInfo("Service {0}{1}: {2}",
               svcCtrl.ServiceName,
               svcCtrl.MachineName != "." ? " on machine " + svcCtrl.MachineName : string.Empty,
               svcCtrl.Status.ToString());
        }
    }
}              
