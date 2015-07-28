using System.Configuration;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MWS.CUIT.AppControls;
using MWS.SharePoint.CUIT.Pages;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using MWS.SharePoint.CUIT.Utilitiy;

namespace MWS.SharePoint.CUIT
{
    /// <summary>
    /// Contains tests for:
    /// - Restoring a deleted app
    /// </summary>
    [CodedUITest]
    public class DeleteAnApp : Base
    {
        private string environment = ConfigurationManager.AppSettings["TeamSiteURL"];
        private string docLibNamePrefix = ConfigurationManager.AppSettings["DocLibNamePrefix"];
        private string docSampleFileBig = ConfigurationManager.AppSettings["SampleFileBig"];

        AddAnApp app;
        ManageAppPage appPage;
        RecycleBinPage bin;
 
        #region XML Comments
        /// <summary>
        /// Restore a previously-deleted document library
        /// <list type="number">
        /// <item>
        /// <description>Create a Document Library</description>
        /// </item>
        /// <item>
        /// <description>Test the document library was created.</description>
        /// </item>
        /// <description>Delete document library.</description>
        /// </item>
        /// <item>
        /// <description>Test the document library can be found in the Recycle bin.</description>
        /// </item>
        /// <item>
        /// <description>Restore the deleted document library.</description>
        /// </item>
        /// <item>
        /// <description>Test the restored document library can be found.</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestMethod, Timeout(TestTimeout.Infinite)]
        [TestCategory("Content Management")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC052")]
        [TestCategory("TeamSite")]
        public void RestoreDocumentLibraryFromRecycleBinAsSiteMember()
        {
            string appName = docLibNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            string appURL = string.Format("{0}{1}", environment, appName);
            string appType = "Document Library";

            RestoreAppFromRecycleBin(appURL, appName, appType);
        }

        /// <summary>
        /// Restore from the recycle bin
        /// </summary>
        /// <param name="appURL">URL of site</param>
        /// <param name="appName">Name of app</param>
        /// <param name="appType">Type of app, eg Document Library</param>
        private void RestoreAppFromRecycleBin(string appURL, string appName, string appType)
        {
            // Arrange
            app = new AddAnApp();
            appPage = new ManageAppPage(null, environment);

            // Create the Doc Lib that a document will be uploaded to
            app.CreateApp(appType, appName, appURL, User.SiteOwnerUserName, User.SiteOwnerPassword, true);

            // Delete app            
            appPage.DeleteApp(appName, User.SiteMemberUserName, User.SiteMemberPassword);

            // Restore app
            bin = new RecycleBinPage(environment);
            bool restoreSucceeded = bin.RestoreItem(appName, appURL, true);
            
            xBrowser browser = new xBrowser(appURL);
            appPage = new ManageAppPage(browser, environment);

            bool appExists = appPage.AppExists(appName);

            Assert.AreEqual(appExists, true, string.Format("Restore of app {0} failed.", appURL));
        }
 
    }
}
