
using System;

namespace ImportPermitPortal.DataObjects
{
    public partial class TransactionInvoiceObject
    {
        public long Id { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentGateway { get; set; }
        public double TotalAmountDue { get; set; }
        public double AmountPaid { get; set; }
        public string CurrencyCode { get; set; }
        public int Status { get; set; }
        public string ServiceDescription { get; set; }
        public bool? IsExpired { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? BookDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public long CustomerId { get; set; }
        public string RRR { get; set; }
    }
}
