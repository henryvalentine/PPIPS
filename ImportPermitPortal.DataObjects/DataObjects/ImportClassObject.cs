
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class ImportClassObject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ImportClassificationRequirementObject> ImportClassificationRequirementObjects { get; set; }
    }
}
