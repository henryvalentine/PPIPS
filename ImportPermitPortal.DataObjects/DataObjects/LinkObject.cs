
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class LinkObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LinkUrl { get; set; }

        public virtual ICollection<UserLinkObject> UserLinkObjects { get; set; }
    }
}
