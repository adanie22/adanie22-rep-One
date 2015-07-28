using MWS.CUIT.AppControls;
using MWS.CUIT.AppControls.WebControls;
using MWS.SharePoint.CUIT.Utilitiy;
using System;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the Query Results page
    /// </summary>
    public class ManageQueryResultsPage
    {
        private string environment = string.Empty;

        private xBrowser browser;

        public ManageQueryResultsPage(xBrowser b, string envUrl)
        {
            this.browser = b;
            this.environment = envUrl;
        }

        /// <summary>
        /// Create a Promoted Property
        /// </summary>
        /// <param name="promotedResultName">Name of Promoted Property</param>
        /// <param name="resultSource">Result Source</param>
        /// <param name="promotedResult">Promoted Result</param>
        /// <returns>True if the Promoted Property is created successfully</returns>
        public bool CreatePromotedResults(string promotedResultName, string resultSource, string promotedResult)
        {
            string qryRuleUrl = string.Format("{0}{1}", environment, "_layouts/15/listqueryrules.aspx?level=sitecol");

            browser.NavigateToUrl(new Uri(qryRuleUrl));

            xHtmlComboBox resultSourceCombo = new xHtmlComboBox(browser, "Choose a result source to filter your list of query rules.", "Title");
            if (!resultSourceCombo.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "resultSourceCombo", qryRuleUrl));
                return false;
            }
            resultSourceCombo.WaitForControlEnabled(ControlHelper.Wait);
            resultSourceCombo.SelectedItem = resultSource;

            xHtmlComboBox promotedResultCombo = new xHtmlComboBox(browser, "Choose a way to filter your list of query rules.", "Title");
            if (!promotedResultCombo.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "promotedResultCombo", qryRuleUrl));
                return false;
            }
            promotedResultCombo.WaitForControlEnabled(ControlHelper.Wait);
            promotedResultCombo.SelectedItem = promotedResult;

            xHtmlHyperlink newQryRuleLink = new xHtmlHyperlink(browser, "New Query Rule", "InnerText");
            if (!newQryRuleLink.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "newQryRuleLink", qryRuleUrl));
                return false;
            }
            newQryRuleLink.WaitForControlEnabled(ControlHelper.Wait);
            newQryRuleLink.Click();

            xHtmlEdit promotedNameEdit = new xHtmlEdit(browser, "Rule name", "Title");
            if (!promotedNameEdit.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "promotedNameEdit", qryRuleUrl));
                return false;
            }
            promotedNameEdit.WaitForControlEnabled(ControlHelper.Wait);
            promotedNameEdit.SendKeys(promotedResultName);

            xHtmlEdit qryExactlyMatchesEdit = new xHtmlEdit(browser, "Query exactly matches one of these phrases (semi-colon separated)", "Title");
            if (!qryExactlyMatchesEdit.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "qryExactlyMatchesEdit", qryRuleUrl));
                return false;
            }
            qryExactlyMatchesEdit.WaitForControlEnabled(ControlHelper.Wait);
            qryExactlyMatchesEdit.SendKeys(promotedResultName);

            xHtmlInputButton saveBtn = new xHtmlInputButton(browser, "Save", "DisplayText");
            if (!saveBtn.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "saveBtn", qryRuleUrl));
                return false;
            }
            saveBtn.WaitForControlEnabled(ControlHelper.Wait);
            saveBtn.Click();

            string promotedResultLinkText = string.Format("{0}\r\nUse SHIFT+ENTER to open the menu (new window).", promotedResultName);
            xHtmlHyperlink promotedResultLink = new xHtmlHyperlink(browser, promotedResultLinkText, "InnerText");
            if (!promotedResultLink.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the Promoted Result: {0} at {1}", promotedResultName, qryRuleUrl));
                browser.Refresh();
                return false;
            }
            else
            {
                Console.WriteLine(string.Format("Found the Promoted Result: {0} at {1}", promotedResultName, qryRuleUrl));
                browser.Refresh();
                return true;
            }
        }
    }
}
