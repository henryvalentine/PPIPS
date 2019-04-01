
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class IssueObject
    {
        public long Id { get; set; }
        public long IssueLogId { get; set; }
        public long AffectedUserId { get; set; }
        public Nullable<long> ResolvedById { get; set; }
        public string Status { get; set; }

        public virtual IssueLogObject IssueLogObject { get; set; }
        public virtual UserProfileObject UserProfileObject { get; set; }
        public virtual UserProfileObject UserProfileObject1 { get; set; }
    }
}



