using System;
using System.Web;

namespace ImportPermitPortal.Helpers
{
    public static class PhysicalToVirtualPathMapper
    {
        public static string MapPath(string path)
        {
            return @"~/" + path.Replace(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"], String.Empty).Replace('\\', '/');
        }
    }
}