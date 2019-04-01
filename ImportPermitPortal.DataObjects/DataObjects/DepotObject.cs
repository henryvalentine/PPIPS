
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class DepotObject
    { 
        public int Id { get; set; }
        public string Name { get; set; }  
        public int JettyId { get; set; }
        public Nullable<long> ImporterId { get; set; }
        public string DepotLicense { get; set; }
        public Nullable<System.DateTime> IssueDate { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<bool> Status { get; set; }

        public virtual ImporterObject ImporterObject { get; set; }
        public virtual JettyObject JettyObject { get; set; }
        public virtual ICollection<NotificationObject> NotificationObjects { get; set; }
        public virtual ICollection<NotificationDischageDataObject> NotificationDischageDataObjects { get; set; }
        public virtual ICollection<NotificationInspectionObject> NotificationInspectionObjects { get; set; }
        public virtual ICollection<StorageTankObject> StorageTankObjects { get; set; }
    }
}


