using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
   public class ProductColumnManager
    {

       public long AddProductColumn(ProductColumnObject productColumn)
       {
           try
           {
               if (productColumn == null)
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   if (db.ProductColumns.Count(k => k.CustomCodeId == productColumn.CustomCodeId && k.ProductId == productColumn.ProductId) > 0)
                   {
                       return -3;
                   }
                   var productColumnEntity = ModelMapper.Map<ProductColumnObject, ProductColumn>(productColumn);
                   if (productColumnEntity == null || productColumnEntity.ProductId < 1 || productColumnEntity.CustomCodeId < 1)
                   {
                       return -2;
                   }
                   var returnStatus = db.ProductColumns.Add(productColumnEntity);
                   db.SaveChanges();
                   return returnStatus.ProductColumnId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public long UpdateProductColumn(ProductColumnObject productColumn)
       {
           try
           {
               if (productColumn == null)
               {
                   return -2;
               }
               

               using (var db = new ImportPermitEntities())
               {
                   if (db.ProductColumns.Count(k => k.CustomCodeId == productColumn.CustomCodeId && k.ProductId == productColumn.ProductId && k.ProductColumnId != productColumn.ProductColumnId) > 0)
                   {
                       return -3;
                   }
                   var productColumnEntity = ModelMapper.Map<ProductColumnObject, ProductColumn>(productColumn);
                   if (productColumnEntity == null || productColumnEntity.ProductColumnId < 1)
                   {
                       return -2;
                   }
                   db.ProductColumns.Attach(productColumnEntity);
                   db.Entry(productColumnEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return productColumn.ProductColumnId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<ProductColumnObject> GetProductColumns()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var productColumns = db.ProductColumns
                       .Include("CustomCode")
                        .Include("Product")
                       .ToList();
                   if (!productColumns.Any())
                   {
                        return new List<ProductColumnObject>();
                   }
                   var objList =  new List<ProductColumnObject>();
                   productColumns.ForEach(app =>
                   {
                       var productColumnObject = ModelMapper.Map<ProductColumn, ProductColumnObject>(app);
                       if (productColumnObject != null && productColumnObject.ProductColumnId > 0)
                       {
                           productColumnObject.ProductName = app.Product.Name;
                           productColumnObject.CustomCodeName = app.CustomCode.Name;
                           objList.Add(productColumnObject);
                       }
                   });

                   return !objList.Any() ? new List<ProductColumnObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

       public List<ProductColumnObject> GetProductColumnsByProduct(int productId)
       {
           try  
           {
               using (var db = new ImportPermitEntities())
               {
                   var productColumns = db.ProductColumns
                       .Where(m => m.ProductId == productId)
                       .Include("CustomCode")
                        .Include("Product")
                       .ToList();
                   if (!productColumns.Any())
                   {
                       return new List<ProductColumnObject>();
                   }
                   var objList = new List<ProductColumnObject>();
                   productColumns.ForEach(app =>
                   {
                       var productColumnObject = ModelMapper.Map<ProductColumn, ProductColumnObject>(app);
                       if (productColumnObject != null && productColumnObject.ProductColumnId > 0)
                       {
                           productColumnObject.ProductName = app.Product.Name;
                           productColumnObject.CustomCodeName = app.CustomCode.Name;
                           objList.Add(productColumnObject);
                       }
                   });

                   return !objList.Any() ? new List<ProductColumnObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
       }
       public List<ProductColumnObject> GetProductColumnsByCustomCode(int customCodeId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var productColumns = db.ProductColumns
                       .Where(m => m.ProductColumnId == customCodeId)
                       .Include("CustomCode")
                       .Include("Product")
                       .ToList();

                   if (!productColumns.Any())
                   {
                       return new List<ProductColumnObject>();
                   }
                   var objList = new List<ProductColumnObject>();
                   productColumns.ForEach(app =>
                   {
                       var productColumnObject = ModelMapper.Map<ProductColumn, ProductColumnObject>(app);
                       if (productColumnObject != null && productColumnObject.ProductColumnId > 0)
                       {
                           productColumnObject.ProductName = app.Product.Name;
                           productColumnObject.CustomCodeName = app.CustomCode.Name;
                           objList.Add(productColumnObject);
                       }
                   });

                   return !objList.Any() ? new List<ProductColumnObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
       }
       public List<ProductColumnObject> GetProductColumns(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var productColumns =
                           db.ProductColumns
                           .Include("CustomCode")
                           .Include("Product")
                           .OrderByDescending(m => m.ProductColumnId)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (productColumns.Any())
                       {
                           var newList = new List<ProductColumnObject>();
                           productColumns.ForEach(app =>
                           {
                               var productColumnObject = ModelMapper.Map<ProductColumn, ProductColumnObject>(app);
                               if (productColumnObject != null && productColumnObject.ProductColumnId > 0)
                               {
                                   productColumnObject.ProductName = app.Product.Name;
                                   productColumnObject.CustomCodeName = app.CustomCode.Name;
                                   newList.Add(productColumnObject);
                               }
                           });
                           countG = db.ProductColumns.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<ProductColumnObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ProductColumnObject>();
           }
       }
       public ProductColumnObject GetProductColumn(long productColumnId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var productColumns =
                        db.ProductColumns.Where(m => m.ProductColumnId == productColumnId)
                        .Include("CustomCode")
                        .Include("Product")
                            .ToList();
                    if (!productColumns.Any())
                    {
                        return new ProductColumnObject();
                    }

                    var app = productColumns[0];
                    var productColumnObject = ModelMapper.Map<ProductColumn, ProductColumnObject>(app);
                    if (productColumnObject == null || productColumnObject.ProductColumnId < 1)
                    {
                        return new ProductColumnObject();
                    }
                    productColumnObject.ProductName = app.Product.Name;
                    productColumnObject.CustomCodeName = app.CustomCode.Name;
                  return productColumnObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ProductColumnObject();
           }
       }
       public long DeleteProductColumn(long productColumnId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.ProductColumns.Where(m => m.ProductColumnId == productColumnId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.ProductColumns.Remove(item);
                   db.SaveChanges();
                   return 5;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<ProductColumnObject> Search(string searchCriteria)
       {
           try
           {
                   using (var db = new ImportPermitEntities())
                   {
                      
                           
                       var  productColumns =   (from pc in db.ProductColumns 
                                                       .Include("Product")
                                                       join cc in 
                                                       db.CustomCodes
                                                       .Where(m => m.Name.ToLower().Contains(searchCriteria.ToLower().Trim()))
                                                       on pc.CustomCodeId equals cc.CustomCodeId
                                                     select new ProductColumnObject
                                                     {
                                                         ProductColumnId = pc.ProductColumnId,
                                                         ProductId = pc.ProductId,
                                                         CustomCodeId = pc.CustomCodeId,
                                                         ProductName = pc.Product.Name,
                                                         CustomCodeName = cc.Name
                                                     }).ToList();


                       var newProductColumns = (from pc in db.ProductColumns
                                                    .Include("CustomCode")
                                                     join pr in
                                                     db.Products
                                                     .Where(m => m.Name.ToLower().Contains(searchCriteria.ToLower().Trim()))
                                                    on pc.ProductId equals pr.ProductId
                                                    select new ProductColumnObject
                                                    {
                                                        ProductColumnId = pc.ProductColumnId,
                                                        ProductId = pc.ProductId,
                                                        CustomCodeId = pc.CustomCodeId,
                                                        ProductName = pr.Name,
                                                        CustomCodeName = pc.CustomCode.Name
                                                    }).ToList();
                      
                       if (newProductColumns.Any())
                       {
                           newProductColumns.ForEach(b =>
                           {
                               if (!productColumns.Exists(k => k.ProductColumnId == b.ProductColumnId))
                               {
                                   productColumns.Add(b);
                               }
                           });
                           
                       }
                       return !productColumns.Any() ? new List<ProductColumnObject>() : productColumns;
                   }
           }
           catch (Exception ex)
           {
               return new List<ProductColumnObject>();
           }
       }
    }
}
