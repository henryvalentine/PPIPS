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
    
    public partial class StorageProviderType
    {
        public StorageProviderType()
        {
            this.ApplicationItems = new HashSet<ApplicationItem>();
            this.StorageProviderRequirements = new HashSet<StorageProviderRequirement>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<ApplicationItem> ApplicationItems { get; set; }
        public virtual ICollection<StorageProviderRequirement> StorageProviderRequirements { get; set; }
    }
}
