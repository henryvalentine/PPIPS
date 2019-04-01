
namespace ImportPermitPortal.DataObjects
{
    
    public partial class FaqObject
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public System.DateTime LastUpdated { get; set; }

        public virtual FaqCategoryObject FaqCategoryObject { get; set; }
    }
}


