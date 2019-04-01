
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class DocumentIssueObject
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public string Description { get; set; }
        public System.DateTime IssueDate { get; set; }
        public int IssueTypeId { get; set; }
        public long FiledById { get; set; }
        public int Status { get; set; }

        public virtual DocumentObject DocumentObject { get; set; }
        public virtual IssueTypeObject IssueTypeObject { get; set; }
        public virtual UserProfileObject UserProfileObject { get; set; }
    }
}
