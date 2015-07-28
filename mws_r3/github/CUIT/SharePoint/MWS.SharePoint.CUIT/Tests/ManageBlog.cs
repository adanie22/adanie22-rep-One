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
    public class ManageBlog : Base
    {
        private string communitiesURL = ConfigurationManager.AppSettings["CommunitiesSiteURL"];
        private string blogNamePrefix = ConfigurationManager.AppSettings["blogNamePrefix"];

        ManageBlogPage blog;

         #region XML Comments
        /// <summary>
        /// Test summary details are:
        /// <list type="bullet">
        /// <item>
        /// <description>Test Case Number: BS_TC028</description>
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
        /// <description>Create a Blog site</description>
        /// </item>
        /// <item>
        /// <description>Create a Post and Set status Pending</description>
        /// </item>
        /// <item>
        /// <description>Create a Post and Approve it</description>
        /// </item>
        /// <item>
        /// <description>Create a Post and Reject it</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestCategory("Social Computing")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC028")]
        [TestCategory("CommunitySite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void ManageBlogPostsAsSiteOwner()
        {
            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(communitiesURL, User.SiteOwnerUserName, User.SiteOwnerPassword);
            //xBrowser browser = new xBrowser(communitiesURL);

            Console.WriteLine("Create blog");
            bool blogCreated = CreateBlog(browser, User.SiteOwnerUserName, User.SiteOwnerPassword);
            if (!blogCreated)
            {
                Assert.AreEqual(blogCreated, true, String.Format("Failed to create blog {0}.", this.blog.Name));
                return;
            }

            // Enable content approval
            Console.WriteLine("Set Content approval on Posts");
            bool contenAprovalEnabled = this.blog.EnableContentApprovalOnPosts();
            if (!contenAprovalEnabled)
            {
                Assert.AreEqual(contenAprovalEnabled, true, String.Format("Failed to set Content Approval on blog {0}.", this.blog.Name));
                return;
            }

            // Create Post
            bool postCreated = ManagePost("pending");
            if (!postCreated)
            {
                Assert.AreEqual(postCreated, true, "Failed to create pending post");
                return;
            }

            postCreated = ManagePost("approve");
            if (!postCreated)
            {
                Assert.AreEqual(postCreated, true, "Failed to create post to approve");
                return;
            }

            postCreated = ManagePost("reject");
            if (!postCreated)
            {
                Assert.AreEqual(postCreated, true, "Failed to create post to reject");
                return;
            }

        }

        private bool ManagePost(string action)
        {
            string postTitle = string.Format("Post_{0}_{1}", action, DateTime.Now.ToString("yyyMMddHHss"));
            Console.WriteLine(string.Format("Create post {0}", postTitle));
           
            this.blog.CreatePost(postTitle);

            bool postCreated = this.blog.ManagePost(action, postTitle);
            return postCreated;
        }

        /// <summary>
        /// Create a blog (sub-site)
        /// </summary>
        /// <returns>Name of the blog that was created.</returns>
        private bool CreateBlog(xBrowser browser, string user, string password)
        {
            // Arrange - Open browser and navigate to App Store
            browser.NavigateToUrl(new Uri(string.Format("{0}_layouts/15/newsbweb.aspx", communitiesURL)));

            this.blog = new ManageBlogPage(browser, communitiesURL);

            blog.Name = blogNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            blog.Description = "MWS Test Blog Site Description";
            blog.Url = blog.Name;

            Console.WriteLine("Creating a new blog");
            if (blog.CreateBlog())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
