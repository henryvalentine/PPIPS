
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class UnclearedPaymentObject
    {
        public long Id { get; set; }
        public long? PaymentLogId { get; set; }
        public string ReferenceCode { get; set; }
        public double Amount { get; set; }
        public string CompanyId { get; set; }
        public bool? IsAssigned { get; set; }
        public string ReferenceNoAssignedTo { get; set; }
        public System.DateTime DateAssigned { get; set; }
    

        public virtual PaymentLogObject PaymentLogObject { get; set; }
    }
}
