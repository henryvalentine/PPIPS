using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class RecertificationHistoryServices
    {
        private readonly RecertificationHistoryManager _recertificationHistoryManager;
        public RecertificationHistoryServices()
        {
            _recertificationHistoryManager = new RecertificationHistoryManager();
        }

        public long AddRecertificationHistory(RecertificationHistoryObject recertificationHistory)
        {
            try
            {
                return _recertificationHistoryManager.AddRecertificationHistory(recertificationHistory);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public ApplicationObject GetApplicationFromHistory(long historyId, long userId)
        {
            try
            {
                return _recertificationHistoryManager.GetApplicationFromHistory(historyId, userId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public long DeleteRecertificationHistory(long recertificationHistoryId)
        {
            try
            {
                return _recertificationHistoryManager.DeleteRecertificationHistory(recertificationHistoryId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateRecertificationHistory(RecertificationHistoryObject recertificationHistory)
        {
            try
            {
                return _recertificationHistoryManager.UpdateRecertificationHistory(recertificationHistory);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<RecertificationHistoryObject> GetRecertificationHistorys()
        {
            try
            {
                var objList = _recertificationHistoryManager.GetRecertificationHistorys();
                if (objList == null || !objList.Any())
                {
                    return new List<RecertificationHistoryObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RecertificationHistoryObject>();
            }
        }

        public RecertificationHistoryObject GetRecertificationHistory(long recertificationHistoryId)
        {
            try
            {
                return _recertificationHistoryManager.GetRecertificationHistory(recertificationHistoryId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new RecertificationHistoryObject();
            }
        }

        public List<RecertificationHistoryObject> GetRecertificationHistorys(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _recertificationHistoryManager.GetRecertificationHistorys(itemsPerPage, pageNumber, out countG);
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

        public List<RecertificationHistoryObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _recertificationHistoryManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<RecertificationHistoryObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RecertificationHistoryObject>();
            }
        }



    }

}
