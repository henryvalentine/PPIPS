
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class BankJobHistoryObject
    {
        public long BankJobHistoryId { get; set; }
        public long CompanyId { get; set; }
        public long ApplicationId { get; set; }
        public long DocumentId { get; set; }

        public virtual ApplicationObject ApplicationObject { get; set; }
        public virtual ImporterObject ImporterObject { get; set; }
        public virtual DocumentObject DocumentObject { get; set; }
    }
}
