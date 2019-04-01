using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public partial class NotificationBankerObject
    {
        public string Name { get; set; }
        public string FinancedProductName { get; set; }
        public string SortCode { get; set; }
        public string FinLetterPath { get; set; }
        public string StatusStr { get; set; }
        public long ImporterId { get; set; }
    }
}
         
