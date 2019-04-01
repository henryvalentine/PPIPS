
namespace ImportPermitPortal.DataObjects
{
    public partial class PageObject
    {
        public int Id { get; set; }
        public string Alias { get; set; }
        public string Title { get; set; }
        public string PageContent { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public long LastedUpdatedById { get; set; }

        public virtual UserProfileObject UserProfileObject { get; set; }
    }
}

