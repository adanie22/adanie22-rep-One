using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;

namespace MWS.CUIT.AppControls.WebControls
{
    /// <summary>
    /// Inherits from Microsoft.VisualStudio.TestTools.UITesting.HtmlControls.HtmlComboBox
    /// Implements base methods and properties defined in the IxControl interface
    /// </summary>
    public class xHtmlList : HtmlList, IxControl
    {
        public xHtmlList(UITestControl parent) : base(parent) { }

        public xHtmlList(UITestControl parent, string id, string searchProperty = "Id")
            : base(parent)
        {
            this.SearchProperties.Add(searchProperty, id);
        }

        /// <summary>
        /// Waits for control to be ready and then selects the desired item by its string value
        /// </summary>
        /// <param name="s">Html List string value</param>
        public void Select(string text)
        {
            this.WaitForControlReady();
            this.SelectedItemsAsString = text;
        }


        public void Focus()
        {
            this.WaitForControlReady();
            this.SetFocus();
            this.SetFocus();
        }

        public void Click()
        {
            this.WaitForControlReady();
            this.SetFocus();
            Mouse.Click(this);
        }

        public void SendKeys(string text)
        {
            this.WaitForControlReady();
            this.SetFocus();
            Keyboard.SendKeys(this, text);
        }

        public void Tab()
        {
            this.WaitForControlReady();
            Keyboard.SendKeys("{ENTER}");
        }
    }
}
