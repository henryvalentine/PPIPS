
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class ApplicationContentObject
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string BodyContent { get; set; }
        public string Href { get; set; }
        public bool IsInUse { get; set; }
    }

    public partial class ApplicationContentObject
    {
        public string IsInUseStr { get; set; }
    }
}

