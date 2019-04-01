
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class PaymentDetailObject
    {
        public long Id { get; set; }
        public long TransactionId { get; set; }
        public string ReceiptNo { get; set; }
        public string BankName { get; set; }
        public string DepositSlipNo { get; set; }
        public string PaymentItemCode { get; set; }
        public string PaymentItemDescription { get; set; }
        public string CustomerName { get; set; }

        public virtual TransactionObject TransactionObject { get; set; }
    }
}
