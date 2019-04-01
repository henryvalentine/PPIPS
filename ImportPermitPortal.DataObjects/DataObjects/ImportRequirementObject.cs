//------------------------------------------------------------------------------

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ImportRequirementObject
    {
        public long Id { get; set; }
        public int ImportStageId { get; set; }
        public int DocumentTypeId { get; set; }
        public string Remark { get; set; }

        public virtual DocumentTypeObject DocumentTypeObject { get; set; }
        public virtual ImportStageObject ImportStageObject { get; set; }
    }
}
