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
    public class ProcessController : Controller
    {
        [HttpGet]
        public ActionResult GetProcessObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ProcessObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetProcesss(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ProcessServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ProcessObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ProcessObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name : c.ImportStageName);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.Name, c.ImportStageName };
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
                return Json(new List<ProcessObject>(), JsonRequestBehavior.AllowGet);
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
        public ActionResult AddProcess(ProcessObject process)
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

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var validationResult = ValidateProcess(process);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new ProcessServices().AddProcess(process);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Process upload failed. Please try again." : "The Process Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Process was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Process processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditProcess(ProcessObject process)
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

                if (string.IsNullOrEmpty(process.Name.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Process.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_Process"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldProcess = Session["_Process"] as ProcessObject;

                if (oldProcess == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                oldProcess.Name = process.Name.Trim();

                var docStatus = new ProcessServices().UpdateProcess(oldProcess);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Process information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldProcess.Id;
                gVal.Error = "Process information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Process information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ProcessObject> GetProcesss(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new ProcessServices().GetProcesss(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<ProcessObject>();
            }
        }

        public ActionResult GetProcess(long id)
        {
            try
            {


                var Process = new ProcessServices().GetProcess(id);
                if (Process == null || Process.Id < 1)
                {
                    return Json(new ProcessObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_Process"] = Process;

                return Json(Process, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new ProcessObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteProcess(long id)
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
                var delStatus = new ProcessServices().DeleteProcess(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Process could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Process Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator ValidateProcess(ProcessObject process)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(process.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select provide Process.";
                    return gVal;
                }


                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "Process Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericProcessList()
                {
                    ImportStages = getImportStages()
                   
                };
                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericProcessList(), JsonRequestBehavior.AllowGet);
            }
        }

        private List<ImportStageObject> getImportStages()
        {
            
            try
            {
                return new ImportStageServices().GetImportStages();

            }
            catch (Exception)
            {
                return new List<ImportStageObject>();
            }
        }

    }
}
