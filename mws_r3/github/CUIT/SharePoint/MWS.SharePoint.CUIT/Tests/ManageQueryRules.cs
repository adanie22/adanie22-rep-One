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
    /// Contains tests for:
    /// - Testing a Query Rule can be created
    /// </summary>
    [CodedUITest]
    public class ManageQueryRules : Base
    {
        private string environment = ConfigurationManager.AppSettings["TeamSiteURL"];
        private string promotedResultPrefix = ConfigurationManager.AppSettings["PromotedResultPrefix"];

        #region XML Comments
        /// <summary>
        /// Test summary details are:
        /// <list type="bullet">
        /// <item>
        /// <description>Test Case Number: BS_TC033</description>
        /// </item>
        /// <item>
        /// <description>Requirement Number: BS8</description>
        /// </item>
        /// <item>
        /// <description>Requirement: Foundation Search</description>
        /// </item>
        /// <item>
        /// <description>Offering: Standard</description>
        /// </item>
        /// </list>
        /// The test steps are:
        /// <list type="number">
        /// <item>
        /// <description>Create a Query Rule</description>
        /// </item>
        /// <item>
        /// <description>Test the Query Rule was successfully created</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestCategory("Foundation Search")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC033")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void CreatePromotedResultsAsSiteCollectionUser()
        {
            string promotedResultName = promotedResultPrefix + DateTime.Now.ToString("yyyMMddHHss");
            string resultSource = "Documents (System)";
            string promotedResult = "Adds Promoted Results";
            
            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(environment, User.SiteCollectionUserName, User.SiteCollectionPassword);

            ManageQueryResultsPage query = new ManageQueryResultsPage(browser, environment);
            bool resultCreatedSuccessfully = query.CreatePromotedResults(promotedResultName, resultSource, promotedResult);

            Assert.AreEqual(resultCreatedSuccessfully, true, string.Format("The creation of the Promoted Result: {0} FAILED.", promotedResultName));

        }
    }
}
