
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class ProductColumnObject
    {
        public int ProductColumnId { get; set; }
        public long ProductId { get; set; }
        public int CustomCodeId { get; set; }

        public virtual CustomCodeObject CustomCodeObject { get; set; }
        public virtual ProductObject ProductObject { get; set; }
    }
}
