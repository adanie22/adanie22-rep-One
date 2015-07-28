using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;

namespace MWS.CUIT.AppControls.WebControls
{
    /// <summary>
    /// Inherits from Microsoft.VisualStudio.TestTools.UITesting.HtmlControls.HtmlComboBox
    /// Implements base methods and properties defined in the IxControl interface
    /// </summary>
    public class xHtmlComboBox : HtmlComboBox, IxControl
    {
        public xHtmlComboBox(UITestControl parent) : base(parent) { }

        public xHtmlComboBox(UITestControl parent, string id, string searchProperty = "Id")
            : base(parent)
        {
            this.SearchProperties.Add(searchProperty, id);
        }

        /// <summary>
        /// Waits for control to be ready and then selects the desired item by its string value
        /// </summary>
        /// <param name="s">Html ComboBox string value</param>
        public void Select(string text)
        {
            this.WaitForControlReady();
            this.SelectedItem = text;
        }

        /// <summary>
        /// Waits for control to be ready and then selects the desired item by its index value
        /// </summary>
        /// <param name="index">Html Combobox index</param>
        public void Select(int index)
        {
            this.WaitForControlReady();
            this.SelectedIndex = index;
        }

        /// <summary>
        /// This method selects a random selection on a ComboBox control. 
        /// Use when the actual control selection is not important for the tests purpose.
        /// </summary>
        /// <param name="indexBegin">Most ComboBox controls have blank values for index 0 so this optional parameter defaults at 1</param>
        public void SelectRandom(int indexBegin = 1)
        {
            this.WaitForControlReady();
            this.SelectedIndex = new Random().Next(indexBegin, this.ItemCount);
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
