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
    public class AspNetUserManager
    {
        public string AddAspNetUser(AspNetUserObject aspNetUser)
        {
            try
            {
                if (aspNetUser == null)
                {
                    return "-2";
                }

                var aspNetUserEntity = ModelMapper.Map<AspNetUserObject, AspNetUser>(aspNetUser);
                if (aspNetUserEntity == null || string.IsNullOrEmpty(aspNetUserEntity.Email))
                {
                    return "-2";
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.AspNetUsers.Add(aspNetUserEntity);
                    db.SaveChanges();
                    return returnStatus.Id;
                }
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
                if (aspNetUser == null)
                {
                    return "-2";
                }

                var aspNetUserEntity = ModelMapper.Map<AspNetUserObject, AspNetUser>(aspNetUser);
                if (aspNetUserEntity == null || string.IsNullOrEmpty(aspNetUserEntity.Id))
                {
                    return "-2";
                }

                using (var db = new ImportPermitEntities())
                {
                    db.AspNetUsers.Attach(aspNetUserEntity);
                    db.Entry(aspNetUserEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return aspNetUser.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return "0";
            }
        }

        public string GetSecurityStamp(string email, out int code, out long id)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    code = 0;
                    id = 0;
                    return "";
                }

                using (var db = new ImportPermitEntities())
                {
                    var usps = db.AspNetUsers.Where(e => e.Email == email).ToList();
                    if (!usps.Any())
                    {
                        code = 0;
                        id = 0;
                        return "";
                    }

                    var usp = usps[0];
                    var security = new Guid().ToString();
                    usp.SecurityStamp = security;
                    db.Entry(usp).State = EntityState.Modified;
                    db.SaveChanges();

                    code = 5;
                    if (usp.UserInfo_Id == null)
                    {
                        code = 0;
                        id = 0;
                        return "";
                    }
                    id = (long)usp.UserInfo_Id;
                    return security;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                code = 0;
                id = 0;
                return "";
            }
        }

        public long UpdateUser(UserProfileObject person)
        {
            try
            {
                if (person == null)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {

                    var existingp = db.UserProfiles.Where(i => i.PersonId == person.PersonId).Include("Person").ToList();
                    if (!existingp.Any())
                    {
                        return -2;
                    }

                    var prs = existingp[0];
                    prs.IsActive = person.IsActive;
                    prs.Person.FirstName = person.FirstName;
                    prs.Person.LastName = person.LastName;

                    db.Entry(prs).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(prs.Person).State = EntityState.Modified;
                    db.SaveChanges();

                    return prs.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdatePassword(string email, string passwordHash)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(passwordHash))
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    var usps = db.AspNetUsers.Where(e => e.Email == email).ToList();
                    if (!usps.Any())
                    {
                      return -2;
                    }

                    var usp = usps[0];
                    var security = new Guid().ToString();
                    usp.SecurityStamp = security;
                    usp.PasswordHash = passwordHash;
                    db.Entry(usp).State = EntityState.Modified;
                    db.SaveChanges();
                    if (usp.UserInfo_Id == null)
                    {
                        return -2;
                    }
                   return (long)usp.UserInfo_Id;
                }
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
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(passwordHash))
                {
                    return new MessageTemplateObject();
                }

                using (var db = new ImportPermitEntities())
                {
                    var usps = db.AspNetUsers.Where(e => e.Email == email).ToList();
                    if (!usps.Any())
                    {
                        return new MessageTemplateObject();
                    }

                    var templates = db.MessageTemplates.Where(m => m.EventTypeId == msgEventTypeId).ToList();
                    if (!templates.Any())
                    {
                        return new MessageTemplateObject();
                    }

                    var app = templates[0];
                    var messageTemplateObject = ModelMapper.Map<MessageTemplate, MessageTemplateObject>(app);
                    if (messageTemplateObject == null || messageTemplateObject.Id < 1)
                    {
                        return new MessageTemplateObject();
                    }

                    var usp = usps[0];
                    if (usp.UserInfo_Id == null)
                    {
                        return new MessageTemplateObject();
                    }
                    var security = new Guid().ToString();
                    usp.SecurityStamp = security;
                    usp.PasswordHash = passwordHash;
                    db.Entry(usp).State = EntityState.Modified;
                    db.SaveChanges();

                    messageTemplateObject.UserId = (long) usp.UserInfo_Id;
                    return messageTemplateObject;
                }
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
                if (id < 1)
                {
                    email = "";
                    return new MessageTemplateObject();
                }
                using (var db = new ImportPermitEntities())
                {
                    var usps = db.AspNetUsers.Where(e => e.UserInfo_Id == id).ToList();
                    if (!usps.Any())
                    {
                        email = "";
                        return new MessageTemplateObject();
                    }

                    var templates = db.MessageTemplates.Where(m => m.EventTypeId == msgEventTypeId).ToList();
                    if (!templates.Any())
                    {
                        email = "";
                        return new MessageTemplateObject();
                    }

                    var app = templates[0];
                    var messageTemplateObject = ModelMapper.Map<MessageTemplate, MessageTemplateObject>(app);
                    if (messageTemplateObject == null || messageTemplateObject.Id < 1)
                    {
                        email = "";
                        return new MessageTemplateObject();
                    }

                    var usp = usps[0];
                    if (usp.UserInfo_Id == null)
                    {
                        email = "";
                        return new MessageTemplateObject();
                    }
                    var security = new Guid().ToString();
                    usp.SecurityStamp = security;
                    usp.PasswordHash = passwordHash;
                    db.Entry(usp).State = EntityState.Modified;
                    db.SaveChanges();

                    messageTemplateObject.UserId = (long) usp.UserInfo_Id;
                    email = usp.Email;
                    return messageTemplateObject;
                }
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
                if (id < 1)
                {
                    email = "";
                    return new MessageTemplateObject();
                }
                using (var db = new ImportPermitEntities())
                {
                    var usps = (from em in db.EmployeeDesks.Where(e => e.Id == id)
                               join p in db.UserProfiles on em.EmployeeId equals  p.Id
                               join asp in db.AspNetUsers on p.Id equals  asp.UserInfo_Id
                               select asp).ToList();

                    if (!usps.Any())
                    {
                        email = "";
                        return new MessageTemplateObject();
                    }

                    var templates = db.MessageTemplates.Where(m => m.EventTypeId == msgEventTypeId).ToList();
                    if (!templates.Any())
                    {
                        email = "";
                        return new MessageTemplateObject();
                    }

                    var app = templates[0];
                    var messageTemplateObject = ModelMapper.Map<MessageTemplate, MessageTemplateObject>(app);
                    if (messageTemplateObject == null || messageTemplateObject.Id < 1)
                    {
                        email = "";
                        return new MessageTemplateObject();
                    }

                    var usp = usps[0];
                    if (usp.UserInfo_Id == null)
                    {
                        email = "";
                        return new MessageTemplateObject();
                    }
                    var security = new Guid().ToString();
                    usp.SecurityStamp = security;
                    usp.PasswordHash = passwordHash;
                    db.Entry(usp).State = EntityState.Modified;
                    db.SaveChanges();

                    messageTemplateObject.UserId = (long)usp.UserInfo_Id;
                    email = usp.Email;
                    return messageTemplateObject;
                }
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
                if (user == null)
                {
                    return -2;
                }
                
                using (var db = new ImportPermitEntities())
                {
                    var persons = db.People.Where(v => v.Id == user.PersonId).ToList();
                    if (!persons.Any())
                    {
                        return -2;
                    }
                    var person = persons[0];
                    var usps = db.UserProfiles.Where(o => o.PersonId == person.Id).ToList();
                    if (!usps.Any())
                    {
                        return -2;
                    }
                    var usp = usps[0];

                    person.FirstName = user.FirstName;
                    person.LastName = user.LastName;
                    usp.IsActive = user.IsActive;

                    db.Entry(person).State = EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(usp).State = EntityState.Modified;
                    db.SaveChanges();
                    return person.Id;
                }
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
                if (user == null)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    var banks = db.Banks.Where(v => v.ImporterId == user.CompanyId).ToList();
                    if (!banks.Any())
                    {
                        return -2;
                    }
                    var bank = banks[0];
                    long personId = 0;
                    if (db.People.Any())
                    {
                        var existingp = db.People.OrderByDescending(i => i.Id).Take(1).ToList();
                        if (!existingp.Any())
                        {
                            personId = 1;
                        }
                        else
                        {
                            personId = existingp[0].Id + 1;
                        }
                    }
                    else
                    {
                        personId = 1;
                    }
                    var person = new Person
                    {
                        Id =  personId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        ImporterId = bank.ImporterId,
                        IsAdmin = user.IsAdmin
                        
                    };

                    var res =db.People.Add(person);
                    db.SaveChanges();
                    return res.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddPerson(UserProfileObject user)
        {
            try
            {
                if (user == null)
                {
                    return -2;
                }
              
                var person = new Person
                {
                    FirstName = user.PersonObject.FirstName,
                    LastName =  user.PersonObject.LastName,
                    ImporterId = user.PersonObject.ImporterId
                };
                
                using (var db = new ImportPermitEntities())
                {
                    var existings = db.People.Where(p => p.FirstName.ToLower().Trim() == user.PersonObject.FirstName.ToLower().Trim() && p.LastName.ToLower().Trim() == user.PersonObject.LastName.ToLower().Trim() && p.ImporterId == user.PersonObject.ImporterId).ToList();
                    if (existings.Any())
                    {
                        return existings[0].Id;
                    }
                   
                    var res = db.People.Add(person);
                    db.SaveChanges();
                    return res.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddBankUser(BankUserObject user)
        {
            try
            {
                if (user == null)
                {
                    return -2;
                }

                var bankUser = new BankUser
                {
                    UserId = user.UserId
                };

                using (var db = new ImportPermitEntities())
                {
                    var branches = db.BankBranches.Where(p => p.BranchCode.ToLower().Trim() == user.BranchCode.ToLower().Trim() && p.BankId == user.BankId).ToList();
                    if (!branches.Any())
                    {
                        return -2;
                    }
                    var branchId = branches[0].Id;
                    bankUser.BranchId = branchId;
                    var res = db.BankUsers.Add(bankUser);
                    db.SaveChanges();
                    return res.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddBankUser2(BankUserObject user)
        {
            try
            {
                if (user == null || user.BranchId < 1 || user.UserId < 1)
                {
                    return -2;
                }

                var bankUser = new BankUser
                {
                    UserId = user.UserId,
                    BranchId = user.BranchId
                };

                using (var db = new ImportPermitEntities())
                {
                    var res = db.BankUsers.Add(bankUser);
                    db.SaveChanges();
                    return res.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public long AddBankUserWithBranch(BankUserObject user)
        {
            try
            {
                if (user == null)
                {
                    return -2;
                }

                var bankUser = new BankUser
                {
                    UserId = user.UserId,
                    BranchId = user.BranchId
                };

                if (bankUser.BranchId < 1 || bankUser.UserId < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    var res = db.BankUsers.Add(bankUser);
                    db.SaveChanges();
                    return res.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateBankUser(BankUserObject user)
        {
            try
            {
                if (user == null)
                {
                    return -2;
                }
                
                using (var db = new ImportPermitEntities())
                {
                    var bankUsers = db.BankUsers.Where(p => p.UserId == user.UserId).ToList();
                    if (!bankUsers.Any())
                    {
                        return -2;
                    }
                    var bankUser = bankUsers[0];
                    bankUser.BranchId = user.BranchId;
                    db.Entry(bankUser).State = EntityState.Modified;
                    db.SaveChanges();
                    return bankUser.Id;
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
         
        public long DeletePerson(long id)
        {
            try
            {
                if (id < 1)
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var res = db.People.Find(id);
                    if (res == null || res.Id < 1)
                    {
                        return -2;
                    }
                    db.People.Remove(res);
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

        public long AddPersonInfo(PersonObject user)
        {
            try
            {
                if (user == null)
                {
                   return -2;
                }
                var userEntity = ModelMapper.Map<PersonObject, Person>(user);
                if (userEntity == null || userEntity.ImporterId < 1)
                {
                    return 0;
                }

                using (var db = new ImportPermitEntities())
                {
                    var banks = db.Banks.Where(b => b.ImporterId == user.ImporterId).ToList();
                    if (!banks.Any())
                    {
                        return 0;
                    }
                   
                    var res = db.People.Add(userEntity);
                    db.SaveChanges();
                    return res.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddDprUser(PersonObject user)
        {
            try
            {
                if (user == null)
                {
                    return -2;
                }
                var userEntity = ModelMapper.Map<PersonObject, Person>(user);
                if (userEntity == null || userEntity.ImporterId < 1)
                {
                    return 0;
                }

                using (var db = new ImportPermitEntities())
                {
                    var existings = db.People.Where(b => b.FirstName.Trim().ToLower().Replace(" ", "") == user.FirstName.Trim().ToLower().Replace(" ", "") && b.LastName.Trim().ToLower().Replace(" ", "") ==  user.LastName.Trim().ToLower().Replace(" ", "")).ToList();
                    if (existings.Any())
                    {
                        return 0;
                    }

                    var res = db.People.Add(userEntity);
                    db.SaveChanges();
                    return res.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public MessageTemplateObject ActivateAccount(string email, string securityStamp, int msgEventId, long msgid)
        {
            try
            {
                if (string.IsNullOrEmpty(securityStamp))
                {
                    return new MessageTemplateObject();
                }
                
                using (var db = new ImportPermitEntities())
                {

                    var users = (from asp in db.AspNetUsers.Where(m => m.SecurityStamp == securityStamp && m.Email == email) 
                                     join usp in db.UserProfiles on asp.UserInfo_Id equals usp.Id 
                                     join ps in db.People on usp.PersonId equals ps.Id
                                     join imp in db.Importers on ps.ImporterId equals imp.Id 
                                     select new {asp, imp, usp, ps}).ToList();

                    if (!users.Any())
                    {
                        return new MessageTemplateObject();
                    }
                    
                    var  user = users[0].asp;
                    var obj = users[0].imp;
                    var person = users[0].ps;
                    var us = users[0].usp;

                    var settings = db.ImportSettings.ToList();
                    if (!settings.Any())
                    {
                        return new MessageTemplateObject();
                    }

                    var msgs = db.Messages.Where(t => t.Id == msgid && t.UserId == us.Id).ToList();
                    if (!msgs.Any())
                    {
                        return new MessageTemplateObject();
                    }
                    var mesg = msgs[0];
                    var setting = settings[0];
                    if (setting.MessageLifeSpan != null)
                    {
                        var hours = (double) setting.MessageLifeSpan;
                        var date = mesg.DateSent.AddHours(hours);
                        var activationDate = DateTime.Now;
                        if (activationDate > date)
                        {
                            return new MessageTemplateObject
                            {
                                ErrorCode = 3
                            };
                        }
                    }
                    else
                    {
                        return new MessageTemplateObject();
                    }
                    var templates = db.MessageTemplates.Where(m => m.EventTypeId == msgEventId).ToList();
                    if (!templates.Any())
                    {
                        return new MessageTemplateObject();
                    }
                   var app = templates[0];
                    user.EmailConfirmed = true;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    var name = "";
                    if (person.IsImporter != null && (bool) person.IsImporter)
                    {
                        name = obj.Name;
                    }
                    else
                    {
                        name = person.FirstName + " " + person.LastName;
                    }
                    var msg = new MessageTemplateObject
                    {
                        Id = app.Id,
                        IsImporter = person.IsImporter != null && (bool) person.IsImporter,
                        UserId = us.Id,
                        UserName = name,
                        Subject = app.Subject,
                        MessageContent = app.MessageContent,
                        Footer = app.Footer,
                        PhoneNumber = user.PhoneNumber
                    };

                    return msg;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new MessageTemplateObject();
            }
        }
     
    }
}
