using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ImportPermitPortal.BizObjects;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.EF.Model;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;
using Microsoft.AspNet.Identity;

namespace ImportPermitPortal.Controllers
{
    [Authorize(Roles = "Super_Admin")]
    public class ProcessingHistoryController : Controller
    {
        [HttpGet]
        public ActionResult GetProcessingHistoryObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ProcessingHistoryObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetProcessingHistorys(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ProcessingHistoryServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ProcessingHistoryObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ProcessingHistoryObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : c.AssignedTimeStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), 
                                 c.ReferenceCode,c.CompanyName, c.EmployeeName, c.Remarks, c.AssignedTimeStr,c.DateLeftStr};
                                  
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
                return Json(new List<ProcessingHistoryObject>(), JsonRequestBehavior.AllowGet);
            }
        }
         [HttpGet]
        public ActionResult GetApplicationFromHistory(long id)
        {
            var rep = new Reporter();
            try
            {
                var db = new ImportPermitEntities();
                //get the id of logged in user
                var aspnetId = User.Identity.GetUserId();

                //get the id of the userprofile table
                var registeredGuys = db.AspNetUsers.Find(aspnetId);
                var profileId = registeredGuys.UserProfile.Id;
                if (id < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetApplicationInfoFromHistory(id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }
        }

        private ApplicationObject GetApplicationInfoFromHistory(long historyId)
        {
            return new ProcessingHistoryServices().GetApplicationFromHistory(historyId);
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
        public ActionResult AddProcessingHistory(ProcessingHistoryObject processingHistory)
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

                var validationResult = ValidateProcessingHistory(processingHistory);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new ProcessingHistoryServices().AddProcessingHistory(processingHistory);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "ProcessingHistory upload failed. Please try again." : "The ProcessingHistory Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "ProcessingHistory was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "ProcessingHistory processingHistorying failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditProcessingHistory(ProcessingHistoryObject processingHistory)
        {
            var gVal = new GenericValidator();

            try
            {
                

                if (string.IsNullOrEmpty(processingHistory.ReferenceCode.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide ProcessingHistory.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_ProcessingHistory"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldProcessingHistory = Session["_ProcessingHistory"] as ProcessingHistoryObject;

                if (oldProcessingHistory == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                oldProcessingHistory.ReferenceCode = processingHistory.ReferenceCode.Trim();

                var docStatus = new ProcessingHistoryServices().UpdateProcessingHistory(oldProcessingHistory);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "ProcessingHistory information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldProcessingHistory.Id;
                gVal.Error = "ProcessingHistory information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "ProcessingHistory information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ProcessingHistoryObject> GetProcessingHistorys(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new ProcessingHistoryServices().GetProcessingHistorys(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<ProcessingHistoryObject>();
            }
        }

        public ActionResult GetProcessingHistory(long id)
        {
            try
            {


                var processingHistory = new ProcessingHistoryServices().GetProcessingHistory(id);
                if (processingHistory == null || processingHistory.Id < 1)
                {
                    return Json(new ProcessingHistoryObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_ProcessingHistory"] = processingHistory;

                return Json(processingHistory, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new ProcessingHistoryObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteProcessingHistory(long id)
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
                var delStatus = new ProcessingHistoryServices().DeleteProcessingHistory(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "ProcessingHistory could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "ProcessingHistory Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator ValidateProcessingHistory(ProcessingHistoryObject processingHistory)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(processingHistory.ReferenceCode))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select provide ProcessingHistory.";
                    return gVal;
                }


                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "ProcessingHistory Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

     
      


      


       
      

     








    }
}
