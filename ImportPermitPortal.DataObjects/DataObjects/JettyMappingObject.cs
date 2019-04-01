
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class JettyMappingObject
    {
        public int Id { get; set; }
        public int JettyId { get; set; }
        public int ZoneId { get; set; }

        public virtual JettyObject JettyObject { get; set; }
        public virtual ZoneObject ZoneObject { get; set; }
    }
}
