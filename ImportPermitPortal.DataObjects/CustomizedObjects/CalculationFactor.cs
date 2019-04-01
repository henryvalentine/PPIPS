

namespace ImportPermitPortal.DataObjects
{
    public class CalculationFactor
    {
        public double Fees { get; set; }
        public double ExpenditionaryFee { get; set; }
        public bool ExpenditionaryFeeApplicable { get; set; }
        public double PriceVolumeThreshold { get; set; }
        public ImportSettingObject ImportSettingObject { get; set; }
    }
}

