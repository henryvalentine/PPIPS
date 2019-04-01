using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportPermitPortal.BizObjects
{

    public enum EnumPermitStatus
    {
        Inactive = 1,
        Active,
        Completed
    }
    public enum EnumStepStatus
    {
       Inactive,
        Active,
        Completed
    }

    public enum EnumStepActivityType
    {
        Review,
        Approve
    }

    public enum EnumEmployeeDesk
    {
       Available,
       Unavailable
    }


    public enum EnumProcessStatus
    {
        Inactive,
        Active,
        Suspended,
        Archived,
        Completed
    }

    public enum Document_Validation
    {
        End = 0,
        Start = 1,
        Account_Document_Cheking = 4,
        Account_Document_Approval = 5
    }

    public enum PermitValidation
    {
        End = 0,
        Start = 1,
        Account_Document_Check = 2,
        Account_Document_Approval = 3

    }

    public enum NewApplication
    {
        End,
        Start,
        Account_Document_Cheking,
        Account_Document_Approval
    }

   
    public enum StepSequenceEnum
    {
        NewApplication = 1,
        AccountDocumentChecking,
        DownstreamDirector
    }
}