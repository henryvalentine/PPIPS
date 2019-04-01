
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ApplicationCountryObject
    {
        public long Id { get; set; }
        public long ApplicationItemId { get; set; }
        public int CountryId { get; set; }

        public virtual ApplicationItemObject ApplicationItemObject { get; set; }
        public virtual CountryObject CountryObject { get; set; }
    }
}

