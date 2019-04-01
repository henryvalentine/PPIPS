
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class LabParameterObject
    {
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<BillOfLadingDataObject> BillOfLadingDataObjects { get; set; }
        public virtual ICollection<NotificationDischageDataObject> NotificationDischageDataObjects { get; set; }
    }
}
