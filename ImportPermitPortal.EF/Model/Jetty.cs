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
    
    public partial class Jetty
    {
        public Jetty()
        {
            this.Depots = new HashSet<Depot>();
            this.JettyMappings = new HashSet<JettyMapping>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public int PortId { get; set; }
    
        public virtual ICollection<Depot> Depots { get; set; }
        public virtual Port Port { get; set; }
        public virtual ICollection<JettyMapping> JettyMappings { get; set; }
    }
}
