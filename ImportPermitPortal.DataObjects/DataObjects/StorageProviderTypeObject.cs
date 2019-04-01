
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class StorageProviderTypeObject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ApplicationObject> ApplicationObjects { get; set; }
        public virtual ICollection<StorageProviderRequirementObject> StorageProviderRequirementObjects { get; set; }
        public virtual ICollection<ApplicationItemObject> ApplicationItemObjects { get; set; }
    }
}
