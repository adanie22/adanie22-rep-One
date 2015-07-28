using Microsoft.VisualStudio.TestTools.UITest.Extension;
using MWS.CUIT.AppControls;
using MWS.CUIT.AppControls.WebControls;
using MWS.SharePoint.CUIT.Utilitiy;
using System;
using System.Threading;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the manage groups page
    /// </summary>
    public class ManageGroupsPage
    {
        private xBrowser browser;

        public ManageGroupsPage(xBrowser b)
        {
            this.browser = b;
        }

        /// <summary>
        /// Add then remove a user from a site collectin group
        /// </summary>
        /// <param name="siteCollectionGroupName">Name of site collection group</param>
        /// <param name="userName">User name to be added then removed</param>
        /// <returns>True if the user was removed</returns>
        public bool RemoveUserFromSiteCollection(string siteCollectionGroupName, string userName)
        {
            AddNewUser(siteCollectionGroupName, userName);

            bool userRemoved = RemoveUsers(siteCollectionGroupName, userName);

            return userRemoved;
        }

        /// <summary>
        /// Remove a user from a site collectin group
        /// </summary>
        /// <param name="siteCollectionGroupName">Name of site collection group</param>
        /// <param name="userName">User name to be added then removed</param>
        /// <returns>True if the user was removed</returns>
        private bool RemoveUsers(string siteCollectionGroupName, string userName)
        {
            xHtmlCheckBox selectUser = new xHtmlCheckBox(this.browser, userName, "Title");
            selectUser.Click();

            xHtmlHyperlink linkActions = new xHtmlHyperlink(this.browser, "Actions\r\nUse SHIFT+ENTER to open the menu (new window).", "InnerText");
            linkActions.Click();

            xHtmlHyperlink linkRemoveUsers = new xHtmlHyperlink(this.browser, "Remove Users from Group", "Title");
            linkRemoveUsers.Click();

            Console.WriteLine("Before Dialog and Sleep");
            Thread.Sleep(ControlHelper.Wait);

            browser.PerformDialogAction(BrowserDialogAction.Ok);

            Console.WriteLine("After Dialog");
            Thread.Sleep(ControlHelper.Wait);

            bool userExists = UserExists(siteCollectionGroupName, userName);

            // test if the hyperlink to the removed user can be found on the page
            if (userExists)
            {
                Console.WriteLine(string.Format("User {1} was NOT removed from the group {0}.", siteCollectionGroupName, userName));
                return false;
            }
            else
            {
                Console.WriteLine(string.Format("User {1} was removed from the group {0}.", siteCollectionGroupName, userName));
                return true;
            }

        }

        /// <summary>
        /// Add a user from a site collectin group
        /// </summary>
        /// <param name="siteCollectionGroupName">Name of site collection group</param>
        /// <param name="userName">User name to be added then removed</param>
        /// <returns>True if the user was removed</returns>
        private bool AddNewUser(string siteCollectionGroupName, string userName)
        {
            Console.WriteLine(string.Format("Adding User {1} to the group {0}.", siteCollectionGroupName, userName));
            xHtmlHyperlink linkGroups = new xHtmlHyperlink(this.browser, "Groups", "InnerText");
            linkGroups.WaitForControlEnabled(ControlHelper.Wait);
            linkGroups.Click();

            xHtmlHyperlink linkGroup = new xHtmlHyperlink(this.browser, siteCollectionGroupName, "InnerText");
            linkGroup.WaitForControlEnabled(ControlHelper.Wait);
            linkGroup.Click();

            xHtmlHyperlink linkNewUser = new xHtmlHyperlink(this.browser, "New\r\nUse SHIFT+ENTER to open the menu (new window).", "InnerText");
            linkNewUser.Click();

            xHtmlEdit groupName = new xHtmlEdit(this.browser, "Enter names, email addresses, or \'Everyone\'.", "Title");
            groupName.SendKeys(userName);

            xHtmlHyperlink linkUserDropDown = new xHtmlHyperlink(this.browser, userName, "InnerText");
            linkUserDropDown.WaitForControlEnabled(ControlHelper.Wait);
            linkUserDropDown.Click();

            xHtmlInputButton btnShare = new xHtmlInputButton(this.browser, "Share", "DisplayText");
            btnShare.Click();

            bool userExists = UserExists(siteCollectionGroupName, userName);

            // test if the hyperlink to the added user can be found on the page
            if (userExists)
            {
                Console.WriteLine(string.Format("User {1} was added to the group {0}.", siteCollectionGroupName, userName));
                return false;
            }
            else
            {
                Console.WriteLine(string.Format("User {1} was NOT added to the group {0}.", siteCollectionGroupName, userName));
                return true;
            }
        }

        /// <summary>
        /// Test if the user can be found on the page
        /// </summary>
        /// <param name="siteCollectionGroupName">Name of site collection group</param>
        /// <param name="userName">User name to be added then removed</param>
        /// <returns>True if the user was found on the page</returns>
        private bool UserExists(string siteCollectionGroupName, string userName)
        {
            xHtmlCheckBox linkUserExists = new xHtmlCheckBox(this.browser, userName, "Title");
            bool userExists = linkUserExists.TryFind();

            return userExists;
        }
    }
}
 