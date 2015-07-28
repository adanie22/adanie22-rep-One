using System.Configuration;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MWS.CUIT.AppControls;
using MWS.SharePoint.CUIT.Pages;
using System;
using System.Threading;
using MWS.CUIT.AppControls.WebControls;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using MWS.SharePoint.CUIT.Utilitiy;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace MWS.SharePoint.CUIT
{
    /// <summary>
    /// Contains App tests for:
    /// - Testing if it is possible to change profile picture
    /// </summary>
    [CodedUITest]
    public class ManageUserProfile : Base
    {
        private string environment = ConfigurationManager.AppSettings["TeamSiteURL"];
        private string mySitesUrl = ConfigurationManager.AppSettings["MySitesURL"];
        private string userProfilePicture = ConfigurationManager.AppSettings["userProfilePicture"];
        private string docSampleFilePath = ConfigurationManager.AppSettings["SampleFilePath"];
        private string docSampleFileBigForMySite = ConfigurationManager.AppSettings["SampleFileOverPersonalQuota"];
        private string docUploadLibNamePrefix = ConfigurationManager.AppSettings["DocLibNamePrefix"];

        private xBrowser browser;

        #region XML Comments
        /// <summary>
        /// Test summary details are:
        /// <list type="bullet">
        /// <item>
        /// <description>Test Case Number: BS_TC034</description>
        /// </item>
        /// <item>
        /// <description>Requirement Number: BS3</description>
        /// </item>
        /// <item>
        /// <description>Requirement: My Sites</description>
        /// </item>
        /// <item>
        /// <description>Offering: Standard</description>
        /// </item>
        /// </list>
        /// The test steps are:
        /// <list type="number">
        /// <item>
        /// <description>Test if a user profile picture can be changed</description>
        /// </item>
        /// <item>
        /// <description>Navigate to the user My Site</description>
        /// </item>
        /// <item>
        /// <description>Change profile picture</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestCategory("MySites")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC034")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void UploadProfilePhoto()
        {
            string user = User.SiteMemberUserName;
            string password = User.SiteMemberPassword;

            Console.WriteLine("Changing profile picture");

            ManageUserProfilePage profilePage = new ManageUserProfilePage(mySitesUrl);

            bool changedProfilePictureSuccessfully = profilePage.ChangeProfilePicture(userProfilePicture, docSampleFilePath, user, password);
            Assert.AreEqual(changedProfilePictureSuccessfully, true, string.Format("Failed to change Profile picture for user {0}", user));
        }

        #region XML Comments
        /// <summary>
        /// Attempt to upload a file that exceeds quota on a personal site.
        /// This test should fail.
        /// <list type="number">
        /// <item>
        /// <description>Create a Document Library</description>
        /// </item>
        /// <item>
        /// <description>Upload a file that exceeds quota on a personal site</description>
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
        [TestCategory("BS_TC077")]
        [TestCategory("MySites")]
        public void UploadInValidLargeDocumentToMySite()
        {
            // need to get the big file and then a second file which is the one that will fail
            NameValueCollection allSampleFiles = (NameValueCollection)ConfigurationManager.GetSection("SampleFiles");
            NameValueCollection sampleFiles = new NameValueCollection();
            sampleFiles.Add(allSampleFiles[0], allSampleFiles[0]);
            sampleFiles.Add(docSampleFileBigForMySite, docSampleFileBigForMySite);

            // expecting the upload to fail
            bool UploadFailed = Upload(sampleFiles);

            // if one of the documents fail to upload successfully then the test as a whole will be failed
            Assert.AreNotEqual(UploadFailed, true, string.Format("The document was incorrectly uploaded when the upload should have failed due to its file size. See test output for specific error message."));
        }

        /// <summary>
        /// Your changes could not be saved because this SharePoint Web site has exceeded the storage quota limit.
        /// You must save your work to another location.  Contact your administrator to change the quota limits for the Web site. 
        /// Upload the documents to the DOcuments library located on a personal site (created by default)
        /// </summary>
        /// <param name="sampleFiles">Collection of files to upload</param>
        /// <returns>True if files were successfully uploaded</returns>
        public bool Upload(NameValueCollection sampleFiles)
        {
            string user = User.SiteVisitorUserName;
            string password = User.SiteVisitorPassword;
            //string user = User.SiteCollectionUserName;
            //string password = User.SiteCollectionPassword;

            // Arrange
            AddAnApp app = new AddAnApp();

            string appName = docUploadLibNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            string appURL = string.Format("{0}{1}", mySitesUrl, appName);

            // Create the Doc Lib that a document will be uploaded to
            app.CreateApp("Document Library", appName, appURL, user, password, true, mySitesUrl);

            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(environment, User.SiteMemberUserName, User.SiteMemberPassword);

            browser.NavigateToUrl(new Uri(appURL));

            ManageDocument document = new ManageDocument(browser);
            string failedDocs = document.UploadDocument(sampleFiles, appURL, docSampleFilePath, true);

            // should expect the 2nd file to fail
            // check that the 2nd file is in the failedDocs var 
            // if it is then this test passed
            if (!string.IsNullOrEmpty(failedDocs))
            {
                Assert.AreNotEqual(failedDocs, string.Empty, string.Format("The document {0} was incorrectly uploaded when the upload should have failed due to its file size. See test output for specific error message.", failedDocs));
                return true;
            }
            else
            {
                return false;
            }
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
                //ManageDocumentPage doc = new ManageDocumentPage(browser);
                ManageUserProfilePage doc = new ManageUserProfilePage(mySitesUrl);

                // Act - Attempt to upload document
                bool docUploadedSuccessfully = doc.UploadDocument(this.browser, appURL, file, docSampleFilePath);

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

        #region XML Comments
        /// <summary>
        /// Remove profile picture
        /// </summary>
        #endregion
        public void RemoveProfilePhoto()
        {
            string user = User.SiteMemberUserName;
            string password = User.SiteMemberPassword;

            Console.WriteLine("Removing profile picture");

            ManageUserProfilePage profilePage = new ManageUserProfilePage(mySitesUrl);

            bool removedProfilePictureSuccessfully = profilePage.RemoveProfilePicture(user, password);
            Assert.AreEqual(removedProfilePictureSuccessfully, true, string.Format("Failed to remove Profile picture for user {0}", user));
        }
    }
}
