
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class FeeObject
    {
        public int FeeId { get; set; }
        public int FeeTypeId { get; set; }
        public int ImportStageId { get; set; } 
        public double Amount { get; set; }
        public string Name { get; set; }
        public string CurrencyCode { get; set; }
        public double PrincipalSplit { get; set; }
        public double VendorSplit { get; set; }
        public double PaymentGatewaySplit { get; set; }
        public bool BillableToPrincipal { get; set; }
        
        public virtual FeeTypeObject FeeTypeObject { get; set; }
        public virtual ImportStageObject ImportStageObject { get; set; }
        public virtual ICollection<InvoiceItemObject> InvoiceItemObjects { get; set; }
    }
}
