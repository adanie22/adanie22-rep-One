using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;

namespace MWS.CUIT.AppControls.WebControls
{
    /// <summary>
    /// Inherits from Microsoft.VisualStudio.TestTools.UITesting.HtmlControls.HtmlHyperlink
    /// Implements base methods and properties defined in the IxControl interface
    /// </summary>
    public class xHtmlHyperlink : HtmlHyperlink, IxControl
    {
        public xHtmlHyperlink(UITestControl parent) : base(parent) { }

        public xHtmlHyperlink(UITestControl parent, string id, string searchProperty = "Id")
            : base(parent)
        {
            this.SearchProperties.Add(searchProperty, id);
        }

        /// <summary>
        /// Returns UITestControlCollection of all links located in a page
        /// This method is useful in situations where dealing with dynamically generated hyperlinks
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public UITestControlCollection GetPageLinks()
        {
            this.WaitForControlExist();
            return this.FindMatchingControls();
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
