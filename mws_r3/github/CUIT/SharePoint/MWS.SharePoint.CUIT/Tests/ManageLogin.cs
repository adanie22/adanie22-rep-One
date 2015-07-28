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
    /// Contains login tests for:
    /// - Testing a user with a valid Site Collection Owner username, valid password and has permissions to SharePoint site can log on
    /// - Testing a user with a valid Site Owner username, valid password and has permissions to SharePoint site can log on
    /// - Testing a user with a valid Site Member username, valid password and has permissions to SharePoint site can log on
    /// - Testing a user with a valid Site Visitor username, valid password and has permissions to SharePoint site can log on
    /// - Testing a user with a valid username, valid password and has NO permissions to SharePoint site cannot log on
    /// - Testing a user with a valid username, invalid password and has permissions to SharePoint site cannot log on
    /// - Testing a user with a invalid username, invalid password cannot log on
    /// </summary>
    [CodedUITest]
    public class ManageLogin : Base
    {
        private string environment = ConfigurationManager.AppSettings["TeamSiteURL"];
        
        /// <summary>
        /// Testing a user with a valid Site Collection Owner username, valid password and has permissions to SharePoint site can log on
        /// </summary>
        [TestCategory("SharePoint Core - Foundation")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC112")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void LogInAsValidSiteCollectionUser()
        {
            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(environment, User.SiteCollectionUserName, User.SiteCollectionPassword);

            // Refresh to get a screenshot of the end result into the TestLog HTML page
            browser.Refresh();

            Assert.IsNotNull(browser, string.Format("Logging into {0} as Site Collection user:{1} with password: {2}, FAILED.", environment, User.SiteVisitorUserName, User.SiteVisitorPassword));
        }

        /// <summary>
        /// Testing a user with a valid Site Owner username, valid password and has permissions to SharePoint site can log on
        /// </summary>
        [TestCategory("SharePoint Core - Foundation")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC113")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void LogInAsValidSiteOwner()
        {
            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(environment, User.SiteOwnerUserName, User.SiteOwnerPassword);

            // Refresh to get a screenshot of the end result into the TestLog HTML page
            browser.Refresh();
            
            Assert.IsNotNull(browser, string.Format("Logging into {0} as Site Owner:{1} with password: {2}, FAILED.", environment, User.SiteVisitorUserName, User.SiteVisitorPassword));
        }

        /// <summary>
        /// Testing a user with a valid Site Member username, valid password and has permissions to SharePoint site can log on
        /// </summary>
        [TestCategory("SharePoint Core - Foundation")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC114")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void LogInAsValidSiteMember()
        {
            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(environment, User.SiteMemberUserName, User.SiteMemberPassword);

            // Refresh to get a screenshot of the end result into the TestLog HTML page
            browser.Refresh();

            Assert.IsNotNull(browser, string.Format("Logging into {0} as Site Member:{1} with password: {2}, FAILED.", environment, User.SiteVisitorUserName, User.SiteVisitorPassword));
        }

        /// <summary>
        /// Testing a user with a valid Site Visitor username, valid password and has permissions to SharePoint site can log on
        /// </summary>
        [TestCategory("SharePoint Core - Foundation")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC115")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void LogInAsValidSiteVistor()
        {
            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(environment, User.SiteVisitorUserName, User.SiteVisitorPassword);

            // Refresh to get a screenshot of the end result into the TestLog HTML page
            browser.Refresh();

            Assert.IsNotNull(browser, string.Format("Logging into {0} as Site Visitor:{1} with password: {2}, FAILED.", environment, User.SiteVisitorUserName, User.SiteVisitorPassword));
        }

        /// <summary>
        /// Testing a user with a valid username, valid password and has NO permissions to SharePoint site cannot log on
        /// </summary>
        [TestCategory("SharePoint Core - Foundation")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC116")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void LogInAsValidUserWithNoPermissionsFailed()
        {
            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(environment, User.NoPermissionSiteUserName, User.NoPermissionSitePassword);

            Assert.IsNull(browser, string.Format("Logging into {0} as user:{1} with password: {2}, PASSED when it should have FAILED.", environment, User.SiteVisitorUserName, User.SiteVisitorPassword));
        }

        /// <summary>
        /// Testing a user with a valid username, invalid password and has permissions to SharePoint site cannot log on
        /// </summary>
        [TestCategory("SharePoint Core - Foundation")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC117")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void LogInAsValidUserWithInvalidPasswordFailed()
        {
            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(environment, User.SiteCollectionUserName, "InvalidPassword");

            Assert.IsNull(browser, string.Format("Logging into {0} as user:{1} with password: {2}, PASSED when it should have FAILED.", environment, User.SiteVisitorUserName, User.SiteVisitorPassword));
        }

        /// <summary>
        /// Testing a user with a invalid username, invalid password cannot log on
        /// </summary>
        [TestCategory("SharePoint Core - Foundation")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC118")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void LogInAsInvalidUserFailed()
        {
            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(environment, "InvalidUserName", "InvalidPassword");

            Assert.IsNull(browser, string.Format("Logging into {0} as user:{1} with password: {2}, PASSED when it should have FAILED.", environment, User.SiteVisitorUserName, User.SiteVisitorPassword));
        }


    }
}
