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
    public class ApplicationContentController : Controller  
    {
        [Authorize(Roles = "Super_Admin")]
        [HttpGet]
        public ActionResult GetApplicationContentObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ApplicationContentObject> filteredParentMenuObjects;
                int countG;

                var pagedParentMenuObjects = GetApplicationContents(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationContentServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationContentObject>(), JsonRequestBehavior.AllowGet);
                }

                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationContentObject, string> orderingFunction = (c => c.Title);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.Title, c.Href, c.IsInUseStr};
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
                return Json(new List<ApplicationContentObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetApplicationContents()
        {
            try
            {
                var applicationContents = new ApplicationContentServices().GetApplicationContents();
                return Json(applicationContents, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<ApplicationContentObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetApplicationContent(int id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new ApplicationContentObject(), JsonRequestBehavior.AllowGet);
                }

                var applicationContent = new ApplicationContentServices().GetApplicationContent(id);
                return Json(applicationContent, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<ApplicationContentObject>(), JsonRequestBehavior.AllowGet);
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

        [Authorize(Roles = "Super_Admin")]
        [HttpPost]
        public ActionResult AddApplicationContent(ApplicationContentObject applicationContent)
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

                var validationResult = ValidateApplicationContent(applicationContent);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new ApplicationContentServices().AddApplicationContent(applicationContent);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Content could not be added. Please try again." : "The Content Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Content was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Content processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Super_Admin")]
        [HttpPost]
        public ActionResult EditApplicationContent(ApplicationContentObject applicationContent)
        {
            var gVal = new GenericValidator();

            try
            {
                var stat = ValidateApplicationContent(applicationContent);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_applicationContent"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldapplicationContent = Session["_applicationContent"] as ApplicationContentObject;

                if (oldapplicationContent == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldapplicationContent.Title = applicationContent.Title;
                oldapplicationContent.BodyContent = applicationContent.BodyContent;
                oldapplicationContent.Href = applicationContent.Href;
                oldapplicationContent.IsInUse = applicationContent.IsInUse;
                var docStatus = new ApplicationContentServices().UpdateApplicationContent(oldapplicationContent);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Content already exists." : "Content information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldapplicationContent.Id;
                gVal.Error = "Content information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Content information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Super_Admin")]
        public ActionResult GetApplicationContentForEdit(int id)
        {
            try
            {
                var applicationContent = new ApplicationContentServices().GetApplicationContent(id);
                if (applicationContent == null || applicationContent.Id < 1)
                {
                    return Json(new ApplicationContentObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_applicationContent"] = applicationContent;

                return Json(applicationContent, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new ApplicationContentObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Super_Admin")]
        [HttpPost]
        public ActionResult DeleteApplicationContent(long id)
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
                var delStatus = new ApplicationContentServices().DeleteApplicationContent(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Content could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Content Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ApplicationContentObject> GetApplicationContents(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new ApplicationContentServices().GetApplicationContents(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<ApplicationContentObject>();
            }
        }
        
        private GenericValidator ValidateApplicationContent(ApplicationContentObject applicationContent)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(applicationContent.Title))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Content Title.";
                    return gVal;
                }

                if (string.IsNullOrEmpty(applicationContent.BodyContent))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Content Body.";
                    return gVal;
                }

                if (string.IsNullOrEmpty(applicationContent.Href))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Content Link/Url.";
                    return gVal;
                }
                
                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Content Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
