
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class CompanyDocumentObject
    {
        public int CompanyDocumentId { get; set; }
        public long DocumentId { get; set; }
        public long CompanyId { get; set; }

        public virtual DocumentObject DocumentObject { get; set; }
        public virtual ImporterObject ImporterObject { get; set; }
    }
}
