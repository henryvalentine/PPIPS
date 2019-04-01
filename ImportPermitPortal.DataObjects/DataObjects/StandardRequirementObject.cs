
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class StandardRequirementObject
    {
        public long Id { get; set; }
        public int StandardRequirementTypeId { get; set; }
        public long ImporterId { get; set; }
        public string DocumentPath { get; set; }
        public System.DateTime ValidFrom { get; set; }
        public Nullable<System.DateTime> ValidTo { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public string Title { get; set; }

        public virtual StandardRequirementTypeObject StandardRequirementTypeObject { get; set; }
        public virtual ImporterObject ImporterObject { get; set; }
    }
}

