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
    
    public partial class ImportSetting
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
        public Nullable<int> MessageLifeSpan { get; set; }
    }
}
