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
    
    public partial class NotificationInspection
    {
        public NotificationInspection()
        {
            this.BillOfLadingDatas = new HashSet<BillOfLadingData>();
        }
    
        public long Id { get; set; }
        public long NotificationId { get; set; }
        public Nullable<double> QuantityOnVessel { get; set; }
        public Nullable<double> QuantityDischarged { get; set; }
        public long ProductId { get; set; }
        public int DepotId { get; set; }
        public Nullable<long> EmployeeId { get; set; }
        public Nullable<int> StatusId { get; set; }
        public Nullable<int> RecommendationId { get; set; }
        public Nullable<System.DateTime> InspectionDate { get; set; }
        public string InspectorComment { get; set; }
        public Nullable<System.DateTime> DischargeCommencementDate { get; set; }
        public Nullable<System.DateTime> DischargeCompletionDate { get; set; }
        public Nullable<int> SubmittionStatus { get; set; }
        public double QuantityOnBillOfLading { get; set; }
        public Nullable<double> QuantityAfterSTS { get; set; }
        public string LoadPortCoQAvailable { get; set; }
        public Nullable<System.DateTime> InspectionSubmittionDate { get; set; }
        public Nullable<System.DateTime> VesselArrivalDate { get; set; }
        public string FileWay { get; set; }
    
        public virtual ICollection<BillOfLadingData> BillOfLadingDatas { get; set; }
        public virtual Depot Depot { get; set; }
        public virtual Notification Notification { get; set; }
        public virtual Product Product { get; set; }
    }
}
