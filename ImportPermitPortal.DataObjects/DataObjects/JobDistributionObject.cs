
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class JobDistributionObject
    {
        public long EmployeeId { get; set; }
        public int? ApplicationCount { get; set; }
        public int? TotalJobCount { get; set; }
        public int? NotificationCount { get; set; }
        public int? RecertificationCount { get; set; }
        public string EmployeeName { get; set; }
    }    
}

