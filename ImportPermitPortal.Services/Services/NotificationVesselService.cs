using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class NotificationVesselServices
    {
        private readonly NotificationVesselManager _notificationVesselManager;
        public NotificationVesselServices()
        {
            _notificationVesselManager = new NotificationVesselManager();
        }

        public long AddNotificationVessel(NotificationVesselObject notificationVessel)
        {
            try
            {
                return _notificationVesselManager.AddNotificationVessel(notificationVessel);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public long UpdateNotificationVessel(NotificationVesselObject notificationVessel)
        {
            try
            {
                return _notificationVesselManager.UpdateNotificationVessel(notificationVessel);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddNotificationVessels(List<NotificationVesselObject> notificationVessels)
        {
            try
            {
                return _notificationVesselManager.AddNotificationVessels(notificationVessels);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateNotificationVessels(List<NotificationVesselObject> oldNotificationVessels, List<NotificationVesselObject> notificationVessels)
        {
            try
            {
                return _notificationVesselManager.UpdateNotificationVessels(oldNotificationVessels, notificationVessels);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<NotificationVesselObject> GetNotificationVessels()
        {
            try
            {
                var objList = _notificationVesselManager.GetNotificationVessels();
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationVesselObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationVesselObject>();
            }
        }

        public List<NotificationVesselObject> GetNotificationVesselsByNotification(long notificationId)
        {
            try
            {
                return _notificationVesselManager.GetNotificationVesselsByNotification(notificationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationVesselObject>();
            }
        }

        public NotificationVesselObject GetNotificationVessel(long notificationVesselId)
        {
            try
            {
                return _notificationVesselManager.GetNotificationVessel(notificationVesselId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationVesselObject();
            }
        }

        public List<NotificationVesselObject> GetNotificationVessels(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _notificationVesselManager.GetNotificationVessels(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationVesselObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<NotificationVesselObject>();
            }
        }

        public List<NotificationVesselObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _notificationVesselManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationVesselObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationVesselObject>();
            }
        }
        public long DeleteNotificationVessel(long notificationVesselId)
        {
            try
            {
                return _notificationVesselManager.DeleteNotificationVessel(notificationVesselId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
    }

}
