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
    
    public partial class NotificationDischageData
    {
        public long Id { get; set; }
        public long NotificationId { get; set; }
        public long TankId { get; set; }
        public long VesselId { get; set; }
        public int DepotId { get; set; }
        public int DischargeParameterBeforeId { get; set; }
        public int DischargeParameterAfterId { get; set; }
        public Nullable<System.DateTime> VesselArrivalDate { get; set; }
        public Nullable<System.DateTime> StartProductDischargeDate { get; set; }
        public Nullable<System.DateTime> EndProductDischargeDate { get; set; }
        public Nullable<double> BillOfLadingQuantity { get; set; }
        public Nullable<double> ActualQuantityReceived { get; set; }
        public Nullable<double> QuantityDiff { get; set; }
        public string Consignee { get; set; }
    
        public virtual Depot Depot { get; set; }
        public virtual DischargeParameterAfter DischargeParameterAfter { get; set; }
        public virtual DischargeParameterBefore DischargeParameterBefore { get; set; }
        public virtual Notification Notification { get; set; }
        public virtual StorageTank StorageTank { get; set; }
        public virtual Vessel Vessel { get; set; }
    }
}