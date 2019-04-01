using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public partial class ApplicationObject
    {
        public string DateAppliedStr { get; set; }
        public string ImporterStr { get; set; }

        public string PermitStr { get; set; }
        public string StatusStr { get; set; }
        public string CurrentDesk { get; set; }
        public string LastModifiedStr { get; set; }
        public string BankAccountNumber { get; set; }
        public string DerivedQuantityStr { get; set; }
        public string AppTypeStr { get; set; }
        public string ServiceDescription { get; set; }
        public string DerivedValueStr { get; set; }
        public string AmountStr { get; set; }
        public string CompanyName { get; set; }
        public string PermitNumber { get; set; }
        public string DateIssued { get; set; }
        public string ExpiryDate { get; set; }
        public double AmountPaid { get; set; }
        public string BankerName { get; set; }
        public string ImportClassName { get; set; } 
        public string Hash { get; set; }
        public string RedirectUri { get; set; }
        public string Eligibility { get; set; }
        public int HistoryCount { get; set; }
        public int IssuesCount { get; set; }
        public long PermitId { get; set; }
        public long UserId { get; set; }
        public string ReferenceCode { get; set; }
        public string Rrr { get; set; }
        public string PhoneNumber { get; set; }
        public string SignOffDocumentPath { get; set; }
        public string Email { get; set; }
        public string PaymentOption { get; set; }
        public int PaymentTypeId { get; set; }
        public string StorageProviderTypeName { get; set; }
        public string CategoryName { get; set; }
        public bool IsSignOffDocUploaded { get; set; }
        public string ApplicationTypeName { get; set; }
        public string Activity { get; set; }
        public float PercentageCompletion { get; set; }

        public EmployeeDeskObject CurrentEmployeeDesk { get; set; }
        public List<DocumentTypeObject> DocumentTypeObjects { get; set; }
        public List<FeeObject> FeeObjects { get; set; }
        public List<StepObject> Steps { get; set; }
        public List<ProcessObject> Processes { get; set; }
        public List<BankObject> Bankers { get; set; }
        public virtual ICollection<StandardRequirementObject> StandardRequirementObjects { get; set; }

        public virtual ICollection<ApplicationIssueObject> IssueObjects { get; set; }

    }
}
