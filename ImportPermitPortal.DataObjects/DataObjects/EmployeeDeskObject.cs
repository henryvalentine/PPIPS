
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class EmployeeDeskObject
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public int GroupId { get; set; }
        public int? JobCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ZoneId { get; set; }
        public int ActivityTypeId { get; set; }
        public bool? IsUserAvailable { get; set; }

        public virtual GroupObject GroupObject { get; set; }
        public virtual StepActivityTypeObject StepActivityTypeObject { get; set; }
        public virtual UserProfileObject UserProfileObject { get; set; }
        public virtual ZoneObject ZoneObject { get; set; }
        public virtual ICollection<ImportRecertificationResultObject> ImportRecertificationResultObjects { get; set; }
        public virtual ICollection<NotificationHistoryObject> NotificationHistoryObjects { get; set; }
        public virtual ICollection<NotificationInspectionQueueObject> NotificationInspectionQueueObjects { get; set; }
        public virtual ICollection<NotificationTrackObject> NotificationTrackObjects { get; set; }
        public virtual ICollection<ProcessingHistoryObject> ProcessingHistoryObjects { get; set; }
        public virtual ICollection<ProcessTrackingObject> ProcessTrackingObjects { get; set; }
    }
}
