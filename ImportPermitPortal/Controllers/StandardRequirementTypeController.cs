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
    public class StandardRequirementTypeController : Controller
    {

        [HttpGet]
        public ActionResult GetStandardRequirementTypeObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<StandardRequirementTypeObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetStandardRequirementTypes(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new StandardRequirementTypeServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<StandardRequirementTypeObject>(), JsonRequestBehavior.AllowGet);
                }

                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<StandardRequirementTypeObject, string> orderingFunction = (c => c.Name);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.Name };
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
                return Json(new List<StandardRequirementTypeObject>(), JsonRequestBehavior.AllowGet);
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

        public ActionResult AddStandardRequirementType(StandardRequirementTypeObject sReqType)
        {
            var gVal = new GenericValidator();

            try
            {
                if (!ModelState.IsValid)
                {
                    gVal.Code = -1;
                    gVal.Error = "Plese provide all required fields and try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var validationResult = ValidateStandardRequirementType(sReqType);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new StandardRequirementTypeServices().AddStandardRequirementType(sReqType);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Standard Requirement could not be added. Please try again." : "The Standard Requirement Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Standard Requirement was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Standard Requirement processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditStandardRequirementType(StandardRequirementTypeObject sReqType)
        {
            var gVal = new GenericValidator();

            try
            {
                if (!ModelState.IsValid)
                {
                    gVal.Code = -1;
                    gVal.Error = "Plese provide all required fields and try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var stat = ValidateStandardRequirementType(sReqType);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_StandardRequirementType"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldStandardRequirementType = Session["_StandardRequirementType"] as StandardRequirementTypeObject;

                if (oldStandardRequirementType == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldStandardRequirementType.Name = sReqType.Name.Trim();
                var docStatus = new StandardRequirementTypeServices().UpdateStandardRequirementType(oldStandardRequirementType);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Standard Requirement already exists." : "Standard Requirement information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldStandardRequirementType.Id;
                gVal.Error = "Standard Requirement information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Standard Requirement information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult SetDocSession(long id)
        {
            if (id < 1)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }

            Session["_appId"] = id;
            return Json(5, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStandardRequirementType(long id)
        {
            try
            {


                var sReqType = new StandardRequirementTypeServices().GetStandardRequirementType(id);
                if (sReqType == null || sReqType.Id < 1)
                {
                    return Json(new StandardRequirementTypeObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_StandardRequirementType"] = sReqType;

                return Json(sReqType, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new StandardRequirementTypeObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteStandardRequirementType(long id)
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
                var delStatus = new StandardRequirementTypeServices().DeleteStandardRequirementType(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Standard Requirement could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Standard Requirement Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<StandardRequirementTypeObject> GetStandardRequirementTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new StandardRequirementTypeServices().GetStandardRequirementTypes(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<StandardRequirementTypeObject>();
            }
        }

        private GenericValidator ValidateStandardRequirementType(StandardRequirementTypeObject sReqType)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(sReqType.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Standard Requirement.";
                    return gVal;
                }
                
                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Standard Requirement Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
