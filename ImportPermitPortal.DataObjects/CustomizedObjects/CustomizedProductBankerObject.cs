

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ProductBankerObject
    {
        public string DocumentStatus { get; set; }
        public string DocumentPath { get; set; }
        public string BankName { get; set; }
        public string BankName2 { get; set; }
        public string ProductCode { get; set; }
        public long ApplicationId { get; set; }
        public long ImporterId { get; set; }
        public bool IsUploaded { get; set; }
    }


}

