using System.Configuration;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MWS.CUIT.AppControls;
using MWS.SharePoint.CUIT.Pages;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using MWS.SharePoint.CUIT.Utilitiy;
using System.Threading;

namespace MWS.SharePoint.CUIT
{
    /// <summary>
    /// Contains tests for:
    /// - Uploading documents of different types
    /// </summary>
    [CodedUITest]
    public class ManageSearch : Base
    {
        private string teamsUrl = ConfigurationManager.AppSettings["TeamSiteURL"];
        private string mySitesUrl = ConfigurationManager.AppSettings["MySitesURL"];
        private string searchUrl = ConfigurationManager.AppSettings["SearchURL"];
        private string searchDocument = ConfigurationManager.AppSettings["SearchDocument"];
        private string searchText = ConfigurationManager.AppSettings["SearchText"];
        private string searchPerson = ConfigurationManager.AppSettings["SearchPerson"];

        xBrowser browser;
        ManageSearchPage search;

        public ManageSearch()
        {

        }

        public ManageSearch(xBrowser b)
        {
            this.browser = b;
        }

        #region XML Comments
        /// <summary>
        /// Search for a document as a site member
        /// <list type="number">
        /// <item>
        /// <description>Search for a document</description>
        /// </item>
        /// <item>
        /// <description>Test that search finds the document.</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestMethod, Timeout(TestTimeout.Infinite)]
        [TestCategory("Foundation Search")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC104")]
        [TestCategory("TeamSite")]
        public void SearchForDocumentAsSiteMember()
        {
            bool resultFound = ExecuteSearch(this.searchDocument);
            Assert.AreEqual(resultFound, true, string.Format("The following query '{0}' DID NOT return any results", this.searchDocument));
        }


        #region XML Comments
        /// <summary>
        /// Search for a text fragment as a site member
        /// <list type="number">
        /// <item>
        /// <description>Search for a text fragment</description>
        /// </item>
        /// <item>
        /// <description>Test that search finds the text fragment.</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestMethod, Timeout(TestTimeout.Infinite)]
        [TestCategory("Foundation Search")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC105")]
        [TestCategory("TeamSite")]
        public void SearchForTextAsSiteMember()
        {
            bool resultFound = ExecuteSearch(this.searchText);
            Assert.AreEqual(resultFound, true, string.Format("The following query '{0}' DID NOT return any results", this.searchText));
        }

        #region XML Comments
        /// <summary>
        /// Search for a person from a personal site
        /// <list type="number">
        /// <item>
        /// <description>Search for a text fragment</description>
        /// </item>
        /// <item>
        /// <description>Test that search finds the text fragment.</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestMethod, Timeout(TestTimeout.Infinite)]
        [TestCategory("Foundation Search")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC040")]
        [TestCategory("MySites")]
        public void SearchForPersonAsSiteMember()
        {
            string user = User.SiteMemberUserName;
            string password = User.SiteMemberPassword;

            LoginDialog login = new LoginDialog();
            this.browser = login.LogInAs(this.mySitesUrl, user, password);
            //this.browser = new xBrowser(this.mySitesUrl);

            Console.WriteLine(string.Format("Searching for {0}", this.searchPerson));
            search = new ManageSearchPage(this.browser, this.mySitesUrl);
            bool resultFound = search.ExecutePersonSearch(this.searchUrl, this.searchPerson);

            Assert.AreEqual(resultFound, true, string.Format("The following query '{0}' DID NOT return any results", this.searchPerson));
        }

        private bool ExecuteSearch(string searchQry)
        {
            string user = User.SiteMemberUserName;
            string password = User.SiteMemberPassword;

            LoginDialog login = new LoginDialog();
            this.browser = login.LogInAs(teamsUrl, user, password);
            //this.browser = new xBrowser(teamsUrl);

            Console.WriteLine(string.Format("Searching for {0}", searchQry));
            search = new ManageSearchPage(this.browser, this.teamsUrl);
            bool resultFound = search.ExecuteSearch(searchQry);

            return resultFound;
        }
    }
}
