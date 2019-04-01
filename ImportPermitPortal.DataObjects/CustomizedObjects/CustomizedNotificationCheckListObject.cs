

using System.Collections.Generic;

namespace ImportPermitPortal.DataObjects
{
    public partial class NotificationCheckListObject
    {
        public string CriteriaRuleStr { get; set; }
        public bool IsNull { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsFinal { get; set; }

        public bool CantGeneratePermit { get; set; }
        public bool NoEmployee { get; set; }

        public bool IsError { get; set; }
        public string MySelection { get; set; }
        public string YesSelection { get; set; }
        public string NoSelection { get; set; }
        public int NotificationId { get; set; }

       
      
     

    }
}

