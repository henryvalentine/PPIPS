

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class PermitObject
    {
        public ApplicationObject ApplicationObject { get; set; }

        public string PermitStatusStr { get; set; }
        public string IssueDateStr { get; set; }
        public string ExpiryDateStr { get; set; }
        public string CompanyName { get; set; }

        public string QuantityStr { get; set; }


        public bool IsExpired { get; set; }
        public bool IsValid { get; set; }
        public bool IsNull { get; set; }

        public bool IsError { get; set; }
        public bool NoEmployee { get; set; }
        

    }
}

