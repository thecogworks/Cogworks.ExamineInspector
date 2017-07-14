namespace Cogworks.ExamineInspector.Helpers
{
    public class GlobalAssemblyInfo
    {
        public const string SimpleVersion = ThisAssembly.Git.BaseVersion.Major + "." + ThisAssembly.Git.BaseVersion.Minor + "." + ThisAssembly.Git.BaseVersion.Patch;

        public const string InformationalVersion = ThisAssembly.Git.Tag;
    }
}