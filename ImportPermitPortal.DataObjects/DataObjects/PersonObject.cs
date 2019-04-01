
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class PersonObject
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long ImporterId { get; set; }
        public bool IsAdmin { get; set; }
        public bool? IsImporter { get; set; }

        public virtual ImporterObject ImporterObject { get; set; }
        public virtual ICollection<UserProfileObject> UserProfileObjects { get; set; }
    }
}
