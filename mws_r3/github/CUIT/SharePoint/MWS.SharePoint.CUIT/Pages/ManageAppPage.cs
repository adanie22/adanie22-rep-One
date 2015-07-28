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
    /// Represent's the home page of the SharePoint App Store
    /// </summary>
    public class ManageAppPage
    {
        private string environment = string.Empty;

        private xBrowser browser;
        private xHtmlEdit searchBox;
        private xHtmlEdit addAppNameBox;
        private xHtmlButton addAppCreateBtn;
        private xHtmlHyperlink docLibLink;

        public ManageAppPage(xBrowser b, string envUrl)
        {
            this.browser = b;
            this.environment = envUrl;
        }

        /// <summary>
        /// Search the SharePoint App store for an App eg, Document Library
        /// </summary>
        /// <param name="appType">App Type eg, Document Library</param>
        public bool Search(string appType)
        {
            searchBox = new xHtmlEdit(this.browser, "idStorefrontSearchBox");
            searchBox.WaitForControlExist(10000);

            searchBox.SendKeys(appType);
            // couldnt get the search button click working hence the enter (tab) key is sent
            searchBox.Tab();
            string searchResultText = string.Empty;
            bool isFound = false;
            string error = string.Empty;

            HtmlHyperlink linkVisible = new HtmlHyperlink(this.browser);
            linkVisible = FindVisibleControlByInnerText(appType);

            // test if the hyperlink to the app can be found on the page after the search
            if (linkVisible.InnerText != appType)
            {
                Console.WriteLine(string.Format("{1} app WAS NOT found during search.{0}{2}", Environment.NewLine, appType, error));
                isFound = false;
            }
            else
            {
                Console.WriteLine(string.Format("{0} app WAS found during search.", appType));
                isFound = true;
            }

            return isFound;
        }

        /// <summary>
        /// Create an app eg, Document Library
        /// </summary>
        /// <param name="appType">App Type eg, Document Library</param>
        /// <param name="appName">Name of App</param>
        public void Create(string appType = "Document Library", string appName = "Test")
        {
            HtmlHyperlink linkVisible = new HtmlHyperlink(this.browser);
            linkVisible = FindVisibleControlByInnerText(appType);

            // FindVisibleControlByInnerText method cannot return the custom xHtmlHyperlink the custom 'Click' extension method cannot be used
            linkVisible.WaitForControlReady();
            linkVisible.SetFocus();
            Mouse.Click(linkVisible);

            // The create windows is SilverLight which runs on a different thread, this command ensures we wait for the screen to render
            Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.AllThreads;

            addAppNameBox = new xHtmlEdit(this.browser, "onetidListTitle");
            addAppNameBox.WaitForControlEnabled(ControlHelper.Wait);
            addAppNameBox.SendKeys(appName);

            // Silverlight Windows is visible so reset status to default
            Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.UIThreadOnly;

            addAppCreateBtn = new xHtmlButton(this.browser, "Create", "DisplayText");
            bool onError = Playback.PlaybackSettings.ContinueOnError;
            try
            {
                // the Click event was being executed a number of times then raising an error
                // I suspect its related to Silverlight so this Try/Catch and ContinueOnError is to workaround the issue
                Playback.PlaybackSettings.ContinueOnError = true;

                addAppCreateBtn.Click();
            }
            catch (Microsoft.VisualStudio.TestTools.UITest.Extension.UITestControlNotFoundException ex)
            {
                string error = ex.BasicMessage;
                Console.WriteLine(string.Format("Error while creating {1}.{0}{2}", Environment.NewLine, appType, error));
            }
            finally
            {
                Playback.PlaybackSettings.ContinueOnError = onError;
            }
        }

        /// <summary>
        /// There seems to be a bug with limiting the search to on visible elements on the page
        /// Therefore multiple controls are found even though only one is visible on the page
        /// This method uses the BoundingRectangle property which is negative for invisible elements 
        /// The invisible elements are filtered out and only the visible element is returned.
        /// Note: That a HtmlHyperlink not xHtmlHyperlink is returned.
        /// </summary>
        /// <param name="appType">App Type eg, Document Library</param>
        /// <returns>Visible hyperlink within collection</returns>
        private HtmlHyperlink FindVisibleControlByInnerText(string appType)
        {
            HtmlHyperlink linkVisible = new HtmlHyperlink(this.browser);

            xHtmlHyperlink linkSearch = new xHtmlHyperlink(this.browser);
            linkSearch.SearchConfigurations.Add(SearchConfiguration.VisibleOnly);
            linkSearch.SearchProperties.Add
                (
                    xHtmlHyperlink.PropertyNames.InnerText,
                    appType,
                    PropertyExpressionOperator.EqualTo
                );
            var links = linkSearch.FindMatchingControls();

            foreach (UITestControl link in links)
            {
                string name = link.FriendlyName;
                int bottom = link.BoundingRectangle.Bottom;
                int top = link.BoundingRectangle.Top;
                int left = link.BoundingRectangle.Left;
                int right = link.BoundingRectangle.Right;
                if ((bottom >= 0) && (top >= 0) && (left >= 0) && (right >= 0))
                {
                    linkVisible = (HtmlHyperlink)link;
                    break;
                }
            }
            return linkVisible;
        }

        /// <summary>
        /// Test if the app exists by navigating the app 'Home Page'.
        /// Once there look for the title to determine if it is the correct app.
        /// </summary>
        /// <param name="appName">Name of App</param>
        public bool AppExists(string appName)
        {
            Thread.Sleep(ControlHelper.Wait);
            docLibLink = new xHtmlHyperlink(this.browser, appName, "InnerText");

            if (docLibLink.TryFind())
            {
                Console.WriteLine(string.Format("{0} app WAS found.", appName));
                return true;
            }
            else
            {
                Console.WriteLine(string.Format("{0} app WAS NOT found.", appName));
                return false;
            }

        }

        /// <summary>
        /// Delete an app
        /// </summary>
        /// <param name="appName">Name of app</param>
        /// <param name="username">User name</param>
        /// <param name="password">User password</param>
        /// <returns>True if app was deleted</returns>
        public bool DeleteApp(string appName, string username, string password)
        {
            string documentLibURL = string.Format("{0}{1}", environment, appName);
            
            LoginDialog login = new LoginDialog();
            browser = login.LogInAs(environment, username, password);

            browser.NavigateToUrl(new Uri(documentLibURL));

            //browser = new xBrowser(documentLibURL);

            // find the ribboon named 'Library'
            xHtmlHyperlink libraryRibbon = new xHtmlHyperlink(browser, "\r\nLibrary\r\nLibrary Tools group. Tab 2 of 2.", "InnerText");
            if (libraryRibbon.TryFind())
            {
                libraryRibbon.WaitForControlEnabled(ControlHelper.Wait);
                libraryRibbon.Click();

                // find the library settings button
                xHtmlHyperlink librarySettingLink = new xHtmlHyperlink(browser, "Library\r\nSettings", "InnerText");
                if (librarySettingLink.TryFind())
                {
                    librarySettingLink.WaitForControlEnabled(ControlHelper.Wait);
                    librarySettingLink.Click();

                    xHtmlHyperlink deleteLibraryLink = new xHtmlHyperlink(browser, "Delete this document library", "InnerText");
                    if (deleteLibraryLink.TryFind())
                    {
                        deleteLibraryLink.WaitForControlEnabled(ControlHelper.Wait);
                        deleteLibraryLink.Click();

                        Console.WriteLine("Before Delete Dialog");
                        Thread.Sleep(ControlHelper.Wait);

                        browser.PerformDialogAction(BrowserDialogAction.Ok);

                        Console.WriteLine("After Delete Dialog");
                        Thread.Sleep(ControlHelper.Wait);

                        browser.Close();

                        return true;
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Delete library link was NOT found."));
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine(string.Format("Delete link on Files Ribbon was NOT found."));
                    return false;
                }
            }
            else
            {
                Console.WriteLine(string.Format("Files Ribbon was NOT found."));
                return false;
            }
        }
    }
}
