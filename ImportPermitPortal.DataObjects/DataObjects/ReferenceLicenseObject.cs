
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class ReferenceLicenseObject
    {
        public long Id { get; set; }
        public int ReferenceLicenseTypeId { get; set; }
        public string LicenceCode { get; set; }
        public System.DateTime IssueDate { get; set; }
        public System.DateTime ExpiryDate { get; set; }
        public int Status { get; set; }

        public virtual ICollection<ApplicationLicenseMappingObject> ApplicationLicenseMappingObjects { get; set; }
        public virtual ICollection<ProductsLicenseRequirementObject> ProductsLicenseRequirementObjects { get; set; }
        public virtual ReferenceLicenseTypeObject ReferenceLicenseTypeObject { get; set; }
    }
}
