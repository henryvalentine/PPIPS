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
    
    public partial class UnitOfMeasurement
    {
        public UnitOfMeasurement()
        {
            this.StorageTanks = new HashSet<StorageTank>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<StorageTank> StorageTanks { get; set; }
    }
}
