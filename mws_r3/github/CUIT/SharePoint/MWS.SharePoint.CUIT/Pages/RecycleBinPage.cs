using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using MWS.CUIT.AppControls;
using MWS.CUIT.AppControls.WebControls;
using MWS.SharePoint.CUIT.Utilitiy;
using System;
using System.Configuration;
using System.Threading;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the Recycle Bin page
    /// </summary>
    public class RecycleBinPage
    {
        private string environment;

        private xBrowser browser;

        public RecycleBinPage(string envUrl)
        {
            this.environment = envUrl;
        }

        /// <summary>
        /// Identify the object to be restored, restore it and then check it has been restored to the correct page.
        /// </summary>
        /// <param name="fileToBeRestored">File to be restored.</param>
        public bool RestoreItem(string fileToBeRestored, string appURL, bool closeBrowser = false)
        {
            string recycleBinURL = string.Format("{0}_layouts/15/RecycleBin.aspx", environment);
            //browser = new xBrowser(recycleBinURL);
            LoginDialog login = new LoginDialog();
            browser = login.LogInAs(environment, User.SiteMemberUserName, User.SiteMemberPassword);

            browser.NavigateToUrl(new Uri(recycleBinURL));

            // find the doc icon of the document to be restored
            xHtmlImage docImage = new xHtmlImage(browser, fileToBeRestored, "Alt");
            if (docImage.TryFind())
            {
                // select check box of the document to be restored
                Console.WriteLine(string.Format("File {0} was found in recycle bin at {1}.", fileToBeRestored, browser.Uri.ToString()));
                xHtmlCheckBox chkBox = new xHtmlCheckBox(browser, fileToBeRestored, "Title");
                chkBox.Click();

                Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.AllThreads;

                bool onError = Playback.PlaybackSettings.ContinueOnError;
                try
                {
                    // the Click event was being executed a number of times then raising an error
                    // I suspect its related to Silverlight so this Try/Catch and ContinueOnError is to workaround the issue
                    Playback.PlaybackSettings.ContinueOnError = true;

                    // click on Restore button
                    xHtmlHyperlink restoreLink = new xHtmlHyperlink(browser, "Restore Selection", "Title");
                    restoreLink.Click();

                    Console.WriteLine("Before Dialog and Sleep");
                    Thread.Sleep(ControlHelper.Wait);

                    browser.PerformDialogAction(BrowserDialogAction.Ok);

                    Console.WriteLine("After Dialog");
                    Thread.Sleep(ControlHelper.Wait);

                    browser.WaitForControlReady();

                    if (closeBrowser)
                    {
                        browser.Close();
                    }

                    return true;
                }
                catch (Microsoft.VisualStudio.TestTools.UITest.Extension.UITestControlNotFoundException ex)
                {
                    string error = ex.BasicMessage;
                    Console.WriteLine(string.Format("Error while uploading {1}.{0}{2}", Environment.NewLine, "filename", error));
                }
                finally
                {
                    Playback.PlaybackSettings.ContinueOnError = onError;
                    Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.UIThreadOnly;
                }
            }
            else
            {
                Console.WriteLine(string.Format("File {0} was NOT found in recycle bin at {1}.", fileToBeRestored, browser.Uri.ToString()));
                return false;
            }

            return false;
        }

        /// <summary>
        /// Check if the upload was successful
        /// </summary>
        /// <param name="url">Url of the site</param>
        /// <param name="filename">Name of uploaded file</param>
        /// <returns>True if file was uploaded</returns>
        private bool CheckUploadSuccessful(string url, string filename)
        {
            browser.Close();
            // check if a link to the uploaded file can be found on the page
            string documentUrl = string.Format("{0}/{1}", url, filename);
            browser = new xBrowser(url);
            
            Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.AllThreads;

            xHtmlHyperlink docLink = new xHtmlHyperlink(this.browser, documentUrl, "Href");
            Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.UIThreadOnly;

            if (docLink.TryFind())
            {
                Console.WriteLine(string.Format("Document link was found: {0}", documentUrl));
                return true;
            }
            else
            {
                Console.WriteLine(string.Format("Document link was NOT found: {0}", documentUrl));
                return false;
            }
        }  
    }
}
