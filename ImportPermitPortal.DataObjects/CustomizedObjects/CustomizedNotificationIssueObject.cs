

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class NotificationIssueObject
    {
        public string IssueTypeName { get; set; }
        public string StatusStr { get; set; }
        public string IssueDateStr { get; set; }
        public string AmendedDateStr { get; set; }

        public string ReferenceCode { get; set; }

        public string EmployeeName { get; set; }

        public string CompanyName { get; set; }
    }
}



