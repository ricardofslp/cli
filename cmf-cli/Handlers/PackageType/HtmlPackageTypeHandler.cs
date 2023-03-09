
using System;
using System.Collections.Generic;
using Cmf.CLI.Builders;
using Cmf.CLI.Commands.restore;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    /// Handler for UI packages
    /// </summary>
    /// <seealso cref="PresentationPackageTypeHandler" />
    public class HtmlPackageTypeHandler : PresentationPackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage"></param>
        public HtmlPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            List<Step> deploySteps; 
            var minimumVersion = new Version("10.0.0");
            // TODO: check if this is where this should be
            // var projectConfig = FileSystemUtilities.ReadProjectConfig(this.fileSystem);
            // var mesVersion = projectConfig.RootElement.GetProperty("MESVersion").GetString();
            var mesVersion = "10.0.0"; 
            var targetVersion = new Version(mesVersion!);
            
            if (targetVersion.CompareTo(minimumVersion) < 0)
            {
                // Gulp managed package
                deploySteps = new List<Step>
                {
                    new Step(StepType.DeployFiles)
                    {
                        ContentPath = "bundles/**"
                    }
                };
                
                BuildSteps = new IBuildCommand[]
                {
                    new ExecuteCommand<RestoreCommand>()
                    {
                        Command = new RestoreCommand(),
                        DisplayName = "cmf restore",
                        Execute = command =>
                        {
                            command.Execute(cmfPackage.GetFileInfo().Directory, null);
                        }
                    },
                    new NPMCommand()
                    {
                        DisplayName = "NPM Install",
                        Command  = "install",
                        Args = new []{ "--force" },
                        WorkingDirectory = cmfPackage.GetFileInfo().Directory
                    },
                    new GulpCommand()
                    {
                        GulpFile = "gulpfile.js",
                        Task = "install",
                        DisplayName = "Gulp Install",
                        GulpJS = "node_modules/gulp/bin/gulp.js",
                        Args = new [] { "--update" },
                        WorkingDirectory = cmfPackage.GetFileInfo().Directory
                    },
                    new GulpCommand()
                    {
                        GulpFile = "gulpfile.js",
                        Task = "build",
                        DisplayName = "Gulp Build",
                        GulpJS = "node_modules/gulp/bin/gulp.js",
                        Args = new [] { "--production" , "--dist", "--brotli"},
                        WorkingDirectory = cmfPackage.GetFileInfo().Directory
                    },
                };
            }
            else
            {
                // @angular-cli managed package
                deploySteps = new List<Step>
                {
                    new Step(StepType.DeployFiles)
                    {
                        ContentPath = "**"
                    }
                };
                
                BuildSteps = new IBuildCommand[]
                {
                    new ExecuteCommand<RestoreCommand>()
                    {
                        Command = new RestoreCommand(),
                        DisplayName = "cmf restore",
                        Execute = command =>
                        {
                            command.Execute(cmfPackage.GetFileInfo().Directory, null);
                        }
                    },
                    new NgCommand()
                    {
                        Command = "build"
                        // Projects must contain projects in the dependency order
                    },
                    new NgCommand()
                    {
                        Command = "build"
                    }
                };
            }
            
            cmfPackage.SetDefaultValues
            (
                targetDirectory:
                    "UI/Html",
                targetLayer:
                    "ui",
                steps:
                    deploySteps
            );

            cmfPackage.DFPackageType = PackageType.Presentation;
        }
    }
}