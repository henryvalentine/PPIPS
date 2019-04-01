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
    public class StorageProviderTypeManager
    {
       public long AddStorageProviderType(StorageProviderTypeObject storageProviderType)
       {
           try
           {
               if (storageProviderType == null)
               {
                   return -2;
               }

               var storageProviderTypeEntity = ModelMapper.Map<StorageProviderTypeObject, StorageProviderType>(storageProviderType);
               if (storageProviderTypeEntity == null || string.IsNullOrEmpty(storageProviderTypeEntity.Name))
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   if (db.StorageProviderTypes.Count(m => m.Name.ToLower() == storageProviderType.Name.ToLower() && m.Name.ToLower() == storageProviderType.Name.ToLower()) >0)
                   {
                       return -3;
                   }
                   var returnStatus = db.StorageProviderTypes.Add(storageProviderTypeEntity);
                   db.SaveChanges();

                   var requirements = new List<StorageProviderRequirementObject>();
                   if (storageProviderType.StorageProviderRequirementObjects != null)
                   {
                       requirements = storageProviderType.StorageProviderRequirementObjects.ToList();
                   }

                   if (requirements.Any())
                   {
                        requirements.ToList().ForEach(k =>
                        {
                            if (db.StorageProviderRequirements.Count(m => m.StorageProviderTypeId == storageProviderType.Id && m.DocumentTypeId == k.DocumentTypeId) < 1)
                            {
                                k.StorageProviderTypeId = returnStatus.Id;
                                var storageProviderTypeReqEntity = ModelMapper.Map<StorageProviderRequirementObject, StorageProviderRequirement>(k);
                                if (storageProviderTypeReqEntity != null && storageProviderTypeReqEntity.DocumentTypeId > 0 && storageProviderTypeReqEntity.StorageProviderTypeId > 0)
                                {
                                    db.StorageProviderRequirements.Add(storageProviderTypeReqEntity);
                                    db.SaveChanges();
                                }
                            }
                        });
                       
                   }

                   return returnStatus.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateStorageProviderType(StorageProviderTypeObject storageProviderType, List<StorageProviderRequirementObject> newReqs)
       {
           try
           {
               if (storageProviderType == null)
               {
                   return -2;
               }

               var storageProviderTypeEntity = ModelMapper.Map<StorageProviderTypeObject, StorageProviderType>(storageProviderType);
               if (storageProviderTypeEntity == null || storageProviderTypeEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   if (db.StorageProviderTypes.Count( m => m.Name.ToLower() == storageProviderType.Name.ToLower() && m.Name.ToLower() == storageProviderType.Name.ToLower() && m.Id != storageProviderType.Id) > 0)
                   {
                       return -3;
                   }

                   db.StorageProviderTypes.Attach(storageProviderTypeEntity);
                   db.Entry(storageProviderTypeEntity).State = EntityState.Modified;
                   db.SaveChanges();

                   var oldReqs = new List<StorageProviderRequirementObject>();
                   if (storageProviderType.StorageProviderRequirementObjects != null)
                   {
                       oldReqs = storageProviderType.StorageProviderRequirementObjects.ToList();
                   }

                   var newRequirements = new List<StorageProviderRequirementObject>();
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
                                       db.StorageProviderRequirements.Count(
                                           m => m.StorageProviderTypeId == storageProviderType.Id && m.DocumentTypeId == k.DocumentTypeId) <
                                       1)
                                   {
                                       k.StorageProviderTypeId = storageProviderTypeEntity.Id;
                                       var storageProviderTypeReqEntity =
                                           ModelMapper.Map<StorageProviderRequirementObject, StorageProviderRequirement>(k);
                                       if (storageProviderTypeReqEntity != null && storageProviderTypeReqEntity.DocumentTypeId > 0 &&
                                           storageProviderTypeReqEntity.StorageProviderTypeId > 0)
                                       {
                                           db.StorageProviderRequirements.Add(storageProviderTypeReqEntity);
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
                                if (db.StorageProviderRequirements.Count(m => m.StorageProviderTypeId == storageProviderType.Id && m.DocumentTypeId == k.DocumentTypeId) < 1)
                                {
                                    k.StorageProviderTypeId = storageProviderTypeEntity.Id;
                                    var storageProviderTypeReqEntity = ModelMapper.Map<StorageProviderRequirementObject, StorageProviderRequirement>(k);
                                    if (storageProviderTypeReqEntity != null && storageProviderTypeReqEntity.DocumentTypeId > 0 && storageProviderTypeReqEntity.StorageProviderTypeId > 0)
                                    {
                                        db.StorageProviderRequirements.Add(storageProviderTypeReqEntity);
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
                                       db.StorageProviderRequirements.Where(
                                           m =>
                                               m.DocumentTypeId == c.DocumentTypeId &&
                                               m.StorageProviderTypeId == storageProviderTypeEntity.Id).ToList();
                                   if (reqsToRemove.Any())
                                   {
                                       var item = reqsToRemove[0];
                                       db.StorageProviderRequirements.Remove(item);
                                       db.SaveChanges();
                                   }
                               }
                           });
                       }
                   }
                   return storageProviderType.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public List<StorageProviderTypeObject> GetStorageProviderTypes()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var storageProviderTypes = db.StorageProviderTypes.OrderBy(m => m.Name).Include("StorageProviderRequirements").ToList();
                   if (!storageProviderTypes.Any())
                   {
                        return new List<StorageProviderTypeObject>();
                   }
                   var objList =  new List<StorageProviderTypeObject>();
                   storageProviderTypes.ForEach(app =>
                   {
                       var storageProviderTypeObject = ModelMapper.Map<StorageProviderType, StorageProviderTypeObject>(app);
                       if (storageProviderTypeObject != null && storageProviderTypeObject.Id > 0)
                       {
                           objList.Add(storageProviderTypeObject);
                       }
                   });

                   return !objList.Any() ? new List<StorageProviderTypeObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }
        
        public List<StorageProviderTypeObject> GetStorageProviderTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var storageProviderTypes =
                           db.StorageProviderTypes
                           .OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize).Include("StorageProviderRequirements")
                               .ToList();
                       if (storageProviderTypes.Any())
                       {
                           var newList = new List<StorageProviderTypeObject>();
                           storageProviderTypes.ForEach(app =>
                           {
                               var storageProviderTypeObject = ModelMapper.Map<StorageProviderType, StorageProviderTypeObject>(app);
                               if (storageProviderTypeObject != null && storageProviderTypeObject.Id > 0)
                               {
                                   if (app.StorageProviderRequirements.Any())
                                   {
                                       app.StorageProviderRequirements.ToList().ForEach(b =>
                                       {
                                           var reqs = db.DocumentTypes.Where(u => u.DocumentTypeId == b.DocumentTypeId).ToList();
                                           if (reqs.Any())
                                           {
                                               var req = reqs[0];
                                               if (string.IsNullOrEmpty(storageProviderTypeObject.Requirements))
                                               {
                                                   storageProviderTypeObject.Requirements = req.Name;
                                               }
                                               else
                                               {
                                                   storageProviderTypeObject.Requirements += ", " + req.Name;
                                               }
                                           }
                                       });
                                       
                                   }
                                   newList.Add(storageProviderTypeObject);
                               }

                           });
                           countG = db.StorageProviderTypes.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<StorageProviderTypeObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<StorageProviderTypeObject>();
           }
       }
       
       public StorageProviderTypeObject GetStorageProviderType(long storageProviderTypeId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var storageProviderTypes =
                        db.StorageProviderTypes.Where(m => m.Id == storageProviderTypeId).Include("StorageProviderRequirements")
                            .ToList();
                    if (!storageProviderTypes.Any())
                    {
                        return new StorageProviderTypeObject();
                    }

                    var app = storageProviderTypes[0];
                    var storageProviderTypeObject = ModelMapper.Map<StorageProviderType, StorageProviderTypeObject>(app);
                    if (storageProviderTypeObject == null || storageProviderTypeObject.Id < 1)
                    {
                        return new StorageProviderTypeObject();
                    }
                    if (app.StorageProviderRequirements.Any())
                    {
                       storageProviderTypeObject.StorageProviderRequirementObjects = new List<StorageProviderRequirementObject>();
                        app.StorageProviderRequirements.ToList().ForEach(b =>
                        {
                            var reqs = db.DocumentTypes.Where(u => u.DocumentTypeId == b.DocumentTypeId).ToList();
                            if (reqs.Any())
                            {
                                var req = reqs[0];
                                storageProviderTypeObject.StorageProviderRequirementObjects.Add(new StorageProviderRequirementObject
                                {
                                    DocumentTypeName = req.Name,
                                    DocumentTypeId = req.DocumentTypeId,
                                    StorageProviderTypeId = b.StorageProviderTypeId,
                                    Id = b.Id
                                });
                                
                            }
                        });
                    }
                    
                  return storageProviderTypeObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new StorageProviderTypeObject();
           }
       }

       public List<StorageProviderTypeObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var storageProviderTypes =
                       db.StorageProviderTypes.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim() || m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .Include("StorageProviderRequirements")
                       .ToList();

                   if (storageProviderTypes.Any())
                   {
                       var newList = new List<StorageProviderTypeObject>();
                       storageProviderTypes.ForEach(app =>
                       {
                           var storageProviderTypeObject = ModelMapper.Map<StorageProviderType, StorageProviderTypeObject>(app);
                           if (storageProviderTypeObject != null && storageProviderTypeObject.Id > 0)
                           {
                               if (app.StorageProviderRequirements.Any())
                               {
                                   app.StorageProviderRequirements.ToList().ForEach(b =>
                                   {
                                       var reqs = db.DocumentTypes.Where(u => u.DocumentTypeId == b.DocumentTypeId).ToList();
                                       if (reqs.Any())
                                       {
                                           var req = reqs[0];
                                           if (string.IsNullOrEmpty(storageProviderTypeObject.Requirements))
                                           {
                                               storageProviderTypeObject.Requirements = req.Name;
                                           }
                                           else
                                           {
                                               storageProviderTypeObject.Requirements += ", " + req.Name;
                                           }
                                       }
                                   });

                               }
                               newList.Add(storageProviderTypeObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<StorageProviderTypeObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<StorageProviderTypeObject>();
           }
       }

       public long DeleteStorageProviderType(long storageProviderTypeId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems = db.StorageProviderTypes.Where(m => m.Id == storageProviderTypeId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.StorageProviderTypes.Remove(item);
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
