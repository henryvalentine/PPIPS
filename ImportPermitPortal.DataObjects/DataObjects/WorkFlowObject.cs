
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class WorkFlowObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? WorkFlowEvent { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
