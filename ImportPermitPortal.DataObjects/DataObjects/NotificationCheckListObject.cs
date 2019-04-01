namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class NotificationCheckListObject
    {
        public int Id { get; set; }
        public string CheckListItem { get; set; }
        public int CriteriaRuleId { get; set; }
        public string ExpectedValue1 { get; set; }
        public string ExpectedValue2 { get; set; }
        public string ItemScore { get; set; }

        public virtual ICollection<NotificationCheckListOutcomeObject> NotificationCheckListOutcomeObjects { get; set; }
    }
}
