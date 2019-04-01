using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Models;
using ImportPermitPortal.Services.Services;
using Mandrill;
using Mandrill.Model;
using Newtonsoft.Json;
using WebGrease.Css.Extensions;

namespace ImportPermitPortal.Controllers
{
    
    public class TransactionController : Controller   
    {
        [HttpPost]
        public async Task<ActionResult> Invoice ()
        {
            var application = new ApplicationObject();

            try
            {
                var req = Request.InputStream;
                req.Seek(0, SeekOrigin.Begin);
                var json = new StreamReader(req).ReadToEnd();

                if (string.IsNullOrEmpty(json))
                {
                    return Json("Request Stream could not be read", JsonRequestBehavior.AllowGet);
                }

                List<PaymentResult> bankPaymentResults;
                try
                {
                    bankPaymentResults = JsonConvert.DeserializeObject<List<PaymentResult>>(json);
                   
                }
                catch (Exception e)
                {
                    ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                    application.Id = -1;
                    return Json(e.Message, JsonRequestBehavior.AllowGet); 
                }
                
                if (!bankPaymentResults.Any())
                {
                    return Json("Request Content could not be processed", JsonRequestBehavior.AllowGet);
                }
                
                foreach (var paymentResult in bankPaymentResults)
                {
                    double amount;
                    var pRes = double.TryParse(paymentResult.Amount, out amount);
                    if (!pRes || amount < 1)
                    {
                        return Json("Request Content could not be processed", JsonRequestBehavior.AllowGet);
                    }

                    var payLog = new PaymentLogObject
                    {
                        Type = true,
                        PaymentReference = paymentResult.OrderRef,
                        PaymentAmount = amount,
                        PaymentDate = paymentResult.Transactiondate,
                        PaymentMethod = PaymentOption.Bank.ToString().Replace("_", " "),
                        PaymentChannel = paymentResult.Channnel,
                        BankName = paymentResult.Bank,
                        Location = paymentResult.Branch,
                        CustomerName = paymentResult.PayerName,
                        XmlData = json,
                        Rrr = paymentResult.Rrr
                    };

                    var transactionInfo = new TransactionInvoiceServices().AddBankPayment(payLog);
                    if (transactionInfo.UserId < 1)
                    {
                        return Json("Transaction information Could not be updated", JsonRequestBehavior.AllowGet);
                    }

                    var stFee = "";
                    var prFee = "";
                    var expFee = "";
                    transactionInfo.FeeObjects.ForEach(k =>
                    {
                        if (k.FeeTypeId == (int)FeeTypeEnum.Statutory_Fee)
                        {
                            stFee = k.AmountStr;
                        }
                        if (k.FeeTypeId == (int)FeeTypeEnum.Processing_Fee)
                        {
                            prFee = k.AmountStr;
                        }
                        if (k.FeeTypeId == (int)FeeTypeEnum.Expeditionary)
                        {
                            expFee = k.AmountStr;
                        }
                    });


                    if (string.IsNullOrEmpty(expFee))
                    {
                        expFee = "Not Applicable";
                    }
                    var sr = "<br/><b>Reference Number</b>:" + transactionInfo.ReferenceCode +
                              "<br/><b>Payment Reference (RRR)</b>: " + transactionInfo.RRR +
                              "<br/><b>Statutory Fee</b>: " + stFee +
                              "<br/><b> Processing Fee</b>: " + prFee +
                              "<br/><b>Others- {e.g expeditionary Fee}</b>:" + expFee +
                              "<br/><b>Date Applied</b>:" + transactionInfo.DateAppliedStr;


                    var mailObj = new AppMailObject
                    {
                        UserId = transactionInfo.UserId,
                        ImporterName = transactionInfo.CompanyName,
                        Email = transactionInfo.Email,
                        MssageContent = sr,
                        Bankers = transactionInfo.Bankers,
                        PhoneNumber = transactionInfo.PhoneNumber
                    };

                    await SendMail(mailObj, transactionInfo.RRR);
                    
                }
                
                return Json("Request was successfully processed.", JsonRequestBehavior.AllowGet); 

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                application.Id = -1;
                return Json("Error! Request could not be processed.", JsonRequestBehavior.AllowGet); 
            }
        }

        public async Task<ActionResult> WebPayment()
        {
            var application = new ApplicationObject();

            try
            {
                if (Request.QueryString.HasKeys())
                {
                    var statuses = Request.QueryString.GetValues("status");
                    var status = "";
                    if (statuses != null && statuses.Any())
                    {
                        status = statuses[0];
                    }
                    else
                    {
                        application.Id = -1;
                        application.StatusStr = "Process failed. Please contact the Admin quoting your Payment Reference Code(RRR).";
                        return View(application);
                    }

                    string refCode;
                    var rrrList = Request.QueryString.GetValues("RRR");
                    var rrr = "";
                    if (rrrList != null && rrrList.Any())
                    {
                        rrr = rrrList[0];
                    }
                    var orderIds = Request.QueryString.GetValues("orderID");
                    if (orderIds != null && orderIds.Any())
                    {
                        refCode = orderIds[0];
                    }

                    else
                    {
                        application.Id = -1;
                        application.Rrr = rrr;
                        application.StatusStr = "Process failed. Please contact the Admin quoting your Payment Reference Code(RRR).";
                        return View(application);
                    }
                    
                    if (status.ToLower().Replace(" ", string.Empty) != "transactionapproved")
                    {
                        application.Id = -1;
                        application.ReferenceCode = refCode;
                        application.Rrr = rrr;
                        application.StatusStr = status;
                        return View(application);
                    }

                    application = new TransactionInvoiceServices().AddWebPayment(refCode);
                    if (application.Id < 1)
                    {
                        application.Id = -1;

                        application.StatusStr = "An error was encountered while updating your Payment information.";
                        return View(application);
                    }
                   
                    var sr = "<br/><b>Reference Number</b>:" + application.ReferenceCode +
                             "<br/><b>Payment Reference (RRR)</b>: " + application.Rrr;

                               application.FeeObjects.ForEach(k =>
                               {
                                   sr += "<br/><b>" + k.FeeTypeName + "</b>: " + k.AmountStr;

                               });

                           sr += "<br/><b>Date Applied</b>:" + application.DateAppliedStr;


                    var mailObj = new AppMailObject
                    {
                        UserId = application.UserId,
                        ImporterName = application.CompanyName,
                        Email = application.Email,
                        MssageContent = sr,
                        Bankers = application.Bankers
                    };

                   await SendMail(mailObj, application.Rrr);

                    return View(application);
                }
                application.Id = -1;
                application.StatusStr = "An error was encountered while updating your Payment information. Please contact the Admin, quoting your Payment reference code(RRR).";
                return View(application);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                application.Id = -1;
                application.StatusStr = "An error was encountered while updating your Payment information. Please contact the Admin, quoting your Payment reference code(RRR).";
                return View(application);
            }
        }

        public ActionResult RRRResponse()
        {
            try
            {
               var data = Request.QueryString;
                ErrorLogger.LoggError("Remita", "RRR Response", data.ToString());
                return View(data.ToString());
            }
            catch (Exception ex)
            {
                var error = "Not Generated";
                return View(error);
            }
        }

        public async Task<ActionResult> WebPayment2()
        {
            var notification = new NotificationObject();

            try
            {
                if (Request.QueryString.HasKeys())
                {
                    var statuses = Request.QueryString.GetValues("status");
                    var status = "";
                    if (statuses != null && statuses.Any())
                    {
                        status = statuses[0];
                    }
                    else
                    {
                        notification.StatusStr = status;
                        notification.Id = -1;
                        return View(notification);
                    }

                    string refCode;
                    var orderIds = Request.QueryString.GetValues("orderID");
                    if (orderIds != null && orderIds.Any())
                    {
                        refCode = orderIds[0];
                    }

                    else
                    {
                        notification.StatusStr = status;
                        notification.Id = -1;
                        return View(notification);
                    }
                    
                    if (status.ToLower().Replace(" ", string.Empty) != "transactionapproved")
                    {
                        notification.Id = -1;
                        notification.StatusStr = status;
                        return View(notification);
                    }

                    notification = new TransactionInvoiceServices().AddNotificationWebPayment(refCode);
                    if (notification.Id < 1)
                    {
                        notification.Id = -1;
                        notification.StatusStr = "An error was encountered while updating your Payment information. Please contact the Admin, quoting your Payment reference code(RRR).";
                        return View(notification);
                    }

                    var stFee = "";
                    var prFee = "";
                    var expFee = "";
                    notification.FeeObjects.ForEach(k =>
                    {
                        if (k.FeeTypeId == (int)FeeTypeEnum.Statutory_Fee)
                        {
                            stFee = k.AmountStr;
                        }
                        if (k.FeeTypeId == (int)FeeTypeEnum.Processing_Fee)
                        {
                            prFee = k.AmountStr;
                        }
                        if (k.FeeTypeId == (int)FeeTypeEnum.Expeditionary)
                        {
                            expFee = k.AmountStr;
                        }
                    });


                    if (string.IsNullOrEmpty(expFee))
                    {
                        expFee = "Not Applicable";
                    }
                    var sr = "<br/><b>Reference Number</b>:" + notification.ReferenceCode +
                              "<br/><b>Payment Reference (RRR)</b>: " + notification.Rrr +
                              "<br/><b>Statutory Fee</b>: " + stFee +
                              "<br/><b> Processing Fee</b>: " + prFee +
                              "<br/><b>Others- {e.g expeditionary Fee}</b>:" + expFee +
                              "<br/><b>Date Applied</b>:" + notification.DateCreatedStr;


                    var mailObj = new AppMailObject
                    {
                        UserId = notification.UserId,
                        ImporterName = notification.ImporterName,
                        Email = notification.Email,
                        MssageContent = sr,
                        Bankers = notification.Bankers
                    };

                   await SendMail(mailObj, notification.Rrr);

                    return View(notification);
                }
                notification.StatusStr = "An error was encountered while updating your Payment information. Please contact the Admin, quoting your Payment reference code(RRR).";
                notification.Id = -1;
                return View(notification);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                notification.Id = -1;
                notification.StatusStr = "An error was encountered while updating your Payment information. Please contact the Admin, quoting your Payment reference code(RRR).";
                return View(notification);
            }
        }

        public ActionResult CheckInvoiceValidity(string rrrCode, string paymentDate)
        {
            var status = new TransactionInvoiceServices().CheckRrrValidity(rrrCode, paymentDate);

            // { "response" : "ok"}, { "response" : "inactive"}, { "response" : "internal server error"}
           
            if (status < 1)
            {
                if (status == -4)
                {
                    return Json(JsonConvert.SerializeObject(new ValidityResponse { Response = "inactive" }), JsonRequestBehavior.AllowGet);
                }

                return Json(JsonConvert.SerializeObject(new ValidityResponse { Response = "internal server error" }), JsonRequestBehavior.AllowGet);  
            }

            return Json(JsonConvert.SerializeObject(new ValidityResponse { Response = "ok" }), JsonRequestBehavior.AllowGet);
        }
        
        private async Task<bool> SendMail(AppMailObject model, string refCode)
        {
            try
            {
                if (model == null || model.UserId < 1)
                {
                    return false;
                }

                const int type = (int)MessageEventEnum.Payment_Receipt;
                
                var msgBody = "";
                var msg = new MessageTemplateServices().GetMessageTemp(type);
                if (msg.Id < 1)
                {
                    return false;
                }
                
                var emMs = new MessageObject
                {
                    UserId = model.UserId,
                    MessageTemplateId = msg.Id,
                    Status = (int)MessageStatus.Pending,
                    DateSent = DateTime.Now,
                    MessageBody = msg.MessageContent
                };

                var sta = new MessageServices().AddMessage(emMs);
                if (sta < 1)
                {
                    return false;
                }

                if (Request.Url != null)
                {
                    msg.Subject = msg.Subject.Replace("{PaymentReference}", refCode).Replace("\n", "<br/>");
                    msgBody = "DPR/DS/IEP/" + DateTime.Today.ToString("dd/MM/yyyy") + "/" + sta + "<br/><br/>";

                    msg.MessageContent = msg.MessageContent.Replace("{PaymentReference}", model.PaymentReference);

                    msgBody += msg.MessageContent.Replace("{paymentInformation}", model.MssageContent).Replace("{referenceNo}", model.ReferenceNumber).Replace("\n", "<br/>");

                    msgBody += "<br/><br/>" + msg.Footer.Replace("\n", "<br/>");
                }

                if (Request.Url != null)
                {
                    var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                    var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                    var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                    var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                    if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                    {
                        return false;
                    }

                    #region Using Mandrill
                    var api = new MandrillApi(apiKey);
                    var receipint = new List<MandrillMailAddress> { new MandrillMailAddress(model.Email) };
                    var message = new MandrillMessage()
                    {
                        AutoHtml = true,
                        To = receipint,
                        FromEmail = settings.Smtp.From,
                        FromName = appName,
                        Subject = msg.Subject,
                        Html = msgBody
                    };

                    var result = await api.Messages.SendAsync(message);

                    if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                    {
                        emMs.Status = (int)MessageStatus.Failed;
                    }
                    else
                    {
                        emMs.Status = (int)MessageStatus.Sent;
                    }

                    #endregion
                    ServiceHelper.SendSmsNotification(model.PhoneNumber, msg.Subject);
                    emMs.Id = sta;
                    emMs.MessageBody = msgBody;
                    var tts = new MessageServices().UpdateMessage(emMs);
                    if (tts < 1)
                    {
                        return false;
                    }

                    await NotifyBankers(model.Bankers, model.PaymentReference, model.ImporterName);

                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                return false;
            }
        }


        private async Task<bool> NotifyBankers(List<BankObject> bankers, string refCode, string companyName)
        {
            try
            {
                if (!bankers.Any())
                {
                    return false;
                }
                const int documentSupport = (int)MessageEventEnum.Document_Support;

                var support = new MessageTemplateServices().GetMessageTemp(documentSupport);
                if (support.Id < 1)
                {
                    return false;
                }

                var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                {
                    return false;
                }

                var successCount = 0;

                foreach(var g in bankers)
                {
                    var emMs = new MessageObject
                    {
                        UserId = g.UserProfileId,
                        MessageTemplateId = support.Id,
                        Status = (int)MessageStatus.Pending,
                        DateSent = DateTime.Now,
                        MessageBody = support.MessageContent
                    };

                    var sta = new MessageServices().AddMessage(emMs);
                    if (sta < 1)
                    {
                        return false;
                    }

                    var msgBody = "";
                    if (Request.Url != null)
                    {

                        msgBody = "DPR/DS/IEP/" + DateTime.Today.ToString("dd/MM/yyyy") + "/" + sta + "<br/><br/>";

                        support.MessageContent = support.MessageContent.Replace("{companyName}", companyName).Replace("{reference}", refCode).Replace("{products}", g.ProductCode).Replace("\n", "<br/>");
                        
                        msgBody += "<br/><br/>" + support.Footer.Replace("\n", "<br/>");

                    }

                    if (Request.Url != null)
                    {
                       
                        #region Using Mandrill
                        var api = new MandrillApi(apiKey);
                        var receipint = new List<MandrillMailAddress> { new MandrillMailAddress(g.Email),  new MandrillMailAddress(g.NotificationEmail) };
                        var message = new MandrillMessage()
                        {
                            AutoHtml = true,
                            To = receipint,
                            FromEmail = settings.Smtp.From,
                            FromName = appName,
                            Subject = support.Subject,
                            Html = msgBody
                        };

                        var result = await api.Messages.SendAsync(message);

                        if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                        {
                            emMs.Status = (int)MessageStatus.Failed;
                        }
                        else
                        {
                            emMs.Status = (int)MessageStatus.Sent;
                        }

                        #endregion

                        emMs.Id = sta;
                        emMs.MessageBody = msgBody;
                        var tts = new MessageServices().UpdateMessage(emMs);
                        if (tts < 1)
                        {
                            return false;
                        }
                        successCount++;
                    }
                }

                return bankers.Count == successCount;
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                return false;
            }
        }

        [HttpGet]
        [Authorize(Roles = "Applicant")]
        public ActionResult GetMyInvoices(JQueryDataTableParamModel param)
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<InvoiceObject>(), JsonRequestBehavior.AllowGet);
                }
                IEnumerable<InvoiceObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new InvoiceServices().GetInvoices (param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new InvoiceServices().Search(param.sSearch, importerInfo.Id);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<InvoiceObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<InvoiceObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.RRR : sortColumnIndex == 2 ? c.DateAddedStr :
                  sortColumnIndex == 3 ? c.TotalAmountDueStr : sortColumnIndex == 4 ? c.PaymentOption : sortColumnIndex == 4 ? c.ServiceDescription : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.RRR, c.DateAddedStr, c.TotalAmountDueStr,
                                 c.PaymentOption, c.ServiceDescription, c.StatusStr
                             };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = countG,
                    iTotalDisplayRecords = countG,
                    aaData = result
                },
                   JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<InvoiceObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Applicant")]
        public ActionResult GetMyReceipts(JQueryDataTableParamModel param)
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<PaymentReceiptObject>(), JsonRequestBehavior.AllowGet);
                }
                IEnumerable<PaymentReceiptObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new TransactionInvoiceServices().GetMyReceipts(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new TransactionInvoiceServices().SearchMyReceipts(param.sSearch, importerInfo.Id);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<PaymentReceiptObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<PaymentReceiptObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.RRR : sortColumnIndex == 2 ? c.DateAddedStr :
                  sortColumnIndex == 3 ? c.TotalAmountDueStr : sortColumnIndex == 4 ? c.PaymentOption : sortColumnIndex == 4 ? c.ServiceDescription : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.RRR, c.DateAddedStr, c.TotalAmountDueStr,
                                 c.PaymentOption, c.ServiceDescription, c.StatusStr
                             };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = countG,
                    iTotalDisplayRecords = countG,
                    aaData = result
                },
                   JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<PaymentReceiptObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetReceipts(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<PaymentReceiptObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new TransactionInvoiceServices().GetReceipts(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new TransactionInvoiceServices().SearchReceipts(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<PaymentReceiptObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<PaymentReceiptObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReceiptNoStr : sortColumnIndex == 2 ? c.RRR : sortColumnIndex == 3 ? c.ImporterName : sortColumnIndex == 4 ?  c.TotalAmountPaidStr : sortColumnIndex == 5 ? c.DateAddedStr :
                  sortColumnIndex == 6 ? c.PaymentOption : c.ServiceDescription);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),c.ReceiptNoStr,
                                 c.RRR, c.ImporterName, c.TotalAmountPaidStr, c.DateAddedStr,
                                 c.PaymentOption, c.ServiceDescription
                             };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = countG,
                    iTotalDisplayRecords = countG,
                    aaData = result
                },
                   JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<PaymentReceiptObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,Support,DownstreamDirector")]
        public ActionResult GetInvoices(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<InvoiceObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new InvoiceServices().GetInvoices(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new InvoiceServices().Search(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<InvoiceObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<InvoiceObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ImporterName : sortColumnIndex == 2 ? c.RRR : sortColumnIndex == 3 ? c.DateAddedStr :
                  sortColumnIndex == 4 ? c.TotalAmountDueStr : sortColumnIndex == 5 ? c.PaymentOption : sortColumnIndex == 6 ? c.ServiceDescription : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.RRR, c.ImporterName, c.DateAddedStr, c.TotalAmountDueStr,
                                 c.PaymentOption, c.ServiceDescription, c.StatusStr
                             };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = countG,
                    iTotalDisplayRecords = countG,
                    aaData = result
                },
                   JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<InvoiceObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,Support,DownstreamDirector")]
        public ActionResult GetPaidInvoices(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<InvoiceObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new InvoiceServices().GetPaidInvoices(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new InvoiceServices().SearchPaidInvoices(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<InvoiceObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<InvoiceObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ImporterName : sortColumnIndex == 2 ? c.RRR : sortColumnIndex == 3 ? c.DateAddedStr :
                  sortColumnIndex == 4 ? c.TotalAmountDueStr : sortColumnIndex == 5 ? c.PaymentOption : sortColumnIndex == 6 ? c.ServiceDescription : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.ImporterName,
                                 c.RRR, c.DateAddedStr, c.TotalAmountDueStr,
                                 c.PaymentOption, c.ServiceDescription, c.StatusStr
                             };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = countG,
                    iTotalDisplayRecords = countG,
                    aaData = result
                },
                   JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<InvoiceObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpGet]
        [Authorize(Roles = "Applicant")]
        public ActionResult GetMyInvoice(long id)
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new InvoiceObject(), JsonRequestBehavior.AllowGet);
                }
              
                var invoice = new InvoiceServices().GetInvoice(id, importerInfo.Id);
                if (invoice == null || invoice.Id < 1)
                {
                    return Json(new InvoiceObject(), JsonRequestBehavior.AllowGet);
                }
                return Json(invoice, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<InvoiceObject>(), JsonRequestBehavior.AllowGet);
            }
        }

         [Authorize(Roles = "Applicant")]
        public ActionResult GetMyReceipt(long id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new PaymentReceiptObject(), JsonRequestBehavior.AllowGet);
                }
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new PaymentReceiptObject(), JsonRequestBehavior.AllowGet);
                }

                var receipt = new InvoiceServices().GetReceipt(id, importerInfo.Id);
                if (receipt == null || receipt.Id < 1)
                {
                    return Json(new PaymentReceiptObject(), JsonRequestBehavior.AllowGet);
                }
                return Json(receipt, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<PaymentReceiptObject>(), JsonRequestBehavior.AllowGet);
            }
        }

         [Authorize(Roles = "Super_Admin,Support,Support,DownstreamDirector")]
        public ActionResult GetReceipt(long id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new PaymentReceiptObject(), JsonRequestBehavior.AllowGet);
                }
                var receipt = new InvoiceServices().GetReceiptInfo(id);
                if (receipt == null || receipt.Id < 1)
                {
                    return Json(new PaymentReceiptObject(), JsonRequestBehavior.AllowGet);
                }
                return Json(receipt, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<PaymentReceiptObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,Support,DownstreamDirector")]
        public ActionResult GetInvoice(long invoiceId)
        {
            try
            {
                if (invoiceId < 1)
                {
                    return Json(new InvoiceObject(), JsonRequestBehavior.AllowGet);
                }

                var invoice = new InvoiceServices().GetInvoice(invoiceId);
                if (invoice == null || invoice.Id < 1)
                {
                    return Json(new InvoiceObject(), JsonRequestBehavior.AllowGet);
                }
                return Json(invoice, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<InvoiceObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        
        private ImporterObject GetLoggedOnUserInfo()
        {
            try
            {
                if (Session["_importerInfo"] == null)
                {
                    return new ImporterObject();
                }

                var importerInfo = Session["_importerInfo"] as ImporterObject;
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return new ImporterObject();
                }

                return importerInfo;

            }
            catch (Exception)
            {
                return new ImporterObject();
            }
        }
        
        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support")]
        public async Task<ActionResult> GetTransactionDetails(string rrr)
        {
            var rrrRes = new RrrResponse();
            try
            {
                const string merchantId = "442773233";
                var hash = Hasher.GenerateHashStatus(rrr);
                 
                var url = "https://login.remita.net/remita/ecomm/" + merchantId + "/" + rrr + "/" + hash + "/status.reg";

                using (var httpClient = new HttpClient())
                {
                    try
                    {
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var rrt = await httpClient.GetAsync(url);
                        var result = await rrt.Content.ReadAsStringAsync();
                        result = result.Replace("jsonp", "");
                        result = result.Replace("(", "");
                        result = result.Replace(")", "");
                        if (result.Contains('<'))
                        {
                           rrrRes.Message = "An internal server error was encountered. Please try again later.";
                           return Json(new RrrResponse(), JsonRequestBehavior.AllowGet);
                        }

                        var res = JsonConvert.DeserializeObject<RrrResponse>(result);
                        if (string.IsNullOrEmpty(res.Rrr) || string.IsNullOrEmpty(res.Transactiontime))
                        {
                            rrrRes.Message = !string.IsNullOrEmpty(res.Message) ? res.Message : "Transaction Details could not be retrieved. Please try again.";
                            return Json(new RrrResponse(), JsonRequestBehavior.AllowGet); 
                        }

                        var name = Enum.GetName(typeof(RemitaTransactionCodes), res.Status);
                        if (name != null)
                        {
                            rrrRes.Message = name.Replace("_", " ");
                        }

                        if (string.IsNullOrEmpty(res.Message))
                        {
                            if (res.Status == 1)
                            {
                                res.Message = "Transaction Approved";
                            }

                            if (res.Status == 0)
                            {
                                res.Message = "Transaction Completed Successfully";
                            }
                        } 


                        return Json(res, JsonRequestBehavior.AllowGet);
                    }

                    catch (HttpRequestException e)
                    {
                        return Json(new InvoiceObject(), JsonRequestBehavior.AllowGet);
                    }
                }

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new InvoiceObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support")]
        public async Task<ActionResult> InsertPayment(string rrr, string orderId)
        {
            var gVal = new GenericValidator();

            try
            {
                if (string.IsNullOrEmpty(rrr) || string.IsNullOrEmpty(orderId))
                {
                    gVal.Code = -1;
                    gVal.Error = "Invalid request";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var application = new TransactionInvoiceServices().InsertPayment(rrr, orderId);
                if (application.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "An error was encountered while updating Payment information";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var sr = "<br/><b>Reference Number</b>:" + application.ReferenceCode +
                         "<br/><b>Payment Reference (RRR)</b>: " + application.RRR;

                application.FeeObjects.ForEach(k =>
                {
                    sr += "<br/><b>" + k.FeeTypeName + "</b>: " + k.AmountStr;
                });

                sr += "<br/><b>Date Applied</b>:" + application.DateAppliedStr;

                var mailObj = new AppMailObject
                {
                    UserId = application.UserId,
                    ImporterName = application.CompanyName,
                    Email = application.Email,
                    MssageContent = sr
                };

               await SendMail(mailObj, application.RRR);

                gVal.Code = 5;
                gVal.Error = "Payment successfull updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "An error was encountered while updating Payment information";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support")]
        public ActionResult GenerateRrr(long id)
        {
            var gVal = new GenericValidator();
            try
            {
                if (id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Invalid request.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                var app = new ApplicationServices().GetRrrInfo(id);
                if (app == null || app.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Invoice information could not be retrieved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (app.Status < 2)
                {
                    var url = "";
                    if (app.PaymentTypeId == (int)PaymentType.Bank)
                    {
                        url = "http://ppips2.shopkeeper.ng/Transaction/Invoice";
                    }
                    else
                    {
                        url = "http://ppips2.shopkeeper.ng/Transaction/WebPayment";
                    }

                    if (string.IsNullOrEmpty(app.RRR))
                    {
                        var rrr =  new SplitPayments().PostSplitPaymentForApplicationFee(app.TotalAmountDue, app.ReferenceCode, app.ImporterName, app.Email, app.PhoneNumber, url, app.FeeObjects);
                        if (rrr.Code < 1 || rrr.Error.Contains("<"))
                        {
                            gVal.Code = -1;
                            gVal.Error = "RRR could not be generated from the payment gateway.";
                            return Json(gVal, JsonRequestBehavior.AllowGet);
                        }
                       
                        new InvoiceServices().UpdateInvoiceRrr(app.ReferenceCode, rrr.Error);
                        gVal.Code = 5;
                        gVal.Error = "RRR : " + rrr.Error + "  ";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                }

                gVal.Code = -1;
                gVal.Error = "The selected Invoice does not need an RRR.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }
        }
        
        //[Authorize(Roles = "Accounts")]
        //public ActionResult GetPaymentCounts()
        //{
        //    try
        //    {
        //        var receipt = new TransactionInvoiceServices().GetMyReceipt(id, importerInfo.Id);
        //        if (receipt == null || receipt.Id < 1)
        //        {
        //            return Json(new PaymentReceiptObject(), JsonRequestBehavior.AllowGet);
        //        }
        //        return Json(receipt, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return Json(new List<PaymentReceiptObject>(), JsonRequestBehavior.AllowGet);
        //    }
        //}
    }
}
