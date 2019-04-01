
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ProcessTrackingObject
    {
        public long Id { get; set; }
        public long ApplicationId { get; set; }
        public int StepId { get; set; }
        public long EmployeeId { get; set; }
        public int StatusId { get; set; }
        public DateTime? AssignedTime { get; set; }
        public DateTime? DueTime { get; set; }
        public DateTime? ActualDeliveryDateTime { get; set; }
        public int StepCode { get; set; }
        public int? OutComeCode { get; set; }

        public virtual ApplicationObject ApplicationObject { get; set; }
        public virtual EmployeeDeskObject EmployeeDeskObject { get; set; }
        public virtual StepObject StepObject { get; set; }
    }
}



