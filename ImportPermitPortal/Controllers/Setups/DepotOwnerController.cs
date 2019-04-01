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
    public class DepotOwnerController : Controller
    {

        [HttpGet]
        public ActionResult GetDepotOwnerObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<DepotOwnerObject> filteredParentMenuObjects;
                int countG;

                var pagedParentMenuObjects = GetDepotOwners(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new DepotOwnerServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<DepotOwnerObject>(), JsonRequestBehavior.AllowGet);
                }

                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<DepotOwnerObject, string> orderingFunction = (c => c.Name);

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
                return Json(new List<DepotOwnerObject>(), JsonRequestBehavior.AllowGet);
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

        public ActionResult AddDepotOwner(DepotOwnerObject depotOwner)
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

                var validationResult = ValidateDepotOwner(depotOwner);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new DepotOwnerServices().AddDepotOwner(depotOwner);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "DepotOwner upload failed. Please try again." : "The DepotOwner Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "DepotOwner was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "DepotOwner processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditDepotOwner(DepotOwnerObject depotOwner)
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

                var stat = ValidateDepotOwner(depotOwner);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_depotOwner"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var olddepotOwner = Session["_depotOwner"] as DepotOwnerObject;

                if (olddepotOwner == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                olddepotOwner.Name = depotOwner.Name.Trim();
                //olddepotOwner.CountryCode = depotOwner.CountryCode;
                var docStatus = new DepotOwnerServices().UpdateDepotOwner(olddepotOwner);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "DepotOwner already exists." : "DepotOwner information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = olddepotOwner.Id;
                gVal.Error = "DepotOwner information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "DepotOwner information could not be updated. Please try again later";
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

        public ActionResult GetDepotOwner(long id)
        {
            try
            {


                var depotOwner = new DepotOwnerServices().GetDepotOwner(id);
                if (depotOwner == null || depotOwner.Id < 1)
                {
                    return Json(new DepotOwnerObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_depotOwner"] = depotOwner;

                return Json(depotOwner, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new DepotOwnerObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteDepotOwner(long id)
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
                var delStatus = new DepotOwnerServices().DeleteDepotOwner(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "DepotOwner could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "DepotOwner Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<DepotOwnerObject> GetDepotOwners(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new DepotOwnerServices().GetDepotOwners(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<DepotOwnerObject>();
            }
        }

        private GenericValidator ValidateDepotOwner(DepotOwnerObject depotOwner)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(depotOwner.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide DepotOwner.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "DepotOwner Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }



        public List<DepotOwnerObject> GetDepotOwners()
        {
            try
            {


                var depotOwner = new DepotOwnerServices().GetDepotOwners();


                Session["_depotOwner"] = depotOwner;

                return new List<DepotOwnerObject>();

            }
            catch (Exception)
            {
                return new List<DepotOwnerObject>();
            }
        }

    }
}
