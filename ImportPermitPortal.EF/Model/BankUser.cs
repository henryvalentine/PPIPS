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
    
    public partial class BankUser
    {
        public long Id { get; set; }
        public int BranchId { get; set; }
        public long UserId { get; set; }
    
        public virtual BankBranch BankBranch { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}
