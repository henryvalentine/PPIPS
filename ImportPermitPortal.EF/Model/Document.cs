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
    
    public partial class Document
    {
        public Document()
        {
            this.ApplicationDocuments = new HashSet<ApplicationDocument>();
            this.CompanyDocuments = new HashSet<CompanyDocument>();
            this.NotificationBankers = new HashSet<NotificationBanker>();
            this.DocumentIssues = new HashSet<DocumentIssue>();
            this.FormMDetails = new HashSet<FormMDetail>();
            this.NotificationDocuments = new HashSet<NotificationDocument>();
            this.ProductBankers = new HashSet<ProductBanker>();
            this.ThroughPuts = new HashSet<ThroughPut>();
            this.VesselArrivalDocuments = new HashSet<VesselArrivalDocument>();
        }
    
        public long DocumentId { get; set; }
        public int DocumentTypeId { get; set; }
        public long ImporterId { get; set; }
        public System.DateTime DateUploaded { get; set; }
        public int Status { get; set; }
        public long UploadedById { get; set; }
        public string DocumentPath { get; set; }
        public string IpAddress { get; set; }
        public string IsValid { get; set; }
    
        public virtual ICollection<ApplicationDocument> ApplicationDocuments { get; set; }
        public virtual ICollection<CompanyDocument> CompanyDocuments { get; set; }
        public virtual ICollection<NotificationBanker> NotificationBankers { get; set; }
        public virtual DocumentType DocumentType { get; set; }
        public virtual Importer Importer { get; set; }
        public virtual ICollection<DocumentIssue> DocumentIssues { get; set; }
        public virtual ICollection<FormMDetail> FormMDetails { get; set; }
        public virtual ICollection<NotificationDocument> NotificationDocuments { get; set; }
        public virtual ICollection<ProductBanker> ProductBankers { get; set; }
        public virtual ICollection<ThroughPut> ThroughPuts { get; set; }
        public virtual ICollection<VesselArrivalDocument> VesselArrivalDocuments { get; set; }
    }
}
