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
    public class StepController : Controller
    {
        [HttpGet]
        public ActionResult GetStepObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<StepObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetSteps(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new StepServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<StepObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<StepObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name : c.ProcessName);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.Name, c.SequenceNumber.ToString(),c.ProcessName,c.ImportStageName };
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
                return Json(new List<StepObject>(), JsonRequestBehavior.AllowGet);
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
        public ActionResult AddStep(StepObject step)
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

                var validationResult = ValidateStep(step);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new StepServices().AddStep(step);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Step upload failed. Please try again." : "The Step Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Step was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Step steping failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditStep(StepObject step)
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

                if (string.IsNullOrEmpty(step.Name.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Step.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_Step"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldStep = Session["_Step"] as StepObject;

                if (oldStep == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                oldStep.Name = step.Name.Trim();

                var docStatus = new StepServices().UpdateStep(oldStep);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Step information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldStep.Id;
                gVal.Error = "Step information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Step information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<StepObject> GetSteps(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new StepServices().GetSteps(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<StepObject>();
            }
        }

        public ActionResult GetStep(long id)
        {
            try
            {


                var step = new StepServices().GetStep(id);
                if (step == null || step.Id < 1)
                {
                    return Json(new StepObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_Step"] = step;

                return Json(step, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new StepObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteStep(long id)
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
                var delStatus = new StepServices().DeleteStep(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Step could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Step Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator ValidateStep(StepObject step)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(step.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select provide Step.";
                    return gVal;
                }


                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "Step Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericStepList()
                {
                   
                   Groups = getGroups(),
                   Steps = getSteps(),
                   StepActivityTypes = getStepActivityTypes(),
                   ImportStages = getStagesWithProcesses()
                };
                

                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        
        public ActionResult RefreshImportStage()
        {
            try
            {
                return Json(getStagesWithProcesses(), JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }
       

        private List<GroupObject> getGroups()
        {

            try
            {
                return new GroupServices().GetGroups();

            }
            catch (Exception)
            {
                return new List<GroupObject>();
            }
        }


        private List<StepObject> getSteps()
        {

            try
            {
                return new StepServices().GetSteps();

            }
            catch (Exception)
            {
                return new List<StepObject>();
            }
        }

        private List<StepActivityTypeObject> getStepActivityTypes()
        {

            try
            {
                return new StepActivityTypeServices().GetStepActivityTypes();

            }
            catch (Exception)
            {
                return new List<StepActivityTypeObject>();
            }
        }

        private List<ImportStageObject> getStagesWithProcesses()
        {
            try
            {
                return new ImportStageServices().GetStagesWithProcesses();

            }
            catch (Exception)
            {
                return new List<ImportStageObject>();
            }
        }


      



       

    }
}
