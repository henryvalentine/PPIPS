
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class RecertificationHistoryObject
    {
        public long Id { get; set; }
        public long RecertificationId { get; set; }
        public int StepId { get; set; }
        public long EmployeeId { get; set; }
        public Nullable<System.DateTime> AssignedTime { get; set; }
        public Nullable<System.DateTime> DueTime { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> OutComeCode { get; set; }
        public Nullable<System.DateTime> FinishedTime { get; set; }

        public virtual EmployeeDeskObject EmployeeDeskObject { get; set; }
        public virtual RecertificationObject RecertificationObject { get; set; }
        public virtual StepObject StepObject { get; set; }
    }
}
