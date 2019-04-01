
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class AddressObject
    {
        public long AddressId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string LastUpdated { get; set; }
        public long CityId { get; set; }

        public virtual CityObject CityObject { get; set; }
        public virtual ICollection<ImporterAddressObject> ImporterAddressObjects { get; set; }
    }
}
