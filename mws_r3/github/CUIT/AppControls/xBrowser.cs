using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MWS.CUIT.AppControls
{
    /// <summary>
    /// Inherits from for Microsoft.VisualStudio.TestTools.UITesting.BrowserWindow
    /// Use this class for generic utility methods that manage browser and playback configuration
    /// </summary>
    public class xBrowser : BrowserWindow
    {
        public xBrowser() : base() { }
        public xBrowser(string url)
        {
            BrowserWindow.Launch(url);
        }

        /// <summary>
        /// Makes the browser pause by stopping the current thread
        /// </summary>
        /// <param name="seconds">Takes in the number of seconds to wait</param>
        public static void Wait(int seconds)
        {
            Thread.Sleep(seconds * 1000);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public static void SkipPropertyVerification(bool skipVerify)
        {
            Playback.PlaybackSettings.SkipSetPropertyVerification = skipVerify;
        }

        /// <summary>
        /// Clear browser cookies and cache and closes all running instances of browser
        /// Selenium plugin is required for Coded UI Cross-Browser compatibility
        /// </summary>
        /// <param name="browserProcessName">Defaults to IE and can additionally take in process names for Chrome, Firefox</param>
        public static void GetFreshBrowser(string browserProcessName = "iexplore")
        {
            foreach (Process p in Process.GetProcessesByName(browserProcessName))
            {
                p.Kill();
            }

            BrowserWindow.ClearCookies();
            BrowserWindow.ClearCache();
        }

        /// <summary>
        /// Utilized to switch between browser windows
        /// Selenium plugin is required for Coded UI Cross-Browser compatibility
        /// </summary>
        /// <param name="b">Defaults to Internet Explorer class and can additionally take in class names for Chrome, Firefox</param>
        /// <returns></returns>
        public static UITestControlCollection SwitchWindow(BrowserWindow b, string browserClass = "IEFrame")
        {
            b.SearchProperties[PropertyNames.ClassName] = browserClass;
            return b.FindMatchingControls();
        }

        /// <summary>
        /// Selects 'OK' for browser dialog popups
        /// </summary>
        /// <param name="b">Takes current browser</param>
        public static void AlertOK(BrowserWindow b)
        {
            b.PerformDialogAction(BrowserDialogAction.Ok);
        }

        /// <summary>
        /// Waits for control to exist, be enabled, and ready
        /// </summary>
        /// <param name="control">Target UITestControl</param>
        public static void WaitForControl(UITestControl control)
        {
            control.Find();
            control.WaitForControlExist();
            control.WaitForControlEnabled();
            control.WaitForControlReady();
        }
    }
}
