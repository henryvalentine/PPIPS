
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ProductsLicenseRequirementObject
    {
        public int Id { get; set; }
        public long ProductId { get; set; }
        public long ReferenceLicenseId { get; set; }

        public virtual ProductObject ProductObject { get; set; }
        public virtual ReferenceLicenseObject ReferenceLicenseObject { get; set; }
    }
}
