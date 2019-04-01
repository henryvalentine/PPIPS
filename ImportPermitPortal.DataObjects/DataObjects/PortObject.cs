
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class PortObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }

        public virtual CountryObject CountryObject { get; set; }
        public virtual ICollection<JettyObject> JettyObjects { get; set; }
        public virtual ICollection<NotificationObject> NotificationObjects { get; set; }
    }
}
