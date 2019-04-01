using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;
using Mandrill;
using Mandrill.Model;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

 
namespace ImportPermitPortal.Controllers
{
    [Authorize]
    public class NotificationController : Controller  
    {


        [HttpGet]
        [Authorize(Roles = "Applicant")]
        public ActionResult GetNotificationObjects(JQueryDataTableParamModel param)
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }
                IEnumerable<NotificationObject> filteredParentMenuObjects;
                int countG;

                var pagedParentMenuObjects = GetCompanyNotifications(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationServices().SearchCompanyNotifications(param.sSearch, importerInfo.Id);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.PermitValue :
                    sortColumnIndex == 3 ? c.ProductName : sortColumnIndex == 4 ? c.QuantityToDischargeStr : sortColumnIndex == 5 ? c.ArrivalDateStr :c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;
                
                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.PermitValue, c.ProductName, c.QuantityToDischargeStr, c.AmountDueStr,
                                 c.ArrivalDateStr, c.StatusStr
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
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetNotifications(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<NotificationObject> filteredParentMenuObjects;
                int countG;

                var pagedParentMenuObjects = GetNotifications(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationServices().Search(param.sSearch); countG = filteredParentMenuObjects.Count(); 
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ImporterName : sortColumnIndex == 2 ? c.ReferenceCode : sortColumnIndex == 3 ? c.PermitValue :
                    sortColumnIndex == 4 ? c.ProductName : sortColumnIndex == 5 ? c.QuantityToDischargeStr : sortColumnIndex == 6 ? c.ArrivalDateStr : sortColumnIndex == 7 ? c.DischargeDateStr : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),  c.ReferenceCode,c.ImporterName,
                                 c.PermitValue, c.ProductName, c.QuantityToDischargeStr, c.AmountDueStr,
                                 c.ArrivalDateStr, c.StatusStr
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
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetPaidNotifications(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<NotificationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new NotificationServices().GetPaidNotifications(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationServices().SearchPaidNotifications(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ImporterName : sortColumnIndex == 2 ? c.ReferenceCode : sortColumnIndex == 3 ? c.PermitValue :
                    sortColumnIndex == 4 ? c.ProductName : sortColumnIndex == 5 ? c.QuantityToDischargeStr : sortColumnIndex == 6 ? c.ArrivalDateStr : sortColumnIndex == 7 ? c.DischargeDateStr : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),c.ReferenceCode, c.ImporterName,
                                  c.PermitValue, c.ProductName, c.QuantityToDischargeStr, c.AmountDueStr,
                                 c.ArrivalDateStr, c.StatusStr
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
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetSubmittedNotifications(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<NotificationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new NotificationServices().GetSubmittedNotifications(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationServices().SearchSubmittedNotifications(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ImporterName : sortColumnIndex == 2 ? c.ReferenceCode : sortColumnIndex == 3 ? c.PermitValue :
                    sortColumnIndex == 4 ? c.ProductName : sortColumnIndex == 5 ? c.QuantityToDischargeStr : sortColumnIndex == 6 ? c.ArrivalDateStr : sortColumnIndex == 7 ? c.DischargeDateStr : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),c.ReferenceCode, c.ImporterName,
                                  c.PermitValue, c.ProductName, c.QuantityToDischargeStr, c.AmountDueStr,
                                 c.ArrivalDateStr, c.StatusStr
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
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetProcessingNotifications(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<NotificationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new NotificationServices().GetProcessingNotifications(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationServices().SearchProcessingNotifications(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ImporterName : sortColumnIndex == 2 ? c.ReferenceCode : sortColumnIndex == 3 ? c.PermitValue :
                    sortColumnIndex == 4 ? c.ProductName : sortColumnIndex == 5 ? c.QuantityToDischargeStr : sortColumnIndex == 6 ? c.ArrivalDateStr : sortColumnIndex == 7 ? c.DischargeDateStr : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.ReferenceCode, c.ImporterName,
                                 c.PermitValue, c.ProductName, c.QuantityToDischargeStr, c.AmountDueStr,
                                 c.ArrivalDateStr, c.StatusStr
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
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetApprovedNotifications(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<NotificationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new NotificationServices().GetApprovedNotifications(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationServices().SearchApprovedNotifications(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ImporterName : sortColumnIndex == 2 ? c.ReferenceCode : sortColumnIndex == 3 ? c.PermitValue :
                    sortColumnIndex == 4 ? c.ProductName : sortColumnIndex == 5 ? c.QuantityToDischargeStr : sortColumnIndex == 6 ? c.ArrivalDateStr : sortColumnIndex == 7 ? c.DischargeDateStr : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.ReferenceCode,c.ImporterName,
                                  c.PermitValue, c.ProductName, c.QuantityToDischargeStr, c.AmountDueStr,
                                 c.ArrivalDateStr, c.StatusStr
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
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support,DownstreamDirector")]
        public ActionResult GetRejectedNotifications(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<NotificationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new NotificationServices().GetRejectedNotifications(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationServices().SearchRejectedNotifications(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ImporterName : sortColumnIndex == 2 ? c.ReferenceCode : sortColumnIndex == 3 ? c.PermitValue :
                    sortColumnIndex == 4 ? c.ProductName : sortColumnIndex == 5 ? c.QuantityToDischargeStr : sortColumnIndex == 6 ? c.ArrivalDateStr : sortColumnIndex == 7 ? c.DischargeDateStr : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),  c.ReferenceCode,c.ImporterName,
                                 c.PermitValue, c.ProductName, c.QuantityToDischargeStr, c.AmountDueStr,
                                 c.ArrivalDateStr, c.StatusStr
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
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpGet]
        [Authorize(Roles = "Banker,Bank_User")]
        public ActionResult GetBankAssignedNotifications(JQueryDataTableParamModel param)
        {
            try
            { //SearchBankJobHistory
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }
                IEnumerable<NotificationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new NotificationServices().GetBankAssignedNotifications(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationServices().SearchBankAssignedNotifications(param.sSearch, importerInfo.Id);
                    countG = filteredParentMenuObjects.Count(); 
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.ImporterName : 
                    sortColumnIndex == 3 ? c.ProductCode : sortColumnIndex == 4 ? c.QuantityToDischargeStr : sortColumnIndex == 5 ? c.ArrivalDateStr :c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;
                
                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.ImporterName, c.ProductCode, c.QuantityToDischargeStr,
                                 c.ArrivalDateStr, c.StatusStr
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
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpGet]
        [Authorize(Roles = "Banker")]
        public ActionResult GetBankNotificationHistory(JQueryDataTableParamModel param)
        {
            try
            { 
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }
                IEnumerable<NotificationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects =  new NotificationServices().GetBankNotificationHistory(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationServices().SearchBankNotificationHistory(param.sSearch, importerInfo.Id);
                    countG = filteredParentMenuObjects.Count(); 
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.ImporterName :
                    sortColumnIndex == 3 ? c.ProductCode : sortColumnIndex == 4 ? c.QuantityToDischargeStr : sortColumnIndex == 5 ? c.ArrivalDateStr :c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.ImporterName, c.ProductCode, c.QuantityToDischargeStr,
                                 c.ArrivalDateStr, c.StatusStr
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
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Bank_User")]
        public ActionResult GetBankUserNotificationHistory(JQueryDataTableParamModel param)
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }
                
                IEnumerable<NotificationObject> filteredParentMenuObjects;
                var countG = 0;
                 
                var pagedParentMenuObjects =  new NotificationServices().GetBankUserNotificationHistory(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.UserProfileObject.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationServices().SearchBankUserNotificationHistory(param.sSearch, importerInfo.UserProfileObject.Id);
                    countG = filteredParentMenuObjects.Count(); 
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.ImporterName :
                    sortColumnIndex == 3 ? c.ProductCode : sortColumnIndex == 4 ? c.QuantityToDischargeStr : sortColumnIndex == 5 ? c.ArrivalDateStr :c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.ImporterName, c.ProductCode, c.QuantityToDischargeStr,
                                 c.ArrivalDateStr, c.StatusStr
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
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpGet]
        [Authorize(Roles = "Employee")]
        public ActionResult GetEmployeeNotifications(JQueryDataTableParamModel param)
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }
                var userId = User.Identity.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }

                IEnumerable<NotificationObject> filteredParentMenuObjects;
                int countG;

                var pagedParentMenuObjects = GetEmployeeNotifications(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.UserProfileObject.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationServices().SearchEmployeeNotifications(param.sSearch, importerInfo.UserProfileObject.Id);
                    countG = filteredParentMenuObjects.Count(); 
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.PermitValue :
                    sortColumnIndex == 3 ? c.ProductName : sortColumnIndex == 4 ? c.QuantityToDischargeStr : sortColumnIndex == 5 ? c.ArrivalDateStr :c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.PermitValue, c.ProductName, c.QuantityToDischargeStr, c.AmountDueStr,
                                 c.ArrivalDateStr, c.StatusStr
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
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetImporterNotifications(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<NotificationObject> filteredParentMenuObjects;
                int countG;

                var id = GetImporterId();
                if (id < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }
                

                var pagedParentMenuObjects = GetCompanyNotifications(param.iDisplayLength, param.iDisplayStart, out countG, id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationServices().SearchCompanyNotifications(param.sSearch, id);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : sortColumnIndex == 2 ? c.PermitValue :
                    sortColumnIndex == 3 ? c.ProductName : sortColumnIndex == 4 ? c.QuantityToDischargeStr : sortColumnIndex == 5 ? c.ArrivalDateStr :c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.ReferenceCode, c.PermitValue, c.ProductName, c.QuantityToDischargeStr, c.AmountDueStr,
                                 c.ArrivalDateStr, c.StatusStr
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
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public string GetVolumeBalance(long dischargeQuantityTolerance, long estimatedQuantity, double quantityImported)
        {
            try
            {
                const float percentageLimit = 100;
                if (estimatedQuantity > quantityImported)
                {
                    var balaceVolume = ((dischargeQuantityTolerance*estimatedQuantity)/percentageLimit) -
                                       (double) quantityImported;
                    return " - (Balance Volume(MT) : " + balaceVolume.ToString("N") + ")";
                }
              
                return "";
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return "";
            }
        }

         public ActionResult GetRrr(NotificationRequirementDetails details)
         {
             var baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority;

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

                if (details == null || details.NotificationId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "An unknown error was encountered. The payment process could not be initiated. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                var invoice = new InvoiceObject
                {
                    PaymentTypeId = details.PaymentTypeId,
                    TotalAmountDue = details.ExpenditionaryFee.Amount,
                    AmountPaid = 0,
                    ExpiryDate = DateTime.Now.AddDays(7),
                    ServiceDescriptionId = (int)ServiceDescriptionEnum.Vessel_Arrival_Expeditionary_Fee,
                    Status = (int)NotificationStatusEnum.Pending,
                    //Status = (int)NotificationStatusEnum.Paid,
                    ImporterId = importerInfo.Id,
                    IPAddress = ClientIpHelper.GetClientIpAddress(Request),
                    DateAdded = DateTime.Now,
                    InvoiceItemObjects = new List<InvoiceItemObject>()
                };

                invoice.InvoiceItemObjects.Add(new InvoiceItemObject
                {
                    FeeId = details.ExpenditionaryFee.FeeId,
                    AmountDue = details.ExpenditionaryFee.Amount
                });

                var invStatus = new InvoiceServices().AddExpenditionaryInvoice(invoice, details.NotificationId);
                if (invStatus.InvoiceId < 1 || string.IsNullOrEmpty(invStatus.RefCode))
                {
                    gVal.Code = -5;
                    gVal.Error = "Application was submitted but with errors. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var url = "";
                if (details.PaymentTypeId == (int)PaymentType.Bank)
                {
                    url = baseUrl + "/Transaction/Invoice";
                }
                else
                {
                    url = baseUrl + "/Transaction/WebPayment2";
                }

                var remitaRef = "";
                var rrr = new SplitPayments().PostSplitPaymentForExpenditionaryFee(details.ExpenditionaryFee.Amount, invStatus.RefCode, importerInfo.Name, importerInfo.UserProfileObject.Email, importerInfo.UserProfileObject.PhoneNumber, url, details.ExpenditionaryFee);

                if (rrr.Code < 1 || rrr.Error.Contains("<"))
                {
                    gVal.Code = -11;
                    gVal.ErrorCode = -9;
                    gVal.Error = "A payment Reference Number could not be provided for the Expenditionar fee because the payment gateway could not be reached. We are sorry for this unforseen issue and will resolve it ASAP.";
                    remitaRef = "Not Available";
                }
                else
                {

                    remitaRef = rrr.Error;
                    gVal.ErrorCode = 5;
                    gVal.Code = 5;
                    var tt = new InvoiceServices().UpdateInvoiceRrr(invStatus.RefCode, rrr.Error);
                    if (tt < 1)
                    {
                        gVal.Code = -7;
                    }
                }

                gVal.RemitRes = rrr.Error;
                string hash;
                if ((gVal.ErrorCode < 1 && gVal.ErrorCode == -9) || rrr.Error.Contains("<"))
                {
                    hash = "";
                }
                else
                {
                    hash = Hasher.GenerateHash(remitaRef);
                }

                gVal.Hash = hash;
                gVal.Rrr = rrr.Error;
                gVal.CompanyName = importerInfo.Name;
                gVal.RedirectUri = url;
                gVal.RefCode = invStatus.RefCode;
                gVal.NotificationId = details.NotificationId;
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                if (gVal.Code > 0)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Error = "Notification process failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Notification(NotificationObject notification)
        {
            var baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority;

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

                var validationResult = ValidateNotification(notification);

                if (validationResult.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please Provide all required fields and try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (notification.ClassificationId == (int)NotificationClassEnum.Coastal_Importation)
                {
                    var limitStatus = CalculateQuantityLimit(notification);
                    if (limitStatus.Code < 1)
                    {
                        return Json(limitStatus, JsonRequestBehavior.AllowGet);
                    }
                }

                var invoice = new InvoiceObject
                {
                    PaymentTypeId = notification.PaymentTypeId,
                    TotalAmountDue = notification.DerivedValue,
                    AmountPaid = 0,
                    ExpiryDate = DateTime.Now.AddDays(7),
                    ServiceDescriptionId = (int)ServiceDescriptionEnum.Vessel_Arrival_Notification_Fee,
                    //Status = (int)NotificationStatusEnum.Pending,
                    Status = (int)NotificationStatusEnum.Paid,
                    ImporterId = importerInfo.Id,
                    IPAddress = ClientIpHelper.GetClientIpAddress(Request),
                    DateAdded = DateTime.Now,
                    InvoiceItemObjects = new List<InvoiceItemObject>()
                };

                notification.FeeObjects.ForEach(l =>
                {
                    invoice.InvoiceItemObjects.Add(new InvoiceItemObject
                    {
                        FeeId = l.FeeId,
                        AmountDue = l.Amount
                    });
                });

                const int expenditionaryFee = (int)FeeTypeEnum.Expeditionary;
                var expFee = notification.FeeObjects.Find(o => o.FeeTypeId == expenditionaryFee);
                var isExpenditionaryApplicable = expFee != null && expFee.FeeId > 0;

                var invStatus = new InvoiceServices().AddInvoice(invoice);
                if (invStatus.InvoiceId < 1 || string.IsNullOrEmpty(invStatus.RefCode))
                {
                    gVal.Code = -5;
                    gVal.Error = "Application was submitted but with errors. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var url = "";
                if (notification.PaymentTypeId == (int)PaymentType.Bank)
                {
                    url = baseUrl + "/Transaction/Invoice";
                }
                else
                {
                    url = baseUrl + "/Transaction/WebPayment2";
                }

                var remitaRef = "";
                var rrr = new SplitPayments().PostSplitPaymentForNotificationFee(notification.DerivedValue, invStatus.RefCode, importerInfo.Name, importerInfo.UserProfileObject.Email, importerInfo.UserProfileObject.PhoneNumber, url, notification.FeeObjects, isExpenditionaryApplicable);

                if (rrr.Code < 1 || rrr.Error.Contains("<"))
                {
                    gVal.Code = -11;
                    gVal.ErrorCode = -9;
                    gVal.Error = "A payment Reference Number could not be provided for your Notification because The payment gateway could not be reached. Your Notification has been saved so that you will be able to make payment later. We are sorry for this unforseen issue and will resolve it ASAP.";
                    remitaRef = "Not Available";
                }
                else
                {
                    remitaRef = rrr.Error;
                   remitaRef = Guid.NewGuid().ToString().Split('-')[0];
                    gVal.ErrorCode = 5;
                    gVal.Code = 5;
                    var tt = new InvoiceServices().UpdateInvoiceRrr(invStatus.RefCode, rrr.Error);
                    if (tt < 1)
                    {
                        gVal.Code = -7;
                    }
                }

                gVal.RemitRes = rrr.Error;
                notification.InvoiceId = invStatus.InvoiceId;
                notification.ReferenceCode = invStatus.RefCode;
                notification.DateCreated = DateTime.Now;
                notification.ImporterId = importerInfo.Id;
                //notification.Status = (int)NotificationStatusEnum.Pending;
                notification.Status = (int)NotificationStatusEnum.Paid;
                var appId = new NotificationServices().AddNotification(notification);
                if (appId < 1)
                {
                    new InvoiceServices().DeleteInvoice(invStatus.InvoiceId);
                    gVal.Code = -1;
                    gVal.Error = appId == -2 ? "Notification could not be Processed. Please try again." : "A similar notification by you is still being processing.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                string hash;
                if ((gVal.ErrorCode < 1 && gVal.ErrorCode == -9) || rrr.Error.Contains("<"))
                {
                    hash = "";
                }
                else
                {
                    hash = Hasher.GenerateHash(remitaRef);
                }

                gVal.Hash = hash;
                gVal.Rrr = rrr.Error;
                gVal.RedirectUri = url;
                gVal.RefCode = invStatus.RefCode;
                gVal.NotificationId = appId;
                var mailObj = new AppMailObject
                {
                    UserId = importerInfo.UserProfileObject.Id,
                    ImporterName = importerInfo.Name,
                    AmountDue = invoice.TotalAmountDue.ToString("n"),
                    ReferenceNumber = invStatus.RefCode,
                    PaymentReference = rrr.Error,
                    ApplicationCategory = Enum.GetName(typeof(NotificationClassEnum), notification.ClassificationId).Replace("_", " "),
                    //PaymentStatus = "Pending",
                    PaymentStatus = "Paid",
                    DateApplied = DateTime.Now.ToString("dd/MM/yyyy"),
                    TypeId = 2,
                    Email = importerInfo.UserProfileObject.Email
                };
                await SendMail(mailObj);
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                if (gVal.Code > 0)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Error = "Notification process failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditNotification(NotificationObject iNotification)
        {
            var baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority;

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

                var validationResult = ValidateNotification(iNotification);

                if (validationResult.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please Provide all required fields and try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (iNotification.ClassificationId == (int)NotificationClassEnum.Coastal_Importation)
                {
                    var limitStatus = CalculateQuantityLimit(iNotification);
                    if (limitStatus.Code < 1)
                    {
                        return Json(limitStatus, JsonRequestBehavior.AllowGet);
                    }
                }

                if (Session["_notification"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var notification = Session["_notification"] as NotificationObject;

                if (notification == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                notification.DischargeDate = iNotification.DischargeDate;
                notification.QuantityToDischarge = iNotification.QuantityToDischarge;
                notification.QuantityOnVessel = iNotification.QuantityOnVessel;
                notification.ArrivalDate = iNotification.ArrivalDate;
                notification.CargoInformationTypeId = iNotification.CargoInformationTypeId;
                notification.PortOfOriginId = iNotification.PortOfOriginId;
                notification.DischargeDepotId = iNotification.DischargeDepotId;
                notification.AmountDue = iNotification.AmountDue;
                notification.DischargeDate = iNotification.DischargeDate;
                notification.PortName = iNotification.PortName;

                var ip = ClientIpHelper.GetClientIpAddress(Request);
                var invoice = new InvoiceObject
                {
                    Id = notification.InvoiceId,
                    PaymentTypeId = notification.PaymentTypeId,
                    TotalAmountDue = notification.AmountDue,
                    AmountPaid = 0,
                    IPAddress = ip,
                    InvoiceItemObjects = new List<InvoiceItemObject>()
                };

                iNotification.FeeObjects.ForEach(l =>
                {
                    invoice.InvoiceItemObjects.Add(new InvoiceItemObject
                    {
                        InvoiceId = notification.InvoiceId,
                        FeeId = l.FeeId,
                        AmountDue = l.Amount
                    });
                });

                var invStatus = new InvoiceServices().UpdateInvoiceWithouthAttach(invoice);
                if (invStatus.InvoiceId < 1)
                {
                    gVal.Code = -5;
                    gVal.Error = "Application was submitted but with errors. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var url = "";
                var rrr = "";
                if (notification.PaymentTypeId == (int)PaymentType.Bank)
                {
                    url = baseUrl + "/Transaction/Invoice";
                }
                else
                {
                    url = baseUrl + "/Transaction/WebPayment2";
                }

                const int expenditionaryFee = (int)FeeTypeEnum.Expeditionary;
                var expFee = notification.FeeObjects.Find(o => o.FeeTypeId == expenditionaryFee);
                var isExpenditionaryApplicable = expFee != null && expFee.FeeId > 0;
                //ServiceType

                if (string.IsNullOrEmpty(invStatus.RRR))
                {
                    var remitaRef = "";
                    var remitaResponse = new SplitPayments().PostSplitPaymentForNotificationFee(notification.DerivedValue, invStatus.RefCode, importerInfo.Name, importerInfo.UserProfileObject.Email, importerInfo.UserProfileObject.PhoneNumber, url, notification.FeeObjects, isExpenditionaryApplicable);

                    if (remitaResponse.Code < 1 || remitaResponse.Error.Contains("<"))
                    {
                        gVal.Code = -11;
                        gVal.ErrorCode = -9;
                        gVal.Error = "A payment Reference Number could not be provided for your Notification because The payment gateway could not be reached. Your Notification has been saved so that you will be able to make payment later. We are sorry for this unforseen issue and will resolve it ASAP.";
                        remitaRef = "Not Available";
                    }
                    else
                    {
                        remitaRef = remitaResponse.Error;
                        rrr = remitaResponse.Error;
                        gVal.ErrorCode = 5;
                        gVal.Code = 5;
                        var tt = new InvoiceServices().UpdateInvoiceRrr(invStatus.RefCode, remitaResponse.Error);
                        if (tt < 1)
                        {
                            gVal.Code = -7;
                        }
                    }

                    gVal.RemitRes = remitaRef;

                }

                else
                {
                    rrr = invStatus.RRR;
                    if (isExpenditionaryApplicable)
                    {
                        var newInvoice = new InvoiceObject
                        {
                            PaymentTypeId = notification.PaymentTypeId,
                            TotalAmountDue = notification.DerivedValue,
                            AmountPaid = 0,
                            ExpiryDate = DateTime.Now.AddDays(7),
                            ServiceDescriptionId = (int)ServiceDescriptionEnum.Import_Permit_Application_Fee,
                            Status = (int)NotificationStatusEnum.Pending,
                            ImporterId = importerInfo.Id,
                            IPAddress = ClientIpHelper.GetClientIpAddress(Request),
                            DateAdded = DateTime.Now,
                            InvoiceItemObjects = new List<InvoiceItemObject>()
                        };

                        notification.FeeObjects.ForEach(l =>
                        {
                            invoice.InvoiceItemObjects.Add(new InvoiceItemObject
                            {
                                FeeId = l.FeeId,
                                AmountDue = l.Amount
                            });
                        });
                        
                        var invRes = new InvoiceServices().AddExpenditionaryInvoice(newInvoice, notification.InvoiceId);
                        if (invRes.InvoiceId < 1 || string.IsNullOrEmpty(invRes.RefCode))
                        {
                            gVal.Code = -5;
                            gVal.Error = "Application was submitted but with errors. Please try again.";
                            return Json(gVal, JsonRequestBehavior.AllowGet);
                        }

                        var remitaRef = "";
                        var remitaResponse = new SplitPayments().PostSplitPaymentForExpenditionaryFee(notification.DerivedValue, invRes.RefCode, importerInfo.Name, importerInfo.UserProfileObject.Email, importerInfo.UserProfileObject.PhoneNumber, url, expFee);

                        if (remitaResponse.Code < 1 || remitaResponse.Error.Contains("<"))
                        {
                            gVal.Code = -11;
                            gVal.ErrorCode = -9;
                            gVal.Error = "A payment Reference Number could not be provided for your Notification because The payment gateway could not be reached. Your Notification has been saved so that you will be able to make payment later. We are sorry for this unforseen issue and will resolve it ASAP.";
                            remitaRef = "Not Available";
                        }
                        else
                        {
                            remitaRef = remitaResponse.Error;
                            rrr = remitaResponse.Error;
                            gVal.ErrorCode = 5;
                            gVal.Code = 5;
                            var tt = new InvoiceServices().UpdateInvoiceRrr(invRes.RefCode, remitaResponse.Error);
                            if (tt < 1)
                            {
                                gVal.Code = -7;
                            }
                        }
                       
                        gVal.RemitRes = remitaRef;
                    }
                }

                var res = new NotificationServices().UpdateNotification(notification);
                if (res < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Notification could not be Processed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                string hash;
                if ((gVal.ErrorCode < 1 && gVal.ErrorCode == -9) || string.IsNullOrEmpty(rrr))
                {
                    hash = "";
                }
                else
                {
                    hash = Hasher.GenerateHash(rrr);
                }

                gVal.Hash = hash;
                gVal.Rrr = rrr;
                gVal.RedirectUri = url;
                gVal.RefCode = invStatus.RefCode;
                gVal.NotificationId = res;
                var mailObj = new AppMailObject
                {
                    UserId = importerInfo.UserProfileObject.Id,
                    ImporterName = importerInfo.Name,
                    AmountDue = invoice.TotalAmountDue.ToString("n"),
                    ReferenceNumber = invStatus.RefCode,
                    PaymentReference = rrr,
                    ApplicationCategory = Enum.GetName(typeof(NotificationClassEnum), notification.ClassificationId).Replace("_", " "),
                    PaymentStatus = "Pending",
                    DateApplied = DateTime.Now.ToString("dd/MM/yyyy"),
                    TypeId = 2,
                    Email = importerInfo.UserProfileObject.Email
                };
               await SendMail(mailObj);
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                if (gVal.Code > 0)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = -1;
                gVal.Error = "Notification Modification process failed. Please try again later";
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
                    type = (int)MessageEventEnum.New_Notification;
                }

                else
                {
                    type = (int)MessageEventEnum.Notification_Edit;
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
                    msg.Subject = msg.Subject.Replace("\n", "<br/>");
                    msgBody = "DPR/DS/IEP/" + DateTime.Today.ToString("dd/MM/yyyy") + "/" + sta + "<br/><br/>";
                   
                    msg.MessageContent = msg.MessageContent.Replace("{payment reference}", model.PaymentReference).Replace("{company}", model.ImporterName);

                    var sr = "<br/><b>Reference Number</b>:" + model.ReferenceNumber +
                               "<br/><b>Payment Reference (RRR)</b>: " + model.PaymentReference +
                               "<br/><b>Notification Category</b>: " + model.ApplicationCategory +
                               "<br/><b>Amount Due</b>: " + model.AmountDue +
                               "<br/><b>Payment Status</b>:" + model.PaymentStatus +
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
        
        private List<DocumentTypeObject> GetApplicationRequirements(RequirementProp requirementProp)
        {
            return new DocumentTypeServices().GetApplicationStageDocumentTypes(requirementProp) ?? new List<DocumentTypeObject>();
        }

        public ActionResult GetNotificationForEdit(long id)
        {
            var baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority;

            if (id < 1)
            {
                return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
            }

            var app = new NotificationServices().GetNotificationForEdit(id);

            if (app == null || app.Id < 1)
            {
                return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(app.Rrr))
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }
            
                const int expenditionaryFee = (int)FeeTypeEnum.Expeditionary;
                var expFee = app.FeeObjects.Find(o => o.FeeTypeId == expenditionaryFee);
                var isExpenditionaryApplicable = expFee != null && expFee.FeeId > 0;
            
                var url = "";
                if (app.PaymentTypeId == (int)PaymentType.Bank)
                {
                    url = baseUrl + "/Transaction/Invoice";
                }
                else
                {
                    url = baseUrl + "/Transaction/WebPayment2";
                }

                var rrr = new SplitPayments().PostSplitPaymentForNotificationFee(app.AmountDue, app.ReferenceCode, importerInfo.Name, importerInfo.UserProfileObject.Email, importerInfo.UserProfileObject.PhoneNumber, url, app.FeeObjects, isExpenditionaryApplicable);
                if (rrr.Code > 0 && !rrr.Error.Contains("<"))
                {
                    new InvoiceServices().UpdateInvoiceRrr(app.ReferenceCode, rrr.Error);
                    app.Rrr = rrr.Error;
                }
            }
          

            Session["_notification"] = app;
            return Json(app, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBankNotifications(long id)
        {
            if (id < 1)
            {
                return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
            }

            var importerInfo = GetLoggedOnUserInfo();
            if (importerInfo == null || importerInfo.Id < 1)
            {
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }

            var app = new NotificationServices().GetBankNotifications(id, importerInfo.Id);

            if (app == null || app.Id < 1)
            {
                return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
            }

            return Json(app, JsonRequestBehavior.AllowGet);
        }

         //
        public ActionResult SearchBankNotifications(string referenceCode)
        {
            if (string.IsNullOrEmpty(referenceCode))
            {
                return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
            }

            var importerInfo = GetLoggedOnUserInfo();
            if (importerInfo == null || importerInfo.Id < 1)
            {
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }

            var app = new NotificationServices().SearchBankProcessedNotifications(referenceCode, importerInfo.Id);

            if (app == null || app.Id < 1)
            {
                return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
            }

            return Json(app, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBankProcessedNotifications(long id)
        {
            if (id < 1)
            {
                return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
            }

            var importerInfo = GetLoggedOnUserInfo();
            if (importerInfo == null || importerInfo.Id < 1)
            {
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }

            var app = new NotificationServices().GetBankProcessedNotifications(id, importerInfo.Id);

            if (app == null || app.Id < 1)
            {
                return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
            }

            return Json(app, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBankNotification(long id)
        {
             if (id < 1)
            {
                return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
            }

            var importerInfo = GetLoggedOnUserInfo();
            if (importerInfo == null || importerInfo.Id < 1)
            {
                return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
            }

            var app = new NotificationServices().GetBankNotification(id, importerInfo.Id);

            if (app == null || app.Id < 1)
            {
                return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
            }

            return Json(app, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult CheckNotificationForSubmit(long id) 
        {
           var gVal = new GenericValidator();
            if (id < 1)
            {
                gVal.Code = -1;
                gVal.Error = "An unknown error was encountered. Please try again";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

            var checkOutCome = new NotificationServices().CheckNotificationSubmit(id);
            if (checkOutCome.Code < 1)
            {
                if (checkOutCome.Code == -7)
                {
                    checkOutCome.Error = "An Expenditionary fee already incured for this Notification is yet to be paid for.";
                    return Json(checkOutCome, JsonRequestBehavior.AllowGet);
                }

                checkOutCome.Error = "An unknown error was encountered. The Notification requirement status could not be evaluated. Please try again later.";
                return Json(checkOutCome, JsonRequestBehavior.AllowGet);
            }

            return Json(checkOutCome, JsonRequestBehavior.AllowGet);
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

        public ActionResult SubmitNotification(long id)
        {
            var gVal = new GenericValidator();
            if (id < 1)
            {
                gVal.Code = -1;
                gVal.Error = "Unknown error was encountered. Please try again";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            
            var ntfDetails = new NotificationServices().SubmitNotification(id);
            if ((ntfDetails.UnsuppliedDocuments.Any() || ntfDetails.ExpenditionaryFee.Amount > 0) && ntfDetails.Code < 10)
            {
                return Json(ntfDetails, JsonRequestBehavior.AllowGet);
            }
            
            var assignmentStatus = new WorkFlowServices().AssignNotificationToEmployee(id);
            if (!assignmentStatus)
            {
                new NotificationServices().UnSubmitNotification(id);
                gVal.Code = -1;
                gVal.Error = "Unknown error was encountered. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

            gVal.Code = 10;
            gVal.Error = "Notification was successfully submitted.";
            return Json(gVal, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGenericList()
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    return Json(new List<PermitObject>(), JsonRequestBehavior.AllowGet);
                }

                var genList = new NotificationGenericList
                {
                     //Countries = GetCountriesWithPorts(),
                     //DepotList = GetDepotList(),
                     //Products = GetProducts(),
                     PermitObjects = new NotificationServices().GetApplicantsValidPermits(importerInfo.Id),
                     ImportSettingObject = GetImportSetting(),
                     ImportClasses = GetImportClasses()
                };
                
                return Json(genList, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new List<ProductObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetDepotCollection()
        {
            try
            {
                var genList = new NotificationGenericList
                {
                    DepotList = GetDepotList()
                };

                return Json(genList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new List<ProductObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult RefreshSession()
        {
            try
            {
                Session["_notification"] = null;
                return Json(5, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetNotification(long id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetNotificationInfo(id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetApplicantsActivePermits()
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    return Json(new List<PermitObject>(), JsonRequestBehavior.AllowGet);
                }

                var permits = new NotificationServices().GetApplicantsValidPermits(importerInfo.Id);
                if (!permits.Any())
                {
                    return Json(new List<PermitObject>(), JsonRequestBehavior.AllowGet);
                }

                return Json(permits, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<PermitObject>(), JsonRequestBehavior.AllowGet);
            }
        }

       public ActionResult GetPermitApplicationByPermitValue(string permitValue)
        {
            try
            {
                if (string.IsNullOrEmpty(permitValue))
                {
                    return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<NotificationObject>(), JsonRequestBehavior.AllowGet);
                }
                var app = GetApplicationByPermitNumber(permitValue, importerInfo.Id);
                if (app == null || app.Id < 1)
                {                    
                    var licRes = AddUserPermit(importerInfo.Name, importerInfo.Id);
                    return Json(licRes, JsonRequestBehavior.AllowGet);
                }

                return Json(app, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
            }
        }

       private List<NotificationObject> GetEmployeeNotifications(int? itemsPerPage, int? pageNumber, out int countG, long userId)
        {
            return new NotificationServices().GetEmployeeNotifications(itemsPerPage, pageNumber, out countG, userId) ?? new List<NotificationObject>();
        }
       private List<NotificationObject> GetCompanyNotifications(int? itemsPerPage, int? pageNumber, out int countG, long Id)
        {
            return new NotificationServices().GetCompanyNotifications(itemsPerPage, pageNumber, out countG, Id) ?? new List<NotificationObject>();
        }
       private List<NotificationObject> GetNotifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            return new NotificationServices().GetNotifications(itemsPerPage, pageNumber, out countG) ?? new List<NotificationObject>();
        }

        private ImportSettingObject GetImportSettings()
        {
            return new NotificationServices().GetImportSettings() ?? new ImportSettingObject();
        }
      
       public ActionResult GetNotificationProcesses(long notificationId)
       {
           var gVal = new GenericValidator();
            try
            {
                if (notificationId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Invalid selection!";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var notfication = new NotificationServices().GetNotificationProcesses(notificationId);
                return Json(notfication, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Notification information could not be retrieved.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            } 
        }
        
       [HttpPost]
       public ActionResult ComputeAmountDueWithRequirements(NotificationRequirementProp notificationProps)
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

                var setting = GetImportSettings();
                if (setting == null || setting.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "An unknown error was encountered. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                CalculationFactor calculationFactor;
                var appFees = new FeeServices().GetNotificationFees(out calculationFactor);
                if (!appFees.Any() || calculationFactor == null || calculationFactor.ImportSettingObject == null || calculationFactor.ImportSettingObject.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Notification process failed. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                var amountDue = 0.00;
                if (notificationProps.DischargeDate == DateTime.Today)
                {
                    amountDue = calculationFactor.Fees + calculationFactor.ExpenditionaryFee;
                }

                var daysLapse = notificationProps.DischargeDate - DateTime.Today;
                if (daysLapse.Days < calculationFactor.ImportSettingObject.VesselDischargeLeadTime)
                {
                    amountDue = calculationFactor.Fees + calculationFactor.ExpenditionaryFee;
                    calculationFactor.ExpenditionaryFeeApplicable = true;
                }
                else
                {
                    if (daysLapse.Days >= calculationFactor.ImportSettingObject.VesselDischargeLeadTime)
                    {
                        amountDue = calculationFactor.Fees;
                    }
                }

                if (!calculationFactor.ExpenditionaryFeeApplicable)
                {
                    const int expenditionaryFee = (int)FeeTypeEnum.Expeditionary;
                    appFees.Remove(appFees.Find(o => o.FeeTypeId == expenditionaryFee));
                }
               
                var requiredDocs = GetNotificationRequirements(notificationProps.NotificationClassId);
                if (!requiredDocs.Any())
                {
                    gVal.Code = -1;
                    gVal.Error = "Requesting processing failed. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                var quantity = GetVolumeBalance(setting.DischargeQuantityTolerance, notificationProps.EstimatedQuantity, notificationProps.TotalImportedQuantity);
                gVal.Code = 5;
                gVal.BalanceVolume = quantity; 
                gVal.DocumentTypeObjects = requiredDocs;
                gVal.FeeObjects = appFees;
                gVal.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum), notificationProps.NotificationClassId);
                var name = Enum.GetName(typeof(CargoTypeEnum), notificationProps.CargoInformationTypeId);
                if (name != null) gVal.CargoTypeName = name.Replace("_", " ");
                gVal.Extent = amountDue;
                gVal.ArrivalDate = notificationProps.ArrivalDate.ToString("dd/MM/yyyy");
                gVal.DischargeDate = notificationProps.DischargeDate.ToString("dd/MM/yyyy");
                gVal.UserName = importerInfo.Name;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Requesting processing failed. Please try again later.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            } 
        }

       private List<DocumentTypeObject> GetNotificationRequirements(int classId)
       {
           return new DocumentTypeServices().GetNotificationStageDocumentTypes(classId) ?? new List<DocumentTypeObject>();
       }
       private NotificationObject GetNotificationInfo(long appId)
       {
          return new NotificationServices().GetNotification(appId);
       }

       private ApplicationObject GetApplicationByPermitNumber(string permitValue, long id)
       {
           return new NotificationServices().GetImportApplicationByPermitNumber(permitValue, id);
       }

       private ApplicationItemObject GetPreviousDischargeInfo(long importApplicationId, long productId, out double tolerancePercentage)
       {
          return new ApplicationItemServices().GetPreviousDischargeInfo(importApplicationId, productId, out tolerancePercentage);
       }

       private GenericValidator CalculateQuantityLimit(NotificationObject notification)
       {
           var gVal = new GenericValidator();
           try
           {
               if (notification.PermitId != null)
               {
                   var appsetting = new ImportSettingServices().GetImportSetting();

                  if (appsetting == null || appsetting.DischargeQuantityTolerance < 1)
                   {
                       gVal.Code = -1;
                       gVal.Error = "An unknown error was encountered. Please try again later.";
                       return gVal;
                   }
                   const float percentageLimit = 100;
                   var toleranceLevel = (appsetting.DischargeQuantityTolerance * notification.ApplicationQuantity)/percentageLimit;
                   var previousQuantity = notification.QuantityImported;
                   var currentQuantity = previousQuantity < 1 ? notification.QuantityToDischarge : notification.QuantityToDischarge + previousQuantity;
                   var limit = notification.ApplicationQuantity + toleranceLevel;

                   if (currentQuantity > limit)
                   {
                       gVal.Code = -1;
                       gVal.Error = "The Quantity to Disharge must not be higher than the Quantity Applied for.";
                       return gVal;
                   }
                   gVal.Code = 5;
                   return gVal;
               }
               gVal.Code = -1;
               gVal.Error = "An unknown error was encountered. Please try again later.";
               return gVal;
           }
           catch (Exception)
           {
               gVal.Code = -1;
               gVal.Error = "An unknown error was encountered. Please try again later.";
               return gVal;
           }
       }
       private List<CountryObject> GetCountriesWithPorts()
       {
           return new CountryServices().GetCountriesWithPorts() ?? new List<CountryObject>();
       }
       private List<ImportClassObject> GetImportClasses()
       {
           return new ImportClassServices().GetImportClasses();
       }

       private ImportSettingObject GetImportSetting()
       {
           return new ImportSettingServices().GetImportSetting();
       }

       private List<DepotObject> GetDepotList()
       {
           return new JettyServices().GetDeotList() ?? new List<DepotObject>();
       }
       private List<ProductObject> GetProducts()
       {
           return new ProductServices().GetProducts() ?? new List<ProductObject>();
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
       private GenericValidator ValidateNotification(NotificationObject notification)
        {
            var gVal = new GenericValidator();
            try
            {
                if (notification.PermitId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "An unknown error was encountered. Please try again later.";
                    return gVal;
                }

                if (notification.ClassificationId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select Notification Type.";
                    return gVal;
                }
                
                if (notification.DischargeDepotId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select Discharge Depot.";
                    return gVal;
                }
               
                if (notification.ProductId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a product.";
                     return gVal;
                }

                if (string.IsNullOrEmpty(notification.PortName))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Load Port.";
                     return gVal;
                } 
                if (notification.QuantityToDischarge < 1) 
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Quantity to Discharge.";
                     return gVal;
                }

                if (notification.QuantityOnVessel < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Quantity on Vessel.";
                    return gVal;
                }
                
                if (notification.ArrivalDate <= DateTime.Today)
                {
                    gVal.Code = -1;
                    gVal.Error = "Vessel arrival date should not be less than or equal to today.";
                     return gVal;
                }

                if (notification.DischargeDate < notification.ArrivalDate)
                {
                    gVal.Code = -1;
                    gVal.Error = "Discharge date should be greater than or equal to the Arrival date.";
                     return gVal;
                }

                gVal.Code = 5;
                return gVal;
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Notification Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

       private List<GenericValidator> ValidateVessels(List<NotificationVesselObject> notificationVessels)
       {
           var gVals = new List<GenericValidator>();
           try
           {
              notificationVessels.ForEach(m =>
              {
                  var gVal = new GenericValidator();

                  if (m.VesselClassTypeId < 1)
                  {
                      gVal.Code = -1;
                      gVal.Error = "Please select Vessel Class.";
                      gVals.Add(gVal); 
                  }

                  if (string.IsNullOrEmpty(m.Name))
                  {
                      gVal.Code = -1;
                      gVal.Error = "Please provide Vessel Name.";
                      gVals.Add(gVal); 
                  }
              });

              return !gVals.Any() ? new List<GenericValidator>() : gVals;
           }
           catch (Exception)
           {
               return new List<GenericValidator>{ new GenericValidator
               {
                   Code = -1,
                   Error = "Notification Validation failed. Please provide all required fields and try again."
               }};
           }
       }

       private ApplicationObject AddUserPermit(string importerName, long importerId)
       {
           var gVal = new ApplicationObject();
           try
           {
               var filePath = System.Web.Hosting.HostingEnvironment.MapPath(Path.Combine("~/BulkUploads", "permits.json"));
               if (string.IsNullOrEmpty(filePath))
               {
                   return gVal;
               }

               List<PermitObject> licenses;
               var serializer = new JsonSerializer();
               using (var re = System.IO.File.OpenText(filePath))
               using (var reader = new JsonTextReader(re))
               {
                   licenses = serializer.Deserialize<List<PermitObject>>(reader);
               }

               //Honeywell Oil and Gas Limited

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
                   return gVal;
               }
               license.ImporterId = importerId;
               var status = new PermitServices().AddPermit(license);
               if (status < 1)
               {
                   return gVal;
               }

               return  GetApplicationByPermitNumber(license.PermitValue, importerId);
           }
           catch (Exception e)
           {
               ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
               return gVal;
           }
       }
    }
}
