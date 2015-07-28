using Microsoft.VisualStudio.TestTools.UITest.Extension;
using MWS.CUIT.AppControls;
using MWS.CUIT.AppControls.WebControls;
using MWS.SharePoint.CUIT.Utilitiy;
using System;
using System.Threading;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the create web (sub-site) page and the delete web (sub-site) page
    /// </summary>
    public class ManageBlogPage
    {
        private string environment;
        private xBrowser browser;

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        private string url;
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        public ManageBlogPage(xBrowser b, string envUrl)
        {
            this.browser = b;
            this.environment = envUrl;
        }

        /// <summary>
        /// Create a blog (sub-site)
        /// </summary>
        /// <returns>True if successful</returns>
        public bool CreateBlog()
        {
            Console.WriteLine("Type blog title");
            xHtmlEdit title = new xHtmlEdit(this.browser, "Title", "Title");
            if (!title.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "title"));
                return false;
            }
            title.WaitForControlEnabled(ControlHelper.Wait);
            title.SendKeys(this.name);

            Console.WriteLine("Type blog description");
            xHtmlTextArea description = new xHtmlTextArea(this.browser, "Description", "Title");
            if (!description.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "description"));
                return false;
            }
            description.SendKeys(this.description);

            Console.WriteLine("Type blog url");
            xHtmlEdit url = new xHtmlEdit(this.browser, "Create Subsite Name", "Title");
            if (!url.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "url"));
                return false;
            }
            url.SendKeys(this.url);

            Console.WriteLine("Select template");
            xHtmlList templateList = new xHtmlList(this.browser, "Select a template:", "LabeledBy");
            if (!templateList.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "templateList"));
                return false;
            }
            //templateList.SelectedItemsAsString = "Blog";
            templateList.Select("Blog");

            bool rv = ClickButtonAndGoHome("Create", "DisplayText");

            if (!rv)
                return false;

            return true;
        }

        /// <summary>
        /// Enable Content Approval on Posts list
        /// </summary>
        /// <returns>True if successful</returns>
        public bool EnableContentApprovalOnPosts()
        {
            Console.WriteLine("Click Manage posts");
            xHtmlHyperlink managePosts = new xHtmlHyperlink(this.browser, " Manage posts ", "InnerText");
            if (!managePosts.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "managePosts"));
                return false;
            }
            managePosts.Click();

            Console.WriteLine("Click List group on the ribbon");
            xHtmlHyperlink listsLink = new xHtmlHyperlink(this.browser, "ListList Tools group. Tab 2 of 2.", "InnerText");
            if (!listsLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "listsLink"));
                return false;
            }
            listsLink.Click();

            Console.WriteLine("Click List settings");
            xHtmlHyperlink listSettings = new xHtmlHyperlink(this.browser, "List\r\nSettings", "InnerText");
            if (!listSettings.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "listSettings"));
                return false;
            }
            listSettings.Click();

            Console.WriteLine("Click Versioning settings");
            xHtmlHyperlink versioning = new xHtmlHyperlink(this.browser, "Versioning settings", "InnerText");
            if (!versioning.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "versioning"));
                return false;
            }
            versioning.Click();

            Console.WriteLine("Set Content Approval to Yes");
            xHtmlRadioButton contentApprovalYes = new xHtmlRadioButton(this.browser, "Require content approval for submitted items: Yes", "Title");
            if (!contentApprovalYes.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "contentApprovalYes"));
                return false;
            }
            contentApprovalYes.Click();

            bool rv = ClickButtonAndGoHome("OK", "DisplayText");

            if (!rv)
                return false;

            return true;
        }

        /// <summary>
        /// Create a Post with a given title
        /// </summary>
        /// <param name="postTitle">Post title</param>
        /// <returns>True if successful</returns>
        public bool CreatePost(string postTitle)
        {
            Console.WriteLine("Click Create post");
            xHtmlHyperlink createPostLink = new xHtmlHyperlink(this.browser, " Create a post ", "InnerText");
            if (!createPostLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "createPostLink"));
                return false;
            }
            createPostLink.Click();

            Console.WriteLine("Type Post title");
            xHtmlEdit title = new xHtmlEdit(this.browser, "Title", "Title");
            if (!title.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "title"));
                return false;
            }
            title.SendKeys(postTitle);

            Console.WriteLine("Click Publish button");
            xHtmlHyperlink publishBtn = new xHtmlHyperlink(this.browser, "Publish", "InnerText");
            if (!publishBtn.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "publishBtn"));
                return false;
            }
            publishBtn.WaitForControlEnabled(ControlHelper.Wait);
            publishBtn.Click();

            xHtmlHyperlink linkWeb = new xHtmlHyperlink(this.browser, "HomeCurrently Selected", "InnerText");
            if (!linkWeb.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "linkWeb"));
                return false;
            }
            linkWeb.WaitForControlEnabled(ControlHelper.Wait);
            linkWeb.Click();

            return true;
        }

        private bool ClickButtonAndGoHome(string buttonId, string buttonSearchPropertyId)
        {
            Console.WriteLine(string.Format("Click {0} button", buttonId));
            xHtmlInputButton okBtn = new xHtmlInputButton(this.browser, buttonId, buttonSearchPropertyId);
            if (!okBtn.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", buttonId));
                return false;
            }
            okBtn.Click();

            bool ifBrowse = false;
            try
            {
                Console.WriteLine("Click Home link if it is not hidden");
                xHtmlHyperlink linkWeb = new xHtmlHyperlink(this.browser, "HomeCurrently Selected", "InnerText");
                if (!linkWeb.TryFind())
                {
                    Console.WriteLine(string.Format("The {0} control was NOT found.", "linkWeb"));
                }
                else
                {
                    linkWeb.WaitForControlEnabled(ControlHelper.Wait);
                    linkWeb.Click();
                }
            }
            catch
            {
                ifBrowse = true;
            }

            if (ifBrowse)
            {
                Console.WriteLine("Click Browse tab if it is not hidden");

                xHtmlHyperlink browseTab = new xHtmlHyperlink(this.browser, "BrowseTab 1 of 3.", "InnerText");
                if (!browseTab.TryFind())
                {
                    Console.WriteLine(string.Format("The {0} control was NOT found.", "browseTab"));
                    return false;
                }
                browseTab.Click();

                Console.WriteLine("Click Home link if it is not hidden");
                xHtmlHyperlink linkWeb = new xHtmlHyperlink(this.browser, "HomeCurrently Selected", "InnerText");
                if (!linkWeb.TryFind())
                {
                    Console.WriteLine(string.Format("The {0} control was NOT found.", "linkWeb"));
                    return false;

                }

                linkWeb.WaitForControlEnabled(ControlHelper.Wait);
                linkWeb.Click();
            }

            return true;
        }

        /// <summary>
        /// Check if a post with a given name exists
        /// </summary>
        /// <param name="postTitle">Post title</param>
        /// <returns>True if successful</returns>
        public bool ManagePost(string action, string postTitle)
        {
            Console.WriteLine("Click Manage post");
            xHtmlHyperlink managePostsLink = new xHtmlHyperlink(this.browser, " Manage posts ", "InnerText");
            if (!managePostsLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "managePostsLink"));
                return false;
            }
            managePostsLink.Click();

            Console.WriteLine("Select post");
            xHtmlDiv post = new xHtmlDiv(this.browser, postTitle, "InnerText");
            if (!post.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "post"));
                return false;
            }
            post.Click();

            Console.WriteLine("Click Items tab on the ribbon");
            xHtmlHyperlink itemsTab = new xHtmlHyperlink(this.browser, "ItemsList Tools group. Tab 1 of 2.", "InnerText");
            if (!itemsTab.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "itemsTab"));
                return false;
            }
            itemsTab.Click();

            Console.WriteLine("Click Approve/Reject link to set/view post status");
            xHtmlHyperlink approveOrRejectLink = new xHtmlHyperlink(this.browser, "Approve/Reject", "InnerText");
            if (!approveOrRejectLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "approveOrRejectLink"));
                return false;
            }
            approveOrRejectLink.Click();

            string status = string.Empty;
            string comment = string.Empty;

            if (action.ToLower() == "pending")
            {
                Console.WriteLine("Select Pending radiobutton");
                status = "Pending. This item will remain visible to its creator and all users who can see draft items.";
                comment = string.Format("{0} has been left pending", postTitle);
            }
            else if (action.ToLower() == "approve")
            {
                Console.WriteLine("Select Approve radiobutton");
                status = "Approved. This item will become visible to all users.";
                comment = string.Format("{0} has been approved", postTitle);
            }
            else if (action.ToLower() == "reject")
            {
                Console.WriteLine("Select Reject radiobutton");
                status = "Rejected. This item will be returned to its creator and only be visible to its creator and all users who can see draft items.";
                comment = string.Format("{0} has been rejected", postTitle);
            }
            else
            {
                Console.WriteLine("Invalid action");
                return false;
            }

            xHtmlRadioButton radioBtn = new xHtmlRadioButton(this.browser, status, "LabeledBy");
            if (!radioBtn.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "radioBtn"));
                return false;
            }
            radioBtn.Click();

            xHtmlTextArea commentsTxtArea = new xHtmlTextArea(this.browser, "Approval Comments", "Title");
            if (!commentsTxtArea.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "commentsTxtArea"));
                return false;
            }
            commentsTxtArea.SendKeys(comment);           
        
            bool rv = ClickButtonAndGoHome("OK", "DisplayText");

            if (!rv)
                return false;

            return true;
        }

        /// <summary>
        /// Delete a web (sub-site)
        /// </summary>
        /// <param name="webName">Name of the web</param>
        /// <returns>True if the Web was deleted</returns>
        public bool Delete(string webName)
        {
            browser.NavigateToUrl(new Uri(string.Format("{0}_layouts/15/deleteweb.aspx?Subweb={1}", environment, webName)));

            xHtmlInputButton delete = new xHtmlInputButton(this.browser, "Delete", "DisplayText");
            delete.WaitForControlEnabled(ControlHelper.Wait);
            delete.Click();

            Console.WriteLine("Before Dialog and Sleep");
            Thread.Sleep(ControlHelper.Wait);

            browser.PerformDialogAction(BrowserDialogAction.Ok);

            Console.WriteLine("After Dialog");
            Thread.Sleep(ControlHelper.Wait);

            return WebDeleted(webName);
        }

        /// <summary>
        /// Test if Web was deleted.
        /// </summary>
        /// <param name="webName">Name of Web</param>
        /// <returns>True if the Web was deleted</returns>
        private bool WebDeleted(string webName)
        {           
            xHtmlHyperlink linkCreate = new xHtmlHyperlink(this.browser, "Create", "InnerText");
            linkCreate.WaitForControlEnabled(ControlHelper.Wait);

            xHtmlHyperlink linkWeb = new xHtmlHyperlink(this.browser, webName, "InnerText");

            if (linkWeb.TryFind())
            {
                Console.WriteLine(String.Format("Link to web {0} was found. Delete of Web failed.", webName));
                return false;
            }
            else
            {
                Console.WriteLine(String.Format("Link to web {0} was NOT found.Deleted of Web succeeded.", webName));
                return true;
            }
        }
    }
}
