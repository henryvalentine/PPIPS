
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class StandardRequirementTypeObject
    {
        public string DocumentPath { get; set; }
        public long StandardRequirementId { get; set; }
        public long ImporterId { get; set; }
        public string ValidFromStr { get; set; }
        public bool IsUploaded { get; set; }
        public string ValidToStr { get; set; }
    }
}
