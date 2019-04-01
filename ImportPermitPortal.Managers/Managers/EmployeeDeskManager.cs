using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;


namespace ImportPermitPortal.Managers.Managers
{
    public class EmployeeDeskManager
    {

        public long AddEmployeeDesk(EmployeeDeskObject employeeDesk)
        {
            try
            {
                
                if (employeeDesk == null)
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var loginUser = db.AspNetUsers.Where(a => a.Email.Equals(employeeDesk.Email)).ToList();
                    long employeeId = 0;
                    if (loginUser.Any())
                    {
                        var profileId = loginUser[0].UserInfo_Id;

                        var employee = db.EmployeeDesks.Where(e => e.UserProfile.Id == profileId).ToList();
                        if (employee.Any())
                        {
                            employeeId = employee[0].Id;
                        }
                        else
                        {
                            return -2;
                        }

                    }
                   

                    employeeDesk.EmployeeId = employeeId;
                    employeeDesk.JobCount = 0;

                var employeeDeskEntity = ModelMapper.Map<EmployeeDeskObject, EmployeeDesk>(employeeDesk);
                if (employeeDeskEntity == null)
                {
                    return -2;
                }
               
                    var returnStatus = db.EmployeeDesks.Add(employeeDeskEntity);
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

        public long UpdateEmployeeDesk(EmployeeDeskObject employeeDesk)
        {
            try
            {
                if (employeeDesk == null)
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var loginUser = db.AspNetUsers.Where(a => a.Email.Equals(employeeDesk.Email)).ToList();
                     long employeeId = 0;
                    if (loginUser.Any())
                    {
                        var profileId = loginUser[0].UserInfo_Id;

                         var employee = db.EmployeeDesks.Where(e => e.UserProfile.Id == profileId).ToList();
                    if (employee.Any())
                    {
                        employeeId = employee[0].Id;
                    }
                    else
                    {
                        return -2;
                    }

                    }
                   
                   
                    employeeDesk.EmployeeId = employeeId;

                var employeeDeskEntity = ModelMapper.Map<EmployeeDeskObject, EmployeeDesk>(employeeDesk);
                if (employeeDeskEntity == null || employeeDeskEntity.Id < 1)
                {
                    return -2;
                }

              
                    db.EmployeeDesks.Attach(employeeDeskEntity);
                    db.Entry(employeeDeskEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return employeeDesk.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<EmployeeDeskObject> GetEmployeeDesks()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var employeeDesks = db.EmployeeDesks.ToList();
                    if (!employeeDesks.Any())
                    {
                        return new List<EmployeeDeskObject>();
                    }
                    var objList = new List<EmployeeDeskObject>();
                    employeeDesks.ForEach(app =>
                    {
                        var employeeDeskObject = ModelMapper.Map<EmployeeDesk, EmployeeDeskObject>(app);
                        if (employeeDeskObject != null && employeeDeskObject.Id > 0)
                        {
                           
                            objList.Add(employeeDeskObject);
                        }
                    });

                    return !objList.Any() ? new List<EmployeeDeskObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        
        public List<EmployeeDeskObject> GetEmployeeDesks(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var employeeDesks =
                            db.EmployeeDesks.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)
                                .Include("UserProfile").Include("Group").Include("StepActivityType").Include("Zone")
                                .ToList();

                        if (employeeDesks.Any())
                        {
                            var newList = new List<EmployeeDeskObject>();
                            employeeDesks.ForEach(app =>
                            {
                                var employeeDeskObject = ModelMapper.Map<EmployeeDesk, EmployeeDeskObject>(app);
                                if (employeeDeskObject != null && employeeDeskObject.Id > 0)
                                {
                                    employeeDeskObject.EmployeeName = app.UserProfile.Person.FirstName;
                                    employeeDeskObject.GroupName = app.Group.Name;
                                    employeeDeskObject.ActivityTypeName = app.StepActivityType.Name;
                                    employeeDeskObject.ZoneName = app.Zone.Name;
                                    employeeDeskObject.JobCountStr = app.JobCount.ToString();
                                    newList.Add(employeeDeskObject);
                                }
                            });
                            countG = db.EmployeeDesks.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<EmployeeDeskObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<EmployeeDeskObject>();
            }
        }


        public List<UserProfileObject> GetAppUsers(int? itemsPerPage, int? pageNumber, out int countG, long id)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var users =
                            db.UserProfiles.Where(j => j.Person.ImporterId == id && !j.EmployeeDesks.Any()).OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)
                                .Include("Person").Include("AspNetUsers")
                                .ToList();
                         

                        if (!users.Any())
                        {
                            countG = 0;
                            return new List<UserProfileObject>();
                        }

                        var newList = new List<UserProfileObject>();
                        users.ForEach(app =>
                        {
                            var aspUsers = app.AspNetUsers.ToList()[0];
                            var userObject = ModelMapper.Map<UserProfile, UserProfileObject>(app);
                            if (userObject != null && userObject.Id > 0)
                            {
                                userObject.Name = app.Person.FirstName + " " + app.Person.LastName;
                                userObject.PhoneNumber = aspUsers.PhoneNumber;
                                userObject.Email = aspUsers.Email;
                                userObject.UserId = aspUsers.Id;
                                userObject.StatusStr = app.IsActive ? "Active" : "Inactive";
                                newList.Add(userObject);
                            }
                        });
                        countG = db.EmployeeDesks.Count();
                        return newList;
                    }

                }
                countG = 0;
                return new List<UserProfileObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<UserProfileObject>();
            }
        }

        public UserProfileObject GetAppUser(long id)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var users = db.UserProfiles.Where(m => m.Id == id)
                            .Include("Person").Include("AspNetUsers")
                            .ToList();

                    if (!users.Any())
                    {
                        return new UserProfileObject();
                    }

                    var usr = users[0];

                    var aspUser = usr.AspNetUsers.ToList()[0];
                    var userObject = ModelMapper.Map<UserProfile, UserProfileObject>(usr);
                    if (userObject == null || userObject.Id < 1)
                    {
                        return new UserProfileObject();
                    }

                    userObject.FirstName = usr.Person.FirstName;
                    userObject.LastName = usr.Person.LastName;
                    userObject.PhoneNumber = aspUser.PhoneNumber;
                    userObject.Email = aspUser.Email;
                    userObject.UserId = aspUser.Id;
                    userObject.Status = usr.IsActive;
                    return userObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new UserProfileObject();
            }
        }

        public EmployeeDeskObject GetEmployeeDesk(long employeeDeskId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var employeeDesks =
                        db.EmployeeDesks.Where(m => m.Id == employeeDeskId)
                            .ToList();
                    if (!employeeDesks.Any())
                    {
                        return new EmployeeDeskObject();
                    }

                    var app = employeeDesks[0];
                    var employeeDeskObject = ModelMapper.Map<EmployeeDesk, EmployeeDeskObject>(app);
                    if (employeeDeskObject == null || employeeDeskObject.Id < 1)
                    {
                       
                        
                        return new EmployeeDeskObject();
                    }
                    
                    employeeDeskObject.FirstName = app.UserProfile.Person.FirstName;
                    employeeDeskObject.LastName = app.UserProfile.Person.LastName;

                    var regUser = db.AspNetUsers.Where(a => a.UserInfo_Id == app.EmployeeId).ToList();
                    if (regUser.Any())
                    {
                        employeeDeskObject.Email = regUser[0].Email;
                        employeeDeskObject.Phone = regUser[0].PhoneNumber;
                    }

                    //get step activity type
                    var activity = db.StepActivityTypes.Where(s => s.Id == app.ActivityTypeId).ToList();
                    if (activity.Any())
                    {
                        employeeDeskObject.ActivityTypeName = activity[0].Name;
                        employeeDeskObject.StepDescription = activity[0].Description;
                    }
                    return employeeDeskObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new EmployeeDeskObject();
            }
        }

        public List<EmployeeDeskObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var employeeDesks =
                        db.EmployeeDesks.Where(m => m.Group.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (employeeDesks.Any())
                    {
                        var newList = new List<EmployeeDeskObject>();
                        employeeDesks.ForEach(app =>
                        {
                            var employeeDeskObject = ModelMapper.Map<EmployeeDesk, EmployeeDeskObject>(app);
                            if (employeeDeskObject != null && employeeDeskObject.Id > 0)
                            {
                               
                                newList.Add(employeeDeskObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<EmployeeDeskObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<EmployeeDeskObject>();
            }
        }

        public List<UserProfileObject> SearchAppUsers(string searchCriteria, long id)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var users =
                        db.UserProfiles.Where(m => (m.Person.FirstName.Contains(searchCriteria.Trim()) || m.Person.LastName.Contains(searchCriteria.Trim())) && m.Person.ImporterId == id)
                            .Include("Person").Include("AspNetUsers").ToList();

                    if (!users.Any())
                    {
                        return new List<UserProfileObject>();
                    }

                    var newList = new List<UserProfileObject>();
                    users.ForEach(app =>
                    {
                        var aspUsers = app.AspNetUsers.ToList()[0];
                        var userObject = ModelMapper.Map<UserProfile, UserProfileObject>(app);
                        if (userObject != null && userObject.Id > 0)
                        {
                            userObject.Name = app.Person.FirstName + " " + app.Person.LastName;
                            userObject.PhoneNumber = aspUsers.PhoneNumber;
                            userObject.Email = aspUsers.Email;
                            userObject.UserId = aspUsers.Id;
                            userObject.StatusStr = app.IsActive ? "Active" : "Inactive";
                            newList.Add(userObject);
                        }
                    });
                    return newList;
                }
            }
            catch (Exception ex)
            {
                return new List<UserProfileObject>();
            }
        }
        public long DeleteEmployeeDesk(long employeeDeskId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.EmployeeDesks.Where(m => m.Id == employeeDeskId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.EmployeeDesks.Remove(item);
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
