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
   public class ApplicationItemManager
    {
       public long AddApplicationItem(ApplicationItemObject importItem)
       {
           try
           {
               if (importItem == null)
               {
                   return -2;
               }

               var importItemEntity = ModelMapper.Map<ApplicationItemObject, ApplicationItem>(importItem);
               if (importItemEntity == null || importItemEntity.ApplicationId < 1)
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.ApplicationItems.Add(importItemEntity);
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

       public long AddApplicationItems(List<ApplicationItemObject> importItems, long applicationId)
       {
           try
           {
               if (!importItems.Any())
               {
                   return -2;
               }

               var successCount = 0;
               using (var db = new ImportPermitEntities())
               {
                   importItems.ForEach(importItem =>
                   {
                       importItem.ApplicationId = applicationId;
                       var importItemEntity = ModelMapper.Map<ApplicationItemObject, ApplicationItem>(importItem);
                       if (importItemEntity != null && importItemEntity.ApplicationId > 0)
                       {
                           db.ApplicationItems.Add(importItemEntity);
                           db.SaveChanges();
                           successCount += 1;
                       }
                   });

                   return successCount;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateApplicationItems(List<ApplicationItemObject> importItems,List<ApplicationItemObject> oldImportItems, long applicationId)
       {
           try
           {
               if (!importItems.Any())
               {
                   return -2;
               }
               var successCount = 0;
               using (var db = new ImportPermitEntities())
               {
                   var existingItems = db.ApplicationItems.Where(v => v.ApplicationId == applicationId).ToList();
                   importItems.ForEach(importItem =>
                   {
                       var existingItem = existingItems.Find(o => o.ProductId == importItem.ProductId);
                       if (existingItem != null && existingItem.Id > 0)
                       {
                            existingItem.EstimatedQuantity = importItem.EstimatedQuantity;
                            existingItem.EstimatedValue = importItem.EstimatedValue;
                            existingItem.PSFNumber = importItem.PSFNumber;
                            existingItem.ReferenceLicenseCode = importItem.ReferenceLicenseCode;
                            db.Entry(existingItem).State = EntityState.Modified;
                            db.SaveChanges();
                            successCount += 1;
                       }

                       else
                       {
                           importItem.ApplicationId = applicationId;
                           var importItemEntity = ModelMapper.Map<ApplicationItemObject, ApplicationItem>(importItem);
                           if (importItemEntity != null && importItemEntity.ApplicationId > 0)
                           {
                               db.ApplicationItems.Add(importItemEntity);
                               db.SaveChanges();
                               successCount += 1;
                           }
                       }
                   });

                   oldImportItems.ForEach(importItem =>
                   {
                       if (!importItems.Exists(h => h.ProductId == importItem.ProductId))
                       {
                           var myItems = db.ApplicationItems.Where(m => m.ProductId == importItem.ProductId && m.ApplicationId == importItem.ApplicationId).ToList();
                           if (myItems.Any())
                           {
                               db.ApplicationItems.Remove(myItems[0]);
                               db.SaveChanges();
                           }
                       }
                   });
                   return successCount;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateApplicationItem(ApplicationItemObject importItem)
       {
           try
           {
               if (importItem == null)
               {
                   return -2;
               }

               var importItemEntity = ModelMapper.Map<ApplicationItemObject, ApplicationItem>(importItem);
               if (importItemEntity == null || importItemEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.ApplicationItems.Attach(importItemEntity);
                   db.Entry(importItemEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return importItem.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<ApplicationItemObject> GetApplicationItems()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications = db.ApplicationItems.Include("Product").ToList();
                   if (!myApplications.Any())
                   {
                        return new List<ApplicationItemObject>();
                   }
                   var objList =  new List<ApplicationItemObject>();
                   myApplications.ForEach(app =>
                   {
                       var importObject = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(app);
                       if (importObject != null && importObject.Id > 0)
                       {
                           objList.Add(importObject);
                       }
                   });

                   return !objList.Any() ? new List<ApplicationItemObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

        public List<ApplicationItemObject> GetApplicationItemsByApplication(int? itemsPerPage, int? pageNumber, out int countG, long importApplicationId)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var importItems =
                           db.ApplicationItems.Where(m => m.ApplicationId == importApplicationId)
                           .OrderByDescending(m => m.ProductId)
                               .Skip(tpageNumber).Take(tsize)
                               .Include("Product")
                               .ToList();
                       if (importItems.Any())
                       {
                           var newList = new List<ApplicationItemObject>();
                           importItems.ForEach(app =>
                           {
                               var importObject = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(app);
                               if (importObject != null && importObject.Id > 0)
                               {
                                   newList.Add(importObject);
                               }
                           });
                           countG = db.ApplicationItems.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<ApplicationItemObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationItemObject>();
           }
       }

        public List<ApplicationItemObject> GetApplicationItemsByApplication(long importApplicationId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var importItems =
                        db.ApplicationItems.Where(m => m.ApplicationId == importApplicationId)
                            .Include("Product")
                            .ToList();
                    if (!importItems.Any())
                    {
                        return new List<ApplicationItemObject>();  
                    }
                    var newList = new List<ApplicationItemObject>();
                    importItems.ForEach(app =>
                    {
                        var importObject = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(app);
                        if (importObject != null && importObject.Id > 0)
                        {
                            newList.Add(importObject);
                        }
                    });
                    return newList;
                }

            }
            catch (Exception ex)
            {
                return new List<ApplicationItemObject>();
            }
        }

       public ApplicationItemObject GetApplicationItem(long importItemId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var myApplications =
                        db.ApplicationItems.Where(m => m.Id == importItemId)
                            .Include("Product")
                            .ToList();
                    if (!myApplications.Any())
                    {
                        return new ApplicationItemObject();
                    }

                    var app = myApplications[0];
                    var importObject = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new ApplicationItemObject();
                    }
                    
                  return importObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ApplicationItemObject();
           }
       }

       public ApplicationItemObject GetPreviousDischargeInfo(long importApplicationId, long productId, out double tolerancePercentage)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var importItems = db.ApplicationItems.Where(d => d.ApplicationId == importApplicationId && d.ProductId == productId).ToList();
                   if (!importItems.Any())
                   {
                       tolerancePercentage = 0;
                       return new ApplicationItemObject();
                   }
                   var settings = db.ImportSettings.ToList();
                   if (!settings.Any())
                   {
                       tolerancePercentage = 0;
                       return new ApplicationItemObject();
                   }
                   var app = importItems[0];
                   var importObject = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(app);
                   if (importObject == null || importObject.Id < 1)
                   {
                       tolerancePercentage = 0;
                       return new ApplicationItemObject();
                   }

                   tolerancePercentage = settings[0].DischargeQuantityTolerance;
                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               tolerancePercentage = 0;
               return new ApplicationItemObject();
           }
       }

       public List<ApplicationItemObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.ApplicationItems.Where(m => m.Product.Name.ToLower().Trim().Contains(searchCriteria.ToLower().Trim()))
                        .Include("Product")
                        .ToList();

                   if (myApplications.Any())
                   {
                       var newList = new List<ApplicationItemObject>();
                       myApplications.ForEach(app =>
                       {
                           var importObject = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               newList.Add(importObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<ApplicationItemObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationItemObject>();
           }
       }

       public long DeleteApplicationItem(long importItemId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.ApplicationItems.Where(m => m.Id == importItemId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.ApplicationItems.Remove(item);
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

       public long DeleteApplicationItems(long applicationId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems = db.ApplicationItems.Where(m => m.ApplicationId == applicationId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }
                   myItems.ForEach(item =>
                   {
                       db.ApplicationItems.Remove(item);
                       db.SaveChanges();
                   });
                 
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
