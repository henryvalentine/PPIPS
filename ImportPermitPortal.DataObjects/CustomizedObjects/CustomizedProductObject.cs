using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public partial class ProductObject
    {
        public string AvailableStr { get; set; }
        public string Requirements { get; set; }
        public string PrPSF { get; set; }
        public bool RequiresFormM { get; set; }
        public long QuantityImported { get; set; }
        public bool RequiresPsf { get; set; }
        public bool RequireReferenceCode { get; set; }
        public bool ReferenceCode { get; set; }
        public string ReferenceLicenseType { get; set; }
        public long FormMId { get; set; }
        public int DishargeDepotId { get; set; }
    }
}

                           
                           
                               