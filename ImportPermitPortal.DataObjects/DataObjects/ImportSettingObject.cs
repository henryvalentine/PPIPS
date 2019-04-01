
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ImportSettingObject
    {
        public int Id { get; set; }
        public int ApplicationExpiry { get; set; }
        public int ApplicationLifeCycle { get; set; }
        public double PriceVolumeThreshold { get; set; }
        public int VesselArrivalLeadTime { get; set; }
        public int VesselDischargeLeadTime { get; set; }
        public long DischargeQuantityTolerance { get; set; }
        public int PermitExpiryTolerance { get; set; }
        public int PermitValidity { get; set; }
        public int? MessageLifeSpan { get; set; }
        
    }
}
