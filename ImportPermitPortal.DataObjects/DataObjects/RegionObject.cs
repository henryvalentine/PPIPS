
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class RegionObject
    {
        
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<CountryObject> CountryObjects { get; set; }
    }
}
