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
    
    public partial class Bank
    {
        public Bank()
        {
            this.NotificationBankers = new HashSet<NotificationBanker>();
            this.BankBranches = new HashSet<BankBranch>();
            this.FormMDetails = new HashSet<FormMDetail>();
            this.ProductBankers = new HashSet<ProductBanker>();
        }
    
        public int BankId { get; set; }
        public string SortCode { get; set; }
        public long ImporterId { get; set; }
        public string Name { get; set; }
        public string NotificationEmail { get; set; }
    
        public virtual ICollection<NotificationBanker> NotificationBankers { get; set; }
        public virtual Importer Importer { get; set; }
        public virtual ICollection<BankBranch> BankBranches { get; set; }
        public virtual ICollection<FormMDetail> FormMDetails { get; set; }
        public virtual ICollection<ProductBanker> ProductBankers { get; set; }
    }
}
