using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public partial class ImportStageObject
    {
        public virtual ICollection<StepObject> StepObjects { get; set; }
    }
}
         
