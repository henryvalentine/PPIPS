using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class NotificationCheckListServices
    {
        private readonly NotificationCheckListManager _notificationCheckListManager;
        public NotificationCheckListServices()
        {
            _notificationCheckListManager = new NotificationCheckListManager();
        }

        public long AddNotificationCheckList(NotificationCheckListObject notificationCheckList)
        {
            try
            {
                return _notificationCheckListManager.AddNotificationCheckList(notificationCheckList);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteNotificationCheckList(long notificationCheckListId)
        {
            try
            {
                return _notificationCheckListManager.DeleteNotificationCheckList(notificationCheckListId);
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
                return _notificationCheckListManager.UpdateNotificationCheckList(notificationCheckList);
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
                var objList = _notificationCheckListManager.GetNotificationCheckLists();
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationCheckListObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationCheckListObject>();
            }
        }


     

        public NotificationCheckListObject GetNotificationCheckList(long notificationCheckListId)
        {
            try
            {
                return _notificationCheckListManager.GetNotificationCheckList(notificationCheckListId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationCheckListObject();
            }
        }

        public List<NotificationCheckListObject> GetNotificationCheckLists(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _notificationCheckListManager.GetNotificationCheckLists(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationCheckListObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<NotificationCheckListObject>();
            }
        }




        public List<NotificationCheckListObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _notificationCheckListManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationCheckListObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationCheckListObject>();
            }
        }
    }

}
