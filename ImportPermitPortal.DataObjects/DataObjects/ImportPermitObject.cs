
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ImportPermitObject
    {
        public long ImportPermitId { get; set; }
        public long ApplicationId { get; set; }
        public string PermitNo { get; set; }
        public int Status { get; set; }
        public System.DateTime DateIssued { get; set; }
        public System.DateTime ExpiryDate { get; set; }
        public long QuantityImported { get; set; }

        public virtual ApplicationObject ApplicationObject { get; set; }
    }
}
