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
    
    public partial class ProductBanker
    {
        public long Id { get; set; }
        public long ApplicationItemId { get; set; }
        public int BankId { get; set; }
        public Nullable<long> DocumentId { get; set; }
        public string BankAccountNumber { get; set; }
    
        public virtual ApplicationItem ApplicationItem { get; set; }
        public virtual Bank Bank { get; set; }
        public virtual Document Document { get; set; }
    }
}
