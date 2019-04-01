using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web.Configuration;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Models;
using ImportPermitPortal.Services.Services;

namespace ImportPermitPortal.Controllers
{
    [Authorize]
    public class IssueController : Controller
    {

        [HttpGet]
        public ActionResult GetIssueObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<IssueObject> filteredParentMenuObjects;
                int countG;

                var pagedParentMenuObjects = GetIssues(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new IssueServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<IssueObject>(), JsonRequestBehavior.AllowGet);
                }

                Func<IssueObject, string> orderingFunction = (c => c.IssueCategoryName);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;
                
                var result = from c in displayedPersonnels select new[] { Convert.ToString(c.Id), c.IssueCategoryName,c.AffectedCompanyName, c.ResolvedByName, c.Status};

                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = countG,
                    iTotalDisplayRecords = countG,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<IssueObject>(), JsonRequestBehavior.AllowGet);
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

        public ActionResult AddIssue(IssueObject issue)
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

                var validationResult = ValidateIssue(issue);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new IssueServices().AddIssue(issue);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Issue could not be added. Please try again." : "The Issue Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Issue was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Issue processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ResolveIssue(IssueObject issue)
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

                var validationResult = ValidateIssue(issue);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                if (Session["_issue"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldissue = Session["_issue"] as IssueObject;

                if (oldissue == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldissue.ResolvedById = importerInfo.UserProfileObject.Id;
                oldissue.Status = issue.Status;

                var docStatus = new IssueServices().UpdateIssue(oldissue);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error =  "Issue could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = docStatus;
                gVal.Error = "You have successsfully marked this Issue as resolved.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

            catch (Exception)
            {
                gVal.Error = "Issue processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult SendSupportRequest(IssueObject issue)
        {
            var gVal = new GenericValidator();
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out. Please refresh the page.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var validationResult = ValidateIssue(issue);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                issue.IssueLogObject.DateCreated = DateTime.Now;
                issue.AffectedUserId = importerInfo.UserProfileObject.Id;
                issue.Status = IssueStatusEnum.Pending.ToString();

                var appStatus = new IssueServices().AddIssue(issue);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = "Your request/complaint could not be processed. Please try again.";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                if (Request.Url != null)
                {

                    #region Using SendGrid

                    var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                    var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                    if (settings == null)
                    {
                        gVal.Code = -1;
                        gVal.Error = "Internal server error. Your request could not be processed. Please try again.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    var str = "<b>Issue Category : </b> "  + "<b>" +  issue.IssueCategoryName + "</b>";
                    str += "<b>Affected Company : </b> " + "<b>" + importerInfo.Name + "</b>";
                    str += "<b>Request/Issue : </b> " + "<b>" + issue.IssueLogObject.Issue + "</b>";

                    var mail = new MailMessage(new MailAddress(settings.Smtp.From), new MailAddress("ppips@dpr.gov.ng")) 
                    {
                        Subject = issue.IssueCategoryName,
                        Body = str,
                        IsBodyHtml = true
                    };

                    var smtp = new SmtpClient(settings.Smtp.Network.Host)
                    {
                        Credentials = new NetworkCredential(settings.Smtp.Network.UserName, settings.Smtp.Network.Password),
                        EnableSsl = true,
                        Port = settings.Smtp.Network.Port
                    };

                    smtp.Send(mail);
                    gVal.Code = 5;
                    gVal.Error = "Your message has been sent. Be rest assured it will be handled as soon as possible.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                    #endregion

                }

                gVal.Code = -1;
                gVal.Error = "Internal server error. Your request could not be processed. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                gVal.Code = -1;
                gVal.Error = "Internal server error. Your request could not be processed. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditIssue(IssueObject issue)
        {
            var gVal = new GenericValidator();

            try
            {
                var stat = ValidateIssue(issue);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_issue"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldissue = Session["_issue"] as IssueObject;

                if (oldissue == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldissue.AffectedUserId = issue.AffectedUserId;
                oldissue.IssueLogId = issue.IssueLogId;
                oldissue.ResolvedById = issue.ResolvedById;
                oldissue.Status = issue.Status;

                var docStatus = new IssueServices().UpdateIssue(oldissue);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Issue already exists." : "Issue could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldissue.Id;
                gVal.Error = "Issue was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Issue could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
       
        public ActionResult GetIssue(long id)
        {
            try
            {
                var issue = new IssueServices().GetIssue(id);
                if (issue == null || issue.Id < 1)
                {
                    return Json(new IssueObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_issue"] = issue;

                return Json(issue, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new IssueObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetIssueCategories()
        {
            try
            {
                var issues = new IssueCategoryServices().GetIssueCategories();
                if (!issues.Any())
                {
                    return Json(new List<IssueCategoryObject>(), JsonRequestBehavior.AllowGet);
                }

                return Json(issues, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new IssueObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteIssue(long id)
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
                var delStatus = new IssueServices().DeleteIssue(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Issue could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Issue Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<IssueObject> GetIssues(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new IssueServices().GetIssues(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<IssueObject>();
            }
        }

        private GenericValidator ValidateIssue(IssueObject issue)
        {
            var gVal = new GenericValidator();
            try
            {
                if (issue.IssueLogObject.IssueCategoryId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select Issue Category.";
                    return gVal;
                }

                if (string.IsNullOrEmpty(issue.IssueLogObject.Issue))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide you complaint.";
                    return gVal;
                }
                
                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Issue Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
