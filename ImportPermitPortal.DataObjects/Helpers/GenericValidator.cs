using System;
using System.Collections.Generic;

namespace ImportPermitPortal.DataObjects.Helpers
{
   public class GenericValidator
    {
        public string Error { get; set; }
        public string RedirectUri { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string Eligibility { get; set; }
        public string Email { get; set; }
        public long PersonId { get; set; }
        public long AppId { get; set; }
        public long NotificationId { get; set; }
        public long CompanyId { get; set; } 
        public string ArrivalDate { get; set; }
        public string DischargeDate { get; set; }
        public string Password { get; set; }
        public string Path { get; set; }
        public string ServiceType { get; set; }
        public long Code { get; set; }
        public int ErrorCode { get; set; }
        public int DocumentTypeId { get; set; }
        public string RemitRes { get; set; }
        public double Multiplier { get; set; }
        public double Extent { get; set; }
        public string RefCode { get; set; }
        public bool IsDocsSupplied { get; set; }
        public string OrderId { get; set; }
        public string Hash { get; set; }
        public string Rrr { get; set; }
        public string Phone { get; set; }
        public string DerivedQuantityStr { get; set; }
        public string BalanceVolume { get; set; }
        public double DerivedValue { get; set; }
        public string NotificationTypeName  { get; set; }
        public string PaymentOption { get; set; }
        public string CargoTypeName { get; set; }
        public string CompanyName { get; set; }
        public string FileName { get; set; }
        public bool IsRequirementsMet { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsAddressProvided { get; set; }
        public List<string> UserRoles { get; set; }
        public List<DocumentTypeObject> DocumentTypeObjects { get; set; }
        public List<FeeObject> FeeObjects { get; set; }
        public List<BankUserInfo> BankUserInfoList { get; set; }
    }

   public class RequirementProp
   {
       public int ImportClassId { get; set; }
       public List<long> StorageProviderTypeIds { get; set; }
       public List<long> ProductIds { get; set; }
   }

   public class BankUserInfo
   {
       public string Name { get; set; }
       public string Email { get; set; }
       public string Password { get; set; }
   }

   public class NotificationRequirementProp
   {
       public int NotificationClassId { get; set; }
       public long QuantityToDischarge { get; set; }
       public long EstimatedQuantity { get; set; }
       public long TotalImportedQuantity { get; set; } 
       public int CargoInformationTypeId { get; set; }
       public DateTime DischargeDate { get; set; }
       public DateTime ArrivalDate { get; set; }
   }

   public class NotificationRequirementDetails
   {
       public string OrderId { get; set; }
       public string Hash { get; set; }
       public string Rrr { get; set; }
       public string ServiceType { get; set; }
       public string RedirectUri { get; set; }
       public long Code { get; set; }
       public string Error { get; set; }
       public int PaymentTypeId { get; set; }
       public long NotificationId { get; set; }
       public bool IsVessselsProvided { get; set; }
       public bool IsExpenditionaryApplicable { get; set; }
       public FeeObject ExpenditionaryFee { get; set; }
       public List<string> UnsuppliedDocuments { get; set; }
   }
    
   public class PaymentResponse
   {
      public string OrderId { get; set; }
       public string Status { get; set; }
       public string Rrr { get; set; }
       public string StatusCode { get; set; }
       public int Code { get; set; }
   }


}

