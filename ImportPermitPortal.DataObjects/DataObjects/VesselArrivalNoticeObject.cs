
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class VesselArrivalNoticeObject
    {
        public long VesselArrivalNoticeId { get; set; }
        public string ShipmentIdentitificationNo { get; set; }
        public long ProductId { get; set; }
        public long Quantity { get; set; }
        public System.DateTime ArrivalDate { get; set; }
        public System.DateTime DishargeDate { get; set; }
        public long MotherVesselId { get; set; }
        public long? ShuttleVesselId { get; set; }

        public virtual ProductObject ProductObject { get; set; }
        public virtual VesselObject VesselObject { get; set; }
        public virtual ICollection<VesselArrivalDocumentObject> VesselArrivalDocumentObjects { get; set; }
    }
}
