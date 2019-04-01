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
    
    public partial class ImportStage
    {
        public ImportStage()
        {
            this.Fees = new HashSet<Fee>();
            this.ImportClassificationRequirements = new HashSet<ImportClassificationRequirement>();
            this.ImportRequirements = new HashSet<ImportRequirement>();
            this.Processes = new HashSet<Process>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<Fee> Fees { get; set; }
        public virtual ICollection<ImportClassificationRequirement> ImportClassificationRequirements { get; set; }
        public virtual ICollection<ImportRequirement> ImportRequirements { get; set; }
        public virtual ICollection<Process> Processes { get; set; }
    }
}