
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class StepsInGroupObject
    {
        public int Id { get; set; }
        public int StepId { get; set; }
        public int GroupId { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
