

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class AspNetRoleObject
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<DocumentTypeRightObject> DocumentTypeRightObjects { get; set; }
        public virtual ICollection<AspNetUserObject> AspNetUserObjects { get; set; }
    }
}
