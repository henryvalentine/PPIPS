
using System;
using Newtonsoft.Json;

namespace ImportPermitPortal.DataObjects
{
    public class PaymentResult
    {
         [JsonProperty("id")]
        public int Id { get; set; }

         [JsonProperty("rrr")]
        public string Rrr { get; set; }

        [JsonProperty("channnel")]
        public string Channnel { get; set; }

         [JsonProperty("amount")]
        public string Amount { get; set; }
        
         [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

         [JsonProperty("transactiondate")]
        public string Transactiondate { get; set; }

         [JsonProperty("debitdate")]
        public string Debitdate { get; set; }

         [JsonProperty("bank")]
        public string Bank { get; set; }

         [JsonProperty("branch")]
        public string Branch { get; set; }

         [JsonProperty("serviceTypeId")]
        public string ServiceTypeId { get; set; }

         [JsonProperty("dateSent")]
        public string DateSent { get; set; }

         [JsonProperty("dateRequested")]
        public string DateRequested { get; set; }

         [JsonProperty("orderRef")]
        public string OrderRef { get; set; }

         [JsonProperty("payerName")]
        public string PayerName { get; set; }

         [JsonProperty("payerEmail")]
        public string PayerEmail { get; set; }

         [JsonProperty("payerPhoneNumber")]
        public string PayerPhoneNumber { get; set; }

         [JsonProperty("uniqueIdentifier")]
        public string UniqueIdentifier { get; set; }
    }
}