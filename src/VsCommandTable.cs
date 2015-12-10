namespace WebPackTaskRunner
{
    using System;
    
    /// <summary>
    /// Helper class that exposes all GUIDs used across VS Package.
    /// </summary>
    internal sealed partial class PackageGuids
    {
        public const string guidWebPackPackageString = "c18648a6-4f3b-4b34-9218-471a1e085457";
        public const string guidWebPackPackageCmdSetString = "ef7252d5-aa4c-4765-b747-19099ca6fb18";
        public const string guidTaskRunnerExplorerCmdSetString = "9e78b319-2142-4381-873c-6ec83f092915";
        public static Guid guidWebPackPackage = new Guid(guidWebPackPackageString);
        public static Guid guidWebPackPackageCmdSet = new Guid(guidWebPackPackageCmdSetString);
        public static Guid guidTaskRunnerExplorerCmdSet = new Guid(guidTaskRunnerExplorerCmdSetString);
    }
    /// <summary>
    /// Helper class that encapsulates all CommandIDs uses across VS Package.
    /// </summary>
    internal sealed partial class PackageIds
    {
        public const int cmdDisplayModules = 0x0100;
        public const int cmdDisplayReasons = 0x0200;
        public const int cmdDisplayChunks = 0x0300;
        public const int IDG_TASKRUNNER_TOOLBAR_CUSTOM_COMMANDS = 0x2002;
    }
}
