using System;
using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public partial class DischargeParameterBeforeObject
    {
        public int Id { get; set; }
        public long TankId { get; set; }
        public long NotificationId { get; set; }
        public Nullable<double> TankGuage { get; set; }
        public Nullable<double> TankTC { get; set; }
        public Nullable<double> CrossVol_TkPcLTRS { get; set; }
        public Nullable<double> SGtC_Lab { get; set; }
        public Nullable<double> VolOfWaterLTRS { get; set; }
        public Nullable<double> NetVolOfOil_TkTc { get; set; }
        public Nullable<double> SG_515C { get; set; }
        public Nullable<double> VolCorrFactor { get; set; }
        public Nullable<double> NetVol_1515C { get; set; }
        public Nullable<double> EquivVolInM_1515C { get; set; }

        public virtual NotificationObject NotificationObject { get; set; }
        public virtual StorageTankObject StorageTankObject { get; set; }
        public virtual ICollection<NotificationDischageDataObject> NotificationDischageDataObjects { get; set; }
    }
}
