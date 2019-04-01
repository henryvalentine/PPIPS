using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class IssueLogServices
	{
        private readonly IssueLogManager _issueLogManager;
        public IssueLogServices()
		{
            _issueLogManager = new IssueLogManager();
		}

        public long AddIssueLog(IssueLogObject issueLog)
		{
			try
			{
                return _issueLogManager.AddIssueLog(issueLog);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteIssueLog(long issueLogId)
        {
            try
            {
                return _issueLogManager.DeleteIssueLog(issueLogId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateIssueLog(IssueLogObject issueLog)
        {
            try
            {
                return _issueLogManager.UpdateIssueLog(issueLog);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

     

        public List<IssueLogObject> GetIssueLogs()
        {
            try
            {
                var objList = _issueLogManager.GetIssueLogs();
                if (objList == null || !objList.Any())
                {
                    return new List<IssueLogObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<IssueLogObject>();
            }
        }


     

        public IssueLogObject GetIssueLog(long issueLogId)
        {
            try
            {
                return _issueLogManager.GetIssueLog(issueLogId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new IssueLogObject();
            }
        }

        public List<IssueLogObject> GetIssueLogs(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _issueLogManager.GetIssueLogs(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<IssueLogObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<IssueLogObject>();
            }
        }

        public List<IssueLogObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _issueLogManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<IssueLogObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<IssueLogObject>();
            }
        }
	}

}
