
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class DocumentTypeObject
    {
        public int DocumentTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<DocumentObject> DocumentObjects { get; set; }
        public virtual ICollection<DocumentTypeRightObject> DocumentTypeRightObjects { get; set; }
        public virtual ICollection<ImportClassificationRequirementObject> ImportClassificationRequirementObjects { get; set; }
        public virtual ICollection<ImportRequirementObject> ImportRequirementObjects { get; set; }
        public virtual ICollection<ProductDocumentRequirementObject> ProductDocumentRequirementObjects { get; set; }
        public virtual ICollection<StorageProviderRequirementObject> StorageProviderRequirementObjects { get; set; }
    }
}
