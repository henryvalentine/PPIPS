
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class IssueTypeObject
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ApplicationIssueObject> ApplicationIssueObjects { get; set; }
    }
}
