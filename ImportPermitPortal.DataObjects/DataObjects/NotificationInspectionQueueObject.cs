
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class NotificationInspectionQueueObject
    {
        public long Id { get; set; }
        public long NotificationId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? AssignedTime { get; set; }
        public DateTime? InspectionDate { get; set; }
        public int OutComeCode { get; set; }
        public DateTime? DueTime { get; set; }
        public DateTime? ActualDeliveryDateTime { get; set; }
        public int StepCode { get; set; }
        public int StepId { get; set; }
        public int StatusId { get; set; }

        public virtual EmployeeDeskObject EmployeeDeskObject { get; set; }
        public virtual NotificationObject NotificationObject { get; set; }
        public virtual StepObject StepObject { get; set; }
    }
}
