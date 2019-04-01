
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class JettyObject
    {
    
        public int Id { get; set; }
        public string Name { get; set; }
        public int PortId { get; set; }

        public virtual ICollection<DepotObject> Depoto { get; set; }
        public virtual PortObject PortObject { get; set; }
        public virtual ICollection<JettyMappingObject> JettyMappingObjects { get; set; }
        public virtual ICollection<NotificationObject> NotificationObjects { get; set; }
        public virtual ICollection<NotificationInspectionObject> NotificationInspectionObjects { get; set; }
    }
}
