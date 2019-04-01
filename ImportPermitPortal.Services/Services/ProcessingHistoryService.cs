using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class ProcessingHistoryServices
    {
        private readonly ProcessingHistoryManager _processingHistoryManager;
        public ProcessingHistoryServices()
        {
            _processingHistoryManager = new ProcessingHistoryManager();
        }

        public long AddProcessingHistory(ProcessingHistoryObject processingHistory)
        {
            try
            {
                return _processingHistoryManager.AddProcessingHistory(processingHistory);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public ApplicationObject GetApplicationFromHistory(long historyId)
        {
            try
            {
                return _processingHistoryManager.GetApplicationFromHistory(historyId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public long DeleteProcessingHistory(long processingHistoryId)
        {
            try
            {
                return _processingHistoryManager.DeleteProcessingHistory(processingHistoryId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateProcessingHistory(ProcessingHistoryObject processingHistory)
        {
            try
            {
                return _processingHistoryManager.UpdateProcessingHistory(processingHistory);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<ProcessingHistoryObject> GetProcessingHistorys()
        {
            try
            {
                var objList = _processingHistoryManager.GetProcessingHistorys();
                if (objList == null || !objList.Any())
                {
                    return new List<ProcessingHistoryObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProcessingHistoryObject>();
            }
        }

        public ProcessingHistoryObject GetProcessingHistory(long processingHistoryId)
        {
            try
            {
                return _processingHistoryManager.GetProcessingHistory(processingHistoryId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ProcessingHistoryObject();
            }
        }

        public List<ProcessingHistoryObject> GetProcessingHistorys(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _processingHistoryManager.GetProcessingHistorys(itemsPerPage, pageNumber, out countG);
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

        public List<ProcessingHistoryObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _processingHistoryManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ProcessingHistoryObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProcessingHistoryObject>();
            }
        }

       

    }

}
