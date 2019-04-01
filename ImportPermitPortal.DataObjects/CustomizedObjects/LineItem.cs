
using Newtonsoft.Json;

namespace ImportPermitPortal.DataObjects
{
    public class Lineitem
    {
        [JsonProperty("lineItemsId")]
        public string LineItemsId { get; set; }

        [JsonProperty("beneficiaryName")]
        public string BeneficiaryName { get; set; }

        [JsonProperty("beneficiaryAccount")]
        public string BeneficiaryAccount { get; set; }

        [JsonProperty("bankCode")]
        public string BankCode { get; set; }

        [JsonProperty("beneficiaryAmount")]
        public string BeneficiaryAmount { get; set; }

        [JsonProperty("deductFeeFrom")]
        public int DeductFeeFrom { get; set; }
    }
    
}

