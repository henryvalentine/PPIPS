using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ImportPermitPortal.BizObjects;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;

namespace ImportPermitPortal.Controllers
{
    [Authorize]
    public class DepotTrunkedOutController : Controller
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
        public ActionResult GetDepotTrunkedOutObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<DepotTrunkedOutObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetDepotTrunkedOuts(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new DepotTrunkedOutServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<DepotTrunkedOutObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<DepotTrunkedOutObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.DepotName : c.TrunkedOutDate.ToString());

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.DepotName, c.QuantityTrunkedOutInDepot.ToString(),c.TrunkedOutDate.ToString("dd/MM/yyyy") };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = countG,
                    iTotalDisplayRecords = filteredParentMenuObjects.Count(),
                    aaData = result
                },
                   JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<DepotTrunkedOutObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AddDepotTrunkedOut(DepotTrunkedOutObject depotTrunkedOut)
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

                var validationResult = ValidateDepotTrunkedOut(depotTrunkedOut);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
              

                var appStatus = new DepotTrunkedOutServices().AddDepotTrunkedOut(depotTrunkedOut);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "DepotTrunkedOut upload failed. Please try again." : "The DepotTrunkedOut Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "DepotTrunkedOut was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "DepotTrunkedOut processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditDepotTrunkedOut(DepotTrunkedOutObject depotTrunkedOut)
        {
            var gVal = new GenericValidator();

            try
            {
             

                var stat = ValidateDepotTrunkedOut(depotTrunkedOut);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_depotTrunkedOut"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var olddepotTrunkedOut = Session["_depotTrunkedOut"] as DepotTrunkedOutObject;

                if (olddepotTrunkedOut == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                olddepotTrunkedOut.DepotName = depotTrunkedOut.DepotName;
                
                
                var docStatus = new DepotTrunkedOutServices().UpdateDepotTrunkedOut(olddepotTrunkedOut);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "DepotTrunkedOut already exists." : "DepotTrunkedOut information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = olddepotTrunkedOut.Id;
                gVal.Error = "DepotTrunkedOut information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "DepotTrunkedOut information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }


       

        public ActionResult GetDepotTrunkedOut(long id)
        {
            try
            {


                var depotTrunkedOut = new DepotTrunkedOutServices().GetDepotTrunkedOut(id);
                if (depotTrunkedOut == null || depotTrunkedOut.Id < 1)
                {
                    return Json(new DepotTrunkedOutObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_depotTrunkedOut"] = depotTrunkedOut;

                return Json(depotTrunkedOut, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new DepotTrunkedOutObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public List<DepotTrunkedOutObject> GetDepotTrunkedOuts()
        {
            try
            {


                var depotTrunkedOut = new DepotTrunkedOutServices().GetDepotTrunkedOuts();
             

                Session["_depotTrunkedOut"] = depotTrunkedOut;

                return new List<DepotTrunkedOutObject>();

            }
            catch (Exception)
            {
                return new List<DepotTrunkedOutObject>();
            }
        }

        public CalculatorObject Calculator(DepotTrunkedOutObject calculator)
        {
            try
            {


                var calc = new DepotTrunkedOutServices().Calculator(calculator);


                Session["_depotTrunkedOut"] = calc;

                return new CalculatorObject();

            }
            catch (Exception)
            {
                return new CalculatorObject();
            }
        }

        [HttpPost]
        public ActionResult DeleteDepotTrunkedOut(long id)
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
                var delStatus = new DepotTrunkedOutServices().DeleteDepotTrunkedOut(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "DepotTrunkedOut could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "DepotTrunkedOut Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

       

       

      
       
        private GenericValidator ValidateDepotTrunkedOut(DepotTrunkedOutObject depotTrunkedOut)
        {
            var gVal = new GenericValidator();
            try
            {
                if (depotTrunkedOut.DepotId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a Port.";
                    return gVal;
                }

               

                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "DepotTrunkedOut Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<DepotTrunkedOutObject> GetDepotTrunkedOuts(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new DepotTrunkedOutServices().GetDepotTrunkedOuts(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<DepotTrunkedOutObject>();
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

        public ActionResult GetAllDepotList()
        {
            try
            {


                var newList = new GenericDepotList()
                {

                    Depots = getAllDepots()
                };

                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        private List<DepotObject> getAllDepots()
        {

            try
            {
                return new DepotServices().GetDepots();

            }
            catch (Exception)
            {
                return new List<DepotObject>();
            }
        }


    }
}
