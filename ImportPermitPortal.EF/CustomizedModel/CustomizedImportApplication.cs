using System;
using System.Collections.Generic;
namespace ImportPermitPortal.EF.Model
{
    public partial class Application
    {
        public string DateAppliedStr { get; set; }
        public string StatusStr { get; set; }
        public string LastModifiedStr { get; set; }
        public string CompanyName { get; set; }
    }

    public partial class UserInfo
    {
        public string CompanyName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Rrr { get; set; }
        public string Name { get; set; }
        public string StartDateStr { get; set; }
        public string CompletionDateStr { get; set; }
        public string ActionPerformed { get; set; }
        public List<ApplicationItem> ApplicationItems { get; set; }
    }
}
