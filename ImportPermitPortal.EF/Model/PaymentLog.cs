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
    
    public partial class PaymentLog
    {
        public PaymentLog()
        {
            this.UnclearedPayments = new HashSet<UnclearedPayment>();
        }
    
        public long Id { get; set; }
        public Nullable<bool> Type { get; set; }
        public string PaymentReference { get; set; }
        public double PaymentAmount { get; set; }
        public string PaymentDate { get; set; }
        public string PaymentCurrency { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentChannel { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string DepositSlipNumber { get; set; }
        public string BankName { get; set; }
        public string Location { get; set; }
        public string CustomerName { get; set; }
        public string PaymentStatus { get; set; }
        public string XmlData { get; set; }
        public System.DateTime ExpiryDate { get; set; }
    
        public virtual ICollection<UnclearedPayment> UnclearedPayments { get; set; }
    }
}
