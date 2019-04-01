using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class IssueServices
	{
        private readonly IssueManager _issueManager;
        public IssueServices()
		{
            _issueManager = new IssueManager();
		}

        public long AddIssue(IssueObject issue)
		{
			try
			{
                return _issueManager.AddIssue(issue);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteIssue(long issueId)
        {
            try
            {
                return _issueManager.DeleteIssue(issueId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateIssue(IssueObject issue)
        {
            try
            {
                return _issueManager.UpdateIssue(issue);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<IssueObject> GetIssues()
        {
            try
            {
                var objList = _issueManager.GetIssues();
                if (objList == null || !objList.Any())
                {
                    return new List<IssueObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<IssueObject>();
            }
        }

        public IssueObject GetIssue(long issueId)
        {
            try
            {
                return _issueManager.GetIssue(issueId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new IssueObject();
            }
        }

        public List<IssueObject> GetIssues(int? itemsPerPage, int? pageNumber, out int countG)
        {
           return _issueManager.GetIssues(itemsPerPage, pageNumber, out countG);
        }

        public List<IssueObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _issueManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<IssueObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<IssueObject>();
            }
        }
	}

}
