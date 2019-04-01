using System;
using System.Net;
using Newtonsoft.Json;
using RestSharp;

namespace ImportPermitPortal.Helpers
{
    public static class ServiceHelper
    {
        public static bool SendSmsNotification(string phoneNumber, string text)
        {
            try
            {
                var client = new RestClient("https://api.infobip.com/sms/1/text/single");
                var request = new RestRequest(Method.POST);
                request.AddHeader("accept", "application/json");
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", "Basic cHBpcHM6MTIzcHBpcHMx");
                if (phoneNumber.StartsWith("0"))
                {
                    phoneNumber = "234" + phoneNumber.Remove(0, 1); ;
                }
                var textbody = new SmsPayLoad
                {
                    From = "DPR-PPIPS",
                    To = phoneNumber,
                    Text = text
                };

                var payload = JsonConvert.SerializeObject(textbody);

                request.AddParameter("application/json", payload, ParameterType.RequestBody);

                var response = client.Execute(request);
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}