//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ImportPermitPortal.EF.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class NotificationBanker
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
    
        public virtual Bank Bank { get; set; }
        public virtual Document Document { get; set; }
        public virtual Notification Notification { get; set; }
        public virtual Product Product { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}
