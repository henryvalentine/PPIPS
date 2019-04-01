
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class BillOfLadingDataObject
    {
        public long Id { get; set; }
        public long NotificationInspectionId { get; set; }
        public int LabParameterId { get; set; }
        public double ValueIndicated { get; set; }

        public virtual LabParameterObject LabParameterObject { get; set; }
        public virtual NotificationInspectionObject NotificationInspectionObject { get; set; }
    }
}
