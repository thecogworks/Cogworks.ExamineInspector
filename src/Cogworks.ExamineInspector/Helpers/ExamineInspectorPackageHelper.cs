using System;
using System.IO;
using System.Xml;
using Cogworks.ExamineInspector.Constants;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Cogworks.ExamineInspector.Helpers
{
    public class ExamineInspectorPackageHelper
    {
        public static void AddExamineInspectorTabToDashboardSectionIfNotExists()
        {
            try
            {
                var dashboardFilePath = IOHelper.MapPath(SystemFiles.DashboardConfig);

                var dashboardXml = new XmlDocument();
                dashboardXml.Load(dashboardFilePath);

                var startupDeveloperDashboardSection = dashboardXml.SelectSingleNode("//section [@alias='" + ExamineInspectorConstants.SectionAlias + "']");
                if (startupDeveloperDashboardSection == null) return;

                var examineInspectorTab = startupDeveloperDashboardSection.SelectSingleNode("//tab [@caption='" + ExamineInspectorConstants.TabCaption + "']");
                if (examineInspectorTab != null) return;

                var xmlToAdd = BuildSectionTabXml(ExamineInspectorConstants.TabCaption, ExamineInspectorConstants.ViewlPath);
                if (startupDeveloperDashboardSection.OwnerDocument == null || xmlToAdd == null) return;

                LogHelper.Info<ExamineInspectorPackageHelper>("Adding ExamineInspector Tab (" + ExamineInspectorConstants.SectionAlias + ") section in: " + dashboardFilePath);

                startupDeveloperDashboardSection.AppendChild(startupDeveloperDashboardSection.OwnerDocument.ImportNode(xmlToAdd, true));
                dashboardXml.Save(dashboardFilePath);
            }
            catch (Exception ex)
            {
                LogHelper.Error<ExamineInspectorPackageHelper>("Something went wrong during the package instalation, applying rollback...", ex);
                RemoveExamineInspectorTabToDashboardSectionIfExists();
                throw;
            }
        }

        public static void RemoveExamineInspectorTabToDashboardSectionIfExists()
        {
            RemovePackagesFiles();

            var dashboardFilePath = IOHelper.MapPath(SystemFiles.DashboardConfig);
            var dashboardXml = new XmlDocument();
            dashboardXml.Load(dashboardFilePath);

            var startupDeveloperDashboardSection = dashboardXml.SelectSingleNode("//section [@alias='" + ExamineInspectorConstants.SectionAlias + "']");
            if (startupDeveloperDashboardSection == null) return;

            var examineInspectorTab = startupDeveloperDashboardSection.SelectSingleNode("//tab [@caption='" + ExamineInspectorConstants.TabCaption + "']");
            if (examineInspectorTab == null) return;

            startupDeveloperDashboardSection.RemoveChild(examineInspectorTab);

            LogHelper.Info<ExamineInspectorPackageHelper>("Removing ExamineInspector Tab (" + ExamineInspectorConstants.SectionAlias + ") section in: " + dashboardFilePath);
            dashboardXml.Save(dashboardFilePath);
        }

        private static XmlNode BuildSectionTabXml(string tabCaption, string viewPath)
        {
            using (var sw = new StringWriter())
            {
                var settings = new XmlWriterSettings
                {
                    ConformanceLevel = ConformanceLevel.Fragment,
                    OmitXmlDeclaration = true
                };

                using (var xmlWriter = XmlWriter.Create(sw, settings))
                {
                    xmlWriter.WriteStartElement("tab");
                    xmlWriter.WriteAttributeString("caption", tabCaption);

                    xmlWriter.WriteStartElement("control");
                    xmlWriter.WriteAttributeString("addPanel", "true");
                    xmlWriter.WriteAttributeString("panelCaption", "");
                    xmlWriter.WriteString(viewPath);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                }

                var xmlNodeToAdd = new XmlDocument();
                xmlNodeToAdd.LoadXml(sw.ToString());

                return xmlNodeToAdd.SelectSingleNode("*");
            }
        }

        private static void RemovePackagesFiles()
        {
            var installedPackagesFilePath = IOHelper.MapPath(SystemDirectories.Packages + IOHelper.DirSepChar + "installed" + IOHelper.DirSepChar + "installedPackages.config");

            if (File.Exists(installedPackagesFilePath))
            {
                var installedPackagesXml = new XmlDocument();
                installedPackagesXml.Load(installedPackagesFilePath);

                var examineInspectorPackageNode = installedPackagesXml.SelectSingleNode("//package [@name='" + ExamineInspectorConstants.PackageName + "']");

                if (examineInspectorPackageNode != null)
                {
                    LogHelper.Info<ExamineInspectorPackageHelper>("Removing ExamineInspector from installedPackages.config");
                    examineInspectorPackageNode.ParentNode.RemoveChild(examineInspectorPackageNode);
                    installedPackagesXml.Save(installedPackagesFilePath);
                }
            }

            var folderPath = IOHelper.MapPath(ExamineInspectorConstants.InstalationPath);
            if (Directory.Exists(folderPath))
            {
                LogHelper.Info<ExamineInspectorPackageHelper>("Removing ExamineInspector Folder");
                Directory.Delete(folderPath, true);
            }

            var dllPath = IOHelper.MapPath(ExamineInspectorConstants.DllPath);
            if (File.Exists(dllPath))
            {
                LogHelper.Info<ExamineInspectorPackageHelper>("Removing ExamineInspector DLL");
                File.Delete(dllPath);
            }
        }
    }
}