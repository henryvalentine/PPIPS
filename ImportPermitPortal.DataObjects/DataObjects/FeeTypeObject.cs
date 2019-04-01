
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class FeeTypeObject
    {
        public int FeeTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<FeeObject> FeeObjects { get; set; }
    }
}
