using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public partial class ImportRequirementObject
    {
        public string ImportStageName { get; set; }
        public string DocumentTypeName { get; set; }
        public string Requirements { get; set; }
        public List<DocumentTypeObject> DocumentTypeObjects { get; set; }
    }
}

        
                           
                               