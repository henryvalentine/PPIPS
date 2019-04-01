
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class UnitOfMeasurementObject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<StorageTankObject> StorageTankObjects { get; set; }
    }
}
