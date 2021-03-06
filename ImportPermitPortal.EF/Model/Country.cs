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
    
    public partial class Country
    {
        public Country()
        {
            this.ApplicationCountries = new HashSet<ApplicationCountry>();
            this.Cities = new HashSet<City>();
            this.Ports = new HashSet<Port>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string CountryCode { get; set; }
        public int RegionId { get; set; }
    
        public virtual ICollection<ApplicationCountry> ApplicationCountries { get; set; }
        public virtual ICollection<City> Cities { get; set; }
        public virtual Region Region { get; set; }
        public virtual ICollection<Port> Ports { get; set; }
    }
}
