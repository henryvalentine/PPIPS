
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class EmployeeRoleObject
    {
        public int Id { get; set; }
        public long EmployeeId { get; set; }
        public string RoleId { get; set; }
        public DateTime? Created_At { get; set; }
        public DateTime? Updated_At { get; set; }

        public virtual AspNetRoleObject AspNetRoleObject { get; set; }
        public virtual UserProfileObject UserProfileObject { get; set; }
    }
}
