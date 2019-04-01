
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class PaymentDistributionSummaryObject
    {
        public long Id { get; set; }
        public int ServiceId { get; set; }
        public string Beneficiary { get; set; }
        public double Amount { get; set; }
        public string PaymentReference { get; set; }
        public System.DateTime PaymentDate { get; set; }
        public bool Status { get; set; }
    }
}

