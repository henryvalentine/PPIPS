
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class CountryObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CountryCode { get; set; }
        public int RegionId { get; set; }

        public virtual ICollection<CityObject> CityObjects { get; set; }
        public virtual RegionObject RegionObject { get; set; }
        public virtual ICollection<PortObject> PortObjects { get; set; }
    }
}

