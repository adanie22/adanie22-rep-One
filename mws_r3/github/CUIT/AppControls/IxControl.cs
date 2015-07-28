using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWS.CUIT.AppControls
{
    /// <summary>
    /// This interface defines the base methods to be implemented by your custom control classes
    /// </summary>
    public interface IxControl
    {
        void Focus();
        void Click();
        void SendKeys(String s);
        void Tab();
    }
}
