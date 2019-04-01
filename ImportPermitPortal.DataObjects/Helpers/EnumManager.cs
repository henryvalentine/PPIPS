namespace ImportPermitPortal.DataObjects.Helpers
{
    public enum AvailabilityEnum
    {
        Available = 1,
        Un_Available
    }

    public enum StorageProviderTypeEnum
    {
        Own_Depot = 1,
        Third_Party_Depot,
        Industrial_Consumer
    }

    public enum BankRoleEnum
    {
        Banker = 1,
        Bank_User = 5
    }
    public enum RemitaTransactionCodes
    {
       Transaction_Completed_Successfully = 0,
	    Transaction_Approved = 1,
        Transaction_Failed = 2,
        User_Aborted_Transaction = 12,
        Invalid_User_Authentication = 20,      
        Transaction_Pending = 21,
        Invalid_Request = 22,
        Service_Type_or_Merchant_Does_not_Exist = 23,
        Payment_Reference_Generated = 25,
        Invalid_Bank_Code = 29,
        Insufficient_Balance = 30,
        No_Funding_Account = 31,
        Invalid_Date_Format = 32,
        Initial_Request_OK = 40,
        Unknown_Error = 999
    }

    public enum RemitaServiceType
    {
        MecharntId = 442773233,
        ApiKey = 486213,
        NotificationExpenditionary = 481863784,
        Expenditionary =  442725336,
        Notification = 442725330,
        Recertification = 442725330,
        Application = 437411514
    }
    
    public enum SpecialRolesEnum
    {
        Support = 7,
        ICT = 8,
        Accounts = 9,
        Verifier = 10
    }

    public enum InvoiceStatus
    {
        Pending = 1,
        Paid,
        Uncleared
    }

    public enum Dpr
    {
        DPR = 9
    }
    public enum MessageStatus
    {
        Sent = 1,
        Pending,
        Failed
    }

    public enum UserStatusEnum
    {
        Active = 1,
        Inactive
    }

    public enum AddressTypeEnum2
    {
        Registered = 1,
        Operational,
        Branch_Office
    }
    public enum IssueStatus
    {
        Open = 1,
        Resolved

    }

    public enum PaymentOption
    {
        Online = 1,
        Bank

    }
    
    public enum CountryEnum
    {
        Nigeria = 9,
    }

    public enum FeeTypeEnum
    {
        Statutory_Fee = 1,
        Processing_Fee,
        Expeditionary
    }

    public enum SpecialDocsEnum
    {
        Form_M = 9,
        Telex_Copy = 22,
        Throughput_agreement = 25,
        Dry_Tank_Certificate = 36,
        Bank_Reference_Letter = 38,
        ROB = 37
    }
    
    public enum AccountTypeEnum
    {
        Operating_Company = 1,
        Financial_Institution,
        Depot_Owner
        
    }

    public enum NotificationClassEnum
    {
        Coastal_Importation = 1,
        Trucking
    }

    public enum CargoTypeEnum
    {
        Direct_Shipment = 1,
        Ship_to_Ship
    }

    public enum VesselClassEnum
    {
       Mother_Vessel = 1, 
        Shuttle_Vessel
    }
    public enum EligibilityEnum
    {
        PPMC = 1,
        Major_Marketer,
        IPMAN,
        DAPPMA,
        Financial_Instituiton
    }
   
    public enum AppStatus
    {
        Not_Available = 0,
        Pending,
        Paid,
        Submitted,
        Processing,
        Verifying,
        Approved,
        Declined, 
        Archived,
        Rejected
    }

    public enum AdminAppStatus
    {
        Not_Available = 0,
        Pending,
        Paid,
        Submitted,
        Processing,
        Verifying,
        SignedOff,
        Declined,
        Archived,
        Rejected
    }
    
    public enum NotificationStatusEnum
    {
        Pending = 1, 
        Paid,
        Submitted,
        Declined,
        Scheduled,
        Processing,
        Approved,
        Completed,
        Recertifying,
        Rejected

    }

    public enum RecertificationStatusEnum
    {
        
        Processing =1,
        Approved,
        Rejected

    }
    public enum VesselTypeEnum
    {
        Mother_Vessel = 1,
        Shuttle_Vessel
    }

    public enum AppRole
    {
        Banker = 1,
        Applicant = 2,
        Depot_Owner = 6
    }

    public enum AppStage
    {
        Application = 1,
        Notification = 2,
        Recertification = 3
    }

    public enum AppTypeEnum
    {
        New = 1,
        Renewal = 2,
        Inclusion = 3
    }

    public enum CustomColEnum
    {
        Psf = 1,
        Reference_Code
    }

    public enum DocStatus
    {
        Pending = 1,
        Valid,
        Invalid
    }

    public enum IssueStatusEnum
    {
        Open = 1,
        Resolved,
        Pending

    }

    public enum PaymentStatusEnum
    {
        Pending = 1,
        Paid,
        Uncleared

    }

    public enum PaymentType
    {
        Online = 1,
        Bank
    }

    public enum RefLicenseTypeEnum
    {
        Lube_Blending_Plant_License = 1,
        LPG_Plant_License,
        PSF,
        Depot_License_Number,
        Coastal_Vessel_License_Number
    }

    public enum PermitStatusEnum
    {
        Inactive = 1,
        Active,
        Completed,
        Expired
    }

    public enum ServiceDescriptionEnum
    {
        Import_Permit_Application_Fee = 1,
        Vessel_Arrival_Notification_Fee,
        Vessel_Arrival_Expeditionary_Fee
    }

    public enum StepStatusEnum
    {
        Inactive = 1,
        Active,
        Completed

    }

    public enum StepActivityTypeEnum
    {
        Review = 1,
        Approve
    }

    public enum EmployeeDeskEnum
    {
        Available = 1,
        Unavailable
    }

    public enum ProcessStatusEnum
    {
        Open = 1,
        Treated,
        Rejected,
        Transfered
    }

    public enum OutComeCodeEnum  
    {
        Open = 1,
        Treated,
        Rejected,
        Transfered
    }

    public enum EnumNotificationOutComeCode
    {
        Request = 1,
        Redeemed,
        Completed,
        Cancelled,
        Transferred
    }

    public enum EnumCheckListOutComeStatus
    {
        Saved = 1,
        Submitted,
        Verified

    }
    public enum EnumCheckListCriteriaRule
    {
        EqualTo = 1,
        Greater_Than,
        Less_Than,
        Inrange

    }

    public enum EnumDischargeApproval
    {
        Yes = 1,
        No

    }


    public enum MessageEventEnum
    {
        New_Account = 1,
        New_User,
        Account_Confirmation,
        Password_Reset,
        Password_Reset_Successful,
        Activation_Link_Request,
        Payment_Alert,
        New_Application,
        Application_Edit,
        Application_Submission,
        New_Notification,
        Notification_Edit,
        Notification_Submission,
        New_Permit,
        Permit_Renewal,
        Payment_Receipt,
        Confirmation_Of_Throughput,
        Confirmation_Of_Previous_Import_Permit,
        Employee_NewJob,
        Document_Support,
    }

    public enum EnumActivity
    {
        TeamLeader = 1,
        JettyInspection

    }

    public enum EnumMeasurement
    {
        m3 = 1,
        cm3

    }
   
}