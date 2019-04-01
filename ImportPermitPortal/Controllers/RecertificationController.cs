using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;
using Microsoft.AspNet.Identity;

namespace ImportPermitPortal.Controllers
{
    [Authorize]
    public class RecertificationController : Controller
    {
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
        [HttpGet]
        public ActionResult GetUserRecertificationObjects(JQueryDataTableParamModel param)
        {
            try
            {
                //get the id of logged in user
                var userId = User.Identity.GetUserId();
                var importer = new ImporterServices().GetImporterByLoggedOnUser(userId, true);
                var importerId = importer.Id;
                IEnumerable<RecertificationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetUserRecertifications(param.iDisplayLength, param.iDisplayStart, out countG, importerId);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new RecertificationServices().Search(param.sSearch, importerId);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<RecertificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<RecertificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : c.DateApplied.ToString());

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.ReferenceCode,c.DateApplied.ToString(), c.StatusStr };
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
                return Json(new List<RecertificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetImporterCertificationObjects(JQueryDataTableParamModel param)
        {
            try
            {
               
                IEnumerable<RecertificationObject> filteredParentMenuObjects;
                var countG = 0;

                var id = GetImporterId();
                if (id < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var pagedParentMenuObjects = GetUserRecertifications(param.iDisplayLength, param.iDisplayStart, out countG, id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new RecertificationServices().Search(param.sSearch, id);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<RecertificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<RecertificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : c.DateApplied.ToString());

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.ReferenceCode, c.DateApplied.ToString(), c.StatusStr };
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
                return Json(new List<RecertificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AddRecertification(RecertificationObject recertification)
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

                var validationResult = ValidateRecertification(recertification);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
              

                var appStatus = new RecertificationServices().AddRecertification(recertification);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Recertification upload failed. Please try again." : "The Recertification Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Recertification was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Recertification processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditRecertification(RecertificationObject recertification)
        {
            var gVal = new GenericValidator();

            try
            {
               

                var stat = ValidateRecertification(recertification);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_recertification"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldrecertification = Session["_recertification"] as RecertificationObject;

                if (oldrecertification == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                //oldrecertification.Name = recertification.Name;
                //oldrecertification.RecertificationOwnerId = recertification.RecertificationOwnerId;
                //oldrecertification.JettyId = recertification.JettyId;
                
                var docStatus = new RecertificationServices().UpdateRecertification(oldrecertification);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Recertification already exists." : "Recertification information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldrecertification.Id;
                gVal.Error = "Recertification information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Recertification information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }


        //public ActionResult GetGenericList()
        //{
        //    try
        //    {
        //        var newList = new GenericRecertificationList()
        //        {

        //           Recerts = getRecertificationObjects()
                   


        //        };


        //        return Json(newList, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception)
        //    {
        //        return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
        //    }
        //}


        private List<NotificationObject> GetUserRecertificationObjects()
        {
           //get the id of logged in user
                var userId = User.Identity.GetUserId();
            var importer = new ImporterServices().GetImporterByLoggedOnUser(userId, true);
            var importerId = importer.Id;
            try
            {
                return new NotificationServices().GetCompletedNotifications(importerId);

            }
            catch (Exception)
            {
                return new List<NotificationObject>();
            }
        }


        public ActionResult GetRecertification(long id)
        {
            try
            {


                var recertification = new RecertificationServices().GetRecertification(id);
                if (recertification == null || recertification.Id < 1)
                {
                    return Json(new RecertificationObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_recertification"] = recertification;

                return Json(recertification, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new RecertificationObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetCompletedNotifications(long importerId)
        {
            try
            {


                var recertification = new NotificationServices().GetCompletedNotifications(importerId);
                if (recertification == null)
                {
                    return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_recertification"] = recertification;

                return Json(recertification, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new NotificationObject(), JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetRecertificationDocs(long id)
        {
            try
            {


                var recertification = new RecertificationServices().GetRecertificationDocs(id);
                if (recertification == null)
                {
                    return Json(new List<DocumentObject>(), JsonRequestBehavior.AllowGet);
                }

                Session["_recertification"] = recertification;

                return Json(recertification, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new List<DocumentObject>(), JsonRequestBehavior.AllowGet);
            }
        }


      
        [HttpPost]
        public ActionResult DeleteRecertification(long id)
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
                var delStatus = new RecertificationServices().DeleteRecertification(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Recertification could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Recertification Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

       
        private GenericValidator ValidateRecertification(RecertificationObject recertification)
        {
            var gVal = new GenericValidator();
            try
            {
                if (recertification.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a Recertification.";
                    return gVal;
                }

               

                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "Recertification Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<RecertificationObject> GetUserRecertifications(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
                return new RecertificationServices().GetUserRecertifications(itemsPerPage, pageNumber, out countG, importerId);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<RecertificationObject>();
            }
        }

        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericRecertificationList()
                {

                    Recerts = getRecertificationObjects()



                };


                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }


        private List<NotificationObject> getRecertificationObjects()
        {
            //get the id of logged in user
            var userId = User.Identity.GetUserId();
            var importer = new ImporterServices().GetImporterByLoggedOnUser(userId, true);
            var importerId = importer.Id;
            try
            {
                return new NotificationServices().GetCompletedNotifications(importerId);

            }
            catch (Exception)
            {
                return new List<NotificationObject>();
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

    }
}
