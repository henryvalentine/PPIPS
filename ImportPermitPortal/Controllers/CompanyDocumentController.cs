using System;
using System.Collections.Generic;
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
    public class CompanyDocumentController : Controller
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

        private string GetAccessToken()
        {
            try
            {
                if (Session["_token"] == null)
                {
                    return "";
                }

                var token = (string)Session["_token"];
                if (string.IsNullOrEmpty(token))
                {
                    return "";
                }

                return token;

            }
            catch (Exception)
            {
                return "";
            }
        }
        
        [HttpGet]
        public ActionResult GetStandardRequirement(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<StandardRequirementObject> filteredParentMenuObjects;
                int countG;
                //var token = GetAccessToken();
                //if (string.IsNullOrEmpty(token))
                //{
                //    return Json(new List<StandardRequirementObject>(), JsonRequestBehavior.AllowGet);
                //}
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    return Json(new List<StandardRequirementObject>(), JsonRequestBehavior.AllowGet);
                }

                var pagedParentMenuObjects = new StandardRequirementServices().GetStandardRequirements(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);
                if (pagedParentMenuObjects == null)
                {
                    return Json(new List<StandardRequirementObject>(), JsonRequestBehavior.AllowGet);
                }
                
                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new StandardRequirementServices().Search(param.sSearch, importerInfo.Id);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<StandardRequirementObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<StandardRequirementObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Title : sortColumnIndex == 2 ? c.StandardRequirementTypeName : sortColumnIndex == 3 ? c.ValidFromStr : c.ValidToStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;
                 
                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.Title, c.StandardRequirementTypeName, c.ValidFromStr, c.ValidToStr };
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
                return Json(new List<FeeObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetStandardReqTypes()
        {
            try
            {  
                // var token = GetAccessToken();
                //if (string.IsNullOrEmpty(token))
                //{
                //    return Json(new List<StandardRequirementTypeObject>(), JsonRequestBehavior.AllowGet);
                //}

                var response = new StandardRequirementTypeServices().GetStandardRequirementTypes();

                if (response == null)
                {
                    return Json(new List<StandardRequirementTypeObject>(), JsonRequestBehavior.AllowGet);
                }
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<StandardRequirementTypeObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddCompanyDocument(StandardRequirementObject model)
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

                if (string.IsNullOrEmpty(model.TempPath))
                {
                    gVal.Error = "Document processing failed. Please try again.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
               
                var valStatus = ValidateControl(model);
                if (valStatus.Code < 1)
                {
                    gVal.Error = valStatus.Error;
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var path = MoveFile(importerInfo.Id, model.TempPath);
                if (string.IsNullOrEmpty(path))
                {
                    gVal.Error = "Document processing failed. Please try again.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                model.DocumentPath = path;
                model.ImporterId = importerInfo.Id;
                model.LastUpdated = DateTime.Now;
                var response = new StandardRequirementServices().AddStandardRequirement(model);

                if (response < 1)
                {
                    DeleteFile(model.DocumentPath);
                    gVal.Error = "Process failed.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.IsDocsSupplied = false;
                gVal.Error = "File was successfully processed.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Error = "Process failed. Please try again.";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult AddCompanyDoc(StandardRequirementObject model)
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

                if (string.IsNullOrEmpty(model.TempPath))
                {
                    gVal.Error = "Document processing failed. Please try again.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                if (string.IsNullOrEmpty(model.Title))
                {
                    gVal.Error = "Error: Document processing failed. Please try again.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (model.StandardRequirementTypeId < 1)
                {
                    gVal.Error = "Please select Document to process";
                    gVal.Code = 0;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var path = MoveFile(importerInfo.Id, model.TempPath);
                if (string.IsNullOrEmpty(path))
                {
                    gVal.Error = "Document processing failed. Please try again.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                model.DocumentPath = path;
                model.ImporterId = importerInfo.Id;
                model.LastUpdated = DateTime.Now;
                model.ValidFrom = DateTime.Now;
                var response = new StandardRequirementServices().AddStandardRequirement(model);

                if (response < 1)
                {
                    DeleteFile(model.DocumentPath);
                    gVal.Error = "Process failed.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Path = path;
                gVal.Code = response;
                gVal.Error = "File was successfully processed.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Error = "Process failed. Please try again.";
                gVal.Code = -1;
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

        [HttpPost]
        public ActionResult EditCompanyDocument(StandardRequirementObject model)
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
                
                var valStatus = ValidateControl(model);
                if (valStatus.Code < 1)
                {
                    gVal.Error = "Process failed.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
               
                if (Session["_sReq"] == null)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var sReq = Session["_sReq"] as StandardRequirementObject;

                if (sReq == null)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (!string.IsNullOrEmpty(model.TempPath))
                {
                    var path = MoveFile(importerInfo.Id, model.TempPath);
                    if (string.IsNullOrEmpty(path))
                    {
                        gVal.Error = "Document processing failed. Please try again.";
                        gVal.Code = -1;
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    sReq.DocumentPath = path;
                }


                var response = new StandardRequirementServices().UpdateStandardRequirement(sReq);

                if (response < 1)
                {
                    gVal.Error = response == -3? "A similar document already exists" : "Process failed. Please try again.";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (!string.IsNullOrEmpty(sReq.DocumentPath))
                {
                    DeleteFile(sReq.DocumentPath);
                }
                gVal.Code = 5;
                gVal.Error = "Standard Requirement was successfull processed.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
              
                gVal.Error = "Process failed. Please try again.";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

        }
        private static GenericValidator ValidateControl(StandardRequirementObject model)
        {
            var gVal = new GenericValidator();

            try
            {
                if (string.IsNullOrEmpty(model.Title.Trim()))
                {
                    gVal.Error = "Please provide Document Title.";
                    gVal.Code = 0;
                    return gVal;
                }
                if (model.ValidFrom.Year < 1)
                {
                    gVal.Error = "Please provide Validity Period Start Date.";
                    gVal.Code = 0;
                    return gVal;
                }

                if (model.ValidTo != null && model.ValidTo.Value.Year > 1)
                {
                    var outDate2 = (DateTime)model.ValidTo;

                    if (model.ValidFrom.Year > outDate2.Year)
                    {
                        gVal.Error = "Date obtained must not be later than expiry date.";
                        gVal.Code = 0;
                        return gVal;
                    }
                }

                if (model.StandardRequirementTypeId < 1)
                {
                    gVal.Error = "Please select Document Type";
                    gVal.Code = 0;
                    return gVal;
                }
                gVal.Code = 1;
                return gVal;
            }
            catch (Exception)
            {
                gVal.Error = "Process validation failed. Please supply all required entries and try again.";
                gVal.Code = 0;
                return gVal;
            }
        }

        #region Save Document

        private string MoveFile(long importerId, string tempPath)
        {
            try
            {
                var path = HostingEnvironment.MapPath("~/StandardRequirements/" + importerId.ToString(CultureInfo.InvariantCulture));
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

                return PhysicalToVirtualPathMapper.MapPath(newPathv).Replace("~", ""); 
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
                    return path;
                }
                return path;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return string.Empty;
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
                       System.IO.File.Delete((formerFilePath));
                    }
                    path = Path.Combine(folderPath, fileName);
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

        public ActionResult GetDocument(long id)
        {
            try
            {
                var document = new StandardRequirementServices().GetStandardRequirement(id);
                if (document == null || document.Id < 1)
                {
                    return Json(new StandardRequirementObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_sReq"] = document;

                return Json(document, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new StandardRequirementObject(), JsonRequestBehavior.AllowGet);
            }
        }
      
        [HttpPost]
        public GenericValidator DeleteDocument(long id)
        {
            var gVal = new GenericValidator();
            try
            {
                if (id < 1)
                {
                    gVal.Error = "Invalid Selection";
                    gVal.Code = -1;
                    return gVal;
                }
                var path = new StandardRequirementServices().DeleteStandardRequirement(id);
                if (string.IsNullOrEmpty(path))
                {
                    gVal.Error = "Process Failed! Please try again later";
                    gVal.Code = -1;
                    return gVal;
                }
                if (!string.IsNullOrWhiteSpace(path))
                {
                    System.IO.File.Delete((path));
                    gVal.Error = "Process Failed! Please try again later";
                    gVal.Code = -1;
                    return gVal;
                }

                gVal.Error = "Document Information was successfully deleted.";
                gVal.Code = 5;
                return gVal;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Error = "An unknown error was encountered. Please contact the Administrator or try again later.";
                gVal.Code = -1;
                return gVal;
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
                System.IO.File.Delete(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }
}
