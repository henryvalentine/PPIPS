
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ImportRecertificationResultObject
    {
        public long Id { get; set; }
        public long ImportNotificationId { get; set; }
        public double Density { get; set; }
        public double Flashpoint { get; set; }
        public double InitialBoilingPoint { get; set; }
        public double TotalSulphur { get; set; }
        public double ResearchOctaneNumber { get; set; }
        public double REIDVapourPressure { get; set; }
        public double Benzene { get; set; }
        public double Ethanol { get; set; }
        public double DieselIndex { get; set; }
        public double FreezingPoint { get; set; }
        public double MSEP { get; set; }
        public double DoctorTest { get; set; }
        public double OffLimitVariance { get; set; }
        public double WithinLimitVariance { get; set; }
        public int EmployeeId { get; set; }
        public string CaptainName { get; set; }
        public bool DischargeApproval { get; set; }

        public virtual EmployeeDeskObject EmployeeDeskObject { get; set; }
        public virtual NotificationObject NotificationObject { get; set; }
    }
}
