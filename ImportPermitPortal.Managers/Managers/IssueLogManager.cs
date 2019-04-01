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
    public class IssueLogManager
    {

        public long AddIssueLog(IssueLogObject issueLogObject)
       {
           try
           {
               if (issueLogObject == null || issueLogObject.IssueCategoryId < 1)
               {
                   return -2;
               }

               var issueLogEntity = ModelMapper.Map<IssueLogObject, IssueLog>(issueLogObject);
               if (issueLogEntity == null || issueLogEntity.IssueCategoryId < 1)
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   var processedLog = db.IssueLogs.Add(issueLogEntity);
                   db.SaveChanges();
                   return processedLog.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

        public long UpdateIssueLog(IssueLogObject issueLogObject)
       {
           try
           {
               if (issueLogObject == null)
               {
                   return -2;
               }

               var issueLogEntity = ModelMapper.Map<IssueLogObject, IssueLog>(issueLogObject);
               if (issueLogEntity == null || issueLogEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.IssueLogs.Attach(issueLogEntity);
                   db.Entry(issueLogEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return issueLogObject.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
        public List<IssueLogObject> GetIssueLogs()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var issueLogObjects = db.IssueLogs.ToList();
                   if (!issueLogObjects.Any())
                   {
                        return new List<IssueLogObject>();
                   }
                   var objList =  new List<IssueLogObject>();
                   issueLogObjects.ForEach(app =>
                   {
                       var issueLogObjectObject = ModelMapper.Map<IssueLog, IssueLogObject>(app);
                       if (issueLogObjectObject != null && issueLogObjectObject.Id > 0)
                       {
                           objList.Add(issueLogObjectObject);
                       }
                   });

                   return !objList.Any() ? new List<IssueLogObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

       public List<IssueLogObject> GetIssueLogs(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var issueLogObjects =
                           db.IssueLogs.OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (issueLogObjects.Any())
                       {
                           var newList = new List<IssueLogObject>();
                           issueLogObjects.ForEach(app =>
                           {
                               var issueLogObjectObject = ModelMapper.Map<IssueLog, IssueLogObject>(app);
                               if (issueLogObjectObject != null && issueLogObjectObject.Id > 0)
                               {
                                   newList.Add(issueLogObjectObject);
                               }
                           });
                           countG = db.IssueLogs.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<IssueLogObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<IssueLogObject>();
           }
       }


        public IssueLogObject GetIssueLog(long issueLogObjectId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var issueLogObjects =
                        db.IssueLogs.Where(m => m.Id == issueLogObjectId)
                            .ToList();
                    if (!issueLogObjects.Any())
                    {
                        return new IssueLogObject();
                    }

                    var app = issueLogObjects[0];
                    var issueLogObjectObject = ModelMapper.Map<IssueLog, IssueLogObject>(app);
                    if (issueLogObjectObject == null || issueLogObjectObject.Id < 1)
                    {
                        return new IssueLogObject();
                    }
                    
                  return issueLogObjectObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new IssueLogObject();
           }
       }

       public List<IssueLogObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var issueLogObjects =
                       db.IssueLogs.Where(m => m.IssueCategory.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (issueLogObjects.Any())
                   {
                       var newList = new List<IssueLogObject>();
                       issueLogObjects.ForEach(app =>
                       {
                           var issueLogObjectObject = ModelMapper.Map<IssueLog, IssueLogObject>(app);
                           if (issueLogObjectObject != null && issueLogObjectObject.Id > 0)
                           {
                               newList.Add(issueLogObjectObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<IssueLogObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<IssueLogObject>();
           }
       }

       public long DeleteIssueLog(long issueLogObjectId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.IssueLogs.Where(m => m.Id == issueLogObjectId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.IssueLogs.Remove(item);
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
