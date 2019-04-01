
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class IssueLogObject
    {
        public long Id { get; set; }
        public int IssueCategoryId { get; set; }
        public string Issue { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<System.DateTime> DateResolved { get; set; }

        public virtual ICollection<IssueObject> IssueObjects { get; set; }
        public virtual IssueCategoryObject IssueCategoryObject { get; set; }
    }
}

