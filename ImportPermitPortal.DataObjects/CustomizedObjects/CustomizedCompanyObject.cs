

namespace ImportPermitPortal.DataObjects
{
    public partial class ImporterObject
    {
        public int ImportEligibilityId { get; set; }
        public string ImportEligibilityName { get; set; }
        public UserProfileObject UserProfileObject { get; set; }

        public AppCountObject AppCountObject { get; set; }

        public string StatusStr { get; set; }
        public string AddressStr { get; set; }
        public string StructureName { get; set; }
        public string BusinessCommencementDateStr { get; set; } 
    }
}  

