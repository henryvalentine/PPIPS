
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class VesselObject
    {
        public long VesselId { get; set; } 
        public string Name { get; set; }
        public Nullable<double> Capacity { get; set; }
        public System.DateTime DateAdded { get; set; }
        public string VesselLicense { get; set; }
        public Nullable<System.DateTime> IssueDate { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<bool> Status { get; set; }
        public string CompanyName { get; set; }

        public virtual ICollection<NotificationDischageDataObject> NotificationDischageDataObjects { get; set; }
        public virtual ICollection<NotificationVesselObject> NotificationVesselObjects { get; set; }
        public virtual ICollection<VesselArrivalNoticeObject> VesselArrivalNoticeObjects { get; set; }
    }
}


