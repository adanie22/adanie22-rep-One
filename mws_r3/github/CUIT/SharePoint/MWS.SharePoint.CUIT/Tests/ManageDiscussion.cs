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
    /// Contains App tests for Community Site:
    /// - Testing a Blog creation
    /// - Tetsing blog's Posts management
    /// - Testing Discussion
    /// </summary>
    [CodedUITest]
    public class ManageDiscussion : Base
    {
        private string communitiesURL = ConfigurationManager.AppSettings["CommunitiesSiteURL"];

        ManageDiscussionPage discussion;

        #region XML Comments
        /// <summary>
        /// Test summary details are:
        /// <list type="bullet">
        /// <item>
        /// <description>Test Case Number: BS_TC110</description>
        /// </item>
        /// <item>
        /// <description>Requirement Number: BS2</description>
        /// </item>
        /// <item>
        /// <description>Requirement: Social Computing</description>
        /// </item>
        /// <item>
        /// <description>Offering: Standard</description>
        /// </item>
        /// </list>
        /// The test steps are:
        /// <list type="number">
        /// <item>
        /// <description>Create a Discussion</description>
        /// </item>
        /// <item>
        /// <description>Reply to the Discussion</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestCategory("Social Computing")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC110")]
        [TestCategory("CommunitySite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void ManageDiscussionAsSiteMember()
        {
            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(communitiesURL, User.SiteMemberUserName, User.SiteMemberPassword);
            //xBrowser browser = new xBrowser(communitiesURL);

            discussion = new ManageDiscussionPage(browser, communitiesURL);    
            Console.WriteLine("Create Discussion");
            string subject = string.Format("MyDiscussion_{0}", DateTime.Now.ToString("yyyMMddHHss"));
            discussion.CreateDiscussion(subject);
            bool discCreated = discussion.DiscussionCreated(subject);
            if (!discCreated)
            {
                Assert.AreEqual(discCreated, true, String.Format("Failed to save discussion {0}.", subject));
                return;
            }

            // Post reply to the discussion
            Console.WriteLine("Post reply");
            discussion.PostReply(subject);
            bool replyPosted = discussion.ReplyPosted();
            if (!replyPosted)
            {
                Assert.AreEqual(replyPosted, true, "Failed to post reply.");
                return;
            }
        }
    }
}
