

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class AspNetUserLoginObject
    {
        public string UserId { get; set; }
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }

        public virtual AspNetUserObject AspNetUserObject { get; set; }
    }
}
