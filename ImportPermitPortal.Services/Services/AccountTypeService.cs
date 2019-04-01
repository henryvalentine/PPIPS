using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class AccountTypeServices
	{
        private readonly AccountTypeManager _accountTypeManager;
        public AccountTypeServices()
		{
            _accountTypeManager = new AccountTypeManager();
		}

        public long AddAccountType(AccountTypeObject accountType)
		{
			try
			{
                return _accountTypeManager.AddAccountType(accountType);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteAccountType(long accountTypeId)
        {
            try
            {
                return _accountTypeManager.DeleteAccountType(accountTypeId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateAccountType(AccountTypeObject accountType)
        {
            try
            {
                return _accountTypeManager.UpdateAccountType(accountType);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<AccountTypeObject> GetAccountTypes()
		{
			try
			{
                var objList = _accountTypeManager.GetAccountTypes();
                if (objList == null || !objList.Any())
			    {
                    return new List<AccountTypeObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<AccountTypeObject>();
			}
		}

        public AccountTypeObject GetAccountType(long accountTypeId)
        {
            try
            {
                return _accountTypeManager.GetAccountType(accountTypeId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new AccountTypeObject();
            }
        }

        public UserProfileObject GetUser(string id)
        {
            try
            {
                return _accountTypeManager.GetUser(id);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new UserProfileObject();
            }
        }

        public List<AccountTypeObject> GetAccountTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _accountTypeManager.GetAccountTypes(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<AccountTypeObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<AccountTypeObject>();
            }
        }

        public List<AccountTypeObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _accountTypeManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<AccountTypeObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<AccountTypeObject>();
            }
        }
	}

}
