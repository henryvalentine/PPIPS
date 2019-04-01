
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class RecertificationObject
    {
        public long Id { get; set; }
        public string ReferenceCode { get; set; }
        public long NotificationId { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<System.DateTime> DateApplied { get; set; }
        public Nullable<System.DateTime> LastModified { get; set; }

        public virtual NotificationObject NotificationObject { get; set; }
    }
}
