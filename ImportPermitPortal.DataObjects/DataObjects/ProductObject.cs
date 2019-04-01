
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ProductObject
    {
        public long ProductId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Availability { get; set; }

        public virtual ICollection<NotificationBankerObject> NotificationBankerObjects { get; set; }
        public virtual ICollection<ApplicationItemObject> ApplicationItemObjects { get; set; }
        public virtual ICollection<FormMDetailObject> FormMDetailObjects { get; set; }
        public virtual ICollection<NotificationObject> NotificationObjects { get; set; }
        public virtual ICollection<NotificationInspectionObject> NotificationInspectionObjects { get; set; }
        public virtual ICollection<ProductColumnObject> ProductColumnObjects { get; set; }
        public virtual ICollection<ProductDocumentRequirementObject> ProductDocumentRequirementObjects { get; set; }
        public virtual ICollection<ProductsLicenseRequirementObject> ProductsLicenseRequirementObjects { get; set; }
        public virtual ICollection<StorageTankObject> StorageTankObjects { get; set; }
        public virtual ICollection<VesselArrivalNoticeObject> VesselArrivalNoticeObjects { get; set; }
    }
}
