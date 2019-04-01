using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class ApplicationItemServices
	{
        private readonly ApplicationItemManager _importItemManager;
        public ApplicationItemServices()
		{
            _importItemManager = new ApplicationItemManager();
		}

        public long AddApplicationItem(ApplicationItemObject importItem)
		{
			try
			{
                return _importItemManager.AddApplicationItem(importItem);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long AddApplicationItems(List<ApplicationItemObject> importItems, long applicationId)
        {
            try
            {
                return _importItemManager.AddApplicationItems(importItems, applicationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateApplicationItems(List<ApplicationItemObject> importItems, List<ApplicationItemObject> oldImportItems, long applicationId)
        {
            try
            {
                return _importItemManager.UpdateApplicationItems(importItems, oldImportItems, applicationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteApplicationItem(long importItemId)
        {
            try
            {
                return _importItemManager.DeleteApplicationItem(importItemId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteApplicationItems(long applicationId)
        {
            try
            {
                return _importItemManager.DeleteApplicationItems(applicationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateApplicationItem(ApplicationItemObject importItem)
        {
            try
            {
                return _importItemManager.UpdateApplicationItem(importItem);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<ApplicationItemObject> GetApplicationItems()
		{
			try
			{
                var objList = _importItemManager.GetApplicationItems();
                if (objList == null || !objList.Any())
			    {
                    return new List<ApplicationItemObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ApplicationItemObject>();
			}
		}

        public ApplicationItemObject GetApplicationItem(long importItemId)
        {
            try
            {
                return _importItemManager.GetApplicationItem(importItemId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationItemObject();
            }
        }

        public ApplicationItemObject GetPreviousDischargeInfo(long importApplicationId, long productId, out double tolerancePercentage)
        {
            try
            {
                return _importItemManager.GetPreviousDischargeInfo(importApplicationId, productId, out tolerancePercentage);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                tolerancePercentage = 0;
                return new ApplicationItemObject();
            }
        }

        public List<ApplicationItemObject> GetApplicationItemsByApplicationApplication(int? itemsPerPage, int? pageNumber, out int countG, long importApplicationId)
        {
            try
            {
                var objList = _importItemManager.GetApplicationItemsByApplication(itemsPerPage, pageNumber, out countG, importApplicationId);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationItemObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ApplicationItemObject>();
            }
        }

        public List<ApplicationItemObject> GetApplicationItemsByApplicationApplication(long importApplicationId)
        {
            try
            {
                var objList = _importItemManager.GetApplicationItemsByApplication(importApplicationId);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationItemObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ApplicationItemObject>();
            }
        }

        public List<ApplicationItemObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _importItemManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationItemObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ApplicationItemObject>();
            }
        }
	}

}
