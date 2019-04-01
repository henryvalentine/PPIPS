using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportPermitPortal.DataObjects
{
    public partial class StandardRequirementObject
    {
        public string StandardRequirementTypeName { get; set; }
        public string ImporterName { get; set; }
        public string TempPath { get; set; }
        public string ValidToStr { get; set; }
        public string ValidFromStr { get; set; }

        public string DateStr { get; set; }
    }

} 