
using System;
using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public partial class NotificationHistoryObject
    {


        public string ReferenceCode { get; set; }

        public string StepName { get; set; }
        public string EmployeeName { get; set; }
        public string StatusName { get; set; }
        public string AssignedTimeStr { get; set; }
        public string DueTimeStr { get; set; }
        public string ActualDeliveryDateTimeStr { get; set; }
        public string ProcessName { get; set; }
        public string OutComeCodeStr { get; set; }

        public string Processor { get; set; }
        public string DateLeftStr { get; set; }

        public string CompanyName { get; set; }
    }
}
