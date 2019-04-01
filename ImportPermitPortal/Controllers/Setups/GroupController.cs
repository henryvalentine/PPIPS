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
    public class GroupController : Controller
    {
        [HttpGet]
        public ActionResult GetGroupObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<GroupObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetGroups(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new GroupServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<GroupObject>(), JsonRequestBehavior.AllowGet);
                }

                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<GroupObject, string> orderingFunction = (c => c.Name);

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
                return Json(new List<GroupObject>(), JsonRequestBehavior.AllowGet);
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
        public ActionResult AddGroup(GroupObject group)
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

                var validationResult = ValidateGroup(group);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new GroupServices().AddGroup(group);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Group upload failed. Please try again." : "The Group Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Group was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Group processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditGroup(GroupObject group)
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

                if (string.IsNullOrEmpty(group.Name.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Group.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_group"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldgroup = Session["_group"] as GroupObject;

                if (oldgroup == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                oldgroup.Name = group.Name.Trim();

                var docStatus = new GroupServices().UpdateGroup(oldgroup);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Group information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldgroup.Id;
                gVal.Error = "Group information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Group information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<GroupObject> GetGroups(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new GroupServices().GetGroups(itemsPerPage, pageNumber, out countG);
                
            }
            catch (Exception)
            {
                countG = 0;
                return new List<GroupObject>(); 
            }
        }

        public ActionResult GetGroup(long id)
        {
            try
            {


                var group = new GroupServices().GetGroup(id);
                if (group == null || group.Id < 1)
                {
                    return Json(new GroupObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_group"] = group;

                return Json(group, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new GroupObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteGroup(long id)
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
                var delStatus = new GroupServices().DeleteGroup(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Group could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Group Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        private GenericValidator ValidateGroup(GroupObject group)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(group.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select provide Group.";
                    return gVal;
                }


                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Group Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
