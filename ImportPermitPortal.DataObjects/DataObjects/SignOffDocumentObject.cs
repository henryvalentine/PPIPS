
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class SignOffDocumentObject
    {
        public long Id { get; set; }
        public long ApplicationId { get; set; }
        public string DocumentPath { get; set; }
        public long UploadedById { get; set; }
        public System.DateTime DateUploaded { get; set; }
        public string IPAddress { get; set; }

        public virtual ApplicationObject ApplicationObject { get; set; }
        public virtual UserProfileObject UserProfileObject { get; set; }
    }
}
