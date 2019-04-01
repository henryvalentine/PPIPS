
namespace ImportPermitPortal.DataObjects
{
    public partial class MessageTemplateObject
    {
        public string EventTypeName { get; set; }
        public string SecurityStamp { get; set; }
        public string UserName { get; set; }
        public int? MessageLifeSpan { get; set; }
        public long UserId { get; set; }
        public int ErrorCode { get; set; }
        public bool IsImporter { get; set; }
        public string PhoneNumber { get; set; }
    }
     
}