
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class NotificationDischageDataObject
    {
        public long Id { get; set; }
        public long NotificationId { get; set; }
        public long TankId { get; set; }
        public long VesselId { get; set; }
        public int JettyId { get; set; }
        public int DischargeParameterBeforeId { get; set; }
        public int DischargeParameterAfterId { get; set; }
        public Nullable<System.DateTime> VesselArrivalDate { get; set; }
        public Nullable<System.DateTime> StartProductDischargeDate { get; set; }
        public Nullable<System.DateTime> EndProductDischargeDate { get; set; }
        public Nullable<double> BillOfLadingQuantity { get; set; }
        public Nullable<double> ActualQuantityReceived { get; set; }
        public Nullable<double> QuantityDiff { get; set; }
        public string Consignee { get; set; }

        public virtual DischargeParameterAfterObject DischargeParameterAfterObject { get; set; }
        public virtual DischargeParameterBeforeObject DischargeParameterBeforeObject { get; set; }
        public virtual JettyObject JettyObject { get; set; }
        public virtual NotificationObject NotificationObject { get; set; }
        public virtual VesselObject VesselObject { get; set; }
    }
}
