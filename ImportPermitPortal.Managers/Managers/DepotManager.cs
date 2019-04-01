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
    public class DepotManager
    {
        public long AddDepot(DepotObject depot)
        {
            try
            {
                if (depot == null)
                {
                    return -2;
                }

               using (var db = new ImportPermitEntities())
                {
                    var depotEntity = new Depot
                    {
                        Name = depot.Name,
                        ImporterId = depot.ImporterId,
                        JettyId = depot.JettyId
                    };
                    var returnStatus = db.Depots.Add(depotEntity);
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


        public long AddDepot(List<DepotObject> depotList)
        {
            try
            {
                if (!depotList.Any())
                {
                    return -2;
                }
                var successCount = 0;
                using (var db = new ImportPermitEntities())
                {
                    depotList.ForEach(depot =>
                    {
                        var depotEntity = ModelMapper.Map<DepotObject, Depot>(depot);
                        if (depotEntity != null && !string.IsNullOrEmpty(depotEntity.DepotLicense))
                        {
                            db.Depots.Add(depotEntity);
                            db.SaveChanges();
                            successCount += 1;
                        }
                       
                    });

                    return successCount;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateDepot(DepotObject depot)
        {
            try
            {
                if (depot == null)
                {
                    return -2;
                }

                var depotEntity = ModelMapper.Map<DepotObject, Depot>(depot);
                if (depotEntity == null || depotEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.Depots.Attach(depotEntity);
                    db.Entry(depotEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return depot.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<DepotObject> GetLocalDepots()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    const int nigeria = (int) CountryEnum.Nigeria;

                    var depots = db.Depots.Where(c => c.Jetty.Port.Country.Name == "nigeria").ToList();

                    if (!depots.Any())
                    {
                        return new List<DepotObject>();
                    }

                    var objList = new List<DepotObject>();

                    depots.ForEach(app =>
                    {
                      
                       
                            var depotObject = ModelMapper.Map<Depot, DepotObject>(app);
                            if (depotObject != null && depotObject.Id > 0)
                            {
                               objList.Add(depotObject);
                            }
                        
                   });

                    return !objList.Any() ? new List<DepotObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
        public List<DepotObject> GetDepots()
        {
            try
            {

                using (var db = new ImportPermitEntities())
                {
                    var depots = db.Depots.Where(m => !string.IsNullOrEmpty(m.DepotLicense) && m.ExpiryDate != null && m.ExpiryDate >= DateTime.Now && m.Status == true).ToList();

                    var depotList = new List<DepotObject>();

                    //return depots.Select(item => new DepotObject {Name = item.Name, Id = item.Id}).ToList();

                    foreach (var item in depots)
                    {
                        var depotObj = new DepotObject();
                        depotObj.Id = item.Id;
                        depotObj.Name = item.Name;

                        depotList.Add(depotObj);
                    }

                    return depotList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DepotObject>();
            }
        }
        public List<DepotObject> GetDepots(long userProfileId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var objList = new List<DepotObject>();
                    //get the inspector's location
                    int zoneId;
                    var employee = db.EmployeeDesks.Where(e => e.EmployeeId == userProfileId).ToList();
                    if (employee.Any())
                    {
                        zoneId = employee[0].ZoneId;
                        //get the jettymapping in the zone
                        var jettymappings = db.JettyMappings.Where(j => j.ZoneId == zoneId).ToList();

                        if (jettymappings.Any())
                        {
                            foreach (var item in jettymappings)
                            {
                                var depot = db.Depots.Where(d => d.JettyId == item.JettyId).ToList();
                               
                                if (depot.Any())
                                {
                                    var depotObject = new DepotObject();
                                    depotObject.Id = depot[0].Id;
                                    depotObject.Name = depot[0].Name;
                                    objList.Add(depotObject);
                                }
                            }
                        }
                        return objList;
                    }

                    return new List<DepotObject>(); 
              
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public UserProfileObject GetDepotAdmin(int depotId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                   
                    var depotAdmins = (from dp in db.Depots.Where(m => m.Id == depotId).Include("Jetty")
                                      join cm in db.Importers on dp.ImporterId equals cm.Id
                                      join cp in db.People on cm.Id equals cp.ImporterId
                                      join us in db.UserProfiles.Where(n => n.IsAdmin) on cp.Id equals us.PersonId
                                      join id in db.AspNetUsers on us.Id equals id.UserInfo_Id

                                      select new UserProfileObject
                                      {
                                          Id = us.Id,
                                          JettyId = dp.Jetty.Id,
                                          JettyName = dp.Jetty.Name,
                                          CompanyName = cm.Name,
                                          DepotId = dp.Id,
                                          UserId = id.Id,
                                          PhoneNumber = id.PhoneNumber,
                                          CompanyId = cm.Id,
                                          PersonId = cp.Id,
                                          FirstName = cp.FirstName,
                                          LastName = cp.LastName,
                                          Email = id.Email,
                                          IsActive = us.IsActive,
                                          DepotLicense = dp.DepotLicense,
                                          IssueDate = dp.IssueDate,
                                          ExpiryDate = dp.ExpiryDate,
                                          Status = dp.Status

                                      }).ToList();

                    if (!depotAdmins.Any())
                    {
                        return new UserProfileObject();
                    }

                    var depotUser = depotAdmins[0];
                    if (depotUser.IssueDate != null)
                        depotUser.IssueDateStr = depotUser.IssueDate.Value.ToString("dd/MM/yyyy");
                    if (depotUser.ExpiryDate != null)
                        depotUser.ExpiryDateStr = depotUser.ExpiryDate.Value.ToString("dd/MM/yyyy");
                    return depotUser;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new UserProfileObject();
            }
        }
        
        public List<DepotObject> GetDepots(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var banks = db.Depots.OrderByDescending(m => m.Id).Include("Importer").Include("Jetty").Skip(tpageNumber).Take(tsize).ToList();

                        if (!banks.Any())
                        {
                            countG = 0;
                            return new List<DepotObject>();
                        }

                        var newList = new List<DepotObject>();
                        banks.ForEach(h =>
                        {
                            var dpt = new DepotObject
                            {
                                Id = h.Id,
                                Name = h.Name,
                                JettyId = h.Jetty.Id,
                                JettyName = h.Jetty.Name
                            };

                            var usrs = (from pp in db.People.Where(n => n.ImporterId == h.ImporterId)
                                        join tt in db.UserProfiles.Include("AspNetUsers") on pp.Id equals tt.PersonId
                                        where tt.IsAdmin
                                        select new { tt, pp }).ToList();
                            if (usrs.Any())
                            {
                                var usr = usrs[0].tt;
                                var p = usrs[0].pp;
                                if (usr != null && usr.Id > 0 && p.Id > 0)
                                {
                                    dpt.UserId = usr.AspNetUsers.ElementAt(0).Id; 
                                    dpt.LastName = p.FirstName + " " + p.LastName;
                                    dpt.PhoneNumber = usr.AspNetUsers.ElementAt(0).PhoneNumber;
                                }
                            }
                            else
                            {
                                dpt.UserId = "";
                                dpt.PhoneNumber = "";
                                dpt.LastName = "";
                            }

                            newList.Add(dpt);
                        });

                        countG = db.Banks.Count();
                        return newList;

                    }

                }
                countG = 0;
                return new List<DepotObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<DepotObject>();
            }
        }
        
        public DepotObject GetDepot(long depotId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var depots =
                        db.Depots.Where(m => m.Id == depotId)
                            .ToList();
                    if (!depots.Any())
                    {
                        return new DepotObject();
                    }

                    var app = depots[0];
                    var depotObject = ModelMapper.Map<Depot, DepotObject>(app);
                    if (depotObject == null || depotObject.Id < 1)
                    {
                        return new DepotObject();
                    }
                    depotObject.JettyName = app.Jetty.Name;
                    return depotObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new DepotObject();
            }
        }

        public List<DepotObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var depots =
                        db.Depots
                        .Include("Jetty")
                        
                        .Where(m => m.Name.ToLower() == searchCriteria.ToLower().Trim()
                        || m.Jetty.Name.ToLower() == searchCriteria.ToLower().Trim())
                        .ToList();
                    if (!depots.Any())
                    {
                        return new List<DepotObject>();
                    }
                    var newList = new List<DepotObject>();
                    depots.ForEach(app =>
                    {
                        var depotObject = ModelMapper.Map<Depot, DepotObject>(app);
                        if (depotObject != null && depotObject.Id > 0)
                        {
                            depotObject.JettyName = app.Jetty.Name;
                            newList.Add(depotObject);
                        }
                    });
                    return newList;
                }
            }
            catch (Exception ex)
            {
                return new List<DepotObject>();
            }
        }
        public long DeleteDepot(long depotId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.Depots.Where(m => m.Id == depotId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.Depots.Remove(item);
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
