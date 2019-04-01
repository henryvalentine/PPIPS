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
   public class ProductDocumentRequirementManager
    {
       public long AddProductDocumentRequirement(ProductDocumentRequirementObject productRequirement)
       {
           try
           {
               if (productRequirement == null)
               {
                   return -2;
               }

               var productRequirementEntity = ModelMapper.Map<ProductDocumentRequirementObject, ProductDocumentRequirement>(productRequirement);

               if (productRequirementEntity == null || productRequirementEntity.ProductId < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.ProductDocumentRequirements.Add(productRequirementEntity);
                   db.SaveChanges();
                   return returnStatus.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateProductDocumentRequirement(ProductDocumentRequirementObject productRequirement)
       {
           try
           {
               if (productRequirement == null)
               {
                   return -2;
               }

               var productRequirementEntity = ModelMapper.Map<ProductDocumentRequirementObject, ProductDocumentRequirement>(productRequirement);
               if (productRequirementEntity == null || productRequirementEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.ProductDocumentRequirements.Attach(productRequirementEntity);
                   db.Entry(productRequirementEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return productRequirement.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public List<ProductDocumentRequirementObject> GetProductDocumentRequirementsByDocumentType(int documentTypeId)
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var productRequirements = db.ProductDocumentRequirements.Where(m => m.DocumentTypeId == documentTypeId).Include("DocumentType").Include("Product").ToList();
                   if (!productRequirements.Any())
                   {
                        return new List<ProductDocumentRequirementObject>();
                   }
                   var objList =  new List<ProductDocumentRequirementObject>();
                   productRequirements.ForEach(app =>
                   {
                       var productRequirementObject = ModelMapper.Map<ProductDocumentRequirement, ProductDocumentRequirementObject>(app);
                       if (productRequirementObject != null && productRequirementObject.Id > 0)
                       {
                           productRequirementObject.ProductName = app.Product.Name;
                           productRequirementObject.DocumentTypeName = app.DocumentType.Name;    
                           objList.Add(productRequirementObject);
                       }
                   });

                   return !objList.Any() ? new List<ProductDocumentRequirementObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }
        public List<ProductDocumentRequirementObject> GetProductDocumentRequirementsByProduct(long productId)
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var productRequirements = db.ProductDocumentRequirements.Where(m => m.ProductId == productId).Include("DocumentType").Include("Product").ToList();
                   if (!productRequirements.Any())
                   {
                        return new List<ProductDocumentRequirementObject>();
                   }
                   var objList =  new List<ProductDocumentRequirementObject>();
                   productRequirements.ForEach(app =>
                   {
                       var productRequirementObject = ModelMapper.Map<ProductDocumentRequirement, ProductDocumentRequirementObject>(app);
                       if (productRequirementObject != null && productRequirementObject.Id > 0)
                       {
                           productRequirementObject.ProductName = app.Product.Name;
                           productRequirementObject.DocumentTypeName = app.DocumentType.Name;
                           objList.Add(productRequirementObject);
                       }
                   });

                   return !objList.Any() ? new List<ProductDocumentRequirementObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

        public List<ProductDocumentRequirementObject> GetProductDocumentRequirementsByProducts(List<ApplicationItemObject> importItems)
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                  var objList = new List<ProductDocumentRequirementObject>();
                  importItems.ForEach(c =>
                  {
                      var productRequirements = db.ProductDocumentRequirements.Where(m => m.ProductId == c.ProductId).ToList();

                      if (productRequirements.Any())
                      {
                          productRequirements.ForEach(app =>
                          {
                              var productRequirementObject = ModelMapper.Map<ProductDocumentRequirement, ProductDocumentRequirementObject>(app);
                              if (productRequirementObject != null && productRequirementObject.Id > 0)
                              {
                                  productRequirementObject.ProductName = app.Product.Name;
                                  productRequirementObject.DocumentTypeName = app.DocumentType.Name;
                                  objList.Add(productRequirementObject);
                              }
                          }); 
                      }
                  });

                   return !objList.Any() ? new List<ProductDocumentRequirementObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

       public List<ProductDocumentRequirementObject> GetProductDocumentRequirements()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var productRequirements = db.ProductDocumentRequirements.Include("DocumentType").Include("Product").ToList();
                   if (!productRequirements.Any())
                   {
                        return new List<ProductDocumentRequirementObject>();
                   }
                   var objList =  new List<ProductDocumentRequirementObject>();
                   productRequirements.ForEach(app =>
                   {
                       var productRequirementObject = ModelMapper.Map<ProductDocumentRequirement, ProductDocumentRequirementObject>(app);
                       if (productRequirementObject != null && productRequirementObject.Id > 0)
                       {
                           productRequirementObject.ProductName = app.Product.Name;
                           productRequirementObject.DocumentTypeName = app.DocumentType.Name;
                           objList.Add(productRequirementObject);
                       }
                   });

                   return !objList.Any() ? new List<ProductDocumentRequirementObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

       public List<ProductDocumentRequirementObject> GetProductDocumentRequirements(long productId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var productRequirements = db.ProductDocumentRequirements.Where(m => m.ProductId == productId).Include("DocumentType")
                       .Include("Product").ToList();
                   if (!productRequirements.Any())
                   {
                       return new List<ProductDocumentRequirementObject>();
                   }
                   var objList = new List<ProductDocumentRequirementObject>();
                   productRequirements.ForEach(app =>
                   {
                       var productRequirementObject = ModelMapper.Map<ProductDocumentRequirement, ProductDocumentRequirementObject>(app);
                       if (productRequirementObject != null && productRequirementObject.Id > 0)
                       {
                           productRequirementObject.ProductName = app.Product.Name;
                           productRequirementObject.DocumentTypeName = app.DocumentType.Name;
                           objList.Add(productRequirementObject);
                       }
                   });

                   return !objList.Any() ? new List<ProductDocumentRequirementObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
       }
        public List<ProductDocumentRequirementObject> GetProductDocumentRequirements(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var productRequirements =
                           db.ProductDocumentRequirements
                           .OrderByDescending(m => m.Id)
                            .Include("DocumentType")
                            .Include("Product")
                            .Skip(tpageNumber)
                            .Take(tsize)
                            .ToList();
                       if (productRequirements.Any())
                       {
                           var newList = new List<ProductDocumentRequirementObject>();
                           productRequirements.ForEach(app =>
                           {
                               var productRequirementObject = ModelMapper.Map<ProductDocumentRequirement, ProductDocumentRequirementObject>(app);
                               if (productRequirementObject != null && productRequirementObject.Id > 0)
                               {
                                   var req = newList.Find(o => o.ProductId == app.ProductId);

                                   if (req != null && req.Id > 0)
                                   {
                                       req.DocumentTypeName += ", " + app.DocumentType.Name;
                                   }
                                   else
                                   {
                                       productRequirementObject.ProductName = app.Product.Name;
                                       productRequirementObject.DocumentTypeName = app.DocumentType.Name;
                                       newList.Add(productRequirementObject);
                                   }
                               }
                           });
                           countG = db.ProductDocumentRequirements.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<ProductDocumentRequirementObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ProductDocumentRequirementObject>();
           }
       }
       
       public ProductDocumentRequirementObject GetProductDocumentRequirement(long productRequirementId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var productRequirements =
                        db.ProductDocumentRequirements.Where(m => m.Id == productRequirementId)
                            .ToList();
                    if (!productRequirements.Any())
                    {
                        return new ProductDocumentRequirementObject();
                    }

                    var app = productRequirements[0];
                    var productRequirementObject = ModelMapper.Map<ProductDocumentRequirement, ProductDocumentRequirementObject>(app);
                    if (productRequirementObject == null || productRequirementObject.Id < 1)
                    {
                        return new ProductDocumentRequirementObject();
                    }
                    
                  return productRequirementObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ProductDocumentRequirementObject();
           }
       }

       public List<ProductDocumentRequirementObject> Search(string searchCriteria)
       {
           try
           {
                   using (var db = new ImportPermitEntities())
                   {
                       var productRequirements =
                           db.ProductDocumentRequirements
                           .Include("DocumentType")
                           .Include("Product")
                           .Where(m => m.DocumentType.Name.ToLower() == searchCriteria.ToLower().Trim()
                           || m.Product.Name.ToLower() == searchCriteria.ToLower().Trim())
                           .ToList();
                       if (!productRequirements.Any())
                       {
                           return new List<ProductDocumentRequirementObject>();
                       }
                       var newList = new List<ProductDocumentRequirementObject>();
                       productRequirements.ForEach(app =>
                       {
                           var productRequirementObject = ModelMapper.Map<ProductDocumentRequirement, ProductDocumentRequirementObject>(app);
                           if (productRequirementObject != null && productRequirementObject.DocumentTypeId > 0)
                           {
                               var req = newList.Find(o => o.ProductId == app.ProductId);

                               if (req != null && req.Id > 0)
                               {
                                   req.DocumentTypeName += ", " + app.DocumentType.Name;
                               }
                               else
                               {
                                   productRequirementObject.ProductName = app.Product.Name;
                                   productRequirementObject.DocumentTypeName = app.DocumentType.Name;
                                   newList.Add(productRequirementObject);
                               }
                           }
                       });
                       return newList;
                   }
           }
           catch (Exception ex)
           {
               return new List<ProductDocumentRequirementObject>();
           }
       }
       public long DeleteProductDocumentRequirement(long productRequirementId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.ProductDocumentRequirements.Where(m => m.Id == productRequirementId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.ProductDocumentRequirements.Remove(item);
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
    }
}
