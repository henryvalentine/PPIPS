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
    public class ImportClassificationRequirementController : Controller
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
       public ActionResult GetImportClassificationRequirementObjects(JQueryDataTableParamModel param)
       {
           try
           {
               IEnumerable<ImportClassificationRequirementObject> filteredParentMenuObjects;
               int countG;

               var pagedParentMenuObjects = GetImportClassificationRequirements(param.iDisplayLength, param.iDisplayStart, out countG);

               if (!string.IsNullOrEmpty(param.sSearch))
               {
                   filteredParentMenuObjects = new ImportClassificationRequirementServices().Search(param.sSearch);
               }
               else
               {
                   filteredParentMenuObjects = pagedParentMenuObjects;
               }

               if (!filteredParentMenuObjects.Any())
               {
                   return Json(new List<ImportClassificationRequirementObject>(), JsonRequestBehavior.AllowGet);
               }

               var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
               Func<ImportClassificationRequirementObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ImportClassName : c.ImportStageName);

               var sortDirection = Request["sSortDir_0"]; // asc or desc
               filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

               var displayedPersonnels = filteredParentMenuObjects;
               
               var result = from c in displayedPersonnels
                            select new[] { Convert.ToString(c.Id), c.ImportClassName, c.ImportStageName, c.Requirements };
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
               return Json(new List<ImportClassificationRequirementObject>(), JsonRequestBehavior.AllowGet);
           }
       }
       public ActionResult AddImportClassificationRequirement(List<ImportClassificationRequirementObject> importClassificationRequirements)
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

                var validationResult = ValidateImportClassificationRequirement(importClassificationRequirements);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new ImportClassificationRequirementServices().AddImportClassificationRequirement(importClassificationRequirements);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "ImportClassificationRequirement upload failed. Please try again." : "The Import Classification Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Import Classification was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Import Classification processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
       public ActionResult EditImportClassificationRequirement(List<ImportClassificationRequirementObject> importClassificationRequirements)
        {
            var gVal = new GenericValidator();

            try
            {
                var stat = ValidateImportClassificationRequirement(importClassificationRequirements);

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

                var oldimportClassificationRequirement = Session["_classificationReq"] as ImportClassificationRequirementObject;

                if (oldimportClassificationRequirement == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                var docStatus = new ImportClassificationRequirementServices().UpdateImportClassificationRequirement(oldimportClassificationRequirement, importClassificationRequirements);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Import Classification already exists." : "Import Classification Requirement could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldimportClassificationRequirement.Id;
                gVal.Error = "Import Classification Requirement was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Import Classification Requirement could not be updated. Please try again later";
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

        public ActionResult GetImportClassificationRequirement(long id)
        {
            try
            {
                var importClassificationRequirement = new ImportClassificationRequirementServices().GetImportClassificationRequirement(id);
                if (importClassificationRequirement == null || importClassificationRequirement.Id < 1)
                {
                    return Json(new ImportClassificationRequirementObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_classificationReq"] = importClassificationRequirement;

                return Json(importClassificationRequirement, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new ImportClassificationRequirementObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericClassificationList
                {
                    DocumentTypes = GetDocumentTypes(),
                    Classes = GetClassifications(),
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
        public ActionResult DeleteImportClassificationRequirement(long id)
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
                var delStatus = new ImportClassificationRequirementServices().DeleteImportClassificationRequirement(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Import Classification could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Import Classification Information was successfully deleted";
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

        private List<ImportClassObject> GetClassifications()
        {
            try
            {
                var classes = new ImportClassServices().GetImportClasses();
                if (!classes.Any())
                {
                    return new List<ImportClassObject>();
                }
                return classes;
            }
            catch (Exception)
            {
                return new List<ImportClassObject>();
            }
        }

        private List<ImportClassificationRequirementObject> GetImportClassificationRequirements(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new ImportClassificationRequirementServices().GetImportClassificationRequirements(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<ImportClassificationRequirementObject>();
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

        private GenericValidator ValidateImportClassificationRequirement(List<ImportClassificationRequirementObject> importClassificationRequirements)
        {
            var gVal = new GenericValidator();
            try
            {
                foreach (var im in importClassificationRequirements)
                {
                    if (im.ClassificationId < 1)
                    {
                        gVal.Code = -1;
                        gVal.Error = "Please select Application Stage.";
                        return gVal;
                    }
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
                gVal.Error = "Import Classification Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
