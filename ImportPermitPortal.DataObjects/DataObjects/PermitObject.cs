
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class PermitObject
    {

        public long Id { get; set; }
        public int PermitNo { get; set; } 
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string file { get; set; }
        public string PermitValue { get; set; }
        public int PermitStatus { get; set; }
        public double QuantityImported { get; set; }
        public System.DateTime DateAdded { get; set; }
        public long ImporterId { get; set; }

        public virtual ImporterObject ImporterObject { get; set; }
        public virtual ICollection<NotificationObject> NotificationObjects { get; set; }
        public virtual ICollection<PermitApplicationObject> PermitApplicationObjects { get; set; }
    }
}

