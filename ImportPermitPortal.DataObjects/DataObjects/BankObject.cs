
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class BankObject
    {
        public int BankId { get; set; }
        public string SortCode { get; set; }
        public long ImporterId { get; set; }
        public string Name { get; set; }
        public string NotificationEmail { get; set; }

        public virtual ICollection<NotificationBankerObject> NotificationBankerObjects { get; set; }
        public virtual ImporterObject ImporterObject { get; set; }
        public virtual ICollection<BankBranchObject> BankBrancheObjects { get; set; }
    }
}
