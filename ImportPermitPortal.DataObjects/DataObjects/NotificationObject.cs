
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class NotificationObject
    {
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

        public virtual DepotObject DepotObject { get; set; }
        public virtual ICollection<DischargeParameterAfterObject> DischargeParameterAfterObjects { get; set; }
        public virtual ICollection<DischargeParameterBeforeObject> DischargeParameterBeforeObjects { get; set; }
        public virtual ICollection<ExpenditionaryInvoiceObject> ExpenditionaryInvoiceObjects { get; set; }
        public virtual ICollection<FormMDetailObject> FormMDetailObjects { get; set; }
        public virtual ImportClassObject ImportClassObject { get; set; }
        public virtual ImporterObject ImporterObject { get; set; }
        public virtual InvoiceObject InvoiceObject { get; set; }
        public virtual ICollection<NotificationBankerObject> NotificationBankerObjects { get; set; }
        public virtual ICollection<RecertificationResultObject> RecertificationResultObjects { get; set; }
        public virtual PermitObject PermitObject { get; set; }
        public virtual PortObject PortObject { get; set; }
        public virtual ProductObject ProductObject { get; set; }
        public virtual ICollection<NotificationCheckListOutcomeObject> NotificationCheckListOutcomeObjects { get; set; }
        public virtual ICollection<NotificationDischageDataObject> NotificationDischageDataObjects { get; set; }
        public virtual ICollection<NotificationDocumentObject> NotificationDocumentObjects { get; set; }
        public virtual ICollection<NotificationHistoryObject> NotificationHistoryObjects { get; set; }
        public virtual ICollection<NotificationInspectionObject> NotificationInspectionObjects { get; set; }
        public virtual ICollection<NotificationInspectionQueueObject> NotificationInspectionQueueObjects { get; set; }
        public virtual ICollection<NotificationIssueObject> NotificationIssueObjects { get; set; }
        public virtual ICollection<NotificationVesselObject> NotificationVesselObjects { get; set; }
        public virtual ICollection<RecertificationObject> RecertificationObjects { get; set; }
    }
}
