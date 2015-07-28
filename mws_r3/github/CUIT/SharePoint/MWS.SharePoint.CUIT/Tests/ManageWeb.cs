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
    /// - Testing a Web (sub-site) was deleted
    /// </summary>
    [CodedUITest]
    public class ManageWeb : Base
    {
        private string environment = ConfigurationManager.AppSettings["TeamSiteURL"];
        private string webNamePrefix = ConfigurationManager.AppSettings["webNamePrefix"];

        ManageSubWebPage web;

        #region XML Comments
        /// <summary>
        /// Test summary details are:
        /// <list type="bullet">
        /// <item>
        /// <description>Test Case Number: BS_TC051</description>
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
        /// <description>Create a web (sub-site)</description>
        /// </item>
        /// <item>
        /// <description>Delete a web (sub-site)</description>
        /// </item>
        /// <item>
        /// <description>Test web (sub-site) was successfully deleted</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestCategory("SharePoint Core - Foundation")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC051")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void DeleteWebAsSiteOwner()
        {
            string webName = CreateWeb(User.SiteOwnerUserName, User.SiteOwnerPassword);

            bool deleteSucceeded = web.Delete(webName);

            Assert.AreEqual(deleteSucceeded, true, String.Format("Link to web {0} was found. Delete of web failed.", webName));
        }

        /// <summary>
        /// Create a web (sub-site)
        /// </summary>
        /// <returns>Name of the web that was created.</returns>
        private string CreateWeb(string user, string password)
        {
            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(environment, user, password);

            // Arrange - Open browser and navigate to App Store
            browser.NavigateToUrl(new Uri(string.Format("{0}_layouts/15/mngsubwebs.aspx", environment)));

            web = new ManageSubWebPage(browser, environment);

            string name = webNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            string webName = name;
            string webDescription = "MWS Test Team Site Description";
            string webUrl = name;

            web.Create(webName, webDescription, webUrl);

            return webName;
        }
    }
}
