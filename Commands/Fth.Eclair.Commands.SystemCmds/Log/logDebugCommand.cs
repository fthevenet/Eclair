using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Eclair.Exceptions;

namespace Eclair.Commands.Log
{
      [CommandInfo(
        "Log", 
        "debug",
        "Write a debug message in log",
        "debug [Message]", "Log\\debug \"Hello World!\"",
        true)]
    public class LogDebugCommand : CommandBase
    {
          public override void ExecuteCommand(CommandContext c)
          {
              if (c.CommandLineArguments.Count < 1)
                  throw new ArgumentNullException();

              this.OutputDebug(c.CommandLineArguments.Aggregate("",(s, t) => s + " " + t));
          }
    }
}
