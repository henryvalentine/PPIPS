using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public partial class DocumentObject
    {
        public string DateUploadedStr { get; set; }
        public string StatusStr { get; set; }
        public string AppReference { get; set; }
        public string DocumentTypeName { get; set; }
        public string CompanyName { get; set; }
        public string Comment { get; set; }
        public long ApplicationId { get; set; }
        public long ApplicationItemId { get; set; }
        public int BankId { get; set; }
        public int DepotId { get; set; }
        public long NotificationId { get; set; }
    }
}

                           
                           
                               