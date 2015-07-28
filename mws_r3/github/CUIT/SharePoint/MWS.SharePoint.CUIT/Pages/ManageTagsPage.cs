using MWS.CUIT.AppControls;
using MWS.CUIT.AppControls.WebControls;
using MWS.SharePoint.CUIT.Utilitiy;
using System;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the Create and Manage Alert pages
    /// </summary>
    public class ManageTagsPage
    {
        private xBrowser browser;
        private string environment;

        public ManageTagsPage(xBrowser b, string envUrl)
        {
            this.browser = b;
            this.environment = envUrl;
        }

        /// <summary>
        /// Create an Alert
        /// </summary>
        /// <param name="appType">App Type eg, Document Library</param>
        public bool CreateTags(string tagName, string appURL, string filename)
        {
            xHtmlImage docImage = new xHtmlImage(browser, filename, "Alt");
            if (!docImage.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "docImage", appURL, filename));
                return false;
            }
            docImage.WaitForControlEnabled(ControlHelper.Wait);
            docImage.Click();

            // find the ribbon named 'Files'
            xHtmlHyperlink filesRibbon = new xHtmlHyperlink(browser, "Files Library Tools Group. Tab 1 of 2.", "InnerText");
            if (!filesRibbon.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "filesRibbon", appURL, filename));
                return false;
            }
            filesRibbon.Click();

            // find the link named 'Tags and Notes'
            xHtmlHyperlink tagLink = new xHtmlHyperlink(browser, "Tags & Notes", "InnerText");
            if (!tagLink.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "tagLink", appURL, filename));
                return false;
            }
            tagLink.Click();

            // enter Tag
            //xHtmlEdit tagEdit = new xHtmlEdit(browser, "Add Terms", "Title");
            xHtmlEditableDiv tagEdit = new xHtmlEditableDiv(browser, "DataFrameManager_ctl07editableRegion");
            
            if (!tagEdit.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "tagEdit", appURL, filename));
                return false;
            }
            tagEdit.WaitForControlEnabled(ControlHelper.Wait);
            //tagEdit.Text = tagName;
            tagEdit.SendKeys(tagName);


            // Save Tag
            xHtmlInputButton saveBtn = new xHtmlInputButton(browser, "Save", "DisplayText");
            if (!saveBtn.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "saveBtn", appURL, filename));
                return false;
            }
            saveBtn.Click();

            xHtmlHyperlink closeDialogLink = new xHtmlHyperlink(browser, "Close dialog", "Title");
            if (!closeDialogLink.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "closeDialogLink", appURL, filename));
                return false;
            }
            closeDialogLink.Click();

            browser.NavigateToUrl(new Uri(string.Format("{0}{1}", environment, "_layouts/15/thoughts.aspx")));

            // test if Tag can be found
            xHtmlHyperlink existingTagLink = new xHtmlHyperlink(browser, tagName, "InnerText");
            if (!existingTagLink.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the Tag: {0} at {1} for file {2}", tagName, appURL, filename));
                browser.Refresh();
                return false;
            }
            else
            {
                Console.WriteLine(string.Format("Found the Tag: {0} at {1} for file {2}", tagName, appURL, filename));
                browser.Refresh();
                return true;
            }
        }
    }
}
