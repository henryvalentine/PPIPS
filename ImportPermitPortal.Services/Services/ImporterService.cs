using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class ImporterServices
	{
        private readonly ImporterManager _importerManager;
        public ImporterServices()
		{
            _importerManager = new ImporterManager();
		}

        public long AddImporter(ImporterObject importer)
		{
			try
			{
                return _importerManager.AddImporter(importer);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long AddImporterAndPerson(ImporterObject importer, PersonObject person, out long importerId)
        {
            try
            {
                return _importerManager.AddImporterAndPerson(importer, person, out importerId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                importerId = 0;
                return 0;
            }
        }


        public long AddImporterDepotAndPerson(UserProfileObject person)
        {
            try
            {
                return _importerManager.AddImporterDepotAndPerson(person);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateImporterDepotAndPerson(UserProfileObject person)
        {
            try
            {
                return _importerManager.UpdateImporterDepotAndPerson(person);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteImporter(long importerId)
        {
            try
            {
                return _importerManager.DeleteImporter(importerId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateImporter(ImporterObject importer)
        {
            try
            {
                return _importerManager.UpdateImporter(importer);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<ImporterObject> GetCompanies()
		{
			try
			{
                var objList = _importerManager.GetImporters();
                if (objList == null || !objList.Any())
			    {
                    return new List<ImporterObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ImporterObject>();
			}
		}

        public ImporterObject GetImporter(long importerId)
        {
            try
            {
                return _importerManager.GetImporter(importerId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ImporterObject();
            }
        }

        public ImporterObject GetImporterByLoggedOnUser(string userId, bool isApplicant)
        {
            try
            {
                return _importerManager.GetImporterByLoggedOnUser(userId, isApplicant);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ImporterObject();
            }
        }

        public ImporterObject GetAdminImporterUser(string userId)
        {
            try
            {
                return _importerManager.GetAdminImporterUser(userId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ImporterObject();
            }
        }

        public List<ImporterObject> GetCompanies(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _importerManager.GetImporters(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ImporterObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ImporterObject>();
            }
        }

        public List<ImporterObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _importerManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ImporterObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ImporterObject>();
            }
        }
	}

}
