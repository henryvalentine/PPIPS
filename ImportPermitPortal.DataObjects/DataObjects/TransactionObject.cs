
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class TransactionObject
    {
        public long Id { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentGateway { get; set; }
        public double TotalAmountDue { get; set; }
        public double AmountPaid { get; set; }
        public string CurrencyCode { get; set; }
        public int Status { get; set; }
        public string ServiceDescription { get; set; }
        public Nullable<bool> IsExpired { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.DateTime> BookDate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime LastModifiedDate { get; set; }
        public long CustomerId { get; set; }
        public string RRR { get; set; }

        public virtual InvoiceObject InvoiceObject { get; set; }
        public virtual ICollection<PaymentDetailObject> PaymentDetailObjects { get; set; }
    }
}
