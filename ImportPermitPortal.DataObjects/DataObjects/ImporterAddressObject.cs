
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ImporterAddressObject
    {
        public long ImporterAddressId { get; set; }
        public int AddressTypeId { get; set; }
        public long AddressId { get; set; }
        public long ImporterId { get; set; }
        public string LastUpdated { get; set; }
        public Nullable<bool> IsRegisteredSameAsOperational { get; set; }

        public virtual AddressObject AddressObject { get; set; }
        public virtual AddressTypeObject AddressTypeObject { get; set; }
        public virtual ImporterObject ImporterObject { get; set; }
    }
}
