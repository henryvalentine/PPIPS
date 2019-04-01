using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class ImportRequirementServices
	{
        private readonly ImportRequirementManager _importClassificationRequirementManager;
        public ImportRequirementServices()
		{
            _importClassificationRequirementManager = new ImportRequirementManager();
		}

        public long AddImportRequirement(List<ImportRequirementObject> importClassificationRequirements)
		{
			try
			{
                return _importClassificationRequirementManager.AddImportRequirement(importClassificationRequirements);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteImportRequirement(long importClassificationRequirementId)
        {
            try
            {
                return _importClassificationRequirementManager.DeleteImportRequirement(importClassificationRequirementId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateImportRequirement(ImportRequirementObject importClassificationRequirement, List<ImportRequirementObject> importClassificationRequirements)
        {
            try
            {
                return _importClassificationRequirementManager.UpdateImportRequirement(importClassificationRequirement, importClassificationRequirements);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

       public ImportRequirementObject GetImportRequirement(long importClassificationRequirementId)
        {
            try
            {
                return _importClassificationRequirementManager.GetImportRequirement(importClassificationRequirementId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ImportRequirementObject();
            }
        }

        public List<ImportRequirementObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _importClassificationRequirementManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ImportRequirementObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ImportRequirementObject>();
            }
        }

        public List<ImportRequirementObject> GetImportRequirements(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _importClassificationRequirementManager.GetImportRequirements(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ImportRequirementObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ImportRequirementObject>();
            }
        }
        
	}

}
