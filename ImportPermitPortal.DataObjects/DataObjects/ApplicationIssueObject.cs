
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ApplicationIssueObject
    {
        public int Id { get; set; }
        public int IssueTypeId { get; set; }
        public long ApplicationId { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
        public DateTime? IssueDate { get; set; }

        public virtual ApplicationObject ApplicationObject { get; set; }
        public virtual IssueTypeObject IssueTypeObject { get; set; }
    }
}
