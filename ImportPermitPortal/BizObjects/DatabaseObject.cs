using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportPermitPortal.BizObjects
{
    public class DatabaseObject
    {
        public long Id { get; set; }

         //Import Application Properties
        public long ApplicationId { get; set; }
        public long ApplicationDerivedTotalQUantity { get; set; }
        public double ApplicationDerivedValue { get; set; }
        public int ApplicationStatusCode { get; set; }
        public System.DateTime ApplicationDateApplied { get; set; }
        public System.DateTime ApplicationLastModified { get; set; }
        public string ApplicationReferenceCode { get; set; }

        public string ApplicationCompanyName { get; set; }

        //Import Item Properties
        public long ApplicationItemId { get; set; }
        public long ApplicationItemApplicationId { get; set; }
        public long ApplicationItemProductId { get; set; }
        public long ApplicationItemEstimatedQuantity { get; set; }
        public double ApplicationItemEstimatedValue { get; set; }
        public string ApplicationItemPortOfOrigin { get; set; }
        public string ApplicationItemPortOfDischarge { get; set; }

        public string ApplicationItemProductName { get; set; }

        //Document
        public int DocumentDocumentTypeId { get; set; }
        public long DocumentId { get; set; }
        public System.DateTime DocumentDateUploaded { get; set; }
        public int DocumentStatus { get; set; }
        public Nullable<long> DocumentUploadedById { get; set; }
        public string DocumentDocumentPath { get; set; }

        public string DocumentName { get; set; }

       
        //for employee roles
        public long EmploeeRolesEmployeeId { get; set; }
        public string EmploeeRolesRoleId { get; set; }
        public Nullable<System.DateTime> EmploeeRolesCreated_At { get; set; }
        public Nullable<System.DateTime> EmploeeRolesUpdated_At { get; set; }

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
        public string HistoryReferenceNumber { get; set; }

        public string HistoryCompanyName { get; set; }

        //for permit
        public long PermitApplicationId { get; set; }
        public int PermitPermitNo { get; set; }
        public string PermitStatus { get; set; }
        public Nullable<System.DateTime> PermitIssueDate { get; set; }
        public Nullable<System.DateTime> PermitExpiryDate { get; set; }
        public string Permitfile { get; set; }

        //issue type
        public string IssueTypeName { get; set; }
    
    }
}