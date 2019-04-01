//------------------------------------------------------------------------------

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ProductBankerObject
    {
        public long Id { get; set; }
        public long ApplicationItemId { get; set; }
        public int BankId { get; set; }
        public long? DocumentId { get; set; }
        public string BankAccountNumber { get; set; }


        public virtual ApplicationItemObject ApplicationItemObject { get; set; }
        public virtual BankObject BankObject { get; set; }
        public virtual DocumentObject DocumentObject { get; set; }
    }
}
