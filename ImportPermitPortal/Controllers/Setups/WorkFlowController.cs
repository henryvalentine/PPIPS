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
    public class WorkFlowController : Controller
    {
        [HttpGet]
        public ActionResult GetWorkFlowObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<WorkFlowObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetWorkFlows(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new WorkFlowServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<WorkFlowObject>(), JsonRequestBehavior.AllowGet);
                }

                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<WorkFlowObject, string> orderingFunction = (c => c.Name);

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
                return Json(new List<WorkFlowObject>(), JsonRequestBehavior.AllowGet);
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
        public ActionResult EditWorkFlow(WorkFlowObject WorkFlow)
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

                if (string.IsNullOrEmpty(WorkFlow.Name.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Application Stage.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_WorkFlow"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldWorkFlow = Session["_WorkFlow"] as WorkFlowObject;

                if (oldWorkFlow == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                oldWorkFlow.Name = WorkFlow.Name.Trim();

                var docStatus = new WorkFlowServices().UpdateWorkFlow(oldWorkFlow);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Application Stage information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldWorkFlow.Id;
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

        private List<WorkFlowObject> GetWorkFlows(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new WorkFlowServices().GetWorkFlows(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<WorkFlowObject>();
            }
        }

        public ActionResult GetWorkFlow(long id)
        {
            try
            {


                var WorkFlow = new WorkFlowServices().GetWorkFlow(id);
                if (WorkFlow == null || WorkFlow.Id < 1)
                {
                    return Json(new WorkFlowObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_WorkFlow"] = WorkFlow;

                return Json(WorkFlow, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new WorkFlowObject(), JsonRequestBehavior.AllowGet);
            }
        }

      

        private GenericValidator ValidateWorkFlow(WorkFlowObject WorkFlow)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(WorkFlow.Name))
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
                gVal.Error = "Application Stage Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
