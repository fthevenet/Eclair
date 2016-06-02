using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Eclair.Exceptions;

namespace Eclair.Commands.Log
{
      [CommandInfo(
        "Log", 
        "warn",
        "Write a warning message in log",
        "warn [Message]", "Log\\warn \"Hello World!\"",
        true)]
    public class logWarnCommand : CommandBase
    {

          public override void ExecuteCommand(CommandContext c)
          {
              if (c.CommandLineArguments.Count < 1)
                  throw new ArgumentNullException();

              this.OutputWarning(c.CommandLineArguments.Aggregate("",(s, t) => s + " " + t));

          }


    }
}
