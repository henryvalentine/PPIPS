
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class PaymentLogObject
    {
        public long Id { get; set; }
        public Nullable<bool> Type { get; set; }
        public string PaymentReference { get; set; }
        public double PaymentAmount { get; set; }
        public string PaymentDate { get; set; }
        public string PaymentCurrency { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentChannel { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string DepositSlipNumber { get; set; }
        public string BankName { get; set; }
        public string Location { get; set; }
        public string CustomerName { get; set; }
        public string PaymentStatus { get; set; }
        public string XmlData { get; set; }
        public System.DateTime ExpiryDate { get; set; }
        public virtual ICollection<UnclearedPaymentObject> UnclearedPaymentObjects { get; set; }
    }
}
