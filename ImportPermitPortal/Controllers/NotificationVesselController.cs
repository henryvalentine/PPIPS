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
    public class NotificationVesselController : Controller  
    {
      
       [HttpGet]
       [Authorize(Roles = "Super_Admin")]
       public ActionResult GetNotificationVesselObjects(JQueryDataTableParamModel param)
       {
           try
           {
               IEnumerable<NotificationVesselObject> filteredParentMenuObjects;
               var countG = 0;

               var pagedParentMenuObjects = GetNotificationVessels(param.iDisplayLength, param.iDisplayStart, out countG);

               if (!string.IsNullOrEmpty(param.sSearch))
               {
                   filteredParentMenuObjects = new NotificationVesselServices().Search(param.sSearch);
               }
               else
               {
                   filteredParentMenuObjects = pagedParentMenuObjects;
               }

               if (!filteredParentMenuObjects.Any())
               {
                   return Json(new List<NotificationVesselObject>(), JsonRequestBehavior.AllowGet);
               }

               var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
               Func<NotificationVesselObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name : sortColumnIndex == 2 ? c.Name : c.VesselClassName);
                
               var sortDirection = Request["sSortDir_0"]; // asc or desc
               filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

               var displayedPersonnels = filteredParentMenuObjects;

               var result = from c in displayedPersonnels
                            select new[] { Convert.ToString(c.NotificationVesselId), c.Name, c.VesselClassName };
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
               return Json(new List<NotificationVesselObject>(), JsonRequestBehavior.AllowGet);
           }
       }

        [HttpPost]
        public ActionResult AddNotificationVessel(NotificationVesselObject notificationVessel)
        {
            var gVal = new GenericValidator();

            try
            {
                
                var validationResult = ValidateNotificationVessel(notificationVessel);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                notificationVessel.DateAdded = DateTime.Today;
                var appStatus = new NotificationVesselServices().AddNotificationVessel(notificationVessel);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Notification Vessel processing failed. Please try again." : "Notification Vessel Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Notification Vessel was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Notification Vessel processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddNotificationVessels(List<NotificationVesselObject> notificationVessels)
        {
            var gVal = new GenericValidator();

            try
            {
                var validationResult = ValidateNotificationVessel(notificationVessels);

                if (validationResult.Code < 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new NotificationVesselServices().AddNotificationVessels(notificationVessels);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Notification Vessel processing failed. Please try again." : "Notification Vessel Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Notification Vessel was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Notification Vessel processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateNotificationVessels(List<NotificationVesselObject> notificationVessels)
        {
            var gVal = new GenericValidator();

            try
            {
                var validationResult = ValidateNotificationVessel(notificationVessels);

                if (validationResult.Code < 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_notificationVessels"] == null)
                {
                     validationResult.Code = -1;
                    validationResult.Error = "Your session has timed out.";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var oldNotificationVessels = Session["_notificationVessels"] as List<NotificationVesselObject>;
                if (oldNotificationVessels == null || !oldNotificationVessels.Any())
                {
                    validationResult.Code = -1;
                    validationResult.Error = "Your session has timed out.";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                var type = notificationVessels.Find(t => t.VesselClassTypeId == (int) VesselClassEnum.Shuttle_Vessel);
                if (type == null || type.VesselClassTypeId < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = "Please Select Shuttle Vessel.";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
               
                var appStatus = new NotificationVesselServices().UpdateNotificationVessels(oldNotificationVessels, notificationVessels);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Notification Vessel processing failed. Please try again." : "Notification Vessel Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                if (appStatus != notificationVessels.Count)
                {
                    gVal.Code = -1;
                    gVal.Error = "Process failed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Process was successfully completed.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpPost]
        public ActionResult EditNotificationVessel(NotificationVesselObject notificationVessel)
        {
            var gVal = new GenericValidator();

            try
            {
                var stat = ValidateNotificationVessel(notificationVessel);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_notificationVessel"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldnotificationVessel = Session["_notificationVessel"] as NotificationVesselObject;

                if (oldnotificationVessel == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldnotificationVessel.Name = notificationVessel.Name.Trim();
                oldnotificationVessel.VesselClassTypeId = notificationVessel.VesselClassTypeId;

                var docStatus = new NotificationVesselServices().UpdateNotificationVessel(oldnotificationVessel);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Notification Vessel already exists." : "Notification Vessel information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldnotificationVessel.NotificationVesselId;
                gVal.Error = "Notification Vessel information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Notification Vessel information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
       
        public ActionResult GetNotificationVessels()
        {
            try
            {
                return Json(GetNotificationVesselLists(), JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetNotificationVessel(long id)
        {
            try
            {
                var notificationVessel = new NotificationVesselServices().GetNotificationVessel(id);
                if (notificationVessel == null || notificationVessel.NotificationVesselId < 1)
                {
                    return Json(new NotificationVesselObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_notificationVessel"] = notificationVessel;

                return Json(notificationVessel, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new NotificationVesselObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetNotificationVesselsByNotification(long notificationId)
        {
            try
            {
                var notificationVessels = new NotificationVesselServices().GetNotificationVesselsByNotification(notificationId);
                if (!notificationVessels.Any())
                {
                    return Json(new NotificationVesselObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_notificationVessels"] = notificationVessels;

                return Json(notificationVessels, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new NotificationVesselObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteNotificationVessel(long id)
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
                var delStatus = new NotificationVesselServices().DeleteNotificationVessel(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "NotificationVessel could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "NotificationVessel Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        private GenericValidator ValidateNotificationVessel(NotificationVesselObject notificationVessel)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(notificationVessel.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide NotificationVessel Name.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "NotificationVessel Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private GenericValidator ValidateNotificationVessel(List<NotificationVesselObject> notificationVessels)
        {
            var count = 0;
            var gVal = new GenericValidator();
            try
            {
                if (notificationVessels.Count(z => z.VesselClassTypeId == (int)VesselTypeEnum.Mother_Vessel) > 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Only one Mother Vessel should be selected.";
                    return gVal;
                }

                notificationVessels.ForEach(k =>
                {
                    if (k.VesselClassTypeId == (int)VesselTypeEnum.Mother_Vessel && !string.IsNullOrEmpty(k.Name))
                    {
                        count += 1;
                    }

                    if (k.VesselClassTypeId == (int)VesselTypeEnum.Shuttle_Vessel && k.VesselId > 0)
                    {
                        count += 1;
                    }

                });

                if (count != notificationVessels.Count)
                {
                    gVal.Code = -1;
                    gVal.Error = "Validation failed. Please provide all required fields and try again.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<NotificationVesselObject> GetNotificationVessels(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new NotificationVesselServices().GetNotificationVessels(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<NotificationVesselObject>();
            }
        }

        private List<NotificationVesselObject> GetNotificationVesselLists()
        {
            try
            {
                return new NotificationVesselServices().GetNotificationVessels();
            }
            catch (Exception)
            {
                return new List<NotificationVesselObject>();
            }
        }

    }
}
