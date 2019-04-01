
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ThroughPutObject
    {
        public string ReferenceCode { get; set; }
        public string DepotName { get; set; }
        public int Status { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string StatusStr { get; set; }
        public string ThName { get; set; }
        public string TempPath { get; set; }
        public long DocumentTypeName { get; set; }
        public string DocumentPath { get; set; }
    }
}


