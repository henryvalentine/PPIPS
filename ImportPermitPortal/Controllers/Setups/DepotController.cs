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
    public class DepotController : Controller
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
        public ActionResult GetDepotObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<DepotObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetDepots(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new DepotServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<DepotObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<DepotObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name : c.JettyName);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.Name, c.JettyName, c.LastName };
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
                return Json(new List<DepotObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AddDepot(DepotObject depot)
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

                var validationResult = ValidateDepot(depot);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }


                var appStatus = new DepotServices().AddDepot(depot);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Depot upload failed. Please try again." : "The Depot Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Depot was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Depot processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditDepot(DepotObject depot)
        {
            var gVal = new GenericValidator();

            try
            {

                var stat = ValidateDepot(depot);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_depot"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var olddepot = Session["_depot"] as DepotObject;

                if (olddepot == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                olddepot.Name = depot.Name;
                olddepot.JettyId = depot.JettyId;

                var docStatus = new DepotServices().UpdateDepot(olddepot);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Depot already exists." : "Depot information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = olddepot.Id;
                gVal.Error = "Depot information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Depot information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetJetties()
        {
            try
            {
                return Json(getJettyObjects(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<JettyObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        private List<JettyObject> getJettyObjects()
        {

            try
            {
                return new JettyServices().GetJetties();

            }
            catch (Exception)
            {
                return new List<JettyObject>();
            }
        }
        public ActionResult GetDepot(int id)
        {
            try
            {
                var depot = new DepotServices().GetDepotAdmin(id);
                if (depot == null || depot.Id < 1)
                {
                    return Json(new UserProfileObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_usProfObj"] = depot;
                return Json(depot, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new UserProfileObject(), JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult DeleteDepot(long id)
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
                var delStatus = new DepotServices().DeleteDepot(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Depot could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Depot Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }


        private GenericValidator ValidateDepot(DepotObject depot)
        {
            var gVal = new GenericValidator();
            try
            {
                if (depot.JettyId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a Depot.";
                    return gVal;
                }



                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "Depot Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<DepotObject> GetDepots(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new DepotServices().GetDepots(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<DepotObject>();
            }
        }

        private List<PortObject> GetPorts()
        {
            try
            {
                return new PortServices().GetPorts();

            }
            catch (Exception)
            {
                return new List<PortObject>();
            }
        }



    }
}
