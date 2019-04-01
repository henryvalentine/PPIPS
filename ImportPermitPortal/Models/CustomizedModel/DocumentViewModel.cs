
using ImportPermitPortal.DataObjects;

namespace ImportPermitPortal.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DocumentViewModel
    {
        public long DocumentId { get; set; }
        public int DocumentTypeId { get; set; }
        public long Id { get; set; }
        public System.DateTime DateUploaded { get; set; }
        public int Status { get; set; }
        public Nullable<long> UploadedById { get; set; }
        public string DocumentPath { get; set; }

        public string DateUploadedStr { get; set; }
        public string DocumentTypeName { get; set; }
        public string StatusStr { get; set; }
        public string CompanyName { get; set; }
        public string AppReference { get; set; }
        public long ApplicationId { get; set; }
        public virtual ImporterObject Company { get; set; }
        public virtual DocumentTypeObject DocumentType { get; set; }
        public virtual ICollection<VesselArrivalDocumentObject> VesselArrivalDocuments { get; set; }
    }
}

