using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
   public class AccountTypeManager
    {

       public long AddAccountType(AccountTypeObject accountType)
       {
           try
           {
               if (accountType == null)
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   if (db.AccountTypes.Count(g => g.Name.ToLower().Trim().Replace(" ", "") == accountType.Name.ToLower().Trim().Replace(" ", "")) > 0)
                   {
                       return -3;
                   }
                   var accountTypeEntity = ModelMapper.Map<AccountTypeObject, AccountType>(accountType);
                   if (accountTypeEntity == null || string.IsNullOrEmpty(accountTypeEntity.Name))
                   {
                       return -2;
                   }
                   var returnStatus = db.AccountTypes.Add(accountTypeEntity);
                   db.SaveChanges();
                   return returnStatus.AccountTypeId;
               }
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
               if (accountType == null)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   if (db.AccountTypes.Count(g => g.Name.ToLower().Trim().Replace(" ", "") == accountType.Name.ToLower().Trim().Replace(" ", "") && g.AccountTypeId != accountType.AccountTypeId) > 0)
                   {
                       return -3;
                   }
                   var accountTypeEntity = ModelMapper.Map<AccountTypeObject, AccountType>(accountType);
                   if (accountTypeEntity == null || accountTypeEntity.AccountTypeId < 1)
                   {
                       return -2;
                   }
                   db.AccountTypes.Attach(accountTypeEntity);
                   db.Entry(accountTypeEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return accountType.AccountTypeId;
               }
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
               using (var db = new ImportPermitEntities())
               {
                   var accountTypes = db.AccountTypes.ToList();
                   if (!accountTypes.Any())
                   {
                        return new List<AccountTypeObject>();
                   }
                   var objList =  new List<AccountTypeObject>();
                   accountTypes.ForEach(app =>
                   {
                       var accountTypeObject = ModelMapper.Map<AccountType, AccountTypeObject>(app);
                       if (accountTypeObject != null && accountTypeObject.AccountTypeId > 0)
                       {
                           objList.Add(accountTypeObject);
                       }
                   });

                   return !objList.Any() ? new List<AccountTypeObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

        public List<AccountTypeObject> GetAccountTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var accountTypes =
                           db.AccountTypes.OrderByDescending(m => m.AccountTypeId)
                               .Skip((tpageNumber)*tsize)
                               .Take(tsize)
                               .ToList();
                       if (accountTypes.Any())
                       {
                           var newList = new List<AccountTypeObject>();
                           accountTypes.ForEach(app =>
                           {
                               var accountTypeObject = ModelMapper.Map<AccountType, AccountTypeObject>(app);
                               if (accountTypeObject != null && accountTypeObject.AccountTypeId > 0)
                               {
                                   newList.Add(accountTypeObject);
                               }
                           });
                           countG = db.AccountTypes.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<AccountTypeObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<AccountTypeObject>();
           }
       }

        
       public AccountTypeObject GetAccountType(long accountTypeId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var accountTypes =
                        db.AccountTypes.Where(m => m.AccountTypeId == accountTypeId)
                            .ToList();
                    if (!accountTypes.Any())
                    {
                        return new AccountTypeObject();
                    }

                    var app = accountTypes[0];
                    var accountTypeObject = ModelMapper.Map<AccountType, AccountTypeObject>(app);
                    if (accountTypeObject == null || accountTypeObject.AccountTypeId < 1)
                    {
                        return new AccountTypeObject();
                    }
                    
                  return accountTypeObject;
                }
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
               using (var db = new ImportPermitEntities())
               {
                   var users =
                       db.AspNetUsers.Where(m => m.Id == id).Include("UserProfile")
                           .ToList();
                   if (!users.Any())
                   {
                       return new UserProfileObject();
                   }

                   var appUser = users[0].UserProfile;
                   var userObject = ModelMapper.Map<UserProfile, UserProfileObject>(appUser);
                   if (userObject == null || userObject.Id < 1)
                   {
                       return new UserProfileObject();
                   }

                   return userObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new UserProfileObject();
           }
       }

       public List<AccountTypeObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var accountTypes =
                       db.AccountTypes.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (accountTypes.Any())
                   {
                       var newList = new List<AccountTypeObject>();
                       accountTypes.ForEach(app =>
                       {
                           var accountTypeObject = ModelMapper.Map<AccountType, AccountTypeObject>(app);
                           if (accountTypeObject != null && accountTypeObject.AccountTypeId > 0)
                           {
                               newList.Add(accountTypeObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<AccountTypeObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<AccountTypeObject>();
           }
       }

       public long DeleteAccountType(long accountTypeId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.AccountTypes.Where(m => m.AccountTypeId == accountTypeId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.AccountTypes.Remove(item);
                   db.SaveChanges();
                   return 5;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
    }
}
