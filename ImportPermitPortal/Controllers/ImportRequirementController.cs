using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;

namespace ImportPermitPortal.Controllers
{
    [Authorize(Roles = "Super_Admin")]
    public class ImportRequirementController : Controller
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

       [HttpGet]
       public ActionResult GetImportRequirementObjects(JQueryDataTableParamModel param)
       {
           try
           {
               IEnumerable<ImportRequirementObject> filteredParentMenuObjects;
               int countG;

               var pagedParentMenuObjects = GetImportRequirements(param.iDisplayLength, param.iDisplayStart, out countG);

               if (!string.IsNullOrEmpty(param.sSearch))
               {
                   filteredParentMenuObjects = new ImportRequirementServices().Search(param.sSearch);
               }
               else
               {
                   filteredParentMenuObjects = pagedParentMenuObjects;
               }

               if (!filteredParentMenuObjects.Any())
               {
                   return Json(new List<ImportRequirementObject>(), JsonRequestBehavior.AllowGet);
               }

               //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
               Func<ImportRequirementObject, string> orderingFunction = (c =>  c.ImportStageName);

               var sortDirection = Request["sSortDir_0"]; // asc or desc
               filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

               var displayedPersonnels = filteredParentMenuObjects;
               
               var result = from c in displayedPersonnels
                            select new[] { Convert.ToString(c.Id), c.ImportStageName, c.Requirements };
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
               return Json(new List<ImportRequirementObject>(), JsonRequestBehavior.AllowGet);
           }
       }
       public ActionResult AddImportRequirement(List<ImportRequirementObject> importRequirements)
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

                var validationResult = ValidateImportRequirement(importRequirements);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new ImportRequirementServices().AddImportRequirement(importRequirements);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Import Requirement processing failed. Please try again." : "The Import Requirement Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Import Requirement was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Import Requirement processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
       public ActionResult EditImportRequirement(List<ImportRequirementObject> importRequirements)
        {
            var gVal = new GenericValidator();

            try
            {
                var stat = ValidateImportRequirement(importRequirements);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_classificationReq"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldimportClassificationRequirement = Session["_classificationReq"] as ImportRequirementObject;

                if (oldimportClassificationRequirement == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                var docStatus = new ImportRequirementServices().UpdateImportRequirement(oldimportClassificationRequirement, importRequirements);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Import Requirement already exists." : "Import  Requirement could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldimportClassificationRequirement.Id;
                gVal.Error = "Import  Requirement was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Import  Requirement could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult SetDocSession(long id)
        {
            if (id < 1)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }

            Session["_appId"] = id;
            return Json(5, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetImportRequirement(long id)
        {
            try
            {
                var importClassificationRequirement = new ImportRequirementServices().GetImportRequirement(id);
                if (importClassificationRequirement == null || importClassificationRequirement.Id < 1)
                {
                    return Json(new ImportRequirementObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_classificationReq"] = importClassificationRequirement;

                return Json(importClassificationRequirement, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new ImportRequirementObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericDocumentList
                {
                    DocumentTypes = GetDocumentTypes(),
                    ImportStages = GetImportStages()
                };
                
                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericClassificationList(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteImportRequirement(long id)
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
                var delStatus = new ImportRequirementServices().DeleteImportRequirement(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Import Requirement could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Import Requirement Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ImportStageObject> GetImportStages()
        {
            try
            {
                return new ImportStageServices().GetImportStages();
            }
            catch (Exception)
            {
                return new List<ImportStageObject>();
            }
        }

        private List<ImportRequirementObject> GetImportRequirements(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new ImportRequirementServices().GetImportRequirements(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<ImportRequirementObject>();
            }
        }

        private List<DocumentTypeObject> GetDocumentTypes()
        {
            try
            {
                return new DocumentTypeServices().GetDocumentTypes();
            }
            catch (Exception)
            {
                return new List<DocumentTypeObject>();
            }
        }

        private GenericValidator ValidateImportRequirement(List<ImportRequirementObject> importRequirements)
        {
            var gVal = new GenericValidator();
            try
            {
                foreach (var im in importRequirements)
                {
                    if (im.ImportStageId < 1)
                    {
                        gVal.Code = -1;
                        gVal.Error = "Please select Import Stage.";
                        return gVal;
                    }

                    if (im.DocumentTypeId < 1)
                    {
                        gVal.Code = -1;
                        gVal.Error = "Please select Document Type.";
                        return gVal;
                    }

                }
                gVal.Code = 5;
                return gVal;
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Import Requirement Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
