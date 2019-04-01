using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ImportPermitPortal.DataObjects;
using Newtonsoft.Json;

namespace ImportPermitPortal.Helpers
{
    public class TokenReq
    {
        public long ClientKey { get; set; }
        public string ClientSecrete { get; set; }
        public string ResponseMessage { get; set; }
    }

    public class Token
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty(".issued")]
        public DateTime Issued { get; set; }

        [JsonProperty(".expires")]
        public DateTime Expires { get; set; }

        public string EXN_TGD { get; set; }

    }

    public class SmsPayLoad
    {
        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class SignUpModel
    {
        public string PhoneNumber { get; set; }
        [Required]
        public string RCNumber { get; set; }
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string CompanyTIN { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public int StructureId { get; set; }
    }

   public class GenericEligibilityList
   {
       public List<ImportEligibilityObject> ImportEligibilities { get; set; }
       public List<DocumentTypeObject> DocumentTypes { get; set; }
   }

   public class ProductColList
   {
       public List<ProductObject> ProductObjects { get; set; }
       public List<CustomCodeObject> CustomCodes { get; set; }
   }

   public class GenericClassificationList
   {
       public List<ImportStageObject> ImportStages { get; set; }
       public List<DocumentTypeObject> DocumentTypes { get; set; }
       public List<ImportClassObject> Classes { get; set; }
   }

   public class GenericProcessList
   {
       public List<ImportStageObject> ImportStages { get; set; }

   }
   public class GenericProductList
   {
       public List<ProductObject> Products { get; set; }
       public List<DocumentTypeObject> DocumentTypes { get; set; }
   }

   public class GenericStorageTankList
   {
       public List<DepotObject> Depots { get; set; }
       public List<ProductObject> Products { get; set; }
       public List<UnitOfMeasurementObject> Measurements { get; set; }
   }


   public class GenericDepoList
   {
       public List<DepotOwnerObject> Owners { get; set; }
       public List<JettyObject> Jettys { get; set; }
      
   }

   public class GenericRecertificationList
   {
       public List<NotificationObject> Recerts { get; set; }
       

   }

   public class GenericFeeList
   {
       public List<ImportStageObject> ImportStages { get; set; }
       public List<FeeTypeObject> FeeTypes { get; set; } 
   }

   public class GenericDocumentList
   {
       public List<ImportStageObject> ImportStages { get; set; }
       public List<DocumentTypeObject> DocumentTypes { get; set; }
   }

   public class GenericJettyList
   {
       public List<PortObject> Ports { get; set; }
   }

   public class GenericPortList
   {
       public List<CountryObject> Countries { get; set; }
   }

   public class GenericCountryList
   {
       public List<RegionObject> Regions { get; set; }
   }

   public class GenericJettyMappingList
   {
       public List<JettyObject> Jetties { get; set; }
       public List<ZoneObject> Zones { get; set; }
   }
    
   public class GenericDocumentRightsList
   {
       public List<AspNetRoleObject> Roles { get; set; }
       public List<DocumentTypeObject> DocumentTypes { get; set; }
   }

   public class NotificationGenericList
   {
       public List<DepotObject> DepotList { get; set; }
       public List<ProductObject> Products { get; set; }
       public List<CountryObject> Countries { get; set; }
       public ImportSettingObject ImportSettingObject { get; set; }
       public List<ImportClassObject> ImportClasses { get; set; }
       public List<PermitObject> PermitObjects { get; set; }
   }

   public class PortAndCountry
   {
       public List<PortObject> Ports { get; set; }
       public List<DepotObject> DepotList { get; set; }
       public List<CountryObject> Countries { get; set; }
   }

   public class ApplicationGenericList
   {
       public List<StorageProviderTypeObject> StorageProviderTypes { get; set; }
       public List<ProductObject> Products { get; set; }
       public List<BankObject> Banks { get; set; }
       public List<ImportClassObject> Classes { get; set; }
       public List<GenericObject> ApplicationTypes { get; set; }
   }

   public class GenericStepList
   {
       public List<ProcessObject> Processes { get; set; }
       public List<GroupObject> Groups { get; set; }

       public List<StepObject> Steps { get; set; }
       public List<StepActivityTypeObject> StepActivityTypes { get; set; }

       public List<ImportStageObject> ImportStages { get; set; }
   }

   public class GenericEmployeeDeskList
   {
       public List<ZoneObject> Zones { get; set; }
       public List<GroupObject> Groups { get; set; }
       public List<StepActivityTypeObject> ActivityTypes { get; set; }
   }

   public class GenericEmplyeeProfileList
   {
      public List<IssueTypeObject> IssueTypes { get; set; }
       public List<ProductObject> Products { get; set; }
       public List<JettyObject> Jettys { get; set; }
       public List<GenericObject> DischargeApprovals { get; set; }

       public List<StorageTankObject> Tanks { get; set; }
       public List<ProductColourObject> ProductColours { get; set; }
      
   }

   public class GenericDepotList
   {
       public List<DepotObject> Depots { get; set; }
       public List<ProductObject> Products { get; set; }


   }

  public class GenericNotificationCheckList
   {
       public List<GenericObject> CriteriaRules { get; set; }
       
   }

    public class AccountValidator
    {
        public string Message { get; set; }
        public string SecurityStamp { get; set; }
        public string Email { get; set; }
        public long Code { get; set; }
    }
    
   
}

