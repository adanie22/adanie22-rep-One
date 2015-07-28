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
    /// Represent's the create web (sub-site) page and the delete web (sub-site) page
    /// </summary>
    public class ManageSubWebPage
    {
        private string environment;
        private xBrowser browser;

        public ManageSubWebPage(xBrowser b, string envUrl)
        {
            this.browser = b;
            this.environment = envUrl;
        }

        /// <summary>
        /// Create a web (sub-site)
        /// </summary>
        /// <param name="webName">Name of the web</param>
        /// <param name="webDescription">Description of the web</param>
        /// <param name="webUrl">Url of the web</param>
        public void Create(string webName, string webDescription, string webUrl)
        {
            xHtmlHyperlink linkCreate = new xHtmlHyperlink(this.browser, "Create", "InnerText");
            if (!linkCreate.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "linkCreate", webUrl, webName));
                return;
            }
            linkCreate.WaitForControlEnabled(ControlHelper.Wait);
            linkCreate.Click();

            xHtmlEdit title = new xHtmlEdit(this.browser, "Title", "Title");
            if (!title.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "title", webUrl, webName));
                return;
            }
            title.WaitForControlEnabled(ControlHelper.Wait);
            title.SendKeys(webName);

            xHtmlTextArea description = new xHtmlTextArea(this.browser, "Description", "Title");
            if (!description.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "description", webUrl, webName));
                return;
            }
            description.SendKeys(webDescription);

            xHtmlEdit url = new xHtmlEdit(this.browser, "Create Subsite Name", "Title");
            if (!url.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "url", webUrl, webName));
                return;
            }
            url.SendKeys(webUrl);

            xHtmlInputButton create = new xHtmlInputButton(this.browser, "Create", "DisplayText");
            if (!create.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "create", webUrl, webName));
                return;
            }
            create.Click();

            xHtmlHyperlink linkWeb = new xHtmlHyperlink(this.browser, string.Format("{0} Currently selected", webName), "InnerText");
            if (!linkWeb.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "linkWeb", webUrl, webName));
                return;
            }
            linkWeb.WaitForControlEnabled(ControlHelper.Wait);
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
            if (!delete.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "delete", webName));
                return false;
            }
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
            if (!linkCreate.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "linkCreate", webName));
                return false;
            }
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
