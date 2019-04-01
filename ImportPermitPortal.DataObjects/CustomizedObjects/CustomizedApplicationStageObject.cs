
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class ApplicationStageObject
    {
        public virtual ICollection<StepObject> StepObjects { get; set; }
    }
}
