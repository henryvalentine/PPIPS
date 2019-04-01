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
    
    public partial class Notification
    {
        public Notification()
        {
            this.DischargeParameterAfters = new HashSet<DischargeParameterAfter>();
            this.DischargeParameterBefores = new HashSet<DischargeParameterBefore>();
            this.ExpenditionaryInvoices = new HashSet<ExpenditionaryInvoice>();
            this.FormMDetails = new HashSet<FormMDetail>();
            this.NotificationBankers = new HashSet<NotificationBanker>();
            this.RecertificationResults = new HashSet<RecertificationResult>();
            this.NotificationCheckListOutcomes = new HashSet<NotificationCheckListOutcome>();
            this.NotificationDischageDatas = new HashSet<NotificationDischageData>();
            this.NotificationDocuments = new HashSet<NotificationDocument>();
            this.NotificationHistories = new HashSet<NotificationHistory>();
            this.NotificationInspections = new HashSet<NotificationInspection>();
            this.NotificationInspectionQueues = new HashSet<NotificationInspectionQueue>();
            this.NotificationIssues = new HashSet<NotificationIssue>();
            this.NotificationVessels = new HashSet<NotificationVessel>();
            this.Recertifications = new HashSet<Recertification>();
        }
    
        public long Id { get; set; }
        public int ClassificationId { get; set; }
        public long PermitId { get; set; }
        public int PortOfOriginId { get; set; }
        public int DischargeDepotId { get; set; }
        public long ProductId { get; set; }
        public double QuantityToDischarge { get; set; }
        public double QuantityOnVessel { get; set; }
        public int CargoInformationTypeId { get; set; }
        public System.DateTime ArrivalDate { get; set; }
        public System.DateTime DischargeDate { get; set; }
        public System.DateTime DateCreated { get; set; }
        public long ImporterId { get; set; }
        public int Status { get; set; }
        public long InvoiceId { get; set; }
    
        public virtual Depot Depot { get; set; }
        public virtual ICollection<DischargeParameterAfter> DischargeParameterAfters { get; set; }
        public virtual ICollection<DischargeParameterBefore> DischargeParameterBefores { get; set; }
        public virtual ICollection<ExpenditionaryInvoice> ExpenditionaryInvoices { get; set; }
        public virtual ICollection<FormMDetail> FormMDetails { get; set; }
        public virtual ImportClass ImportClass { get; set; }
        public virtual Importer Importer { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual ICollection<NotificationBanker> NotificationBankers { get; set; }
        public virtual ICollection<RecertificationResult> RecertificationResults { get; set; }
        public virtual Permit Permit { get; set; }
        public virtual Port Port { get; set; }
        public virtual Product Product { get; set; }
        public virtual ICollection<NotificationCheckListOutcome> NotificationCheckListOutcomes { get; set; }
        public virtual ICollection<NotificationDischageData> NotificationDischageDatas { get; set; }
        public virtual ICollection<NotificationDocument> NotificationDocuments { get; set; }
        public virtual ICollection<NotificationHistory> NotificationHistories { get; set; }
        public virtual ICollection<NotificationInspection> NotificationInspections { get; set; }
        public virtual ICollection<NotificationInspectionQueue> NotificationInspectionQueues { get; set; }
        public virtual ICollection<NotificationIssue> NotificationIssues { get; set; }
        public virtual ICollection<NotificationVessel> NotificationVessels { get; set; }
        public virtual ICollection<Recertification> Recertifications { get; set; }
    }
}
