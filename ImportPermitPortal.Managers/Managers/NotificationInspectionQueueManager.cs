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
    public class NotificationInspectionQueueManager
    {

        public long AddNotificationInspectionQueue(NotificationInspectionQueueObject notificationInspectionQueue)
        {
            try
            {
                if (notificationInspectionQueue == null)
                {
                    return -2;
                }

                var notificationInspectionQueueEntity = ModelMapper.Map<NotificationInspectionQueueObject, NotificationInspectionQueue>(notificationInspectionQueue);
                if (notificationInspectionQueueEntity == null || string.IsNullOrEmpty(notificationInspectionQueueEntity.Notification.Invoice.ReferenceCode))
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.NotificationInspectionQueues.Add(notificationInspectionQueueEntity);
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

        public long UpdateNotificationInspectionQueue(NotificationInspectionQueueObject notificationInspectionQueue)
        {
            try
            {
                if (notificationInspectionQueue == null)
                {
                    return -2;
                }

                var notificationInspectionQueueEntity = ModelMapper.Map<NotificationInspectionQueueObject, NotificationInspectionQueue>(notificationInspectionQueue);
                if (notificationInspectionQueueEntity == null || notificationInspectionQueueEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.NotificationInspectionQueues.Attach(notificationInspectionQueueEntity);
                    db.Entry(notificationInspectionQueueEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return notificationInspectionQueue.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<NotificationInspectionQueueObject> GetNotificationInspectionQueues()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notificationInspectionQueues = db.NotificationInspectionQueues.ToList();
                    if (!notificationInspectionQueues.Any())
                    {
                        return new List<NotificationInspectionQueueObject>();
                    }
                    var objList = new List<NotificationInspectionQueueObject>();
                    notificationInspectionQueues.ForEach(app =>
                    {
                        var notificationInspectionQueueObject = ModelMapper.Map<NotificationInspectionQueue, NotificationInspectionQueueObject>(app);
                        if (notificationInspectionQueueObject != null && notificationInspectionQueueObject.Id > 0)
                        {
                            objList.Add(notificationInspectionQueueObject);
                        }
                    });

                    return !objList.Any() ? new List<NotificationInspectionQueueObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<NotificationInspectionQueueObject> GetNotificationInspectionQueues(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var notificationInspectionQueues =
                            db.NotificationInspectionQueues.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)
.Include("ImportApplication")
                                .ToList();
                        if (notificationInspectionQueues.Any())
                        {
                            var newList = new List<NotificationInspectionQueueObject>();
                            notificationInspectionQueues.ForEach(app =>
                            {
                                var notificationInspectionQueueObject = ModelMapper.Map<NotificationInspectionQueue, NotificationInspectionQueueObject>(app);
                                if (notificationInspectionQueueObject != null && notificationInspectionQueueObject.Id > 0)
                                {
                                    notificationInspectionQueueObject.ReferenceCode = app.Notification.Invoice.ReferenceCode;
                                    newList.Add(notificationInspectionQueueObject);
                                }
                            });
                            countG = db.NotificationInspectionQueues.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<NotificationInspectionQueueObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationInspectionQueueObject>();
            }
        }


        public NotificationInspectionQueueObject GetNotificationInspectionQueue(long notificationInspectionQueueId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notificationInspectionQueues =
                        db.NotificationInspectionQueues.Where(m => m.Id == notificationInspectionQueueId)
                            .ToList();
                    if (!notificationInspectionQueues.Any())
                    {
                        return new NotificationInspectionQueueObject();
                    }

                    var app = notificationInspectionQueues[0];
                    var notificationInspectionQueueObject = ModelMapper.Map<NotificationInspectionQueue, NotificationInspectionQueueObject>(app);
                    if (notificationInspectionQueueObject == null || notificationInspectionQueueObject.Id < 1)
                    {
                        return new NotificationInspectionQueueObject();
                    }

                    return notificationInspectionQueueObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationInspectionQueueObject();
            }
        }

        public List<NotificationInspectionQueueObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notificationInspectionQueues =
                        db.NotificationInspectionQueues.Where(m => m.Notification.Invoice.ReferenceCode.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (notificationInspectionQueues.Any())
                    {
                        var newList = new List<NotificationInspectionQueueObject>();
                        notificationInspectionQueues.ForEach(app =>
                        {
                            var notificationInspectionQueueObject = ModelMapper.Map<NotificationInspectionQueue, NotificationInspectionQueueObject>(app);
                            if (notificationInspectionQueueObject != null && notificationInspectionQueueObject.Id > 0)
                            {
                                newList.Add(notificationInspectionQueueObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<NotificationInspectionQueueObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationInspectionQueueObject>();
            }
        }

        public long DeleteNotificationInspectionQueue(long notificationInspectionQueueId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.NotificationInspectionQueues.Where(m => m.Id == notificationInspectionQueueId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.NotificationInspectionQueues.Remove(item);
                    db.SaveChanges();
                    return 5;

                    //var notificationInspectionQueue = db.NotificationInspectionQueuees.Find(notificationInspectionQueueId);
                    //db.NotificationInspectionQueuees.Remove(notificationInspectionQueue);
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
