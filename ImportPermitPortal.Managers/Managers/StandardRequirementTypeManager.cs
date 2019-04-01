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
    public class StandardRequirementTypeManager
    {
       public long AddStandardRequirementType(StandardRequirementTypeObject sReqType)
       {
           try
           {
               if (sReqType == null)
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   if (db.StandardRequirementTypes.Count(g =>g.Name.ToLower().Trim().Replace(" ", "") == sReqType.Name.ToLower().Trim().Replace(" ", "")) > 0)
                   {
                       return -3;
                   }
                   var sReqTypeEntity = ModelMapper.Map<StandardRequirementTypeObject, StandardRequirementType>(sReqType);
                   if (sReqTypeEntity == null || string.IsNullOrEmpty(sReqTypeEntity.Name))
                   {
                       return -2;
                   }
                   var returnStatus = db.StandardRequirementTypes.Add(sReqTypeEntity);
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

       public long UpdateStandardRequirementType(StandardRequirementTypeObject sReqType)
       {
           try
           {
               if (sReqType == null)
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   if (db.StandardRequirementTypes.Count(g => g.Name.ToLower().Trim().Replace(" ", "") == sReqType.Name.ToLower().Trim().Replace(" ", "") && g.Id != sReqType.Id) > 0)
                   {
                       return -3;
                   }
                   var sReqTypeEntity = ModelMapper.Map<StandardRequirementTypeObject, StandardRequirementType>(sReqType);
                   if (sReqTypeEntity == null || sReqTypeEntity.Id < 1)
                   {
                       return -2;
                   }
                   db.StandardRequirementTypes.Attach(sReqTypeEntity);
                   db.Entry(sReqTypeEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return sReqType.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<StandardRequirementTypeObject> GetStandardRequirementTypes()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var sReqTypes = db.StandardRequirementTypes.ToList();
                   if (!sReqTypes.Any())
                   {
                        return new List<StandardRequirementTypeObject>();
                   }
                   var objList =  new List<StandardRequirementTypeObject>();
                   sReqTypes.ForEach(app =>
                   {
                       var sReqTypeObject = ModelMapper.Map<StandardRequirementType, StandardRequirementTypeObject>(app);
                       if (sReqTypeObject != null && sReqTypeObject.Id > 0)
                       {
                           if (!objList.Exists(v => v.Id == sReqTypeObject.Id))
                           {
                               objList.Add(sReqTypeObject);
                           }
                       }
                   });

                   return !objList.Any() ? new List<StandardRequirementTypeObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

      
        public List<StandardRequirementTypeObject> GetStandardRequirementTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var sReqTypes =
                           db.StandardRequirementTypes.OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (sReqTypes.Any())
                       {
                           var newList = new List<StandardRequirementTypeObject>();
                           sReqTypes.ForEach(app =>
                           {
                               var sReqTypeObject = ModelMapper.Map<StandardRequirementType, StandardRequirementTypeObject>(app);
                               if (sReqTypeObject != null && sReqTypeObject.Id > 0)
                               {
                                   if (!newList.Exists(v => v.Id == sReqTypeObject.Id))
                                   {
                                       newList.Add(sReqTypeObject);
                                   }
                               }
                           });
                           countG = db.StandardRequirementTypes.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<StandardRequirementTypeObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<StandardRequirementTypeObject>();
           }
       }
       
       public StandardRequirementTypeObject GetStandardRequirementType(long sReqTypeId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var sReqTypes =
                        db.StandardRequirementTypes.Where(m => m.Id == sReqTypeId)
                            .ToList();
                    if (!sReqTypes.Any())
                    {
                        return new StandardRequirementTypeObject();
                    }

                    var app = sReqTypes[0];
                    var sReqTypeObject = ModelMapper.Map<StandardRequirementType, StandardRequirementTypeObject>(app);
                    if (sReqTypeObject == null || sReqTypeObject.Id < 1)
                    {
                        return new StandardRequirementTypeObject();
                    }
                    
                  return sReqTypeObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new StandardRequirementTypeObject();
           }
       }

       public List<StandardRequirementTypeObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var sReqTypes =
                       db.StandardRequirementTypes.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (sReqTypes.Any())
                   {
                       var newList = new List<StandardRequirementTypeObject>();
                       sReqTypes.ForEach(app =>
                       {
                           var sReqTypeObject = ModelMapper.Map<StandardRequirementType, StandardRequirementTypeObject>(app);
                           if (sReqTypeObject != null && sReqTypeObject.Id > 0)
                           {
                               if (!newList.Exists(v => v.Id == sReqTypeObject.Id))
                               {
                                   newList.Add(sReqTypeObject);
                               }
                           }
                       });

                       return newList;
                   }
               }
               return new List<StandardRequirementTypeObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<StandardRequirementTypeObject>();
           }
       }

       public long DeleteStandardRequirementType(long sReqTypeId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.StandardRequirementTypes.Where(m => m.Id == sReqTypeId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.StandardRequirementTypes.Remove(item);
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
