using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class EmployeeProfileServices
    {
        private readonly EmployeeProfileManager _employeeProfileManager;
        public EmployeeProfileServices()
        {
            _employeeProfileManager = new EmployeeProfileManager();
        }

        public ApplicationObject GetApplicationAdmin(long applicationId)
        {
            try
            {
                return _employeeProfileManager.GetApplicationAdmin(applicationId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }
        public ApplicationObject GetApplication(long trackId,long userId)
        {
            try
            {
                return _employeeProfileManager.GetApplication(trackId,userId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public ResponseObject GetDashboard(long userId)
        {
            try
            {
                return _employeeProfileManager.GetDashboard(userId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ResponseObject();
            }
        }

        public RecertificationObject GetRecertification(long trackId, long userId)
        {
            try
            {
                return _employeeProfileManager.GetRecertification(trackId, userId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new RecertificationObject();
            }
        }

        public ApplicationObject GetApplicationFromHistory(long historyId, long userId)
        {
            try
            {
                return _employeeProfileManager.GetApplicationFromHistory(historyId, userId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }
        public NotificationObject GetNotification(long trackId, long userId)
        {
            try
            {
                return _employeeProfileManager.GetNotification(trackId, userId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public NotificationObject GetNotificationBack(long Id, long userId)
        {
            try
            {
                return _employeeProfileManager.GetNotificationBack(Id, userId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public ApplicationObject GetPreviousApplicationTasks(long id, string userId)
        {
            try
            {
                return _employeeProfileManager.GetPreviousApplicationTasks(id, userId);

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
                return _employeeProfileManager.AddProcessTracking(processTracking);
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
                return _employeeProfileManager.DeleteProcessTracking(processTrackingId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public NotificationObject GetNotificationFromDetail(long Id, long userId)
        {
            try
            {
                return _employeeProfileManager.GetNotificationFromDetail(Id, userId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public NotificationObject GetNotificationAdmin(long Id)
        {
            try
            {
                return _employeeProfileManager.GetNotificationAdmin(Id);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public long UpdateProcessTracking(ProcessTrackingObject processTracking)
        {
            try
            {
                return _employeeProfileManager.UpdateProcessTracking(processTracking);
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
                var objList = _employeeProfileManager.GetProcessTrackings();
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
                return _employeeProfileManager.GetProcessTracking(processTrackingId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ProcessTrackingObject();
            }
        }

        public List<ProcessTrackingObject> GetEmployeeProfiles(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {
                var objList = _employeeProfileManager.GetEmployeeProfiles(itemsPerPage, pageNumber, out countG, userId);
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

        public List<NotificationInspectionQueueObject> GetNotificationTrackProfiles(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {
                var objList = _employeeProfileManager.GetNotificationTrackProfiles(itemsPerPage, pageNumber, out countG, userId);
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

        public List<RecertificationProcessObject> GetRecertificationTrackProfiles(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {
                var objList = _employeeProfileManager.GetRecertificationTrackProfiles(itemsPerPage, pageNumber, out countG, userId);
                if (objList == null || !objList.Any())
                {
                    return new List<RecertificationProcessObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<RecertificationProcessObject>();
            }
        }


        public List<ProcessingHistoryObject> GetPreviousJobsProfiles(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {
                var objList = _employeeProfileManager.GetPreviousJobsProfiles(itemsPerPage, pageNumber, out countG, userId);
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

        public List<ProcessTrackingObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _employeeProfileManager.Search(searchCriteria);
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
