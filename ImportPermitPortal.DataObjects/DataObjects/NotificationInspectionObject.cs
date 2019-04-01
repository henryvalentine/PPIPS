
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class NotificationInspectionObject
    {
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
        public virtual ICollection<BillOfLadingDataObject> BillOfLadingDataObjects { get; set; }
        public virtual DepotObject DepotObject { get; set; }
        public virtual NotificationObject NotificationObject { get; set; }
        public virtual ProductObject ProductObject { get; set; }
    }
}
