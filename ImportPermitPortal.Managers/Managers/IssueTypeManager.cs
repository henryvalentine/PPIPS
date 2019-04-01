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
   public class IssueTypeManager
    {
       public long AddIssueType(IssueTypeObject issueType)
       {
           try
           {
               if (issueType == null)
               {
                   return -2;
               }

               var issueTypeEntity = ModelMapper.Map<IssueTypeObject, IssueType>(issueType);
               if (issueTypeEntity == null || string.IsNullOrEmpty(issueTypeEntity.Name))
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.IssueTypes.Add(issueTypeEntity);
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

       public long UpdateIssueType(IssueTypeObject issueType)
       {
           try
           {
               if (issueType == null)
               {
                   return -2;
               }

               var issueTypeEntity = ModelMapper.Map<IssueTypeObject, IssueType>(issueType);
               if (issueTypeEntity == null || issueTypeEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.IssueTypes.Attach(issueTypeEntity);
                   db.Entry(issueTypeEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return issueType.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public List<IssueTypeObject> GetIssueTypes()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var issueTypes = db.IssueTypes.ToList();
                   if (!issueTypes.Any())
                   {
                        return new List<IssueTypeObject>();
                   }
                   var objList =  new List<IssueTypeObject>();
                   issueTypes.ForEach(app =>
                   {
                       var issueTypeObject = ModelMapper.Map<IssueType, IssueTypeObject>(app);
                       if (issueTypeObject != null && issueTypeObject.Id > 0)
                       {
                           objList.Add(issueTypeObject);
                       }
                   });

                   return !objList.Any() ? new List<IssueTypeObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }
       
        public List<IssueTypeObject> GetIssueTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var issueTypes =
                           db.IssueTypes.OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (issueTypes.Any())
                       {
                           var newList = new List<IssueTypeObject>();
                           issueTypes.ForEach(app =>
                           {
                               var issueTypeObject = ModelMapper.Map<IssueType, IssueTypeObject>(app);
                               if (issueTypeObject != null && issueTypeObject.Id > 0)
                               {
                                   newList.Add(issueTypeObject);
                               }
                           });
                           countG = db.IssueTypes.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<IssueTypeObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<IssueTypeObject>();
           }
       }
       
       public IssueTypeObject GetIssueType(long issueTypeId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var issueTypes =
                        db.IssueTypes.Where(m => m.Id == issueTypeId)
                            .ToList();
                    if (!issueTypes.Any())
                    {
                        return new IssueTypeObject();
                    }

                    var app = issueTypes[0];
                    var issueTypeObject = ModelMapper.Map<IssueType, IssueTypeObject>(app);
                    if (issueTypeObject == null || issueTypeObject.Id < 1)
                    {
                        return new IssueTypeObject();
                    }
                    
                  return issueTypeObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new IssueTypeObject();
           }
       }
       public List<IssueTypeObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var issueTypes =
                       db.IssueTypes.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (issueTypes.Any())
                   {
                       var newList = new List<IssueTypeObject>();
                       issueTypes.ForEach(app =>
                       {
                           var issueTypeObject = ModelMapper.Map<IssueType, IssueTypeObject>(app);
                           if (issueTypeObject != null && issueTypeObject.Id > 0)
                           {
                               newList.Add(issueTypeObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<IssueTypeObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<IssueTypeObject>();
           }
       }

       public long DeleteIssueType(long issueTypeId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.IssueTypes.Where(m => m.Id == issueTypeId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.IssueTypes.Remove(item);
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
