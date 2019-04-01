using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class ImportClassServices
	{
        private readonly ImportClassManager _importClassManager;
        public ImportClassServices()
		{
            _importClassManager = new ImportClassManager();
		}

        public long AddImportClass(ImportClassObject importClass)
		{
			try
			{
                return _importClassManager.AddImportClass(importClass);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteImportClass(long importClassId)
        {
            try
            {
                return _importClassManager.DeleteImportClass(importClassId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateImportClass(ImportClassObject importClass)
        {
            try
            {
                return _importClassManager.UpdateImportClass(importClass);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<ImportClassObject> GetImportClasses()
		{
			try
			{
                var objList = _importClassManager.GetImportClasses();
                if (objList == null || !objList.Any())
			    {
                    return new List<ImportClassObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ImportClassObject>();
			}
		}

        public ImportClassObject GetImportClass(long importClassId)
        {
            try
            {
                return _importClassManager.GetImportClass(importClassId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ImportClassObject();
            }
        }

        public UserProfileObject GetUser(string id)
        {
            try
            {
                return _importClassManager.GetUser(id);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new UserProfileObject();
            }
        }

        public List<ImportClassObject> GetImportClasss(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _importClassManager.GetImportClasses(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ImportClassObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ImportClassObject>();
            }
        }

        public List<ImportClassObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _importClassManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ImportClassObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ImportClassObject>();
            }
        }
	}

}
