using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportPermitPortal.BizObjects
{
    public class EmployeeStepObject
    {
        public int EmployeeId { get; set; }
        public int StepId { get; set; }
        public int SequenceNumber { get; set; }
        public int ExpectedDeliveryDuration { get; set; }
       
    }
}