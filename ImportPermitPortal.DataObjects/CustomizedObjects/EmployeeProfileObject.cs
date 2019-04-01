using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportPermitPortal.DataObjects
{
    public class EmployeeProfileObject
    {
        public long ApplicationId { get; set; }
        public int IssueTypeId { get; set; }
        public string Description { get; set; }

        public int EmployeeId { get; set; }
    }
}