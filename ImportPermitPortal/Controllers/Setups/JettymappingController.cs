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
    public class JettyMappingController : Controller
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
        public ActionResult GetJettyMappingObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<JettyMappingObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetJettyMappings(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new JettyMappingServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<JettyMappingObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<JettyMappingObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.JettyName : c.ZoneName);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.JettyName, c.ZoneName };
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
                return Json(new List<JettyMappingObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AddJettyMapping(JettyMappingObject jettyMapping)
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

                var validationResult = ValidateJettyMapping(jettyMapping);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
            
                

                var appStatus = new JettyMappingServices().AddJettyMapping(jettyMapping);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "JettyMapping upload failed. Please try again." : "The JettyMapping Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "JettyMapping was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "JettyMapping processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditJettyMapping(JettyMappingObject jettyMapping)
        {
            var gVal = new GenericValidator();

            try
            {
                

                var stat = ValidateJettyMapping(jettyMapping);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_jettyMapping"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldjettyMapping = Session["_jettyMapping"] as JettyMappingObject;

                if (oldjettyMapping == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldjettyMapping.JettyId = jettyMapping.JettyId;
                oldjettyMapping.ZoneId = jettyMapping.ZoneId;
                
                
                var docStatus = new JettyMappingServices().UpdateJettyMapping(oldjettyMapping);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "JettyMapping already exists." : "JettyMapping information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldjettyMapping.Id;
                gVal.Error = "JettyMapping information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "JettyMapping information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericJettyMappingList()
                {
                   
                    Jetties = GetJettyObjects(),
                    Zones = GetZoneObjects()
                };

                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetJettyMapping(long id)
        {
            try
            {


                var jettyMapping = new JettyMappingServices().GetJettyMapping(id);
                if (jettyMapping == null || jettyMapping.Id < 1)
                {
                    return Json(new JettyMappingObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_jettyMapping"] = jettyMapping;

                return Json(jettyMapping, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new JettyMappingObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteJettyMapping(long id)
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
                var delStatus = new JettyMappingServices().DeleteJettyMapping(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "JettyMapping could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "JettyMapping Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

       

       

      
       
        private GenericValidator ValidateJettyMapping(JettyMappingObject jettyMapping)
        {
            var gVal = new GenericValidator();
            try
            {
                if (jettyMapping.JettyId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a Jetty.";
                    return gVal;
                }

                if (jettyMapping.ZoneId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a Zone.";
                    return gVal;
                }
               

                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "JettyMapping Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<JettyMappingObject> GetJettyMappings(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new JettyMappingServices().GetJettyMappings(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<JettyMappingObject>();
            }
        }

        private List<JettyObject> GetJettyObjects()
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

        private List<ZoneObject> GetZoneObjects()
        {
            try
            {
                return new ZoneServices().GetZones();

            }
            catch (Exception)
            {
                return new List<ZoneObject>();
            }
        }

    }
}
