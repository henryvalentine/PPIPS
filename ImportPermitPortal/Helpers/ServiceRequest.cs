using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.Models;
using Newtonsoft.Json;

namespace ImportPermitPortal.Helpers
{
    public static class ServiceRequest
    {
        public static async Task<Token> GetClientAccessToken()
        {
            var token = new Token();
            try
            {
                using(var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://dprconnect.maxfront.com/Token");
                    var request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);
                    var body = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("client_id", "3151987258"),
                            new KeyValuePair<string, string>("client_secret", "5eb493e8-9afd-491f-baa4-b9e73ef09cc1"),
                            new KeyValuePair<string, string>("grant_type", "client_credentials"),
                            new KeyValuePair<string, string>("redirect_uri", "http://ppips2.shopkeeper.ng/Account/AuthorizationCodeCallback")
                        };

                    request.Content = new FormUrlEncodedContent(body);
                    //Execute the request and handle the response
                    var transport = await client.SendAsync(request);
                    if (transport == null)
                    {
                        return token;
                    }
                    // response.EnsureSuccessStatusCode();
                    var response = await transport.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(response))
                    {
                        return token;
                    }

                    var responseToken = JsonConvert.DeserializeObject<Token>(response);

                    if (string.IsNullOrEmpty(responseToken.AccessToken))
                    {
                        return token;
                    }

                    return responseToken;
                }
            }
            catch (HttpException)
            {
                return token;
            }
        }

        public static async Task<Token> GetUserAccessToken(AccessModel model)
        {
            var token = new Token();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://dprconnect.maxfront.com/Token");

                    var request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);

                    var body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("client_id", "3151987258"),
                        new KeyValuePair<string, string>("client_secret", "5eb493e8-9afd-491f-baa4-b9e73ef09cc1"),
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("username", model.Email),
                        new KeyValuePair<string, string>("password", model.Password),
                        new KeyValuePair<string, string>("condition", model.IsExisting.ToString()),
                    };

                    request.Content = new FormUrlEncodedContent(body);
                    var transport = await client.SendAsync(request);
                    if (transport == null)
                    {
                        return token;
                    }
                    
                    var response = await transport.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(response))
                    {
                        return token;
                    }

                    var responseToken = JsonConvert.DeserializeObject<Token>(response);

                    if (string.IsNullOrEmpty(responseToken.AccessToken))
                    {
                        return token;
                    }

                    if (!model.IsExisting)
                    {
                        var extn = request.Headers.GetValues("EXN_TGD").ToList();
                        if (!extn.Any())
                        {
                            return token;
                        }
                        var usrStr = extn.ElementAt(0);
                        responseToken.EXN_TGD = usrStr;
                    }
                   
                    return responseToken;
                }
            }
            catch (HttpException)
            {
                return token;
            }
        }

        public static async Task<T> PostRequest<T>(dynamic model, string url) where T : class
        {
            try
            {
                using(var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);

                    var request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);
                    var json = JsonConvert.SerializeObject(model);
                    var body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("client_id", "3151987258"),
                        new KeyValuePair<string, string>("client_secret", "5eb493e8-9afd-491f-baa4-b9e73ef09cc1"),
                        new KeyValuePair<string, string>("rqst", json)
                    };

                    request.Content = new FormUrlEncodedContent(body);
                    var transport = await client.SendAsync(request, CancellationToken.None);
                    
                    var response = await transport.Content.ReadAsStringAsync();

                    var responseResult = JsonConvert.DeserializeObject<T>(response);
                    return responseResult;
                }
            }
            catch (HttpException)
            {
                return null;
            }
        }

        public static async Task<T> GetRequest<T>(dynamic model, string url) where T : class
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);

                    var request = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);
                    var json = JsonConvert.SerializeObject(model);
                    var body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("client_id", "3151987258"),
                        new KeyValuePair<string, string>("client_secret", "5eb493e8-9afd-491f-baa4-b9e73ef09cc1"),
                        new KeyValuePair<string, string>("rqst", json)
                    };

                    request.Content = new FormUrlEncodedContent(body);
                    var transport = await client.SendAsync(request);

                    var response = await transport.Content.ReadAsStringAsync();

                    var responseResult = JsonConvert.DeserializeObject<T>(response);
                    return responseResult;
                }
            }
            catch (HttpException)
            {
                return null;
            }
        }

        public static async Task<T> GetRequestWithAuthorization<T>(dynamic model, string accessToken, string url) where T : class
        {
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(url)
                };

                var request = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var json = JsonConvert.SerializeObject(model);
                var body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("rqst", json)
                    };

                request.Content = new FormUrlEncodedContent(body);
                var response = await client.SendAsync(request);

                var output = await response.Content.ReadAsStringAsync();

                var responseResult = JsonConvert.DeserializeObject<T>(output);
                return responseResult;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        
        public static async Task<T> PostRequestWithAuthorization<T>(dynamic model, string accessToken, string url) where T : class
        {
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(url)
                };

                var request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var json = JsonConvert.SerializeObject(model);
                var body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("rqst", json)
                    };
                 
                request.Content = new FormUrlEncodedContent(body);
                var response = await client.SendAsync(request);

                var output = await response.Content.ReadAsStringAsync();

                var responseResult = JsonConvert.DeserializeObject<T>(output);
                return responseResult;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static async Task<T> PostRequestWithAuthorization<T>(string url, string accessToken) where T : class
        {
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(url)
                };

                var request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await client.SendAsync(request);

                var output = await response.Content.ReadAsStringAsync();

                var responseResult = JsonConvert.DeserializeObject<T>(output);
                return responseResult;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static async Task<T> GetRequestWithAuthorization<T>(string url, string accessToken) where T : class
        {
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(url)
                };

                var request = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await client.SendAsync(request);

                var output = await response.Content.ReadAsStringAsync();

                var responseResult = JsonConvert.DeserializeObject<T>(output);
                return responseResult;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static async Task<string> GetCompanyAddress(long companyId, string token)
        {
            try
            {
                var address = await GetRequestWithAuthorization<string>("https://dprconnect.maxfront.com/api/ServiceRequest/GetCompanyAddress?companyId=" + companyId, token);
                return address;
            }

            catch (Exception ex)
            {
                return "";
            }
        }
    }

}

