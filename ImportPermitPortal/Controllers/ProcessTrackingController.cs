using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.EF.Model;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;

namespace ImportPermitPortal.Controllers
{
    [Authorize(Roles = "Super_Admin")]
    public class ProcessTrackingController : Controller
    {
        [HttpGet]
        public ActionResult GetProcessTrackingObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ProcessTrackingObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetProcessTrackings(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ProcessTrackingServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ProcessTrackingObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ProcessTrackingObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : c.AssignedTimeStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), 
                                 c.ReferenceCode, c.EmployeeName, c.AssignedTimeStr,
                                 c.DueTimeStr, c.ActualDeliveryDateTimeStr, c.StepName,c.ProcessName,c.OutComeCodeStr };
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
                return Json(new List<ProcessTrackingObject>(), JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public ActionResult AddProcessTracking(ProcessTrackingObject processTracking)
        {
            var gVal = new GenericValidator();

            try
            {
                //if (!ModelState.IsValid)
                //{
                //    gVal.Code = -1;
                //    gVal.Error = "Plese provide all required fields and try again.";
                //    return Json(gVal, JsonRequestBehavior.AllowGet);
                //}

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var validationResult = ValidateProcessTracking(processTracking);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new ProcessTrackingServices().AddProcessTracking(processTracking);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "ProcessTracking upload failed. Please try again." : "The ProcessTracking Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "ProcessTracking was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "ProcessTracking processTrackinging failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditProcessTracking(ProcessTrackingObject processTracking)
        {
            var gVal = new GenericValidator();

            try
            {
                //if (!ModelState.IsValid)
                //{
                //    gVal.Code = -1;
                //    gVal.Error = "Plese provide all required fields and try again.";
                //    return Json(gVal, JsonRequestBehavior.AllowGet);
                //}

                if (string.IsNullOrEmpty(processTracking.ReferenceCode.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide ProcessTracking.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_ProcessTracking"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldProcessTracking = Session["_ProcessTracking"] as ProcessTrackingObject;

                if (oldProcessTracking == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                oldProcessTracking.ReferenceCode = processTracking.ReferenceCode.Trim();

                var docStatus = new ProcessTrackingServices().UpdateProcessTracking(oldProcessTracking);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "ProcessTracking information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldProcessTracking.Id;
                gVal.Error = "ProcessTracking information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "ProcessTracking information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ProcessTrackingObject> GetProcessTrackings(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new ProcessTrackingServices().GetProcessTrackings(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<ProcessTrackingObject>();
            }
        }

        public ActionResult GetProcessTracking(long id)
        {
            try
            {


                var processTracking = new ProcessTrackingServices().GetProcessTracking(id);
                if (processTracking == null || processTracking.Id < 1)
                {
                    return Json(new ProcessTrackingObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_ProcessTracking"] = processTracking;

                return Json(processTracking, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new ProcessTrackingObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteProcessTracking(long id)
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
                var delStatus = new ProcessTrackingServices().DeleteProcessTracking(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "ProcessTracking could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "ProcessTracking Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator ValidateProcessTracking(ProcessTrackingObject processTracking)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(processTracking.ReferenceCode))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select provide ProcessTracking.";
                    return gVal;
                }


                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "ProcessTracking Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

     
      


      


       
      

     








    }
}
