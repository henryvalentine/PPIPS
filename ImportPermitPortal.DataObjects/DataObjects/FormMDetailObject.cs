
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class FormMDetailObject
    {
        public long Id { get; set; }
        public long NotificationId { get; set; }
        public System.DateTime DateIssued { get; set; }
        public string FormMReference { get; set; }
        public double Quantity { get; set; }
        public string LetterOfCreditNo { get; set; }
        public long AttachedDocumentId { get; set; }
        public System.DateTime DateAttached { get; set; }
        public int BankId { get; set; }

        public virtual BankObject BankObject { get; set; }
        public virtual DocumentObject DocumentObject { get; set; }
        public virtual NotificationObject NotificationObject { get; set; }
    }
}



