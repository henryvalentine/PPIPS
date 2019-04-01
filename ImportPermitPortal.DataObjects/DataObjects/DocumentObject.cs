
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class DocumentObject
    {
        public long DocumentId { get; set; }
        public int DocumentTypeId { get; set; }
        public long ImporterId { get; set; }
        public System.DateTime DateUploaded { get; set; }
        public int Status { get; set; }
        public long UploadedById { get; set; }
        public string DocumentPath { get; set; }
        public string IpAddress { get; set; }
        public Nullable<bool> IsValid { get; set; }

        public virtual ICollection<NotificationBankerObject> NotificationBankerObjects { get; set; }
        public virtual ICollection<ApplicationDocumentObject> ApplicationDocumentObjects { get; set; }
        public virtual ICollection<BankJobHistoryObject> BankJobHistoryObjects { get; set; }
        public virtual ICollection<CompanyDocumentObject> CompanyDocumentObjects { get; set; }
        public virtual DocumentTypeObject DocumentTypeObject { get; set; }
        public virtual ImporterObject ImporterObject { get; set; }
        public virtual ICollection<FormMDetailObject> FormMDetailObjects { get; set; }
        public virtual ICollection<NotificationDocumentObject> NotificationDocumentObjects { get; set; }
        public virtual ICollection<VesselArrivalDocumentObject> VesselArrivalDocumentObjects { get; set; }
    }
}
