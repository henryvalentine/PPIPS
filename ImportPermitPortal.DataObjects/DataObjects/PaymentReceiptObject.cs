
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class PaymentReceiptObject
    {
        public long Id { get; set; }
        public long TransactionInvoiceId { get; set; }
        public long ReceiptNo { get; set; }
        public DateTime DateCreated { get; set; }
    
        public virtual TransactionInvoiceObject TransactionInvoiceObject { get; set; }
    }
}
