using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportPermitPortal.DataObjects
{
    public partial class InvoiceObject
    {
        public string DateAddedStr { get; set; }
        public string StatusStr { get; set; }
        public string DateAppliedStr { get; set; }
        public string CompanyName { get; set; }
        public long UserId { get; set; }
        public string TotalAmountDueStr { get; set; }
        public string ReceiptNoStr { get; set; }
        public string ImporterName { get; set; }
        public string PaymentOption { get; set; }
        public string ServiceDescription { get; set; }
        public string AmountInWords { get; set; }
        public string Number { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string RemitaResponse { get; set; }

        public List<ApplicationItemObject> ApplicationItemObjects { get; set; }
        public List<FeeObject> FeeObjects { get; set; }
        public ApplicationObject ApplicationObject { get; set; }
        public NotificationObject NotificationObject { get; set; }
        public List<BankObject> Bankers { get; set; }
    }


}