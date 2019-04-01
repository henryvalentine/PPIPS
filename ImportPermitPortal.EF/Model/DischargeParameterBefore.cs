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
    
    public partial class DischargeParameterBefore
    {
        public DischargeParameterBefore()
        {
            this.NotificationDischageDatas = new HashSet<NotificationDischageData>();
        }
    
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
    
        public virtual Notification Notification { get; set; }
        public virtual StorageTank StorageTank { get; set; }
        public virtual ICollection<NotificationDischageData> NotificationDischageDatas { get; set; }
    }
}
