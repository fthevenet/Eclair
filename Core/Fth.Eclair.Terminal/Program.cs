//----------------------------------------------------------------------------- 
// <copyright file=Program">
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
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Eclair;
using Eclair.Commands;
using Eclair.Tools;
using Eclair.Exceptions;
using log4net;
using log4net.Core;
using log4net.Config;
using System.Threading;


namespace Eclair.Terminal
{   
	/// <summary>
	/// Represent the entry point class for the executable.
	/// </summary>
	public class Program
	{
	   
		private static readonly ILog logger = LogManager.GetLogger(typeof(Program));
	  
		 private static void showUsage()
		{
			string exeName = Assembly.GetEntryAssembly().GetName().Name;
			List<string> help = new List<string>()
				{
					String.Format("\nUSAGE:\t{0} <option> [Command] <Argument1> <Argument2> ...\n", exeName),
					"Options:",
					"   -nologo: Do not display copyright information on startup.",  
					"   -forceInteractive: Do not close the shell after a command has been executed.",
					"   -noStartUpScript: Do not run the start-up script.",
					"   -IoRedirected: Indicates that the shell's I/O are being redirected.",
					"",
					String.Format("For detailled help on a command, type {0} help [Command]\n", exeName)
				};

			help.ForEach((s) => { CommandProcessor.TerminalOutput.Info(s); });
		}

		 private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		 {
			 if (logger.IsDebugEnabled)
				 logger.DebugFormat("Control + C pressed");

			 e.Cancel = ExecutionContextFactory.Instance.SignalCancel();
		 }

		private static void Main(string[] args)
		{
			bool runInConsole = false;

			try
			{
				// log4net.Util.LogLog.InternalDebugging = true;
				XmlConfigurator.Configure();

				Console.CancelKeyPress += Console_CancelKeyPress;
				runInConsole = !args.Contains("-IoRedirected", new CiEqualityComparer());

				if (args.Length > 0 && ((args[0] == "-?") || (args[0].ToUpper() == "-H") || (args[0] == "/?") || (args[0].ToUpper() == "/H")))
				{
					showUsage();
					return;
				}

				if (logger.IsDebugEnabled && args.Length > 0)
					logger.DebugFormat("Build new command processor with argument: {0}", args.Aggregate((p, s) => p + " " + s));

				using (CommandProcessor cp = CommandProcessor.Create(runInConsole, args.ToList()))
				{
					if (cp.IsArgsContainCommands)
					{
						if (logger.IsDebugEnabled)
							logger.Debug("Starting command processor in batch mode");

						cp.Run();

						if (!cp.ForceInteractiveMode)
							return;
					}

					if (logger.IsDebugEnabled)
						logger.Debug("Starting command processor in interactive mode");
					cp.Start();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("\n");
				logger.Fatal("An unrecoverable error has occured:\n", ex);
				Console.WriteLine();

				if (runInConsole)
				{
					int row = Console.CursorTop;
					for (int i = 30; i > 0; i--)
					{

						Console.WriteLine("The application will terminate in: {0}", i.ToString("D2"));
						Console.SetCursorPosition(0, row);
						System.Threading.Thread.Sleep(1000);

					}
				}
			}
		}
	}
}
