using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class NotificationBankerServices
	{
        private readonly NotificationBankerManager _applicationBankerManager; 
        public NotificationBankerServices()
		{
            _applicationBankerManager = new NotificationBankerManager();
		}

        public long AddNotificationBanker(NotificationBankerObject applicationBanker)
		{
			try
			{
                return _applicationBankerManager.AddNotificationBanker(applicationBanker);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteNotificationBanker(long applicationBankerId)
        {
            try
            {
                return _applicationBankerManager.DeleteNotificationBanker(applicationBankerId);
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
                return _applicationBankerManager.UpdateNotificationBanker(applicationBanker);
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
                var objList = _applicationBankerManager.GetNotificationBankers();
                if (objList == null || !objList.Any())
			    {
                    return new List<NotificationBankerObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationBankerObject>();
			}
		}

        public NotificationBankerObject GetNotificationBanker(long applicationBankerId)
        {
            try
            {
                return _applicationBankerManager.GetNotificationBanker(applicationBankerId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationBankerObject();
            }
        }

        public List<NotificationBankerObject> GetNotificationBankers(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _applicationBankerManager.GetNotificationBankers(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationBankerObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<NotificationBankerObject>();
            }
        }

        public List<NotificationBankerObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _applicationBankerManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationBankerObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationBankerObject>();
            }
        }
	}

}
