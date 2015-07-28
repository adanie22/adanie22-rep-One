using System.Configuration;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MWS.CUIT.AppControls;
using MWS.SharePoint.CUIT.Pages;
using System;
using System.Threading;
using MWS.CUIT.AppControls.WebControls;
using MWS.SharePoint.CUIT.Utilitiy;

namespace MWS.SharePoint.CUIT
{
    /// <summary>
    /// Contains App tests for:
    /// - Testing an App is available
    /// - Testing an app can be created
    /// </summary>
    [CodedUITest]
    public class ManageWebPart : Base
    {
        private string environment = ConfigurationManager.AppSettings["TeamSiteURL"];
        private string docLibNamePrefix = ConfigurationManager.AppSettings["DocLibNamePrefix"];
        private string webPartNamePrefix = ConfigurationManager.AppSettings["WebPartNamePrefix"];

        ManageEditPage editPage;

        #region XML Comments
        /// <summary>
        /// Test summary details are:
        /// <list type="bullet">
        /// <item>
        /// <description>Test Case Number: BS_TC023</description>
        /// </item>
        /// <item>
        /// <description>Requirement Number: BS1</description>
        /// </item>
        /// <item>
        /// <description>Requirement: SharePoint Core - Foundation</description>
        /// </item>
        /// <item>
        /// <description>Offering: Standard</description>
        /// </item>
        /// </list>
        /// The test steps are:
        /// <list type="number">
        /// <item>
        /// <description>Test if the Document Library App is available</description>
        /// </item>
        /// <item>
        /// <description>Create Document Library</description>
        /// </item>
        /// <item>
        /// <description>Test Document Library was successfully created</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestCategory("SharePoint Core - Foundation")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC024")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void AddContentEditorWebPartAsSiteOwner()
        {
            string appName = docLibNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            string appURL = string.Format("{0}{1}", environment, appName);
            string appType = "Document Library";

            string webPartCategory = "Media and Content";
            string webPartName = "Content Editor";
            string webPartTitle = webPartNamePrefix + DateTime.Now.ToString("yyyMMddHHss");

            editPage = new ManageEditPage(null, environment);
            bool addedWebPartSuccessfully = editPage.AddWebPart(appURL, webPartCategory, webPartName, webPartTitle, User.SiteOwnerUserName, User.SiteOwnerPassword);

            Assert.AreEqual(addedWebPartSuccessfully, true, string.Format("Web Part {0} | {1} with Title: {5} was not added correctly to {2}: {3}/{4}.", webPartCategory, webPartName, appType, appURL, appName, webPartTitle));
        }
    }
}
