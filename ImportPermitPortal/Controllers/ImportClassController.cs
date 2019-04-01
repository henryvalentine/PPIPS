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
    public class ImportClassController : Controller
    {

        [HttpGet]
        public ActionResult GetImportClassObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ImportClassObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetImportClasss(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ImportClassServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ImportClassObject>(), JsonRequestBehavior.AllowGet);
                }

                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ImportClassObject, string> orderingFunction = (c => c.Name);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.Name };
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
                return Json(new List<ImportClassObject>(), JsonRequestBehavior.AllowGet);
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

        public ActionResult AddImportClass(ImportClassObject importClass)
        {
            var gVal = new GenericValidator();

            try
            {
                if (!ModelState.IsValid)
                {
                    gVal.Code = -1;
                    gVal.Error = "Plese provide all required fields and try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var validationResult = ValidateImportClass(importClass);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new ImportClassServices().AddImportClass(importClass);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "ImportClass upload failed. Please try again." : "The Import Class Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Import Class was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Import Class processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditImportClass(ImportClassObject importClass)
        {
            var gVal = new GenericValidator();

            try
            {
                if (!ModelState.IsValid)
                {
                    gVal.Code = -1;
                    gVal.Error = "Plese provide all required fields and try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var stat = ValidateImportClass(importClass);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_importClass"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldimportClass = Session["_importClass"] as ImportClassObject;

                if (oldimportClass == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldimportClass.Name = importClass.Name.Trim();
                var docStatus = new ImportClassServices().UpdateImportClass(oldimportClass);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Import Class already exists." : "Import Class information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldimportClass.Id;
                gVal.Error = "Import Class information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Import Class information could not be updated. Please try again later";
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

        public ActionResult GetImportClass(long id)
        {
            try
            {
                var importClass = new ImportClassServices().GetImportClass(id);
                if (importClass == null || importClass.Id < 1)
                {
                    return Json(new ImportClassObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_importClass"] = importClass;

                return Json(importClass, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new ImportClassObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteImportClass(long id)
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
                var delStatus = new ImportClassServices().DeleteImportClass(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Import Class could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Import Class Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ImportClassObject> GetImportClasss(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new ImportClassServices().GetImportClasss(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<ImportClassObject>();
            }
        }

        private GenericValidator ValidateImportClass(ImportClassObject importClass)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(importClass.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Import Class.";
                    return gVal;
                }
                
                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Import Class Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
