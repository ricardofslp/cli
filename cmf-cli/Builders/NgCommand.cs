using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Builders;

public class NgCommand : ProcessCommand, IBuildCommand
{
    public string Command { get; set; }
    
    public string[] Args { get; set; }

    public string[] Projects { get; set; }
    
    public override ProcessBuildStep[] GetSteps()
    {
        var args = new List<string>
        {
            this.Command
        };

        if (this.Projects != null)
        {
            // we're building some of the projects
            return this.Projects.Select(projectName => new ProcessBuildStep()
            {
                Command = "ng",
                Args = args.Append(projectName).Concat(this.Args).ToArray(),
                WorkingDirectory = this.WorkingDirectory
            }).ToArray();
        }
        else
        {
            // we're building the entire app
            return new[]
            {
                new ProcessBuildStep()
                {
                    Command = "ng",
                    Args = args.Concat(this.Args).ToArray(),
                    WorkingDirectory = this.WorkingDirectory
                }
            };
        }
    }

    public string DisplayName { get; set; }
    public bool Test { get; set; }
}