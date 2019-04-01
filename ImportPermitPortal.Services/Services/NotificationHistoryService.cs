using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class NotificationHistoryServices
    {
        private readonly NotificationHistoryManager _notificationHistoryManager;
        public NotificationHistoryServices()
        {
            _notificationHistoryManager = new NotificationHistoryManager();
        }

        public long AddNotificationHistory(NotificationHistoryObject notificationHistory)
        {
            try
            {
                return _notificationHistoryManager.AddNotificationHistory(notificationHistory);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        //public ApplicationObject GetApplicationFromHistory(long historyId, long userId)
        //{
        //    try
        //    {
        //        return _notificationHistoryManager.GetApplicationFromHistory(historyId, userId);

        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return new ApplicationObject();
        //    }
        //}

        public long DeleteNotificationHistory(long notificationHistoryId)
        {
            try
            {
                return _notificationHistoryManager.DeleteNotificationHistory(notificationHistoryId);
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
                return _notificationHistoryManager.UpdateNotificationHistory(notificationHistory);
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
                var objList = _notificationHistoryManager.GetNotificationHistorys();
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationHistoryObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationHistoryObject>();
            }
        }

        public NotificationHistoryObject GetNotificationHistory(long notificationHistoryId)
        {
            try
            {
                return _notificationHistoryManager.GetNotificationHistory(notificationHistoryId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationHistoryObject();
            }
        }

        public List<NotificationHistoryObject> GetNotificationHistorys(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _notificationHistoryManager.GetNotificationHistorys(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationHistoryObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<NotificationHistoryObject>();
            }
        }

        public List<NotificationHistoryObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _notificationHistoryManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationHistoryObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationHistoryObject>();
            }
        }



    }

}
