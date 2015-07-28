using System.Configuration;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MWS.CUIT.AppControls;
using MWS.SharePoint.CUIT.Pages;
using System;
using System.Threading;
using MWS.SharePoint.CUIT.Utilitiy;
using System.Collections.Specialized;

namespace MWS.SharePoint.CUIT
{
    /// <summary>
    /// Contains App tests for:
    /// - Testing an App is available
    /// - Testing an app can be created
    /// </summary>
    [CodedUITest]
    public class ManageAlerts : Base
    {
        private string environment = ConfigurationManager.AppSettings["TeamSiteURL"];
        private string docUploadLibNamePrefix = ConfigurationManager.AppSettings["DocLibNamePrefix"];
        private string AlertNamePrefix = ConfigurationManager.AppSettings["AlertNamePrefix"];
        private string docSampleFilePath = ConfigurationManager.AppSettings["SampleFilePath"];

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
        [TestCategory("BS_TC050")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void CreateAlertAsSiteVisitor()

        {
            // Arrange
            AddAnApp app = new AddAnApp();

            string appName = docUploadLibNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            //string appName = "MWSDocLib_201507221432";
            string appURL = string.Format("{0}{1}", environment, appName);

            // Create the Doc Lib that a document will be uploaded to
            app.CreateApp("Document Library", appName, appURL, User.SiteOwnerUserName, User.SiteOwnerPassword, true);

            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(environment, User.SiteMemberUserName, User.SiteMemberPassword);

            browser.NavigateToUrl(new Uri(appURL));

            NameValueCollection sampleFiles = (NameValueCollection)ConfigurationManager.GetSection("SampleFiles");
            NameValueCollection sampleFile = new NameValueCollection();
            sampleFile.Add(sampleFiles[0], sampleFiles[0]);

            ManageDocument document = new ManageDocument(browser);
            string failedDocs = document.UploadDocument(sampleFile, appURL, docSampleFilePath, true);

            if (!string.IsNullOrEmpty(failedDocs))
            {
                Assert.AreNotEqual(failedDocs, string.Empty, string.Format("The document {0} was incorrectly uploaded when the upload should have failed due to its file size. See test output for specific error message.", failedDocs));
                return;
            }

            browser = login.LogInAs(environment, User.SiteMemberUserName, User.SiteMemberPassword);
            browser.NavigateToUrl(new Uri(appURL));

            string alertName = AlertNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            ManageAlertPage alert = new ManageAlertPage(browser);
            bool alertCreatedSuccessfully = alert.CreateAlert(alertName, appName, appURL, sampleFile[0]);

            Assert.AreEqual(alertCreatedSuccessfully, true, string.Format("The creation of the Alert on library: {0} at {1} on file {2} FAILED.", appName, appURL, sampleFile[0]));

        }
    }
}
