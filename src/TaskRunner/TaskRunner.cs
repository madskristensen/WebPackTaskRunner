using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace WebPackTaskRunner
{
    [TaskRunnerExport("webpack.config.js", "webpack.config.babel.js")]
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

        private void InitializeBrunchRunnerOptions()
        {
            _options = new List<ITaskRunnerOption>();
            _options.Add(new TaskRunnerOption("Display Modules", PackageIds.cmdDisplayModules, PackageGuids.guidWebPackPackageCmdSet, false, "--display-modules"));
            _options.Add(new TaskRunnerOption("Display Reasons", PackageIds.cmdDisplayReasons, PackageGuids.guidWebPackPackageCmdSet, false, "--display-reasons"));
            _options.Add(new TaskRunnerOption("Display Chunks", PackageIds.cmdDisplayChunks, PackageGuids.guidWebPackPackageCmdSet, false, "--display-chunks"));
        }

        public List<ITaskRunnerOption> Options
        {
            get
            {
                if (_options == null)
                {
                    InitializeBrunchRunnerOptions();
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

            Telemetry.TrackEvent(configFileName.ToLowerInvariant());

            ITaskRunnerNode root = new TaskRunnerNode(Constants.TASK_CATEGORY);

            // Build
            TaskRunnerNode build = new TaskRunnerNode("Run", false);
            TaskRunnerNode buildDev = CreateTask(cwd, "Development", "Runs 'webpack -d'", "/c webpack -d --colors");
            build.Children.Add(buildDev);

            TaskRunnerNode buildProd = CreateTask(cwd, "Production", "Runs 'webpack -p'", "/c webpack -p --colors");
            build.Children.Add(buildProd);

            root.Children.Add(build);

            // Watch
            TaskRunnerNode watch = new TaskRunnerNode("Watch", false);
            TaskRunnerNode watchDev = CreateTask(cwd, "Development", "Runs 'webpack -d --watch'", "/c webpack -d --watch --colors");
            watch.Children.Add(watchDev);

            TaskRunnerNode watchProd = CreateTask(cwd, "Production", "Runs 'webpack -p --watch'", "/c webpack -p --watch --colors");
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

            return task;
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
