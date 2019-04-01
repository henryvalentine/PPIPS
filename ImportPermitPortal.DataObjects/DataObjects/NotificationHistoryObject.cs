
using System;
using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public partial class NotificationHistoryObject
    {
        public long Id { get; set; }
        public long NotificationId { get; set; }
        public int StepId { get; set; }
        public int EmployeeId { get; set; }
        public Nullable<System.DateTime> AssignedTime { get; set; }
        public Nullable<System.DateTime> DueTime { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> OutComeCode { get; set; }
        public Nullable<System.DateTime> FinishedTime { get; set; }

        public virtual EmployeeDeskObject EmployeeDeskObject { get; set; }
        public virtual NotificationObject NotificationObject { get; set; }
        public virtual StepObject StepObject { get; set; }
    }
}


