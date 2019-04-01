
namespace ImportPermitPortal.DataObjects
{
    public class ResponseObject
    {
        public string HashValue { get; set; }
        public string RefCode { get; set; }
        public long Code { get; set; }
        public long InvoiceId { get; set; }
        public string FeedBackMessage { get; set; }
        public string RRR { get; set; }
        public string TimeStamp { get; set; }
        public string applications { get; set; }
        public string notifications { get; set; }
        public string recertifications { get; set; }
    }
}