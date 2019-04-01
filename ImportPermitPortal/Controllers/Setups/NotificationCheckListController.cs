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
    public class NotificationCheckListController : Controller
    {
        [HttpGet]
        public ActionResult GetNotificationCheckListObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<NotificationCheckListObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetNotificationCheckLists(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationCheckListServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationCheckListObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationCheckListObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ItemScore : c.CheckListItem);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.CheckListItem, c.CriteriaRuleStr};
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
                return Json(new List<NotificationCheckListObject>(), JsonRequestBehavior.AllowGet);
            }
        }


        private ImporterObject GetLoggedOnUserInfo()
        {
            try
            {
                if (Session["_companyInfo"] == null)
                {
                    return new ImporterObject();
                }

                var companyInfo = Session["_companyInfo"] as ImporterObject;
                if (companyInfo == null || companyInfo.Id < 1)
                {
                    return new ImporterObject();
                }

                return companyInfo;

            }
            catch (Exception)
            {
                return new ImporterObject();
            }
        }

        [HttpPost]
        public ActionResult AddNotificationCheckList(NotificationCheckListObject notificationCheckList)
        {
            var gVal = new GenericValidator();

            try
            {
               

                var companyInfo = GetLoggedOnUserInfo();
                if (companyInfo.Id < 1)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var validationResult = ValidateNotificationCheckList(notificationCheckList);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new NotificationCheckListServices().AddNotificationCheckList(notificationCheckList);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "NotificationCheckList upload failed. Please try again." : "The NotificationCheckList Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "NotificationCheckList was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "NotificationCheckList notificationCheckListing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditNotificationCheckList(NotificationCheckListObject notificationCheckList)
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

                if (string.IsNullOrEmpty(notificationCheckList.CheckListItem.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide NotificationCheckList.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_NotificationCheckList"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldNotificationCheckList = Session["_NotificationCheckList"] as NotificationCheckListObject;

                if (oldNotificationCheckList == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                oldNotificationCheckList.CheckListItem = notificationCheckList.CheckListItem.Trim();

                var docStatus = new NotificationCheckListServices().UpdateNotificationCheckList(oldNotificationCheckList);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "NotificationCheckList information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldNotificationCheckList.Id;
                gVal.Error = "NotificationCheckList information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "NotificationCheckList information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<NotificationCheckListObject> GetNotificationCheckLists(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new NotificationCheckListServices().GetNotificationCheckLists(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<NotificationCheckListObject>();
            }
        }

        public ActionResult GetNotificationCheckList(long id)
        {
            try
            {


                var notificationCheckList = new NotificationCheckListServices().GetNotificationCheckList(id);
                if (notificationCheckList == null || notificationCheckList.Id < 1)
                {
                    return Json(new NotificationCheckListObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_NotificationCheckList"] = notificationCheckList;

                return Json(notificationCheckList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new NotificationCheckListObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteNotificationCheckList(long id)
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
                var delStatus = new NotificationCheckListServices().DeleteNotificationCheckList(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "NotificationCheckList could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "NotificationCheckList Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator ValidateNotificationCheckList(NotificationCheckListObject notificationCheckList)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(notificationCheckList.CheckListItem))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select provide NotificationCheckList.";
                    return gVal;
                }


                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "NotificationCheckList Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        public ActionResult GetGenericList()
        {
            try
            {
               
                var newList = new GenericNotificationCheckList()
                {
                   
                   CriteriaRules = getCriteriaRules()
                };
                

                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        
        //public ActionResult RefreshApplicationStage()
        //{
        //    try
        //    {
        //        return Json(getStagesWithProcesses(), JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception)
        //    {
        //        return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
        //    }
        //}
       

        private List<GenericObject> getCriteriaRules()
        {

            try
            {
                var itemList = EnumToObjList.ConvertEnumToList(typeof(EnumCheckListCriteriaRule));
                return itemList;

            }
            catch (Exception)
            {
                return new List<GenericObject>();
            }
        }


        private List<NotificationCheckListObject> getNotificationCheckLists()
        {

            try
            {
                return new NotificationCheckListServices().GetNotificationCheckLists();

            }
            catch (Exception)
            {
                return new List<NotificationCheckListObject>();
            }
        }

      

        //private List<ApplicationStageObject> getStagesWithProcesses()
        //{
        //    try
        //    {
        //        return new ImportStageServices().GetStagesWithProcesses();

        //    }
        //    catch (Exception)
        //    {
        //        return new List<ApplicationStageObject>();
        //    }
        //}


      



       

    }
}
