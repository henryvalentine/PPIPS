
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class StandardRequirementTypeObject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<StandardRequirementObject> StandardRequirementObjects { get; set; }
    }
}

