using System.Configuration;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MWS.CUIT.AppControls;
using MWS.SharePoint.CUIT.Pages;
using System;
using System.Threading;
using MWS.SharePoint.CUIT.Utilitiy;

namespace MWS.SharePoint.CUIT
{
    /// <summary>
    /// Contains App tests for:
    /// - Testing an App is available
    /// - Testing an app can be created
    /// </summary>
    [CodedUITest]
    public class AddAnApp : Base
    {
        private string environment = ConfigurationManager.AppSettings["TeamSiteURL"];
        private string docLibNamePrefix = ConfigurationManager.AppSettings["DocLibNamePrefix"];
        private string listNamePrefix = ConfigurationManager.AppSettings["CustomListNamePrefix"];
        private string taskNamePrefix = ConfigurationManager.AppSettings["TaskNamePrefix"];
        private string wikiNamePrefix = ConfigurationManager.AppSettings["WikiNamePrefix"];

        ManageAppPage app;

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
        [TestCategory("BS_TC023")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void CreateDocumentLibraryAsSiteOwner()
        {
            string appName = docLibNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            string appURL = string.Format("{0}{1}", environment, appName);

            CreateApp("Document Library", appName, appURL, User.SiteOwnerUserName, User.SiteOwnerPassword);
        }

        #region XML Comments
        /// <summary>
        /// <list type="number">
        /// <item>
        /// <description>Test if the Custom List App is available</description>
        /// </item>
        /// <item>
        /// <description>Create Custom List</description>
        /// </item>
        /// <item>
        /// <description>Test Custom List was successfully created</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestMethod, Timeout(TestTimeout.Infinite)]
        [TestCategory("SharePoint Core - Foundation")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC019")]
        [TestCategory("TeamSite")]
        public void CreateCustomListAsSiteOwner()
        {
            string appName = listNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            // Note the 'lists' in URL
            string appURL = string.Format("{0}Lists/{1}", environment, appName);

            CreateApp("Custom List", appName, appURL, User.SiteOwnerUserName, User.SiteOwnerPassword);
        }

        #region XML Comments
        /// <summary>
        /// <list type="number">
        /// <item>
        /// <description>Test if the Task List App is available</description>
        /// </item>
        /// <item>
        /// <description>Create Task List</description>
        /// </item>
        /// <item>
        /// <description>Test Task List was successfully created</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestMethod, Timeout(TestTimeout.Infinite)]
        [TestCategory("Content Management")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC019")]
        [TestCategory("TeamSite")]
        public void CreateTaskListAsSiteOwner()
        {
            string appName = taskNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            // Note the 'lists' in URL
            string appURL = string.Format("{0}Lists/{1}", environment, appName);

            CreateApp("Tasks", appName, appURL, User.SiteOwnerUserName, User.SiteOwnerPassword);
        }

        #region XML Comments
        /// <summary>
        /// <list type="number">
        /// <item>
        /// <description>Test if the Wiki Page Library App is available</description>
        /// </item>
        /// <item>
        /// <description>Create Wiki Page Library</description>
        /// </item>
        /// <item>
        /// <description>Test Wiki Page Library was successfully created</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestMethod, Timeout(TestTimeout.Infinite)]
        [TestCategory("Content Management")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC021")]
        [TestCategory("TeamSite")]
        public void CreateWikiPageLibraryAsSiteOwner()
        {
            string appName = wikiNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            string appURL = string.Format("{0}{1}", environment, appName);

            CreateApp("Wiki Page Library", appName, appURL, User.SiteOwnerUserName, User.SiteOwnerPassword);
        }

        #region XML Comments
        /// <summary>
        /// Test if an App is available
        /// Create App
        /// Test App was successfully created
        /// <list type="number">
        /// <item>
        /// <description>Test if the App is available</description>
        /// </item>
        /// <item>
        /// <description>Create App</description>
        /// </item>
        /// <item>
        /// <description>Test App was successfully created</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="appType">App Type eg, Document Library</param>
        /// <param name="appName">Name of App</param>
        /// <param name="appURL">URL to app that will be used to confirm creation</param>
        #endregion
        public bool CreateApp(string appType, string appName, string appURL, string user, string password, bool closeBrowser = false, string envUrl = "")
        {
            if (!string.IsNullOrEmpty(envUrl))
            {
                environment = envUrl;
            }

            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(environment, user, password);

            // Arrange - Open browser and navigate to App Store
            browser.NavigateToUrl(new Uri(string.Format("{0}_layouts/15/addanapp.aspx", environment)));
            app = new ManageAppPage(browser, environment);

            // Act - Test if app is available in Store
            bool foundApp = app.Search(appType);
            Assert.AreEqual(foundApp, true, string.Format("{0} app WAS NOT found during search.", appType));

            // Act - Create app
            app.Create(appType, appName);

            // Assert - Test if app was created
            browser.NavigateToUrl(new Uri(appURL));
            Thread.Sleep(ControlHelper.Wait);
            bool appExists = app.AppExists(appName);
            Assert.AreEqual(appExists, true, string.Format("{0} app WAS NOT created.", appName));

            if (closeBrowser)
            {
                browser.Close();
            }

            return appExists;
        }

    }
}
