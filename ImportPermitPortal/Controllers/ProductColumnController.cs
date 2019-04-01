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
    public class ProductColumnController : Controller
    {
       [HttpGet]
       public ActionResult GetProductColumnObjects(JQueryDataTableParamModel param)
       {
           try
           {
               IEnumerable<ProductColumnObject> filteredParentMenuObjects;
               int countG;

               var pagedParentMenuObjects = GetProductColumns(param.iDisplayLength, param.iDisplayStart, out countG);

               if (!string.IsNullOrEmpty(param.sSearch))
               {
                   filteredParentMenuObjects = new ProductColumnServices().Search(param.sSearch);
               }
               else
               {
                   filteredParentMenuObjects = pagedParentMenuObjects;
               }

               if (!filteredParentMenuObjects.Any())
               {
                   return Json(new List<ProductColumnObject>(), JsonRequestBehavior.AllowGet);
               }

               var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
               Func<ProductColumnObject, string> orderingFunction = (c => sortColumnIndex == 1? c.ProductName : c.CustomCodeName);

               var sortDirection = Request["sSortDir_0"]; // asc or desc
               filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

               var displayedPersonnels = filteredParentMenuObjects;
               
               var result = from c in displayedPersonnels
                            select new[] { Convert.ToString(c.ProductColumnId),  c.ProductName , c.CustomCodeName };
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
               return Json(new List<ProductColumnObject>(), JsonRequestBehavior.AllowGet);
           }
       }
        public ActionResult AddProductColumn(ProductColumnObject productColumn)
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

                var validationResult = ValidateProductColumn(productColumn);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                var appStatus = new ProductColumnServices().AddProductColumn(productColumn);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Product Added Requirement could not be processed failed. Please try again." : "The Product Added Requirement already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Product Added Requirement was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Product Added Requirement processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditProductColumn(ProductColumnObject productColumn)
        {
            var gVal = new GenericValidator();

            try
            {
                if (!ModelState.IsValid)
                {
                    gVal.Code = -1;
                    gVal.Error = "Plese provide all required fields and try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var stat = ValidateProductColumn(productColumn);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_productColumn"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldproductColumn = Session["_productColumn"] as ProductColumnObject;

                if (oldproductColumn == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                oldproductColumn.CustomCodeId = productColumn.CustomCodeId;
                oldproductColumn.ProductId = productColumn.ProductId;
                var docStatus = new ProductColumnServices().UpdateProductColumn(oldproductColumn);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Product Added Requirement already exists." : "Product Added Requirement could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldproductColumn.ProductColumnId;
                gVal.Error = "Product Added Requirement information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Product Added Requirement could not be updated. Please try again later";
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

        public ActionResult GetProductColumn(long id)
        {
            try
            {


                var productColumn = new ProductColumnServices().GetProductColumn(id);
                if (productColumn == null || productColumn.ProductColumnId < 1)
                {
                    return Json(new ProductColumnObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_productColumn"] = productColumn;

                return Json(productColumn, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new ProductColumnObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new ProductColList
                {
                    CustomCodes = GetCustomCodes(),
                    ProductObjects = GetProducts()
                };
                
                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new ProductColList(), JsonRequestBehavior.AllowGet);
            }
        }

       
        [HttpPost]
        public ActionResult DeleteProductColumn(long id)
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
                var delStatus = new ProductColumnServices().DeleteProductColumn(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Product Added Requirement could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Product Added Requirement was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ProductColumnObject> GetProductColumns(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new ProductColumnServices().GetProductColumns(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<ProductColumnObject>();
            }
        }

        private List<CustomCodeObject> GetCustomCodes()
        {
            try
            {
                return new CustomCodeServices().GetCustomCodes();
            }
            catch (Exception)
            {
                return new List<CustomCodeObject>();
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

       private GenericValidator ValidateProductColumn(ProductColumnObject productColumn)
        {
            var gVal = new GenericValidator();
            try
            {
                if (productColumn.ProductId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a Product.";
                    return gVal;
                }

                if (productColumn.CustomCodeId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select Custom Code.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Product Added Requirement Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
