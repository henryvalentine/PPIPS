
using System.Collections.Generic;

namespace ImportPermitPortal.DataObjects
{
    
    public partial class InvoiceItemObject
    {

        public long Id { get; set; }
        public long InvoiceId { get; set; }
        public int FeeId { get; set; }
        public double AmountDue { get; set; }

        public virtual FeeObject FeeObject { get; set; }
        public virtual InvoiceObject InvoiceObject { get; set; }
    }
}
