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
    
    public partial class UserLink
    {
        public long Id { get; set; }
        public long UserProfileId { get; set; }
        public int LinkSequence { get; set; }
        public int LinkId { get; set; }
    
        public virtual Link Link { get; set; }
    }
}
