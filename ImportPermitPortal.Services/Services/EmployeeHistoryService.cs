using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class EmployeeHistoryServices
    {
        private readonly EmployeeHistoryManager _employeeHistoryManager;
        public EmployeeHistoryServices()
        {
            _employeeHistoryManager = new EmployeeHistoryManager();
        }

        
        public ApplicationObject GetApplication(long trackId)
        {
            try
            {
                return _employeeHistoryManager.GetApplication(trackId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public NotificationObject GetNotification(long trackId)
        {
            try
            {
                return _employeeHistoryManager.GetNotification(trackId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public RecertificationObject GetRecertification(long trackId)
        {
            try
            {
                return _employeeHistoryManager.GetRecertification(trackId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new RecertificationObject();
            }
        }

        public ApplicationObject PreviousTasks(long id, string userId)
        {
            try
            {
                return _employeeHistoryManager.PreviousTasks(id,userId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }
        public long AddProcessTracking(ProcessTrackingObject processTracking)
        {
            try
            {
                return _employeeHistoryManager.AddProcessTracking(processTracking);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteProcessTracking(long processTrackingId)
        {
            try
            {
                return _employeeHistoryManager.DeleteProcessTracking(processTrackingId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateProcessTracking(ProcessTrackingObject processTracking)
        {
            try
            {
                return _employeeHistoryManager.UpdateProcessTracking(processTracking);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<ProcessTrackingObject> GetProcessTrackings()
        {
            try
            {
                var objList = _employeeHistoryManager.GetProcessTrackings();
                if (objList == null || !objList.Any())
                {
                    return new List<ProcessTrackingObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProcessTrackingObject>();
            }
        }

        public ProcessTrackingObject GetProcessTracking(long processTrackingId)
        {
            try
            {
                return _employeeHistoryManager.GetProcessTracking(processTrackingId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ProcessTrackingObject();
            }
        }

        public List<ProcessTrackingObject> GetEmployeeHistorys(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {
                var objList = _employeeHistoryManager.GetEmployeeHistorys(itemsPerPage, pageNumber, out countG, userId);
                if (objList == null || !objList.Any())
                {
                    return new List<ProcessTrackingObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ProcessTrackingObject>();
            }
        }



        public List<ProcessingHistoryObject> GetPreviousJobsHistorys(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {
                var objList = _employeeHistoryManager.GetPreviousJobsHistorys(itemsPerPage, pageNumber, out countG, userId);
                if (objList == null || !objList.Any())
                {
                    return new List<ProcessingHistoryObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ProcessingHistoryObject>();
            }
        }

        public List<NotificationHistoryObject> GetPreviousNotificationJobsHistorys(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {
                var objList = _employeeHistoryManager.GetPreviousNotificationJobsHistorys(itemsPerPage, pageNumber, out countG, userId);
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

        public List<RecertificationHistoryObject> GetPreviousRecertificationJobsHistorys(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {
                var objList = _employeeHistoryManager.GetPreviousRecertificationJobsHistorys(itemsPerPage, pageNumber, out countG, userId);
                if (objList == null || !objList.Any())
                {
                    return new List<RecertificationHistoryObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<RecertificationHistoryObject>();
            }
        }

        public List<ProcessTrackingObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _employeeHistoryManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ProcessTrackingObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProcessTrackingObject>();
            }
        }
    }

}
