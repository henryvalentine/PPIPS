
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class NotificationDocumentObject
    {
        public long Id { get; set; }
        public long NotificationId { get; set; }
        public long DocumentId { get; set; }
        public string Comment { get; set; }
        public System.DateTime DateAttached { get; set; }

        public virtual DocumentObject DocumentObject { get; set; }
        public virtual NotificationObject NotificationObject { get; set; }
    }
}
