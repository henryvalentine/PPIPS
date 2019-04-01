
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class StorageProviderRequirementObject
    {
        public int Id { get; set; }
        public int StorageProviderTypeId { get; set; }
        public int DocumentTypeId { get; set; }

        public virtual DocumentTypeObject DocumentTypeObject { get; set; }
        public virtual StorageProviderTypeObject StorageProviderTypeObject { get; set; }
    }
}


