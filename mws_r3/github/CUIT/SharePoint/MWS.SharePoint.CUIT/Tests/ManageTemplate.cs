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
    /// Contains tests for:
    /// - Creating Document Library template
    /// </summary>
    [CodedUITest]
    public class ManageTemplate : Base
    {
        private string environment = ConfigurationManager.AppSettings["TeamSiteURL"];
        private string docLibNamePrefix = ConfigurationManager.AppSettings["DocLibNamePrefix"];
        private string templatePrefix = ConfigurationManager.AppSettings["TemplatePrefix"];
        private string docSampleFilePath = ConfigurationManager.AppSettings["SampleFilePath"];

        //ManageAppPage app;

        #region XML Comments
        /// <summary>
        /// Test summary details are:
        /// <list type="bullet">
        /// <item>
        /// <description>Test Case Number: BS_TC030</description>
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
        /// <description>Create Document Library</description>
        /// </item>
        /// <item>
        /// <description>Upload document</description>
        /// </item>
        /// <item>
        /// <description>Save document library as template</description>
        /// </item>
        /// <item>
        /// <description>Create new document library using template</description>
        /// </item>
        /// <item>
        /// <description>Test if new document library was created and contains the included content</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestCategory("SharePoint Core - Foundation")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC030")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void CreateDocumentLibraryTemplateAsSiteOwner()
        {
            // Arrange
            AddAnApp app = new AddAnApp();

            string appName = docLibNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            //string appName = "MWSDocLib_201507271441";
            
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

            browser = login.LogInAs(environment, User.SiteOwnerUserName, User.SiteOwnerPassword);
            browser.NavigateToUrl(new Uri(appURL));

            string templateName = templatePrefix + DateTime.Now.ToString("yyyMMddHHss");
            ManageTemplatePage template = new ManageTemplatePage(browser, environment);
            bool templateCreatedSuccessfully = template.CreateDocumentLibraryTemplate(templateName, appName, appURL, sampleFile[0]);

            if (!templateCreatedSuccessfully)
            {
                Assert.AreEqual(templateCreatedSuccessfully, true, string.Format("The creation of the template named {2} on library: {0} at {1} FAILED.", appName, appURL, templateName));
            }
            else
            {
                browser.Close();

                appName = string.Format("{0}_{1}", "MWSDocLibCustom", DateTime.Now.ToString("yyyMMddHHss"));
                appURL = string.Format("{0}{1}", environment, appName);

                bool customDocLibCreatedSuccessfully = app.CreateApp(templateName, appName, appURL, User.SiteOwnerUserName, User.SiteOwnerPassword, true);
                if (!customDocLibCreatedSuccessfully)
                {
                    Assert.AreEqual(customDocLibCreatedSuccessfully, true, string.Format("The creation of the Document Library: {0} using the template named {1} at {2} FAILED.", appName, templateName, appURL));
                }
                else
                {
                    // test if content was also included
                    login = new LoginDialog();
                    browser = login.LogInAs(environment, User.SiteMemberUserName, User.SiteMemberPassword);

                    browser.NavigateToUrl(new Uri(appURL));

                    ManageDocumentPage docPage = new ManageDocumentPage(browser);
                    bool documentExists = docPage.DocumentExists(appURL, sampleFile[0]);

                    Assert.AreEqual(documentExists, true, string.Format("The document Library: {0} using the template named {1} at {2} DOES NOT contain the file {3}.", appName, templateName, appURL, sampleFile[0]));
                }

            }

        }


    }
}
