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
    
    public partial class Structure
    {
        public Structure()
        {
            this.Importers = new HashSet<Importer>();
        }
    
        public int StructureId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<Importer> Importers { get; set; }
    }
}
