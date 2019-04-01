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
    public class ApplicationIssueController : Controller
    {
        [HttpGet]
        public ActionResult GetApplicationIssueObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ApplicationIssueObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetApplicationIssues(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationIssueServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationIssueObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationIssueObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : c.IssueDateStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), 
                                 c.ReferenceCode,c.CompanyName, c.IssueTypeName, c.Description,c.IssueDateStr};
                                  
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
                return Json(new List<ApplicationIssueObject>(), JsonRequestBehavior.AllowGet);
            }
        }
         [HttpGet]
        public ActionResult GetApplicationFromIssue(long id)
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

                return Json(GetApplicationInfoFromIssue(id, profileId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }
        }

        private ApplicationObject GetApplicationInfoFromIssue(long historyId, long userId)
        {
            return new ApplicationIssueServices().GetApplicationFromIssue(historyId, userId);
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
        public ActionResult AddApplicationIssue(ApplicationIssueObject applicationIssue)
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

                var validationResult = ValidateApplicationIssue(applicationIssue);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new ApplicationIssueServices().AddApplicationIssue(applicationIssue);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "ApplicationIssue upload failed. Please try again." : "The ApplicationIssue Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "ApplicationIssue was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "ApplicationIssue applicationIssueing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditApplicationIssue(ApplicationIssueObject applicationIssue)
        {
            var gVal = new GenericValidator();

            try
            {
                

                if (string.IsNullOrEmpty(applicationIssue.ReferenceCode.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide ApplicationIssue.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_ApplicationIssue"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldApplicationIssue = Session["_ApplicationIssue"] as ApplicationIssueObject;

                if (oldApplicationIssue == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                oldApplicationIssue.ReferenceCode = applicationIssue.ReferenceCode.Trim();

                var docStatus = new ApplicationIssueServices().UpdateApplicationIssue(oldApplicationIssue);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "ApplicationIssue information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldApplicationIssue.Id;
                gVal.Error = "ApplicationIssue information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "ApplicationIssue information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ApplicationIssueObject> GetApplicationIssues(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new ApplicationIssueServices().GetApplicationIssues(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<ApplicationIssueObject>();
            }
        }

        public ActionResult GetApplicationIssue(long id)
        {
            try
            {


                var applicationIssue = new ApplicationIssueServices().GetApplicationIssue(id);
                if (applicationIssue == null || applicationIssue.Id < 1)
                {
                    return Json(new ApplicationIssueObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_ApplicationIssue"] = applicationIssue;

                return Json(applicationIssue, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new ApplicationIssueObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteApplicationIssue(long id)
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
                var delStatus = new ApplicationIssueServices().DeleteApplicationIssue(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "ApplicationIssue could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "ApplicationIssue Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator ValidateApplicationIssue(ApplicationIssueObject applicationIssue)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(applicationIssue.ReferenceCode))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select provide ApplicationIssue.";
                    return gVal;
                }


                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "ApplicationIssue Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

     
      


      


       
      

     








    }
}
