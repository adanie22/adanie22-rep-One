using Microsoft.VisualStudio.TestTools.UITest.Extension;
using MWS.CUIT.AppControls;
using MWS.CUIT.AppControls.WebControls;
using MWS.SharePoint.CUIT.Utilitiy;
using System;
using System.Threading;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the home page of the SharePoint App Store
    /// </summary>
    public class ManageWorkflowPage
    {
        private string environment;

        public ManageWorkflowPage(string envUrl)
        {
            this.environment = envUrl;
        }

        /// <summary>
        /// Delete the workflow
        /// </summary>
        /// <param name="manageContentTypeURL">Url to manage content typoe page</param>
        /// <param name="workflowName">Name of workflow to delete</param>
        /// <param name="contentType">Type of conent type</param>
        /// <returns>True if the workflow is deleted</returns>
        public bool DeleteWorkFlow(string manageContentTypeURL, string workflowName, string contentType)
        {
            xBrowser browser;
            LoginDialog login = new LoginDialog();
            browser = login.LogInAs(environment, User.SiteOwnerUserName, User.SiteOwnerPassword);

            browser.NavigateToUrl(new Uri(manageContentTypeURL));

            if (!FindWorkflow(manageContentTypeURL, contentType, browser))
            {
                return false;
            }

            xHtmlHyperlink deleteWorkflowLink = new xHtmlHyperlink(browser, "Remove, Block, or Restore a Workflow", "InnerText");
            if (!deleteWorkflowLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "deleteWorkflowLink"));
                return false;
            }
            deleteWorkflowLink.WaitForControlEnabled(ControlHelper.Wait);
            deleteWorkflowLink.Click();

            // ASSUMPTION: the workflow to be deleted is alwasy the first one in the list on this page
            xHtmlRadioButton deleteWorkflowRadioBtn = new xHtmlRadioButton(browser, "Remove", "Title");
            if (!deleteWorkflowRadioBtn.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "deleteWorkflowRadioBtn"));
                return false;
            }
            deleteWorkflowRadioBtn.WaitForControlEnabled(ControlHelper.Wait);
            deleteWorkflowRadioBtn.Click();

            xHtmlInputButton okBtn = new xHtmlInputButton(browser, "OK", "DisplayText");
            if (!okBtn.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "OK"));
                return false;
            }
            okBtn.WaitForControlEnabled(ControlHelper.Wait);
            okBtn.Click();

            Console.WriteLine("Before Dialog and Sleep");
            Thread.Sleep(ControlHelper.Wait);

            browser.PerformDialogAction(BrowserDialogAction.Ok);

            Console.WriteLine("After Dialog");
            Thread.Sleep(ControlHelper.Wait);

            browser.WaitForControlReady();

            // if the workflow can be found then deletion failed
            xHtmlHyperlink editWorkflowLink = new xHtmlHyperlink(browser, workflowName, "InnerText");
            if (editWorkflowLink.TryFind())
            {
                Console.WriteLine(string.Format("The workflow {0} control was found, deletion FAILED.", workflowName));
                return false;
            }

            browser.Refresh();

            return true;
        }

        /// <summary>
        /// Edit the Workflow title
        /// </summary>
        /// <param name="manageContentTypeURL">Url to manage content typoe page</param>
        /// <param name="workflowName">Name of workflow to delete</param>
        /// <param name="contentType">Type of conent type</param>
        /// <returns>True if the workflow is edited</returns>
        public bool EditContentTypeWorkFlow(string manageContentTypeURL, string workflowName, string contentType)
        {
            xBrowser browser;
            LoginDialog login = new LoginDialog();
            browser = login.LogInAs(environment, User.SiteOwnerUserName, User.SiteOwnerPassword);

            browser.NavigateToUrl(new Uri(manageContentTypeURL));

            if (!FindWorkflow(manageContentTypeURL, contentType, browser))
            {
                browser.Close();
                return false;
            }

            xHtmlHyperlink editWorkflowLink = new xHtmlHyperlink(browser, workflowName, "InnerText");
            if (!editWorkflowLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "editWorkflowLink"));
                browser.Close();
                return false;
            }
            editWorkflowLink.WaitForControlEnabled(ControlHelper.Wait);
            editWorkflowLink.Click();

            if (!EditAndSaveWorkflow(workflowName, browser, "EDITED"))
            {
                browser.Close();
                return false;
            }

            browser.Close();
            return true;
        }

        /// <summary>
        /// Create a workflow attached to a content type
        /// </summary>
        /// <param name="manageContentTypeURL">Url to manage content typoe page</param>
        /// <param name="workflowName">Name of workflow to delete</param>
        /// <param name="contentType">Type of conent type</param>
        /// <returns>True if the workflow is created</returns>
        public bool CreateWorkflow(string manageContentTypeURL, string workflowName, string contentType)
        {
            xBrowser browser;
            LoginDialog login = new LoginDialog();
            browser = login.LogInAs(environment, User.SiteOwnerUserName, User.SiteOwnerPassword);

            browser.NavigateToUrl(new Uri(manageContentTypeURL));

            if (!FindWorkflow(manageContentTypeURL, contentType, browser))
            {
                browser.Close();
                return false;
            }

            xHtmlHyperlink addWorkflowLink = new xHtmlHyperlink(browser, "Add a Workflow", "InnerText");
            if (!addWorkflowLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "addWorkflowLink"));
                browser.Close();
                return false;
            }
            addWorkflowLink.WaitForControlEnabled(ControlHelper.Wait);
            addWorkflowLink.Click();

            if (!EditAndSaveWorkflow(workflowName, browser))
            {
                browser.Close();
                return false;
            }

            browser.Close();
            return true;
        }

        /// <summary>
        /// Edit the workflow title and save changes
        /// </summary>
        /// <param name="workflowName">Name of the workflow</param>
        /// <param name="browser">Browser object</param>
        /// <param name="editText">Workflow Title</param>
        /// <returns>True if the workflow title is edited and the workflow saved</returns>
        public static bool EditAndSaveWorkflow(string workflowName, xBrowser browser, string editText = "")
        {
            xHtmlEdit workflowNameEdit = new xHtmlEdit(browser, "WorkflowName");
            if (!workflowNameEdit.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "workflowNameEdit"));
                return false;
            }
            workflowNameEdit.WaitForControlEnabled(ControlHelper.Wait);
            //workflowNameEdit.SendKeys(string.Format("{0}{1}", editText, workflowName));
            workflowNameEdit.SetProperty("Text", string.Format("{0}{1}", editText, workflowName));

            xHtmlInputButton saveWorkflowBtn = new xHtmlInputButton(browser, "idBtnNext");
            if (!saveWorkflowBtn.TryFind())
            {
                return false;
            }
            saveWorkflowBtn.WaitForControlEnabled(ControlHelper.Wait);
            saveWorkflowBtn.Click();

            return true;
        }

        /// <summary>
        /// Find the workflow based on workflow title
        /// </summary>
        /// <param name="manageContentTypeURL">Url to manage content typoe page</param>
        /// <param name="contentType">Type of conent type</param>
        /// <param name="browser">Browser object</param>
        /// <returns>True if the workflow is found</returns>
        public bool FindWorkflow(string manageContentTypeURL, string contentType, xBrowser browser)
        {
            xHtmlHyperlink commentContentTypeLink = new xHtmlHyperlink(browser, contentType, "InnerText");
            if (!commentContentTypeLink.TryFind())
            {
                return false;
            }
            commentContentTypeLink.WaitForControlEnabled(ControlHelper.Wait);
            commentContentTypeLink.Click();

            xHtmlHyperlink workflowSettingsLink = new xHtmlHyperlink(browser, "Workflow settings", "InnerText");
            if (!workflowSettingsLink.TryFind())
            {
                return false;
            }
            workflowSettingsLink.WaitForControlEnabled(ControlHelper.Wait);
            workflowSettingsLink.Click();

            return true;
        }
    }
}
