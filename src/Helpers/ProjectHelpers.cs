using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace WebPackTaskRunner
{
    public static class ProjectHelpers
    {
        private static DTE2 _dte = (DTE2)Package.GetGlobalService(typeof(DTE));

        public static void AddFileToProject(this Project project, string file, string itemType = null)
        {
            if (project.IsKind(ProjectTypes.ASPNET_5))
                return;

            try
            {
                if (_dte.Solution.FindProjectItem(file) == null)
                {
                    ProjectItem item = project.ProjectItems.AddFromFile(file);

                    if (string.IsNullOrEmpty(itemType)
                        || project.IsKind(ProjectTypes.WEBSITE_PROJECT)
                        || project.IsKind(ProjectTypes.UNIVERSAL_APP))
                        return;

                    item.Properties.Item("ItemType").Value = "None";
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public static bool IsKind(this Project project, string kindGuid)
        {
            return project.Kind.Equals(kindGuid, StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<Project> GetChildProjects(Project parent)
        {
            try
            {
                if (!parent.IsKind(ProjectKinds.vsProjectKindSolutionFolder) && parent.Collection == null)  // Unloaded
                    return Enumerable.Empty<Project>();

                if (!string.IsNullOrEmpty(parent.FullName))
                    return new[] { parent };
            }
            catch (COMException)
            {
                return Enumerable.Empty<Project>();
            }

            return parent.ProjectItems
                    .Cast<ProjectItem>()
                    .Where(p => p.SubProject != null)
                    .SelectMany(p => GetChildProjects(p.SubProject));
        }
    }

    public static class ProjectTypes
    {
        public const string ASPNET_5 = "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}";
        public const string WEBSITE_PROJECT = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";
        public const string UNIVERSAL_APP = "{262852C6-CD72-467D-83FE-5EEB1973A190}";
    }
}
