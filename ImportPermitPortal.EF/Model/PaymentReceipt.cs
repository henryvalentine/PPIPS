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
    
    public partial class PaymentReceipt
    {
        public long Id { get; set; }
        public long TransactionInvoiceId { get; set; }
        public long ReceiptNo { get; set; }
        public System.DateTime DateCreated { get; set; }
    
        public virtual TransactionInvoice TransactionInvoice { get; set; }
    }
}
