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
   public class BankManager
    {

       public long AddBank(BankObject bank)
       {
           try
           {
               if (bank == null)
               {
                   return -2;
               }

               var bankEntity = ModelMapper.Map<BankObject, Bank>(bank);
               if (bankEntity == null || bankEntity.ImporterId < 1)
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   if (db.Banks.Count(
                           m => m.SortCode.ToLower() == bank.SortCode.ToLower() || m.ImporterId == bank.ImporterId) >
                       0)
                   {
                       return -3;
                   }
                   var returnStatus = db.Banks.Add(bankEntity);
                   db.SaveChanges();
                   return returnStatus.BankId;
               }
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
               if (bankBranch == null || bankBranch.ImporterId < 1)
               {
                   return -2;
               }

               var bankEntity = ModelMapper.Map<BankBranchObject, BankBranch>(bankBranch);
               if (bankEntity == null || string.IsNullOrEmpty(bankEntity.Name) || string.IsNullOrEmpty(bankEntity.BranchCode))
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   if (db.BankBranches.Count(m => m.Name.ToLower().Trim() == bankBranch.Name.ToLower().Trim() && m.BankId == bankBranch.BankId) > 0)
                   {
                       return -3;
                   }
                   var bankId = GetBankId(bankBranch.ImporterId);

                   if (bankId < 1)
                   {
                       return -2;
                   }
                   bankEntity.BankId = bankId;
                   var returnStatus = db.BankBranches.Add(bankEntity);
                   db.SaveChanges();
                   return returnStatus.Id;
               }
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
                if (bankBranch == null || bankBranch.BankId < 1)
                {
                    return -2;
                }

                var bankEntity = ModelMapper.Map<BankBranchObject, BankBranch>(bankBranch);
                if (bankEntity == null || string.IsNullOrEmpty(bankEntity.Name) || string.IsNullOrEmpty(bankEntity.BranchCode))
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    var existings = db.BankBranches.Where(m => m.Name.ToLower().Trim() == bankBranch.Name.ToLower().Trim() && m.BankId == bankBranch.BankId).ToList();
                    if (existings.Any())
                    {
                        return existings[0].Id;
                    }
                    
                    var returnStatus = db.BankBranches.Add(bankEntity);
                    db.SaveChanges();
                    return returnStatus.Id;
                }
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
                if (bankBranch == null || bankBranch.BankId < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    var existings = db.BankBranches.Where(m => m.Id == bankBranch.Id).ToList();
                    if (!existings.Any())
                    {
                        return -2;
                    }
                    var branch = existings[0];
                    branch.Name = bankBranch.Name;
                    branch.BranchCode = bankBranch.BranchCode;
                    db.Entry(branch).State = EntityState.Modified;
                    db.SaveChanges();
                    return branch.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public int GetBankId(long importerId)
       {
           try
           {

               using (var db = new ImportPermitEntities())
               {
                   var banks = db.Banks.Where(b => b.ImporterId == importerId).ToList();
                   if (!banks.Any())
                   {
                       return 0;
                   }
                   return banks[0].BankId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateBankBranch(BankBranchObject bankBranch)
       {
           try
           {
               if (bankBranch == null)
               {
                   return -2;
               }

               var bankEntity = ModelMapper.Map<BankBranchObject, BankBranch>(bankBranch);
               if (bankEntity == null || bankEntity.BankId < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   if (db.BankBranches.Count(m => m.Name.ToLower().Trim() == bankBranch.Name.ToLower().Trim() && m.BankId == bankBranch.BankId && m.Id != bankBranch.Id) > 0)
                   {
                       return -3;
                   }
                   db.BankBranches.Attach(bankEntity);
                   db.Entry(bankEntity).State = EntityState.Modified;
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

       public long UpdateBank(BankObject bank)
       {
           try
           {
               if (bank == null)
               {
                   return -2;
               }

               var bankEntity = ModelMapper.Map<BankObject, Bank>(bank);
               if (bankEntity == null || bankEntity.BankId < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   if (db.Banks.Count(m => (m.SortCode.ToLower() == bank.SortCode.ToLower() || m.Name.ToLower().Trim() == bank.Name.ToLower().Trim()) && m.BankId != bank.BankId) > 0)
                   {
                       return -3;
                   }
                   db.Banks.Attach(bankEntity);
                   db.Entry(bankEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return bank.BankId;
               }
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
               using (var db = new ImportPermitEntities())
               {
                   var banks = db.Banks.OrderBy(m => m.Name).ToList();
                   if (!banks.Any())
                   {
                        return new List<BankObject>();
                   }
                   var objList =  new List<BankObject>();
                   banks.ForEach(app =>
                   {
                       var bankObject = ModelMapper.Map<Bank, BankObject>(app);
                       if (bankObject != null && bankObject.BankId > 0)
                       {
                           objList.Add(bankObject);
                       }
                   });

                   return !objList.Any() ? new List<BankObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

       public List<BankObject> GetBanks(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var banks = db.Banks.OrderByDescending(m => m.BankId).Include("Importer").Skip(tpageNumber).Take(tsize).ToList();
                   
                   if (!banks.Any())
                   {
                       countG = 0;
                       return new List<BankObject>();
                   }
                   
                   var newList = new List<BankObject>();
                   banks.ForEach(h =>
                   {
                       var bnk = new BankObject
                       {
                           BankId = h.BankId,
                           ImporterId = h.ImporterId,
                           SortCode = h.SortCode,
                           Name = h.Name,
                           NotificationEmail = h.NotificationEmail
                       };

                       var usrs = (from pp in db.People.Where(n => n.ImporterId == h.ImporterId)
                                   join tt in db.UserProfiles.Include("AspNetUsers") on pp.Id equals tt.PersonId where tt.IsAdmin
                                       select new {tt, pp}).ToList();
                       if (usrs.Any())
                       {
                           var usr = usrs[0].tt;
                           var p = usrs[0].pp;
                           if (usr != null && usr.Id > 0 && p.Id > 0)
                           {
                               bnk.UserId = usr.AspNetUsers.ElementAt(0).Id;
                               bnk.LastName = p.FirstName + " " + p.LastName;
                               bnk.PhoneNumber = usr.AspNetUsers.ElementAt(0).PhoneNumber;
                           }
                       }
                       else
                       {
                           bnk.UserId = "";
                           bnk.PhoneNumber = "";
                           bnk.LastName = "";
                       }

                       newList.Add(bnk);
                   });

                   countG = db.Banks.Count();
                   return newList;
                       
                   }
                   
               }
               countG = 0;
               return new List<BankObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<BankObject>();
           }
       }

       public List<BankBranchObject> GetBankBranches(int? itemsPerPage, int? pageNumber, out int countG, long impoterId)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var banks = db.Banks.Where(b => b.ImporterId == impoterId).ToList();

                       if (!banks.Any())
                       {
                           countG = 0;
                           return new List<BankBranchObject>();
                       }
                       var bank = banks[0];

                       var bankBranches = db.BankBranches.Where(b => b.BankId == bank.BankId).OrderByDescending(m => m.Name).Skip(tpageNumber).Take(tsize).ToList();

                       if (!bankBranches.Any())
                       {
                           countG = 0;
                           return new List<BankBranchObject>();
                       }

                       var newList = new List<BankBranchObject>();
                       bankBranches.ForEach(h =>
                       {
                           var bnk = new BankBranchObject
                           {
                               BankId = h.BankId,
                               BranchCode = h.BranchCode,
                               Name = h.Name,
                               Id = h.Id
                           };

                           newList.Add(bnk);
                       });

                       countG = (from bnk in db.Banks.Where(b => b.ImporterId == impoterId)
                                 join bnkBrnch in db.BankBranches
                                 on bnk.BankId equals bnkBrnch.BankId
                                 select bnkBrnch).Count();
                       return newList;
                   }
               }
               countG = 0;
               return new List<BankBranchObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<BankBranchObject>();
           }
       }
     
       public BankObject GetBank(long bankId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var banks = db.Banks.Where(m => m.BankId == bankId).Include("Importer").ToList();
                    if (!banks.Any())
                    {
                        return new BankObject();
                    }

                    var app = banks[0];
                    var bankObject = ModelMapper.Map<Bank, BankObject>(app);
                    if (bankObject == null || bankObject.BankId < 1)
                    {
                        return new BankObject();
                    }

                    bankObject.ImporterObject = new ImporterObject
                    {
                        Name = app.Importer.Name,
                        TIN = app.Importer.TIN,
                        RCNumber = app.Importer.RCNumber
                    };
                  return bankObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new BankObject();
           }
       }

       public BankBranchObject GetBankBranch(long bankBranchId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var bankBranches = db.BankBranches.Where(m => m.Id == bankBranchId).ToList();
                   if (!bankBranches.Any())
                   {
                       return new BankBranchObject();
                   }

                   var app = bankBranches[0];
                   var bankObject = ModelMapper.Map<BankBranch, BankBranchObject>(app);
                   if (bankObject == null || bankObject.Id < 1)
                   {
                       return new BankBranchObject();
                   }
                   
                   return bankObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new BankBranchObject();
           }
       }

       public List<BankBranchObject> GetBankBranches(long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                 var branches =  (from bnk in db.Banks.Where(b => b.ImporterId == importerId)
                    join bnkBrnch in db.BankBranches.OrderByDescending(m => m.BankId)
                    on bnk.BankId equals bnkBrnch.BankId
                    select bnkBrnch).ToList();

                   if (!branches.Any())
                   {
                       return new List<BankBranchObject>();
                   }
                  
                   var newList = new List<BankBranchObject>();
                  branches.ForEach(v =>
                  {
                      var bankObject = ModelMapper.Map<BankBranch, BankBranchObject>(v);
                      if (bankObject != null && bankObject.Id > 0)
                      {
                          bankObject.Name = bankObject.Name + " (" + bankObject.BranchCode + ")";
                          newList.Add(bankObject);
                      }
                  });

                  return newList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<BankBranchObject>();
           }
       }

       public UserProfileObject GetBankAdmin(int bankId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var bankUser = new UserProfileObject();
                   var bankAdmins = (from bk in
                       db.Banks.Where(m => m.BankId == bankId)
                       join cm in db.Importers on bk.ImporterId equals cm.Id
                       join cp in db.People on cm.Id equals cp.ImporterId
                       join us in db.UserProfiles.Where(n => n.IsAdmin) on cp.Id equals us.PersonId
                       join id in db.AspNetUsers on us.Id equals id.UserInfo_Id
                       select new UserProfileObject
                       {
                            Id = us.Id,
                            BankId = bk.BankId,
                            UserId = id.Id,
                            PhoneNumber = id.PhoneNumber,
                            CompanyId = cm.Id,
                            PersonId = cp.Id,
                            FirstName = cp.FirstName,
                            LastName = cp.LastName,
                            Email = id.Email,
                            IsActive = us.IsActive
                       }).ToList();

                   if (!bankAdmins.Any())
                   {
                       var banks = (from bk in db.Banks.Where(m => m.BankId == bankId)
                           join cm in db.Importers on bk.ImporterId equals cm.Id
                           select bk).ToList();

                       if (!banks.Any())
                       {
                           return new UserProfileObject();
                       }

                       bankUser.BankId = banks[0].BankId;
                       bankUser.CompanyId = banks[0].ImporterId;
                   }
                   else
                   {
                       bankUser = bankAdmins[0];
                       var branchUsers = db.BankUsers.Where(b => b.UserId == bankUser.Id).Include("BankBranch").ToList();
                       if (branchUsers.Any())
                       {
                           var branch = branchUsers[0];
                           bankUser.BranchCode = branch.BankBranch.BranchCode;
                           bankUser.BankBranchName = branch.BankBranch.Name;
                           bankUser.BranchId = branch.BranchId;
                       }
                   }
                   
                    if (bankUser.BankId < 1)
                    {
                        return new UserProfileObject();
                    }

                    return bankUser;
               }  
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new UserProfileObject();
           }
       }

       public UserProfileObject GetBankUser(long userId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var bankUsers = (from us  in db.UserProfiles.Where(m => m.Id == userId)
                                    join cp in db.People on us.PersonId equals cp.Id
                                    join bnkusr in db.BankUsers on us.Id equals bnkusr.UserId
                                    join sp in db.AspNetUsers on us.Id equals sp.UserInfo_Id
                                    select new UserProfileObject
                                    {
                                        Id = us.Id,
                                        UserId = sp.Id,
                                        PersonId = cp.Id,
                                        PhoneNumber = sp.PhoneNumber,
                                        FirstName = cp.FirstName,
                                        LastName = cp.LastName,
                                        Email = sp.Email,
                                        BranchId = bnkusr.BranchId,
                                        IsActive = us.IsActive
                                    }).ToList();
                   return !bankUsers.Any() ? new UserProfileObject() : bankUsers[0];
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new UserProfileObject();
           }
       }

       public List<UserProfileObject> GetBankUsers(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                   {
                       var tpageNumber = (int) pageNumber;
                       var tsize = (int) itemsPerPage;

                       var bankUsers = (from cp in db.People.Where(a => a.ImporterId == importerId).OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                                        join us in db.UserProfiles.Include("AspNetUsers").Include("Person") on cp.Id equals us.PersonId
                                        select us).ToList();

                       if (!bankUsers.Any())
                       {
                           countG = 0;
                           return new List<UserProfileObject>();
                       }
                       var newList = new List<UserProfileObject>();
                       bankUsers.ForEach(l =>
                       {
                           var sp = l.AspNetUsers.ToList()[0];
                           var cp = l.Person;

                           newList.Add(new UserProfileObject
                           {
                               Id = l.Id,
                               UserId = sp.Id,
                               PhoneNumber = sp.PhoneNumber,
                               FirstName = cp.FirstName,
                               LastName = cp.LastName,
                               Email = sp.Email,
                               IsActive = l.IsActive
                           });
                       });
                       
                       countG = (from cp in db.People.Where(a => a.ImporterId == importerId)
                                 join us in db.UserProfiles.Where(k => k.IsAdmin == false) on cp.Id equals us.PersonId
                                 select us).ToList().Count;

                       newList.ForEach(v =>
                       {
                           v.StatusStr = v.IsActive ? "Active" : "Inactivated";
                       });

                       return newList;
                   }

                   countG = 0;
                   return new List<UserProfileObject>();
               }
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
               using (var db = new ImportPermitEntities())
               {
                   var bankUsers =(from apb in  db.Banks.Where(a => a.BankId == bankId)
                        join cm in db.Importers on apb.ImporterId equals cm.Id
                        join cp in db.People on cm.Id equals cp.ImporterId
                        join us in db.UserProfiles.Where(k => !k.IsAdmin)
                        on cp.Id equals us.PersonId
                        join sp in db.AspNetUsers on us.Id equals sp.UserInfo_Id

                        select new UserProfileObject
                        {
                            Id = us.Id,
                            UserId = sp.Id,
                            PhoneNumber = sp.PhoneNumber,
                            FirstName = cp.FirstName,
                            LastName = cp.LastName,
                            Email = sp.Email,
                            IsActive = us.IsActive
                        }).ToList();
                   if (!bankUsers.Any())
                   {
                       return new List<UserProfileObject>();
                   }

                   return bankUsers;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<UserProfileObject>();
           }
       }
       
       public List<UserProfileObject> SearchUsers(string searchCriteria, long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var bankUsers = (from cmp in db.Importers.Where(h => h.Id == importerId)
                                  join cp in db.People.Where(a => a.LastName.ToLower().Trim().Contains(searchCriteria.Trim().ToLower()) || a.FirstName.Trim().ToLower().Contains(searchCriteria.Trim().ToLower()))
                                  on cmp.Id equals cp.ImporterId
                                  join us in db.UserProfiles
                                  on cp.Id equals us.PersonId
                                  join sp in db.AspNetUsers on us.Id equals sp.UserInfo_Id

                        select new UserProfileObject
                        {
                            Id = us.Id,
                            UserId = sp.Id,
                            PhoneNumber = sp.PhoneNumber,
                            FirstName = cp.FirstName,
                            LastName = cp.LastName,
                            Email = sp.Email,
                            IsActive = us.IsActive,
                            StatusStr = us.IsActive? "Active" : "Inactive"
                        }).ToList();
                         
                   if (!bankUsers.Any())
                   {
                       return new List<UserProfileObject>();
                   }
                   return bankUsers;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<UserProfileObject>();
           }
       }

       public UserProfileObject GetUserByLogin(string userId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var users = (from usr in db.AspNetUsers.Where(m => m.Id == userId)
                                join sp in db.UserProfiles on usr.UserInfo_Id equals sp.Id
                                join cp in db.People on sp.PersonId equals cp.Id
                                    select new UserProfileObject
                                    {
                                        Id = sp.Id,
                                        FirstName = cp.FirstName,
                                        LastName = cp.LastName,
                                        IsActive = sp.IsActive,
                                    }).ToList();
                   return !users.Any() ? new UserProfileObject() : users[0];
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new UserProfileObject();
           }
       }

       public List<BankObject> Search(string searchCriteria)
       {
           try  //where us.IsAdmin
           {
               using (var db = new ImportPermitEntities())
               {
                   var banks = (from bk in
                        db.Banks.Where(m => m.SortCode.Trim().Contains(searchCriteria.Trim()) || m.Name.ToLower().Trim().Contains(searchCriteria.ToLower().Trim()))
                       join cm in db.Importers on bk.ImporterId equals cm.Id
                       join cp in db.People on cm.Id equals cp.ImporterId
                       join us in db.UserProfiles on cp.Id equals us.PersonId
                       join id in db.AspNetUsers on us.Id equals id.UserInfo_Id
                       select new BankObject
                       {
                            BankId = bk.BankId,
                            UserId = id.Id,
                            PhoneNumber = id.PhoneNumber,
                            FirstName = cp.FirstName,
                            LastName = cp.LastName,
                            ImporterId = cm.Id,
                            Name = cm.Name,
                            TIN = cm.TIN,
                            RCNumber = cm.RCNumber,
                            NotificationEmail = bk.NotificationEmail
                       }).ToList();
                    
                   if (!banks.Any())
                   {
                       return new List<BankObject>();
                   }
                   return banks;
               }
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<BankObject>();
           }
       }

       public List<BankBranchObject> SearchBankBranches(string searchCriteria, long impoterId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var bankBranches = (from bnk in db.Banks.Where(b => b.ImporterId == impoterId)
                    join bnkBrnch in db.BankBranches.Where(m => m.Name.Trim().Contains(searchCriteria.Trim()) || m.Name.ToLower().Trim().Contains(searchCriteria.ToLower().Trim()) || m.BranchCode.ToLower().Trim().Contains(searchCriteria.ToLower().Trim()))
                    on bnk.BankId equals bnkBrnch.BankId

                    select new BankBranchObject
                    {
                        BankId = bnkBrnch.BankId,
                        Id = bnkBrnch.Id,
                        BranchCode = bnkBrnch.BranchCode,
                        Name = bnkBrnch.Name
                    }).ToList();

                   if (!bankBranches.Any())
                   {
                       return new List<BankBranchObject>();
                   }

                   return bankBranches;
               }
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<BankBranchObject>();
           }
       }

       public long DeleteBank(long bankId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.Banks.Where(m => m.BankId == bankId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.Banks.Remove(item);
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
