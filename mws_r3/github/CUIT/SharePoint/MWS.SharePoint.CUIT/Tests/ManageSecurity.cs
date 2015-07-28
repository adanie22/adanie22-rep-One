using System.Configuration;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MWS.CUIT.AppControls;
using MWS.SharePoint.CUIT.Pages;
using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using MWS.SharePoint.CUIT.Utilitiy;

namespace MWS.SharePoint.CUIT
{
    /// <summary>
    /// Contains App tests for:
    /// - Testing an App is available
    /// - Testing an app can be created
    /// </summary>
    [CodedUITest]
    public class ManageSecurity : Base
    {
        private string environment = ConfigurationManager.AppSettings["TeamSiteURL"];
        private string userName = ConfigurationManager.AppSettings["UserName"];
        private string siteCollectionGroupNamePrefix = ConfigurationManager.AppSettings["siteCollectionGroupNamePrefix"];

        xBrowser browser;
        ManageUserPage user;
        ManageGroupsPage groups;

        #region XML Comments
        /// <summary>
        /// Create a site collection group.
        /// Test summary details are:
        /// <list type="bullet">
        /// <item>
        /// <description>Test Case Number: BS_TC046</description>
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
        /// <description>Create Site Collection Group</description>
        /// </item>
        /// <item>
        /// <description>Test Site Collection Group was successfully created</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestCategory("SharePoint Core - Foundation")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC046")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void CreateSiteCollectionGroupAsSiteOwner()
        {
            CreateSiteCollectionGroup(true);
        }

        #region XML Comments
        /// <summary>
        /// Create a site collection group.
        /// Test summary details are:
        /// <list type="bullet">
        /// <item>
        /// <description>Test Case Number: BS_TC046</description>
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
        /// <description>Create Site Collection Group</description>
        /// </item>
        /// <item>
        /// <description>Add a user to the group</description>
        /// </item>
        /// <item>
        /// <item>Remove the user from the group</description>
        /// </item>
        /// <item>
        /// <item>Test the user was removed</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestCategory("SharePoint Core - Foundation")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC048")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void RemoveUserFromSiteCollectionAsSiteOwner()
        {
            Base.SetPlayBackFast();

            string siteCollectionGroupName = CreateSiteCollectionGroup(false);

            browser.NavigateToUrl(new Uri(string.Format("{0}_layouts/15/groups.aspx", environment)));

            groups = new ManageGroupsPage(browser);
            bool userRemovedSuccessfully = groups.RemoveUserFromSiteCollection(siteCollectionGroupName, userName);

            // this refresh here is to show the end result in the Test log
            browser.Refresh();

            Base.SetPlayBackNormal();

            Assert.AreEqual(userRemovedSuccessfully, true, string.Format("User {0} was NOT removed from the group {1}.", siteCollectionGroupName, userName));
        }

        /// <summary>
        /// Create a Site Collection Group
        /// </summary>
        /// <param name="makeAssertion">True if this is the main focus of the test and an assertion should be made</param>
        /// <returns>True if site collection group was created</returns>
        private string CreateSiteCollectionGroup(bool makeAssertion)
        {
            Base.SetPlayBackFast();

            string siteCollectionGroupName = siteCollectionGroupNamePrefix + DateTime.Now.ToString("yyyMMddHHss");

            LoginDialog login = new LoginDialog();
            browser = login.LogInAs(environment, User.SiteOwnerUserName, User.SiteOwnerPassword);

            // Arrange - Open browser and navigate to App Store
            browser.NavigateToUrl(new Uri(string.Format("{0}_layouts/15/user.aspx", environment)));

            user = new ManageUserPage(browser);

            bool groupCreatedSuccessfully = user.CreateSiteCollectionGroup(siteCollectionGroupName);

            // caters for when this test is supporting others tests and we dont want an assertion
            if (makeAssertion)
            {
                Assert.AreEqual(groupCreatedSuccessfully, true, string.Format("The site collection group {0} WAS NOT created.", siteCollectionGroupName));
            }

            Base.SetPlayBackNormal();

            return siteCollectionGroupName;
        }

    }
}
