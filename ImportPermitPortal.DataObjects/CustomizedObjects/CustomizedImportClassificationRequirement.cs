using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public partial class ImportClassificationRequirementObject
    {
        public string ImportClassName { get; set; }
        public string Requirements { get; set; }
        public string ImportStageName { get; set; }
        public List<DocumentTypeObject> DocumentTypeObjects { get; set; }
    }
}

        
                           
  