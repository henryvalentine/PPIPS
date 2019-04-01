
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class UserLinkObject
    {
        public long Id { get; set; }
        public long UserProfileId { get; set; }
        public int LinkSequence { get; set; }
        public int LinkId { get; set; }

        public virtual LinkObject LinkObject { get; set; }
    }
}
