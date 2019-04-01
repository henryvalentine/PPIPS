using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class ImportStageServices
	{
        private readonly ImportStageManager _applicationStageManager;
        public ImportStageServices()
		{
            _applicationStageManager = new ImportStageManager();
		}

        public long AddImportStage(ImportStageObject applicationStage)
		{
			try
			{
                return _applicationStageManager.AddImportStage(applicationStage);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteImportStage(long applicationStageId)
        {
            try
            {
                return _applicationStageManager.DeleteImportStage(applicationStageId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateImportStage(ImportStageObject applicationStage)
        {
            try
            {
                return _applicationStageManager.UpdateImportStage(applicationStage);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<ImportStageObject> GetImportStages()
		{
			try
			{
                var objList = _applicationStageManager.GetImportStages();
                if (objList == null || !objList.Any())
			    {
                    return new List<ImportStageObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ImportStageObject>();
			}
		}

        public ImportStageObject GetImportStage(long applicationStageId)
        {
            try
            {
                return _applicationStageManager.GetImportStage(applicationStageId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ImportStageObject();
            }
        }

        public List<ImportStageObject> GetImportStages(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _applicationStageManager.GetImportStages(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ImportStageObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ImportStageObject>();
            }
        }

        public List<ImportStageObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _applicationStageManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ImportStageObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ImportStageObject>();
            }
        }

        public List<ImportStageObject> GetStagesWithProcesses()
        {
            try
            {
                var objList = _applicationStageManager.GetStagesWithProcesses();
                if (objList == null || !objList.Any())
                {
                    return new List<ImportStageObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ImportStageObject>();
            }
        }
	}

}
