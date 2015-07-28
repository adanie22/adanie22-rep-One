using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MWS.CUIT.AppControls;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using System.IO;

namespace MWS.SharePoint.CUIT
{
    /// <summary>
    /// Base class for test classes 
    /// </summary>
    [CodedUITest]
    public class Base
    {
        private static string testOutputFolder = ConfigurationManager.AppSettings["TestOutputFolder"]; 

        /// <summary>
        /// Run ONCE at the start of ALL tests
        /// </summary>
        /// <param name="context"></param>
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            if (Directory.Exists(testOutputFolder))
            {
                Console.WriteLine(string.Format("Deleting output folder: {0}", testOutputFolder));
                Directory.Delete(testOutputFolder, true);
            }

            Console.WriteLine(string.Format("Creating output folder: {0}", testOutputFolder));
            Directory.CreateDirectory(testOutputFolder);
        }

        /// <summary>
        /// Run ONCE at the end of ALL tests
        /// </summary>
        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {
            if (Directory.Exists(testOutputFolder))
            {
                string targetFolder =string.Format("{0}_{1}", testOutputFolder, DateTime.Now.ToString("yyyMMddHHss"));
                Console.WriteLine(string.Format("Moving output folder from: {0} to: {1} ", testOutputFolder, targetFolder));
                Directory.Move(testOutputFolder, targetFolder);
            }
        }

        /// <summary>
        /// Set the Playback settings to run slower. This is the default option for every test.
        /// </summary>
        public static void SetPlayBackNormal()
        {
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.UIThreadOnly;
            Playback.PlaybackSettings.ShouldSearchFailFast = true;
            Playback.PlaybackSettings.MaximumRetryCount = 3;
            Playback.PlaybackSettings.SearchTimeout = 120000;
        }

        /// <summary>
        /// Configure the playback engine to run 'faster'
        /// Before using a test needs to be run to ensure that CUIT 'wait' long enough to ensure success.
        /// </summary>
        public static void SetPlayBackFast()
        {
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.Disabled;
            Playback.PlaybackSettings.SearchTimeout = 1000;
            Playback.PlaybackSettings.MaximumRetryCount = 1;
        }

        [TestInitialize]
        public void Init()
        {
            // original config settings
            BrowserWindow.CurrentBrowser = ConfigurationManager.AppSettings["Browser"];
            SetPlayBackNormal();
            Playback.PlaybackError += Playback_PlaybackError;

            if (ConfigurationManager.AppSettings["ClearBrowserAtTestStart"] == "true")
            {
                xBrowser.GetFreshBrowser();
            }

        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (Playback.IsInitialized)
            {
                RenameTestOutputFile();
                Playback.Cleanup();
            }
        }

        /// <summary>
        /// Rename the UITestActionLog.html file to {Name of Test}.html
        /// Note the original file remains in the original location
        /// </summary>
        private void RenameTestOutputFile()
        {
            string currentFile = "UITestActionLog.html";
            string testName = TestContext.TestName.ToString();
            string sourcefile = System.IO.Path.Combine(TestContext.TestResultsDirectory, currentFile);
            string destfile = System.IO.Path.Combine(testOutputFolder, testName + ".html");
            // file doesnt exist if the 'EqtTraceLevel' value is set to 0
            if (File.Exists(sourcefile))
            {
                System.IO.File.Copy(sourcefile, destfile);
            }
        }

        // Retry failed action error handler
        private void Playback_PlaybackError(object sender, PlaybackErrorEventArgs e)
        {
            Console.WriteLine("Retrying .... ");
            e.Result = PlaybackErrorOptions.Retry;
            Keyboard.SendKeys("{Enter}");
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;
    }
}
