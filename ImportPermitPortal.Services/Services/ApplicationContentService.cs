using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class ApplicationContentServices
	{
        private readonly ApplicationContentManager _applicationContentManager;
        public ApplicationContentServices()
		{
            _applicationContentManager = new ApplicationContentManager();
		}

        public long AddApplicationContent(ApplicationContentObject applicationContent)
		{
			try
			{
                return _applicationContentManager.AddApplicationContent(applicationContent);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteApplicationContent(long applicationContentId)
        {
            try
            {
                return _applicationContentManager.DeleteApplicationContent(applicationContentId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateApplicationContent(ApplicationContentObject applicationContent)
        {
            try
            {
                return _applicationContentManager.UpdateApplicationContent(applicationContent);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<ApplicationContentObject> GetApplicationContents()
        {
            try
            {
                var objList = _applicationContentManager.GetApplicationContents();
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationContentObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ApplicationContentObject>();
            }
        }

        public ApplicationContentObject GetApplicationContent(long applicationContentId)
        {
            try
            {
                return _applicationContentManager.GetApplicationContent(applicationContentId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationContentObject();
            }
        }

        public List<ApplicationContentObject> GetApplicationContents(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _applicationContentManager.GetApplicationContents(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationContentObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ApplicationContentObject>();
            }
        }

        public List<ApplicationContentObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _applicationContentManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationContentObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ApplicationContentObject>();
            }
        }
	}

}
