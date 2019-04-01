

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class StepObject
    {
       
        public string ProcessName { get; set; }
     
        public string GroupName { get; set; }
        public string StatusName { get; set; }
        public string ActivityTypeName { get; set; }

        public int PreviousStepSequence { get; set; }

        public int ImportStageId { get; set; }
        public string IsFinalValue { get; set; }
        public int PreviousStepId { get; set; }
        public string PreviousStepName { get; set; }
        public string ImportStageName { get; set; }
        public string ExpectedDeliveryDurationStr { get; set; }
        public bool IsCurrentStep { get; set; }
        public bool IsStepProcessed { get; set; }
        public string IsLocationRequiredStr { get; set; }
    }
}
