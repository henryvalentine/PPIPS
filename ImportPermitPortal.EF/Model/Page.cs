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
    
    public partial class Page
    {
        public int Id { get; set; }
        public string Alias { get; set; }
        public string Title { get; set; }
        public string PageContent { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public long LastedUpdatedById { get; set; }
    
        public virtual UserProfile UserProfile { get; set; }
    }
}