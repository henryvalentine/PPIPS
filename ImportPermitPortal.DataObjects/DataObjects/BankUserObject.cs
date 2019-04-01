
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class BankUserObject
    {
        public long Id { get; set; }
        public int BranchId { get; set; }
        public long UserId { get; set; }

        public virtual BankBranchObject BankBranchObject { get; set; }
        public virtual UserProfileObject UserProfileObject { get; set; }
    }
}
