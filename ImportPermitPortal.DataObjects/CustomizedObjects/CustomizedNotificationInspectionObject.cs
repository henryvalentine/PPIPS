
namespace ImportPermitPortal.DataObjects
{
    using System;

    public partial class NotificationInspectionObject
    {
        public string Recommendation { get; set; }

        public string InspectionDateStr { get; set; }

        public string DepotName { get; set; }

        public string QuantityDischargedStr { get; set; }
        public string ProductName { get; set; }

        public string DischargeCommencementDateStr { get; set; }
        public string DischargeCompletionDateStr { get; set; }

        public string InspectionSubmittionDateStr { get; set; }

        public string VesselArrivalDateStr { get; set; }
      
    }
}
