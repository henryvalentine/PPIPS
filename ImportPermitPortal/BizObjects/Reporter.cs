using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportPermitPortal.BizObjects
{
    public class Reporter
    {
        public bool IsNull { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsFinal { get; set; }

        public bool CantGeneratePermit { get; set; }
        public bool NoEmployee { get; set; }

        public bool IsError { get; set; }

        public long notid { get; set; }

        public bool IsExpired { get; set; }
        public bool IsValid { get; set; }

        public string IsTimedOut { get; set; }
        public bool IsEmail { get; set; }

        public bool IsPhone { get; set; }

        public bool IsMailSent { get; set; }
        public bool IsUserRegistered { get; set; }

        public bool IsChecklistSaved { get; set; }

        public bool IsRecertificationSaved { get; set; }
        public bool IsDischargeDataSaved { get; set; }

        public bool IsCertificateGenerated { get; set; }

        public bool IsVesselReport { get; set; }

        public long TrackId { get; set; }

        public long UserId { get; set; }

        public string Company { get; set; }

        public long Id { get; set; }
        public long TankId { get; set; }

       
        public string DocPath { get; set; }

        public int ApplicationCount { get; set; }
        public int NotificationCount { get; set; }
        public int RecertificationCount { get; set; }
        public int ExpiringPermitCount { get; set; }
    }
}