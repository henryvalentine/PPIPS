using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Services.Services;
using Newtonsoft.Json;

namespace ImportPermitPortal.Controllers
{
    public class DepotLicenseBulkToJsonController : Controller
    {
        public ActionResult BulkUpload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult BulkUpload(HttpPostedFileBase file)
        {
            var gVal = new GenericValidator();

            try
            {
                if (file.ContentLength > 0)
                {
                    const string folderPath = "~/BulkUploads";

                    var fileName = file.FileName;
                    var path = Server.MapPath(folderPath + "/" + fileName);

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    file.SaveAs(path);

                    var filePath = Server.MapPath(folderPath + "/" + "depotLicenses.json");

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    var msg = string.Empty;
                    var errorList = new List<long>();
                    var permits = new List<LicenseRefObject>();
                    var successfulUploads = ReadExcelData(path, "DepotLicences", ref errorList, ref msg, ref permits);
                    var saveCount = 0;
                    if (!successfulUploads.Any())
                    {
                        gVal.Code = -1;
                        gVal.Error = msg;
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    using (var fs = System.IO.File.Open(filePath, FileMode.CreateNew))

                    using (var sw = new StreamWriter(fs))

                    using (var jw = new JsonTextWriter(sw))
                    {
                        jw.Formatting = Formatting.Indented;
                        var serializer = new JsonSerializer();
                        serializer.Serialize(jw, permits.ToArray());
                        saveCount = permits.Count;
                    }
                    
                    if (errorList.Any() && successfulUploads.Any())
                    {
                        var feedbackMessage = successfulUploads.Count + " records were successfully uploaded." +
                            "\n" + errorList.Count + " record(s) could not be uploaded due to specified/unspecified errors.";
                        if (msg.Length > 0)
                        {
                            feedbackMessage += "<br/>" + msg;
                        }

                        gVal.Code = -1;
                        gVal.Error = feedbackMessage;
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    if (saveCount != permits.Count)
                    {
                        var feedbackMessage = saveCount + " records were successfully uploaded." +
                            "\n" + (permits.Count - saveCount) + " record(s) could not be uploaded due to specified/unspecified errors.";
                        if (msg.Length > 0)
                        {
                            feedbackMessage += "<br/>" + msg;
                        }

                        gVal.Code = -1;
                        gVal.Error = feedbackMessage;
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    if (errorList.Any() && !successfulUploads.Any())
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

                    if (!errorList.Any() && successfulUploads.Any())
                    {
                        var feedbackMessage = successfulUploads.Count + " records were successfully uploaded.";

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

        private List<long> ReadExcelData(string filePath, string sheetName, ref List<long> errorList, ref string msg, ref List<LicenseRefObject> permits)
        {
            if (filePath.Length < 3 || new FileInfo(filePath).Exists == false) 
            {
                msg = "Invalid Excel File Format";
                errorList = new List<long>();
                permits = new List<LicenseRefObject>();
                return new List<long>();
            }


            if (sheetName.Length < 1)
            {
                msg = "Invalid Excel Sheet Name";
                errorList = new List<long>();
                permits = new List<LicenseRefObject>();
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
                permits = new List<LicenseRefObject>();
                return new List<long>();
            }

            var selectString = @"SELECT [Company],[License_Code],[Jetty] FROM [" + sheetName + "$]";
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
                    msg = "Invalid  License Template!";
                    errorList = new List<long>();
                    permits = new List<LicenseRefObject>();
                    return new List<long>();
                }
                var dv = new DataView(ds.Tables[0]);
                if (dv.Count < 1)
                {
                    msg = "Invalid Call Destination Template!";
                    errorList = new List<long>();
                    permits = new List<LicenseRefObject>();
                    return new List<long>();
                }

                msg = string.Empty;
                var sb = new StringBuilder();
                sb.AppendLine("<table width=\"98%\" cellspacing=\"1px\" border=\"1\" cellpadding=\"2px\">");
                sb.AppendLine(string.Format("<tr><th width=\"45%\">Company</th><th width=\"55%\">Error</th></tr>"));
                var errorExist = false;
                var successList = new List<long>();
                var parseList = new List<LicenseRefObject>();
                for (var i = 0; i < dv.Count; i++)
                {
                    var mymsg = string.Empty;
                    var country = dv[i].Row["Company"].ToString().Trim();
                    if(country.Trim().Length < 3)
                    {
                        continue;
                    }

                    var mInfo = ProcessRecord(dv[i], ref mymsg);

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
                permits = parseList;
                return successList;
            }

            catch (Exception ex)
            {
                myCon.Close();
                msg = ex.Message;
                errorList = new List<long>();
                permits = new List<LicenseRefObject>();
                return new List<long>();
            }
        }
        private LicenseRefObject ProcessRecord(DataRowView dv, ref string msg)
        {
            if (dv == null)
            {
                return null;
            }
            try
            {
                var depotLicense = new LicenseRefObject();

                var companyName = dv.Row["Company"].ToString().Trim();

                if (string.IsNullOrEmpty(companyName))
                {
                    msg = "Company Name is empty.";
                    return null;
                }

                depotLicense.ImporterName = companyName;

                var refCode = dv.Row["License_Code"].ToString().Trim();
                if (string.IsNullOrEmpty(refCode))
                {
                    msg = "License Number is empty.";
                    return null;
                }

                var jettyName = dv.Row["Jetty"].ToString().Trim();
                if (string.IsNullOrEmpty(jettyName))
                {
                    msg = "Jetty Name is empty.";
                    return null;
                }

                var jettyId = new JettyServices().GetJettyIdByName(jettyName);
                if (jettyId < 1)
                {
                    msg = "Jetty Information could not be verified.";
                    return null;
                }
               
                depotLicense.ExpiryDate = DateTime.Parse("31/12/2015");
                depotLicense.IssueDate = DateTime.Parse("01/01/2015");
                depotLicense.Status = true;
                depotLicense.RefCode = refCode;
                depotLicense.JettyName = jettyName;
                depotLicense.JettyId = jettyId;
                return depotLicense;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }

        }

   }
}