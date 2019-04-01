using System;
using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public partial class StorageTankObject
    {
        public long Id { get; set; }
        public int DepotId { get; set; }
        public string TankNo { get; set; }
        public long ProductId { get; set; }
        public Nullable<double> Capacity { get; set; }
        public int UoMId { get; set; }

        public virtual DepotObject DepotObject { get; set; }
        public virtual ICollection<DischargeParameterAfterObject> DischargeParameterAfterObjects { get; set; }
        public virtual ICollection<DischargeParameterBeforeObject> DischargeParameterBeforeObjects { get; set; }
        public virtual ProductObject ProductObject { get; set; }
        public virtual UnitOfMeasurementObject UnitOfMeasurementObject { get; set; }
        public virtual ICollection<NotificationDischageDataObject> NotificationDischageDataObjects { get; set; }
    }
}
