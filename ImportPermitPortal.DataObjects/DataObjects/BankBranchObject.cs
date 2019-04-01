
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class BankBranchObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BranchCode { get; set; }
        public int BankId { get; set; }

        public virtual BankObject BankObject { get; set; }
        public virtual ICollection<BankUserObject> BankUserObjects { get; set; }
    }
}
