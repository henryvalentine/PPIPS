using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class NotificationInspectionQueueServices
    {
        private readonly NotificationInspectionQueueManager _notificationInspectionQueueManager;
        public NotificationInspectionQueueServices()
        {
            _notificationInspectionQueueManager = new NotificationInspectionQueueManager();
        }

        public long AddNotificationInspectionQueue(NotificationInspectionQueueObject notificationInspectionQueue)
        {
            try
            {
                return _notificationInspectionQueueManager.AddNotificationInspectionQueue(notificationInspectionQueue);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteNotificationInspectionQueue(long notificationInspectionQueueId)
        {
            try
            {
                return _notificationInspectionQueueManager.DeleteNotificationInspectionQueue(notificationInspectionQueueId);
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
                return _notificationInspectionQueueManager.UpdateNotificationInspectionQueue(notificationInspectionQueue);
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
                var objList = _notificationInspectionQueueManager.GetNotificationInspectionQueues();
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationInspectionQueueObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationInspectionQueueObject>();
            }
        }

        public NotificationInspectionQueueObject GetNotificationInspectionQueue(long notificationInspectionQueueId)
        {
            try
            {
                return _notificationInspectionQueueManager.GetNotificationInspectionQueue(notificationInspectionQueueId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationInspectionQueueObject();
            }
        }

        public List<NotificationInspectionQueueObject> GetNotificationInspectionQueues(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _notificationInspectionQueueManager.GetNotificationInspectionQueues(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationInspectionQueueObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<NotificationInspectionQueueObject>();
            }
        }

        public List<NotificationInspectionQueueObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _notificationInspectionQueueManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationInspectionQueueObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationInspectionQueueObject>();
            }
        }
    }

}
