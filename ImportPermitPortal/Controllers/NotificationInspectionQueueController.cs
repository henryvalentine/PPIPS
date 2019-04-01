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
    public class NotificationInspectionQueueController : Controller
    {
        [HttpGet]
        public ActionResult GetNotificationInspectionQueueObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<NotificationInspectionQueueObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetNotificationInspectionQueues(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationInspectionQueueServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationInspectionQueueObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationInspectionQueueObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : c.AssignedTimeStr);

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
                return Json(new List<NotificationInspectionQueueObject>(), JsonRequestBehavior.AllowGet);
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
        public ActionResult AddNotificationInspectionQueue(NotificationInspectionQueueObject notificationInspectionQueue)
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

                var validationResult = ValidateNotificationInspectionQueue(notificationInspectionQueue);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new NotificationInspectionQueueServices().AddNotificationInspectionQueue(notificationInspectionQueue);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "NotificationInspectionQueue upload failed. Please try again." : "The NotificationInspectionQueue Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "NotificationInspectionQueue was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "NotificationInspectionQueue notificationInspectionQueueing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditNotificationInspectionQueue(NotificationInspectionQueueObject notificationInspectionQueue)
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

                if (string.IsNullOrEmpty(notificationInspectionQueue.ReferenceCode.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide NotificationInspectionQueue.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_NotificationInspectionQueue"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldNotificationInspectionQueue = Session["_NotificationInspectionQueue"] as NotificationInspectionQueueObject;

                if (oldNotificationInspectionQueue == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                oldNotificationInspectionQueue.ReferenceCode = notificationInspectionQueue.ReferenceCode.Trim();

                var docStatus = new NotificationInspectionQueueServices().UpdateNotificationInspectionQueue(oldNotificationInspectionQueue);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "NotificationInspectionQueue information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldNotificationInspectionQueue.Id;
                gVal.Error = "NotificationInspectionQueue information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "NotificationInspectionQueue information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<NotificationInspectionQueueObject> GetNotificationInspectionQueues(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new NotificationInspectionQueueServices().GetNotificationInspectionQueues(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<NotificationInspectionQueueObject>();
            }
        }

        public ActionResult GetNotificationInspectionQueue(long id)
        {
            try
            {


                var notificationInspectionQueue = new NotificationInspectionQueueServices().GetNotificationInspectionQueue(id);
                if (notificationInspectionQueue == null || notificationInspectionQueue.Id < 1)
                {
                    return Json(new NotificationInspectionQueueObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_NotificationInspectionQueue"] = notificationInspectionQueue;

                return Json(notificationInspectionQueue, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new NotificationInspectionQueueObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteNotificationInspectionQueue(long id)
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
                var delStatus = new NotificationInspectionQueueServices().DeleteNotificationInspectionQueue(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "NotificationInspectionQueue could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "NotificationInspectionQueue Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator ValidateNotificationInspectionQueue(NotificationInspectionQueueObject notificationInspectionQueue)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(notificationInspectionQueue.ReferenceCode))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select provide NotificationInspectionQueue.";
                    return gVal;
                }


                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "NotificationInspectionQueue Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

     
      


      


       
      

     








    }
}
