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
    public class FeeController : Controller
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
       public ActionResult GetFeeObjects(JQueryDataTableParamModel param)
       {
           try
           {
               IEnumerable<FeeObject> filteredParentMenuObjects;
               var countG = 0;

               var pagedParentMenuObjects = GetFees(param.iDisplayLength, param.iDisplayStart, out countG);

               if (!string.IsNullOrEmpty(param.sSearch))
               {
                   filteredParentMenuObjects = new FeeServices().Search(param.sSearch);
               }
               else
               {
                   filteredParentMenuObjects = pagedParentMenuObjects;
               }

               if (!filteredParentMenuObjects.Any())
               {
                   return Json(new List<FeeObject>(), JsonRequestBehavior.AllowGet);
               }

               var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
               Func<FeeObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.FeeTypeName : c.ImportStageName);

               var sortDirection = Request["sSortDir_0"]; // asc or desc
               filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

               var displayedPersonnels = filteredParentMenuObjects;

               var result = from c in displayedPersonnels
                            select new[] { Convert.ToString(c.FeeId), c.FeeTypeName, c.ImportStageName, c.Amount.ToString("N2") };
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
               return Json(new List<FeeObject>(), JsonRequestBehavior.AllowGet);
           }
       }
        public ActionResult AddFee(FeeObject fee)
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

                var validationResult = ValidateFee(fee);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new FeeServices().AddFee(fee);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Fee could not be added. Please try again." : "The Fee Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Fee was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Fee processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditFee(FeeObject fee)
        {
            var gVal = new GenericValidator();

            try
            {
                var stat = ValidateFee(fee);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_fee"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldfee = Session["_fee"] as FeeObject;

                if (oldfee == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldfee.ImportStageId = fee.ImportStageId;
                oldfee.FeeTypeId = fee.FeeTypeId;
                oldfee.Amount = fee.Amount;
                oldfee.Name = fee.Name;

                oldfee.PrincipalSplit = fee.PrincipalSplit;
                oldfee.VendorSplit = fee.VendorSplit;
                oldfee.PaymentGatewaySplit = fee.PaymentGatewaySplit;
                oldfee.BillableToPrincipal = fee.BillableToPrincipal;

                oldfee.CurrencyCode = fee.CurrencyCode;
                var docStatus = new FeeServices().UpdateFee(oldfee);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Fee already exists." : "Fee information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldfee.FeeId;
                gVal.Error = "Fee information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Fee information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
       
        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericFeeList
                {
                    ImportStages = GetImportStages(),
                    FeeTypes = GetFeeTypes()
                };
                 
                return Json(newList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetFee(long id)
        {
            try
            {


                var fee = new FeeServices().GetFee(id);
                if (fee == null || fee.FeeId < 1)
                {
                    return Json(new FeeObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_fee"] = fee;

                return Json(fee, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new FeeObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteFee(long id)
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
                var delStatus = new FeeServices().DeleteFee(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Fee could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Fee Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator ValidateFee(FeeObject fee)
        {
            var gVal = new GenericValidator();
            try
            {
                if (fee.FeeTypeId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select Fee.";
                    return gVal;
                }

                if (fee.ImportStageId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select Document Type.";
                    return gVal;
                }

                if (fee.Amount < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please fee Amount.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Fee Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<FeeObject> GetFees(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new FeeServices().GetFees(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<FeeObject>();
            }
        }

        private List<FeeTypeObject> GetFeeTypes()
        {
            try
            {
                return new FeeTypeServices().GetFeeTypes();

            }
            catch (Exception)
            {
                return new List<FeeTypeObject>();
            }
        }

        private List<ImportStageObject> GetImportStages()
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
