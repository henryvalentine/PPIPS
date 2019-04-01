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
    public class JettyController : Controller
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
        public ActionResult GetJettyObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<JettyObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetJettys(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new JettyServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<JettyObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<JettyObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name : c.PortName);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.Name, c.PortName };
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
                return Json(new List<JettyObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AddJetty(JettyObject jetty)
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

                var validationResult = ValidateJetty(jetty);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
              

                var appStatus = new JettyServices().AddJetty(jetty);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Jetty upload failed. Please try again." : "The Jetty Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Jetty was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Jetty processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditJetty(JettyObject jetty)
        {
            var gVal = new GenericValidator();

            try
            {
             

                var stat = ValidateJetty(jetty);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_jetty"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldjetty = Session["_jetty"] as JettyObject;

                if (oldjetty == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldjetty.Name = jetty.Name;
                oldjetty.PortId = jetty.PortId;
                
                var docStatus = new JettyServices().UpdateJetty(oldjetty);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Jetty already exists." : "Jetty information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldjetty.Id;
                gVal.Error = "Jetty information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Jetty information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericJettyList()
                {
                   
                    Ports = GetPorts()
                };

                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetJetty(long id)
        {
            try
            {


                var jetty = new JettyServices().GetJetty(id);
                if (jetty == null || jetty.Id < 1)
                {
                    return Json(new JettyObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_jetty"] = jetty;

                return Json(jetty, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new JettyObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public List<JettyObject> GetJettys()
        {
            try
            {


                var jetty = new JettyServices().GetJetties();
             

                Session["_jetty"] = jetty;

                return new List<JettyObject>();

            }
            catch (Exception)
            {
                return new List<JettyObject>();
            }
        }

        [HttpPost]
        public ActionResult DeleteJetty(long id)
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
                var delStatus = new JettyServices().DeleteJetty(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Jetty could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Jetty Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

       

       

      
       
        private GenericValidator ValidateJetty(JettyObject jetty)
        {
            var gVal = new GenericValidator();
            try
            {
                if (jetty.PortId < 1)
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
                gVal.Error = "Jetty Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<JettyObject> GetJettys(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new JettyServices().GetJetties(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<JettyObject>();
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
