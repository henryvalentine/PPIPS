using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportPermitPortal.DataObjects
{
    public class EmployeeStepObject
    {
        public long EmployeeDeskId { get; set; }
        public long EmployeeId { get; set; }
        public int StepId { get; set; }
        public int SequenceNumber { get; set; }
        public int ExpectedDeliveryDuration { get; set; }
        public int ActivityTypeId { get; set; }
        public int? JobCount { get; set; }
       
    }
}