using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;
using WebGrease.Css.Extensions;

namespace ImportPermitPortal.Controllers
{
    [Authorize]
    public class ThroughPutController : Controller    
    {
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
        
        [HttpPost]
        [Authorize(Roles = "Depot_Owner")]
        public ActionResult AddThroughPutByDepotOwner(ThroughPutObject throughPut)
        {
            var gVal = new GenericValidator();
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (throughPut == null)
                {
                    gVal.Error = "Invalid process call.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (throughPut.Quantity <  1)
                {
                    gVal.Error = "Please provide Quantity.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(throughPut.TempPath))
                {
                    gVal.Error = "Document processing failed. Please try again.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                var path = MoveFile(importerInfo.Id, throughPut.TempPath);
                if (string.IsNullOrEmpty(path))
                {
                    gVal.Error = "Document processing failed. Please try again.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ip = ClientIpHelper.GetClientIpAddress(Request);
                var doc = new DocumentObject
                {
                    ImporterId = importerInfo.Id,
                    DateUploaded = DateTime.Now,
                    UploadedById = importerInfo.UserProfileObject.Id,
                    DocumentPath = path,
                    DocumentTypeId = (int)SpecialDocsEnum.Throughput_agreement,
                    Status = (int) AppStatus.Pending,
                    IpAddress = ip
                };
                      
                throughPut.DocumentObject = doc;
                throughPut.IPAddress = ip;
                var docStatus = new ThroughPutServices().AddThroughPut(throughPut);
                if (docStatus < 1)
                {
                    gVal.Error = "Process failed. Please try again.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = docStatus; 
                gVal.Path = path.Replace("~", string.Empty);
                gVal.Error = "Throughput information was successfully processed.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

            catch (Exception)
            {
                gVal.Error = "ThroughPut processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Applicant")]
        public ActionResult AddThroughPutByApplicant(HttpPostedFileBase file, long throughPutId, double quantity)
        {
            var gVal = new GenericValidator();
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (throughPutId < 1 || quantity < 1)
                {
                    gVal.Error = "Invalid process call.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                var path = SaveFile2("", file, importerInfo.Id);

                if (string.IsNullOrEmpty(path))
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ip = ClientIpHelper.GetClientIpAddress(Request);
                var doc = new DocumentObject
                {
                    DateUploaded = DateTime.Now,
                    UploadedById = importerInfo.UserProfileObject.Id,
                    DocumentPath = path,
                    DocumentTypeId = (int)SpecialDocsEnum.Throughput_agreement,
                    Status = (int)AppStatus.Pending,
                    ImporterId = importerInfo.Id,
                    IpAddress = ip
                };

                var throughPut = new ThroughPutObject {DocumentObject = doc, IPAddress = ip, Id = throughPutId, Quantity = quantity};

                var docStatus = new ThroughPutServices().AddThroughPut(throughPut);
                if (docStatus < 1)
                {
                    gVal.Error = "Process failed. Please try again.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = docStatus;
                gVal.Error = path;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

            catch (Exception)
            {
                gVal.Error = "ThroughPut processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditThroughPut(HttpPostedFileBase file, long throughPutId, long documentId, double quantity)
        {
            var gVal = new GenericValidator();
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (throughPutId < 1 || quantity < 1)
                {
                    gVal.Error = "Invalid process call.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var doc = new DocumentServices().GetDocument(documentId);
                if (doc.DocumentId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document could not be processed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ip = ClientIpHelper.GetClientIpAddress(Request);
                var path = "";
                if (file != null && file.ContentLength > 0)
                {
                    path = SaveFile2(doc.DocumentPath, file, importerInfo.Id);
                    if (string.IsNullOrEmpty(path))
                    {
                        gVal.Code = -1;
                        gVal.Error = "Document Could not be processed.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }
                    
                    doc.DocumentPath = path;
                    doc.UploadedById = importerInfo.UserProfileObject.Id;
                    doc.IpAddress = ip;
                    doc.Status = (int)AppStatus.Pending;
                    doc.DateUploaded = DateTime.Now;
                    doc.IpAddress = ip;
                }

                var throughPut = new ThroughPutObject { DocumentObject = doc, IPAddress = ip, Id = throughPutId, Quantity = quantity };

                var docStatus = new ThroughPutServices().UpdateThroughPut(throughPut);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "ThroughPut information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                gVal.Code = docStatus;
                gVal.Error = path;
                Session["_throughPut"] = null;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "ThroughPut information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        } 
        
        [HttpPost]
        public ActionResult SaveTempFile(HttpPostedFileBase file)
        {
            var gVal = new GenericValidator();
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var path = PrepareAndSaveFile(file, importerInfo.Id, "");

                if (string.IsNullOrEmpty(path))
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Path = PhysicalToVirtualPathMapper.MapPath(path).Replace("~", "");
                gVal.Code = 5;
                return Json(gVal, JsonRequestBehavior.AllowGet);  
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        public string SaveFile2(string formerPath, HttpPostedFileBase file, long importerId)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    var mainPath = HostingEnvironment.MapPath("~/ImportDocuments/" + importerId.ToString(CultureInfo.InvariantCulture));
                    if (string.IsNullOrEmpty(mainPath))
                    {
                        return "";
                    }

                    if (!Directory.Exists(mainPath))
                    {
                        Directory.CreateDirectory(mainPath);
                        var dInfo = new DirectoryInfo(mainPath);
                        var dSecurity = dInfo.GetAccessControl();
                        dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                        dInfo.SetAccessControl(dSecurity);
                    }
                    var path = "";
                    if (SaveToFolder(file, ref path, mainPath, formerPath))
                    {
                        return PhysicalToVirtualPathMapper.MapPath(path);
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return string.Empty;
            }
        }

        private string PrepareAndSaveFile(HttpPostedFileBase file, long companyId, string formerFilePath)
        {
            try
            {
                var mainPath = "~/tempFiles/" + companyId.ToString(CultureInfo.InvariantCulture);
                var subPath = HostingEnvironment.MapPath(mainPath);
                if (string.IsNullOrEmpty(subPath))
                {
                    return null;
                }

                if (!Directory.Exists(subPath))
                {
                    Directory.CreateDirectory(subPath);
                    var dInfo = new DirectoryInfo(subPath);
                    var dSecurity = dInfo.GetAccessControl();
                    dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                    dInfo.SetAccessControl(dSecurity);
                }
                var path = "";
                if (SaveToFolder(file, ref path, subPath, formerFilePath))
                {
                    return PhysicalToVirtualPathMapper.MapPath(path);
                }
                return path;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return string.Empty;
            }
        }
        
        public ActionResult GetThroughPut(long applicationId)
        {
            try
            {
                var throughPut = new ThroughPutServices().GetThroughPut(applicationId);
                if (throughPut == null || throughPut.Id < 1)
                {
                    return Json(new ThroughPutObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_throughPut"] = throughPut;

                return Json(throughPut, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new ThroughPutObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteThroughPut(long id)
        {
            var gVal = new GenericValidator();
            try
            {
                if (id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Invalid selection";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                var delStatus = new ThroughPutServices().DeleteThroughPut(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Docment could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "ThroughPut Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        private static string GenerateUniqueName()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid();
        }
        private bool SaveToFolder(HttpPostedFileBase file, ref string path, string folderPath, string formerFilePath = null)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    var fileExtension = Path.GetExtension(file.FileName);
                    var fileName = GenerateUniqueName() + fileExtension;
                    var newPathv = Path.Combine(folderPath, fileName);
                    file.SaveAs(newPathv);
                    if (!string.IsNullOrWhiteSpace(formerFilePath))
                    {
                        if (!DeleteFile(formerFilePath))
                        {
                            return false;
                        }
                    }
                    path = newPathv;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return false;
            }
        }

        private string MoveFile(long importerId, string tempPath)
        {
            try
            {
                var path = HostingEnvironment.MapPath("~/ApplicationThroughPuts/" + importerId.ToString(CultureInfo.InvariantCulture));
                if (string.IsNullOrEmpty(path))
                {
                    return "";
                }
                var tmpPath = "~/tempFiles/" + importerId.ToString(CultureInfo.InvariantCulture);
                var mappedTmpPath = HostingEnvironment.MapPath(tmpPath);
                if (string.IsNullOrEmpty(mappedTmpPath))
                {
                    return "";
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    var dInfo = new DirectoryInfo(path);
                    var dSecurity = dInfo.GetAccessControl();
                    dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                    dInfo.SetAccessControl(dSecurity);
                }
                var fpth = HostingEnvironment.MapPath(tempPath);
                var fileName = Path.GetFileName(fpth);
                if (string.IsNullOrEmpty(fileName))
                {
                    return "";
                }

                var newPathv = Path.Combine(path, fileName);
                System.IO.File.Copy(fpth, newPathv);
                var dir = new DirectoryInfo(mappedTmpPath);
                var files = dir.GetFiles();
                if (files.Any())
                {
                    files.ForEach(j =>
                    {
                        System.IO.File.Delete(j.FullName);
                    });
                }

                return PhysicalToVirtualPathMapper.MapPath(newPathv);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return string.Empty;
            }
        }
        private bool DeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return false;
                }

                if (!filePath.StartsWith("~"))
                {
                    filePath = "~" + filePath;
                }

                System.IO.File.Delete(Server.MapPath(filePath));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
