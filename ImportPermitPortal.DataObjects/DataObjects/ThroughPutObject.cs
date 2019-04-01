
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ThroughPutObject
    {
        public long Id { get; set; }
        public long ApplicationItemId { get; set; } 
        public int DepotId { get; set; }
        public long ProductId { get; set; }
        public double Quantity { get; set; }
        public string Comment { get; set; }
        public long? DocumentId { get; set; }
        public string IPAddress { get; set; }

        public virtual ApplicationItemObject ApplicationItemObject { get; set; }
        public virtual DepotObject DepotObject { get; set; }
        public virtual DocumentObject DocumentObject { get; set; }
        public virtual ProductObject ProductObject { get; set; }
    }
}


