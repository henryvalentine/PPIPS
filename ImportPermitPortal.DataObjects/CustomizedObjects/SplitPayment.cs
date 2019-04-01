
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ImportPermitPortal.DataObjects
{
   public class SplitPayment
    {
         [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

          [JsonProperty("serviceTypeId")]
        public string ServiceTypeId { get; set; }

          [JsonProperty("totalAmount")]
        public string TotalAmount { get; set; }

          [JsonProperty("hash")]
        public string Hash { get; set; }

          [JsonProperty("payerName")]
        public string PayerName { get; set; }

          [JsonProperty("payerEmail")]
        public string PayerEmail { get; set; }

          [JsonProperty("payerPhone")]
        public string PayerPhone { get; set; }

          [JsonProperty("orderId")]
        public string OrderId { get; set; }

          [JsonProperty("responseurl")]
        public string Responseurl { get; set; }

          [JsonProperty("lineItems")]
          public Lineitem[] LineItems { get; set; }
    }

  
   public class RrrResponse
   {
       [JsonProperty("message")]
       public string Message { get; set; }

       [JsonProperty("status")]
       public int Status { get; set; }

       [JsonProperty("RRR")]
       public string Rrr { get; set; }

       [JsonProperty("orderId")]
       public string OrderId { get; set; }

       [JsonProperty("transactiontime")]
       public string Transactiontime { get; set; }

   }  
}





