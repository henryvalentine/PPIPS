
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ZoneObject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<EmployeeDeskObject> EmployeeDeskObjects { get; set; }
        public virtual ICollection<JettyMappingObject> JettyMappingObjects { get; set; }
    }
}
