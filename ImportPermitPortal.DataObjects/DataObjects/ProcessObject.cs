
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ProcessObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ImportStageId { get; set; }
        public string Description { get; set; }
        public int? StatusId { get; set; }
        public int? SequenceNumber { get; set; }

        public virtual ImportStageObject ImportStageObject { get; set; }
        public virtual ICollection<StepObject> StepObjects { get; set; }
    }
}

