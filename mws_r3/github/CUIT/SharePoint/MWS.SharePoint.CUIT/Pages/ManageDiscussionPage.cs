using MWS.CUIT.AppControls;
using MWS.CUIT.AppControls.WebControls;
using MWS.SharePoint.CUIT.Utilitiy;
using System;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the create web (sub-site) page and the delete web (sub-site) page
    /// </summary>
    public class ManageDiscussionPage
    {
        private string environment;
        private xBrowser browser;

        public ManageDiscussionPage(xBrowser b, string envUrl)
        {
            this.browser = b;
            this.environment = envUrl;
        }

        /// <summary>
        /// Create a discussion
        /// </summary>
        /// <returns>True if successful</returns>
        public bool CreateDiscussion(string subject)
        {
            Console.WriteLine("Click New discussion");
            xHtmlHyperlink newDiscLink = new xHtmlHyperlink(this.browser, "new discussion", "InnerText");
            if (!newDiscLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "newDiscLink"));
                return false;
            }
            newDiscLink.Click();

            Console.WriteLine("Type discussion subject");
            xHtmlEdit title = new xHtmlEdit(this.browser, "Subject", "Title");
            if (!title.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "title"));
                return false;
            }
            title.SendKeys(subject);

            Console.WriteLine("Click Save button");
            xHtmlHyperlink saveBtn = new xHtmlHyperlink(this.browser, "Save", "InnerText");
            if (!saveBtn.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "saveBtn"));
                return false;
            }
            saveBtn.WaitForControlEnabled(ControlHelper.Wait);
            saveBtn.Click();

            return true;
        }

        /// <summary>
        /// Check if discussion with given Subject exists
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        public bool DiscussionCreated(string subject)
        {
            Console.WriteLine("Check if discussion exists");
            xHtmlHyperlink discLink = new xHtmlHyperlink(this.browser, subject, "InnerText");
            if (!discLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "discLink"));
                return false;
            }

            return true;
        }


        /// <summary>
        /// Post reply
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        public bool PostReply(string subject)
        {
            Console.WriteLine("Click on discussion subject");
            xHtmlHyperlink discLink = new xHtmlHyperlink(this.browser, subject, "InnerText");
            if (!discLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "discLink"));
                return false;
            }
            discLink.Click();

            xHtmlEditableDiv discBody = new xHtmlEditableDiv(this.browser, "Add a reply", "InnerText");
            if (!discBody.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "discBody"));
                return false;
            }
            discBody.SendKeys("Communities Site Demo");

            Console.WriteLine("Click Reply button");            
            xHtmlButton replyBtn = new xHtmlButton(this.browser, "Reply", "DisplayText");
            if (!replyBtn.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "replyBtn"));
                return false;
            }
            replyBtn.Click();

            Console.WriteLine("Click on Home link");
            xHtmlHyperlink homeLink = new xHtmlHyperlink(this.browser, "Home", "InnerText");
            if (!homeLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "homeLink"));
                return false;
            }
            homeLink.Click();

            return true;
        }

        /// <summary>
        /// Finds if 1st reply was posted
        /// </summary>
        /// <returns></returns>
        public bool ReplyPosted()
        {
            Console.WriteLine("Find Replies");
            xHtmlDiv replyCount = new xHtmlDiv(this.browser, "1 reply", "InnerText");
            if (!replyCount.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "replyCount"));
                return false;
            }
            
            return true;
        }
    }
}
