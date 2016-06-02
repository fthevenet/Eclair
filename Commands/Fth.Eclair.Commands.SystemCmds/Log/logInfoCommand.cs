using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Eclair.Exceptions;

namespace Eclair.Commands.Log
{
      [CommandInfo(
        "Log", 
        "info",
        "Write an info message in log",
        "info [Message]", "Log\\info \"Hello World!\"",
        true)]
    public class logInfoCommand : CommandBase
    {

          public override void ExecuteCommand(CommandContext c)
          {
              if (c.CommandLineArguments.Count < 1)
                  throw new ArgumentNullException();

              this.OutputInfo(c.CommandLineArguments.Aggregate("",(s, t) => s + " " + t));

          }


    }
}
