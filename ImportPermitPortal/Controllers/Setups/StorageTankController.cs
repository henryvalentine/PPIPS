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
    public class StorageTankController : Controller
    {
        [HttpGet]
        public ActionResult GetStorageTankObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<StorageTankObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetStorageTanks(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new StorageTankServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<StorageTankObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<StorageTankObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.TankNo : c.Capacity.ToString());

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.TankNo, c.Capacity.ToString(),c.DepotName,c.ProductName,c.Measurement};
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
                return Json(new List<StorageTankObject>(), JsonRequestBehavior.AllowGet);
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
        public ActionResult AddStorageTank(StorageTankObject storageTank)
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

                var validationResult = ValidateStorageTank(storageTank);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new StorageTankServices().AddStorageTank(storageTank);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "StorageTank upload failed. Please try again." : "The StorageTank Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "StorageTank was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "StorageTank storageTanking failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditStorageTank(StorageTankObject storageTank)
        {
            var gVal = new GenericValidator();

            try
            {
              

                if (string.IsNullOrEmpty(storageTank.TankNo.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide StorageTank.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_StorageTank"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldStorageTank = Session["_StorageTank"] as StorageTankObject;

                if (oldStorageTank == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldStorageTank.TankNo = storageTank.TankNo;
                oldStorageTank.TankNo = storageTank.TankNo.Trim();

                var docStatus = new StorageTankServices().UpdateStorageTank(oldStorageTank);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "StorageTank information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldStorageTank.Id;
                gVal.Error = "StorageTank information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "StorageTank information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<StorageTankObject> GetStorageTanks(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new StorageTankServices().GetStorageTanks(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<StorageTankObject>();
            }
        }

        public ActionResult GetStorageTank(long id)
        {
            try
            {


                var storageTank = new StorageTankServices().GetStorageTank(id);
                if (storageTank == null || storageTank.Id < 1)
                {
                    return Json(new StorageTankObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_StorageTank"] = storageTank;

                return Json(storageTank, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new StorageTankObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteStorageTank(long id)
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
                var delStatus = new StorageTankServices().DeleteStorageTank(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "StorageTank could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "StorageTank Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator ValidateStorageTank(StorageTankObject storageTank)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(storageTank.TankNo))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select provide StorageTank.";
                    return gVal;
                }


                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "StorageTank Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericStorageTankList()
                {
                   
                  Products = getProductTypes(),
                  Depots = getDepotObjects(),
                  Measurements = getMeasurements()
                };
                

                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }


        private List<ProductObject> getProductTypes()
        {

            try
            {
                return new ProductServices().GetProducts();

            }
            catch (Exception)
            {
                return new List<ProductObject>();
            }
        }


        private List<DepotObject> getDepotObjects()
        {

            try
            {
                return new DepotServices().GetDepots();

            }
            catch (Exception)
            {
                return new List<DepotObject>();
            }
        }

        
        private List<UnitOfMeasurementObject> getMeasurements()
        {
            try
            {
                return new UnitOfMeasurementServices().GetUnitOfMeasurements();

            }
            catch (Exception)
            {
                return new List<UnitOfMeasurementObject>();
            }
        }



    }
}
