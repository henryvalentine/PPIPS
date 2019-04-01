using System.Collections.Generic;
namespace ImportPermitPortal.EF.Model
{
    public partial class Permit
    {
        public string ReferenceCode { get; set; }
        public string IssueDateStr { get; set; }
        public string ExpiryDateStr { get; set; }
    }
}
