

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class RecertificationObject
    {
        public string StatusStr { get; set; }
        public string Activity { get; set; }

        public string Company { get; set; }

        public string DateStr { get; set; }

        public List<DocumentObject> DocumentObjects { get; set; }
        public virtual ICollection<NotificationDocumentObject> NotificationDocumentObjects { get; set; }
      
    }
}

