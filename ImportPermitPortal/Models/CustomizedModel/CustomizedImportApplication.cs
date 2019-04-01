
namespace ImportPermitPortal.Models
{
    public partial class Application
    {
        public string DateAppliedStr { get; set; }
        public string LastModifiedStr { get; set; }
        public string StatusStr { get; set; }
        public bool Rejected { get; set; }
        public string Approved { get; set; }
        public string CompanyName { get; set; }
        public long Code { get; set; }
        public int CompanyCategoryId { get; set; }
    }
}

