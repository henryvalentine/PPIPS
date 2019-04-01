

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ProcessesInWorkFlowObject
    {
        public int Id { get; set; }
        public int ProcessId { get; set; }
        public int WorkFlowId { get; set; }
        public int? SequenceNo { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
