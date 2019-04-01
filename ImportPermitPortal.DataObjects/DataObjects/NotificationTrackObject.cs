

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class NotificationTrackObject
    {
        public int Id { get; set; }
        public long NotificationId { get; set; }
        public int StepId { get; set; }
        public int EmployeeId { get; set; }
        public int StatusId { get; set; }
        public Nullable<System.DateTime> AssignedTime { get; set; }
        public Nullable<System.DateTime> DueTime { get; set; }
        public Nullable<System.DateTime> ActualDeliveryDateTime { get; set; }
        public int StepCode { get; set; }
        public Nullable<int> OutComeCode { get; set; }

        public virtual EmployeeDeskObject EmployeeDeskObject { get; set; }
        public virtual NotificationObject NotificationObject { get; set; }
        public virtual StepObject StepObject { get; set; }
    }
}


 