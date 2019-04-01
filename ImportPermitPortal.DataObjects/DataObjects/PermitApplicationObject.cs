//------------------------------------------------------------------------------

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class PermitApplicationObject
    {
        public long Id { get; set; }
        public long PermitId { get; set; }
        public long ApplicationId { get; set; }

        public virtual ApplicationObject ApplicationObject { get; set; }
        public virtual PermitObject PermitObject { get; set; }
    }
}


