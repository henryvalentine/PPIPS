using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Models;
using ImportPermitPortal.Services.Services;
using Mandrill;
using Mandrill.Model;
using Newtonsoft.Json;
using WebGrease.Css.Extensions;

namespace ImportPermitPortal.Controllers
{
    [Authorize]
    public class ApplicationController : Controller
    {
        [HttpGet]
        [Authorize(Roles = "Applicant")]
        public ActionResult GetApplicationObjects(JQueryDataTableParamModel param)
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }
                IEnumerable<ApplicationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetApplications(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchByCompany(param.sSearch, importerInfo.Id);
                    countG = filteredParentMenuObjects.Count();

                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;

                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                  Func<ApplicationObject, string> orderingFunction = (c => sortColumnIndex== 1? c.ReferenceCode : sortColumnIndex == 2?  c.DerivedTotalQUantity.ToString(CultureInfo.InvariantCulture):
                    sortColumnIndex == 3 ? c.DerivedValue.ToString(CultureInfo.InvariantCulture) : sortColumnIndex == 4 ?  c.DateAppliedStr : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;
                
                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.DerivedQuantityStr, c.DerivedValue.ToString("N", CultureInfo.InvariantCulture),
                                 c.DateAppliedStr, c.StatusStr
                             };
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
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector,Verifier")]
        public ActionResult GetApplicationProcesses(long applicationId)
        {
            try
            {
                if (applicationId < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }
               
               var appProcess = new ApplicationServices().GetApplicationProcesses(applicationId);
               return Json(appProcess, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetAdminCounts()
        {
            try
            {
                var appCounts = new ApplicationServices().GetAdminCounts();
                return Json(appCounts, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Applicant")]
        public ActionResult GetAppCount()
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var appCounts = new ApplicationServices().GetCounts(importerInfo.Id);
                return Json(appCounts, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        
         [HttpGet]
        [Authorize(Roles = "Depot_Owner")]
        public ActionResult GetDepotOwnerCounts()
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }
                
                var appCounts = new ApplicationServices().GetDepotOwnerCounts(importerInfo.Id, importerInfo.UserProfileObject.Id);
                return Json(appCounts, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

         [HttpGet]
         [Authorize(Roles = "Verifier")]
         public ActionResult GetVerifierCounts()
         {
             try
             {
                 var appCounts = new ApplicationServices().GetVerifierCounts();
                 return Json(appCounts, JsonRequestBehavior.AllowGet);
             }
             catch (Exception ex)
             {
                 ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                 return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
             }
         }

         [HttpGet]
         [Authorize(Roles = "Banker,Bank_User")]
         public ActionResult GetBankerCounts()
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var appCounts = new ApplicationServices().GetBankerCounts(importerInfo.Id, importerInfo.UserProfileObject.Id);
                return Json(appCounts, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }  
        
        [HttpGet]
        [Authorize(Roles = "Depot_Owner")]
        public ActionResult GetDepotOwnerItems(JQueryDataTableParamModel param)
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<ApplicationItemObject>(), JsonRequestBehavior.AllowGet);
                }
                IEnumerable<ApplicationItemObject> filteredParentMenuObjects;
                int countG;

                var pagedParentMenuObjects = new ApplicationServices().GetDepotAssignedApplicationItems(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchDepotOwnerApplicationItems(param.sSearch, importerInfo.Id);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationItemObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationItemObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ImporterName : sortColumnIndex == 2 ? c.ProductName :
                    sortColumnIndex == 3 ? c.CountryOfOriginName : sortColumnIndex == 4 ? c.EstimatedQuantityStr : sortColumnIndex == 5 ?  c.EstimatedValueStr : c.StatusStr);

                var sortDirection = Request["sSortDir_0"];
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                              c.ImporterName, c.ProductName, c.CountryOfOriginName,c.EstimatedQuantityStr, c.EstimatedValueStr, c.StatusStr
                             };
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
                return Json(new List<ApplicationItemObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Depot_Owner")]
        public ActionResult GetDepotOwnerHistory(JQueryDataTableParamModel param)
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<ApplicationItemObject>(), JsonRequestBehavior.AllowGet);
                }
                IEnumerable<ApplicationItemObject> filteredParentMenuObjects;
                int countG;

                var pagedParentMenuObjects = new ApplicationServices().GetDepotAssignedApplicationHistory(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);

                if (!string.IsNullOrEmpty(param.sSearch))  
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchDepotOwnerApplicationHistory(param.sSearch, importerInfo.Id);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationItemObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationItemObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ImporterName : sortColumnIndex == 2 ? c.ProductName :
                    sortColumnIndex == 3 ? c.CountryOfOriginName : sortColumnIndex == 4 ? c.EstimatedQuantityStr : sortColumnIndex == 5 ? c.EstimatedValueStr : c.StatusStr);

                var sortDirection = Request["sSortDir_0"];
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                              c.ImporterName, c.ProductName, c.CountryOfOriginName,c.EstimatedQuantityStr, c.EstimatedValueStr, c.StatusStr
                             };
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
                return Json(new List<ApplicationItemObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetEmployeeApplicationProcesses(long employeeId)
        {
            try
            {
                var appProcesses = new ApplicationServices().GetEmployeeApplicationProcesses(employeeId);
                
                return Json(appProcesses, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetJobDistributions()
        {
            try
            {
                var jobDistributions = new ApplicationServices().GetJobDistributions();
                return Json(jobDistributions, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetAdminApplications(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ApplicationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetApplications(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchPaidApplications(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.ImporterStr : sortColumnIndex == 3 ? c.AppTypeStr : sortColumnIndex == 4 ? c.ImportClassName : sortColumnIndex == 5 ? c.DerivedQuantityStr:
                  sortColumnIndex == 6 ? c.LastModifiedStr : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.ImporterStr, c.AppTypeStr, c.ImportClassName, c.DerivedQuantityStr, c.LastModifiedStr, c.StatusStr
                                 
                             };
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
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetPaidApplications(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ApplicationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new ApplicationServices().GetPaidApplications(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchPaidApplications(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.DerivedTotalQUantity.ToString(CultureInfo.InvariantCulture) :
                  sortColumnIndex == 3 ? c.DerivedValue.ToString(CultureInfo.InvariantCulture) : sortColumnIndex == 4 ? c.DateAppliedStr : sortColumnIndex == 4 ? c.StatusStr : c.LastModifiedStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.ImporterStr, c.DerivedQuantityStr, c.DerivedValue.ToString("N", CultureInfo.InvariantCulture),
                                 c.DateAppliedStr, c.LastModifiedStr, c.StatusStr

                             };
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
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetApplicationsPendingSubmission(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ApplicationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new ApplicationServices().GetApplicationsPendingSubmission(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchApplicationsPendingSubmission(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.DerivedQuantityStr.ToString(CultureInfo.InvariantCulture) :
                  sortColumnIndex == 3 ? c.DerivedValueStr : sortColumnIndex == 4 ? c.DateAppliedStr : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.ImporterStr, c.DerivedQuantityStr, c.DerivedValueStr,
                                 c.DateAppliedStr, c.StatusStr
                             };
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
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetSubmittedApplications(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ApplicationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new ApplicationServices().GetSubmittedApplications(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchSubmittedApplications(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.DerivedTotalQUantity.ToString(CultureInfo.InvariantCulture) :
                  sortColumnIndex == 3 ? c.DerivedValue.ToString(CultureInfo.InvariantCulture) : sortColumnIndex == 4 ? c.DateAppliedStr : sortColumnIndex == 4 ? c.StatusStr : c.LastModifiedStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.ImporterStr, c.DerivedQuantityStr, c.DerivedValue.ToString("N", CultureInfo.InvariantCulture),
                                 c.DateAppliedStr, c.LastModifiedStr, c.StatusStr

                             };
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
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetProcessingApplications(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ApplicationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new ApplicationServices().GetProcessingApplications(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchProcessingApplications(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.DerivedTotalQUantity.ToString(CultureInfo.InvariantCulture) :
                  sortColumnIndex == 3 ? c.DerivedValue.ToString(CultureInfo.InvariantCulture) : sortColumnIndex == 4 ? c.DateAppliedStr : sortColumnIndex == 4 ? c.StatusStr : c.LastModifiedStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.ImporterStr, c.DerivedQuantityStr, c.DerivedValue.ToString("N", CultureInfo.InvariantCulture),
                                 c.DateAppliedStr, c.LastModifiedStr, c.StatusStr

                             };
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
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector,Verifier")]
        public ActionResult GetApprovedApplications(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ApplicationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new ApplicationServices().GetApprovedApplications(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchApprovedApplications(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.DerivedTotalQUantity.ToString(CultureInfo.InvariantCulture) :
                  sortColumnIndex == 3 ? c.DerivedValue.ToString(CultureInfo.InvariantCulture) : sortColumnIndex == 4 ? c.DateAppliedStr : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.ImporterStr, c.DerivedQuantityStr, c.DerivedValue.ToString("N", CultureInfo.InvariantCulture),
                                 c.DateAppliedStr, c.StatusStr

                             };
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
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Verifier,DownstreamDirector")]
        public ActionResult GetApplicationInVerification(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ApplicationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new ApplicationServices().GetApplicationsInVerification(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchApplicationsInVerification(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.DerivedTotalQUantity.ToString(CultureInfo.InvariantCulture) :
                  sortColumnIndex == 3 ? c.DerivedValue.ToString(CultureInfo.InvariantCulture) : sortColumnIndex == 4 ? c.DateAppliedStr : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.ImporterStr, c.DerivedQuantityStr, c.DerivedValue.ToString("N", CultureInfo.InvariantCulture),
                                 c.DateAppliedStr, c.StatusStr, c.IsSignOffDocUploaded.ToString()

                             };
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
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetRejectedApplications(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ApplicationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new ApplicationServices().GetRejectedApplications(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchRejectedApplications(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.DerivedTotalQUantity.ToString(CultureInfo.InvariantCulture) :
                  sortColumnIndex == 3 ? c.DerivedValue.ToString(CultureInfo.InvariantCulture) : sortColumnIndex == 4 ? c.DateAppliedStr : sortColumnIndex == 4 ? c.StatusStr : c.LastModifiedStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.ImporterStr, c.DerivedQuantityStr, c.DerivedValue.ToString("N", CultureInfo.InvariantCulture),
                                 c.DateAppliedStr, c.LastModifiedStr, c.StatusStr

                             };
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
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector,Verifier")]
        public ActionResult GetApplicationAdmin(long id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetApplicationAdminObj(id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetPaidApplicationAdmin(long id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetPaidApplicationObj(id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpGet]
        [Authorize(Roles = "Banker,Bank_User")]
        public ActionResult GetBankAssignedApplications(JQueryDataTableParamModel param)
        {
            try
            { //SearchBankJobHistory
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }
                IEnumerable<ApplicationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetAssignedApplications(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchByBankAssignedApplications(param.sSearch, importerInfo.Id);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.ImporterStr : sortColumnIndex == 3 ? c.AppTypeStr : sortColumnIndex == 4 ? c.ImportClassName : sortColumnIndex == 5 ? c.DerivedQuantityStr :
                  sortColumnIndex == 6 ? c.LastModifiedStr : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.ImporterStr, c.AppTypeStr, c.ImportClassName, c.DerivedQuantityStr, c.LastModifiedStr, c.StatusStr

                             };
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
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Banker")]
        public ActionResult GetBankJobHistory(JQueryDataTableParamModel param)
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }
                IEnumerable<ApplicationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetBankJobHistory(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchBankJobHistory(param.sSearch, importerInfo.Id);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.DerivedTotalQUantity.ToString(CultureInfo.InvariantCulture) :
                  sortColumnIndex == 3 ? c.DerivedValue.ToString(CultureInfo.InvariantCulture) : sortColumnIndex == 4 ? c.DateAppliedStr : sortColumnIndex == 4 ? c.StatusStr : c.LastModifiedStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode,c.ImporterStr, c.DerivedQuantityStr, c.DerivedValue.ToString("N", CultureInfo.InvariantCulture),
                                 c.DateAppliedStr, c.StatusStr
                             };

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
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Bank_User")]
        public ActionResult GetBankUserJobHistory(JQueryDataTableParamModel param)
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }
                
                IEnumerable<ApplicationObject> filteredParentMenuObjects;
                var countG = 0;
                 
                var pagedParentMenuObjects = GetBankUserJobHistory(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.UserProfileObject.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchBankUserJobHistory(param.sSearch, importerInfo.UserProfileObject.Id);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApplicationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.DerivedTotalQUantity.ToString(CultureInfo.InvariantCulture) :
                  sortColumnIndex == 3 ? c.DerivedValue.ToString(CultureInfo.InvariantCulture) : sortColumnIndex == 4 ? c.DateAppliedStr : sortColumnIndex == 4 ? c.StatusStr : c.LastModifiedStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode,c.ImporterStr, c.DerivedQuantityStr, c.DerivedValue.ToString("N", CultureInfo.InvariantCulture),
                                 c.DateAppliedStr, c.StatusStr
                             };
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
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult GetImporterApplications(JQueryDataTableParamModel param, long id)
        {
            try
            {
                //var id = GetImporterId();
                if (id < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }
                
                IEnumerable<ApplicationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetApplications(param.iDisplayLength, param.iDisplayStart, out countG, id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ApplicationServices().SearchByCompany(param.sSearch, id);
                    countG = filteredParentMenuObjects.Count();

                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;

                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                  Func<ApplicationObject, string> orderingFunction = (c => sortColumnIndex== 1? c.ReferenceCode : sortColumnIndex == 2?  c.DerivedTotalQUantity.ToString(CultureInfo.InvariantCulture):
                    sortColumnIndex == 3 ? c.DerivedValue.ToString(CultureInfo.InvariantCulture) : sortColumnIndex == 4 ?  c.DateAppliedStr : sortColumnIndex== 4 ? c.StatusStr : c.LastModifiedStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;
                
                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.DerivedQuantityStr, c.DerivedValue.ToString("N", CultureInfo.InvariantCulture),
                                 c.DateAppliedStr, c.LastModifiedStr, c.StatusStr, c.ImporterStr
                             };
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
                return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetBankAssignedAppDocuments(long id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }


                return Json(GetBankAssignedAppDocs(id, importerInfo.Id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ViewApplication(), JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult GetAppDocuments(long id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetAppDocs(id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ViewApplication(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetApplicationEmployees(long applicationId)
        {
            try
            {
                if (applicationId < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }

                return Json(new ApplicationServices().GetApplicationEmployees(applicationId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ViewApplication(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAppDocsByRef(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }
                var bankerApp = GetBankerApp(code, importerInfo.Id);
                return Json(bankerApp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ViewApplication(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetDepotOwnerApplicationItem(long id)
        {
            if (id < 1)
            {
                return Json(new ApplicationItemObject(), JsonRequestBehavior.AllowGet);
            }

            var importerInfo = GetLoggedOnUserInfo();
            if (importerInfo == null || importerInfo.Id < 1)
            {
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }

            var app = new ApplicationServices().GetDepotOwnerApplicationItem(id, importerInfo.Id);

            if (app == null || app.Id < 1)
            {
                return Json(new ApplicationItemObject(), JsonRequestBehavior.AllowGet);
            }
            Session["_throughPut"] = app;
            return Json(app, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAppXDocuments(long id)
        {
            try
            {
                var baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority;

                if (id < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                  return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }
                
                var app = GetAppDocumentsX(id);
                if (app.Id < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }

                if (app.ApplicationStatusCode < 2)
                {
                    var url = "";
                    if (app.PaymentTypeId == (int)PaymentType.Bank)
                    {
                        url = baseUrl + "/Transaction/Invoice";
                    }
                    else
                    {
                        url = baseUrl + "/Transaction/WebPayment";
                    }

                    if (string.IsNullOrEmpty(app.Rrr))
                    {
                        var rrr = new SplitPayments().PostSplitPaymentForApplicationFee(app.DerivedValue, app.ReferenceCode, importerInfo.Name, importerInfo.UserProfileObject.Email, importerInfo.UserProfileObject.PhoneNumber, url, app.FeeObjects);
                        if (rrr.Code < 1 || rrr.Error.Contains("<"))
                        {
                            return Json(app, JsonRequestBehavior.AllowGet);
                        }

                        new InvoiceServices().UpdateInvoiceRrr(app.ReferenceCode, rrr.Error);
                        app.Rrr = rrr.Error;
                    }

                    var hash = Hasher.GenerateHash(app.Rrr);
                    app.Hash = hash;
                    app.RedirectUri = url;
                }

                return Json(app, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }
        }
    
        public ActionResult GetAppForEdit(long id)
         {
             var baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority;

             if (id < 1) 
             {
                 return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
             }

             var app = new ApplicationServices().GetAppForEdit(id);

             if (app == null || app.Id < 1)
             {
                 return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
             }

            var importerInfo = GetLoggedOnUserInfo();
            if (importerInfo.Id < 1)
            {
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }
                
            if (string.IsNullOrEmpty(app.Rrr))
            {
                var url = "";
                if (app.PaymentTypeId == (int)PaymentType.Bank)
                {
                    url = baseUrl + "/Transaction/Invoice";
                }
                else
                {
                    url = baseUrl + "/Transaction/WebPayment";
                }
                    
                var rrr = new SplitPayments().PostSplitPaymentForApplicationFee(app.DerivedValue, app.ReferenceCode, importerInfo.Name, importerInfo.UserProfileObject.Email, importerInfo.UserProfileObject.PhoneNumber, url, app.FeeObjects);
                if (rrr.Code > 0 && !rrr.Error.Contains("<"))
                {
                    new InvoiceServices().UpdateInvoiceRrr(app.ReferenceCode, rrr.Error);
                    app.Rrr = rrr.Error;
                }
            }

             Session["_app"] = app;
             return Json(app, JsonRequestBehavior.AllowGet);
         }

        public ActionResult GetAppForPayment(long id)
        {
            var baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority;

            if (id < 1)
            {
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }

            var app = new ApplicationServices().GetAppForPayment(id);

            if (app == null || app.Id < 1)
            {
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }

            var importerInfo = GetLoggedOnUserInfo();
            if (importerInfo.Id < 1)
            {
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(app.Rrr))
            {
                var url = "";
                if (app.PaymentTypeId == (int)PaymentType.Bank)
                {
                    url = baseUrl + "/Transaction/Invoice";
                }
                else
                {
                    url = baseUrl + "/Transaction/WebPayment";
                }

                var rrr = new SplitPayments().PostSplitPaymentForApplicationFee(app.DerivedValue, app.ReferenceCode, importerInfo.Name, importerInfo.UserProfileObject.Email, importerInfo.UserProfileObject.PhoneNumber, url, app.FeeObjects);
                if (rrr.Code > 0 && !rrr.Error.Contains("<"))
                {
                    new InvoiceServices().UpdateInvoiceRrr(app.ReferenceCode, rrr.Error);
                    app.Rrr = rrr.Error;
                    app.Hash = Hasher.GenerateHash(app.Rrr);
                }
                else
                {
                    app.Rrr = "Not Available";
                }
            }

            return Json(app, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAppBankerInfo(long productId, string permitValue)
         {
             var gVal = new GenericValidator();
            try
            {
                if (productId < 1 || string.IsNullOrEmpty(permitValue))
                {
                    return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.UserProfileObject.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var app = new ApplicationServices().GetAppBankerInfo(productId, importerInfo.Id, permitValue);

                if (app == null || app.Id < 1)
                {
                    return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
                }
                Session["_appBankerInfo"] = app;
                return Json(app, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Application information could not be fully retrieved. Please try again later.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
         }
        
        [HttpPost]
        public ActionResult AddAppBanker(NotificationBankerObject applicationBanker )
          {
              var gVal = new GenericValidator();
              try
              {
                  var validationStatus = ValidateNotificationBanker(applicationBanker);
                  if (validationStatus.Code < 1)
                  {
                      gVal.Code = -1;
                      gVal.Error = validationStatus.Error;
                      return Json(gVal, JsonRequestBehavior.AllowGet);
                  }

                  var importerInfo = GetLoggedOnUserInfo();
                  if (importerInfo.UserProfileObject.Id < 1)
                  {
                      gVal.Code = -1;
                      gVal.Error = "Your session has timed out.";
                      return Json(gVal, JsonRequestBehavior.AllowGet);
                  }

                  if (string.IsNullOrEmpty(applicationBanker.FinLetterPath))
                  {
                      gVal.Code = -1;
                      gVal.Error = "Error: The Process failed. Please try again";
                      return Json(gVal, JsonRequestBehavior.AllowGet);
                  }

                  if (applicationBanker.BankId < 1)
                  {
                      gVal.Code = -1;
                      gVal.Error = "The process failed. Please try again.";
                      return Json(gVal, JsonRequestBehavior.AllowGet);
                  }

                  var path = MoveFile(importerInfo.UserProfileObject.Id, applicationBanker.FinLetterPath);
                  if (string.IsNullOrEmpty(path))
                  {
                      gVal.Error = "Document processing failed. Please try again.";
                      gVal.Code = -1;
                      return Json(gVal, JsonRequestBehavior.AllowGet);
                  }

                  var document = new DocumentObject
                  {
                      ImporterId = applicationBanker.ImporterId,
                      ApplicationId = applicationBanker.NotificationId,
                      NotificationId = applicationBanker.NotificationId,
                      DateUploaded = DateTime.Now,
                      Status = (int)AppStatus.Pending,
                      DocumentTypeId = (int)SpecialDocsEnum.Telex_Copy,
                      UploadedById = importerInfo.UserProfileObject.Id,
                      DocumentPath = path
                  };

                  var docStatus = new DocumentServices().AddNotificationDocument(document);
                  if (docStatus < 1)
                  {
                      gVal.Code = -1;
                      gVal.Error = "The process failed. Please try again.";
                      return Json(gVal, JsonRequestBehavior.AllowGet);
                  }
                  
                  applicationBanker.DateAdded = DateTime.Now;
                  applicationBanker.LastUpdateBy = importerInfo.UserProfileObject.Id;
                  var ip = ClientIpHelper.GetClientIpAddress(Request);
                  applicationBanker.IpAddress = ip;
                  applicationBanker.AttachedDocumentId = docStatus;

                  var stat = new NotificationBankerServices().AddNotificationBanker(applicationBanker);
                 
                  if (stat < 1)
                  {
                      if (!string.IsNullOrEmpty(applicationBanker.FinLetterPath))
                      {
                          DeleteFile(applicationBanker.FinLetterPath);
                      }

                      gVal.Code = -1;
                      gVal.Error = "Your Banker information could not be processed. Please try again.";
                      return Json(gVal, JsonRequestBehavior.AllowGet);
                  }

                  gVal.Code = stat;
                  gVal.Error = "Your Banker information was successfully processed.";
                  return Json(gVal, JsonRequestBehavior.AllowGet);
              }
              catch (Exception)
              {
                  if (!string.IsNullOrEmpty(applicationBanker.FinLetterPath))
                  {
                      DeleteFile(applicationBanker.FinLetterPath);
                  }
                  gVal.Code = -1;
                  gVal.Error = "Your Banker information could not be processed. Please try again.";
                  return Json(gVal, JsonRequestBehavior.AllowGet);
              }
        }
        
        [HttpPost]
        public ActionResult EditAppBanker(NotificationBankerObject applicationBanker)
        {
            var gVal = new GenericValidator();
            try
            {
                var valStatus = ValidateNotificationBanker(applicationBanker);
                if (valStatus.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = valStatus.Error;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.UserProfileObject.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_appBankerInfo"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your Session has expired";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                var appBanker = Session["_appBankerInfo"] as NotificationBankerObject;
                if (appBanker == null || appBanker.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your Session has expired";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (applicationBanker.BankId < 1 || applicationBanker.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "The process failed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldAppBanker = appBanker;
                oldAppBanker.ProductId = applicationBanker.ProductId;
                oldAppBanker.FinancedQuantity = applicationBanker.FinancedQuantity;
                oldAppBanker.TransactionAmount = applicationBanker.TransactionAmount;
                oldAppBanker.ActualQuantity = applicationBanker.ActualQuantity;
                oldAppBanker.LastUpdateBy = importerInfo.UserProfileObject.Id;
                var ip = ClientIpHelper.GetClientIpAddress(Request);
                oldAppBanker.IpAddress = ip;

                var contactid = new NotificationBankerServices().UpdateNotificationBanker(oldAppBanker);
                if (contactid < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Process could not be completed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = 5;
                gVal.Error = "Process was successfully completed.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Process could not be completed. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateFinLtFile(HttpPostedFileBase file)
        {
            var gVal = new GenericValidator();
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.UserProfileObject.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                if (Session["_appBankerInfo"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your Session has expired";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                var appBanker = Session["_appBankerInfo"] as NotificationBankerObject;
                if (appBanker == null || appBanker.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your Session has expired";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var doc = appBanker.DocumentObject;

                if (doc == null || doc.DocumentId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your Session has expired";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var filePath = SaveFile(doc.DocumentPath, file);

                if (string.IsNullOrEmpty(filePath))
                {
                    gVal.Code = -1;
                    gVal.Error = "Document processing failed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                doc.DocumentPath = filePath;
                doc.DateUploaded = DateTime.Now;
                doc.UploadedById = importerInfo.UserProfileObject.Id;

                var docStatus = new DocumentServices().UpdateBankerDocument(doc);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                gVal.Code = 5;
                gVal.Path = filePath;
                gVal.Error = "Document was successfully Processed.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Error = "Process failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        public string SaveFile(string formerPath, HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    var mainPath = Server.MapPath("~/ImportDocuments");

                    if (!Directory.Exists(mainPath))
                    {
                        Directory.CreateDirectory(mainPath);
                        var dInfo = new DirectoryInfo(mainPath);
                        var dSecurity = dInfo.GetAccessControl();
                        dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                        dInfo.SetAccessControl(dSecurity);
                    }
                    var path = "";
                    if (SaveToFolder(file, ref path, mainPath, formerPath))
                    {
                        return PhysicalToVirtualPathMapper.MapPath(path);
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return string.Empty;
            }
        }
        
        private bool SaveToFolder(HttpPostedFileBase file, ref string path, string folderPath, string formerFilePath = null)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    var fileExtension = Path.GetExtension(file.FileName);
                    var fileName = GenerateUniqueName() + fileExtension;
                    var newPathv = Path.Combine(folderPath, fileName);
                    file.SaveAs(newPathv);
                    if (!string.IsNullOrWhiteSpace(formerFilePath))
                    {
                        if (!DeleteFile(formerFilePath))
                        {
                            return false;
                        }
                    }
                    path = newPathv;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return false;
            }
        }

        private static string GenerateUniqueName()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid();
        }


        [HttpPost]
        public ActionResult VerifyCode(LicenseRefObject verifier)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(verifier.RefCode) || verifier.LicenseType < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide all requirements and try again";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                verifier.ImporterId = importerInfo.Id;
                var res = new ProductServices().VerifyCode(verifier);
                if (res < 1)
                {
                    gVal.Code = -1;
                    if (verifier.LicenseType == (int)RefLicenseTypeEnum.LPG_Plant_License)
                    {
                        gVal.Error = "The provided LPG License Number is invalid.";
                        
                    }

                    if (verifier.LicenseType == (int)RefLicenseTypeEnum.PSF)
                    {
                        gVal.Error = "The provided PSF Number is invalid.";
                    }
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = res;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Error = "Process failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
         [HttpPost]
        public ActionResult VerifyVesselLicense(LicenseRefObject verifier)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(verifier.RefCode) || verifier.LicenseType < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Shuttle Vessel License Number";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                var res = new ProductServices().VerifyVesselLicense(verifier);
                if (res < 1)
                {
                    gVal.Code = -1;
                    if (verifier.LicenseType == (int)RefLicenseTypeEnum.LPG_Plant_License)
                    {
                        gVal.Error = "The provided Shuttle Vessel License Number is invalid.";
                        
                    }
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = res;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Error = "Process failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
         public ActionResult VerifyDepotLicenseCode(LicenseRefObject verifier)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(verifier.RefCode) || verifier.LicenseType < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Shuttle Vessel License Number";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out. Please refresh the page.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                verifier.ImporterName = importerInfo.Name;
                verifier.ImporterId = importerInfo.Id;
                var res = new ProductServices().VerifyDepotLicenseCode(verifier);
                if (res < 1)
                {
                    gVal.Code = -1;
                    if (verifier.LicenseType == (int)RefLicenseTypeEnum.LPG_Plant_License)
                    {
                        gVal.Error = "The provided Shuttle Vessel License Number is invalid.";
                    }
                    else
                    {
                        gVal.Error = "The provided Depot License Number is invalid.";
                    }

                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = res;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Error = "Process failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateBankAccounts(List<ProductBankerObject> bankers)
        {
            var gVal = new GenericValidator();
            try
            {
                if (bankers.Any(bnk => string.IsNullOrEmpty(bnk.BankAccountNumber)))
                {
                    gVal.ErrorCode = -1;
                    gVal.Error = "Please provide your Bank Account(s) for the selected Sponsoring Bank(s).";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var tt = new ApplicationServices().UpdateBankAccounts(bankers);
                if (tt < 1)
                {
                    gVal.ErrorCode = -1;
                    gVal.Error = "Process could not be completed. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.ErrorCode = 5;
                gVal.Error = "Process was successfully completed.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                gVal.Code = -1;
                gVal.Error = "The request could not be processed. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Application(ApplicationObject importApplication)
        {
            var baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority;

            var gVal = new GenericValidator();
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.ErrorCode = -1;
                    gVal.Error = "Your session has timed out. Please refresh the page.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (!importApplication.ApplicationItemObjects.Any())
                {
                    gVal.Code = -1;
                    gVal.Error = "Plese provide all required fields and try again.";
                    gVal.ErrorCode = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var validationResult = ValidateApplication(importApplication.ApplicationItemObjects.ToList());
                if (validationResult.Any())
                {
                    gVal.Code = -1;
                    gVal.Error = "Plese Provide all required fields and try again.";
                    gVal.ErrorCode = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                const int statutory = (int)FeeTypeEnum.Statutory_Fee;
                const int processing = (int)FeeTypeEnum.Processing_Fee;

                var statutoryFee = importApplication.FeeObjects.Find(f => f.FeeTypeId == statutory);
                var processingFee = importApplication.FeeObjects.Find(f => f.FeeTypeId == processing);

                if (statutoryFee == null || statutoryFee.Amount < 1 || processingFee == null || processingFee.Amount < 1)
                {
                    gVal.Code = -1;
                    gVal.ErrorCode = -1;
                    gVal.Error = "An internal server error was encountered. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                foreach (var item in importApplication.ApplicationItemObjects)
                {
                    if (item.StorageProviderTypeId == (int)StorageProviderTypeEnum.Own_Depot)
                    {
                        if (!string.IsNullOrEmpty(item.DepotLicense))
                        {
                            var verifier = new LicenseRefObject
                            {
                                LicenseType = (int)StorageProviderTypeEnum.Own_Depot,
                                RefCode = item.DepotLicense,
                                ImporterId = importerInfo.Id
                            };
                            var vv = new ProductServices().VerifyCode(verifier);
                            if (vv < 1)
                            {
                                gVal.Code = -1;
                                gVal.ErrorCode = -1;
                                gVal.Error = "The provided Depot License is invalid.";
                                return Json(gVal, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            gVal.Code = -1;
                            gVal.ErrorCode = -1;
                            gVal.Error = "Please provide your Depot License";
                            return Json(gVal, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (item.ProductBankerObjects.Any(bnk => string.IsNullOrEmpty(bnk.BankAccountNumber)))
                    {
                        gVal.Code = -1;
                        gVal.ErrorCode = -1;
                        gVal.Error = "Please provide your Bank Account(s) for the selected Sponsoring Bank(s).";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }
                }

                importApplication.ImporterId = importerInfo.Id;
                importApplication.DateApplied = DateTime.Now;
                importApplication.LastModified = DateTime.Now;
                //importApplication.ApplicationStatusCode = (int)AppStatus.Pending;
                importApplication.ApplicationStatusCode = (int)AppStatus.Paid;
                importApplication.DerivedTotalQUantity = importApplication.DerivedTotalQUantity;
                importApplication.DerivedValue = importApplication.DerivedValue;
                
                var invoice = new InvoiceObject
                {
                    PaymentTypeId = importApplication.PaymentTypeId,
                    TotalAmountDue = importApplication.DerivedValue,
                    AmountPaid = 0,
                    ServiceDescriptionId = (int)ServiceDescriptionEnum.Import_Permit_Application_Fee,
                    //Status = (int)AppStatus.Pending,
                    Status = (int)AppStatus.Paid,
                    ImporterId = importerInfo.Id,
                    IPAddress = ClientIpHelper.GetClientIpAddress(Request),
                    DateAdded = DateTime.Now,
                    InvoiceItemObjects = new List<InvoiceItemObject>()
                };
                
                importApplication.FeeObjects.ForEach(l =>
                {
                    invoice.InvoiceItemObjects.Add(new InvoiceItemObject
                    {
                        FeeId = l.FeeId,
                        AmountDue = l.Amount
                    });
                });
                
                var invStatus = new InvoiceServices().AddInvoice(invoice);
                if (invStatus.InvoiceId < 1 || string.IsNullOrEmpty(invStatus.RefCode))
                {
                    gVal.Code = -5;
                    gVal.ErrorCode = -1;
                    gVal.Error = "Process failed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var url = "";
                if (importApplication.PaymentTypeId == (int)PaymentType.Bank)
                {
                    url = baseUrl + "/Transaction/Invoice";
                }
                else
                {
                    url = baseUrl + "/Transaction/WebPayment";
                }
                
                var remitaRef = "";
                var rrr = new SplitPayments().PostSplitPaymentForApplicationFee(importApplication.DerivedValue, invStatus.RefCode, importerInfo.Name, importerInfo.UserProfileObject.Email, importerInfo.UserProfileObject.PhoneNumber, url, importApplication.FeeObjects);
                if (rrr.Code < 1 || rrr.Error.Contains("<"))
                {
                    gVal.Code = -11;
                    gVal.ErrorCode = -9;
                    gVal.Error = "A payment Reference Number could not be provided for your Application because The payment gateway could not be reached. Your Application has been saved so that you will be able to make payment later. We are sorry for this unforseen issue and will resolve it ASAP.";
                    remitaRef = "Not Available";
                }
                else
                {
                    remitaRef = rrr.Error;
                    gVal.ErrorCode = 5;
                    gVal.Code = 5;
                    var tt = new InvoiceServices().UpdateInvoiceRrr(invStatus.RefCode, remitaRef);
                    if (tt < 1)
                    {
                        gVal.Code = -7;
                    }
                }

                gVal.RemitRes = remitaRef;
                importApplication.InvoiceId = invStatus.InvoiceId;
                importApplication.ReferenceCode = invStatus.RefCode;
                var appId = new ApplicationServices().AddApplication(importApplication);
                if (appId < 1)
                {
                    new InvoiceServices().DeleteInvoice(invStatus.InvoiceId);
                    gVal.Error = appId == -2 ?  "Application could not be made. Please try again." : "A similar application by you is still under processing.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.AppId = appId;
                importApplication.Id = appId;

                var appHistory = new ImportApplicationHistoryObject
                {
                    ApplicationId = appId,
                    Event = "New",
                    Date = DateTime.Now
                };

                new ApplicationHistoryServices().AddApplicationHistory(appHistory);
                var hash = "";
                if ((gVal.ErrorCode < 1 && gVal.ErrorCode == -9) || rrr.Error.Contains("<"))
                {
                    hash = "";
                }
                else
                {
                    hash = Hasher.GenerateHash(remitaRef);
                }

                gVal.Hash = hash;
                gVal.Rrr = remitaRef;
                gVal.RedirectUri = url;
                gVal.RefCode = invStatus.RefCode;
                
                Session["_appObj"] = importApplication;

                var mailObj = new AppMailObject
                {
                    UserId = importerInfo.UserProfileObject.Id,
                    ImporterName = importerInfo.Name,
                    AmountDue = invoice.TotalAmountDue.ToString("n"),
                    ReferenceNumber = invStatus.RefCode,
                    PaymentReference = remitaRef,
                    ApplicationType = Enum.GetName(typeof(AppTypeEnum), importApplication.ApplicationTypeId).Replace("_", " "),
                    ApplicationCategory = Enum.GetName(typeof(NotificationClassEnum), importApplication.ClassificationId).Replace("_", " "),
                    //PaymentStatus = "Pending",
                    PaymentStatus = "Paid",
                    DateApplied = DateTime.Now.ToString("dd/MM/yyyy"),
                    TypeId = 1,
                    Email =  importerInfo.UserProfileObject.Email
                };

                await SendMail(mailObj);
                var factor = statutoryFee.Amount + processingFee.Amount;
                var multiplier = importApplication.DerivedValue / factor;
                gVal.Multiplier = multiplier;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.InnerException.StackTrace, e.InnerException.Source, e.InnerException.Message);
                if (gVal.Code > 0)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Error = "Application process failed. Please try again later";
                gVal.Code = -1;
                gVal.ErrorCode = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditApplication(ApplicationObject importApplication)
        {
            var gVal = new GenericValidator();

            try
            {
                if (!importApplication.ApplicationItemObjects.Any())
                {
                    gVal.Code = -1;
                    gVal.Error = "Plese provide all required fields and try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out. Please refresh the page.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var validationResult = ValidateApplicationItems(importApplication.ApplicationItemObjects.ToList());

                if (validationResult.Any())
                {
                    gVal.Code = -1;
                    gVal.Error = "Plese Provide all required fields and try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                foreach (var item in importApplication.ApplicationItemObjects)
                {
                    if (item.StorageProviderTypeId == (int)StorageProviderTypeEnum.Own_Depot)
                    {
                        if (!string.IsNullOrEmpty(item.DepotLicense))
                        {
                            var verifier = new LicenseRefObject
                            {
                                LicenseType = (int)StorageProviderTypeEnum.Own_Depot,
                                RefCode = item.DepotLicense,
                                ImporterId = importerInfo.Id
                            };
                            var vv = new ProductServices().VerifyCode(verifier);
                            if (vv < 1)
                            {
                                gVal.Code = -1;
                                gVal.Error = "The provided Depot License is invalid.";
                                return Json(gVal, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            gVal.Code = -1;
                            gVal.Error = "Please provide your Depot License";
                            return Json(gVal, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                List<DocumentTypeObject> docList;

                var res = new ApplicationServices().UpdateApplicationItems(importApplication.ApplicationItemObjects.ToList(), importApplication.Id, out docList);
                if (res < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Application information could not be Modified successfully. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (docList.Any())
                {
                    docList.ForEach(d =>
                    {
                        MoveToArchive(importApplication.ImporterId, d.DocumentPath, d.Name);
                    });
                }

                gVal.Code = 5;
                gVal.Error = "Application information was successfully modified.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.InnerException.StackTrace, e.InnerException.Source, e.InnerException.Message);
                if (gVal.Code > 0)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = -1;
                gVal.Error = "Application Modification process failed. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        private async Task<bool> SendMail(AppMailObject model)
        {
            try
            {
                if (model == null || model.UserId < 1)
                {
                    return false;
                }

                var type = 0;
                if (model.TypeId == 1)
                {
                    type = (int)MessageEventEnum.New_Application;
                }

                else
                {
                    type = (int)MessageEventEnum.Application_Edit;
                }

                var msgBody = "";
                var msg = new MessageTemplateServices().GetMessageTemp(type);
                if (msg.Id < 1)
                {
                    return false;
                }
                
                var emMs = new MessageObject
                {
                    UserId = model.UserId,
                    MessageTemplateId = msg.Id,
                    Status = (int)MessageStatus.Pending,
                    DateSent = DateTime.Now,
                    MessageBody = msg.MessageContent
                };

                var sta = new MessageServices().AddMessage(emMs);
                if (sta < 1)
                {
                    return false;
                }

                if (Request.Url != null)
                {
                    msgBody = "DPR/DS/IEP/" + DateTime.Today.ToString("dd/MM/yyyy") + "/" + sta + "<br/><br/>";
                    msg.Subject = msg.Subject.Replace("\n", "<br/>").Replace("{payment reference}", model.PaymentReference).Replace("{type}", model.ApplicationType);
                    msg.MessageContent = msg.MessageContent.Replace("{company}", model.ImporterName).Replace("\n", "<br/>");
                 
                    var sr = "<br/><b>Reference Number</b>:" + model.ReferenceNumber +
                               "<br/><b>Payment Reference (RRR)</b>: " +  model.PaymentReference +
                               "<br/><b>Application Type</b>: New" +
                               "<br/><b>Application Category</b>: " +  model.ApplicationCategory +
                               "<br/><b>Amount Due</b>: " +  model.AmountDue +
                               "<br/><b>Payment Status</b>:" +  model.PaymentStatus +
                               "<br/><b>Date Applied</b>:" + model.DateApplied;


                    msgBody += msg.MessageContent.Replace("{applicationDetails}", sr);

                    msgBody += "<br/><br/>" + msg.Footer.Replace("\n", "<br/>");
                }

                if (Request.Url != null)
                {
                    var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                    var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                    var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                    var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                    if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                    {
                        return false;
                    }

                    #region Using Mandrill
                    var api = new MandrillApi(apiKey);
                    var receipint = new List<MandrillMailAddress> { new MandrillMailAddress(model.Email) };
                    var message = new MandrillMessage()
                    {
                        AutoHtml = true,
                        To = receipint,
                        FromEmail = settings.Smtp.From,
                        FromName = appName,
                        Subject = msg.Subject,
                        Html = msgBody
                    };

                    var result = await api.Messages.SendAsync(message);

                    if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                    {
                        emMs.Status = (int)MessageStatus.Failed;
                    }
                    else
                    {
                        emMs.Status = (int)MessageStatus.Sent;
                    }
                    #endregion
                    
                    emMs.Id = sta;
                    emMs.MessageBody = msgBody;
                    var tts = new MessageServices().UpdateMessage(emMs);
                    if (tts < 1)
                    {
                        return false;
                    }

                    return true;

                }

                return false;
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                return false;
            }
        }

        public ActionResult GetList()
        {
            try
            {
                var list = new ApplicationGenericList
                {
                    Products = new ProductServices().GetProducts(),
                    StorageProviderTypes = GetStorageProviderTypes(),
                    Banks = GetBankObjects(),
                    Classes = GetImportClass(),
                    ApplicationTypes = GetApplicationTypes()
                };
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<ProductObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetRunList()
        {
            try
            {
                var list = new ApplicationGenericList
                {
                    StorageProviderTypes = GetStorageProviderTypes(),
                    Banks = GetBankObjects()
                };
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<ProductObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetBankers()
        {
            try
            {
                return Json(GetBankObjects(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<ProductObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult GetPortsAndCountries()
        {
            try
            {
                var list = new PortAndCountry
                {
                    DepotList = new DepotServices().GetDepots(),
                    Countries = GetCountries()
                };

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<ProductObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetProductList()
        {
            try
            {
                return Json(new ProductServices().GetProducts(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<ProductObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> GetApplicationDetails(long id)
        {
            try
            {
                var baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority;

                if (id < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }

                var app = new ApplicationServices().GetApplicationDetails(id);
                if (app == null || app.Id < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }
                
                if (app.ApplicationStatusCode < 2)
                {
                    var url = "";
                    if (app.PaymentTypeId == (int)PaymentType.Bank)
                    {
                        url = baseUrl + "/Transaction/Invoice";
                    }
                    else
                    {
                        url = baseUrl + "/Transaction/WebPayment";
                    }

                    if (string.IsNullOrEmpty(app.Rrr))
                    {
                        var rrr = new SplitPayments().PostSplitPaymentForApplicationFee(app.DerivedValue, app.ReferenceCode, importerInfo.Name, importerInfo.UserProfileObject.Email, importerInfo.UserProfileObject.PhoneNumber, url, app.FeeObjects);
                        if (rrr.Code < 1 || rrr.Error.Contains("<"))
                        {
                            return Json(app, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            app.Rrr = "Not Available.";
                        }
                    
                        new InvoiceServices().UpdateInvoiceRrr( app.ReferenceCode, rrr.Error);
                        app.Rrr = rrr.Error;
                    }

                    var hash = Hasher.GenerateHash(app.Rrr);
                    app.Hash = hash;
                    app.RedirectUri = url;
                }
                Session["_app"] = app;
                return Json(app, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateAppPaymentOption(int paymentTypeId, long applicationId)
        {
            var gVal = new GenericValidator();

            try
            {
                if (paymentTypeId < 1 || applicationId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Invald selection!";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
               
                var retVal = new ApplicationServices().UpdateAppPaymentOption(paymentTypeId, applicationId);
                if (retVal < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Payment option could not be updated. But you can proceed to payment with the previous option.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = retVal;
                gVal.Error = "Payment option was successfully updated.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetApplication(long id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetApplicationInfo(id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ViewApplication(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CheckToAppSubmit(long id)
        {
            var gVal = new GenericValidator();
            try
            {
                if (id < 1)
                {
                    gVal.IsRequirementsMet = false;
                    gVal.Code = 0;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var status = CheckAppSubmit(id);
                if (!status)
                {
                    gVal.Code = 0;
                    gVal.IsRequirementsMet = false;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = 5;
                gVal.IsRequirementsMet = true;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.IsRequirementsMet = false;
                gVal.Code = 0;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AppSubmit(long id)
        {
            var gVal = new GenericValidator();
            try
            {
                if (id < 1)
                {
                    gVal.Code = 0;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var application = SubmitApp(id);
                if (application.UserId < 1)
                {
                    gVal.Code = 0;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                //Check if the Application was successfully Assigned to an 
                // Employee through an interval bound service
               var smSt = new WorkFlowServices().AssignApplicationToEmployee(id);
                if (!smSt)
                {
                    UnSubmitApp(id);
                    gVal.Code = 0;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var msg = "<b>Date Submitted</b>: " + DateTime.Now.ToString("dd/MM/yyyy") +
                          "<b>Reference Code</b>: " + application.ReferenceCode +
                          "<b>Payment Reference Code (RRR)</b>: " + application.Rrr +
                          "<b>Type</b>: " + application.ApplicationTypeName +
                          "<b>Amount Paid</b>: " + application.AmountStr +
                          "<b>Application Period</b>: " + application.DateAppliedStr;
                 msg += "<table class=\"table\" style=\"width: 100%;\"><tr><th style=\"width: 12%\">Product Code</th><th style=\"width: 12%\">" +
                              "Est. Volume(MT)</th><th style=\"width: 12%\">Est. Value($)</th><th style=\"width: 18%\"></tr>";

                application.ApplicationItemObjects.ToList().ForEach(prd =>
                {
                    msg += "<tr> "
                           + "<td style=\"width: 12%\">" + prd.Code + "</td>"
                           + " <td style=\"width: 12%\">" + prd.EstimatedQuantityStr + "</td>"
                           + " <td style=\"width: 12%\">" + prd.EstimatedValueStr + "</td>"
                           + " </tr>";
                });

                msg += "</table>";

                var mailObj = new AppMailObject
                {
                    UserId = application.UserId,
                    ReferenceNumber = application.ReferenceCode,
                    PaymentReference = application.Rrr,
                    ImporterName = application.CompanyName,
                    Email = application.Email,
                    MssageContent = msg
                };

                SendAppSubmitMail(mailObj);

                gVal.Code = (int)AppStatus.Submitted;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.IsRequirementsMet = false;
                gVal.Code = 0;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private bool SendAppSubmitMail(AppMailObject model)
        {
            try
            {
                if (model == null || model.UserId < 1)
                {
                    return false;
                }

                const int type = (int)MessageEventEnum.Application_Submission;

                var msgBody = "";
                var msg = new MessageTemplateServices().GetMessageTemp(type);
                if (msg.Id < 1)
                {
                    return false;
                }

                var emMs = new MessageObject
                {
                    UserId = model.UserId,
                    MessageTemplateId = msg.Id,
                    Status = (int)MessageStatus.Pending,
                    DateSent = DateTime.Now,
                    MessageBody = msg.MessageContent
                };

                var sta = new MessageServices().AddMessage(emMs);
                if (sta < 1)
                {
                    return false;
                }

                if (Request.Url != null)
                {
                    msgBody = "DPR/DS/IEP/" + DateTime.Today.ToString("dd/MM/yyyy") + "/" + sta + "<br/><br/>";

                    msg.MessageContent = msg.MessageContent.Replace("{reference number}", model.PaymentReference);

                    msgBody += msg.MessageContent.Replace("{applicableProducts}", model.MssageContent);

                    msgBody += "<br/><br/>" + msg.Footer;
                }

                if (Request.Url != null)
                {

                    #region Using SendGrid

                    var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                    var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                    if (settings == null)
                    {
                        return false;
                    }

                    var mail = new MailMessage(new MailAddress(settings.Smtp.From), new MailAddress(model.Email))
                    {
                        Subject = "DPR-PPIPS",
                        Body = msgBody,
                        IsBodyHtml = true
                    };

                    var smtp = new SmtpClient(settings.Smtp.Network.Host)
                    {
                        Credentials = new NetworkCredential(settings.Smtp.Network.UserName, settings.Smtp.Network.Password),
                        EnableSsl = true,
                        Port = settings.Smtp.Network.Port
                    };

                    smtp.Send(mail);

                    emMs.Id = sta;
                    emMs.MessageBody = msgBody;
                    emMs.Status = (int)MessageStatus.Sent;
                    var tts = new MessageServices().UpdateMessage(emMs);
                    if (tts < 1)
                    {
                        return false;
                    }

                    return true;
                    #endregion

                }

                return false;
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                return false;
            }
        }

        public ActionResult GetApplicationByRef(string referenceCode)
        {
            try
            {
                if (string.IsNullOrEmpty(referenceCode))
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }
               
                return Json(GetApplicationByReference(referenceCode), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ViewApplication(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAppForRenewal(string referenceCode)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(referenceCode))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide your permit number.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has expired.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var app = GetApplicationForRenewal(referenceCode, importerInfo.Id);
                if (app < 1)
                {
                    var stopDate = DateTime.Parse("2015-07-31");
                    if (DateTime.Today <= stopDate)
                    {
                        var licRes = AddUserPermit(importerInfo.Name, importerInfo.Id, referenceCode);
                        return Json(licRes, JsonRequestBehavior.AllowGet);
                    }
                    gVal.Code = -1;
                    gVal.Error = "Permit could not be verified.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = app;
                gVal.Error = "Permit was successfully verified.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Permit could not be verified.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator AddUserPermit(string importerName, long importerId, string permitValue)
        {

            var gVal = new GenericValidator();
            try
            {
                var filePath = HostingEnvironment.MapPath(Path.Combine("~/BulkUploads", "permits.json"));
                if (string.IsNullOrEmpty(filePath))
                {
                    gVal.Code = -1;
                    gVal.Error = "Internal server error. An unknown error was encountered. Please try again.";
                    return gVal;
                }

                List<PermitObject> licenses;
                var serializer = new JsonSerializer();
                using (var re = System.IO.File.OpenText(filePath))
                using (var reader = new JsonTextReader(re))
                {
                    licenses = serializer.Deserialize<List<PermitObject>>(reader);
                }

                var impName = importerName.ToLower().Trim();

                if (impName.Contains("ltd"))
                {
                    impName = impName.Replace("ltd", "limited");
                }

                var license = new PermitObject();

                licenses.ForEach(x =>
                {
                    var nm = x.CompanyName.ToLower().Trim();

                    if (nm.Contains("ltd"))
                    {
                        nm = nm.Replace("ltd", "limited");
                    }

                    if (nm.Replace(" ", string.Empty) == impName.Replace(" ", string.Empty))
                    {
                        license = x;
                    }
                });

                if (string.IsNullOrEmpty(license.PermitValue)) 
                {
                    gVal.Code = -1;
                    gVal.Error = "Permit could not be found.";
                    return gVal;
                }

                if (license.PermitValue.Trim() != permitValue) 
                {
                    gVal.Code = -1;
                    gVal.Error = "Permit could not be found.";
                    return gVal; 
                }

                license.ImporterId = importerId;
                var status = new PermitServices().AddPermit(license);
                if (status < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Permit could not be verified. Please try again later.";
                    return gVal;
                }
                
                gVal.Code = status;
                gVal.Error = "Permit was successfully verified.";
                return gVal;
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                gVal.Code = -1;
                gVal.Error = "Permit could not be verified";
                return gVal;
            }
        }

        private GenericValidator UpdateUserDepot(string importerName, long importerId, string depotLicense)
        {

            var gVal = new GenericValidator();
            try
            {
                var filePath = HostingEnvironment.MapPath(Path.Combine("~/BulkUploads", "permits.json"));
                if (string.IsNullOrEmpty(filePath))
                {
                    gVal.Code = -1;
                    gVal.Error = "Internal server error. An unknown error was encountered. Please try again.";
                    return gVal;
                }

                List<PermitObject> licenses;
                var serializer = new JsonSerializer();
                using (var re = System.IO.File.OpenText(filePath))
                using (var reader = new JsonTextReader(re))
                {
                    licenses = serializer.Deserialize<List<PermitObject>>(reader);
                }

                var impName = importerName.ToLower().Trim();

                if (impName.Contains("ltd"))
                {
                    impName = impName.Replace("ltd", "limited");
                }

                var license = new PermitObject();

                licenses.ForEach(x =>
                {
                    var nm = x.CompanyName.ToLower().Trim();

                    if (nm.Contains("ltd"))
                    {
                        nm = nm.Replace("ltd", "limited");
                    }

                    if (nm.Replace(" ", string.Empty) == impName.Replace(" ", string.Empty))
                    {
                        license = x;
                    }
                });

                if (string.IsNullOrEmpty(license.PermitValue))
                {
                    gVal.Code = -1;
                    gVal.Error = "Permit could not be found.";
                    return gVal;
                }

                if (license.PermitValue.Trim() != depotLicense)
                {
                    gVal.Code = -1;
                    gVal.Error = "Permit could not be found.";
                    return gVal;
                }

                license.ImporterId = importerId;
                var status = new PermitServices().AddPermit(license);
                if (status < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Permit could not be verified. Please try again later.";
                    return gVal;
                }

                gVal.Code = status;
                gVal.Error = "Permit was successfully verified.";
                return gVal;
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                gVal.Code = -1;
                gVal.Error = "Permit could not be verified";
                return gVal;
            }
        }

        public ActionResult GetAppForInclusion(string referenceCode)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(referenceCode))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide your permit number.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var app = GetApplicationForInclusion(referenceCode, importerInfo.Id);
                if (app < 1)
                {
                    var stopDate = DateTime.Parse("2015-07-31");
                    if (DateTime.Today <= stopDate)
                    {
                        var licRes = AddUserPermit(importerInfo.Name, importerInfo.Id, referenceCode);
                        return Json(licRes, JsonRequestBehavior.AllowGet);
                    }
                    gVal.Code = -1;
                    gVal.Error = "Permit could not be verified.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = app;
                gVal.Error = "Permit was successfully verified.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Permit could not be verified.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ApplicationObject> GetBankUserJobHistory(int? itemsPerPage, int? pageNumber, out int countG, long userId)
        {
            return new ApplicationServices().GetBankUserJobHistory(itemsPerPage, pageNumber, out countG, userId) ?? new List<ApplicationObject>();
        }

        private List<ApplicationObject> GetBankJobHistory(int? itemsPerPage, int? pageNumber, out int countG, long id)
        {
            return new ApplicationServices().GetBankJobHistory(itemsPerPage, pageNumber, out countG, id) ?? new List<ApplicationObject>();
        }
        
        private List<ApplicationObject> GetApplications(int? itemsPerPage, int? pageNumber, out int countG, long id)
        {
            return new ApplicationServices().GetApplications(itemsPerPage, pageNumber, out countG, id) ?? new List<ApplicationObject>();
        }

        private List<ApplicationObject> GetAssignedApplications(int? itemsPerPage, int? pageNumber, out int countG, long id)
        {
            return new ApplicationServices().GetBankAssignedApplications(itemsPerPage, pageNumber, out countG, id) ?? new List<ApplicationObject>();
        }

        private List<ApplicationObject> GetApplications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            return new ApplicationServices().GetAdminApplications(itemsPerPage, pageNumber, out countG) ?? new List<ApplicationObject>();
        }

        private List<DocumentTypeObject> GetApplicationRequirements(RequirementProp requirementProp)
        {
            return new DocumentTypeServices().GetApplicationStageDocumentTypes(requirementProp) ?? new List<DocumentTypeObject>();
        }

        public ActionResult ComputeDerivedValue(ApplicationObject application)
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
                CalculationFactor calculationFactor;
                var appFees = new FeeServices().GetAppliationStageFees(out calculationFactor);
                if (!appFees.Any() || calculationFactor == null || calculationFactor.PriceVolumeThreshold < 1)
                {
                    gVal.DerivedValue = 0;
                    gVal.Error = "The Application process failed. Please try again later.";
                    gVal.FeeObjects = new List<FeeObject>();
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.FeeObjects = appFees;
                var quantityMod = application.DerivedValue/calculationFactor.PriceVolumeThreshold;
                double derivedValue = 0;
                if (quantityMod >= 1)
                {
                    var val = quantityMod.ToString(CultureInfo.InvariantCulture);
                    if (val.Contains('.'))
                    {
                        var parts = val.Split('.');
                        var wholePart = long.Parse(parts[0]);
                        derivedValue = wholePart * calculationFactor.Fees;
                        var fracPart = parts[1];
                        if (!string.IsNullOrEmpty(fracPart))
                        {
                            derivedValue += calculationFactor.Fees;
                        }
                    }
                    else
                    {
                        derivedValue += quantityMod * calculationFactor.Fees;
                    }

                }
                else
                {
                    derivedValue = calculationFactor.Fees;
                }

                if (derivedValue < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Internal server error: the request could not be processed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                const int statutoryFeeId = (int)FeeTypeEnum.Statutory_Fee;
                const int processingFeeId = (int)FeeTypeEnum.Processing_Fee;
                var statutoryFee = appFees.Find(m => m.FeeTypeId == statutoryFeeId);
                var processingFee = appFees.Find(m => m.FeeTypeId == processingFeeId);
                if (statutoryFee == null || statutoryFee.FeeId < 1)
                {
                    gVal.DerivedValue = 0;
                    gVal.Error = "The Application process failed. Please try again later.";
                    gVal.FeeObjects = new List<FeeObject>();
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                var processingFeeAmount = processingFee.Amount;
                if (processingFeeAmount < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Internal server error: the request could not be processed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                statutoryFee.FeeTypeName = statutoryFee.FeeTypeName + " (" + WebUtility.HtmlDecode("&#8358;") + statutoryFee.Amount.ToString("n1") + "/" + "30,000 MT)";
                statutoryFee.Amount = derivedValue;
                
                derivedValue += processingFeeAmount;

                var productIds = new List<long>();
                var strTyps = new List<long>(); 
                application.ApplicationItemObjects.ToList().ForEach(item =>
                {
                    productIds.Add(item.ProductObject.ProductId);
                    strTyps.Add(item.StorageProviderTypeId);
                
                });

                if (!productIds.Any() || !strTyps.Any())
                {
                    gVal.DerivedValue = 0;
                    gVal.Error = "The process failed. Please try again later.";
                    gVal.FeeObjects = new List<FeeObject>();
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                var reqprops = new RequirementProp
                {
                    ImportClassId = application.ClassificationId,
                    ProductIds = productIds,
                    StorageProviderTypeIds = strTyps
                };
                
                var docs = GetDocRequirements(reqprops);
                if (!docs.Any())
                {
                    gVal.DerivedValue = 0;
                    gVal.Error = "The process failed. Please try again later.";
                    gVal.FeeObjects = new List<FeeObject>();
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.DocumentTypeObjects = docs;
                gVal.CompanyName = importerInfo.Name;
                gVal.DerivedValue = derivedValue;
                gVal.FeeObjects = appFees;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                gVal.DerivedValue = 0;
                gVal.FeeObjects = new List<FeeObject>();
                return Json(gVal, JsonRequestBehavior.AllowGet);
            } 
        }
        
        private List<DocumentTypeObject> GetDocRequirements(RequirementProp requirementProp)
        {
            try
            {
                if (requirementProp == null || !requirementProp.ProductIds.Any() || !requirementProp.StorageProviderTypeIds.Any())
                {
                    return new List<DocumentTypeObject>();
                }

                var requiredDocs = GetApplicationRequirements(requirementProp);
                if (!requiredDocs.Any())
                {
                    return new List<DocumentTypeObject>();
                }

                return requiredDocs;
            }
            catch (Exception)
            {
                return new List<DocumentTypeObject>();
            }
        }

        private ApplicationObject GetAppDocs(long appId)
        {
            return new ApplicationServices().GetAppDocuments(appId) ?? new ApplicationObject();
        }
      
        private ApplicationObject GetBankerApp(string code, long bankerImpoterId)
        {
            return new ApplicationServices().GetBankerAppByReference(code, bankerImpoterId) ?? new ApplicationObject();
        }

        private ApplicationObject GetAppDocumentsX(long id)
        {
            return new ApplicationServices().GetAppDocumentsX(id) ?? new ApplicationObject();
        }

        private bool CheckAppSubmit(long id)
        {
            return new ApplicationServices().CheckAppSubmit(id);
        }

       private ApplicationObject SubmitApp(long id)
       {
        return new ApplicationServices().SubmitApp(id);
       }

       private bool UnSubmitApp(long id)
       {
           return new ApplicationServices().UnSubmitApp(id);
       }
        
        private ApplicationObject GetBankAssignedAppDocs(long appId, long companyId)
        {
            return new ApplicationServices().GetBankAssignedAppDocs(appId, companyId) ?? new ApplicationObject();
        }

        private ApplicationObject GetApplicationInfo(long appId)
        {
            return new ApplicationServices().GetApplication(appId);
        }

        private ApplicationObject GetApplicationAdminObj(long appId)
        {
            return new ApplicationServices().GetApplication(appId);
        }

        private ApplicationObject GetPaidApplicationObj(long invoiceId)
        {
            return new ApplicationServices().GetPaidApplication(invoiceId);
        }

        private ApplicationObject GetApplicationByReference(string code)
        {
            return new ApplicationServices().GetApplicationByRef(code);
        }

        private long GetApplicationForRenewal(string code, long id)
        {
            return new ApplicationServices().GetApplicationForRenewal(code, id);
        }

        private long GetApplicationForInclusion(string code, long id)
        {
            return new ApplicationServices().GetApplicationForInclusion(code, id);
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


         private long GetImporterId()
        {
            try
            {
                if (Session["_importerInfo"] == null)
                {
                    return 0;
                }

                var importerId = (long)Session["_importerId"];
                if (importerId < 1)
                {
                    return 0;
                }

                return importerId;
                
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private List<StorageProviderTypeObject> GetStorageProviderTypes() 
        {
            return new StorageProviderTypeServices().GetStorageProviderTypes() ?? new List<StorageProviderTypeObject>();
        }
        private List<BankObject> GetBankObjects()
        {
            return new BankServices().GetBanks() ?? new List<BankObject>();
        }

        private List<ImportClassObject> GetImportClass()
        {
           return new ImportClassServices().GetImportClasses();
        }

        private List<PortObject> GetPorts()
        {
            return new PortServices().GetPorts();
        }

        private List<CountryObject> GetCountries()
        {
            return new CountryServices().GetCountries();
        }

        private List<GenericObject> GetApplicationTypes()
        {
            return EnumToObjList.ConvertEnumToList(typeof(AppTypeEnum)).OrderBy(m => m.Name).ToList();
        }

        private List<GenericValidator> ValidateApplication(List<ApplicationItemObject> importItems)
        {
            var errorList = new List<GenericValidator>();
            try
            {
                foreach (var app in importItems)
                {
                    if (app.ProductId < 1)
                    {
                        errorList.Add(new GenericValidator
                        {
                            Code = -1,
                            Error = "Please select a Product."
                        });
                    }

                    if (app.EstimatedQuantity < 1)
                    {
                        errorList.Add(new GenericValidator
                        {
                            Code = -1,
                            Error = "Please provide Product' Estimated Quantity."
                        });
                    }
                    if (app.StorageProviderTypeId < 1)
                    {
                        errorList.Add(new GenericValidator
                        {
                            Code = -1,
                            Error = "Please select Storage Type."
                        });
                    }
                    if (app.EstimatedValue < 1)
                    {
                        errorList.Add(new GenericValidator
                        {
                            Code = -1,
                            Error = "Please provide Product' Estimated Monetary Value."
                        });
                    }
                    
                    if (!app.ThroughPutObjects.Any()) 
                    {
                        errorList.Add(new GenericValidator
                        {
                            Code = -1,
                            Error = "Please please provide at least one Discharge Depot."
                        });
                    }

                    if (!app.ProductBankerObjects.Any())
                    {
                        errorList.Add(new GenericValidator
                        {
                            Code = -1,
                            Error = "Please please provide at least one Sponsoring Bank."
                        });
                    }

                    if (!app.ApplicationCountryObjects.Any())
                    {
                        errorList.Add(new GenericValidator
                        {
                            Code = -1,
                            Error = "Please provide at least one Country of Origin."
                        });
                    }

                }
                if (!errorList.Any())
                {
                    return new List<GenericValidator>();
                }
                else
                {
                    return errorList;
                }
            }
            catch (Exception)
            {
                return new List<GenericValidator>
                {
                    new GenericValidator
                    {
                        Code = -1,
                        Error = "Application Validation failed. Please provide all required fields and try again."
                    }
                };
            }
        }

        private List<GenericValidator> ValidateApplicationItems(List<ApplicationItemObject> importItems)
        {
            var errorList = new List<GenericValidator>();
            try
            {
                foreach (var app in importItems)
                {
                    if (app.StorageProviderTypeId < 1)
                    {
                        errorList.Add(new GenericValidator
                        {
                            Code = -1,
                            Error = "Please select Storage Type."
                        });
                    }
                    
                    if (!app.ThroughPutObjects.Any())
                    {
                        errorList.Add(new GenericValidator
                        {
                            Code = -1,
                            Error = "Please please provide at least one Discharge Depot."
                        });
                    }

                    if (!app.ProductBankerObjects.Any())
                    {
                        errorList.Add(new GenericValidator
                        {
                            Code = -1,
                            Error = "Please please provide at least one Sponsoring Bank."
                        });
                    }

                }
                if (!errorList.Any())
                {
                    return new List<GenericValidator>();
                }
                else
                {
                    return errorList;
                }
            }
            catch (Exception)
            {
                return new List<GenericValidator>
                {
                    new GenericValidator
                    {
                        Code = -1,
                        Error = "Application Validation failed. Please provide all required fields and try again."
                    }
                };
            }
        }

        private GenericValidator ValidateNotificationBanker(NotificationBankerObject applicationBanker)
        {
            var gVal = new GenericValidator();
            try
            {
                if (applicationBanker.ProductId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a Product.";
                    return gVal;
                }

                if (applicationBanker.NotificationId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Error: The Application information is empty. Please try again.";
                    return gVal;
                }
                
                if (applicationBanker.FinancedQuantity < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Financed Quantity.";
                    return gVal;
                }

                if (applicationBanker.TransactionAmount < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Transaction Amount.";
                    return gVal;
                }
               
                if (applicationBanker.ActualQuantity < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Product' Port of Origin.";
                    return gVal;
                }
                gVal.Code = 5;
                return gVal;
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Application Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private string MoveFile(long importerId, string tempPath)
        {
            try
            {
                var path = HostingEnvironment.MapPath("~/ImportDocuments/" + importerId.ToString(CultureInfo.InvariantCulture));
                if (string.IsNullOrEmpty(path))
                {
                    return "";
                }
                var tmpPath = "~/tempFiles/" + importerId.ToString(CultureInfo.InvariantCulture);
                var mappedTmpPath = HostingEnvironment.MapPath(tmpPath);
                if (string.IsNullOrEmpty(mappedTmpPath))
                {
                    return "";
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    var dInfo = new DirectoryInfo(path);
                    var dSecurity = dInfo.GetAccessControl();
                    dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                    dInfo.SetAccessControl(dSecurity);
                }
                var fpth = HostingEnvironment.MapPath(tempPath);
                var fileName = Path.GetFileName(fpth);
                if (string.IsNullOrEmpty(fileName))
                {
                    return "";
                }

                var newPathv = Path.Combine(path, fileName);
                System.IO.File.Copy(fpth, newPathv);
                var dir = new DirectoryInfo(mappedTmpPath);
                var files = dir.GetFiles();
                if (files.Any())
                {
                    files.ForEach(j =>
                    {
                        System.IO.File.Delete(j.FullName);
                    });
                }

                return PhysicalToVirtualPathMapper.MapPath(newPathv).Replace("~", "");
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return string.Empty;
            }
        }

        private string MoveToArchive(long importerId, string filePath, string fileName)
        {
            try
            {
                var path = HostingEnvironment.MapPath("~/Archives/" + importerId.ToString(CultureInfo.InvariantCulture));
                if (string.IsNullOrEmpty(path))
                {
                    return "";
                }

                var mappedTmpPath = HostingEnvironment.MapPath(filePath);
                if (string.IsNullOrEmpty(mappedTmpPath))
                {
                    return "";
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    var dInfo = new DirectoryInfo(path);
                    var dSecurity = dInfo.GetAccessControl();
                    dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                    dInfo.SetAccessControl(dSecurity);
                }
                
                var newPathv = Path.Combine(path, fileName);
                System.IO.File.Copy(mappedTmpPath, newPathv);
                System.IO.File.Delete(filePath);

                return PhysicalToVirtualPathMapper.MapPath(newPathv).Replace("~", "");
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return string.Empty;
            }
        }

        private bool DeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return false;
                }

                if (!filePath.StartsWith("~"))
                {
                    filePath = "~" + filePath;
                }

                System.IO.File.Delete(Server.MapPath(filePath));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private GenericValidator SaveDocument(DocumentObject document)
        {
            var gVal = new GenericValidator();

            try
            {
                var docStatus = new DocumentServices().AddDocument(document);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document upload failed. Please try again.";
                    return gVal;
                }

                gVal.Code = docStatus;
                gVal.Error = "";
                return gVal;

            }
            catch (Exception)
            {
                gVal.Error = "Document processing failed. Please try again later";
                gVal.Code = -1;
                return gVal;
            }
        } 

    }
}
