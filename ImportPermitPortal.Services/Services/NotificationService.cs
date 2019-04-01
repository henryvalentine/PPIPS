using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class NotificationServices  
	{
        private readonly NotificationManager _notificationManager;
        public NotificationServices()
		{
            _notificationManager = new NotificationManager();
		}

        public long AddNotification(NotificationObject notification)
		{
			try
			{
                return _notificationManager.AddNotification(notification);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}
        public long DeleteNotification(long notificationId)
        {
            try
            {
                return _notificationManager.DeleteNotification(notificationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public long UpdateNotification(NotificationObject notification)
		{
			try
			{
                return _notificationManager.UpdateNotification(notification);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public bool UpdateDischargeDepot(long notificationId, int depotId)
        {
           return _notificationManager.UpdateDischargeDepot(notificationId, depotId);
        }

        public NotificationObject GetNotificationForEdit(long notificationId)
        {
            try
            {
                return _notificationManager.GetNotificationForEdit(notificationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public NotificationObject GetBankNotification(long notificationId, long CompanyId)
        {
            try
            {
                return _notificationManager.GetBankNotification(notificationId, CompanyId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public NotificationObject SearchBankNotifications(string referenceCode, long CompanyId)
        {
            try
            {
                return _notificationManager.SearchBankNotifications(referenceCode, CompanyId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }
        
        public NotificationObject GetBankNotifications(long notificationId, long importerId)
        {
            return _notificationManager.GetBankNotifications(notificationId, importerId);
        }
       
        public NotificationObject SearchBankProcessedNotifications(string referenceCode, long CompanyId)
        {
           return _notificationManager.SearchBankProcessedNotifications(referenceCode, CompanyId);
        }

        public NotificationObject GetBankProcessedNotifications(long notificationId, long companyId)
        {
            return _notificationManager.GetBankProcessedNotifications(notificationId, companyId);
        }

        public ApplicationObject GetNotificationProcesses(long notificationId)
        {
           return _notificationManager.GetNotificationProcesses(notificationId);
        }
        public NotificationObject GetNotification(long notificationId)
        {
            try
            {
                return _notificationManager.GetNotification(notificationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public List<PermitObject> GetApplicantsValidPermits(long importerId)
        {
           return _notificationManager.GetApplicantsValidPermits(importerId);
        }

        public NotificationRequirementDetails CheckNotificationSubmit(long id)
        {
           return _notificationManager.CheckNotificationSubmit(id);
            
        }

        public NotificationRequirementDetails SubmitNotification(long id)
        {
           return _notificationManager.SubmitNotification(id);
        }

        public bool UnSubmitNotification(long id)
        {
            try
            {
                return _notificationManager.UnSubmitNotification(id);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return false;
            }
        }

        public List<NotificationObject> GetNotifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _notificationManager.GetNotifications(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> GetBankAssignedNotifications(int? itemsPerPage, int? pageNumber, out int countG, long CompanyId)
        {
            try
            {
                var objList = _notificationManager.GetBankAssignedNotifications(itemsPerPage, pageNumber, out countG, CompanyId);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<NotificationObject>();
            }
        }

       public List<NotificationObject> GetBankNotificationHistory(int? itemsPerPage, int? pageNumber, out int countG, long CompanyId)
        {
            try
            {
                var objList = _notificationManager.GetBankNotificationHistory(itemsPerPage, pageNumber, out countG, CompanyId);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<NotificationObject>();
            }
        }
       public List<NotificationObject> GetBankUserNotificationHistory(int? itemsPerPage, int? pageNumber, out int countG, long userId)
       {
           try
           {
               var objList = _notificationManager.GetBankUserNotificationHistory(itemsPerPage, pageNumber, out countG, userId);
               if (objList == null || !objList.Any())
               {
                   return new List<NotificationObject>();
               }

               return objList;
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               countG = 0;
               return new List<NotificationObject>();
           }
       }
        public List<NotificationObject> GetEmployeeNotifications(int? itemsPerPage, int? pageNumber, out int countG, long employeeId)
        {
            try
            {
                var objList = _notificationManager.GetEmployeeNotifications(itemsPerPage, pageNumber, out countG, employeeId);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> GetCompanyNotifications(int? itemsPerPage, int? pageNumber, out int countG, long CompanyId)
        {
            try
            {
                var objList = _notificationManager.GetCompanyNotifications(itemsPerPage, pageNumber, out countG, CompanyId);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> GetPreviousNotification(long permitId, long productId, out double tolerancePercentage)
        {
            try
            {
                return _notificationManager.GetPreviousNotification(permitId, productId, out tolerancePercentage);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                tolerancePercentage = 0;
                return new List<NotificationObject>();
            }
        }
        public List<NotificationObject> GetNotifications()
        {
            try
            {
                var objList = _notificationManager.GetNotifications();
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> GetCompletedNotifications(long id)
        {
            try
            {
                var objList = _notificationManager.GetCompletedNotifications(id);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> SearchCompanyNotifications(string searchCriteria, long CompanyId)
        {
            try
            {
                var objList = _notificationManager.SearchCompanyNotifications(searchCriteria, CompanyId);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> SearchEmployeeNotifications(string searchCriteria, long emplyeeId)
        {
            try
            {
                var objList = _notificationManager.SearchEmployeeNotifications(searchCriteria, emplyeeId);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }
       
        public List<NotificationObject> Search(string searchCriteria)
        {
            try
            {
                return _notificationManager.Search(searchCriteria);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }

        public ApplicationObject GetImportApplicationByPermitNumber(string permitValue, long companyId)
        {
            try
            {
                return _notificationManager.GetApplicationByPermitNumber(permitValue, companyId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public NotificationObject GetNotificationByRef(string code, string rrr)
        {
            try
            {
                return _notificationManager.GetNotificationByRef(code, rrr);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public ImportSettingObject GetImportSettings()
        {
            try
            {
                return _notificationManager.GetImportSettings();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ImportSettingObject();
            }
        }

        public NotificationObject GetNotificationByRef(string code)
        {
            try
            {
                return _notificationManager.GetNotificationByRef(code);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public List<NotificationObject> SearchBankAssignedNotifications(string searchCriteria, long CompanyId)
        {
            try
            {
                var objList = _notificationManager.SearchBankAssignedNotifications(searchCriteria, CompanyId);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }
        public List<NotificationObject> SearchBankNotificationHistory(string searchCriteria, long CompanyId)
        {
            try
            {
                var objList = _notificationManager.SearchBankNotificationHistory(searchCriteria, CompanyId);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }
        public List<NotificationObject> SearchBankUserNotificationHistory(string searchCriteria, long userId)
        {
            try
            {
                var objList = _notificationManager.SearchBankUserNotificationHistory(searchCriteria, userId);
                if (objList == null || !objList.Any())
                {
                    return new List<NotificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }




        public List<NotificationObject> GetPaidNotifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            return _notificationManager.GetPaidNotifications(itemsPerPage, pageNumber, out countG);
        }
        public List<NotificationObject> GetSubmittedNotifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            return _notificationManager.GetSubmittedNotifications(itemsPerPage, pageNumber, out countG);
        }
        public List<NotificationObject> GetProcessingNotifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            return _notificationManager.GetProcessingNotifications(itemsPerPage, pageNumber, out countG);
        }
        public List<NotificationObject> GetApprovedNotifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            return _notificationManager.GetApprovedNotifications(itemsPerPage, pageNumber, out countG);
        }
        public List<NotificationObject> GetRejectedNotifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            return _notificationManager.GetRejectedNotifications(itemsPerPage, pageNumber, out countG);
        }

        public List<NotificationObject> SearchPaidNotifications(string searchCriteria)
        {
            return _notificationManager.SearchPaidNotifications(searchCriteria);
        }
        public List<NotificationObject> SearchSubmittedNotifications(string searchCriteria)
        {
            return _notificationManager.SearchSubmittedNotifications(searchCriteria);
        }
        public List<NotificationObject> SearchProcessingNotifications(string searchCriteria)
        {
            return _notificationManager.SearchProcessingNotifications(searchCriteria);
        }
        public List<NotificationObject> SearchApprovedNotifications(string searchCriteria)
        {
            return _notificationManager.SearchApprovedNotifications(searchCriteria);
        }
        public List<NotificationObject> SearchRejectedNotifications(string searchCriteria)
        {
            return _notificationManager.SearchRejectedNotifications(searchCriteria);
        }
	}

}

