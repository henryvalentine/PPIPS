
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class DepotTrunkedOutObject
    {
        public long Id { get; set; }
        public double QuantityTrunkedOutInDepot { get; set; }
        public System.DateTime TrunkedOutDate { get; set; }
        public int DepotId { get; set; }

        public virtual DepotObject DepotObject { get; set; }
    }
}
