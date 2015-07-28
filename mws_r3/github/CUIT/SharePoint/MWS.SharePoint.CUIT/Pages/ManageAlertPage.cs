using MWS.CUIT.AppControls;
using MWS.CUIT.AppControls.WebControls;
using MWS.SharePoint.CUIT.Utilitiy;
using System;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the Create and Manage Alert pages
    /// </summary>
    public class ManageAlertPage
    {
        private xBrowser browser;

        public ManageAlertPage(xBrowser b)
        {
            this.browser = b;
        }

        /// <summary>
        /// Create an Alert
        /// </summary>
        /// <param name="appType">App Type eg, Document Library</param>
        public bool CreateAlert(string alertName, string appName, string appURL, string filename)
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

            // find the link named 'Alert'
            xHtmlHyperlink alertLink = new xHtmlHyperlink(browser, "Alert Me", "InnerText");
            if (!alertLink.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "alertLink", appURL, filename));
                return false;
            }
            alertLink.Click();

            // find the link named 'Set Alert'
            xHtmlHyperlink setAlertLink = new xHtmlHyperlink(browser, "Set alert on this document", "InnerText");
            if (!setAlertLink.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "setAlertLink", appURL, filename));
                return false;
            }
            setAlertLink.Click();

            // set the Alert Title
            xHtmlEdit titleEdit = new xHtmlEdit(browser, "Alert Title", "Title");
            if (!titleEdit.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "titleEdit", appURL, filename));
                return false;
            }
            titleEdit.SetProperty("Text", alertName);

            // Save Alert
            xHtmlInputButton okBtn = new xHtmlInputButton(browser, "OK", "DisplayText");
            if (!okBtn.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "okBtn", appURL, filename));
                return false;
            }           
            okBtn.Click();

            docImage.WaitForControlEnabled(ControlHelper.Wait);
            docImage.Click();

            // find Manage my Alerts
            alertLink.Click();
            xHtmlHyperlink manageAlertLink = new xHtmlHyperlink(browser, "Manage My alerts", "InnerText");
            if (!manageAlertLink.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1} for file {2}", "manageAlertLink", appURL, filename));
                return false;
            }
            manageAlertLink.Click();


            // test if Alert can be found
            xHtmlHyperlink existingAlertLink = new xHtmlHyperlink(browser, alertName, "InnerText");
            if (!existingAlertLink.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the Alert: {0} at {1} for file {2}", alertName, appURL, filename));
                browser.Refresh();
                return false;
            }
            else
            {
                Console.WriteLine(string.Format("Found the Alert: {0} at {1} for file {2}", alertName, appURL, filename));
                browser.Refresh();
                return true;
            }
        }
    }
}
