using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class IssueTypeServices
	{
        private readonly IssueTypeManager _issueTypeManager;
        public IssueTypeServices()
		{
            _issueTypeManager = new IssueTypeManager();
		}

        public long AddIssueType(IssueTypeObject issueType)
		{
			try
			{
                return _issueTypeManager.AddIssueType(issueType);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteIssueType(long issueTypeId)
        {
            try
            {
                return _issueTypeManager.DeleteIssueType(issueTypeId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateIssueType(IssueTypeObject issueType)
        {
            try
            {
                return _issueTypeManager.UpdateIssueType(issueType);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

     

        public List<IssueTypeObject> GetIssueTypes()
        {
            try
            {
                var objList = _issueTypeManager.GetIssueTypes();
                if (objList == null || !objList.Any())
                {
                    return new List<IssueTypeObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<IssueTypeObject>();
            }
        }


     

        public IssueTypeObject GetIssueType(long issueTypeId)
        {
            try
            {
                return _issueTypeManager.GetIssueType(issueTypeId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new IssueTypeObject();
            }
        }

        public List<IssueTypeObject> GetIssueTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _issueTypeManager.GetIssueTypes(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<IssueTypeObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<IssueTypeObject>();
            }
        }

        public List<IssueTypeObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _issueTypeManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<IssueTypeObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<IssueTypeObject>();
            }
        }
	}

}
