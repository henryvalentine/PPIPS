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
    public class NotificationHistoryManager
    {

        public long AddNotificationHistory(NotificationHistoryObject notificationHistory)
        {
            try
            {
                if (notificationHistory == null)
                {
                    return -2;
                }

                var notificationHistoryEntity = ModelMapper.Map<NotificationHistoryObject, NotificationHistory>(notificationHistory);
                if (notificationHistoryEntity == null || string.IsNullOrEmpty(notificationHistoryEntity.Notification.Invoice.ReferenceCode))
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.NotificationHistories.Add(notificationHistoryEntity);
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

        public long UpdateNotificationHistory(NotificationHistoryObject notificationHistory)
        {
            try
            {
                if (notificationHistory == null)
                {
                    return -2;
                }

                var notificationHistoryEntity = ModelMapper.Map<NotificationHistoryObject, NotificationHistory>(notificationHistory);
                if (notificationHistoryEntity == null || notificationHistoryEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.NotificationHistories.Attach(notificationHistoryEntity);
                    db.Entry(notificationHistoryEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return notificationHistory.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<NotificationHistoryObject> GetNotificationHistorys()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notificationHistorys = db.NotificationHistories.ToList();
                    if (!notificationHistorys.Any())
                    {
                        return new List<NotificationHistoryObject>();
                    }
                    var objList = new List<NotificationHistoryObject>();
                    notificationHistorys.ForEach(app =>
                    {
                        var notificationHistoryObject = ModelMapper.Map<NotificationHistory, NotificationHistoryObject>(app);
                        if (notificationHistoryObject != null && notificationHistoryObject.Id > 0)
                        {
                            objList.Add(notificationHistoryObject);
                        }
                    });

                    return !objList.Any() ? new List<NotificationHistoryObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<NotificationHistoryObject> GetNotificationHistorys(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var notificationHistorys =
                            db.NotificationHistories.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)
.Include("Notification")
                                .ToList();
                        if (notificationHistorys.Any())
                        {
                            var newList = new List<NotificationHistoryObject>();
                            notificationHistorys.ForEach(app =>
                            {
                                var notificationHistoryObject = ModelMapper.Map<NotificationHistory, NotificationHistoryObject>(app);
                                if (notificationHistoryObject != null && notificationHistoryObject.Id > 0)
                                {
                                    notificationHistoryObject.ReferenceCode = app.Notification.Invoice.ReferenceCode;
                                    newList.Add(notificationHistoryObject);
                                }
                            });
                            countG = db.NotificationHistories.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<NotificationHistoryObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationHistoryObject>();
            }
        }


        public NotificationHistoryObject GetNotificationHistory(long notificationHistoryId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notificationHistorys =
                        db.NotificationHistories.Where(m => m.Id == notificationHistoryId)
                            .ToList();
                    if (!notificationHistorys.Any())
                    {
                        return new NotificationHistoryObject();
                    }

                    var app = notificationHistorys[0];
                    var notificationHistoryObject = ModelMapper.Map<NotificationHistory, NotificationHistoryObject>(app);
                    if (notificationHistoryObject == null || notificationHistoryObject.Id < 1)
                    {
                        return new NotificationHistoryObject();
                    }

                    return notificationHistoryObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationHistoryObject();
            }
        }

        public List<NotificationHistoryObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notificationHistorys =
                        db.NotificationHistories.Where(m => m.Notification.Invoice.ReferenceCode.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (notificationHistorys.Any())
                    {
                        var newList = new List<NotificationHistoryObject>();
                        notificationHistorys.ForEach(app =>
                        {
                            var notificationHistoryObject = ModelMapper.Map<NotificationHistory, NotificationHistoryObject>(app);
                            if (notificationHistoryObject != null && notificationHistoryObject.Id > 0)
                            {
                                newList.Add(notificationHistoryObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<NotificationHistoryObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationHistoryObject>();
            }
        }

        public long DeleteNotificationHistory(long notificationHistoryId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.NotificationHistories.Where(m => m.Id == notificationHistoryId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.NotificationHistories.Remove(item);
                    db.SaveChanges();
                    return 5;

                    //var notificationHistory = db.NotificationHistoryes.Find(notificationHistoryId);
                    //db.NotificationHistoryes.Remove(notificationHistory);
                    //db.SaveChanges();
                    //return 5;
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
