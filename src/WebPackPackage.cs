using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using task = System.Threading.Tasks.Task;

namespace WebPackTaskRunner
{
    [Guid(PackageGuids.guidWebPackPackageString)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(ActivationContextGuid)]
    [ProvideUIContextRule(ActivationContextGuid, Vsix.Id,
        "( WAP | WebSite | DotNetCoreWeb | Cordova ) & !Node ",
        new string[] {
            "WAP",
            "WebSite",
            "DotNetCoreWeb",
            "Cordova",
            "Node"
        },
        new string[] {
            "ActiveProjectFlavor:{349C5851-65DF-11DA-9384-00065B846F21}",
            "ActiveProjectFlavor:{E24C65DC-7377-472B-9ABA-BC803B73C61A}",
            "ActiveProjectCapability:DotNetCoreWeb",
            "ActiveProjectCapability:DependencyPackageManagement",
            "ActiveProjectFlavor:{3AF33F2E-1136-4D97-BBB7-1795711AC8B8}",
        })]
    public sealed class WebPackPackage : AsyncPackage
    {
        private const string ActivationContextGuid = "{b9faacfa-4783-4a9c-aa64-7f0d6b3abb1e}";

        protected override async task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await Logger.InitializeAsync(this, Vsix.Name);
        }
    }
}
