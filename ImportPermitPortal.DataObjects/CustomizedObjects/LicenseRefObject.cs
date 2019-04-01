using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportPermitPortal.DataObjects
{
    public class LicenseRefObject
    {
        public string RefCode { get; set; }
        public string ImporterName { get; set; }
        public int LicenseType { get; set; }
        public string JettyName { get; set; }
        public int JettyId { get; set; } 
        public long ImporterId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool Status { get; set; }
    }
}


 
