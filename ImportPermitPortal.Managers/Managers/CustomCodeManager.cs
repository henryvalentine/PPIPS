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
   public class CustomCodeManager
    {
       public long AddCustomCode(CustomCodeObject customCode)
       {
           try
           {
               if (customCode == null)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   if (db.CustomCodes.Count(k => k.Name.ToLower().Trim() == customCode.Name.ToLower().Trim()) > 0)
                   {
                       return -3;
                   }
                   var customCodeEntity = ModelMapper.Map<CustomCodeObject, CustomCode>(customCode);

                   if (customCodeEntity == null || string.IsNullOrEmpty(customCodeEntity.Name))
                   {
                       return -2;
                   }
                   var returnStatus = db.CustomCodes.Add(customCodeEntity);
                   db.SaveChanges();
                   return returnStatus.CustomCodeId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateCustomCode(CustomCodeObject customCode)
       {
           try
           {
               if (customCode == null)
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   if (db.CustomCodes.Count(k => k.Name.ToLower().Trim() == customCode.Name.ToLower().Trim() && k.CustomCodeId == customCode.CustomCodeId) > 0)
                   {
                       return -3;
                   }
                   var customCodeEntity = ModelMapper.Map<CustomCodeObject, CustomCode>(customCode);
                   if (customCodeEntity == null || customCodeEntity.CustomCodeId < 1)
                   {
                       return -2;
                   }
                   db.CustomCodes.Attach(customCodeEntity);
                   db.Entry(customCodeEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return customCode.CustomCodeId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<CustomCodeObject> GetCustomCodes()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var customCodes = db.CustomCodes.ToList();
                   if (!customCodes.Any())
                   {
                        return new List<CustomCodeObject>();
                   }
                   var objList =  new List<CustomCodeObject>();
                   customCodes.ForEach(app =>
                   {
                       var customCodeObject = ModelMapper.Map<CustomCode, CustomCodeObject>(app);
                       if (customCodeObject != null && customCodeObject.CustomCodeId > 0)
                       {
                           objList.Add(customCodeObject);
                       }
                   });

                   return !objList.Any() ? new List<CustomCodeObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

        public List<CustomCodeObject> GetCustomCodes(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var customCodes =
                           db.CustomCodes.OrderByDescending(m => m.CustomCodeId)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (customCodes.Any())
                       {
                           var newList = new List<CustomCodeObject>();
                           customCodes.ForEach(app =>
                           {
                               var customCodeObject = ModelMapper.Map<CustomCode, CustomCodeObject>(app);
                               if (customCodeObject != null && customCodeObject.CustomCodeId > 0)
                               {
                                   newList.Add(customCodeObject);
                               }
                           });
                           countG = db.CustomCodes.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<CustomCodeObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<CustomCodeObject>();
           }
       }

        
       public CustomCodeObject GetCustomCode(long customCodeId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var customCodes =
                        db.CustomCodes.Where(m => m.CustomCodeId == customCodeId)
                            .ToList();
                    if (!customCodes.Any())
                    {
                        return new CustomCodeObject();
                    }

                    var app = customCodes[0];
                    var customCodeObject = ModelMapper.Map<CustomCode, CustomCodeObject>(app);
                    if (customCodeObject == null || customCodeObject.CustomCodeId < 1)
                    {
                        return new CustomCodeObject();
                    }
                    
                  return customCodeObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new CustomCodeObject();
           }
       }

       public List<CustomCodeObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var customCodes =
                       db.CustomCodes.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (customCodes.Any())
                   {
                       var newList = new List<CustomCodeObject>();
                       customCodes.ForEach(app =>
                       {
                           var customCodeObject = ModelMapper.Map<CustomCode, CustomCodeObject>(app);
                           if (customCodeObject != null && customCodeObject.CustomCodeId > 0)
                           {
                               newList.Add(customCodeObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<CustomCodeObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<CustomCodeObject>();
           }
       }

       public long DeleteCustomCode(long customCodeId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.CustomCodes.Where(m => m.CustomCodeId == customCodeId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.CustomCodes.Remove(item);
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
