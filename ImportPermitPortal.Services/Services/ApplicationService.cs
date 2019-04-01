using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class ApplicationServices
	{
        private readonly ApplicationManager _importApplicationManager;
        public ApplicationServices()
		{
            _importApplicationManager = new ApplicationManager();
		}

        public long AddApplication(ApplicationObject importApplication)
		{
			try
			{
                return _importApplicationManager.AddApplication(importApplication);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long UpdateApplicationItems(List<ApplicationItemObject> importItems, long applicationId, out  List<DocumentTypeObject> docList)
        {
            return _importApplicationManager.UpdateApplicationItems(importItems, applicationId, out docList);
        }

        public long GetApplicationForRenewal(string code, long id)
        {
            try
            {
                return _importApplicationManager.GetApplicationForRenewal(code, id);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public ApplicationObject GetApplicationProcesses(long applicationId)
        {
            try
            {
                return _importApplicationManager.GetApplicationProcesses(applicationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public long GetApplicationForInclusion(string code, long id)
        {
            try
            {
                return _importApplicationManager.GetApplicationForInclusion(code, id);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public long DeleteApplication(long importApplicationId)
        {
            try
            {
                return _importApplicationManager.DeleteApplication(importApplicationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public long UpdateApplication(ApplicationObject importApplication)
		{
			try
			{
                return _importApplicationManager.UpdateApplication(importApplication);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long UpdateAppPaymentOption(int paymentTypeId, long applicationId)
        {
            try
            {
                return _importApplicationManager.UpdateAppPaymentOption(paymentTypeId, applicationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public ApplicationObject GetApplication(long applicationId)
        {
            try
            {
                return _importApplicationManager.GetApplication(applicationId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public AppInfoObject GetApplicationEmployees(long applicationId)
        {
           return _importApplicationManager.GetApplicationEmployees(applicationId);
        }

        public ApplicationObject GetPaidApplication(long invoiceId)
        {
            try
            {
                return _importApplicationManager.GetPaidApplication(invoiceId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public ApplicationObject GetApplicationDetails(long applicationId)
        {
            try
            {
                return _importApplicationManager.GetApplicationDetails(applicationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public InvoiceObject GetRrrInfo(long applicationId)
        {
            try
            {
                return _importApplicationManager.GetRrrInfo(applicationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new InvoiceObject();
            }
        }
        
        public ApplicationObject GetAppForEdit(long applicationId)
        {
            try
            {
                return _importApplicationManager.GetAppForEdit(applicationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }


        public ApplicationObject GetAppForPayment(long applicationId)
        {
            try
            {
                return _importApplicationManager.GetAppForPayment(applicationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public int UpdateBankAccounts(List<ProductBankerObject> bankers)
        {
            return _importApplicationManager.UpdateBankAccounts(bankers);
        }

        public ApplicationObject GetAppDocuments(long appId)
         {
             try
             {
                 return _importApplicationManager.GetAppDocuments(appId);
             }
             catch (Exception ex)
             {
                 ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                 return new ApplicationObject();
             }
         }

        public ApplicationObject GetAppDocumentsX(long id)
        {
            try
            {
                return _importApplicationManager.GetAppDocumentsX(id);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public bool CheckAppSubmit(long id)
        {
            try
            {
                return _importApplicationManager.CheckAppSubmit(id);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return false;
            }
        }

        public ApplicationObject SubmitApp(long id)
        {
            try
            {
                return _importApplicationManager.SubmitApp(id);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public bool UnSubmitApp(long id)
        {
            try
            {
                return _importApplicationManager.UnSubmitApp(id);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return false;
            }
        }

        public ApplicationObject GetAppDocuments(string code, long companyId)
        {
            try
            {
                return _importApplicationManager.GetAppDocuments(code, companyId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public ApplicationObject GetBankerAppByReference(string code, long importerId)
        {
            try
            {
                return _importApplicationManager.GetBankerAppByReference(code, importerId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public NotificationObject GetAppBankerInfo(long productId, long importerId, string permitValue)
        {
            try
            {
                return _importApplicationManager.GetAppBankerInfo(productId, importerId, permitValue);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public ApplicationObject GetBankAssignedAppDocs(long appId, long companyId)
         {
             try
             {
                 return _importApplicationManager.GetBankAssignedAppDocuments(appId, companyId);

             }
             catch (Exception ex)
             {
                 ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                 return new ApplicationObject();
             }
         }

        public ApplicationObject GetApplicationByRef(string code)
        {
            try
            {
                return _importApplicationManager.GetApplicationByRef(code);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public ApplicationObject GetAppByRef(string code, string rrr)
        {
            try
            {
                return _importApplicationManager.GetAppByRef(code, rrr);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public ApplicationObject GetAppByRef(string code)
        {
            try
            {
                return _importApplicationManager.GetAppByRef(code);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public ApplicationObject GetAppById(long id)
        {
            try
            {
                return _importApplicationManager.GetAppById(id);

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }


        public ItemCountObject GetCounts(long importerId)
        {
            try
            {
                return _importApplicationManager.GetCounts(importerId);
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ItemCountObject();
            }
        }

        public ItemCountObject GetDepotOwnerCounts(long importerId, long userId)
        {
            try
            {
                return _importApplicationManager.GetDepotOwnerCounts(importerId, userId);
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ItemCountObject();
            }
        }

        public ItemCountObject GetVerifierCounts()
        {
           return _importApplicationManager.GetVerifierCounts();
        }

        public ItemCountObject GetBankerCounts(long importerId, long userId)
        {
            try
            {
                return _importApplicationManager.GetBankerCounts(importerId, userId);
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ItemCountObject();
            }
        }

        public List<AppCountObject> GetAdminCounts()
        {
           return _importApplicationManager.GetAdminCounts();
        }
        
        public List<ApplicationObject> GetApplications(int? itemsPerPage, int? pageNumber, out int countG, long companyId)
        {
            try
            {
                var objList = _importApplicationManager.GetApplications(itemsPerPage, pageNumber, out countG, companyId);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ApplicationObject>();
            }
        }

        public List<ProcessTrackingObject> SearchEmployeeAssignedJobProcesses(string searchCriteria, long employeeId)
        {
            return _importApplicationManager.SearchEmployeeAssignedJobProcesses(searchCriteria, employeeId);
        }

        public List<ProcessTrackingObject> GetEmployeeApplicationProcesses(int? itemsPerPage, int? pageNumber, out int countG, long employeeId)
        {
             return _importApplicationManager.GetEmployeeApplicationProcesses(itemsPerPage, pageNumber, out countG, employeeId);
        }

        public List<ProcessTrackingObject> GetEmployeeApplicationProcesses(long employeeId)
        {
            return _importApplicationManager.GetEmployeeApplicationProcesses(employeeId);
        }

        public List<ApplicationObject> GetBankAssignedApplications(int? itemsPerPage, int? pageNumber, out int countG, long companyId)
        {
            try
            {
                var objList = _importApplicationManager.GetBankAssignedApplications(itemsPerPage, pageNumber, out countG, companyId);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ApplicationObject>();
            }
        }

        public List<ApplicationObject> GetBankJobHistory(int? itemsPerPage, int? pageNumber, out int countG, long CompanyId)
        {
            try
            {
                var objList = _importApplicationManager.GetBankJobHistory(itemsPerPage, pageNumber, out countG, CompanyId);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ApplicationObject>();
            }
        }
        
        public List<ApplicationItemObject> GetDepotAssignedApplicationItems(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
               return _importApplicationManager.GetDepotAssignedApplicationItems(itemsPerPage, pageNumber, out countG, importerId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ApplicationItemObject>();
            }
        }

        public List<ApplicationItemObject> GetDepotAssignedApplicationHistory(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
                return _importApplicationManager.GetDepotAssignedApplicationHistory(itemsPerPage, pageNumber, out countG, importerId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ApplicationItemObject>();
            }
        }

       public List<ApplicationItemObject> SearchDepotOwnerApplicationItems(string searchCriteria, long importerId)
        {
            try
            {
                return _importApplicationManager.SearchDepotOwnerApplicationItems(searchCriteria, importerId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ApplicationItemObject>();
            }
        }

       public List<ApplicationItemObject> SearchDepotOwnerApplicationHistory(string searchCriteria, long importerId)
       {
           try
           {
               return _importApplicationManager.SearchDepotOwnerApplicationHistory(searchCriteria, importerId);
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationItemObject>();
           }
       }

       public ApplicationItemObject GetDepotOwnerApplicationItem(long applicationItemId, long importerId)
        {
            try
            {
                return _importApplicationManager.GetDepotOwnerApplicationItem(applicationItemId, importerId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationItemObject();
            }
        }

        public List<ApplicationObject> GetBankUserJobHistory(int? itemsPerPage, int? pageNumber, out int countG, long userId)
        {
            try
            {
                var objList = _importApplicationManager.GetBankUserJobHistory(itemsPerPage, pageNumber, out countG, userId);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ApplicationObject>();
            }
        }

        public List<ApplicationObject> GetApplications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _importApplicationManager.GetApplications(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ApplicationObject>();
            }
        }

        public List<ApplicationObject> GetAdminApplications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _importApplicationManager.GetAdminApplications(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ApplicationObject>();
            }
        }

        public List<JobDistributionObject> GetJobDistributions()
        {
            return _importApplicationManager.GetJobDistributions();
        }

        public List<ApplicationObject> GetApplications()
        {
            try
            {
                var objList = _importApplicationManager.GetApplications();
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ApplicationObject>();
            }
        }
        public List<ApplicationObject> SearchByCompany(string searchCriteria, long CompanyId)
        {
            try
            {
                var objList = _importApplicationManager.SearchByCompany(searchCriteria, CompanyId);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ApplicationObject>();
            }
        }
        public List<ApplicationObject> SearchByBankAssignedApplications(string searchCriteria, long CompanyId)
        {
        try
        {
            var objList = _importApplicationManager.SearchByBankAssignedApplications(searchCriteria, CompanyId);
            if (objList == null || !objList.Any())
            {
                return new List<ApplicationObject>();
            }

            return objList;
        }
        catch (Exception ex)
        {
            ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
            return new List<ApplicationObject>();
        }
    }
        public List<ApplicationObject> SearchBankJobHistory(string searchCriteria, long companyId)
        {
            try
            {
                var objList = _importApplicationManager.SearchBankJobHistory(searchCriteria, companyId);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ApplicationObject>();
            }
        }
        public List<ApplicationObject> SearchBankUserJobHistory(string searchCriteria, long userId)
        {
            try
            {
                var objList = _importApplicationManager.SearchBankUserJobHistory(searchCriteria, userId);
                if (objList == null || !objList.Any())
                {
                    return new List<ApplicationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ApplicationObject>();
            }
        }
        public List<ApplicationObject> Search(string searchCriteria)
        {
            return _importApplicationManager.Search(searchCriteria);
        }

        public List<ApplicationObject> GetApplicationsPendingSubmission(int? itemsPerPage, int? pageNumber, out int countG)
        {
            return _importApplicationManager.GetApplicationsPendingSubmission(itemsPerPage, pageNumber, out countG);
        }
        public List<ApplicationObject> GetPaidApplications(int? itemsPerPage, int? pageNumber, out int countG)
        {
           return _importApplicationManager.GetPaidApplications(itemsPerPage, pageNumber, out countG);
        }
       public List<ApplicationObject> GetSubmittedApplications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            return _importApplicationManager.GetSubmittedApplications(itemsPerPage, pageNumber, out countG);
        }
       public List<ApplicationObject> GetProcessingApplications(int? itemsPerPage, int? pageNumber, out int countG)
       {
          return _importApplicationManager.GetProcessingApplications(itemsPerPage, pageNumber, out countG);
       }
       public List<ApplicationObject> GetApprovedApplications(int? itemsPerPage, int? pageNumber, out int countG)
       {
         return _importApplicationManager.GetApprovedApplications(itemsPerPage, pageNumber, out countG);
       }
       public List<ApplicationObject> GetApplicationsInVerification(int? itemsPerPage, int? pageNumber, out int countG)
       {
           return _importApplicationManager.GetApplicationsInVerification(itemsPerPage, pageNumber, out countG);
       }
       public List<ApplicationObject> GetRejectedApplications(int? itemsPerPage, int? pageNumber, out int countG)
       {
         return _importApplicationManager.GetRejectedApplications(itemsPerPage, pageNumber, out countG);
       }
        public List<ApplicationObject> SearchApplicationsPendingSubmission(string searchCriteria)
        {
            return _importApplicationManager.SearchApplicationsPendingSubmission(searchCriteria);
        }
        public List<ApplicationObject> SearchPaidApplications(string searchCriteria)
        {
            return _importApplicationManager.SearchPaidApplications(searchCriteria);
        }
       public List<ApplicationObject> SearchSubmittedApplications(string searchCriteria)
        {
            return _importApplicationManager.SearchSubmittedApplications(searchCriteria);
        }
       public List<ApplicationObject> SearchProcessingApplications(string searchCriteria)
        {
            return _importApplicationManager.SearchProcessingApplications(searchCriteria);
        }
        public List<ApplicationObject> SearchApprovedApplications(string searchCriteria)
        {
            return _importApplicationManager.SearchApprovedApplications(searchCriteria);
        }
        public List<ApplicationObject> SearchApplicationsInVerification(string searchCriteria)
        {
            return _importApplicationManager.SearchApplicationsInVerification(searchCriteria);
        }
       public List<ApplicationObject> SearchRejectedApplications(string searchCriteria)
       {
           return _importApplicationManager.SearchRejectedApplications(searchCriteria);
       }
	}

}