
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class NotificationBankerObject
    {
        public long Id { get; set; }
        public int BankId { get; set; }
        public System.DateTime DateAdded { get; set; }
        public double FinancedQuantity { get; set; }
        public double TransactionAmount { get; set; }
        public double ActualQuantity { get; set; }
        public long AttachedDocumentId { get; set; }
        public long ProductId { get; set; }
        public long LastUpdateBy { get; set; }
        public Nullable<long> ApprovedBy { get; set; }
        public string IpAddress { get; set; }
        public long NotificationId { get; set; }

        public virtual BankObject BankObject { get; set; }
        public virtual DocumentObject DocumentObject { get; set; }
        public virtual NotificationObject NotificationObject { get; set; }
        public virtual ProductObject ProductObject { get; set; }
        public virtual UserProfileObject UserProfileObject { get; set; }

    }
}


