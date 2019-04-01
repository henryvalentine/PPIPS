
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ApplicationDocumentObject
    {
        public long ApplicationDocumentId { get; set; }
        public long ApplicationId { get; set; }
        public long DocumentId { get; set; }

        public virtual ApplicationObject ApplicationObject { get; set; }
        public virtual DocumentObject DocumentObject { get; set; }
    }
}
