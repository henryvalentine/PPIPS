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
    public class UnitOfMeasurementController : Controller
    {

        [HttpGet]
        public ActionResult GetUnitOfMeasurementObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<UnitOfMeasurementObject> filteredParentMenuObjects;
                int countG;

                var pagedParentMenuObjects = GetUnitOfMeasurements(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new UnitOfMeasurementServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<UnitOfMeasurementObject>(), JsonRequestBehavior.AllowGet);
                }

                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<UnitOfMeasurementObject, string> orderingFunction = (c => c.Name);

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
                return Json(new List<UnitOfMeasurementObject>(), JsonRequestBehavior.AllowGet);
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

        public ActionResult AddUnitOfMeasurement(UnitOfMeasurementObject unitOfMeasurement)
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

                var validationResult = ValidateUnitOfMeasurement(unitOfMeasurement);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new UnitOfMeasurementServices().AddUnitOfMeasurement(unitOfMeasurement);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "UnitOfMeasurement upload failed. Please try again." : "The UnitOfMeasurement Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "UnitOfMeasurement was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "UnitOfMeasurement processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditUnitOfMeasurement(UnitOfMeasurementObject unitOfMeasurement)
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

                var stat = ValidateUnitOfMeasurement(unitOfMeasurement);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_unitOfMeasurement"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldunitOfMeasurement = Session["_unitOfMeasurement"] as UnitOfMeasurementObject;

                if (oldunitOfMeasurement == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldunitOfMeasurement.Name = unitOfMeasurement.Name.Trim();
                //oldunitOfMeasurement.CountryCode = unitOfMeasurement.CountryCode;
                var docStatus = new UnitOfMeasurementServices().UpdateUnitOfMeasurement(oldunitOfMeasurement);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "UnitOfMeasurement already exists." : "UnitOfMeasurement information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldunitOfMeasurement.Id;
                gVal.Error = "UnitOfMeasurement information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "UnitOfMeasurement information could not be updated. Please try again later";
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

        public ActionResult GetUnitOfMeasurement(long id)
        {
            try
            {


                var unitOfMeasurement = new UnitOfMeasurementServices().GetUnitOfMeasurement(id);
                if (unitOfMeasurement == null || unitOfMeasurement.Id < 1)
                {
                    return Json(new UnitOfMeasurementObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_unitOfMeasurement"] = unitOfMeasurement;

                return Json(unitOfMeasurement, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new UnitOfMeasurementObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteUnitOfMeasurement(long id)
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
                var delStatus = new UnitOfMeasurementServices().DeleteUnitOfMeasurement(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "UnitOfMeasurement could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "UnitOfMeasurement Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<UnitOfMeasurementObject> GetUnitOfMeasurements(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new UnitOfMeasurementServices().GetUnitOfMeasurements(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<UnitOfMeasurementObject>();
            }
        }

        private GenericValidator ValidateUnitOfMeasurement(UnitOfMeasurementObject unitOfMeasurement)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(unitOfMeasurement.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide UnitOfMeasurement.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "UnitOfMeasurement Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }



        public List<UnitOfMeasurementObject> GetUnitOfMeasurements()
        {
            try
            {


                var unitOfMeasurement = new UnitOfMeasurementServices().GetUnitOfMeasurements();


                Session["_unitOfMeasurement"] = unitOfMeasurement;

                return new List<UnitOfMeasurementObject>();

            }
            catch (Exception)
            {
                return new List<UnitOfMeasurementObject>();
            }
        }

    }
}
