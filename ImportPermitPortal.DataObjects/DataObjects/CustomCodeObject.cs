
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class CustomCodeObject
    {
        public int CustomCodeId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ProductColumnObject> ProductColumnObjects { get; set; }
    }
}
