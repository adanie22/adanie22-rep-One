using Microsoft.VisualStudio.TestTools.UITesting;
using MWS.CUIT.AppControls;
using MWS.CUIT.AppControls.WebControls;
using MWS.SharePoint.CUIT.Utilitiy;
using System;
using System.Threading;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the edit profile page
    /// </summary>
    public class ManageUserProfilePage
    {
        private string mySitesUrl;
        xBrowser browser;

        public ManageUserProfilePage(string envUrl)
        {
            this.mySitesUrl = envUrl;
        }

        /// <summary>
        /// Change user photo on the profile page and test to see if the change was committed
        /// </summary>
        /// <param name="userProfilePicture">Name of image that will uploaded onto the user profile</param>
        /// <param name="docSampleFilePath">Location of the user profile photo</param>
        /// <returns>True if web part was added successfully</returns>
        public bool ChangeProfilePicture(string userProfilePicture, string docSampleFilePath, string user, string password)
        {
            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(mySitesUrl, user, password);
        
            // Click on the Sites link in the ribbon
            Console.WriteLine("Click on link Sites");
            xHtmlHyperlink linkSitesRibbon = new xHtmlHyperlink(browser, "Sites", "InnerText");
            #region Checking if Control can be found         
            if (!linkSitesRibbon.TryFind())
            {
                
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Sites", mySitesUrl));
                return false;
            }
            #endregion
            linkSitesRibbon.WaitForControlEnabled(ControlHelper.Wait);
            linkSitesRibbon.Click();           

            // Personal site is opened. 
            // Click on the image to edit the profile
            Console.WriteLine("Click on profile image");
            xHtmlImage userPhoto = new xHtmlImage(browser, "User Photo", "Alt");
            #region Checking if Control can be found
            if (!userPhoto.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "User Photo", "Personal Site"));
                return false;
            }
            #endregion
            userPhoto.WaitForControlEnabled(ControlHelper.Wait);

            Console.WriteLine(string.Format("Original Profile photo: {0}", userPhoto.AbsolutePath));
            userPhoto.Click();

            // Edit profile page is opened
            // Click on the Edit profile link
            Console.WriteLine("Click on link Edit");
            xHtmlHyperlink linkEditProfile = new xHtmlHyperlink(browser, "edit", "InnerText");
            #region Checking if Control can be found
            if (!linkEditProfile.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Edit", "Edit profile page"));
                return false;
            }
            #endregion
            linkEditProfile.WaitForControlEnabled(ControlHelper.Wait);
            linkEditProfile.Click();

            // Click on the Upload picture link
            Console.WriteLine("Click on link Upload Picture");
            xHtmlInputButton uploadPictureBtn = new xHtmlInputButton(browser, "Upload picture", "DisplayText");
            #region Checking if Control can be found
            if (!uploadPictureBtn.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Upload picture", "Edit profile page"));
                return false;
            }
            #endregion
            uploadPictureBtn.WaitForControlEnabled(ControlHelper.Wait);
            uploadPictureBtn.Click();

            // Browse to the image to upload
            Console.WriteLine("Browse to the picture to upload");
            xHtmlFileInput userPhotoFileInput = new xHtmlFileInput(browser, "profileimagepickerinput", "Id");
            #region Checking if Control can be found
            if (!userPhotoFileInput.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "profileimagepickerinput", "Edit profile page"));
                return false;
            }
            #endregion

            Base.SetPlayBackFast();
            Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.AllThreads;

            bool onError = Playback.PlaybackSettings.ContinueOnError;
            try
            {
                // the Click event was being executed a number of times then raising an error
                // I suspect its related to Silverlight so this Try/Catch and ContinueOnError is to workaround the issue
                Playback.PlaybackSettings.ContinueOnError = true;

                Mouse.DoubleClick(userPhotoFileInput);

                OpenDialog.Open(userProfilePicture, docSampleFilePath);
            }
            catch (Microsoft.VisualStudio.TestTools.UITest.Extension.UITestControlNotFoundException ex)
            {
                string error = ex.BasicMessage;
                Console.WriteLine(string.Format("Error while uploading {1}.{0}{2}", Environment.NewLine, userProfilePicture, error));
            }
            finally
            {
                Playback.PlaybackSettings.ContinueOnError = onError;
                Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.UIThreadOnly;
            }
            Base.SetPlayBackNormal();

            Console.WriteLine("Click on Upload button");
            xHtmlInputButton uploadBtn = new xHtmlInputButton(browser, "Upload", "DisplayText");
            #region Checking if Control can be found
            if (!uploadBtn.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Upload", "Edit profile page"));
                return false;
            }
            #endregion
            uploadBtn.WaitForControlEnabled(ControlHelper.Wait);
            uploadBtn.Click();

            Console.WriteLine("Click on Save All And Close button");
            xHtmlInputButton saveAllAndCLoseBtn = new xHtmlInputButton(browser, "Save all and close", "DisplayText");
            #region Checking if Control can be found
            if (!saveAllAndCLoseBtn.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Save all and close", "Edit profile page"));
                return false;
            }
            #endregion
            saveAllAndCLoseBtn.Click();

            Console.WriteLine("Click on OK button");
            xHtmlButton okBtn = new xHtmlButton(browser, "OK", "DisplayText");
            #region Checking if Control can be found
            if (!okBtn.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "OK", "Edit profile page"));
                return false;
            }
            #endregion
            okBtn.Click();

            Thread.Sleep(ControlHelper.Wait);
            Console.WriteLine("Refreshing the browser");
            browser.Refresh();

            // Check if the user profile picture has been uploaded
            Console.WriteLine("Getting new photo");
            xHtmlImage userPhotoNew = new xHtmlImage(browser, "User Photo", "Alt");
            #region Checking if Control can be found
            if (!userPhotoNew.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "User Photo", "Personal Site"));
                return false;
            }
            #endregion

            Console.WriteLine(string.Format("New Profile photo: {0}", userPhotoNew.AbsolutePath));
            if (!userPhotoNew.AbsolutePath.Contains("PersonPlaceholder"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Upload a document to a document library and validate it was uploaded
        /// </summary>
        /// <param name="url">Url to the Document Library</param>
        /// <param name="filename">Name of file to be uploaded</param>
        public bool UploadDocument(xBrowser b, string url, string filename, string docSampleFilePath)
        {
            browser = b;
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

            Console.WriteLine("1");
            Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.AllThreads;

            Console.WriteLine("2");

            xHtmlFileInput FileBrowseBtn = new xHtmlFileInput(this.browser, "HtmlFileInput", "ClassName");
            FileBrowseBtn.WaitForControlEnabled(ControlHelper.Wait);
            bool onError = Playback.PlaybackSettings.ContinueOnError;
            try
            {
                Console.WriteLine("3");

                // the Click event was being executed a number of times then raising an error
                // I suspect its related to Silverlight so this Try/Catch and ContinueOnError is to workaround the issue
                Playback.PlaybackSettings.ContinueOnError = true;
                Console.WriteLine("4");

                // Mouse.Hover(FileBrowseBtn);
                Mouse.DoubleClick(FileBrowseBtn);
                Console.WriteLine("5");

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

            Console.WriteLine("6");

            Console.WriteLine("Getting OK Button");
            xHtmlInputButton UploadDocOKBtn = new xHtmlInputButton(this.browser, "OK", "DisplayText");
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
        private bool DocumentExists(string url, string filename)
        {
            // check if a link to the uploaded file can be found on the page
            string documentUrl = string.Format("{0}/{1}", url, filename);

            Playback.PlaybackSettings.WaitForReadyLevel = Microsoft.VisualStudio.TestTools.UITest.Extension.WaitForReadyLevel.AllThreads;

            //docLink = new xHtmlHyperlink(this.browser, documentUrl, "Href");
            xHtmlHyperlink docLink = new xHtmlHyperlink(this.browser, documentUrl, "Href");
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
        /// Get URL of the existing App
        /// </summary>
        /// <param name="browser">Browser object</param>
        /// <param name="myURL">Personal site URL of the logged on user</param>
        /// <param name="appName">App name that exists on the personal site</param>
        /// <returns></returns>
        public string GetAppUrl(xBrowser browser, string appName)
        {
            
            Console.WriteLine("Click on Settings");
            xHtmlHyperlink settingsLink = new xHtmlHyperlink(browser, "Settings", "Title");
            if (!settingsLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "settingsImg"));
                return string.Empty;
            }
            settingsLink.Click();
            
            Console.WriteLine("Click on Site Content link");
            xHtmlHyperlink siteContentLink = new xHtmlHyperlink(browser, "Site contents", "Title");
            if (!siteContentLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "appsLink"));
                return string.Empty;
            }
            siteContentLink.WaitForControlEnabled(ControlHelper.Wait);
            siteContentLink.Click();

            Console.WriteLine("Find Documents link");
            xHtmlHyperlink apLink = new xHtmlHyperlink(browser, appName, "InnerText");
            if (!apLink.TryFind())
            {
                Console.WriteLine(string.Format("The {0} control was NOT found.", "apLink"));
                return string.Empty;
            }
            apLink.Click();

            return apLink.Href;
        }

        /// <summary>
        /// Remove user photo from the user profile and test to see if the change was committed
        /// </summary>
        /// <returns>True if web part was added successfully</returns>
        public bool RemoveProfilePicture(string user, string password)
        {
            LoginDialog login = new LoginDialog();
            xBrowser browser = login.LogInAs(mySitesUrl, user, password); 

            // Click on the Sites link in the ribbon
            Console.WriteLine("Click on link Sites");
            xHtmlHyperlink linkSitesRibbon = new xHtmlHyperlink(browser, "Sites", "InnerText");
            #region Checking if Control can be found
            if (!linkSitesRibbon.TryFind())
            {

                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Sites", mySitesUrl));
                return false;
            }
            #endregion
            linkSitesRibbon.WaitForControlEnabled(ControlHelper.Wait);
            linkSitesRibbon.Click();

            // Personal site is opened. 
            // Click on the image to edit the profile
            Console.WriteLine("Click on profile image");
            xHtmlImage userPhoto = new xHtmlImage(browser, "User Photo", "Alt");
            #region Checking if Control can be found
            if (!userPhoto.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "User Photo", "Personal Site"));
                return false;
            }
            #endregion
            userPhoto.WaitForControlEnabled(ControlHelper.Wait);

            Console.WriteLine(string.Format("Original Profile photo: {0}", userPhoto.AbsolutePath));
            // If the profile has photo uploaded, remove it first
            if (userPhoto.AbsolutePath.Contains("PersonPlaceholder"))
            {
                Console.WriteLine("No profile picture found.");
                return true;
            }

            userPhoto.Click();

            // Edit profile page is opened
            // Click on the Edit profile link
            Console.WriteLine("Click on link Edit");
            xHtmlHyperlink linkEditProfile = new xHtmlHyperlink(browser, "edit", "InnerText");
            #region Checking if Control can be found
            if (!linkEditProfile.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Edit", "Edit profile page"));
                return false;
            }
            #endregion
            linkEditProfile.WaitForControlEnabled(ControlHelper.Wait);
            linkEditProfile.Click();

            Console.WriteLine("Click on Remove button");
            xHtmlInputButton removeBtn = new xHtmlInputButton(browser, "Remove", "DisplayText");
            if (!removeBtn.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Remove", "Edit profile page"));
                return false;
            }
            removeBtn.Click();

            Console.WriteLine("Click on Save All And Close button");
            xHtmlInputButton saveAllAndCLoseBtn = new xHtmlInputButton(browser, "Save all and close", "DisplayText");
            #region Checking if Control can be found
            if (!saveAllAndCLoseBtn.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "Save all and close", "Edit profile page"));
                return false;
            }
            #endregion
            saveAllAndCLoseBtn.Click();

            Console.WriteLine("Click on OK button");
            xHtmlButton okBtn = new xHtmlButton(browser, "OK", "DisplayText");
            #region Checking if Control can be found
            if (!okBtn.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "OK", "Edit profile page"));
                return false;
            }
            #endregion
            okBtn.Click();

            Thread.Sleep(ControlHelper.Wait);
            Console.WriteLine("Refreshing the browser");
            browser.Refresh();

            // Check if the user profile picture has been uploaded
            Console.WriteLine("Getting new photo");
            xHtmlImage userPhotoNew = new xHtmlImage(browser, "User Photo", "Alt");
            #region Checking if Control can be found
            if (!userPhotoNew.TryFind())
            {
                Console.WriteLine(string.Format("Could not find the control: {0} at {1}", "User Photo", "Personal Site"));
                return false;
            }
            #endregion

            Console.WriteLine(string.Format("New Profile photo: {0}", userPhotoNew.AbsolutePath));
            if (userPhotoNew.AbsolutePath.Contains("PersonPlaceholder"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
