
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class ApplicationObject
    {
        public long Id { get; set; }
        public long ImporterId { get; set; }
        public long DerivedTotalQUantity { get; set; }
        public double DerivedValue { get; set; }
        public int ApplicationStatusCode { get; set; }
        public System.DateTime DateApplied { get; set; }
        public System.DateTime LastModified { get; set; }
        public int ClassificationId { get; set; }
        public long InvoiceId { get; set; }
        public string PreviousPermitNo { get; set; }
        public int ApplicationTypeId { get; set; }

        public virtual ApplicationTypeObject ApplicationTypeObject { get; set; }
        public virtual BankObject BankObject { get; set; }
        public virtual ImporterObject ImporterObject { get; set; }
        public virtual InvoiceObject InvoiceObject { get; set; }
        public virtual ICollection<ApplicationDocumentObject> ApplicationDocumentObjects { get; set; }
        public virtual ICollection<ApplicationIssueObject> ApplicationIssueObjects { get; set; }
        public virtual ICollection<ApplicationItemObject> ApplicationItemObjects { get; set; }
        public virtual ICollection<ApplicationLicenseMappingObject> ApplicationLicenseMappingObjects { get; set; }
        public virtual ICollection<FormMDetailObject> FormMDetailObjects { get; set; }
        public virtual ICollection<ImportApplicationHistoryObject> ImportApplicationHistoryObjects { get; set; }
        public virtual ICollection<PermitApplicationObject> PermitApplicationObjects { get; set; }
        public virtual ICollection<ProcessingHistoryObject> ProcessingHistoryObjects { get; set; }
        public virtual ICollection<ProcessTrackingObject> ProcessTrackingObjects { get; set; }
    }
}
