﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Models;
using ImportPermitPortal.Services.Services;
namespace ImportPermitPortal.Controllers
{
    public class BankBranchBulkController : Controller
    {
        [HttpPost]
        public ActionResult BulkUpload(HttpPostedFileBase file)  
        {
            var gVal = new GenericValidator();

            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var bankid = new AspNetUserServices().GetBankId(importerInfo.Id);
                if (bankid < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Error: process could not be completed. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (file.ContentLength > 0)
                {
                    var folderPath = HostingEnvironment.MapPath("~/BankBranchBulkUploads/" + importerInfo.Id.ToString(CultureInfo.InvariantCulture));
                    if (string.IsNullOrEmpty(folderPath))
                    {
                        gVal.Code = -1;
                        gVal.Error = "Folder access was denied";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                        var dInfo = new DirectoryInfo(folderPath);
                        var dSecurity = dInfo.GetAccessControl();
                        dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                        dInfo.SetAccessControl(dSecurity);
                    }

                    var fileName = file.FileName;
                    var path = Path.Combine(folderPath, fileName);
                    if (string.IsNullOrEmpty(path))
                    {
                        gVal.Code = -1;
                        gVal.Error = "Folder access was denied";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    file.SaveAs(path);

                    gVal.BankUserInfoList = new List<BankUserInfo>();

                    var msg = string.Empty;
                    var errorList = new List<long>();
                    var branches = new List<BankBranchObject>();
                    var successfulUploads = ReadExcelData(path, "Branches", ref errorList, ref msg, ref branches, importerInfo.Id);
                    var saveCount = 0;
                    if (!successfulUploads.Any())
                    {
                        gVal.Code = -1;
                        gVal.Error = msg;
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }
                   
                    branches.ForEach(s =>
                    {
                        s.BankId = bankid;
                        var tt = new BankServices().AddBankBranch(s);
                        if (tt > 0)
                        {
                            saveCount++;
                        }
                        else
                        {
                            errorList.Add(1);
                        }  
                        
                    });


                    if (errorList.Any() && saveCount > 0)
                    {
                        var feedbackMessage = saveCount + " records were successfully uploaded." +
                            "\n" + errorList.Count + " record(s) could not be uploaded due to specified/unspecified errors.";
                        if (msg.Length > 0)
                        {
                            feedbackMessage += "<br/>" + msg;
                        }

                        gVal.Code = -7;
                        gVal.Error = feedbackMessage;
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    if (saveCount != branches.Count && saveCount > 0)
                    {
                        var feedbackMessage = saveCount + " records were successfully uploaded." +
                            "\n" + (branches.Count - saveCount) + " record(s) could not be uploaded due to specified/unspecified errors.";
                        if (msg.Length > 0)
                        {
                            feedbackMessage += "<br/>" + msg;
                        }

                        gVal.Code = -7;
                        gVal.Error = feedbackMessage;
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    if (errorList.Any() && saveCount < 1)
                    {
                        var feedbackMessage = errorList.Count + " record(s) could not be uploaded due to specified/unspecified errors.";
                        ViewBag.ErrorCode = -1;

                        if (msg.Length > 0)
                        {
                            feedbackMessage += "<br/>" + msg;
                        }

                        gVal.Code = -1;
                        gVal.Error = feedbackMessage;
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    if (!errorList.Any() && saveCount > 0)
                    {
                        var feedbackMessage = saveCount + " records were successfully uploaded.";

                        if (msg.Length > 0)
                        {
                            feedbackMessage += "<br/>" + msg;
                        }

                        gVal.Code = 5;
                        gVal.Error = feedbackMessage;
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }
                }
                gVal.Code = -1;
                gVal.Error = "The selected file is invalid";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "File processing failed.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<long> ReadExcelData(string filePath, string sheetName, ref List<long> errorList, ref string msg, ref List<BankBranchObject> branches, long importerId)
        {
            if (filePath.Length < 3 || new FileInfo(filePath).Exists == false) 
            {
                msg = "Invalid Excel File Format";
                errorList = new List<long>();
                branches = new List<BankBranchObject>();
                return new List<long>();
            }

            if (sheetName.Length < 1)
            {
                msg = "Invalid Excel Sheet Name";
                errorList = new List<long>();
                branches = new List<BankBranchObject>();
                return new List<long>();
            }

            var connectionstring = string.Empty;
            switch (Path.GetExtension(filePath))
            {
                case ".xls":
                    connectionstring = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties='Excel 8.0;HDR=YES;'";
                    break;
                case ".xlsx":
                    connectionstring = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1;ImportMixedTypes=Text'";
                    break;

                case ".csv":
                    connectionstring = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties='Excel 8.0;HDR=YES;'";
                    break;
            }

            if (connectionstring == "")
            {
                msg = "Process Error! Please try again later";
                errorList = new List<long>();
                branches = new List<BankBranchObject>();
                return new List<long>();
            }

            var selectString = @"SELECT [Branch_Name],[Branch_Code] FROM [" + sheetName + "$]";
            
            var myCon = new OleDbConnection(connectionstring);
            try
            {
                if (myCon.State == ConnectionState.Closed)
                {
                    myCon.Open();
                }
                var cmd = new OleDbCommand(selectString, myCon);
                var adap = new OleDbDataAdapter(cmd);
                var ds = new DataSet();
                adap.Fill(ds);
                if (ds.Tables.Count < 1)
                {
                    msg = "Invalid  Branch Template!";
                    errorList = new List<long>();
                    branches = new List<BankBranchObject>();
                    return new List<long>();
                }
                var dv = new DataView(ds.Tables[0]);
                if (dv.Count < 1)
                {
                    msg = "Invalid Branch Template!";
                    errorList = new List<long>();
                    branches = new List<BankBranchObject>();
                    return new List<long>();
                }

                msg = string.Empty;
                var sb = new StringBuilder();
                sb.AppendLine("<table width=\"98%\" cellspacing=\"1px\" border=\"1\" cellpadding=\"2px\">");
                sb.AppendLine(string.Format("<tr><th width=\"45%\">Name</th><th width=\"55%\">Email</th><th width=\"55%\">Error</th></tr>"));
                var errorExist = false;
                var successList = new List<long>();
                var parseList = new List<BankBranchObject>();
                for (var i = 0; i < dv.Count; i++)
                {
                    var mymsg = string.Empty;
                    var country = dv[i].Row["Branch_Name"].ToString().Trim();
                    if(country.Trim().Length < 3)
                    {
                        continue;
                    }

                    var mInfo = ProcessRecord(dv[i], ref mymsg, importerId);

                    if (mInfo == null)
                    {
                        errorExist = true;
                        sb.AppendLine(mymsg.Length > 0
                                          ? string.Format("<tr border=\"1\"><td width=\"45%\">{0}</td><td width=\"55%\">{1}</td></tr>", country,
                                              mymsg)
                                          : string.Format(
                                              "<tr border=\"1\"><td width=\"45%\">{0}</td><td width=\"55%\">Unknown Error</td></tr>",
                                              country));
                        errorList.Add(1);
                        continue;
                    }
                    successList.Add(1);
                    parseList.Add(mInfo);
                }
                sb.AppendLine("</table>");
                if (errorExist)
                {
                    var sbb = new StringBuilder();
                    sbb.AppendLine("Following error occurred while loading your data template:");
                    sbb.AppendLine(sb.ToString());
                    msg = sbb.ToString();
                }

                myCon.Close();
                branches = parseList;
                return successList;
            }

            catch (Exception ex)
            {
                myCon.Close();
                msg = ex.Message;
                errorList = new List<long>();
                branches = new List<BankBranchObject>();
                return new List<long>();
            }
        }
        private BankBranchObject ProcessRecord(DataRowView dv, ref string msg, long importerId)
        {
            if (dv == null)
            {
                return null;
            }
            try
            {
                var branch = new BankBranchObject();

                var branchName = dv.Row["Branch_Name"].ToString().Trim();

                if (string.IsNullOrEmpty(branchName))
                {
                    msg = "Branch Name is empty.";
                    return null;
                }

                var branchCode = dv.Row["Branch_Code"].ToString().Trim();

                if (string.IsNullOrEmpty(branchCode))
                {
                    msg = "Branch Code is empty.";
                    return null;
                }
                branch.Name = branchName;
                branch.BranchCode = branchCode;
                return branch;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
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


        private bool SendMail(UserViewModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.SecurityStamp))
                {
                    return false;
                }

                const int type = (int)MessageEventEnum.New_User;
                const string label = "Activate your Account!";
               
                var msgBody = "";
                var msg = new MessageTemplateServices().GetMessageTempWithExpiry(type);
                if (msg.Id < 1)
                {
                    return false;
                }

                var emMs = new MessageObject
                {
                    UserId = model.Id,
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
                    msg.MessageContent = msg.MessageContent.Replace("\n", "<br/>");
                    msg.Subject = msg.Subject.Replace("\n", "<br/>");
                    msgBody = "DPR/DS/IEP/" + DateTime.Today.ToString("ddMMyyyy") + "/" + sta + "<br/><br/>";
                   
                    if (msg.MessageContent.Contains("{hours}") && msg.MessageLifeSpan > 0)
                    {
                        msg.MessageContent = msg.MessageContent.Replace("{hours}", msg.MessageLifeSpan.ToString());
                    }

                    msg.Subject = msg.Subject.Replace("{firstname lastname}", model.FirstName + " " + model.LastName);
                    msg.MessageContent = msg.MessageContent.Replace("{user}", model.LastName).Replace("{password}", model.Password);


                    msgBody += "<br/><br/>" + msg.MessageContent.Replace("{email verification link}", "<br/><a style=\"color:green; cursor:pointer\" title=\"Activate Account\" href=" + Url.Action("ConfirmEmail", "Account", new { email = model.Email, code = model.SecurityStamp, ixf = sta }, Request.Url.Scheme) + ">" + label + "</a>").Replace("\n", "<br/>");

                    msgBody += "<br/><br/>" + msg.Footer.Replace("\n", "<br/>");
                }

                if (Request.Url != null)
                {

                    #region Using SendGrid

                    var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                    var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                    if (settings == null)
                    {
                        return false;
                    }

                    var mail = new MailMessage(new MailAddress(settings.Smtp.From), new MailAddress(model.Email))
                    {
                        Subject = msg.Subject,
                        Body = msgBody,
                        IsBodyHtml = true
                    };

                    var smtp = new SmtpClient(settings.Smtp.Network.Host)
                    {
                        Credentials = new NetworkCredential(settings.Smtp.Network.UserName, settings.Smtp.Network.Password),
                        EnableSsl = true,
                        Port = settings.Smtp.Network.Port
                    };

                    smtp.Send(mail);

                    emMs.Id = sta;
                    emMs.MessageBody = msgBody;
                    emMs.Status = (int)MessageStatus.Sent;
                    var tts = new MessageServices().UpdateMessage(emMs);
                    if (tts < 1)
                    {
                        return false;
                    }

                    return true;
                    #endregion

                }

                return false;
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                return false;
            }
        }
   }
}