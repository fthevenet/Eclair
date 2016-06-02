using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Eclair.Exceptions;

namespace Eclair.Commands.Log
{
      [CommandInfo(
        "Log", 
        "error",
        "Write an error message in log",
        "error [Message]", "Log\\error \"Hello World!\"",
        true)]
    public class logErrorCommand : CommandBase
    {

          public override void ExecuteCommand(CommandContext c)
          {
              if (c.CommandLineArguments.Count < 1)
                  throw new ArgumentNullException();

              this.OutputError(c.CommandLineArguments.Aggregate("",(s, t) => s + " " + t));

          }


    }
}
