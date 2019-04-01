

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ApplicationLicenseMappingObject
    {
        public long Id { get; set; }
        public long ApplicationId { get; set; }
        public long ReferenceLicenseId { get; set; }
        public bool IsConfirmed { get; set; }

        public virtual ApplicationObject ApplicationObject { get; set; }
        public virtual ReferenceLicenseObject ReferenceLicenseObject { get; set; }
    }
}
