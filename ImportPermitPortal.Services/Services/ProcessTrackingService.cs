using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class ProcessTrackingServices
    {
        private readonly ProcessTrackingManager _processTrackingManager;
        public ProcessTrackingServices()
        {
            _processTrackingManager = new ProcessTrackingManager();
        }

        public long AddProcessTracking(ProcessTrackingObject processTracking)
        {
            try
            {
                return _processTrackingManager.AddProcessTracking(processTracking);
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
                return _processTrackingManager.DeleteProcessTracking(processTrackingId);
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
                return _processTrackingManager.UpdateProcessTracking(processTracking);
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
                var objList = _processTrackingManager.GetProcessTrackings();
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
                return _processTrackingManager.GetProcessTracking(processTrackingId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ProcessTrackingObject();
            }
        }

        public List<ProcessTrackingObject> GetProcessTrackings(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _processTrackingManager.GetProcessTrackings(itemsPerPage, pageNumber, out countG);
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

        public List<ProcessTrackingObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _processTrackingManager.Search(searchCriteria);
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
