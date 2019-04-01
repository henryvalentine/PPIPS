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
    public class ProductDocumentRequirementController : Controller
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
       public ActionResult GetProductDocumentRequirementObjects(JQueryDataTableParamModel param)
       {
           try
           {
               IEnumerable<ProductDocumentRequirementObject> filteredParentMenuObjects;
               var countG = 0;

               var pagedParentMenuObjects = GetProductRequirements(param.iDisplayLength, param.iDisplayStart, out countG);

               if (!string.IsNullOrEmpty(param.sSearch))
               {
                   filteredParentMenuObjects = new ProductRequirementServices().Search(param.sSearch);
               }
               else
               {
                   filteredParentMenuObjects = pagedParentMenuObjects;
               }

               if (!filteredParentMenuObjects.Any())
               {
                   return Json(new List<ProductDocumentRequirementObject>(), JsonRequestBehavior.AllowGet);
               }

               var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
               Func<ProductDocumentRequirementObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ProductName : c.DocumentTypeName);

               var sortDirection = Request["sSortDir_0"]; // asc or desc
               filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

               var displayedPersonnels = filteredParentMenuObjects;

               var result = from c in displayedPersonnels
                            select new[] { Convert.ToString(c.Id), c.ProductName, c.DocumentTypeName };
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
               return Json(new List<ProductDocumentRequirementObject>(), JsonRequestBehavior.AllowGet);
           }
       }
        public ActionResult AddProductRequirement(ProductDocumentRequirementObject productRequirement)
        {
            var gVal = new GenericValidator();

            try
            {
                ////if (!ModelState.IsValid)
                ////{
                ////    gVal.Code = -1;
                ////    gVal.Error = "Plese provide all required fields and try again.";
                ////    return Json(gVal, JsonRequestBehavior.AllowGet);
                ////}

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var validationResult = ValidateProductRequirement(productRequirement);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new ProductRequirementServices().AddProductRequirement(productRequirement);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "ProductRequirement upload failed. Please try again." : "The Product Requirement Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Product Requirement was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Product Requirement processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditProductRequirement(ProductDocumentRequirementObject productRequirement)
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

                var stat = ValidateProductRequirement(productRequirement);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_productRequirement"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldproductRequirement = Session["_productRequirement"] as ProductDocumentRequirementObject;

                if (oldproductRequirement == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldproductRequirement.ProductId = productRequirement.ProductId;
                oldproductRequirement.DocumentTypeId = productRequirement.DocumentTypeId;
                var docStatus = new ProductRequirementServices().UpdateProductRequirement(oldproductRequirement);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Product Requirement already exists." : "Product Requirement information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldproductRequirement.Id;
                gVal.Error = "Product Requirement information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Product Requirement information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
       

        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericProductList
                {
                    DocumentTypes = GetDocumentTypes(),
                    Products = GetProducts()
                };

                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetProductRequirement(long id)
        {
            try
            {


                var productRequirement = new ProductRequirementServices().GetProductRequirement(id);
                if (productRequirement == null || productRequirement.Id < 1)
                {
                    return Json(new ProductDocumentRequirementObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_productRequirement"] = productRequirement;

                return Json(productRequirement, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new ProductDocumentRequirementObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteProductRequirement(long id)
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
                var delStatus = new ProductRequirementServices().DeleteProductRequirement(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Product Requirement could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Product Requirement Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetDocumentsReqsByImportStage(long id)
        {
            try
            {
                return Json(GetProductRequirementsByProduct(id), JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new ProductDocumentRequirementObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetImportEligibilityReqsByDocumentType(int id)
        {
            try
            {
                return Json(GetProductRequirementsByDocumentType(id), JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new ProductDocumentRequirementObject(), JsonRequestBehavior.AllowGet);
            }
        }

        private List<ProductDocumentRequirementObject> GetProductRequirementsByProduct(long productId)
        {
            try
            {
                return new ProductRequirementServices().GetProductRequirementsByProduct(productId);
                
            }
            catch (Exception)
            {
                return new List<ProductDocumentRequirementObject>();
            }
        }
        private List<ProductDocumentRequirementObject> GetProductRequirementsByDocumentType(int documentTypeId)
        {
            try
            {
                return new ProductRequirementServices().GetProductRequirementsByDocumentType(documentTypeId);
                
            }
            catch (Exception)
            {
                return new List<ProductDocumentRequirementObject>();
            }
        }
        private GenericValidator ValidateProductRequirement(ProductDocumentRequirementObject productRequirement)
        {
            var gVal = new GenericValidator();
            try
            {
                if (productRequirement.ProductId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a Product.";
                    return gVal;
                }

                if (productRequirement.DocumentTypeId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select Document Type.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Product Requirement Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<ProductDocumentRequirementObject> GetProductRequirements(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new ProductRequirementServices().GetProductRequirements(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<ProductDocumentRequirementObject>();
            }
        }

        private List<ProductObject> GetProducts()
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
