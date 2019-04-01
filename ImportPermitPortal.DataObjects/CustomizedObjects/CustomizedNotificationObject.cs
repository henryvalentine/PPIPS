

using System.Collections.Generic;

namespace ImportPermitPortal.DataObjects
{
    public partial class NotificationObject
    {
        public string PortName { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public long UserId { get; set; }
        public int BankId { get; set; }
        public string Email { get; set; }
        public string CountryCode { get; set; }
        public int RegionId { get; set; }
        public int SequenceNumber { get; set; }
        public int JettyPortId { get; set; }
        public string DepotName { get; set; }
        public string StatusStr { get; set; }
        public string ImportClassName { get; set; }
        public string NotificationClassName { get; set; }
        public double ApplicationQuantity { get; set; }
        public double OutStandingQuantity { get; set; }
        public long QuantityImported { get; set; }
        public long ApplicationId { get; set; }
        public int PaymentTypeId { get; set; }
        public string PermitValue { get; set; }
        public string ProductName { get; set; }
        public string PaymentOption { get; set; }
        public string AmountDueStr { get; set; }
        public string ServiceDescription { get; set; }
        public string Code { get; set; }
        public float DerivedValue { get; set; }
        public double AmountDue { get; set; }
        public string ReferenceCode { get; set; } 
        public string ArrivalDateStr { get; set; }
        public double AmountPaid { get; set; }
        public string ProductCode { get; set; }
        public string DischargeDateStr { get; set; }
        public string QuantityToDischargeStr { get; set; }
        public string QuantityOnVesselStr { get; set; }
        public string AmountPaidStr { get; set; }
        public string DateCreatedStr { get; set; }
        public string NotificationTypeName { get; set; }
        public string CargoTypeName { get; set; }
        public string ImporterName { get; set; }
        public string Rrr { get; set; }
       
        public bool IsReportSubmitted { get; set; }
        public bool IsReportVerified { get; set; }
        public bool IsReportCheckListSubmitted { get; set; }
        public bool IsReportCheckListVerified { get; set; }
        public bool IsReportCheckListSaved { get; set; }

        public bool IsRecertificationSubmitted { get; set; }
        public bool IsRecertificationSaved { get; set; }

        public bool IsCheckListFirstTime { get; set; }
        public bool IsCheckListStored { get; set; }

        public bool IsVesselReportSaved { get; set; }

        public bool IsVesselReportSubmitted { get; set; }

        public bool IsReview { get; set; }
        public bool IsApproved { get; set; }
        public string Activity { get; set; }
        public bool IsNull { get; set; }
        public int TankId { get; set; }

        public List<int> SelectedDepotList { get; set; }
        public List<DocumentTypeObject> DocumentTypeObjects { get; set; }
        public List<BankObject> Bankers { get; set; }
        public List<FeeObject> FeeObjects { get; set; }
        public List<ApplicationItemObject> ApplicationItemObjects { get; set; }
        public virtual ICollection<EmployeeDeskObject> EmployeeDeskObjects { get; set; }
    }
}

