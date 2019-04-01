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
    public class NotificationCheckListManager
    {

        public long AddNotificationCheckList(NotificationCheckListObject notificationCheckList)
        {
            try
            {
                if (notificationCheckList == null)
                {
                    return -2;
                }

                var notificationCheckListEntity = ModelMapper.Map<NotificationCheckListObject, NotificationCheckList>(notificationCheckList);

                if (notificationCheckListEntity == null)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.NotificationCheckLists.Add(notificationCheckListEntity);
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

        public long UpdateNotificationCheckList(NotificationCheckListObject notificationCheckList)
        {
            try
            {
                if (notificationCheckList == null)
                {
                    return -2;
                }

                var notificationCheckListEntity = ModelMapper.Map<NotificationCheckListObject, NotificationCheckList>(notificationCheckList);
                if (notificationCheckListEntity == null || notificationCheckListEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.NotificationCheckLists.Attach(notificationCheckListEntity);
                    db.Entry(notificationCheckListEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return notificationCheckList.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<NotificationCheckListObject> GetNotificationCheckLists()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notificationCheckLists = db.NotificationCheckLists.ToList();
                    if (!notificationCheckLists.Any())
                    {
                        return new List<NotificationCheckListObject>();
                    }
                    var objList = new List<NotificationCheckListObject>();
                    notificationCheckLists.ForEach(app =>
                    {
                        var notificationCheckListObject = ModelMapper.Map<NotificationCheckList, NotificationCheckListObject>(app);
                        if (notificationCheckListObject != null && notificationCheckListObject.Id > 0)
                        {
                            objList.Add(notificationCheckListObject);
                        }
                    });

                    return !objList.Any() ? new List<NotificationCheckListObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }




       


        public List<NotificationCheckListObject> GetNotificationCheckLists(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var notificationCheckLists =
                            db.NotificationCheckLists.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)

                                .ToList();
                        if (notificationCheckLists.Any())
                        {
                            var newList = new List<NotificationCheckListObject>();
                            notificationCheckLists.ForEach(app =>
                            {
                                var notificationCheckListObject = ModelMapper.Map<NotificationCheckList, NotificationCheckListObject>(app);
                                if (notificationCheckListObject != null && notificationCheckListObject.Id > 0)
                                {
                                   
                                    newList.Add(notificationCheckListObject);
                                }
                            });
                            countG = db.NotificationCheckLists.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<NotificationCheckListObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationCheckListObject>();
            }
        }


        public NotificationCheckListObject GetNotificationCheckList(long notificationCheckListId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notificationCheckLists =
                        db.NotificationCheckLists.Where(m => m.Id == notificationCheckListId)
                            .ToList();
                    if (!notificationCheckLists.Any())
                    {
                        return new NotificationCheckListObject();
                    }

                    var app = notificationCheckLists[0];
                    var notificationCheckListObject = ModelMapper.Map<NotificationCheckList, NotificationCheckListObject>(app);
                    if (notificationCheckListObject == null || notificationCheckListObject.Id < 1)
                    {
                        return new NotificationCheckListObject();
                    }

                    return notificationCheckListObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationCheckListObject();
            }
        }

        public List<NotificationCheckListObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notificationCheckLists =
                        db.NotificationCheckLists.Where(m => m.CheckListItem.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (notificationCheckLists.Any())
                    {
                        var newList = new List<NotificationCheckListObject>();
                        notificationCheckLists.ForEach(app =>
                        {
                            var notificationCheckListObject = ModelMapper.Map<NotificationCheckList, NotificationCheckListObject>(app);
                            if (notificationCheckListObject != null && notificationCheckListObject.Id > 0)
                            {
                                newList.Add(notificationCheckListObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<NotificationCheckListObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationCheckListObject>();
            }
        }

        public long DeleteNotificationCheckList(long notificationCheckListId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.NotificationCheckLists.Where(m => m.Id == notificationCheckListId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.NotificationCheckLists.Remove(item);
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
