using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class AspNetUserServices
    {
        private readonly AspNetUserManager _aspNetUserManager;
        public AspNetUserServices()
        {
            _aspNetUserManager = new AspNetUserManager();
        }

        public string AddAspNetUser(AspNetUserObject aspNetUser)
        {
            try
            {
                return _aspNetUserManager.AddAspNetUser(aspNetUser);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return "0";
            }
        }
        
        public string UpdateAspNetUser(AspNetUserObject aspNetUser)
        {
            try
            {
                return _aspNetUserManager.UpdateAspNetUser(aspNetUser);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return "0";
            }
        }


        public long UpdateUser(UserProfileObject person)
        {
            try
            {
                return _aspNetUserManager.UpdateUser(person);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public string GetSecurityStamp(string email, out int code, out long id)
        {
            try
            {
                return _aspNetUserManager.GetSecurityStamp(email, out code, out id);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                code = 0;
                id = 0;
                return "0";
            }
        }

        public long UpdatePassword(string email, string passwordHash)
        {
            try
            {
                return _aspNetUserManager.UpdatePassword(email, passwordHash);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return -2;
            }
        }


        public MessageTemplateObject UpdatePassword(string email, string passwordHash, int msgEventTypeId)
        {
            try
            {
                return _aspNetUserManager.UpdatePassword(email, passwordHash, msgEventTypeId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new MessageTemplateObject();
            }
        }

        public MessageTemplateObject UpdatePassword(long id, string passwordHash, int msgEventTypeId, out string email)
        {
            try
            {
                return _aspNetUserManager.UpdatePassword(id, passwordHash, msgEventTypeId, out email);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                email = "";
                return new MessageTemplateObject();
            }
        }

        public MessageTemplateObject UpdatePasswordByAdmin(long id, string passwordHash, int msgEventTypeId, out string email)
        {
            try
            {
                return _aspNetUserManager.UpdatePasswordByAdmin(id, passwordHash, msgEventTypeId, out email);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                email = "";
                return new MessageTemplateObject();
            }
        }

        public long UpdateUserAndPersonInfo(UserProfileObject user)
        {
            try
            {
                return _aspNetUserManager.UpdateUserAndPersonInfo(user);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddUserAndPersonInfo(UserProfileObject user)
        {
            try
            {
                return _aspNetUserManager.AddUserAndPersonInfo(user);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddBankUser(BankUserObject user)
        {
            return _aspNetUserManager.AddBankUser(user);
        }

        public long AddBankUser2(BankUserObject user)
        {
            return _aspNetUserManager.AddBankUser2(user);
        }

        public long AddBankUserWithBranch(BankUserObject user)
        {
            return _aspNetUserManager.AddBankUserWithBranch(user);
        }

        public long UpdateBankUser(BankUserObject user)
        {
            return _aspNetUserManager.UpdateBankUser(user);
        }


        public long AddPerson(UserProfileObject user)
        {
          return _aspNetUserManager.AddPerson(user);
        }

        public int GetBankId(long importerId)
        {
            return _aspNetUserManager.GetBankId(importerId);
        }

        public long DeletePerson(long id)
        {
            try
            {
                return _aspNetUserManager.DeletePerson(id);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddPersonInfo(PersonObject user)
        {
            return _aspNetUserManager.AddPersonInfo(user);
        }

        public long AddDprUser(PersonObject user)
        {
            return _aspNetUserManager.AddDprUser(user);
        }

        public MessageTemplateObject ActivateAccount(string email, string securityStamp, int msgEventId, long msgId)
        {
            try
            {
                return _aspNetUserManager.ActivateAccount(email, securityStamp, msgEventId, msgId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new MessageTemplateObject();
            }
        }
    }

}
