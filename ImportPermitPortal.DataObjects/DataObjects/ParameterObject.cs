using System;
using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public class ParameterObject
    {
        public int Id { get; set; }
        public long TankId { get; set; }
        public string TankNo { get; set; }
        public long NotificationId { get; set; }
        public Nullable<double> TankGuageBefore { get; set; }
        public Nullable<double> TankTCBefore { get; set; }
        public Nullable<double> CrossVol_TkPcLTRSBefore { get; set; }
        public Nullable<double> SGtC_LabBefore { get; set; }
        public Nullable<double> VolOfWaterLTRSBefore { get; set; }
        public Nullable<double> NetVolOfOil_TkTcBefore { get; set; }
        public Nullable<double> SG_515CBefore { get; set; }
        public Nullable<double> VolCorrFactorBefore { get; set; }
        public Nullable<double> NetVol_1515CBefore { get; set; }
        public Nullable<double> EquivVolInM_1515CBefore { get; set; }


        public Nullable<double> TankGuageAfter { get; set; }
        public Nullable<double> TankTCAfter { get; set; }
        public Nullable<double> CrossVol_TkPcLTRSAfter { get; set; }
        public Nullable<double> SGtC_LabAfter { get; set; }
        public Nullable<double> VolOfWaterLTRSAfter { get; set; }
        public Nullable<double> NetVolOfOil_TkTcAfter { get; set; }
        public Nullable<double> SG_515CAfter { get; set; }
        public Nullable<double> VolCorrFactorAfter { get; set; }
        public Nullable<double> NetVol_1515CAfter { get; set; }
        public Nullable<double> EquivVolInM_1515CAfter { get; set; }

       
    }
}
