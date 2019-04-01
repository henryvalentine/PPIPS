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
    public class ImportStageController : Controller
    {
        [HttpGet]
        public ActionResult GetImportStageObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ImportStageObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetImportStages(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ImportStageServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ImportStageObject>(), JsonRequestBehavior.AllowGet);
                }

                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ImportStageObject, string> orderingFunction = (c => c.Name);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),c.Name};
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
                return Json(new List<ImportStageObject>(), JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public ActionResult AddImportStage(ImportStageObject ImportStage)
        {
            var gVal = new GenericValidator();

            try
            {
                //if (!ModelState.IsValid)
                //{
                //    gVal.Code = -1;
                //    gVal.Error = "Plese provide all required fields and try again.";
                //    return Json(gVal, JsonRequestBehavior.AllowGet);
                //}

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var validationResult = ValidateImportStage(ImportStage);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new ImportStageServices().AddImportStage(ImportStage);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "ImportStage upload failed. Please try again." : "The Application Stage Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Application Stage was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Application Stage processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditImportStage(ImportStageObject ImportStage)
        {
            var gVal = new GenericValidator();

            try
            {
                //if (!ModelState.IsValid)
                //{
                //    gVal.Code = -1;
                //    gVal.Error = "Plese provide all required fields and try again.";
                //    return Json(gVal, JsonRequestBehavior.AllowGet);
                //}

                if (string.IsNullOrEmpty(ImportStage.Name.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Application Stage.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_ImportStage"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldImportStage = Session["_ImportStage"] as ImportStageObject;

                if (oldImportStage == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                oldImportStage.Name = ImportStage.Name.Trim();

                var docStatus = new ImportStageServices().UpdateImportStage(oldImportStage);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Application Stage information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldImportStage.Id;
                gVal.Error = "Application Stage information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Application Stage information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ImportStageObject> GetImportStages(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new ImportStageServices().GetImportStages(itemsPerPage, pageNumber, out countG);
                
            }
            catch (Exception)
            {
                countG = 0;
                return new List<ImportStageObject>(); 
            }
        }

        public ActionResult GetImportStage(long id)
        {
            try
            {


                var ImportStage = new ImportStageServices().GetImportStage(id);
                if (ImportStage == null || ImportStage.Id < 1)
                {
                    return Json(new ImportStageObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_ImportStage"] = ImportStage;

                return Json(ImportStage, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new ImportStageObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteImportStage(long id)
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
                var delStatus = new ImportStageServices().DeleteImportStage(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Application Stage could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Application Stage Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        private GenericValidator ValidateImportStage(ImportStageObject ImportStage)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(ImportStage.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select provide Application Stage.";
                    return gVal;
                }


                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Application Stage Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
