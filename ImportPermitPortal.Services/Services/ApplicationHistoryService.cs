using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class ApplicationHistoryServices
    {
        private readonly ImportApplicationHistoryManager _applicationHistoryManager;
        public ApplicationHistoryServices()
        {
            _applicationHistoryManager = new ImportApplicationHistoryManager();
        }

        public long AddApplicationHistory(ImportApplicationHistoryObject applicationHistory)
        {
            try
            {
                return _applicationHistoryManager.AddImportApplicationHistory(applicationHistory);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteApplicationHistory(long applicationHistoryId)
        {
            try
            {
                return _applicationHistoryManager.DeleteImportApplicationHistory(applicationHistoryId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateApplicationHistory(ImportApplicationHistoryObject applicationHistory)
        {
            try
            {
                return _applicationHistoryManager.UpdateImportApplicationHistory(applicationHistory);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public ImportApplicationHistoryObject GetApplicationHistory(long applicationHistoryId)
        {
            try
            {
                return _applicationHistoryManager.GetImportApplicationHistory(applicationHistoryId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ImportApplicationHistoryObject();
            }
        }

       public List<ImportApplicationHistoryObject> GetImportApplicationHistories(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _applicationHistoryManager.GetImportApplicationHistories(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ImportApplicationHistoryObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ImportApplicationHistoryObject>();
            }
        }

        public List<ImportApplicationHistoryObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _applicationHistoryManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ImportApplicationHistoryObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ImportApplicationHistoryObject>();
            }
        }

    }

}
