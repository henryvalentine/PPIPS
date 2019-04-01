

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class FeeObject
    {
        public string FeeTypeName { get; set; }
        public int ErrorCode { get; set; }
        public string AmountStr { get; set; }
        public string ImportStageName { get; set; }
        public string Error { get; set; }
        public int VesselDischargeLeadTime { get; set; }
    }
}

