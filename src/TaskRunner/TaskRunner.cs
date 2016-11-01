using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace WebPackTaskRunner
{
    [TaskRunnerExport("webpack.config.js", "webpack.config.babel.js", "webpack.config.ts", "webpack.config.coffee")]
    class TaskRunner : ITaskRunner
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
            _options = new List<ITaskRunnerOption>();
            _options.Add(new TaskRunnerOption("Display Modules", PackageIds.cmdDisplayModules, PackageGuids.guidWebPackPackageCmdSet, false, "--display-modules"));
            _options.Add(new TaskRunnerOption("Display Reasons", PackageIds.cmdDisplayReasons, PackageGuids.guidWebPackPackageCmdSet, false, "--display-reasons"));
            _options.Add(new TaskRunnerOption("Display Chunks", PackageIds.cmdDisplayChunks, PackageGuids.guidWebPackPackageCmdSet, false, "--display-chunks"));
            _options.Add(new TaskRunnerOption("Display Error Details", PackageIds.cmdDisplayErrorDetails, PackageGuids.guidWebPackPackageCmdSet, false, "--display-error-details"));
            _options.Add(new TaskRunnerOption("Bail", PackageIds.cmdBail, PackageGuids.guidWebPackPackageCmdSet, false, "--bail"));
            _options.Add(new TaskRunnerOption("Inline", PackageIds.cmdInline, PackageGuids.guidWebPackPackageCmdSet, false, "--inline"));
            _options.Add(new TaskRunnerOption("History API Fallback", PackageIds.cmdHistoryApi, PackageGuids.guidWebPackPackageCmdSet, false, "--history-api-fallback"));
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

            const string DEVELOPMENT_TASK_NAME = "Development";
            const string PRODUCTION_TASK_NAME = "Production";

            // Run
            TaskRunnerNode build = new TaskRunnerNode("Run", false);
            TaskRunnerNode buildDev = CreateTask(cwd, $"{build.Name} - {DEVELOPMENT_TASK_NAME}", "Runs 'webpack -d'", "/c SET NODE_ENV=development&& webpack -d --color");
            build.Children.Add(buildDev);

            TaskRunnerNode buildProd = CreateTask(cwd, $"{build.Name} - {PRODUCTION_TASK_NAME}", "Runs 'webpack -p'", "/c SET NODE_ENV=production&& webpack -p --color");
            build.Children.Add(buildProd);

            root.Children.Add(build);

            // Profile
            TaskRunnerNode profile = new TaskRunnerNode("Profile", false);
            TaskRunnerNode profileDev = CreateTask(cwd, $"{profile.Name} - {DEVELOPMENT_TASK_NAME}", "Runs 'webpack -d --profile'", "/c SET NODE_ENV=development&& webpack -d --profile --json > stats.json && echo \x1B[32mThe analyse tool JSON file can be found at ./stats.json. Upload the file at http://webpack.github.io/analyse/.");
            profile.Children.Add(profileDev);

            TaskRunnerNode profileProd = CreateTask(cwd, $"{profile.Name} - {PRODUCTION_TASK_NAME}", "Runs 'webpack -p --profile'", "/c SET NODE_ENV=production&& webpack -p --profile --json > stats.json && echo \x1B[32mThe analyse tool JSON file can be found at ./stats.json. Upload the file at http://webpack.github.io/analyse/.");
            profile.Children.Add(profileProd);

            root.Children.Add(profile);

            // Serve
            TaskRunnerNode start = new TaskRunnerNode("Serve", false);
            TaskRunnerNode startDev = CreateTask(cwd, "Hot", "Runs 'webpack-dev-server --hot'", "/c SET NODE_ENV=development&& webpack-dev-server --hot");
            start.Children.Add(startDev);

            TaskRunnerNode startProd = CreateTask(cwd, "Cold", "Runs 'webpack-dev-server'", "/c SET NODE_ENV=development&& webpack-dev-server");
            start.Children.Add(startProd);

            root.Children.Add(start);

            // Watch
            TaskRunnerNode watch = new TaskRunnerNode("Watch", false);
            TaskRunnerNode watchDev = CreateTask(cwd, $"{watch.Name} - {DEVELOPMENT_TASK_NAME}", "Runs 'webpack -d --watch'", "/c SET NODE_ENV=development&& webpack -d --watch --color");
            watch.Children.Add(watchDev);

            TaskRunnerNode watchProd = CreateTask(cwd, $"{watch.Name} - {PRODUCTION_TASK_NAME}", "Runs 'webpack -p --watch'", "/c SET NODE_ENV=production&& webpack -p --watch --color");
            watch.Children.Add(watchProd);

            root.Children.Add(watch);

            return root;
        }

        private TaskRunnerNode CreateTask(string cwd, string name, string desc, string args)
        {
            var task = new TaskRunnerNode(name, true)
            {
                Description = desc,
                Command = GetCommand(cwd, args)
            };

            ApplyOverrides(task);

            return task;
        }

        private void ApplyOverrides(ITaskRunnerNode parent)
        {
            var files = Directory.EnumerateFiles(parent.Command.WorkingDirectory).Where(f => f.Contains("webpack.") && f.EndsWith(".config.js", StringComparison.OrdinalIgnoreCase));

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                Match match = Regex.Match(fileName, "webpack\\.(?<env>[^\\.]+)\\.config(\\.babel)?\\.js");

                if (!match.Success)
                    continue;

                var task = new TaskRunnerNode($"config: {match.Groups["env"].Value}", true)
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
