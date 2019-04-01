

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class PaymentReceiptObject
    {
        public string DateAddedStr { get; set; }
        public string StatusStr { get; set; }
        public string ReceiptNoStr { get; set; }
        public string TotalAmountDueStr { get; set; }
        public string TotalAmountPaidStr { get; set; }
        public double TotalAmountDue { get; set; }
        public string ImporterName { get; set; }
        public string PaymentOption { get; set; }
        public string ServiceDescription { get; set; }
        public string AmountInWords { get; set; }
        public string Number { get; set; }
        public string RRR { get; set; }
    }
}

