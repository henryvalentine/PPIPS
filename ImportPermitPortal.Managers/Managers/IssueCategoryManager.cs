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
    public class IssueCategoryManager
    {

       public long AddIssueCategory(IssueCategoryObject issueCategory)
       {
           try
           {
               if (issueCategory == null)
               {
                   return -2;
               }

               var issueCategoryEntity = ModelMapper.Map<IssueCategoryObject, IssueCategory>(issueCategory);
               if (issueCategoryEntity == null || string.IsNullOrEmpty(issueCategoryEntity.Name))
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.IssueCategories.Add(issueCategoryEntity);
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

       public long UpdateIssueCategory(IssueCategoryObject issueCategory)
       {
           try
           {
               if (issueCategory == null)
               {
                   return -2;
               }

               var issueCategoryEntity = ModelMapper.Map<IssueCategoryObject, IssueCategory>(issueCategory);
               if (issueCategoryEntity == null || issueCategoryEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.IssueCategories.Attach(issueCategoryEntity);
                   db.Entry(issueCategoryEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return issueCategory.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<IssueCategoryObject> GetIssueCategories()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var issueCategorys = db.IssueCategories.ToList();
                   if (!issueCategorys.Any())
                   {
                        return new List<IssueCategoryObject>();
                   }
                   var objList =  new List<IssueCategoryObject>();
                   issueCategorys.ForEach(app =>
                   {
                       var issueCategoryObject = ModelMapper.Map<IssueCategory, IssueCategoryObject>(app);
                       if (issueCategoryObject != null && issueCategoryObject.Id > 0)
                       {
                           objList.Add(issueCategoryObject);
                       }
                   });

                   return !objList.Any() ? new List<IssueCategoryObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

     
        public List<IssueCategoryObject> GetIssueCategories(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var issueCategorys =
                           db.IssueCategories.OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (issueCategorys.Any())
                       {
                           var newList = new List<IssueCategoryObject>();
                           issueCategorys.ForEach(app =>
                           {
                               var issueCategoryObject = ModelMapper.Map<IssueCategory, IssueCategoryObject>(app);
                               if (issueCategoryObject != null && issueCategoryObject.Id > 0)
                               {
                                   newList.Add(issueCategoryObject);
                               }
                           });
                           countG = db.IssueCategories.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<IssueCategoryObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<IssueCategoryObject>();
           }
       }

        
       public IssueCategoryObject GetIssueCategory(long issueCategoryId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var issueCategorys =
                        db.IssueCategories.Where(m => m.Id == issueCategoryId)
                            .ToList();
                    if (!issueCategorys.Any())
                    {
                        return new IssueCategoryObject();
                    }

                    var app = issueCategorys[0];
                    var issueCategoryObject = ModelMapper.Map<IssueCategory, IssueCategoryObject>(app);
                    if (issueCategoryObject == null || issueCategoryObject.Id < 1)
                    {
                        return new IssueCategoryObject();
                    }
                    
                  return issueCategoryObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new IssueCategoryObject();
           }
       }

       public List<IssueCategoryObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var issueCategorys =
                       db.IssueCategories.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (issueCategorys.Any())
                   {
                       var newList = new List<IssueCategoryObject>();
                       issueCategorys.ForEach(app =>
                       {
                           var issueCategoryObject = ModelMapper.Map<IssueCategory, IssueCategoryObject>(app);
                           if (issueCategoryObject != null && issueCategoryObject.Id > 0)
                           {
                               newList.Add(issueCategoryObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<IssueCategoryObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<IssueCategoryObject>();
           }
       }

       public long DeleteIssueCategory(long issueCategoryId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.IssueCategories.Where(m => m.Id == issueCategoryId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.IssueCategories.Remove(item);
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
