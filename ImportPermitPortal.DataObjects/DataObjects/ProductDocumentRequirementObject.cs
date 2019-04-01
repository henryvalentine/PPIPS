
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ProductDocumentRequirementObject
    {
        public int Id { get; set; }
        public long ProductId { get; set; }
        public int DocumentTypeId { get; set; }

        public virtual DocumentTypeObject DocumentTypeObject { get; set; }
        public virtual ProductObject ProductObject { get; set; }
    }
}
