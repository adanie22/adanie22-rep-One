using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;
using MWS.CUIT.AppControls;
using MWS.CUIT.AppControls.WebControls;
using MWS.SharePoint.CUIT.Utilitiy;
using System;
using System.Threading;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the manage template
    /// </summary>
    public class ManageTemplatePage
    {
        private string environment = string.Empty;

        private xBrowser browser;
        private xHtmlEdit searchBox;
        private xHtmlEdit addAppNameBox;
        private xHtmlButton addAppCreateBtn;
        private xHtmlHyperlink docLibLink;

        public ManageTemplatePage(xBrowser b, string envUrl)
        {
            this.browser = b;
            this.environment = envUrl;
        }





        /// <summary>
        /// Create a document library template
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="appName"></param>
        /// <param name="appURL"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool CreateDocumentLibraryTemplate(string templateName, string appName, string appURL, string p)
        {
            // find the ribbon named 'Library Settings'
            xHtmlHyperlink filesLibraryRibbon = new xHtmlHyperlink(browser, "Library\r\nLibrary Tools group. Tab 2 of 2.", "InnerText");
            if (!filesLibraryRibbon.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "filesLibraryRibbon", appURL));
                return false;
            }
            filesLibraryRibbon.Click();

            // find the ribbon named 'Library Settings'
            xHtmlHyperlink filesLibrarySettings = new xHtmlHyperlink(browser, "Library Settings", "InnerText");
            if (!filesLibrarySettings.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "filesLibrarySettings", appURL));
                return false;
            }
            filesLibrarySettings.Click();

            xHtmlHyperlink saveDocTemplateLink = new xHtmlHyperlink(browser, "Save document library as template", "InnerText");
            if (!saveDocTemplateLink.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "saveDocTemplateLink", appURL));
                return false;
            }
            saveDocTemplateLink.Click();

            xHtmlEdit filenameEdit = new xHtmlEdit(browser, "File Name", "Title");
            if (!filenameEdit.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "filenameEdit", appURL));
                return false;
            }
            filenameEdit.SendKeys(templateName);

            xHtmlEdit titleEdit = new xHtmlEdit(browser, "Title", "Title");
            if (!titleEdit.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "titleEdit", appURL));
                return false;
            }
            titleEdit.SendKeys(templateName);

            xHtmlCheckBox contentChkbox = new xHtmlCheckBox(browser, "Include Content", "LabeledBy");
            if (!contentChkbox.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "contentChkbox", appURL));
                return false;
            }
            contentChkbox.Checked = true;

            xHtmlInputButton okBtn = new xHtmlInputButton(browser, "OK", "DisplayText");
            if (!okBtn.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "okBtn", appURL));
                return false;
            }
            okBtn.Click();

            xHtmlSpan successText = new xHtmlSpan(browser, "Operation Completed Successfully", "InnerText");
            if (!successText.TryFind())
            {
                Console.WriteLine(string.Format("Creation of Document Template: {0} FAILED.", templateName));
                browser.Refresh();
                return false;
            }
            else
            {
                Console.WriteLine(string.Format("Creation of Document Template: {0} SUCCEEDED.", templateName));
                browser.Refresh();
                return true;
            }
        }
    }
}
