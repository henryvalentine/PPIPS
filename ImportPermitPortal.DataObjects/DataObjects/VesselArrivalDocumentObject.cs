
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class VesselArrivalDocumentObject
    {
        public long Id { get; set; }
        public long VesselArrivalNoticeId { get; set; }
        public long DocumentId { get; set; }
        public string ReferenceNo { get; set; }
        public string Remark { get; set; }

        public virtual DocumentObject DocumentObject { get; set; }
        public virtual VesselArrivalNoticeObject VesselArrivalNoticeObject { get; set; }
    }
}
