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
    /// - Testing a tag is created and can be found
    /// </summary>
    [CodedUITest]
    public class ManageTags : Base
    {
        private string environment = ConfigurationManager.AppSettings["TeamSiteURL"];
        private string docUploadLibNamePrefix = ConfigurationManager.AppSettings["DocLibNamePrefix"];
        private string docSampleFilePath = ConfigurationManager.AppSettings["SampleFilePath"];
        private string tagNamePrefix = ConfigurationManager.AppSettings["TagtNamePrefix"];

        ManageAppPage app;

        #region XML Comments
        /// <summary>
        /// Test summary details are:
        /// <list type="bullet">
        /// <item>
        /// <description>Test Case Number: BS_TC063</description>
        /// </item>
        /// <item>
        /// <description>Requirement Number: BS6</description>
        /// </item>
        /// <item>
        /// <description>Requirement: Keywords</description>
        /// </item>
        /// <item>
        /// <description>Offering: Standard</description>
        /// </item>
        /// </list>
        /// The test steps are:
        /// <list type="number">
        /// <item>
        /// <description>Create Document Library</description>
        /// </item>
        /// <item>
        /// <description>Upload file</description>
        /// </item>
        /// <item>
        /// <description>Add Tag to uploaded document</description>
        /// </item>
        /// <item>
        /// <description>Check the Tag can be found</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestCategory("Keywords")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC063")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void CreateTagAsSiteMember()
        {
            AddAnApp app = new AddAnApp();

            string appName = docUploadLibNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
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

            string tagName = tagNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            ManageTagsPage tag = new ManageTagsPage(browser, environment);
            bool tagCreatedSuccessfully = tag.CreateTags(tagName, appURL, sampleFile[0]);

            Assert.AreEqual(tagCreatedSuccessfully, true, string.Format("The creation of the tag: {3} on library: {0} at {1} on file {2} FAILED.", appName, appURL, sampleFile[0], tagName));

        }

    }
}
