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
   public class ProductManager
    {
       public long AddProduct(ProductObject product)
       {
           try
           {
               if (product == null)
               {
                   return -2;
               }

               var productEntity = ModelMapper.Map<ProductObject, Product>(product);
               if (productEntity == null || string.IsNullOrEmpty(productEntity.Name))
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   if (db.Products.Count(m => m.Code.ToLower() == product.Code.ToLower() && m.Name.ToLower() == product.Name.ToLower()) >0)
                   {
                       return -3;
                   }
                   var returnStatus = db.Products.Add(productEntity);
                   db.SaveChanges();

                   var requirements = new List<ProductDocumentRequirementObject>();
                   if (product.ProductDocumentRequirementObjects != null)
                   {
                       requirements = product.ProductDocumentRequirementObjects.ToList();
                   }

                   if (requirements.Any())
                   {
                        requirements.ToList().ForEach(k =>
                        {
                            if (db.ProductDocumentRequirements.Count(m => m.ProductId == product.ProductId && m.DocumentTypeId == k.DocumentTypeId) < 1)
                            {
                                k.ProductId = returnStatus.ProductId;
                                var productReqEntity = ModelMapper.Map<ProductDocumentRequirementObject, ProductDocumentRequirement>(k);
                                if (productReqEntity != null && productReqEntity.DocumentTypeId > 0 && productReqEntity.ProductId > 0)
                                {
                                    db.ProductDocumentRequirements.Add(productReqEntity);
                                    db.SaveChanges();
                                }
                            }
                        });
                       
                   }

                   return returnStatus.ProductId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateProduct(ProductObject product, List<ProductDocumentRequirementObject> newReqs)
       {
           try
           {
               if (product == null)
               {
                   return -2;
               }

               var productEntity = ModelMapper.Map<ProductObject, Product>(product);
               if (productEntity == null || productEntity.ProductId < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   if (db.Products.Count( m => m.Code.ToLower() == product.Code.ToLower() && m.Name.ToLower() == product.Name.ToLower() && m.ProductId != product.ProductId) > 0)
                   {
                       return -3;
                   }
                   db.Products.Attach(productEntity);
                   db.Entry(productEntity).State = EntityState.Modified;
                   db.SaveChanges();

                   var oldReqs = new List<ProductDocumentRequirementObject>();
                   if (product.ProductDocumentRequirementObjects != null)
                   {
                       oldReqs = product.ProductDocumentRequirementObjects.ToList();
                   }

                   var newRequirements = new List<ProductDocumentRequirementObject>();
                   if (newReqs != null)
                   {
                       newRequirements = newReqs;
                   }

                   if (newRequirements.Any())
                   {
                       if (oldReqs.Any())
                       {
                           newRequirements.ForEach(k =>
                           {
                               if (!oldReqs.Exists(n => n.DocumentTypeId == k.DocumentTypeId))
                               {
                                   if (
                                       db.ProductDocumentRequirements.Count(
                                           m => m.ProductId == product.ProductId && m.DocumentTypeId == k.DocumentTypeId) <
                                       1)
                                   {
                                       k.ProductId = productEntity.ProductId;
                                       var productReqEntity =
                                           ModelMapper.Map<ProductDocumentRequirementObject, ProductDocumentRequirement>(k);
                                       if (productReqEntity != null && productReqEntity.DocumentTypeId > 0 &&
                                           productReqEntity.ProductId > 0)
                                       {
                                           db.ProductDocumentRequirements.Add(productReqEntity);
                                           db.SaveChanges();
                                       }
                                   }
                               }
                           });
                       }

                       else
                       {
                           newRequirements.ForEach(k =>
                           {
                                if (db.ProductDocumentRequirements.Count(m => m.ProductId == product.ProductId && m.DocumentTypeId == k.DocumentTypeId) < 1)
                                {
                                    k.ProductId = productEntity.ProductId;
                                    var productReqEntity = ModelMapper.Map<ProductDocumentRequirementObject, ProductDocumentRequirement>(k);
                                    if (productReqEntity != null && productReqEntity.DocumentTypeId > 0 && productReqEntity.ProductId > 0)
                                    {
                                        db.ProductDocumentRequirements.Add(productReqEntity);
                                        db.SaveChanges();
                                    }
                                }
                               
                           });
                       }
                   }
                   else
                   {
                       if (oldReqs.Any())
                       {
                           oldReqs.ForEach(c =>
                           {
                               if (!newRequirements.Exists(n => n.DocumentTypeId == c.DocumentTypeId))
                               {
                                   var reqsToRemove =
                                       db.ProductDocumentRequirements.Where(
                                           m =>
                                               m.DocumentTypeId == c.DocumentTypeId &&
                                               m.ProductId == productEntity.ProductId).ToList();
                                   if (reqsToRemove.Any())
                                   {
                                       var item = reqsToRemove[0];
                                       db.ProductDocumentRequirements.Remove(item);
                                       db.SaveChanges();
                                   }
                               }
                           });
                       }
                   }
                   return product.ProductId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<ProductObject> GetProducts()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var products = db.Products.Where(p => p.Availability).OrderBy(m => m.Name).Include("ProductColumns").ToList();
                   if (!products.Any())
                   {
                        return new List<ProductObject>();
                   }
                   var objList =  new List<ProductObject>();
                   products.ForEach(app =>
                   {
                       var productObject = ModelMapper.Map<Product, ProductObject>(app);
                       if (productObject != null && productObject.ProductId > 0)
                       {
                           if (app.ProductColumns.Any())
                           {
                               if (app.ProductColumns.Any(v => v.CustomCodeId == (int)CustomColEnum.Psf))
                               {
                                   productObject.RequiresPsf = true; 
                               }
                               if (app.ProductColumns.Any(v => v.CustomCodeId == (int)CustomColEnum.Reference_Code))
                               {
                                   productObject.RequireReferenceCode = true;
                               }
                           }
                           productObject.Name = !string.IsNullOrEmpty(productObject.Code)? productObject.Name + " -- " + productObject.Code : productObject.Name;
                           productObject.AvailableStr =  productObject.Availability ? "Available" : "Unavailable";
                           objList.Add(productObject);
                       }
                   });

                   return !objList.Any() ? new List<ProductObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

       public int VerifyCode(LicenseRefObject verifier)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   if (verifier.LicenseType == (int) StorageProviderTypeEnum.Own_Depot)
                   {
                       var depotList = db.Depots.Where(p => p.ImporterId == verifier.ImporterId && p.DepotLicense == verifier.RefCode).ToList();
                       if (!depotList.Any())
                       {
                           return 0;
                       }
                       return depotList[0].Id;
                   }

                   var licenses = db.ReferenceLicenses.Where(p => p.ImporterId == verifier.ImporterId && p.ReferenceLicenseTypeId == verifier.LicenseType && p.LicenceCode == verifier.RefCode).ToList();
                   if (!licenses.Any())
                   {
                       return 0;
                   }
                   return 5;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public int VerifyDepotLicenseCode(LicenseRefObject verifier)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var depotList = db.Depots.Where(p => p.ImporterId == verifier.ImporterId && p.DepotLicense == verifier.RefCode).ToList();
                   if (!depotList.Any())
                   {
                       depotList = db.Depots.Where(p => p.DepotLicense.Trim() == verifier.RefCode.Trim()).ToList();
                       if (!depotList.Any())
                       {
                           return 0;
                       }

                       var depot = depotList[0];
                       if (depot.ImporterId == null)
                       {
                           depot.ImporterId = verifier.ImporterId;
                           db.Entry(depot).State = EntityState.Modified;
                           db.SaveChanges();
                           return depot.Id;
                       }
                       return depot.Id;
                   }

                   return depotList[0].Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long VerifyVesselLicense(LicenseRefObject verifier)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                    var licenseList = db.Vessels.Where(p => p.VesselLicense == verifier.RefCode).ToList();
                    if (!licenseList.Any())
                    {
                        return 0;
                    }
                    return licenseList[0].VesselId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

        public List<ProductObject> GetProducts(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var products =
                           db.Products
                           .OrderByDescending(m => m.ProductId)
                               .Skip(tpageNumber)
                               .Take(tsize).Include("ProductDocumentRequirements")
                               .ToList();
                       if (products.Any())
                       {
                           var newList = new List<ProductObject>();
                           products.ForEach(app =>
                           {
                               var productObject = ModelMapper.Map<Product, ProductObject>(app);
                               if (productObject != null && productObject.ProductId > 0)
                               {
                                   if (app.ProductDocumentRequirements.Any())
                                   {
                                       app.ProductDocumentRequirements.ToList().ForEach(b =>
                                       {
                                           var reqs = db.DocumentTypes.Where(u => u.DocumentTypeId == b.DocumentTypeId).ToList();
                                           if (reqs.Any())
                                           {
                                               var req = reqs[0];
                                               if (string.IsNullOrEmpty(productObject.Requirements))
                                               {
                                                   productObject.Requirements = req.Name;
                                               }
                                               else
                                               {
                                                   productObject.Requirements += ", " + req.Name;
                                               }
                                           }
                                       });
                                       
                                   }
                                   productObject.AvailableStr =  productObject.Availability ? "Available" : "Unavailable";
                                   newList.Add(productObject);
                               }

                           });
                           countG = db.Products.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<ProductObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ProductObject>();
           }
       }
       
       public ProductObject GetProduct(long productId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var products =
                        db.Products.Where(m => m.ProductId == productId).Include("ProductDocumentRequirements")
                            .ToList();
                    if (!products.Any())
                    {
                        return new ProductObject();
                    }

                    var app = products[0];
                    var productObject = ModelMapper.Map<Product, ProductObject>(app);
                    if (productObject == null || productObject.ProductId < 1)
                    {
                        return new ProductObject();
                    }
                    if (app.ProductDocumentRequirements.Any())
                    {
                       productObject.ProductDocumentRequirementObjects = new List<ProductDocumentRequirementObject>();
                        app.ProductDocumentRequirements.ToList().ForEach(b =>
                        {
                            var reqs = db.DocumentTypes.Where(u => u.DocumentTypeId == b.DocumentTypeId).ToList();
                            if (reqs.Any())
                            {
                                var req = reqs[0];
                                productObject.ProductDocumentRequirementObjects.Add(new ProductDocumentRequirementObject
                                {
                                    DocumentTypeName = req.Name,
                                    DocumentTypeId = req.DocumentTypeId,
                                    ProductId = b.ProductId,
                                    Id = b.Id
                                });
                                
                            }
                        });
                    }
                    productObject.AvailableStr = productObject.Availability ? "Available" : "Unavailable";
                  return productObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ProductObject();
           }
       }

       public List<ProductObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var products =
                       db.Products.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim() || m.Code.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .Include("ProductDocumentRequirements")
                       .ToList();

                   if (products.Any())
                   {
                       var newList = new List<ProductObject>();
                       products.ForEach(app =>
                       {
                           var productObject = ModelMapper.Map<Product, ProductObject>(app);
                           if (productObject != null && productObject.ProductId > 0)
                           {
                               if (app.ProductDocumentRequirements.Any())
                               {
                                   app.ProductDocumentRequirements.ToList().ForEach(b =>
                                   {
                                       var reqs = db.DocumentTypes.Where(u => u.DocumentTypeId == b.DocumentTypeId).ToList();
                                       if (reqs.Any())
                                       {
                                           var req = reqs[0];
                                           if (string.IsNullOrEmpty(productObject.Requirements))
                                           {
                                               productObject.Requirements = req.Name;
                                           }
                                           else
                                           {
                                               productObject.Requirements += ", " + req.Name;
                                           }
                                       }
                                   });

                               }

                               productObject.AvailableStr =  productObject.Availability ? "Available" : "Unavailable";
                               newList.Add(productObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<ProductObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ProductObject>();
           }
       }

       public long DeleteProduct(long productId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems = db.Products.Where(m => m.ProductId == productId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.Products.Remove(item);
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
