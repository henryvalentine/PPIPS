
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class DepotOwnerObject
    {

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<DepotObject> DepotObjects { get; set; }
    }
}
