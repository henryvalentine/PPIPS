using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;
using Newtonsoft.Json;
using WebGrease.Css.Extensions;

namespace ImportPermitPortal.Controllers
{
    /// <summary>
    /// Author: David Edokpayi
    /// Date Written:5/16/2015
    /// Finally Modified by Henry Otuadinma
    /// Date: 27/05/2015
    /// Summary Description: Webservice split payments made to DPR
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class SplitPayments : WebService
    {

        [WebMethod]
        public GenericValidator PostSplitPaymentForApplicationFee(double amount, string orderId, string payerName, string payerEmail, string payerPhone, string returnUrl, List<FeeObject> fees)
        {
            var helper = new SplitPaymentHelper();

            var mecharntId = ((long)RemitaServiceType.MecharntId).ToString();
            var apiKey = ((long)RemitaServiceType.ApiKey).ToString();
            var serviceTypeId = ((long)RemitaServiceType.Application).ToString();
            const string url = "https://login.remita.net/remita/ecomm/split/init.reg";

            //const string mecharntId = "2547916";
            //const string serviceTypeId = "4430731";
            //const string apiKey = "1946";
            //const string url = "http://www.remitademo.net/remita/ecomm/v2/init.reg";


            var hashValue = mecharntId + serviceTypeId + orderId + amount.ToString(CultureInfo.InvariantCulture) + returnUrl + apiKey;
            var hash = helper.GetHash(hashValue);
            var items = helper.ComputeApplicationPaymentSplit(amount, fees);
            var gVal = new GenericValidator();
            try
            {
                var splitPay = new SplitPayment
                {
                    MerchantId = mecharntId,
                    ServiceTypeId = serviceTypeId,
                    TotalAmount = amount.ToString(CultureInfo.InvariantCulture),
                    Hash = hash,
                    OrderId = orderId,
                    Responseurl = returnUrl,
                    PayerName = payerName,
                    PayerEmail = payerEmail,
                    PayerPhone = payerPhone,
                    LineItems = items,
                };

               
                var json = JsonConvert.SerializeObject(splitPay);
                using (var client = new WebClient())
                {
                    try
                    {
                        client.Headers[HttpRequestHeader.Accept] = "application/json";
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        var result = client.UploadString(url, "POST", json);

                        //client.UploadString(url, "POST", json);

                        result = result.Replace("jsonp", "");
                        result = result.Replace("(", "");
                        result = result.Replace(")", "");
                        var res = JsonConvert.DeserializeObject<ResponseObject>(result);
                        gVal.Code = 5;
                        gVal.Error = res.RRR;
                        gVal.Hash = hash;
                        var list = new List<PaymentDistributionSummaryObject>();
                        items.ToList().ForEach(m =>
                        {
                            double bAmount;
                            var d = double.TryParse(m.BeneficiaryAmount, out bAmount);
                            if (!d || bAmount < 1)
                            {
                                return;
                            }
                            list.Add(new PaymentDistributionSummaryObject
                            {
                                ServiceId = (int)ServiceDescriptionEnum.Import_Permit_Application_Fee,
                                Beneficiary = m.BeneficiaryName,
                                Amount = bAmount,
                                PaymentReference = res.RRR,
                                PaymentDate = DateTime.Now,
                                Status = false
                            });
                        });

                        var logStatus = new FeeServices().LogPaymentDetails(list);
                        if (logStatus < 1)
                        {
                            gVal.Code = -1;
                            gVal.Error = "An unknown error was encountered. Request could not be completed.";
                            return gVal;
                        }

                        return gVal;
                    }
                    catch (HttpRequestException e)
                    {
                        gVal.Code = -1;
                        gVal.Error = e.Message;
                        return gVal;
                    }
                }

            }
            catch (Exception e)
            {

                gVal.Code = -1;
                gVal.Error = e.Message;
                return gVal;
            }

        }

        [WebMethod]
        public GenericValidator PostSplitPaymentForNotificationFee(double amount, string orderId, string payerName, string payerEmail, string payerPhone, string returnUrl, List<FeeObject> fees, bool isExpenditionaryApplicable)
        {
            var helper = new SplitPaymentHelper();

            var mecharntId = ((long)RemitaServiceType.MecharntId).ToString();
            var serviceTypeId = (isExpenditionaryApplicable ? (long)RemitaServiceType.NotificationExpenditionary : (long)RemitaServiceType.Notification).ToString();
            var apiKey = ((long)RemitaServiceType.ApiKey).ToString();
            const string url = "https://login.remita.net/remita/ecomm/split/init.reg";


            //const string mecharntId = "2547916";
            //const string serviceTypeId = "4430731";
            //const string apiKey = "1946";
            //const string url = "http://www.remitademo.net/remita/ecomm/v2/init.reg";


            var hashValue = mecharntId + serviceTypeId + orderId + amount + returnUrl + apiKey;
            var hash = helper.GetHash(hashValue);
            var items = helper.ComputeNotificationPaymentSplit(fees);

            var gVal = new GenericValidator();
            try
            {
                var splitPay = new SplitPayment
                {
                    MerchantId = mecharntId,
                    ServiceTypeId = serviceTypeId,
                    TotalAmount = amount.ToString(CultureInfo.InvariantCulture),
                    Hash = hash,
                    OrderId = orderId,
                    Responseurl = returnUrl,
                    PayerName = payerName,
                    PayerEmail = payerEmail,
                    PayerPhone = payerPhone,
                    LineItems = items,
                };

                
                var json = JsonConvert.SerializeObject(splitPay);
                using (var client = new WebClient())
                {
                    try
                    {
                        client.Headers[HttpRequestHeader.Accept] = "application/json";
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        var result = client.UploadString(url, "POST", json);

                        result = result.Replace("jsonp", "");
                        result = result.Replace("(", "");
                        result = result.Replace(")", "");
                        var res = JsonConvert.DeserializeObject<ResponseObject>(result);

                        if (string.IsNullOrEmpty(res.RRR))
                        {
                            gVal.Code = -1;
                            gVal.Error = "Payment Reference could not be generated. Please try again later.";
                            return gVal;
                        }
                        gVal.Code = 5;
                        gVal.Error = res.RRR;
                        gVal.Hash = hash;
                        var list = new List<PaymentDistributionSummaryObject>();
                        items.ToList().ForEach(m =>
                        {
                            double bAmount;
                            var d = double.TryParse(m.BeneficiaryAmount, out bAmount);
                            if (!d || bAmount < 1)
                            {
                                return;
                            }

                            list.Add(new PaymentDistributionSummaryObject
                            {
                                ServiceId = (int)ServiceDescriptionEnum.Import_Permit_Application_Fee,
                                Beneficiary = m.BeneficiaryName,
                                Amount = bAmount,
                                PaymentReference = res.RRR,
                                PaymentDate = DateTime.Now,
                                Status = false
                            });
                        });

                        var logStatus = new FeeServices().LogPaymentDetails(list);
                        if (logStatus < 1)
                        {
                            gVal.Code = -1;
                            gVal.Error = "An unknown error was encountered. Request could not be completed.";
                            return gVal;
                        }

                        return gVal;
                    }
                    catch (Exception e)
                    {
                        gVal.Code = -1;
                        gVal.Error = e.Message;
                        return gVal;
                    }
                }
            }
            catch (Exception e)
            {
                gVal.Code = -1;
                gVal.Error = e.Message;
                return gVal;
            }

        }

         [WebMethod]
        public GenericValidator PostSplitPaymentForExpenditionaryFee(double amount, string orderId, string payerName, string payerEmail, string payerPhone, string returnUrl, FeeObject expenditionaryFee)
        {
            var helper = new SplitPaymentHelper();

            var mecharntId = ((long)RemitaServiceType.MecharntId).ToString();
            var serviceTypeId = ((long)RemitaServiceType.Expenditionary).ToString();
            var apiKey = ((long)RemitaServiceType.ApiKey).ToString();
            const string url = "https://login.remita.net/remita/ecomm/split/init.reg";


            //const string mecharntId = "2547916";
            //const string serviceTypeId = "4430731";
            //const string apiKey = "1946";
            //const string url = "http://www.remitademo.net/remita/ecomm/v2/init.reg";


            var hashValue = mecharntId + serviceTypeId + orderId + amount + returnUrl + apiKey;
            var hash = helper.GetHash(hashValue);
            var items = helper.ComputeExpenditionaryPaymentSplit(expenditionaryFee);

            var gVal = new GenericValidator();
            try
            {
                var splitPay = new SplitPayment
                {
                    MerchantId = mecharntId,
                    ServiceTypeId = serviceTypeId,
                    TotalAmount = amount.ToString(CultureInfo.InvariantCulture),
                    Hash = hash,
                    OrderId = orderId,
                    Responseurl = returnUrl,
                    PayerName = payerName,
                    PayerEmail = payerEmail,
                    PayerPhone = payerPhone,
                    LineItems = items,
                };
                
                var json = JsonConvert.SerializeObject(splitPay);
                using (var client = new WebClient())
                {
                    try
                    {
                        client.Headers[HttpRequestHeader.Accept] = "application/json";
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        var result = client.UploadString(url, "POST", json);

                        result = result.Replace("jsonp", "");
                        result = result.Replace("(", "");
                        result = result.Replace(")", "");
                        var res = JsonConvert.DeserializeObject<ResponseObject>(result);

                        if (string.IsNullOrEmpty(res.RRR))
                        {
                            gVal.Code = -1;
                            gVal.Error = "Payment Reference could not be generated. Please try again later.";
                            return gVal;
                        }
                        gVal.Code = 5;
                        gVal.Error = res.RRR;
                        gVal.Hash = hash;
                        var list = new List<PaymentDistributionSummaryObject>();
                        items.ToList().ForEach(m =>
                        {
                            double bAmount;
                            var d = double.TryParse(m.BeneficiaryAmount, out bAmount);
                            if (!d || bAmount < 1)
                            {
                                return;
                            }

                            list.Add(new PaymentDistributionSummaryObject
                            {
                                ServiceId = (int)ServiceDescriptionEnum.Import_Permit_Application_Fee,
                                Beneficiary = m.BeneficiaryName,
                                Amount = bAmount,
                                PaymentReference = res.RRR,
                                PaymentDate = DateTime.Now,
                                Status = false
                            });
                        });

                        var logStatus = new FeeServices().LogPaymentDetails(list);
                        if (logStatus < 1)
                        {
                            gVal.Code = -1;
                            gVal.Error = "An unknown error was encountered. Request could not be completed.";
                            return gVal;
                        }

                        return gVal;
                    }
                    catch (Exception e)
                    {
                        gVal.Code = -1;
                        gVal.Error = e.Message;
                        return gVal;
                    }
                }
            }
            catch (Exception e)
            {
                gVal.Code = -1;
                gVal.Error = e.Message;
                return gVal;
            }

        }
    }
}

