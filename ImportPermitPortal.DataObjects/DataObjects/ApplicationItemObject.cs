
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ApplicationItemObject
    {
        public long Id { get; set; }
        public long ApplicationId { get; set; }
        public long ProductId { get; set; }
        public long EstimatedQuantity { get; set; }
        public double EstimatedValue { get; set; }
        public string PSFNumber { get; set; }
        public string ReferenceLicenseCode { get; set; }
        public double TotalImportedQuantity { get; set; }
        public double ImportedQuantityValue { get; set; }
        public double OutstandingQuantity { get; set; }
        public Nullable<System.DateTime> DateImported { get; set; }
        public int StorageProviderTypeId { get; set; }

        public virtual ApplicationObject ApplicationObject { get; set; }
        public virtual ICollection<ApplicationCountryObject> ApplicationCountryObjects { get; set; }
        public virtual ProductObject ProductObject { get; set; }
        public virtual StorageProviderTypeObject StorageProviderTypeObject { get; set; }
        public virtual ICollection<ProductBankerObject> ProductBankerObjects { get; set; }
        public virtual ICollection<ThroughPutObject> ThroughPutObjects { get; set; }
    }
}



