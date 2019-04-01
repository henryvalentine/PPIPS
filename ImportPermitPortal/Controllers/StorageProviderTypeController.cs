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
    public class StorageProviderTypeController : Controller
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
       public ActionResult GetStorageProviderTypeObjects(JQueryDataTableParamModel param)
       {
           try
           {
               IEnumerable<StorageProviderTypeObject> filteredParentMenuObjects;
               int countG;

               var pagedParentMenuObjects = GetStorageProviderTypes(param.iDisplayLength, param.iDisplayStart, out countG);

               if (!string.IsNullOrEmpty(param.sSearch))
               {
                   filteredParentMenuObjects = new StorageProviderTypeServices().Search(param.sSearch);
               }
               else
               {
                   filteredParentMenuObjects = pagedParentMenuObjects;
               }

               if (!filteredParentMenuObjects.Any())
               {
                   return Json(new List<StorageProviderTypeObject>(), JsonRequestBehavior.AllowGet);
               }
               
               Func<StorageProviderTypeObject, string> orderingFunction = (c => c.Name );

               var sortDirection = Request["sSortDir_0"]; // asc or desc
               filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

               var displayedPersonnels = filteredParentMenuObjects;

               var result = from c in displayedPersonnels
                            select new[] { Convert.ToString(c.Id), c.Name, c.Requirements };
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
               return Json(new List<StorageProviderTypeObject>(), JsonRequestBehavior.AllowGet);
           }
       }
        public ActionResult AddStorageProviderType(StorageProviderTypeObject storageProviderType)
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

                var validationResult = ValidateStorageProviderType(storageProviderType);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new StorageProviderTypeServices().AddStorageProviderType(storageProviderType);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "StorageProviderType upload failed. Please try again." : "The StorageProviderType  Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "StorageProviderType  was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "StorageProviderType  processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditStorageProviderType(StorageProviderTypeObject storageProviderType)
        {
            var gVal = new GenericValidator();

            try
            {
                var stat = ValidateStorageProviderType(storageProviderType);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_storageProviderType"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldstorageProviderType = Session["_storageProviderType"] as StorageProviderTypeObject;

                if (oldstorageProviderType == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                oldstorageProviderType.Name = storageProviderType.Name.Trim();
                var newReq = new List<StorageProviderRequirementObject>();
                if (storageProviderType.StorageProviderRequirementObjects != null)
                {
                    newReq = storageProviderType.StorageProviderRequirementObjects.ToList();
                }
                var docStatus = new StorageProviderTypeServices().UpdateStorageProviderType(oldstorageProviderType, newReq);

                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Storage Provider Type  already exists." : "Storage Provider Type  information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldstorageProviderType.Id;
                gVal.Error = "Storage Provider Type  information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Storage Provider Type  information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
       
        public ActionResult GetDocTypes()
        {
            try
            {
                return Json(GetDocumentTypes(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<DocumentTypeObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetStorageProviderType(long id)
        {
            try
            {


                var storageProviderType = new StorageProviderTypeServices().GetStorageProviderType(id);
                if (storageProviderType == null || storageProviderType.Id < 1)
                {
                    return Json(new StorageProviderTypeObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_storageProviderType"] = storageProviderType;

                return Json(storageProviderType, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new StorageProviderTypeObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteStorageProviderType(long id)
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
                var delStatus = new StorageProviderTypeServices().DeleteStorageProviderType(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "StorageProviderType  could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "StorageProviderType  Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator ValidateStorageProviderType(StorageProviderTypeObject storageProviderType)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(storageProviderType.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select Storage Provider Type.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Storage Provider Type  Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<StorageProviderTypeObject> GetStorageProviderTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new StorageProviderTypeServices().GetStorageProviderTypes(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<StorageProviderTypeObject>();
            }
        }

        private List<StorageProviderTypeObject> GetStorageProviderTypes()
        {
            try
            {
                return new StorageProviderTypeServices().GetStorageProviderTypes();

            }
            catch (Exception)
            {
                return new List<StorageProviderTypeObject>();
            }
        }

        private List<DocumentTypeObject> GetDocumentTypes()
        {
            try
            {
                return new DocumentTypeServices().GetDocumentTypes();
            }
            catch (Exception)
            {
                return new List<DocumentTypeObject>();
            }
        }

    }
}
