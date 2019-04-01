//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ImportPermitPortal.EF.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class DocumentType
    {
        public DocumentType()
        {
            this.Documents = new HashSet<Document>();
            this.DocumentTypeRights = new HashSet<DocumentTypeRight>();
            this.ImportClassificationRequirements = new HashSet<ImportClassificationRequirement>();
            this.ImportRequirements = new HashSet<ImportRequirement>();
            this.NotificationCheckLists = new HashSet<NotificationCheckList>();
            this.ProductDocumentRequirements = new HashSet<ProductDocumentRequirement>();
            this.StorageProviderRequirements = new HashSet<StorageProviderRequirement>();
        }
    
        public int DocumentTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<DocumentTypeRight> DocumentTypeRights { get; set; }
        public virtual ICollection<ImportClassificationRequirement> ImportClassificationRequirements { get; set; }
        public virtual ICollection<ImportRequirement> ImportRequirements { get; set; }
        public virtual ICollection<NotificationCheckList> NotificationCheckLists { get; set; }
        public virtual ICollection<ProductDocumentRequirement> ProductDocumentRequirements { get; set; }
        public virtual ICollection<StorageProviderRequirement> StorageProviderRequirements { get; set; }
    }
}
