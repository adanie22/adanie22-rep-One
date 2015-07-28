using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using MWS.CUIT.AppControls;
using MWS.CUIT.AppControls.WebControls;
using MWS.SharePoint.CUIT.Utilitiy;
using System;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the Log in as a different user dialog
    /// </summary>
    public class LoginDialog
    {
        public LoginDialog()
        {
        }

        /// <summary>
        /// Log in as a different user
        /// Note SharePoint 2013 does not have a 'Sign in as different User'  menu item.
        /// </summary>
        /// <param name="url">Url to redirect to after logging on as different user</param>
        /// <param name="username">Domain name \ User account </param>
        /// <param name="password">Password of account</param>
        /// <returns>xBrowser of page redirected to after logging as as different user</returns>
        public xBrowser LogInAs(string url, string username, string password)
        {
            xBrowser browser = new xBrowser(string.Format("{0}_layouts/closeConnection.aspx?loginasanotheruser=true&Source={0}", url));

            Console.WriteLine(string.Format("Logging into {0} as user:{1} with password: {2}.", url, username, password));

            UITestControl loginDialogWindow = new UITestControl();
            loginDialogWindow.TechnologyName = "MSAA";
            loginDialogWindow.SearchProperties.Add("ControlType", "Window");
            loginDialogWindow.SearchProperties.Add("Name", "Windows Security");
            loginDialogWindow.SetFocus();

            Console.WriteLine(string.Format("2-{0}", DateTime.Now.ToString("HH:mm:ss")));

            WinEdit usernameEdit = new WinEdit(loginDialogWindow);
            usernameEdit.SearchProperties[WinEdit.PropertyNames.Name] = "User name";
            usernameEdit.WindowTitles.Add("Windows Security");
            Keyboard.SendKeys(usernameEdit, username);

            WinEdit passwordEdit = new WinEdit(loginDialogWindow);
            passwordEdit.SearchProperties[WinEdit.PropertyNames.Name] = "Password";
            passwordEdit.WindowTitles.Add("Windows Security");
            Keyboard.SendKeys(passwordEdit, password);

            Console.WriteLine(string.Format("3-{0}", DateTime.Now.ToString("HH:mm:ss")));

            WinSplitButton OKbutton = new WinSplitButton(loginDialogWindow);
            OKbutton.SearchProperties.Add(WinSplitButton.PropertyNames.Name, "OK");

            Console.WriteLine(string.Format("4-{0}", DateTime.Now.ToString("HH:mm:ss")));

            Playback.PlaybackSettings.ContinueOnError = true;

            Base.SetPlayBackFast();

            Console.WriteLine(string.Format("5-{0}", DateTime.Now.ToString("HH:mm:ss")));

            try
            {
                Mouse.Click(OKbutton);
                Console.WriteLine(string.Format("6-{0}", DateTime.Now.ToString("HH:mm:ss")));
            }
            finally
            {
                string b = browser.Uri.ToString();

                if (!b.Contains("closeConnection.aspx"))
                {
                    xHtmlDiv errorMsg = new xHtmlDiv(browser, "ms-error-header");
                    if (errorMsg.TryFind())
                    {
                        errorMsg.WaitForControlEnabled(ControlHelper.Wait);
                        Console.WriteLine(string.Format("Logging into {0} as user:{1} with password: {2}, FAILED.", url, username, password));
                        browser = null;
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Logging into {0} as user:{1} with password: {2}, SUCCEEDED.", url, username, password));
                    }
                }
                else
                {
                    WinSplitButton Cancelbutton = new WinSplitButton(loginDialogWindow);
                    Cancelbutton.SearchProperties.Add(WinSplitButton.PropertyNames.Name, "Cancel");

                    // clicking Cancel twice is required
                    Mouse.Click(Cancelbutton);
                    Mouse.Click(Cancelbutton);
                    
                    Console.WriteLine(string.Format("Logging into {0} as user:{1} with password: {2}, FAILED.", url, username, password));
                    browser = null;
                }
                Console.WriteLine(string.Format("7-{0}", DateTime.Now.ToString("HH:mm:ss")));

                Playback.PlaybackSettings.ContinueOnError = false;
                Base.SetPlayBackNormal();
            }

            return browser;
        }

    }
}
