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
    public class PortController : Controller
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
        public ActionResult GetPortObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<PortObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetPorts(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new PortServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<PortObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<PortObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name : c.CountryName);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.Name, c.CountryName };
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
                return Json(new List<PortObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AddPort(PortObject port)
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

                var validationResult = ValidatePort(port);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
             

                var appStatus = new PortServices().AddPort(port);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Port upload failed. Please try again." : "The Port Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Port was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Port processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditPort(PortObject port)
        {
            var gVal = new GenericValidator();

            try
            {

                var stat = ValidatePort(port);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_port"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldport = Session["_port"] as PortObject;

                if (oldport == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                oldport.Name = port.Name;
                oldport.CountryId = port.CountryId;

                var docStatus = new PortServices().UpdatePort(oldport);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Port already exists." : "Port information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldport.Id;
                gVal.Error = "Port information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Port information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericPortList()
                {

                    Countries = GetCountrys()
                };

                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPort(long id)
        {
            try
            {


                var port = new PortServices().GetPort(id);
                if (port == null || port.Id < 1)
                {
                    return Json(new PortObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_port"] = port;

                return Json(port, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new PortObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public List<PortObject> GetPorts()
        {
            try
            {


                var port = new PortServices().GetPorts();


                Session["_port"] = port;

                return new List<PortObject>();

            }
            catch (Exception)
            {
                return new List<PortObject>();
            }
        }

        [HttpPost]
        public ActionResult DeletePort(long id)
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
                var delStatus = new PortServices().DeletePort(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Port could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Port Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }







        private GenericValidator ValidatePort(PortObject port)
        {
            var gVal = new GenericValidator();
            try
            {
                if (port.CountryId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a Country.";
                    return gVal;
                }



                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "Port Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<PortObject> GetPorts(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new PortServices().GetPorts(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<PortObject>();
            }
        }

        private List<CountryObject> GetCountrys()
        {
            try
            {
                return new CountryServices().GetCountries();

            }
            catch (Exception)
            {
                return new List<CountryObject>();
            }
        }

     

    }
}
