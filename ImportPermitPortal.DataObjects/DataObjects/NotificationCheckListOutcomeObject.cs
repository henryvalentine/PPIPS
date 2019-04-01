
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class NotificationCheckListOutcomeObject
    {
        public int Id { get; set; }
        public long NotificationId { get; set; }
        public int NotificationCheckListId { get; set; }
        public int EmployeeId { get; set; }
        public System.DateTime Date { get; set; }
        public double Value { get; set; }
        public int Status { get; set; }

        public virtual NotificationObject NotificationObject { get; set; }
        public virtual NotificationCheckListObject NotificationCheckListObject { get; set; }
    }
}
