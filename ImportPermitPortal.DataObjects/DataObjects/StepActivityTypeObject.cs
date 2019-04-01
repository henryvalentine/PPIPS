
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class StepActivityTypeObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<EmployeeDeskObject> EmployeeDeskObjects { get; set; }
        public virtual ICollection<StepObject> StepObjects { get; set; }
    }
}
