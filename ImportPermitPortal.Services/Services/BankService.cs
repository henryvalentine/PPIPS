using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class BankServices
	{
        private readonly BankManager _bankManager;
        public BankServices()
		{
            _bankManager = new BankManager();
		}

        public long AddBank(BankObject bank)
		{
			try
			{
                return _bankManager.AddBank(bank);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long AddBankBranch(BankBranchObject bankBranch) 
        {
			try
			{
                return _bankManager.AddBankBranch(bankBranch);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public int AddBankBranchForReg(BankBranchObject bankBranch)
        {
            try
            {
                return _bankManager.AddBankBranchForReg(bankBranch);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public int UpdateBankBranchForReg(BankBranchObject bankBranch)
        {
            try
            {
                return _bankManager.UpdateBankBranchForReg(bankBranch);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public BankBranchObject GetBankBranch(long bankBranchId)
        {
            try
            {
                return _bankManager.GetBankBranch(bankBranchId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new BankBranchObject();
            }
         }

         public List<BankBranchObject> SearchBankBranches(string searchCriteria, long impoterId)
         {
            try
            {
                return _bankManager.SearchBankBranches(searchCriteria, impoterId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<BankBranchObject>();
            }
         }

         public List<BankBranchObject> GetBankBranches(long importerId)
         {
             try
             {
                 return _bankManager.GetBankBranches(importerId);

             }
             catch (Exception ex)
             {
                 ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                 return new List<BankBranchObject>();
             }
         }

         public List<BankBranchObject> GetBankBranches(int? itemsPerPage, int? pageNumber, out int countG, long impoterId)
         {
             return _bankManager.GetBankBranches(itemsPerPage, pageNumber, out countG, impoterId);
         }
        public long UpdateBankBranch(BankBranchObject bankBranch)
        {
            try
            {
                return _bankManager.UpdateBankBranch(bankBranch);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteBank(long bankId)
        {
            try
            {
                return _bankManager.DeleteBank(bankId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateBank(BankObject bank)
        {
            try
            {
                return _bankManager.UpdateBank(bank);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<BankObject> GetBanks()
		{
			try
			{
                var objList = _bankManager.GetBanks();
                if (objList == null || !objList.Any())
			    {
                    return new List<BankObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<BankObject>();
			}
		}

        public BankObject GetBank(long bankId)
        {
            try
            {
                return _bankManager.GetBank(bankId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new BankObject();
            }
        }

        public UserProfileObject GetBankUser(long userId)
        {
            try
            {
                return _bankManager.GetBankUser(userId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new UserProfileObject();
            }
        }

        public List<UserProfileObject> GetBankUsers(int? itemsPerPage, int? pageNumber, out int countG, long CompanyId)
        {
            try
            {
                return _bankManager.GetBankUsers(itemsPerPage, pageNumber, out countG, CompanyId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<UserProfileObject>();
            }
        }

        public List<UserProfileObject> GetBankUsersByBank(int bankId)
        {
            try
            {
                return _bankManager.GetBankUsersByBank(bankId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<UserProfileObject>();
            }
        }
       

        public List<UserProfileObject> SearchUsers(string searchCriteria, long CompanyId)
        {
            try
            {
                return _bankManager.SearchUsers(searchCriteria, CompanyId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<UserProfileObject>();
            }
        }
        public UserProfileObject GetBankAdmin(int bankId)
        {
            try
            {
                return _bankManager.GetBankAdmin(bankId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new UserProfileObject();
            }
        }


        public UserProfileObject GetUserByLogin(string userId)
        {
            try
            {
                return _bankManager.GetUserByLogin(userId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new UserProfileObject();
            }
        }

        public List<BankObject> GetBanks(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _bankManager.GetBanks(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<BankObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<BankObject>();
            }
        }

        public List<BankObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _bankManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<BankObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<BankObject>();
            }
        }
        
	}

}
