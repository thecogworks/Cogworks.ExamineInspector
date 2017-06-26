using Umbraco.Core.IO;

namespace Cogworks.ExamineInspector.Constants
{
    public static class ExamineInspectorConstants
    {
        public const string PackageName = "Cogworks.ExamineInspector";
        public const string SectionAlias = "StartupDeveloperDashboardSection";
        public static readonly string InstalationPath = SystemDirectories.AppPlugins + IOHelper.DirSepChar + "ExamineInspector";
        public static readonly string ViewlPath = InstalationPath + "/dashboard/examineinspector.html";
        public static readonly string DllPath = SystemDirectories.Bin + IOHelper.DirSepChar + PackageName + ".dll";
        public const string TabCaption = "Examine Inspector";
        public const string StandardAnalyzerNameSpace = "Lucene.Net.Analysis.Standard.StandardAnalyzer";
    }
}