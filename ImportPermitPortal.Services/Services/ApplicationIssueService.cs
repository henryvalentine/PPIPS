using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class ApplicationIssueServices
    {
        private readonly ApplicationIssueManager _applicationIssueManager;
        public ApplicationIssueServices()
        {
            _applicationIssueManager = new ApplicationIssueManager();
        }

        public long AddApplicationIssue(ApplicationIssueObject applicationIssue)
        {
            try
            {
                return _applicationIssueManager.AddApplicationIssue(applicationIssue);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public ApplicationObject GetApplicationFromIssue(long historyId, long userId)
        {
            try
            {
                return _applicationIssueManager.GetApplicationFromIssue(historyId, userId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public long DeleteApplicationIssue(long applicationIssueId)
        {
            try
            {
                return _applicationIssueManager.DeleteApplicationIssue(applicationIssueId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateApplicationIssue(ApplicationIssueObject applicationIssue)
        {
            try
            {
                return _applicationIssueManager.UpdateApplicationIssue(applicationIssue);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<ApplicationIssueObject> GetApplicationIssues()
        {
            try
            {
                var objList = _applicationIssueManager.GetApplicationIssues();
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationIssueObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ApplicationIssueObject>();
            }
        }

        public ApplicationIssueObject GetApplicationIssue(long applicationIssueId)
        {
            try
            {
                return _applicationIssueManager.GetApplicationIssue(applicationIssueId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationIssueObject();
            }
        }

        public List<ApplicationIssueObject> GetApplicationIssues(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _applicationIssueManager.GetApplicationIssues(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationIssueObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ApplicationIssueObject>();
            }
        }

        public List<ApplicationIssueObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _applicationIssueManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationIssueObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ApplicationIssueObject>();
            }
        }



    }

}
