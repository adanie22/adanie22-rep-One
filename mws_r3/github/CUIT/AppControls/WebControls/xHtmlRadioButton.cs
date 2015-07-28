using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;

namespace MWS.CUIT.AppControls.WebControls
{
    /// <summary>
    /// Inherits from Microsoft.VisualStudio.TestTools.UITesting.HtmlControls.HtmlRadioButton
    /// Implements base methods and properties defined in the IxControl interface
    /// </summary>
    public class xHtmlRadioButton : HtmlRadioButton, IxControl
    {
        public xHtmlRadioButton(UITestControl parent) : base(parent) { }

        public xHtmlRadioButton(UITestControl parent, string id, string searchProperty = "Id")
            : base(parent)
        {
            this.SearchProperties.Add(searchProperty, id);
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
