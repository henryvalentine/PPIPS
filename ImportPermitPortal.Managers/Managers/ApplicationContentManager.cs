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
    public class ApplicationContentManager
    {
       public long AddApplicationContent(ApplicationContentObject applicationContent)
       {
           try
           {
               if (applicationContent == null)
               {
                   return -2;
               }

               var applicationContentEntity = ModelMapper.Map<ApplicationContentObject, ApplicationContent>(applicationContent);
               if (applicationContentEntity == null || string.IsNullOrEmpty(applicationContentEntity.Title))
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.ApplicationContents.Add(applicationContentEntity);
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

       public long UpdateApplicationContent(ApplicationContentObject applicationContent)
       {
           try
           {
               if (applicationContent == null)
               {
                   return -2;
               }

               var applicationContentEntity = ModelMapper.Map<ApplicationContentObject, ApplicationContent>(applicationContent);
               if (applicationContentEntity == null || applicationContentEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.ApplicationContents.Attach(applicationContentEntity);
                   db.Entry(applicationContentEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return applicationContent.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       
        public List<ApplicationContentObject> GetApplicationContents(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var applicationContents =
                           db.ApplicationContents.OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (applicationContents.Any())
                       {
                           var newList = new List<ApplicationContentObject>();
                           applicationContents.ForEach(app =>
                           {
                               var applicationContentObject = ModelMapper.Map<ApplicationContent, ApplicationContentObject>(app);
                               if (applicationContentObject != null && applicationContentObject.Id > 0)
                               {
                                   applicationContentObject.IsInUseStr = applicationContentObject.IsInUse ? "Actively In Use" : "Disabled";
                                   newList.Add(applicationContentObject);
                               }
                           });
                           countG = db.ApplicationContents.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<ApplicationContentObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationContentObject>();
           }
       }

        public List<ApplicationContentObject> GetApplicationContents()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var applicationContents = db.ApplicationContents.Where(c => c.IsInUse).ToList();
                    if (applicationContents.Any())
                    {
                        var newList = new List<ApplicationContentObject>();
                        applicationContents.ForEach(app =>
                        {
                            var applicationContentObject = ModelMapper.Map<ApplicationContent, ApplicationContentObject>(app);
                            if (applicationContentObject != null && applicationContentObject.Id > 0)
                            {
                                newList.Add(applicationContentObject);
                            }
                        });
                       
                        return newList;
                    }
                    return new List<ApplicationContentObject>();
                }
            }
            catch (Exception ex)
            {
                return new List<ApplicationContentObject>();
            }
        }
       public ApplicationContentObject GetApplicationContent(long applicationContentId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var applicationContents =
                        db.ApplicationContents.Where(m => m.Id == applicationContentId)
                            .ToList();
                    if (!applicationContents.Any())
                    {
                        return new ApplicationContentObject();
                    }

                    var app = applicationContents[0];
                    var applicationContentObject = ModelMapper.Map<ApplicationContent, ApplicationContentObject>(app);
                    if (applicationContentObject == null || applicationContentObject.Id < 1)
                    {
                        return new ApplicationContentObject();
                    }
                    
                  return applicationContentObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ApplicationContentObject();
           }
       }
       public List<ApplicationContentObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var applicationContents =
                       db.ApplicationContents.Where(m => m.Title.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (applicationContents.Any())
                   {
                       var newList = new List<ApplicationContentObject>();
                       applicationContents.ForEach(app =>
                       {
                           var applicationContentObject = ModelMapper.Map<ApplicationContent, ApplicationContentObject>(app);
                           if (applicationContentObject != null && applicationContentObject.Id > 0)
                           {
                               applicationContentObject.IsInUseStr = applicationContentObject.IsInUse ? "Actively In Use" : "Disabled";
                               newList.Add(applicationContentObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<ApplicationContentObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationContentObject>();
           }
       }

       public long DeleteApplicationContent(long applicationContentId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.ApplicationContents.Where(m => m.Id == applicationContentId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.ApplicationContents.Remove(item);
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
