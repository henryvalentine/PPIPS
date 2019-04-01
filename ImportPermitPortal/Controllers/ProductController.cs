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
    public class ProductController : Controller
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
       public ActionResult GetProductObjects(JQueryDataTableParamModel param)
       {
           try
           {
               IEnumerable<ProductObject> filteredParentMenuObjects;
               int countG;

               var pagedParentMenuObjects = GetProducts(param.iDisplayLength, param.iDisplayStart, out countG);

               if (!string.IsNullOrEmpty(param.sSearch))
               {
                   filteredParentMenuObjects = new ProductServices().Search(param.sSearch);
               }
               else
               {
                   filteredParentMenuObjects = pagedParentMenuObjects;
               }

               if (!filteredParentMenuObjects.Any())
               {
                   return Json(new List<ProductObject>(), JsonRequestBehavior.AllowGet);
               }

               var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
               Func<ProductObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name :  sortColumnIndex == 2 ? c.Code  : c.AvailableStr);

               var sortDirection = Request["sSortDir_0"]; // asc or desc
               filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

               var displayedPersonnels = filteredParentMenuObjects;

               var result = from c in displayedPersonnels
                            select new[] { Convert.ToString(c.ProductId), c.Name, c.Code, c.AvailableStr, c.Requirements };
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
               return Json(new List<ProductObject>(), JsonRequestBehavior.AllowGet);
           }
       }
        public ActionResult AddProduct(ProductObject product)
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

                var validationResult = ValidateProduct(product);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new ProductServices().AddProduct(product);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Product upload failed. Please try again." : "The Product  Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Product  was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Product  processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditProduct(ProductObject product)
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

                var stat = ValidateProduct(product);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_product"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldproduct = Session["_product"] as ProductObject;

                if (oldproduct == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                oldproduct.Name = product.Name.Trim();
                oldproduct.Code = product.Code.Trim();
                oldproduct.Availability = product.Availability;
                var newReq = new List<ProductDocumentRequirementObject>();
                if (product.ProductDocumentRequirementObjects != null)
                {
                    newReq = product.ProductDocumentRequirementObjects.ToList();
                }
                var docStatus = new ProductServices().UpdateProduct(oldproduct, newReq);

                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Product  already exists." : "Product  information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldproduct.ProductId;
                gVal.Error = "Product  information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Product  information could not be updated. Please try again later";
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

        public ActionResult GetProduct(long id)
        {
            try
            {


                var product = new ProductServices().GetProduct(id);
                if (product == null || product.ProductId < 1)
                {
                    return Json(new ProductObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_product"] = product;

                return Json(product, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new ProductObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteProduct(long id)
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
                var delStatus = new ProductServices().DeleteProduct(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Product  could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Product  Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator ValidateProduct(ProductObject product)
        {
            var gVal = new GenericValidator();
            try
            {
                if (product.ProductId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a Product.";
                    return gVal;
                }

                if (string.IsNullOrEmpty(product.Code))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select Product Code.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Product  Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<ProductObject> GetProducts(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new ProductServices().GetProducts(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<ProductObject>();
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
