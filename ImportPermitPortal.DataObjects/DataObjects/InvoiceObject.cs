
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class InvoiceObject
    {
        public long Id { get; set; }
        public string ReferenceCode { get; set; }
        public int PaymentTypeId { get; set; }
        public double TotalAmountDue { get; set; }
        public double AmountPaid { get; set; }
        public int Status { get; set; }
        public string IPAddress { get; set; }
        public DateTime DateAdded { get; set; }
        public long ImporterId { get; set; }
        public string RRR { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? ServiceDescriptionId { get; set; }

        public virtual ICollection<ApplicationObject> ApplicationObjects { get; set; }
        public virtual ImporterObject ImporterObject { get; set; }
        public virtual ICollection<InvoiceItemObject> InvoiceItemObjects { get; set; }
        public virtual ICollection<NotificationObject> NotificationObjects { get; set; }
        public virtual ICollection<TransactionObject> TransactionObjects { get; set; }
    }

}

