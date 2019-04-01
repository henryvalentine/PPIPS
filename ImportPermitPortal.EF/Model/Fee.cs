//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ImportPermitPortal.EF.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Fee
    {
        public Fee()
        {
            this.InvoiceItems = new HashSet<InvoiceItem>();
        }
    
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
    
        public virtual FeeType FeeType { get; set; }
        public virtual ImportStage ImportStage { get; set; }
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; }
    }
}
