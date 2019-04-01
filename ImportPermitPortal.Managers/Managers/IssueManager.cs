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
    public class IssueManager
    {

       public long AddIssue(IssueObject issue)
       {
           try
           {
               if (issue == null ||  issue.IssueLogObject.IssueCategoryId < 1)
               {
                   return -2;
               }
               
               var issueEntity = ModelMapper.Map<IssueObject, Issue>(issue);
               if (issueEntity == null || issueEntity.AffectedUserId < 1)
               {
                   return -2;
               }

               var issueLog = issue.IssueLogObject;
               var issueLogEntity = ModelMapper.Map<IssueLogObject, IssueLog>(issueLog);
               if (issueLogEntity == null || issueLogEntity.IssueCategoryId < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   var processedLog = db.IssueLogs.Add(issueLogEntity);
                   db.SaveChanges();

                   issueEntity.IssueLogId = processedLog.Id;
                   var processedIssue = db.Issues.Add(issueEntity);
                   db.SaveChanges();
                   return processedIssue.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateIssue(IssueObject issue)
       {
           try
           {
               if (issue == null)
               {
                   return -2;
               }

               var issueEntity = ModelMapper.Map<IssueObject, Issue>(issue);
               if (issueEntity == null || issueEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.Issues.Attach(issueEntity);
                   db.Entry(issueEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return issue.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<IssueObject> GetIssues()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var issues = db.Issues.ToList();
                   if (!issues.Any())
                   {
                        return new List<IssueObject>();
                   }
                   var objList =  new List<IssueObject>();
                   issues.ForEach(app =>
                   {
                       var issueObject = ModelMapper.Map<Issue, IssueObject>(app);
                       if (issueObject != null && issueObject.Id > 0)
                       {
                           objList.Add(issueObject);
                       }
                   });

                   return !objList.Any() ? new List<IssueObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

     
        public List<IssueObject> GetIssues(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var issues =
                           db.Issues.Include("IssueLog").Include("UserProfile").Include("UserProfile1").OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       
                       if (issues.Any())
                       {
                           var issueCategories = db.IssueCategories.ToList();
                           if (!issueCategories.Any())
                           {
                               countG = 0;
                               return new List<IssueObject>();
                           }

                           var newList = new List<IssueObject>();
                           issues.ForEach(app =>
                           {
                               var issueCategory = issueCategories.Find(c => c.Id == app.IssueLog.IssueCategoryId);
                               if (issueCategory != null && issueCategory.Id > 0)
                               {
                                   var resolvedBy = db.People.Where(p => p.Id == app.UserProfile1.PersonId).ToList();
                                   var affectedCompany = db.People.Where(p => p.Id == app.UserProfile.PersonId).ToList();
                                   if (resolvedBy.Any() && affectedCompany.Any())
                                   {
                                       var issueObject = ModelMapper.Map<Issue, IssueObject>(app);
                                       if (issueObject != null && issueObject.Id > 0)
                                       {
                                           issueObject.IssueCategoryName = issueCategory.Name;
                                           issueObject.ResolvedByName = resolvedBy[0].FirstName + " " + resolvedBy[0].LastName;
                                           issueObject.AffectedCompanyName = affectedCompany[0].FirstName;
                                           newList.Add(issueObject);
                                       }
                                   }
                                 
                               }
                               
                           });
                           countG = db.Issues.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<IssueObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<IssueObject>();
           }
       }
        
       public IssueObject GetIssue(long issueId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var issues = db.Issues.Where(m => m.Id == issueId).Include("IssueLog").ToList();

                    if (!issues.Any())
                    {
                        return new IssueObject();
                    }

                    var app = issues[0];
                    var issueObject = ModelMapper.Map<Issue, IssueObject>(app);
                    if (issueObject == null || issueObject.Id < 1)
                    {
                        return new IssueObject();
                    }
                    var issueCategories = db.IssueCategories.ToList();
                    if (!issueCategories.Any())
                    {
                        return new IssueObject();
                    }

                    var issueCategory = issueCategories.Find(c => c.Id == app.IssueLog.IssueCategoryId);
                    if (issueCategory == null || issueCategory.Id < 1)
                    {
                        return new IssueObject();
                    }

                    var resolvedBy = db.People.Where(p => p.Id == app.UserProfile1.PersonId).ToList();
                    var affectedCompany = db.People.Where(p => p.Id == app.UserProfile.PersonId).ToList();
                    if (resolvedBy.Any() && affectedCompany.Any())
                    {
                        issueObject.IssueCategoryName = issueCategory.Name;
                        issueObject.ResolvedByName = resolvedBy[0].FirstName + " " + resolvedBy[0].LastName;
                        issueObject.AffectedCompanyName = affectedCompany[0].FirstName;
                        issueObject.Issue = app.IssueLog.Issue;
                    }
                    
                  return issueObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new IssueObject();
           }
       }

       public List<IssueObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var issues =
                       db.Issues.Where(m => m.IssueLog.IssueCategory.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (issues.Any())
                   {
                       var issueCategories = db.IssueCategories.ToList();
                       if (!issueCategories.Any())
                       {
                           return new List<IssueObject>();
                       }

                       var newList = new List<IssueObject>();
                       issues.ForEach(app =>
                       {
                           var issueCategory = issueCategories.Find(c => c.Id == app.IssueLog.IssueCategoryId);
                           if (issueCategory != null && issueCategory.Id > 0)
                           {
                               var resolvedBy = db.People.Where(p => p.Id == app.UserProfile1.PersonId).ToList();
                               var affectedCompany = db.People.Where(p => p.Id == app.UserProfile.PersonId).ToList();
                               if (resolvedBy.Any() && affectedCompany.Any())
                               {
                                   var issueObject = ModelMapper.Map<Issue, IssueObject>(app);
                                   if (issueObject != null && issueObject.Id > 0)
                                   {
                                       issueObject.IssueCategoryName = issueCategory.Name;
                                       issueObject.ResolvedByName = resolvedBy[0].FirstName + " " + resolvedBy[0].LastName;
                                       issueObject.AffectedCompanyName = affectedCompany[0].FirstName;
                                       newList.Add(issueObject);
                                   }
                               }

                           }

                       });

                       return newList;
                   }
               }
               return new List<IssueObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<IssueObject>();
           }
       }

       public long DeleteIssue(long issueId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.Issues.Where(m => m.Id == issueId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.Issues.Remove(item);
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
