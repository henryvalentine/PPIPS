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
    [Authorize(Roles = "Super_Admin")]
    public class IssueTypeController : Controller
    {
        [HttpGet]
        public ActionResult GetIssueTypeObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<IssueTypeObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetIssueTypes(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new IssueTypeServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<IssueTypeObject>(), JsonRequestBehavior.AllowGet);
                }

                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<IssueTypeObject, string> orderingFunction = (c => c.Name);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),c.Name};
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
                return Json(new List<IssueTypeObject>(), JsonRequestBehavior.AllowGet);
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
        public ActionResult AddIssueType(IssueTypeObject issueType)
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

                var validationResult = ValidateIssueType(issueType);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new IssueTypeServices().AddIssueType(issueType);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "IssueType upload failed. Please try again." : "Issue Type Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Issue Type was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Issue Type processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditIssueType(IssueTypeObject issueType)
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

                if (string.IsNullOrEmpty(issueType.Name.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Application Stage.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_issueType"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldissueType = Session["_issueType"] as IssueTypeObject;

                if (oldissueType == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                oldissueType.Name = issueType.Name.Trim();

                var docStatus = new IssueTypeServices().UpdateIssueType(oldissueType);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Application Stage information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldissueType.Id;
                gVal.Error = "Application Stage information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Application Stage information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<IssueTypeObject> GetIssueTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new IssueTypeServices().GetIssueTypes(itemsPerPage, pageNumber, out countG);
                
            }
            catch (Exception)
            {
                countG = 0;
                return new List<IssueTypeObject>(); 
            }
        }

        public ActionResult GetIssueType(long id)
        {
            try
            {


                var issueType = new IssueTypeServices().GetIssueType(id);
                if (issueType == null || issueType.Id < 1)
                {
                    return Json(new IssueTypeObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_issueType"] = issueType;

                return Json(issueType, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new IssueTypeObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteIssueType(long id)
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
                var delStatus = new IssueTypeServices().DeleteIssueType(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Issue Type could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Issue Type Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        private GenericValidator ValidateIssueType(IssueTypeObject issueType)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(issueType.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select provide Application Stage.";
                    return gVal;
                }


                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Issue Type Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
