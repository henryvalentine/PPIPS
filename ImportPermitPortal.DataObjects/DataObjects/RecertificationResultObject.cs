using System;
using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public partial class RecertificationResultObject
    {
        public long Id { get; set; }
        public long NotificationId { get; set; }
        public Nullable<double> Density { get; set; }
        public Nullable<double> Flashpoint { get; set; }
        public string Colour { get; set; }
        public Nullable<double> InitialBoilingPoint { get; set; }
        public Nullable<double> FinalBoilingPoint { get; set; }
        public Nullable<double> TotalSulphur { get; set; }
        public Nullable<double> TotalAcidity { get; set; }
        public Nullable<double> ResearchOctaneNumber { get; set; }
        public Nullable<double> REIDVapourPressure { get; set; }
        public Nullable<double> Benzene { get; set; }
        public Nullable<double> Ethanol { get; set; }
        public Nullable<double> DieselIndex { get; set; }
        public Nullable<double> FreezingPoint { get; set; }
        public Nullable<double> MSEP { get; set; }
        public Nullable<double> DoctorTest { get; set; }
        public string LimitVariance { get; set; }
        public string Spec { get; set; }
        public Nullable<long> EmployeeId { get; set; }
        public string CaptainName { get; set; }
        public string DischargeApproval { get; set; }
        public Nullable<int> SubmittionStatus { get; set; }
        public Nullable<int> ProductColour { get; set; }
        public Nullable<int> RecommendationId { get; set; }

        public virtual EmployeeDeskObject EmployeeDeskObject { get; set; }
        public virtual NotificationObject NotificationObject { get; set; }
    }
}
