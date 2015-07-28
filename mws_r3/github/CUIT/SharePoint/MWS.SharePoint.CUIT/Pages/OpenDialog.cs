using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using System;

namespace MWS.SharePoint.CUIT.Pages
{
    /// <summary>
    /// Represent's the browse to a file dialog
    /// </summary>
    public static class OpenDialog
    {
        /// <summary>
        /// Open the Windows Dialog screen, find the file to be uploaded and return it to the calling File Upload control
        /// </summary>
        /// <param name="filename">Name of file to be uploaded</param>
        /// <param name="docSampleFilePath">Path to file to be uploaded</param>
        /// <param name="buttonName">Name of 'OK' button</param>
        /// <param name="windowName">Name of Dialog window</param>
        public static void Open(string filename, string docSampleFilePath, string buttonName = "Open", string windowName = "Choose File to Upload")
        {
            Console.WriteLine("Show Open Dialog Windows");

            UITestControl openDialogWindow = new UITestControl();
            openDialogWindow.TechnologyName = "MSAA";
            openDialogWindow.SearchProperties.Add("ControlType", "Window");
            openDialogWindow.SearchProperties.Add("Name", windowName);
            openDialogWindow.SetFocus();

            Keyboard.SendKeys(openDialogWindow, string.Format("{0}{1}", docSampleFilePath, filename));

            Console.WriteLine("Entered file in Open Dialog Windows");

            WinSplitButton openbutton = new WinSplitButton(openDialogWindow);
            openbutton.SearchProperties.Add(WinSplitButton.PropertyNames.Name, buttonName);
            Mouse.Click(openbutton);

            Console.WriteLine("Click 'Open' in Open Dialog Windows");
        }   
    }
}
