
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class RecertificationProcessObject

    {
        public long Id { get; set; }
        public long RecertificationId { get; set; }
        public int StepId { get; set; }
        public long EmployeeId { get; set; }
        public int StatusId { get; set; }
        public Nullable<System.DateTime> AssignedTime { get; set; }
        public Nullable<System.DateTime> DueTime { get; set; }
        public Nullable<System.DateTime> ActualDeliveryDateTime { get; set; }
        public int StepCode { get; set; }
        public Nullable<int> OutComeCode { get; set; }

        public virtual ApplicationObject ApplicationObject { get; set; }
        public virtual EmployeeDeskObject EmployeeDeskObject { get; set; }
        public virtual StepObject StepObject { get; set; }
    }
}


