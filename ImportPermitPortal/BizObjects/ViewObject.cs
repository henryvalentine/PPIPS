using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;

namespace ImportPermitPortal.BizObjects
{
    public class ViewObject
    {
        public ViewObject()
        {

            listOfApplicationItems = new List<DatabaseObject>();
            listOfApplicationDocuments = new List<DatabaseObject>();
            listOfProcessTrackings = new List<DatabaseObject>();
            listOfEmployeeHistory = new List<DatabaseObject>();
            listOfIssueTypes = new List<DatabaseObject>();


        }


         [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        
       
        public int GroupId { get; set; }
       
      
    

        //for company
        public long Id { get; set; }
        public string CompanyName { get; set; }
        public string CompanyTIN { get; set; }
        public string CompanyRCNumber { get; set; }
        public System.DateTime CompanyDateRegistered { get; set; }
        public int CompanyAccountTypeId { get; set; }
        public int CompanyStructureId { get; set; }

        //for structure
        public int StructureId { get; set; }
        public string StructureName { get; set; }
        public string StructureDescription { get; set; }

        //for account type
        public int AccountTypeId { get; set; }
        public string AccountTypeName { get; set; }
        public string AccountTypeDescription { get; set; }

        //for Application
        public long ApplicationId { get; set; }
        public long ApplicationDerivedTotalQUantity { get; set; }
        public double ApplicationDerivedValue { get; set; }
        public int ApplicationStatusCode { get; set; }
        public System.DateTime ApplicationDateApplied { get; set; }
        public System.DateTime ApplicationLastModified { get; set; }
        public string ApplicationReferenceCode { get; set; }

        public string ApplicationCompanyName { get; set; }

        public List<DatabaseObject> listOfApplicationItems { get; set; }


        //Document
        public long DocumentId { get; set; }
        public int DocumentDocumentTypeId { get; set; }
        public System.DateTime DocumentDateUploaded { get; set; }
        public int DocumentStatus { get; set; }
        public Nullable<long> DocumentUploadedById { get; set; }
        public string DocumentDocumentPath { get; set; }

        public string DocumentName { get; set; }

        public List<DatabaseObject> listOfApplicationDocuments { get; set; }


        //process tracking
        public long ProcessTrackingApplicationId { get; set; }
        public string ProcessTrackingStep { get; set; }
        public string ProcessTrackingEmployee { get; set; }
        public string ProcessTrackingStepStatus { get; set; }
        public string ProcessTrackingStepActivityType { get; set; }
        public string ProcessTrackingProcessStatus { get; set; }
        public Nullable<System.DateTime> ProcessTrackingAssignedTime { get; set; }
        public Nullable<System.DateTime> ProcessTrackingDueTime { get; set; }
        public Nullable<System.DateTime> ProcessTrackingActualDeliveryDateTime { get; set; }

        public List<DatabaseObject> listOfProcessTrackings { get; set; }


        //for history   
        public long HistoryApplicationId { get; set; }
        public int HistoryStepId { get; set; }
        public int HistoryEmployeeId { get; set; }
        public string HistoryStepStatus { get; set; }
        public string HistoryStepActivityType { get; set; }
        public string HistoryProcessStatus { get; set; }
        public Nullable<System.DateTime> HistoryAssignedTime { get; set; }
        public Nullable<System.DateTime> HistoryDateLeft { get; set; }
        public string HistoryComment { get; set; }
        public string HistoryOutcome { get; set; }


        public string IssueTypeName { get; set; }
        public string IssueTypeComment { get; set; }


        public List<DatabaseObject> listOfEmployeeHistory = new List<DatabaseObject>();

        List<DatabaseObject> listOfIssueTypes = new List<DatabaseObject>();

        public string PermitFile { get; set; }
        public string checker { get; set; }
        public string loginuser { get; set; }
    }
}