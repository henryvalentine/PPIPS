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
    public class CustomCodeController : Controller
    {

        [HttpGet]
        public ActionResult GetCustomCodeObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<CustomCodeObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetCustomCodes(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new CustomCodeServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<CustomCodeObject>(), JsonRequestBehavior.AllowGet);
                }

                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<CustomCodeObject, string> orderingFunction = (c => c.Name);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.CustomCodeId), c.Name };
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
                return Json(new List<CustomCodeObject>(), JsonRequestBehavior.AllowGet);
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

        public ActionResult AddCustomCode(CustomCodeObject customCode)
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

                var validationResult = ValidateCustomCode(customCode);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new CustomCodeServices().AddCustomCode(customCode);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "CustomCode upload failed. Please try again." : "The Custom Code Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Custom Code was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Custom Code processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditCustomCode(CustomCodeObject customCode)
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

                var stat = ValidateCustomCode(customCode);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_customCode"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldcustomCode = Session["_customCode"] as CustomCodeObject;

                if (oldcustomCode == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldcustomCode.Name = customCode.Name.Trim();
                var docStatus = new CustomCodeServices().UpdateCustomCode(oldcustomCode);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Custom Code already exists." : "Custom Code information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldcustomCode.CustomCodeId;
                gVal.Error = "Custom Code information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Custom Code information could not be updated. Please try again later";
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

        public ActionResult GetCustomCode(long id)
        {
            try
            {


                var customCode = new CustomCodeServices().GetCustomCode(id);
                if (customCode == null || customCode.CustomCodeId < 1)
                {
                    return Json(new CustomCodeObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_customCode"] = customCode;

                return Json(customCode, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new CustomCodeObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteCustomCode(long id)
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
                var delStatus = new CustomCodeServices().DeleteCustomCode(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Custom Code could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Custom Code Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<CustomCodeObject> GetCustomCodes(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new CustomCodeServices().GetCustomCodes(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<CustomCodeObject>();
            }
        }

        private GenericValidator ValidateCustomCode(CustomCodeObject customCode)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(customCode.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Custom Code.";
                    return gVal;
                }
                
                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Custom Code Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
