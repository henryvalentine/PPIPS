using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public partial class CalculatorObject
    {
        public string QuantityInCountry { get; set; }
        public string DairlyConsumption { get; set; }

        public bool Good { get; set; }
        public bool Bad { get; set; }


        public double QuantityTrunkedOutInDepot { get; set; }
        public System.DateTime TrunkedOutDate { get; set; }
        public int DepotId { get; set; }
     
    }
}
         
