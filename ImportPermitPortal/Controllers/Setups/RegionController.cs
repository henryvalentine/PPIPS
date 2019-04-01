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
    public class RegionController : Controller
    {

        [HttpGet]
        public ActionResult GetRegionObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<RegionObject> filteredParentMenuObjects;
                int countG;

                var pagedParentMenuObjects = GetRegions(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new RegionServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<RegionObject>(), JsonRequestBehavior.AllowGet);
                }

                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<RegionObject, string> orderingFunction = (c => c.Name);

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
                return Json(new List<RegionObject>(), JsonRequestBehavior.AllowGet);
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

        public ActionResult AddRegion(RegionObject region)
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

                var validationResult = ValidateRegion(region);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new RegionServices().AddRegion(region);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Region upload failed. Please try again." : "The Region Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Region was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Region processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditRegion(RegionObject region)
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

                var stat = ValidateRegion(region);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_region"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldregion = Session["_region"] as RegionObject;

                if (oldregion == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldregion.Name = region.Name.Trim();
                //oldregion.CountryCode = region.CountryCode;
                var docStatus = new RegionServices().UpdateRegion(oldregion);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Region already exists." : "Region information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldregion.Id;
                gVal.Error = "Region information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Region information could not be updated. Please try again later";
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

        public ActionResult GetRegion(long id)
        {
            try
            {


                var region = new RegionServices().GetRegion(id);
                if (region == null || region.Id < 1)
                {
                    return Json(new RegionObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_region"] = region;

                return Json(region, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new RegionObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteRegion(long id)
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
                var delStatus = new RegionServices().DeleteRegion(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Region could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Region Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<RegionObject> GetRegions(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new RegionServices().GetRegions(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<RegionObject>();
            }
        }

        private GenericValidator ValidateRegion(RegionObject region)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(region.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Region.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "Region Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }



        public List<RegionObject> GetRegions()
        {
            try
            {


                var region = new RegionServices().GetRegions();


                Session["_region"] = region;

                return new List<RegionObject>();

            }
            catch (Exception)
            {
                return new List<RegionObject>();
            }
        }

    }
}
