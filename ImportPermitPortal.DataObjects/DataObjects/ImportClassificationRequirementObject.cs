
namespace ImportPermitPortal.DataObjects
{
    public partial class ImportClassificationRequirementObject
    {
        public int Id { get; set; }
        public int ClassificationId { get; set; }
        public int ImportStageId { get; set; }
        public int DocumentTypeId { get; set; }

        public virtual DocumentTypeObject DocumentTypeObject { get; set; }
        public virtual ImportClassObject ImportClassObject { get; set; }
        public virtual ImportStageObject ImportStageObject { get; set; }
    }
}


