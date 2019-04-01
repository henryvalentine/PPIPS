using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;

namespace ImportPermitPortal.Controllers
{
    [Authorize]
    public class VesselController : Controller  
    {
      
       [HttpGet]
       public ActionResult GetVesselObjects(JQueryDataTableParamModel param)
       {
           try
           {
               IEnumerable<VesselObject> filteredParentMenuObjects;
               var countG = 0;

               var pagedParentMenuObjects = GetVessels(param.iDisplayLength, param.iDisplayStart, out countG);

               if (!string.IsNullOrEmpty(param.sSearch))
               {
                   filteredParentMenuObjects = new VesselServices().Search(param.sSearch);
                   countG = filteredParentMenuObjects.Count();
               }
               else
               {
                   filteredParentMenuObjects = pagedParentMenuObjects;
               }

               if (!filteredParentMenuObjects.Any())
               {
                   return Json(new List<VesselObject>(), JsonRequestBehavior.AllowGet);
               }

               var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
               Func<VesselObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.CompanyName : sortColumnIndex == 2 ? c.Name :  c.CapacityStr);
                
               var sortDirection = Request["sSortDir_0"]; // asc or desc
               filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

               var displayedPersonnels = filteredParentMenuObjects;

               var result = from c in displayedPersonnels select new[] {Convert.ToString(c.VesselId),  c.CompanyName , c.Name, c.CapacityStr};
               return Json(new
               {
                   param.sEcho,
                   iTotalRecords = countG,
                   iTotalDisplayRecords = countG,
                   aaData = result
               }, JsonRequestBehavior.AllowGet);

           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return Json(new List<VesselObject>(), JsonRequestBehavior.AllowGet);
           }
       }

        [HttpPost]
        [Authorize(Roles = "Super_Admin")]
        public ActionResult AddVessel(VesselObject vessel)
        {
            var gVal = new GenericValidator();

            try
            {
                var validationResult = ValidateVessel(vessel);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                vessel.DateAdded = DateTime.Today;
                var appStatus = new VesselServices().AddVessel(vessel);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Vessel processing failed. Please try again." : "Vessel Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Vessel was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Vessel processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Super_Admin")]
        public ActionResult EditVessel(VesselObject vessel)
        {
            var gVal = new GenericValidator();

            try
            {
                var stat = ValidateVessel(vessel);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_vessel"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldvessel = Session["_vessel"] as VesselObject;

                if (oldvessel == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldvessel.Name = vessel.Name.Trim();
                oldvessel.Capacity = vessel.Capacity;
                oldvessel.IssueDate = vessel.IssueDate;
                oldvessel.ExpiryDate = vessel.ExpiryDate;
                oldvessel.VesselLicense = vessel.VesselLicense;
                oldvessel.CompanyName = vessel.CompanyName;

                var docStatus = new VesselServices().UpdateVessel(oldvessel);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Vessel already exists." : "Vessel information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldvessel.VesselId;
                gVal.Error = "Vessel information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Vessel information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
       
        public ActionResult GetVessels()
        {
            try
            { 
                return Json(GetVesselLists(), JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new List<VesselObject>(), JsonRequestBehavior.AllowGet);
            }
        }

         public ActionResult GetShuttleVessels()
        {
            try
            {
                return Json(new VesselServices().GetValidShuttleVessels(), JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new List<VesselObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetVessel(long id)
        {
            try
            {
                var vessel = new VesselServices().GetVessel(id);
                if (vessel == null || vessel.VesselId < 1)
                {
                    return Json(new VesselObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_vessel"] = vessel;

                return Json(vessel, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new VesselObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteVessel(long id)
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
                var delStatus = new VesselServices().DeleteVessel(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Vessel could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Vessel Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator ValidateVessel(VesselObject model)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(model.CompanyName))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Company name";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Vessel Name";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.VesselLicense))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Vessel License Number";
                    return gVal;
                }

                if (model.IssueDate == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide License Issue Date";
                    return gVal;
                }

                if (model.ExpiryDate == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide License Expiry Date";
                    return gVal;
                }

                if (model.IssueDate != null && model.IssueDate.Value.Year < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide valid License Issue Date";
                    return gVal;
                }
                if (model.ExpiryDate != null && model.ExpiryDate.Value.Year < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide valid License Expiry Date";
                    return gVal;
                }

                if (model.IssueDate.Value > model.ExpiryDate.Value)
                {
                    gVal.Code = -1;
                    gVal.Error = "Vessel License Issue Date must not be later than the Expiry Date";
                    return gVal;
                }

                if (model.Capacity < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Vessel Capacity";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
            }
            catch (Exception ex)
            {
                gVal.Code = -1;
                gVal.Error = "Validation failed. Please try again.";
                return gVal;
            }
        }

        private List<VesselObject> GetVessels(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new VesselServices().GetVessels(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<VesselObject>();
            }
        }

        private List<VesselObject> GetVesselLists()
        {
            try
            {
                return new VesselServices().GetVessels();
            }
            catch (Exception)
            {
                return new List<VesselObject>();
            }
        }

    }
}
