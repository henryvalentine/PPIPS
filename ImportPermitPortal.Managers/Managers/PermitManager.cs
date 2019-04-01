using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
    public class PermitManager
    {
        public long AddPermit(PermitObject permit)
        {
            try
            {
                if (permit == null)
                {
                    return -2;
                }

                var permitEntity = ModelMapper.Map<PermitObject, Permit>(permit);

                if (permitEntity == null)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {

                    var stopDate = DateTime.Parse("2015-07-31");
                    if (DateTime.Today <= stopDate)
                    {
                        var existingSimilarPermits = db.Permits.Where(m => m.PermitValue.Trim() == permit.PermitValue.Trim() && m.ImporterId == permit.ImporterId).ToList();

                        if (existingSimilarPermits.Any())
                        {
                            return existingSimilarPermits[0].Id;
                        }

                        var returnStatus = db.Permits.Add(permitEntity);
                        db.SaveChanges();
                        return returnStatus.Id;
                    }
                    else
                    {
                        var returnStatus = db.Permits.Add(permitEntity);
                        db.SaveChanges();
                        return returnStatus.Id;
                    }

                   
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdatePermit(PermitObject permit)
        {
            try
            {
                if (permit == null)
                {
                    return -2;
                }

                var permitEntity = ModelMapper.Map<PermitObject, Permit>(permit);
                if (permitEntity == null || permitEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.Permits.Attach(permitEntity);
                    db.Entry(permitEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return permit.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<PermitObject> GetPermits()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var permits = db.Permits.ToList();
                    if (!permits.Any())
                    {
                        return new List<PermitObject>();
                    }
                    var objList = new List<PermitObject>();
                    permits.ForEach(app =>
                    {
                        var permitObject = ModelMapper.Map<Permit, PermitObject>(app);
                        if (permitObject != null && permitObject.Id > 0)
                        {
                            
                            objList.Add(permitObject);
                        }
                    });

                    return !objList.Any() ? new List<PermitObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
        
        public List<PermitObject> GetPermits(long applicationId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var permitApps = db.PermitApplications.Where(m => m.ApplicationId == applicationId).Include("Application").Include("Permit").ToList();
                    if (!permitApps.Any())
                    {
                        return new List<PermitObject>();
                    }

                    var objList = new List<PermitObject>();

                    permitApps.ForEach(app =>
                    {
                        var permitObject = ModelMapper.Map<Permit, PermitObject>(app.Permit);
                        if (permitObject != null && permitObject.Id > 0)
                        {
                            var appObject = ModelMapper.Map<Application, ApplicationObject>(app.Application);
                            if (appObject != null && appObject.Id > 0)
                            {
                                permitObject.ApplicationObject = appObject;
                                objList.Add(permitObject);
                            }
                        }
                    });

                    return !objList.Any() ? new List<PermitObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
        public List<PermitObject> GetPermits(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var permits =
                            db.Permits
                            .OrderByDescending(m => m.Id)
                             .Skip(tpageNumber).Take(tsize)
                             .ToList();
                        if (permits.Any())
                        {
                            var newList = new List<PermitObject>();
                            permits.ForEach(app =>
                            {
                                var permitObject = ModelMapper.Map<Permit, PermitObject>(app);
                                if (permitObject != null && permitObject.Id > 0)
                                {
                                    permitObject.Id = app.Id;
                                    permitObject.PermitNo = app.PermitNo;
                                    permitObject.PermitStatusStr = Enum.GetName(typeof(PermitStatusEnum), app.PermitStatus);
                                    permitObject.IssueDateStr = app.IssueDate.Value.ToString("dd/MM/yy");
                                    permitObject.ExpiryDateStr = app.ExpiryDate.Value.ToString("dd/MM/yy");

                                    var permitApp = db.PermitApplications.Where(p => p.PermitId == app.Id).ToList();
                                    if (permitApp.Any())
                                    {
                                        var applicationId = permitApp[0].ApplicationId;

                                        //get the application
                                        var application = db.Applications.Where(a => a.Id == applicationId).ToList();

                                        if (application.Any())
                                        {
                                            permitObject.CompanyName = application[0].Importer.Name;
                                        }
                                    }

                                    
                                    newList.Add(permitObject);
                                }
                            });
                            countG = db.Permits.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<PermitObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<PermitObject>();
            }
        }

        public List<PermitObject> GetApplicantPermits(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var permits = (from app in db.Applications.Where(y => y.ImporterId == importerId)
                                       join pApp in db.PermitApplications on app.Id equals pApp.ApplicationId
                                       join prt in db.Permits.OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                                       on pApp.PermitId equals prt.Id
                                       select prt).ToList();
                        if (permits.Any())
                        {
                            var newList = new List<PermitObject>();
                            permits.ForEach(app =>
                            {
                                var permitObject = ModelMapper.Map<Permit, PermitObject>(app);
                                if (permitObject != null && permitObject.Id > 0)
                                {
                                    permitObject.Id = app.Id;
                                    permitObject.PermitNo = app.PermitNo;
                                    permitObject.PermitStatusStr = Enum.GetName(typeof(PermitStatusEnum), app.PermitStatus);
                                    permitObject.IssueDateStr = app.IssueDate != null ? app.IssueDate.Value.ToString("dd/MM/yy") : "";
                                    permitObject.ExpiryDateStr =app.ExpiryDate != null ? app.ExpiryDate.Value.ToString("dd/MM/yy") : "";

                                    var permitApp = db.PermitApplications.Where(p => p.PermitId == app.Id).ToList();
                                    if (permitApp.Any())
                                    {
                                        var applicationId = permitApp[0].ApplicationId;

                                        //get the application
                                        var application = db.Applications.Where(a => a.Id == applicationId).ToList();

                                        if (application.Any())
                                        {
                                            permitObject.CompanyName = application[0].Importer.Name;
                                        }
                                    }


                                    newList.Add(permitObject);
                                }
                            });
                            countG = db.Permits.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<PermitObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<PermitObject>();
            }
        }

        public PermitObject GetPermit(long permitId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var permits =
                        db.Permits.Where(m => m.Id == permitId)
                            .ToList();
                    if (!permits.Any())
                    {
                        return new PermitObject();
                    }

                    var app = permits[0];
                    var permitObject = ModelMapper.Map<Permit, PermitObject>(app);
                    if (permitObject == null || permitObject.Id < 1)
                    {
                        return new PermitObject();
                    }

                    return permitObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new PermitObject();
            }
        }


        public ApplicationObject GetPermitInfo(long id)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {

                    var permitList = db.PermitApplications.Where(x => x.PermitId == id).Include("Application").Include("Permit").ToList();

                    if (!permitList.Any())
                    {
                        return new ApplicationObject();
                    }

                    var permitApp = permitList[0];
                    var app = permitApp.Application;
                    var permit = permitApp.Permit;

                    var applicationItems = db.ApplicationItems.Where(x => x.ApplicationId == app.Id).ToList();

                    if (!applicationItems.Any())
                    {
                        return new ApplicationObject();
                    }

                    var importers = db.Importers.Where(i => i.Id == app.ImporterId).ToList();
                    if (!importers.Any())
                    {
                        return new ApplicationObject();
                    }

                    var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new ApplicationObject();
                    }
                    
                    importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                    importObject.DateIssued = permit.IssueDate != null ? permit.IssueDate.Value.ToString("dd/MM/yyyy") : "";
                    importObject.ExpiryDate = permit.ExpiryDate != null ? permit.ExpiryDate.Value.ToString("dd/MM/yyyy") : "";
                    importObject.PermitId = permit.Id;
                    importObject.Rrr = app.Invoice.RRR;
                    importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                    importObject.CompanyName = importers[0].Name;
                    importObject.PermitNumber = permit.PermitValue;
                    importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                    
                    applicationItems.ForEach(u =>
                    {
                        var im = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(u);
                        if (im != null && im.Id > 0)
                        {
                          var product = (from pr in db.Products.Where(x => x.ProductId == im.ProductId)
                                                select new ProductObject
                                                {
                                                    ProductId = pr.ProductId,
                                                    Code = pr.Code,
                                                    Name = pr.Name
                                                }).ToList()[0];

                            im.ProductName = product.Name;
                            im.ProductCode = product.Code;
                         
                            //ImportedQuantityValue OutstandingQuantity

                            var bankers = db.ProductBankers.Where(i => i.ApplicationItemId == u.Id).Include("Bank").ToList();
                            var appCountries = db.ApplicationCountries.Where(a => a.ApplicationItemId == im.Id).Include("Country").ToList();
                            var depotList = db.ThroughPuts.Where(a => a.ApplicationItemId == im.Id).Include("Depot").Include("Document").ToList();
                            if (appCountries.Any() && depotList.Any())
                            {
                                im.CountryOfOriginName = "";
                                appCountries.ForEach(c =>
                                {
                                    if (string.IsNullOrEmpty(im.CountryOfOriginName))
                                    {
                                        im.CountryOfOriginName = c.Country.Name;
                                    }
                                    else
                                    {
                                        im.CountryOfOriginName += ", " + c.Country.Name;
                                    }
                                });

                                im.DischargeDepotName = "";
                                if (im.StorageProviderTypeId != (int)StorageProviderTypeEnum.Own_Depot)
                                {
                                    depotList.ForEach(d =>
                                    {
                                        var depotName = im.StorageProviderTypeId == (int)StorageProviderTypeEnum.Own_Depot ? d.Depot.Name + "(" + d.Depot.DepotLicense + ")" : d.Depot.Name;

                                        if (string.IsNullOrEmpty(im.DischargeDepotName))
                                        {
                                            im.DischargeDepotName = depotName;
                                        }
                                        else
                                        {
                                            im.DischargeDepotName += ", " + depotName;
                                        }


                                    });
                                }
                            }

                            im.ProductBankerName = "";
                            bankers.ForEach(c =>
                            {
                                if (string.IsNullOrEmpty(im.ProductBankerName))
                                {
                                    im.ProductBankerName = string.IsNullOrEmpty(c.BankAccountNumber) ? c.Bank.Name : c.Bank.Name + "(" + c.BankAccountNumber + ")";
                                }
                                else
                                {
                                    var bankname = string.IsNullOrEmpty(c.BankAccountNumber) ? c.Bank.Name : c.Bank.Name + "(" + c.BankAccountNumber + ")";
                                    im.ProductBankerName += ", " + bankname;
                                }
                               
                            });

                            importObject.ApplicationItemObjects.Add(im);
                        }
                    });
                    return importObject;
                }

            }
            catch (Exception ex)
            {
                return new ApplicationObject();
            }
        }


        public List<PermitObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var permits =
                        db.Permits
                        .Include("Application")
                        
                        .Where(m => m.PermitValue.ToLower() == searchCriteria.ToLower().Trim()
                        || m.ExpiryDate.ToString().ToLower() == searchCriteria.ToLower().Trim())
                        .ToList();
                    if (!permits.Any())
                    {
                        return new List<PermitObject>();
                    }
                    var newList = new List<PermitObject>();
                    permits.ForEach(app =>
                    {
                        var permitObject = ModelMapper.Map<Permit, PermitObject>(app);
                        if (permitObject != null && permitObject.Id > 0)
                        {
                            permitObject.Id = app.Id;
                            permitObject.PermitNo = app.PermitNo;
                            //permitObject.Status = app.Status;
                            permitObject.IssueDate = app.IssueDate;
                            permitObject.ExpiryDate = app.ExpiryDate;
                            newList.Add(permitObject);
                        }
                    });
                    return newList;
                }
            }
            catch (Exception ex)
            {
                return new List<PermitObject>();
            }
        }

        public List<PermitObject> SearchApplicantPermits(string searchCriteria, long importerId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var permits = (from app in db.Applications.Where(y => y.ImporterId == importerId)
                        join pApp in db.PermitApplications on app.Id equals pApp.ApplicationId
                        join prt in db.Permits.Where(m => m.PermitValue.ToLower() == searchCriteria.ToLower().Trim()).Include("Application")
                        on pApp.PermitId equals prt.Id
                        select prt).ToList();
                      
                    if (!permits.Any())
                    {
                        return new List<PermitObject>();
                    }
                    var newList = new List<PermitObject>();
                    permits.ForEach(app =>
                    {
                        var permitObject = ModelMapper.Map<Permit, PermitObject>(app);
                        if (permitObject != null && permitObject.Id > 0)
                        {
                            permitObject.Id = app.Id;
                            permitObject.PermitNo = app.PermitNo;
                            //permitObject.Status = app.Status;
                            permitObject.IssueDate = app.IssueDate;
                            permitObject.ExpiryDate = app.ExpiryDate;
                            newList.Add(permitObject);
                        }
                    });
                    return newList;
                }
            }
            catch (Exception ex)
            {
                return new List<PermitObject>();
            }
        }
        public long DeletePermit(long permitId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.Permits.Where(m => m.Id == permitId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.Permits.Remove(item);
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

        public long AddBulkPermit(PermitObject permit)
        {
            try
            {
                var permitEntity = ModelMapper.Map<PermitObject, Permit>(permit);

                if (permitEntity == null)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {

                    int permid;

                    var permits = db.Permits.OrderByDescending(l => l.PermitNo).Take(1);

                    if (permits.Any())
                    {
                        permid = permits.First().PermitNo + 1;
                    }
                    else
                    {
                        permid = 1;
                    }
                    var permNum = "";
                    var count = permid.ToString().Length;
                    if (count == 1)
                    {
                        permNum = "00000" + permid;
                    }
                    else if (count == 2)
                    {
                        permNum = "0000" + permid;
                    }
                    else if (count == 3)
                    {
                        permNum = "000" + permid;
                    }
                    else if (count == 4)
                    {
                        permNum = "00" + permid;
                    }
                    else if (count == 5)
                    {
                        permNum = "0" + permid;
                    }
                    else if (count >= 6)
                    {
                        permNum = permid.ToString();
                    }
                    
                    var year = DateTime.Now.Year;

                    var perm = new Permit
                    {
                        IssueDate = DateTime.Now,
                        PermitNo = permid,

                        PermitValue = "DPR/PPIPS/RC" + year + "/" + permNum,
                        DateAdded = DateTime.Now
                    };
                    
                    perm.PermitStatus = (int)PermitStatusEnum.Active;
                    db.Permits.Add(perm);
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
