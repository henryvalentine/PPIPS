

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class FormMDetailObject
    {
        public string Reference { get; set; }
        public string ProductName { get; set; }
        public string DocumentTypeName { get; set; }
        public string DocumentPath { get; set; }
        public string DateAttachedStr { get; set; }
        public string DateIssuedStr { get; set; }
        public string StatusStr { get; set; }
        public ProductObject ProductObject { get; set; }
    }
}



