
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ImportApplicationHistoryObject
    {
        public long Id { get; set; }
        public long ApplicationId { get; set; }
        public string Event { get; set; }
        public System.DateTime Date { get; set; }

        public virtual ApplicationObject ApplicationObject { get; set; }
    }
}
