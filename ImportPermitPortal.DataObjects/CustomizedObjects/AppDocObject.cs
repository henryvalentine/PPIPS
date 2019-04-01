using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportPermitPortal.DataObjects
{
    public class AppDocObject
    {
        public string DateUploadedStr { get; set; }
        public string StatusStr { get; set; }
        public string AppReference { get; set; }
        public string DocumentTypeName { get; set; }
        public string CompanyName { get; set; }
    }

    public class ItemCountObject
    {
       public long  ApplicationCount  { get; set; }
       public long  NotificationCount  { get; set; }
       public long ExpiringPermitCount { get; set; }
       public long RecertificationCount { get; set; }
       public long ApprovedApplicationCount { get; set; }
       public long VerificationApplicationCount { get; set; } 
    }
    public class AppCountObject
    {
        public long RejectedAppCount { get; set; }
        public long ApprovedAppCount { get; set; }
        public long TotalCount { get; set; }
        public long RecertificationCount { get; set; }
        public long ProcessingAppCount { get; set; }
        public long SubmittedAppCount { get; set; }
        public long PaidAppCount { get; set; }
        public string PaidAppHref { get; set; }
        public string SubmittedAppCountHref { get; set; }
        public string ProcessingAppHref { get; set; }
        public string TotalAppHref { get; set; }
        public string ApprovedAppHref { get; set; }
        public string RejectedHref { get; set; }
        public string ItemName { get; set; }
    }
    

    public class AppMailObject
    {
        public string ReferenceNumber { get; set; }
        public string PaymentReference { get; set; }
        public string ApplicationType { get; set; }
        public string PhoneNumber { get; set; }
        public string ApplicationCategory { get; set; }
        public string AmountDue { get; set; }
        public string PaymentStatus { get; set; }
        public string DateApplied { get; set; }
        public string ImporterName { get; set; }
        public string MssageContent { get; set; }
        public string Email { get; set; }
        public long UserId { get; set; }
        public int TypeId { get; set; }
        public string DateSubmitted{ get; set; }
        public string ApplicablePeriod { get; set; }
        public List<BankObject> Bankers { get; set; }
    }


    public class PermitMailObject
    {
        public string ReferenceNumber { get; set; }
        public string PermitNumber { get; set; }
        public string PhoneNumber { get; set; }
        public long UserId { get; set; }
        public string AmountDue { get; set; }
        public string DateApplied { get; set; }
        public string ImporterName { get; set; }
        public string MssageContent { get; set; }
        public string Email { get; set; }
        public string Rrr { get; set; }
        public string DateSubmitted { get; set; }
        public string ApplicablePeriod { get; set; }
    }


    public class EmployeeMailObject
    {

        public long UserId { get; set; }
        
        public string DateApplied { get; set; }
     
        public string MssageContent { get; set; }
        public string Email { get; set; }

        public string Name { get; set; }

        public string DateSubmitted { get; set; }
       
    }


    public class DocumentResponse
    {
        public string Error { get; set; }
        public int Code { get; set; }
        public List<DocumentTypeObject> UnsuppliedDocs { get; set; }
    }


    public class AccountObject
    {
        public long ApplicationTotalAmount { get; set; }
        public long NotificationTotalAmount { get; set; }
        public List<PaymentReceiptObject> PaymentReceipts { get; set; } 
    }
}