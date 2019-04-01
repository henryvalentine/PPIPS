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
   public class FeeTypeManager
    {

       public long AddFeeType(FeeTypeObject feeType)
       {
           try
           {
               if (feeType == null)
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   if (db.FeeTypes.Count(g => g.Name.ToLower().Trim().Replace(" ", "") == feeType.Name.ToLower().Trim().Replace(" ", "")) > 0)
                   {
                       return -3;
                   }
                   var feeTypeEntity = ModelMapper.Map<FeeTypeObject, FeeType>(feeType);
                   if (feeTypeEntity == null || string.IsNullOrEmpty(feeTypeEntity.Name))
                   {
                       return -2;
                   }
                   var returnStatus = db.FeeTypes.Add(feeTypeEntity);
                   db.SaveChanges();
                   return returnStatus.FeeTypeId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateFeeType(FeeTypeObject feeType)
       {
           try
           {
               if (feeType == null)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   if (db.FeeTypes.Count(g => g.Name.ToLower().Trim().Replace(" ", "") == feeType.Name.ToLower().Trim().Replace(" ", "") && g.FeeTypeId != feeType.FeeTypeId) > 0)
                   {
                       return -3;
                   }
                   var feeTypeEntity = ModelMapper.Map<FeeTypeObject, FeeType>(feeType);
                   if (feeTypeEntity == null || feeTypeEntity.FeeTypeId < 1)
                   {
                       return -2;
                   }
                   db.FeeTypes.Attach(feeTypeEntity);
                   db.Entry(feeTypeEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return feeType.FeeTypeId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<FeeTypeObject> GetFeeTypes()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var feeTypes = db.FeeTypes.ToList();
                   if (!feeTypes.Any())
                   {
                        return new List<FeeTypeObject>();
                   }
                   var objList =  new List<FeeTypeObject>();
                   feeTypes.ForEach(app =>
                   {
                       var feeTypeObject = ModelMapper.Map<FeeType, FeeTypeObject>(app);
                       if (feeTypeObject != null && feeTypeObject.FeeTypeId > 0)
                       {
                           objList.Add(feeTypeObject);
                       }
                   });

                   return !objList.Any() ? new List<FeeTypeObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

        public List<FeeTypeObject> GetFeeTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var feeTypes =
                           db.FeeTypes.OrderByDescending(m => m.FeeTypeId)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (feeTypes.Any())
                       {
                           var newList = new List<FeeTypeObject>();
                           feeTypes.ForEach(app =>
                           {
                               var feeTypeObject = ModelMapper.Map<FeeType, FeeTypeObject>(app);
                               if (feeTypeObject != null && feeTypeObject.FeeTypeId > 0)
                               {
                                   newList.Add(feeTypeObject);
                               }
                           });
                           countG = db.FeeTypes.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<FeeTypeObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<FeeTypeObject>();
           }
       }

        
       public FeeTypeObject GetFeeType(long feeTypeId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var feeTypes =
                        db.FeeTypes.Where(m => m.FeeTypeId == feeTypeId)
                            .ToList();
                    if (!feeTypes.Any())
                    {
                        return new FeeTypeObject();
                    }

                    var app = feeTypes[0];
                    var feeTypeObject = ModelMapper.Map<FeeType, FeeTypeObject>(app);
                    if (feeTypeObject == null || feeTypeObject.FeeTypeId < 1)
                    {
                        return new FeeTypeObject();
                    }
                    
                  return feeTypeObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new FeeTypeObject();
           }
       }

       public UserProfileObject GetUser(string id)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var users =
                       db.AspNetUsers.Where(m => m.Id == id).Include("UserProfile")
                           .ToList();
                   if (!users.Any())
                   {
                       return new UserProfileObject();
                   }

                   var appUser = users[0].UserProfile;
                   var userObject = ModelMapper.Map<UserProfile, UserProfileObject>(appUser);
                   if (userObject == null || userObject.Id < 1)
                   {
                       return new UserProfileObject();
                   }

                   return userObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new UserProfileObject();
           }
       }

       public List<FeeTypeObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var feeTypes =
                       db.FeeTypes.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (feeTypes.Any())
                   {
                       var newList = new List<FeeTypeObject>();
                       feeTypes.ForEach(app =>
                       {
                           var feeTypeObject = ModelMapper.Map<FeeType, FeeTypeObject>(app);
                           if (feeTypeObject != null && feeTypeObject.FeeTypeId > 0)
                           {
                               newList.Add(feeTypeObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<FeeTypeObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<FeeTypeObject>();
           }
       }

       public long DeleteFeeType(long feeTypeId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.FeeTypes.Where(m => m.FeeTypeId == feeTypeId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.FeeTypes.Remove(item);
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
