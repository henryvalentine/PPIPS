
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class StepObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProcessId { get; set; }
        public int? SequenceNumber { get; set; }
        public int GroupId { get; set; }
        public int? StatusId { get; set; }
        public int ActivityTypeId { get; set; }
        public string Instructions { get; set; }
        public int? ExpectedDeliveryDuration { get; set; }
        public Nullable<bool> IsLocationRequired { get; set; }
    
       

        public virtual GroupObject Group { get; set; }
        public virtual ICollection<NotificationHistoryObject> NotificationHistoryObjects { get; set; }
        public virtual ICollection<NotificationInspectionQueueObject> NotificationInspectionQueueObjects { get; set; }
        public virtual ICollection<NotificationTrackObject> NotificationTrackObjects { get; set; }
        public virtual ProcessObject ProcessObject { get; set; }
        public virtual ICollection<ProcessingHistoryObject> ProcessingHistoryObjects { get; set; }
        public virtual ICollection<ProcessTrackingObject> ProcessTrackingObjects { get; set; }
        public virtual StepActivityTypeObject StepActivityTypeObject { get; set; }
    }
}
