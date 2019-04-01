using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public partial class DocumentTypeObject
    {
        public int StageId { get; set; }
        public int TempId { get; set; }
        public int Status { get; set; }
        public string StatusStr { get; set; }
        public long DocumentId { get; set; }
        public long ProductRequirementId { get; set; }
        public bool Uploaded { get; set; }
        public bool IsBankDoc { get; set; }
        public bool IsDepotDoc { get; set; }
        public bool IsFinLetter { get; set; }
        public string DocumentPath { get; set; }
        public bool IsFormM { get; set; }
        public bool IsNtDoc { get; set; }
    }
}

                           
                           
                               