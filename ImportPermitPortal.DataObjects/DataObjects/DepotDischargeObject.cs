
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class DepotDischargeObject
    {
        public long Id { get; set; }
        public double QuantityDischargedInDepot { get; set; }
        public System.DateTime DischargedDate { get; set; }
        public int DepotId { get; set; }

        public virtual DepotObject DepotObject { get; set; }
    }
}
