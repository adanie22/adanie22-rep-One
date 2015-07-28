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
    /// Represent's search and serach results page
    /// </summary>
    public class ManageSearchPage
    {
        private string siteUrl;
        private xBrowser browser;

        public ManageSearchPage(xBrowser b, string envUrl)
        {
            this.browser = b;
            this.siteUrl = envUrl;
        }

        /// <summary>
        /// Execute search for a document or text fragment
        /// </summary>
        /// <param name="searchQuery">File name that is uploaded in a document library or text fragment</param>
        /// <returns>True if successful</returns>
        public bool ExecuteSearch(string searchQuery)
        {
            Console.WriteLine("Type the search query into the search box");
            xHtmlEdit searchQueryEdit = new xHtmlEdit(this.browser, "Search this site", "Title");
            if (!searchQueryEdit.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "searchQueryEdit"));
                return false;
            }
            searchQueryEdit.SendKeys(searchQuery);

            Console.WriteLine("Click Search button");

            xHtmlHyperlink searchLink = new xHtmlHyperlink(this.browser, "Search", "Title");
            if (!searchLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "searchLink"));
                return false;
            }
            searchLink.WaitForControlEnabled(ControlHelper.Wait);
            searchLink.Click();

            xHtmlSpan searchResPage = new xHtmlSpan(this.browser, "Search", "InnerText");
            if (!searchResPage.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "searchResPage"));
                return false;
            }
            searchResPage.WaitForControlEnabled(ControlHelper.Wait);

            // Confirm if the search was successful
            // if we are searching for the full document name, the result should be a hypelink
            Console.WriteLine(string.Format("Confirming the search result for the query '{0}'", searchQuery));
            bool docLinkFound = false;
            docLinkFound = this.FindResultsAsHyperLink(searchQuery);

            this.browser.Refresh();

            if (docLinkFound)
            {
                return true;
            }
            else
            {
                return (this.FindResultsByInnerText(searchQuery));
            }
        }

        public bool ExecutePersonSearch(string searchUrl, string searchPersonQuery)
        {
            Thread.Sleep(ControlHelper.Wait);
            // Click on the Sites link in the ribbon
            Console.WriteLine("Click on link Sites");
            xHtmlHyperlink linkSitesRibbon = new xHtmlHyperlink(this.browser, "Sites", "InnerText");
            if (!linkSitesRibbon.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "linkSitesRibbon"));
                return false;
            }
            linkSitesRibbon.WaitForControlEnabled(ControlHelper.Wait);
            linkSitesRibbon.Click();

            // Personal site is opened. 
            Console.WriteLine("Type the search query into the search box");
            xHtmlEdit searchQueryEdit = new xHtmlEdit(this.browser, "Search everything", "Title");
            if (!searchQueryEdit.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "searchQueryEdit"));
                return false;
            }
            searchQueryEdit.SendKeys(searchPersonQuery);

            xHtmlHyperlink searchLink = new xHtmlHyperlink(this.browser, "Search", "Title");
            if (!searchLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "searchLink"));
                return false;
            }
            searchLink.WaitForControlEnabled(ControlHelper.Wait);
            searchLink.Click();

            Thread.Sleep(ControlHelper.Wait);

            // Confirm if the search was successful
            Console.WriteLine(string.Format("Confirming the search result for the query '{0}'", searchPersonQuery));
            bool personFound = false;
            personFound = this.FindResultsAsHyperLink(searchPersonQuery);

            this.browser.Refresh();

            return personFound;
        }


        /// <summary>
        /// Confirm if an item was found (document, person)
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        private bool FindResultsAsHyperLink(string searchQuery)
        {
            Console.WriteLine("Looking for hyperlink");
            xHtmlHyperlink resultLink = new xHtmlHyperlink(this.browser, searchQuery, "InnerText");
            if (!resultLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "docLink"));
                return false;
            }
            resultLink.WaitForControlEnabled(ControlHelper.Wait);
 
            return true;
        }

        /// <summary>
        /// Configrm if a text was found
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        private bool FindResultsByInnerText(string searchQuery)
        {
            Console.WriteLine("Looking for text");

            xHtmlDiv divResults = new xHtmlDiv(this.browser);
            divResults.SearchConfigurations.Add(SearchConfiguration.VisibleOnly);

            // searching for all DIVs that contain the search query
            divResults.SearchProperties.Add
                (
                    xHtmlDiv.PropertyNames.InnerText,
                    searchQuery,
                    PropertyExpressionOperator.Contains
                );
            var results = divResults.FindMatchingControls();

            if((results == null) || (results.Count == 0))
            {
                Console.WriteLine("No results have been found");
                return false;
            }

            // divs with the friendly name Item contain the search results
            int itemsCount = 0;
            foreach (UITestControl res in results)
            {
                string name = res.FriendlyName;
                if (res.FriendlyName == "Item")
                {
                    itemsCount++;
                }
            }

            Console.WriteLine(string.Format("{0} results have been found", itemsCount.ToString()));
            return true;
        }
    }
}
