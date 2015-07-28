using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWS.SharePoint.CUIT.Utilitiy
{
    /// <summary>
    /// User class to provide username and passwords for:
    /// - Site Collection Admins
    /// - Site Owners
    /// - Site Members
    /// - Site Visitors
    /// </summary>
    public static class User
    {
        public static string SiteCollectionUserName
        {
            get { return ConfigurationManager.AppSettings["SiteCollectionUserName"]; ; }
        }

        public static string SiteCollectionPassword
        {
            get { return ConfigurationManager.AppSettings["SiteCollectionPassword"]; ; }
        }

        public static string SiteOwnerUserName
        {
            get { return ConfigurationManager.AppSettings["SiteOwnerUserName"]; ; }
        }
        public static string SiteOwnerPassword
        {
            get { return ConfigurationManager.AppSettings["SiteOwnerPassword"]; ; }
        }

        public static string SiteMemberUserName
        {
            get { return ConfigurationManager.AppSettings["SiteMemberUserName"]; ; }
        }
        public static string SiteMemberPassword
        {
            get { return ConfigurationManager.AppSettings["SiteMemberPassword"]; ; }
        }

        public static string SiteVisitorUserName
        {
            get { return ConfigurationManager.AppSettings["SiteVisitorUserName"]; ; }
        }
        public static string SiteVisitorPassword
        {
            get { return ConfigurationManager.AppSettings["SiteVisitorPassword"]; ; }
        }

        public static string NoPermissionSiteUserName
        {
            get { return ConfigurationManager.AppSettings["NoPermissionSiteUserName"]; ; }
        }
        public static string NoPermissionSitePassword
        {
            get { return ConfigurationManager.AppSettings["NoPermissionSitePassword"]; ; }
        }
    }
}
