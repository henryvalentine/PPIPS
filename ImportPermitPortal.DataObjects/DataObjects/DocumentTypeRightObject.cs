
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class DocumentTypeRightObject
    {
        public long DocumentTypeRightId { get; set; }
        public string Permission { get; set; }
        public string RoleId { get; set; }
        public int DocumentTypeId { get; set; }

        public virtual AspNetRoleObject AspNetRoleObject { get; set; }
        public virtual DocumentTypeObject DocumentTypeObject { get; set; }
    }
}
