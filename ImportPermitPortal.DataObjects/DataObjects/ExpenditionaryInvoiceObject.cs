
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ExpenditionaryInvoiceObject
    {
        public long Id { get; set; }
        public long NotificationId { get; set; }
        public long InvoiceId { get; set; }

        public virtual InvoiceObject InvoiceObject { get; set; }
        public virtual NotificationObject NotificationObject { get; set; }
    }
}
