using MWS.CUIT.AppControls;
using MWS.CUIT.AppControls.WebControls;
using MWS.SharePoint.CUIT.Utilitiy;
using System;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the Manage User page
    /// </summary>
    public class ManageUserPage
    {
        private xBrowser browser;

        public ManageUserPage(xBrowser b)
        {
            this.browser = b;
        }

        /// <summary>
        /// Create a site collection group
        /// </summary>
        /// <param name="siteCollectionGroupName">Name of the site collection group</param>
        /// <returns>True if the site collection group was added</returns>
        public bool CreateSiteCollectionGroup(string siteCollectionGroupName)
        {
            xHtmlHyperlink linkCreateGroup = new xHtmlHyperlink(this.browser, "Create\r\nGroup", "InnerText");
            linkCreateGroup.WaitForControlEnabled(ControlHelper.Wait);
            linkCreateGroup.Click();

            xHtmlEdit groupName = new xHtmlEdit(this.browser, "Name of the group", "Title");
            groupName.SendKeys(siteCollectionGroupName);

            xHtmlCheckBox fullPermission = new xHtmlCheckBox(this.browser, "Full Control - Has full control.", "LabeledBy");
            fullPermission.Click();

            xHtmlInputButton createGroup = new xHtmlInputButton(this.browser, "Create", "DisplayText");
            createGroup.WaitForControlEnabled(ControlHelper.Wait);
            createGroup.Click();

            xHtmlHyperlink linkGroups = new xHtmlHyperlink(this.browser, "Groups", "InnerText");
            linkGroups.WaitForControlEnabled(ControlHelper.Wait);
            linkGroups.Click();

            xHtmlHyperlink linkNewGroup = new xHtmlHyperlink(this.browser, siteCollectionGroupName, "InnerText");

            // test if the hyperlink to the newly created group can be found on the page
            if (linkNewGroup.TryFind())
            {
                Console.WriteLine(string.Format("The site collection group {0} WAS created.", siteCollectionGroupName));
                return true;
            }
            else
            {
                Console.WriteLine(string.Format("The site collection group {0} WAS NOT created.", siteCollectionGroupName));
                return false;
            }
        }
    }
}
