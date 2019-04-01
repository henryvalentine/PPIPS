
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class CalculatorObject
    {
        public int Id { get; set; }
        public double Quantity { get; set; }
        public long ProductId { get; set; }

        public virtual ProductObject ProductObject { get; set; }
    }
} 
