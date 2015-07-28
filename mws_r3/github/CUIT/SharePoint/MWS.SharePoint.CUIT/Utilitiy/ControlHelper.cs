using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWS.SharePoint.CUIT.Utilitiy
{
    /// <summary>
    /// Helper class for controls
    /// </summary>
    public static class ControlHelper
    {
        /// <summary>
        /// If an invalid value is found in app.config default will be 5000
        /// </summary>
        static int returnWait = 5000;

        /// <summary>
        /// Return the persion in milliseconds to wait
        /// </summary>
        public static int Wait
        {
            get
            {
                if (int.TryParse(ConfigurationManager.AppSettings["Wait"], out returnWait))
                {
                    return returnWait;
                }
                // If an invalid value is found in app.config default will be 5000
                return 5000;
                
            }
        }
    }
}
