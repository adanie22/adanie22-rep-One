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
    public class ManageDocument : Base
    {
        private string environment = ConfigurationManager.AppSettings["TeamSiteURL"];
        private string docUploadLibNamePrefix = ConfigurationManager.AppSettings["DocLibNamePrefix"];
        private string docSampleFileBig = ConfigurationManager.AppSettings["SampleFileBig"];
        private string docSampleFilePath = ConfigurationManager.AppSettings["SampleFilePath"];

        xBrowser browser;
        AddAnApp app;
        ManageDocumentPage doc;
        RecycleBinPage bin;

        public ManageDocument()
        {

        }

        public ManageDocument(xBrowser b)
        {
            this.browser = b;
        }

        #region XML Comments
        /// <summary>
        /// When multiple documents are uploaded if one of the documents fail to upload successfully then the test as a whole will be failed
        /// <list type="number">
        /// <item>
        /// <description>Create a Document Library</description>
        /// </item>
        /// <item>
        /// <description>Upload one or more Documents to the library. If document are already uploaded they will be overwritten.</description>
        /// </item>
        /// <item>
        /// <description>Test that each document was successfully uploaded.</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestMethod, Timeout(TestTimeout.Infinite)]
        [TestCategory("Microsoft Office Web Apps")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC016")]
        [TestCategory("TeamSite")]
        public void UploadValidDocumentAsSiteMember()
        {
            NameValueCollection sampleFiles = (NameValueCollection)ConfigurationManager.GetSection("SampleFiles");

            string failedDocs = Upload(sampleFiles);

            // if one of the documents fail to upload successfully then the test as a whole will be failed
            Assert.AreEqual(failedDocs, string.Empty, string.Format("The following documents WERE NOT uploaded successfully: {0}", failedDocs));
        }

        #region XML Comments
        /// <summary>
        /// Attempt to upload a file that exceeds the 'Maximum Upload Size' of 1GB configured in Central Admin.
        /// This test should fail.
        /// <list type="number">
        /// <item>
        /// <description>Create a Document Library</description>
        /// </item>
        /// <item>
        /// <description>Upload a file greater than the 'Maximum Upload Size' of 1GB configured in Central Admin..</description>
        /// </item>
        /// <item>
        /// <description>Test that the document was not uploaded.</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestMethod, Timeout(TestTimeout.Infinite)]
        [TestCategory("Content Storage")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC111")]
        [TestCategory("TeamSite")]
        public void UploadInValidLargeDocumentAsSiteMember()
        {
            NameValueCollection sampleFiles = new NameValueCollection();
            sampleFiles.Add(docSampleFileBig, docSampleFileBig);

            string failedDocs = Upload(sampleFiles);

            // if one of the documents fail to upload successfully then the test as a whole will be failed
            Assert.AreNotEqual(failedDocs, string.Empty, string.Format("The document {0} was incorrectly uploaded when the upload should have failed due to its file size. See test output for specific error message.", failedDocs));
        }

        #region XML Comments
        /// <summary>
        /// Restore a previously-deleted document.
        /// <list type="number">
        /// <item>
        /// <description>Create a Document Library</description>
        /// </item>
        /// <item>
        /// <description>Test the document library was created.</description>
        /// </item>
        /// <item>
        /// <description>Upload a file.</description>
        /// </item>
        /// <item>
        /// <description>Test that the document was uploaded.</description>
        /// </item>
        /// <item>
        /// <description>Delete document.</description>
        /// </item>
        /// <item>
        /// <description>Test the document can be found in the Recycle bin.</description>
        /// </item>
        /// <item>
        /// <description>Restore the deleted file.</description>
        /// </item>
        /// <item>
        /// <description>Test the restored file can be found in the document library created in earlier step.</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestMethod, Timeout(TestTimeout.Infinite)]
        [TestCategory("Content Management")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC038")]
        [TestCategory("TeamSite")]
        public void RestoreDocumentFromRecycleBinAsSiteMember()
        {
            // Arrange
            app = new AddAnApp();

            string appName = docUploadLibNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            string appURL = string.Format("{0}{1}", environment, appName);

            // Create the Doc Lib that a document will be uploaded to
            app.CreateApp("Document Library", appName, appURL, User.SiteOwnerUserName, User.SiteOwnerPassword, true);

            NameValueCollection sampleFiles = (NameValueCollection)ConfigurationManager.GetSection("SampleFiles");
            NameValueCollection deleteFile = new NameValueCollection();
            deleteFile.Add(sampleFiles[0], sampleFiles[0]);

            // Create the document library and upload the document
            LoginDialog login = new LoginDialog();
            browser = login.LogInAs(environment, User.SiteMemberUserName, User.SiteMemberPassword);

            browser.NavigateToUrl(new Uri(appURL));

            string failedDocs = UploadDocument(deleteFile, appURL, docSampleFilePath);

            if (!string.IsNullOrEmpty(failedDocs))
            {
                Console.WriteLine(string.Format("The document '{0}' was not uploaded successfully, so the delete and restore will not proceed.", failedDocs));
            }
            else
            {
                string fileToBeRestored = deleteFile[0];

                // Open new browser and navigate to document library
                browser.Close();
                //browser = new xBrowser(appURL);
                login = new LoginDialog();
                browser = login.LogInAs(environment, User.SiteMemberUserName, User.SiteMemberPassword);

                browser.NavigateToUrl(new Uri(appURL));

                doc = new ManageDocumentPage(browser);

                bool deleteSucceded = doc.DeleteDocument(fileToBeRestored);
                if (!deleteSucceded)
                {
                    Assert.AreEqual(deleteSucceded, false, string.Format("Delete of file {0} failed.", fileToBeRestored));
                }
                else
                {
                    // restore from recycle bin
                    // navigate to the Recycle bin page
                    browser.Close();

                    bin = new RecycleBinPage(environment);
                    bool restoreSucceeded = bin.RestoreItem(fileToBeRestored, appURL, true);

                    //browser.Close();
                    
                    if (restoreSucceeded)
                    {
                        LoginDialog loginRestore = new LoginDialog();
                        xBrowser browserRestore = loginRestore.LogInAs(environment, User.SiteMemberUserName, User.SiteMemberPassword);

                        browserRestore.NavigateToUrl(new Uri(appURL));

                        doc = new ManageDocumentPage(browserRestore);
                        restoreSucceeded = doc.IsDocumentUploaded(appURL, fileToBeRestored);
                    }

                    Assert.AreEqual(restoreSucceeded, true, string.Format("Restore of file {0} failed.", fileToBeRestored));
                }
            }
        }

        /// <summary>
        /// Create the document library and upload the documents
        /// </summary>
        /// <param name="sampleFiles">Collection of files to upload</param>
        /// <returns>True if files were successfully uploaded</returns>
        public string Upload(NameValueCollection sampleFiles)
        {
            // Arrange
            app = new AddAnApp();

            string appName = docUploadLibNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            string appURL = string.Format("{0}{1}", environment, appName);

            // Create the Doc Lib that a document will be uploaded to
            app.CreateApp("Document Library", appName, appURL, User.SiteOwnerUserName, User.SiteOwnerPassword, true);

            LoginDialog login = new LoginDialog();
            browser = login.LogInAs(environment, User.SiteMemberUserName, User.SiteMemberPassword);

            //browser = new xBrowser(appURL);            
            browser.NavigateToUrl(new Uri(appURL));

            string failedDocs = UploadDocument(sampleFiles, appURL, docSampleFilePath);

            return failedDocs;
        }

        /// <summary>
        /// Accepts a collection of files to be uploaded
        /// </summary>
        /// <param name="sampleFiles">Collection of files to be uploaded</param>
        /// <returns>A comma delimited string of files that have not been uploaded successfully</returns>
        public string UploadDocument(NameValueCollection sampleFiles, string appURL, string docSampleFilePath, bool closeBrowser = false)
        {
            // Upload all documents in the Samplefiles section of the app.config
            List<string> failedUploads = new List<string>();
            foreach (string file in sampleFiles)
            {
                doc = new ManageDocumentPage(browser);

                // Act - Attempt to upload document
                bool docUploadedSuccessfully = doc.UploadDocument(appURL, file, docSampleFilePath);

                // Assert - Was document uploaded successfully
                if (!docUploadedSuccessfully)
                {
                    failedUploads.Add(file);
                }

                browser.Refresh();
            }

            if (closeBrowser)
            {
                browser.Close();
            }

            return String.Join(", ", failedUploads.ToArray());
        }

    }
}
