using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class IssueCategoryServices
	{
        private readonly IssueCategoryManager _issueCategoryManager;
        public IssueCategoryServices()
		{
            _issueCategoryManager = new IssueCategoryManager();
		}

        public long AddIssueCategory(IssueCategoryObject issueCategory)
		{
			try
			{
                return _issueCategoryManager.AddIssueCategory(issueCategory);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteIssueCategory(long issueCategoryId)
        {
            try
            {
                return _issueCategoryManager.DeleteIssueCategory(issueCategoryId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateIssueCategory(IssueCategoryObject issueCategory)
        {
            try
            {
                return _issueCategoryManager.UpdateIssueCategory(issueCategory);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<IssueCategoryObject> GetIssueCategories()
        {
            try
            {
                var objList = _issueCategoryManager.GetIssueCategories();
                if (objList == null || !objList.Any())
                {
                    return new List<IssueCategoryObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<IssueCategoryObject>();
            }
        }

        public IssueCategoryObject GetIssueCategory(long issueCategoryId)
        {
            try
            {
                return _issueCategoryManager.GetIssueCategory(issueCategoryId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new IssueCategoryObject();
            }
        }

        public List<IssueCategoryObject> GetIssueCategories(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _issueCategoryManager.GetIssueCategories(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<IssueCategoryObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<IssueCategoryObject>();
            }
        }

        public List<IssueCategoryObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _issueCategoryManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<IssueCategoryObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<IssueCategoryObject>();
            }
        }
	}

}
