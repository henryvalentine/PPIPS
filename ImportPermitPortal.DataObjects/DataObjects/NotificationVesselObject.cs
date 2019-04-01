
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class NotificationVesselObject
    {
        public long NotificationVesselId { get; set; }
        public long VesselId { get; set; }
        public long NotificationId { get; set; }
        public int VesselClassTypeId { get; set; }
        public DateTime? DateAdded { get; set; }

        public virtual NotificationObject NotificationObject { get; set; }
        public virtual VesselObject VesselObject { get; set; }
    }
}
