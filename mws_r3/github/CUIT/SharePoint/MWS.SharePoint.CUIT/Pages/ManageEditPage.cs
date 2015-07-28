using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWS.CUIT.AppControls;
using MWS.CUIT.AppControls.WebControls;
using System.Threading;
using System.Configuration;
using MWS.SharePoint.CUIT.Utilitiy;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the edit page
    /// </summary>
    public class ManageEditPage
    {
        private string environment;

        public ManageEditPage(xBrowser b, string envUrl)
        {
            this.environment = envUrl;
        }

        /// <summary>
        /// Add the webpart and test to see if it exists
        /// </summary>
        /// <param name="appURL">Url of the page where the web part is added</param>
        /// <param name="webPartCategory">Category the web part belongs to</param>
        /// <param name="webPartName">Name of the web part</param>
        /// <param name="webPartTitle">Title of the web part. This is what will be searched for in order to determine pass/fail</param>
        /// <param name="user">Name of user</param>
        /// <param name="password">Password of user</param>
        /// <returns>True if web part was added successfully</returns>
        public bool AddWebPart(string appURL, string webPartCategory, string webPartName, string webPartTitle, string user, string password)
        {
            // Arrange
            AddAnApp app = new AddAnApp();

            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(environment, user, password);

            if (!EditPageMode(browser))
            {
                return false;
            }

            if (!CreateWebPart(browser))
            {
                return false;
            }

            if (!EditAndSaveWebPart(webPartTitle, browser))
            {
                return false;
            }

            if (!WebPartExists(webPartTitle, browser))
            {
                return false;
            }

            browser.Refresh();
            return true;
        }

        /// <summary>
        /// Test if web part exists
        /// </summary>
        /// <param name="webPartTitle">Title of web part</param>
        /// <param name="browser">Browser object</param>
        /// <returns>True if web part exists on page</returns>
        private bool WebPartExists(string webPartTitle, xBrowser browser)
        {
            // look for web part title
            xHtmlSpan spanContentEditor = new xHtmlSpan(browser, webPartTitle, "InnerText");
            if (spanContentEditor.TryFind())
            {
                spanContentEditor.WaitForControlEnabled(ControlHelper.Wait);
                Console.WriteLine(string.Format("Found the Content Editor with the title {0} at {1}", webPartTitle, environment));
                return true;
            }
            else
            {
                Console.WriteLine(string.Format("Could not find the control: {0}", "Add"));
                return false;
            }
        }

        /// <summary>
        /// Set Webpart title and save
        /// </summary>
        /// <param name="webPartTitle">Title of web part</param>
        /// <param name="browser">Browser object</param>
        /// <returns>True if all controls were found</returns>
        private bool EditAndSaveWebPart(string webPartTitle, xBrowser browser)
        {
            // set Web Part Title
            xHtmlDiv divContentEditorWebPart = new xHtmlDiv(browser, "Content Editor  ", "InnerText");
            if (!divContentEditorWebPart.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Content Editor", environment));
                return false;
            }
            divContentEditorWebPart.Click();

            xHtmlHyperlink linkWebPartRibbon = new xHtmlHyperlink(browser, "Web Part\r\nWeb Part Tools group. Tab 1 of 1.", "InnerText");
            if (!linkWebPartRibbon.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Web Part\r\nWeb Part Tools group. Tab 1 of 1.", environment));
                return false;
            }
            linkWebPartRibbon.WaitForControlEnabled(ControlHelper.Wait);
            linkWebPartRibbon.Click();

            xHtmlHyperlink linkWebPartProp = new xHtmlHyperlink(browser, "Web Part\r\nProperties", "InnerText");
            if (!linkWebPartProp.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Web Part\r\nProperties", environment));
                return false;
            }
            linkWebPartProp.WaitForControlEnabled(ControlHelper.Wait);
            linkWebPartProp.Click();

            xHtmlImage imgExpandAppearance = new xHtmlImage(browser, "Expand category: Appearance", "Alt");
            if (!imgExpandAppearance.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Expand category: Appearance", environment));
                return false;
            }
            imgExpandAppearance.WaitForControlEnabled(ControlHelper.Wait);
            imgExpandAppearance.Click();

            xHtmlEdit webPartTitleEdit = new xHtmlEdit(browser, "Title", "LabeledBy");
            if (!webPartTitleEdit.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Title", environment));
                return false;
            }
            webPartTitleEdit.WaitForControlEnabled(ControlHelper.Wait);
            // used this method instead of SendKeys as there is existing text in the text box 
            // which we want to replace entriely and not prefix
            webPartTitleEdit.SetProperty("Text", webPartTitle);

            xHtmlInputButton btnOK = new xHtmlInputButton(browser, "OK", "DisplayText");
            if (!btnOK.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "OK", environment));
                return false;
            }
            btnOK.Click();

            // Save entire Page
            xHtmlHyperlink linkSaveRibbon = new xHtmlHyperlink(browser, "Save", "InnerText");
            if (!linkSaveRibbon.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Save", environment));
                return false;
            }
            linkSaveRibbon.WaitForControlEnabled(ControlHelper.Wait);
            linkSaveRibbon.Click();

            return true;
        }

        /// <summary>
        /// Add the web part to the page
        /// </summary>
        /// <param name="browser">Browser object</param>
        /// <returns>True if all controls were found</returns>
        private bool CreateWebPart(xBrowser browser)
        {
            xHtmlHyperlink linkAddWebPart = new xHtmlHyperlink(browser, "Web Part", "InnerText");
            if (!linkAddWebPart.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Web Part", environment));
                return false;
            }
            linkAddWebPart.WaitForControlEnabled(ControlHelper.Wait);
            linkAddWebPart.Click();

            xHtmlDiv divMediaContent = new xHtmlDiv(browser, "Media and Content", "InnerText");
            if (!divMediaContent.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Media and Content", environment));
                return false;
            }
            divMediaContent.WaitForControlEnabled(ControlHelper.Wait);
            divMediaContent.Click();

            xHtmlDiv divContentEditor = new xHtmlDiv(browser, "Content Editor", "InnerText");
            if (!divContentEditor.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Content Editor", environment));
                return false;
            }
            divContentEditor.WaitForControlEnabled(ControlHelper.Wait);
            divContentEditor.Click();

            xHtmlButton btnAdd = new xHtmlButton(browser, " Add", "DisplayText");
            if (!btnAdd.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Add", environment));
                return false;
            }
            btnAdd.Click();

            return true;
        }

        /// <summary>
        /// Place the eb page in edit mode
        /// </summary>
        /// <param name="browser">Browser object</param>
        /// <returns>True if all controls were found</returns>
        private bool EditPageMode(xBrowser browser)
        {
            // Click on settings page
            xHtmlHyperlink linkPageRibbon = new xHtmlHyperlink(browser, "Page\r\nTab 2 of 2.", "InnerText");
            if (!linkPageRibbon.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Page\r\nTab 2 of 2.", environment));
                return false;
            }
            linkPageRibbon.WaitForControlEnabled(ControlHelper.Wait);
            linkPageRibbon.Click();

            xHtmlHyperlink linkEditPage = new xHtmlHyperlink(browser, "Edit", "InnerText");
            if (!linkEditPage.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Edit", environment));
                return false;
            }
            linkEditPage.Click();

            xHtmlHyperlink linkInsertRibbon = new xHtmlHyperlink(browser, "Insert", "Title");
            if (!linkInsertRibbon.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Insert", environment));
                return false;
            }
            linkInsertRibbon.WaitForControlEnabled(ControlHelper.Wait);
            linkInsertRibbon.Click();
            return true;
        }
    }
}
