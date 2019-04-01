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
    public class FaqController : Controller
    {
        
        [HttpGet]
        public ActionResult GetFaqObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<FaqObject> filteredParentMenuObjects;
                int countG;

                var pagedParentMenuObjects = GetFaqs(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new FaqServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<FaqObject>(), JsonRequestBehavior.AllowGet);
                }

                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<FaqObject, string> orderingFunction = (c => c.FaqCategoryName);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.FaqCategoryName, c.Question, c.Answer};
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
                return Json(new List<FaqObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetFaqs()
        {
            try
            {
                var faqs = new FaqServices().GetFaqs();
                return Json(faqs, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<FaqObject>(), JsonRequestBehavior.AllowGet);
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
        public ActionResult AddFaq(FaqObject faq)
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

                var validationResult = ValidateFaq(faq);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                faq.LastUpdated = DateTime.Now;
                var appStatus = new FaqServices().AddFaq(faq);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "FAQ could not be added. Please try again." : "The FAQ Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "FAQ was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "FAQ processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditFaq(FaqObject faq)
        {
            var gVal = new GenericValidator();

            try
            {
                var stat = ValidateFaq(faq);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_faq"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldfaq = Session["_faq"] as FaqObject;

                if (oldfaq == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldfaq.CategoryId = faq.CategoryId;
                oldfaq.Question = faq.Question;
                oldfaq.Answer = faq.Answer;
                var docStatus = new FaqServices().UpdateFaq(oldfaq);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "FAQ already exists." : "FAQ information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldfaq.Id;
                gVal.Error = "FAQ information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "FAQ information could not be updated. Please try again later";
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

        public ActionResult GetFaq(long id)
        {
            try
            {
                var faq = new FaqServices().GetFaq(id);
                if (faq == null || faq.Id < 1)
                {
                    return Json(new FaqObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_faq"] = faq;

                return Json(faq, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new FaqObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetFaqCategories()
        {
            try
            {
                return Json(new FaqCategoryServices().GetFaqCategories(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<FaqCategoryObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteFaq(long id)
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
                var delStatus = new FaqServices().DeleteFaq(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "FAQ could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "FAQ Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<FaqObject> GetFaqs(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new FaqServices().GetFaqs(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<FaqObject>();
            }
        }


        private GenericValidator ValidateFaq(FaqObject faq)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(faq.Question))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide FAQ.";
                    return gVal;
                }

                if (string.IsNullOrEmpty(faq.Answer))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide FAQ Answer.";
                    return gVal;
                }

                if (faq.CategoryId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select FAQ Category.";
                    return gVal;
                }
                
                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "FAQ Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
