using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
   public class ImportClassManager
    {

       public long AddImportClass(ImportClassObject importClass)
       {
           try
           {
               if (importClass == null)
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   if (db.ImportClasses.Count(g => g.Name.ToLower().Trim().Replace(" ", "") == importClass.Name.ToLower().Trim().Replace(" ", "")) > 0)
                   {
                       return -3;
                   }
                   var importClassEntity = ModelMapper.Map<ImportClassObject, ImportClass>(importClass);
                   if (importClassEntity == null || string.IsNullOrEmpty(importClassEntity.Name))
                   {
                       return -2;
                   }
                   var returnStatus = db.ImportClasses.Add(importClassEntity);
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

       public long UpdateImportClass(ImportClassObject importClass)
       {
           try
           {
               if (importClass == null)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   if (db.ImportClasses.Count(g => g.Name.ToLower().Trim().Replace(" ", "") == importClass.Name.ToLower().Trim().Replace(" ", "") && g.Id != importClass.Id) > 0)
                   {
                       return -3;
                   }
                   var importClassEntity = ModelMapper.Map<ImportClassObject, ImportClass>(importClass);
                   if (importClassEntity == null || importClassEntity.Id < 1)
                   {
                       return -2;
                   }
                   db.ImportClasses.Attach(importClassEntity);
                   db.Entry(importClassEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return importClass.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<ImportClassObject> GetImportClasses()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var importClasss = db.ImportClasses.OrderBy(m => m.Name).ToList();
                   if (!importClasss.Any())
                   {
                        return new List<ImportClassObject>();
                   }
                   var objList =  new List<ImportClassObject>();
                   importClasss.ForEach(app =>
                   {
                       var importClassObject = ModelMapper.Map<ImportClass, ImportClassObject>(app);
                       if (importClassObject != null && importClassObject.Id > 0)
                       {
                           objList.Add(importClassObject);
                       }
                   });

                   return !objList.Any() ? new List<ImportClassObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

        public List<ImportClassObject> GetImportClasses(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var importClasss =
                           db.ImportClasses.OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (importClasss.Any())
                       {
                           var newList = new List<ImportClassObject>();
                           importClasss.ForEach(app =>
                           {
                               var importClassObject = ModelMapper.Map<ImportClass, ImportClassObject>(app);
                               if (importClassObject != null && importClassObject.Id > 0)
                               {
                                   newList.Add(importClassObject);
                               }
                           });
                           countG = db.ImportClasses.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<ImportClassObject>();
           }
           catch (DbEntityValidationException ex)
           {
               countG = 0;
               return new List<ImportClassObject>();
           }
       }

        
       public ImportClassObject GetImportClass(long importClassId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var importClasss =
                        db.ImportClasses.Where(m => m.Id == importClassId)
                            .ToList();
                    if (!importClasss.Any())
                    {
                        return new ImportClassObject();
                    }

                    var app = importClasss[0];
                    var importClassObject = ModelMapper.Map<ImportClass, ImportClassObject>(app);
                    if (importClassObject == null || importClassObject.Id < 1)
                    {
                        return new ImportClassObject();
                    }
                    
                  return importClassObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ImportClassObject();
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

       public List<ImportClassObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var importClasss =
                       db.ImportClasses.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (importClasss.Any())
                   {
                       var newList = new List<ImportClassObject>();
                       importClasss.ForEach(app =>
                       {
                           var importClassObject = ModelMapper.Map<ImportClass, ImportClassObject>(app);
                           if (importClassObject != null && importClassObject.Id > 0)
                           {
                               newList.Add(importClassObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<ImportClassObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ImportClassObject>();
           }
       }

       public long DeleteImportClass(long importClassId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.ImportClasses.Where(m => m.Id == importClassId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.ImportClasses.Remove(item);
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
