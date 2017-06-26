using Cogworks.ExamineInspector.Constants;
using Cogworks.ExamineInspector.Helpers;
using umbraco.cms.businesslogic.packager;
using Umbraco.Core;

namespace Cogworks.ExamineInspector.EventHandlers
{
    public class UmbracoEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ExamineInspectorPackageHelper.AddExamineInspectorTabToDashboardSectionIfNotExists();

            InstalledPackage.BeforeDelete += InstalledPackage_BeforeDelete;
        }

        private void InstalledPackage_BeforeDelete(InstalledPackage sender, System.EventArgs e)
        {
            if (sender.Data.Name == ExamineInspectorConstants.PackageName)
            {
                ExamineInspectorPackageHelper.RemoveExamineInspectorTabToDashboardSectionIfExists();
            }
        }
    }
}