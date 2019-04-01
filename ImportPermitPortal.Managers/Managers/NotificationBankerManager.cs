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
   public class NotificationBankerManager
    {

       public long AddNotificationBanker(NotificationBankerObject applicationBanker)
       {
           try
           {
               if (applicationBanker == null)
               {
                   return -2;
               }

               var applicationBankerEntity = ModelMapper.Map<NotificationBankerObject, NotificationBanker>(applicationBanker);
               if (applicationBankerEntity == null || applicationBankerEntity.NotificationId < 1)
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.NotificationBankers.Add(applicationBankerEntity);
                   db.SaveChanges();
                   return returnStatus.NotificationId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateNotificationBanker(NotificationBankerObject applicationBanker)
       {
           try
           {
               if (applicationBanker == null)
               {
                   return -2;
               }

               var applicationBankerEntity = ModelMapper.Map<NotificationBankerObject, NotificationBanker>(applicationBanker);
               if (applicationBankerEntity == null || applicationBankerEntity.NotificationId < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.NotificationBankers.Attach(applicationBankerEntity);
                   db.Entry(applicationBankerEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return applicationBanker.NotificationId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<NotificationBankerObject> GetNotificationBankers()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var applicationBankers = db.NotificationBankers.ToList();
                   if (!applicationBankers.Any())
                   {
                        return new List<NotificationBankerObject>();
                   }
                   var objList =  new List<NotificationBankerObject>();
                   applicationBankers.ForEach(app =>
                   {
                       var applicationBankerObject = ModelMapper.Map<NotificationBanker, NotificationBankerObject>(app);
                       if (applicationBankerObject != null && applicationBankerObject.Id > 0)
                       {
                           objList.Add(applicationBankerObject);
                       }
                   });

                   return !objList.Any() ? new List<NotificationBankerObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

        public List<NotificationBankerObject> GetNotificationBankers(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var applicationBankers =
                           db.NotificationBankers
                           .OrderByDescending(m => m.NotificationId)
                            .Skip(tpageNumber).Take(tsize)
                               .ToList();
                       if (applicationBankers.Any())
                       {
                           var newList = new List<NotificationBankerObject>();
                           applicationBankers.ForEach(app =>
                           {
                               var applicationBankerObject = ModelMapper.Map<NotificationBanker, NotificationBankerObject>(app);
                               if (applicationBankerObject != null && applicationBankerObject.Id > 0)
                               {
                                   newList.Add(applicationBankerObject);
                               }
                           });
                           countG = db.NotificationBankers.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<NotificationBankerObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<NotificationBankerObject>();
           }
       }

        
       public NotificationBankerObject GetNotificationBanker(long applicationBankerId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var applicationBankers =
                        db.NotificationBankers.Where(m => m.Id == applicationBankerId)
                            .ToList();
                    if (!applicationBankers.Any())
                    {
                        return new NotificationBankerObject();
                    }

                    var app = applicationBankers[0];
                    var applicationBankerObject = ModelMapper.Map<NotificationBanker, NotificationBankerObject>(app);
                    if (applicationBankerObject == null || applicationBankerObject.Id < 1)
                    {
                        return new NotificationBankerObject();
                    }
                    
                  return applicationBankerObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new NotificationBankerObject();
           }
       }

       public List<NotificationBankerObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities()) 
               {
                   var lower = searchCriteria.ToLower().Trim();
                   var applicationBankers = (from apb in db.NotificationBankers
                       join bnk in db.Banks.Where(j => j.Name.Contains(lower)) on apb.BankId equals bnk.BankId
                                             join imp in db.Applications on apb.NotificationId equals imp.Id

                       select new NotificationBankerObject
                       {
                          Id = apb.Id,
                          NotificationId = imp.Id,
                          BankId = bnk.BankId,
                          FinancedQuantity = apb.FinancedQuantity,
                          TransactionAmount = apb.TransactionAmount,
                          ActualQuantity = apb.ActualQuantity,
                          AttachedDocumentId = apb.AttachedDocumentId,
                          ProductId = apb.ProductId
                       }).ToList();
                       
                   if (!applicationBankers.Any())
                   {
                       return new List<NotificationBankerObject>();
                   }
                   return applicationBankers;
               }
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<NotificationBankerObject>();
           }
       }

       public long DeleteNotificationBanker(long applicationBankerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.NotificationBankers.Where(m => m.Id == applicationBankerId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.NotificationBankers.Remove(item);
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
