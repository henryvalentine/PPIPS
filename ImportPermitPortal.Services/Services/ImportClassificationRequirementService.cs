using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class ImportClassificationRequirementServices
	{
        private readonly ImportClassificationRequirementManager _importClassificationRequirementManager;
        public ImportClassificationRequirementServices()
		{
            _importClassificationRequirementManager = new ImportClassificationRequirementManager();
		}

        public long AddImportClassificationRequirement(List<ImportClassificationRequirementObject> importClassificationRequirements)
		{
			try
			{
                return _importClassificationRequirementManager.AddImportClassificationRequirement(importClassificationRequirements);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteImportClassificationRequirement(long importClassificationRequirementId)
        {
            try
            {
                return _importClassificationRequirementManager.DeleteImportClassificationRequirement(importClassificationRequirementId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateImportClassificationRequirement(ImportClassificationRequirementObject importClassificationRequirement, List<ImportClassificationRequirementObject> importClassificationRequirements)
        {
            try
            {
                return _importClassificationRequirementManager.UpdateImportClassificationRequirement(importClassificationRequirement, importClassificationRequirements);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

       public ImportClassificationRequirementObject GetImportClassificationRequirement(long importClassificationRequirementId)
        {
            try
            {
                return _importClassificationRequirementManager.GetImportClassificationRequirement(importClassificationRequirementId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ImportClassificationRequirementObject();
            }
        }

        public List<ImportClassificationRequirementObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _importClassificationRequirementManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ImportClassificationRequirementObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ImportClassificationRequirementObject>();
            }
        }

        public List<ImportClassificationRequirementObject> GetImportClassificationRequirements(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _importClassificationRequirementManager.GetImportClassificationRequirements(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ImportClassificationRequirementObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ImportClassificationRequirementObject>();
            }
        }
        
	}

}
