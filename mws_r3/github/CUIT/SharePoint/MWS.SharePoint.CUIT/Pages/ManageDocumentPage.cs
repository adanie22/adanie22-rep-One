using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using MWS.CUIT.AppControls;
using MWS.CUIT.AppControls.WebControls;
using MWS.SharePoint.CUIT.Utilitiy;
using System;
using System.Threading;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the Document Library Page
    /// </summary>
    public class ManageDocumentPage
    {
        private xBrowser browser;
        private xHtmlFileInput FileBrowseBtn;
        private xHtmlInputButton UploadDocOKBtn;
        private xHtmlHyperlink docLink;

        public ManageDocumentPage(xBrowser b)
        {
            this.browser = b;
        }

        /// <summary>
        /// Upload a document to a document library and validate it was uploaded
        /// </summary>
        /// <param name="url">Url to the Document Library</param>
        /// <param name="filename">Name of file to be uploaded</param>
        public bool UploadDocument(string url, string filename, string docSampleFilePath)
        {
            Console.WriteLine("About click on Add Document Link");
            xHtmlHyperlink docAddLink = new xHtmlHyperlink(this.browser, "idHomePageNewDocument-WPQ2");
            if (!docAddLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found. Trying to find 'new document' link", "docAddLink"));
                xHtmlHyperlink newDocLink = new xHtmlHyperlink(this.browser, "new document", "InnerText");
                if (!newDocLink.TryFind())
                {
                    Console.WriteLine(string.Format("The {0} control was NOT found.", "newDocLink"));
                    return false;
                }
                newDocLink.Click();

                xHtmlHyperlink uploadDocLink = new xHtmlHyperlink(this.browser, "Upload existing file", "InnerText");
                if (!uploadDocLink.TryFind())
                {
                    Console.WriteLine(string.Format("The {0} control was NOT found.", "uploadDocLink"));
                    return false;
                }
                uploadDocLink.Click();
                Console.WriteLine("Clicked on Upload Existing Document Link");
            }
            else
            {
                docAddLink.Click();
                Console.WriteLine("Clicked on Add Document Link");
            }
            Console.WriteLine("Clicked on Add Document Link");

            Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.AllThreads;

            FileBrowseBtn = new xHtmlFileInput(this.browser, "Choose a file", "Title");
//            xHtmlFileInput FileBrowseBtn = new xHtmlFileInput(this.browser, "HtmlFileInput", "ClassName");
            if (!FileBrowseBtn.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "FileBrowseBtn"));
                return false;
            }

            FileBrowseBtn.WaitForControlEnabled(ControlHelper.Wait);
            bool onError = Playback.PlaybackSettings.ContinueOnError;
            try
            {
                // the Click event was being executed a number of times then raising an error
                // I suspect its related to Silverlight so this Try/Catch and ContinueOnError is to workaround the issue
                Playback.PlaybackSettings.ContinueOnError = true;

                // Mouse.Hover(FileBrowseBtn);
                Mouse.DoubleClick(FileBrowseBtn);

                OpenDialog.Open(filename, docSampleFilePath);
            }
            catch (Microsoft.VisualStudio.TestTools.UITest.Extension.UITestControlNotFoundException ex)
            {
                string error = ex.BasicMessage;
                Console.WriteLine(string.Format("Error while uploading {1}.{0}{2}", Environment.NewLine, filename, error));
            }
            finally
            {
                Playback.PlaybackSettings.ContinueOnError = onError;
                Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.UIThreadOnly;
            }

            Console.WriteLine("Getting OK Button");
            UploadDocOKBtn = new xHtmlInputButton(this.browser, "OK", "DisplayText");
            UploadDocOKBtn.WaitForControlEnabled(ControlHelper.Wait);

            onError = Playback.PlaybackSettings.ContinueOnError;
            try
            {
                // the Click event was being executed a number of times then raising an error
                // I suspect its related to Silverlight so this Try/Catch and ContinueOnError is to workaround the issue
                Playback.PlaybackSettings.ContinueOnError = true;
                Console.WriteLine("Got OK Button");

                Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.AllThreads;

                UploadDocOKBtn.SetFocus();
                Console.WriteLine("Set Focus on OK Button");
                UploadDocOKBtn.Tab();

                Console.WriteLine("Clicked OK Button");
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

            return IsDocumentUploaded(url, filename);
        }

        /// <summary>
        /// Look for a hyperlink that has the same name as the file that was uploaded.
        /// If found then the test is passed.
        /// </summary>
        /// <param name="url">Url to the Document Library</param>
        /// <param name="filename">Name of file to be uploaded</param>
        /// <returns>True if upload was successfull</returns>
        public bool IsDocumentUploaded(string url, string filename)
        {
            bool uploadSuccessful = DocumentExists(url, filename);

            return uploadSuccessful;
        }

        /// <summary>
        /// Check if document link can be found on page
        /// </summary>
        /// <param name="url">Url to the Document Library</param>
        /// <param name="filename">Name of file to be uploaded</param>
        /// <returns>True if document link was found</returns>
        public bool DocumentExists(string url, string filename)
        {
            // check if a link to the uploaded file can be found on the page
            string documentUrl = string.Format("{0}/{1}", url, filename);

            Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.AllThreads;

            //docLink = new xHtmlHyperlink(this.browser, documentUrl, "Href");
            docLink = new xHtmlHyperlink(this.browser, documentUrl, "Href");
            Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.UIThreadOnly;
            
            if (docLink.TryFind())
            {
                Console.WriteLine(string.Format("Document link found: {0}", documentUrl));
                return true;
            }
            else
            {
                Console.WriteLine(string.Format("Document link was NOT found: {0}", documentUrl));
                return false;
            }
        }

        /// <summary>
        /// Delete a document from a document library and validate it was uploaded
        /// </summary>
        /// <param name="filename">Name of file to be deleted</param>
        public bool DeleteDocument(string filename)
        {
            // find doc icon image mapped to the file to be deleted
            xHtmlImage docImage = new xHtmlImage(browser, filename, "Alt");
            if (docImage.TryFind())
            {
                Thread.Sleep(ControlHelper.Wait);
                docImage.Click();

                // find the ribboon named 'Files'
                xHtmlHyperlink filesRibbon = new xHtmlHyperlink(browser, "Files Library Tools Group. Tab 1 of 2.", "InnerText");
                if (filesRibbon.TryFind())
                {
                    filesRibbon.Click();

                    // find the delete document button
                    xHtmlHyperlink deleteLink = new xHtmlHyperlink(browser, "Ribbon.Documents.Manage.Delete-Medium");
                    if (deleteLink.TryFind())
                    {
                        deleteLink.Click();

                        Console.WriteLine("Before Delete Dialog");
                        Thread.Sleep(ControlHelper.Wait);

                        browser.PerformDialogAction(BrowserDialogAction.Ok);

                        Console.WriteLine("After Delete Dialog");
                        Thread.Sleep(ControlHelper.Wait);

                        // check if the document has been deleted
                        xHtmlImage docImagePostDelete = new xHtmlImage(browser, filename, "Title");
                        if (docImagePostDelete.TryFind())
                        {
                            Console.WriteLine(string.Format("Delete of file {0} failed.", filename));
                            return false;
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Delete of file {0} succeeded.", filename));
                            return true;
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
            else
            {
                Console.WriteLine(string.Format("Image was NOT found: {0}", filename));
                return false;
            }
        }

    }
}
