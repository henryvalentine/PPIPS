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
    [Authorize]
    public class StepActivityTypeController : Controller
    {

        [HttpGet]
        public ActionResult GetStepActivityTypeObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<StepActivityTypeObject> filteredParentMenuObjects;
                int countG;

                var pagedParentMenuObjects = GetStepActivityTypes(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new StepActivityTypeServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<StepActivityTypeObject>(), JsonRequestBehavior.AllowGet);
                }

                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<StepActivityTypeObject, string> orderingFunction = (c => c.Name);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.Name};
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
                return Json(new List<StepActivityTypeObject>(), JsonRequestBehavior.AllowGet);
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

        public ActionResult AddStepActivityType(StepActivityTypeObject stepActivityType)
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

                var validationResult = ValidateStepActivityType(stepActivityType);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new StepActivityTypeServices().AddStepActivityType(stepActivityType);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "StepActivityType upload failed. Please try again." : "The StepActivityType Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "StepActivityType was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "StepActivityType processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditStepActivityType(StepActivityTypeObject stepActivityType)
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

                var stat = ValidateStepActivityType(stepActivityType);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_stepActivityType"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldstepActivityType = Session["_stepActivityType"] as StepActivityTypeObject;

                if (oldstepActivityType == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldstepActivityType.Name = stepActivityType.Name.Trim();
                //oldstepActivityType.CountryCode = stepActivityType.CountryCode;
                var docStatus = new StepActivityTypeServices().UpdateStepActivityType(oldstepActivityType);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "StepActivityType already exists." : "StepActivityType information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldstepActivityType.Id;
                gVal.Error = "StepActivityType information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "StepActivityType information could not be updated. Please try again later";
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

        public ActionResult GetStepActivityType(long id)
        {
            try
            {


                var stepActivityType = new StepActivityTypeServices().GetStepActivityType(id);
                if (stepActivityType == null || stepActivityType.Id < 1)
                {
                    return Json(new StepActivityTypeObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_stepActivityType"] = stepActivityType;

                return Json(stepActivityType, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new StepActivityTypeObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteStepActivityType(long id)
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
                var delStatus = new StepActivityTypeServices().DeleteStepActivityType(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "StepActivityType could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "StepActivityType Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<StepActivityTypeObject> GetStepActivityTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new StepActivityTypeServices().GetStepActivityTypes(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<StepActivityTypeObject>();
            }
        }

        private GenericValidator ValidateStepActivityType(StepActivityTypeObject stepActivityType)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(stepActivityType.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide StepActivityType.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "StepActivityType Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }



        public List<StepActivityTypeObject> GetStepActivityTypes()
        {
            try
            {


                var stepActivityType = new StepActivityTypeServices().GetStepActivityTypes();


                Session["_stepActivityType"] = stepActivityType;

                return new List<StepActivityTypeObject>();

            }
            catch (Exception)
            {
                return new List<StepActivityTypeObject>();
            }
        }

    }
}
