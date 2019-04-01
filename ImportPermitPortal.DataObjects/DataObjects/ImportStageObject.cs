
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ImportStageObject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<FeeObject> FeeObjects { get; set; }
        public virtual ICollection<ImportClassificationRequirementObject> ImportClassificationRequirementObjects { get; set; }
        public virtual ICollection<ImportRequirementObject> ImportRequirementObjects { get; set; }
        public virtual ICollection<ProcessObject> ProcessObjects { get; set; }
    }
}
