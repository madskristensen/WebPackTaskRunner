using Microsoft.VisualStudio.TaskRunnerExplorer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WebPackTaskRunner
{
    [TaskRunnerExport("webpack.config.js", "webpack.config.babel.js", "webpack.config.ts", "webpack.config.coffee")]
    internal class TaskRunner : ITaskRunner
    {
        private static ImageSource _icon;
        private List<ITaskRunnerOption> _options = null;

        public TaskRunner()
        {
            if (_icon == null)
            {
                string folder = GetExecutableFolder();
                _icon = new BitmapImage(new Uri(Path.Combine(folder, "Resources\\logo.png")));
            }
        }

        private void InitializeWebPackRunnerOptions()
        {
            _options = new List<ITaskRunnerOption>
            {
                new TaskRunnerOption("Display Modules", PackageIds.cmdDisplayModules, PackageGuids.guidWebPackPackageCmdSet, false, "--display-modules"),
                new TaskRunnerOption("Display Reasons", PackageIds.cmdDisplayReasons, PackageGuids.guidWebPackPackageCmdSet, false, "--display-reasons"),
                new TaskRunnerOption("Display Chunks", PackageIds.cmdDisplayChunks, PackageGuids.guidWebPackPackageCmdSet, false, "--display-chunks"),
                new TaskRunnerOption("Display Error Details", PackageIds.cmdDisplayErrorDetails, PackageGuids.guidWebPackPackageCmdSet, false, "--display-error-details"),
                new TaskRunnerOption("Bail", PackageIds.cmdBail, PackageGuids.guidWebPackPackageCmdSet, false, "--bail"),
                new TaskRunnerOption("Inline", PackageIds.cmdInline, PackageGuids.guidWebPackPackageCmdSet, false, "--inline"),
                new TaskRunnerOption("History API Fallback", PackageIds.cmdHistoryApi, PackageGuids.guidWebPackPackageCmdSet, false, "--history-api-fallback")
            };
        }

        public List<ITaskRunnerOption> Options
        {
            get
            {
                if (_options == null)
                {
                    InitializeWebPackRunnerOptions();
                }

                return _options;
            }
        }

        public async Task<ITaskRunnerConfig> ParseConfig(ITaskRunnerCommandContext context, string configPath)
        {
            return await Task.Run(() =>
            {
                ITaskRunnerNode hierarchy = LoadHierarchy(configPath);

                return new TaskRunnerConfig(context, hierarchy, _icon);
            });
        }

        private ITaskRunnerNode LoadHierarchy(string configPath)
        {
            string configFileName = Path.GetFileName(configPath);
            string cwd = Path.GetDirectoryName(configPath);

            ITaskRunnerNode root = new TaskRunnerNode("WebPack");

            const string DEVELOPMENT_TASK_NAME_OLD = "Development (old)";
            const string PRODUCTION_TASK_NAME_OLD = "Production (old)";
            const string DEVELOPMENT_TASK_NAME = "Development";
            const string PRODUCTION_TASK_NAME = "Production";

            // Run
            TaskRunnerNode build = new TaskRunnerNode("Run", false);
            TaskRunnerNode buildDevOld = CreateTask(configFileName, cwd, $"{build.Name} - {DEVELOPMENT_TASK_NAME_OLD}", "Runs 'webpack '", "/c SET NODE_ENV=development&& webpack --color");
            build.Children.Add(buildDevOld);

            TaskRunnerNode buildProdOld = CreateTask(configFileName, cwd, $"{build.Name} - {PRODUCTION_TASK_NAME_OLD}", "Runs 'webpack '", "/c SET NODE_ENV=production&& webpack --color");
            build.Children.Add(buildProdOld);

            TaskRunnerNode buildDev = CreateTask(configFileName, cwd, $"{build.Name} - {DEVELOPMENT_TASK_NAME}", "Runs 'webpack '", "/c SET NODE_ENV=development&& webpack --mode=development --color");
            build.Children.Add(buildDev);

            TaskRunnerNode buildProd = CreateTask(configFileName, cwd, $"{build.Name} - {PRODUCTION_TASK_NAME}", "Runs 'webpack '", "/c SET NODE_ENV=production&& webpack --mode=production --color");
            build.Children.Add(buildProd);

            root.Children.Add(build);

            // Profile
            TaskRunnerNode profile = new TaskRunnerNode("Profile", false);
            TaskRunnerNode profileDevOld = CreateTask(configFileName, cwd, $"{profile.Name} - {DEVELOPMENT_TASK_NAME_OLD}", "Runs 'webpack --profile'", "/c SET NODE_ENV=development&& webpack --profile --json > stats.json && echo \x1B[32mThe analyse tool JSON file can be found at ./stats.json. Upload the file at http://webpack.github.io/analyse/.");
            profile.Children.Add(profileDevOld);

            TaskRunnerNode profileProdOld = CreateTask(configFileName, cwd, $"{profile.Name} - {PRODUCTION_TASK_NAME_OLD}", "Runs 'webpack --profile'", "/c SET NODE_ENV=production&& webpack --profile --json > stats.json && echo \x1B[32mThe analyse tool JSON file can be found at ./stats.json. Upload the file at http://webpack.github.io/analyse/.");
            profile.Children.Add(profileProdOld);

            TaskRunnerNode profileDev = CreateTask(configFileName, cwd, $"{profile.Name} - {DEVELOPMENT_TASK_NAME}", "Runs 'webpack --profile'", "/c SET NODE_ENV=development&& webpack --mode=development --profile --json > stats.json && echo \x1B[32mThe analyse tool JSON file can be found at ./stats.json. Upload the file at http://webpack.github.io/analyse/.");
            profile.Children.Add(profileDev);

            TaskRunnerNode profileProd = CreateTask(configFileName, cwd, $"{profile.Name} - {PRODUCTION_TASK_NAME}", "Runs 'webpack --profile'", "/c SET NODE_ENV=production&& webpack --mode=production --profile --json > stats.json && echo \x1B[32mThe analyse tool JSON file can be found at ./stats.json. Upload the file at http://webpack.github.io/analyse/.");
            profile.Children.Add(profileProd);

            root.Children.Add(profile);

            // Serve
            TaskRunnerNode start = new TaskRunnerNode("Serve", false);
            TaskRunnerNode startDev = CreateTask(configFileName, cwd, "Hot", "Runs 'webpack-dev-server --hot --colors'", "/c SET NODE_ENV=development&& webpack-dev-server --hot --colors");
            start.Children.Add(startDev);

            TaskRunnerNode startProd = CreateTask(configFileName, cwd, "Cold", "Runs 'webpack-dev-server --colors'", "/c SET NODE_ENV=development&& webpack-dev-server --colors");
            start.Children.Add(startProd);

            root.Children.Add(start);

            // Watch
            TaskRunnerNode watch = new TaskRunnerNode("Watch", false);
            TaskRunnerNode watchDevOld = CreateTask(configFileName, cwd, $"{watch.Name} - {DEVELOPMENT_TASK_NAME_OLD}", "Runs 'webpack --watch'", "/c SET NODE_ENV=development&& webpack --watch --color");
            watch.Children.Add(watchDevOld);

            TaskRunnerNode watchProdOld = CreateTask(configFileName, cwd, $"{watch.Name} - {PRODUCTION_TASK_NAME_OLD}", "Runs 'webpack --watch'", "/c SET NODE_ENV=production&& webpack --watch --color");
            watch.Children.Add(watchProdOld);

            TaskRunnerNode watchDev = CreateTask(configFileName, cwd, $"{watch.Name} - {DEVELOPMENT_TASK_NAME}", "Runs 'webpack --watch'", "/c SET NODE_ENV=development&& webpack --mode=development --watch --color");
            watch.Children.Add(watchDev);

            TaskRunnerNode watchProd = CreateTask(configFileName, cwd, $"{watch.Name} - {PRODUCTION_TASK_NAME}", "Runs 'webpack --watch'", "/c SET NODE_ENV=production&& webpack --mode=production --watch --color");
            watch.Children.Add(watchProd);

            root.Children.Add(watch);

            return root;
        }

        private TaskRunnerNode CreateTask(string configFileName, string cwd, string name, string desc, string args)
        {
            TaskRunnerNode task = new TaskRunnerNode(name, true)
            {
                Description = desc,
                Command = GetCommand(cwd, args)
            };

            ApplyOverrides(configFileName, task);

            return task;
        }

        private void ApplyOverrides(string configFileName, ITaskRunnerNode parent)
        {
            Match currentExtensionMatch = Regex.Match(configFileName, "webpack\\.config\\.(?<ext>.+)$");
            Group currentExtension = currentExtensionMatch.Groups["ext"];

            IEnumerable<string> files = Directory
                .EnumerateFiles(parent.Command.WorkingDirectory)
                .Where(f => f.Contains("webpack.") && f.EndsWith($".config.{currentExtension}", StringComparison.OrdinalIgnoreCase));

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                Match match = Regex.Match(fileName, $"webpack\\.(?<env>[^\\.]+)\\.config\\.{currentExtension}");

                if (!match.Success)
                {
                    continue;
                }

                TaskRunnerNode task = new TaskRunnerNode($"config: {match.Groups["env"].Value}", true)
                {
                    Description = $"Runs '{parent.Name} --config {fileName}'",
                    Command = GetCommand(parent.Command.WorkingDirectory, $"{parent.Command.Args.Replace("webpack ", $"webpack --config {fileName} ")}")
                };

                parent.Children.Add(task);
            }
        }

        private ITaskRunnerCommand GetCommand(string cwd, string arguments)
        {
            ITaskRunnerCommand command = new TaskRunnerCommand(cwd, "cmd", arguments);

            return command;
        }

        private static string GetExecutableFolder()
        {
            string assembly = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(assembly);
        }
    }
}
