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
    [Authorize]
    public class FaqCategoryController : Controller
    {

        [HttpGet]
        public ActionResult GetFaqCategoryObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<FaqCategoryObject> filteredParentMenuObjects;
                int countG;

                var pagedParentMenuObjects = GetFaqCategorys(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new FaqCategoryServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<FaqCategoryObject>(), JsonRequestBehavior.AllowGet);
                }

                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<FaqCategoryObject, string> orderingFunction = (c => c.Name);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels select new[] { Convert.ToString(c.Id), c.Name };

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
                return Json(new List<FaqCategoryObject>(), JsonRequestBehavior.AllowGet);
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
        public ActionResult AddFaqCategory(FaqCategoryObject faqCategory)
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

                var validationResult = ValidateFaqCategory(faqCategory);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new FaqCategoryServices().AddFaqCategory(faqCategory);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "FAQ Category could not be added. Please try again." : "The FAQ Category Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "FAQ Category was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "FAQ Category processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditFaqCategory(FaqCategoryObject faqCategory)
        {
            var gVal = new GenericValidator();

            try
            {
                var stat = ValidateFaqCategory(faqCategory);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_faqCategory"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldfaqCategory = Session["_faqCategory"] as FaqCategoryObject;

                if (oldfaqCategory == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldfaqCategory.Name = faqCategory.Name.Trim();
                oldfaqCategory.Description = faqCategory.Description;
                var docStatus = new FaqCategoryServices().UpdateFaqCategory(oldfaqCategory);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "FAQ Category already exists." : "FAQ Category information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldfaqCategory.Id;
                gVal.Error = "FAQ Category information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "FAQ Category information could not be updated. Please try again later";
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

        public ActionResult GetFaqCategory(long id)
        {
            try
            {
                var faqCategory = new FaqCategoryServices().GetFaqCategory(id);
                if (faqCategory == null || faqCategory.Id < 1)
                {
                    return Json(new FaqCategoryObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_faqCategory"] = faqCategory;

                return Json(faqCategory, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new FaqCategoryObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteFaqCategory(long id)
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
                var delStatus = new FaqCategoryServices().DeleteFaqCategory(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "FAQ Category could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "FAQ Category Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<FaqCategoryObject> GetFaqCategorys(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new FaqCategoryServices().GetFaqCategories(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<FaqCategoryObject>();
            }
        }

        private GenericValidator ValidateFaqCategory(FaqCategoryObject faqCategory)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(faqCategory.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide FAQ Category.";
                    return gVal;
                }
                
                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "FAQ Category Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
