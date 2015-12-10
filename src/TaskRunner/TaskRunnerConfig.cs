using System;
using System.IO;
using System.Text;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace WebPackTaskRunner
{
    class TaskRunnerConfig : ITaskRunnerConfig
    {
        private ImageSource _icon;
        private ITaskRunnerCommandContext _context;
        ITaskRunnerNode _hierarchy;

        public TaskRunnerConfig(ITaskRunnerCommandContext context, ITaskRunnerNode hierarchy, ImageSource icon)
        {
            _context = context;
            _hierarchy = hierarchy;
            _icon = icon;
        }

        public ImageSource Icon
        {
            get { return _icon; }
        }

        public ITaskRunnerNode TaskHierarchy
        {
            get { return _hierarchy; }
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        public string LoadBindings(string configPath)
        {
            return TaskRunnerFileBindingsHelper.LoadBindings(configPath);
        }

        public bool SaveBindings(string configPath, string bindingsXml)
        {
            return TaskRunnerFileBindingsHelper.SaveBindings(configPath, bindingsXml);
        }
    }
}
