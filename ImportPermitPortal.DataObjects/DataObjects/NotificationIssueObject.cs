
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class NotificationIssueObject
    {
        public long Id { get; set; }
        public int IssueTypeId { get; set; }
        public long NotificationId { get; set; }
        public string Description { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<System.DateTime> IssueDate { get; set; }

        public virtual IssueTypeObject IssueTypeObject { get; set; }
        public virtual NotificationObject NotificationObject { get; set; }
    }
}
