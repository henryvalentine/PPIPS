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
    public class ProcessingHistoryManager
    {


        public ApplicationObject GetApplicationFromHistory(long historyId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var track = db.ProcessingHistories.Find(historyId);

                    var myApplications =
                        db.Applications.Where(m => m.Id == track.ApplicationId)
                           .Include("Importer")
                            .Include("ApplicationItems")
                            .Include("ApplicationDocuments")
                            .Include("NotificationBankers")
                            .ToList();
                    if (!myApplications.Any())
                    {
                        return new ApplicationObject();
                    }

                    var app = myApplications[0];
                    var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new ApplicationObject();
                    }

                    importObject.ReferenceCode = app.Invoice.ReferenceCode;
                    importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                    importObject.StatusStr = Enum.GetName(typeof(AppStatus), app.ApplicationStatusCode);
                    importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                    importObject.ImporterStr = app.Importer.Name;
                    importObject.ApplicationTypeName = app.ApplicationType.Name;


                    importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                    importObject.ApplicationDocumentObjects = new List<ApplicationDocumentObject>();
                    importObject.StandardRequirementObjects = new List<StandardRequirementObject>();

                    var permApp = db.PermitApplications.Where(p => p.ApplicationId == app.Id).ToList();

                    if (permApp.Any())
                    {
                        var permId = permApp[0].PermitId;
                        var perm = db.Permits.Where(m => m.Id == permId).ToList();
                        if (perm.Any())
                        {
                            importObject.PermitStr = perm[0].PermitValue;
                        }
                        else if (!perm.Any())
                        {
                            importObject.PermitStr = "Nil";
                        }

                    }



                    app.ApplicationItems.ToList().ForEach(u =>
                    {
                        var im = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(u);
                        if (im != null && im.ApplicationId > 0)
                        {
                            im.ProductObject = (from pr in db.Products.Where(x => x.ProductId == im.ProductId)
                                                select new ProductObject
                                                {
                                                    ProductId = pr.ProductId,
                                                    Code = pr.Code,
                                                    Name = pr.Name,
                                                    Availability = pr.Availability
                                                }).ToList()[0];

                            var appCountries = db.ApplicationCountries.Where(a => a.ApplicationItemId == im.Id).Include("Country").ToList();
                            var depotList = db.ThroughPuts.Where(a => a.ApplicationItemId == im.Id).Include("Depot").ToList();
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
                                depotList.ForEach(d =>
                                {
                                    if (string.IsNullOrEmpty(im.DischargeDepotName))
                                    {
                                        im.DischargeDepotName = d.Depot.Name;
                                    }
                                    else
                                    {
                                        im.DischargeDepotName += ", " + d.Depot.Name;
                                    }
                                });
                            }
                            importObject.ApplicationItemObjects.Add(im);

                        }
                    });


                    var doc = (from ad in app.ApplicationDocuments
                               join d in db.Documents on ad.DocumentId equals d.DocumentId

                               select new ApplicationDocumentObject
                               {
                                   DocumentTypeName = d.DocumentType.Name,
                                   DateUploaded = d.DateUploaded,
                                   DocumentPathStr = d.DocumentPath
                               }).ToList();
                    if (doc.Any())
                    {
                        foreach (var item in doc)
                        {
                            item.DateUploadedStr = item.DateUploaded.ToString("dd/MM/yyyy");
                            item.DocumentPathStr = item.DocumentPathStr.Replace("~", "").Replace("/", "\\");
                            importObject.ApplicationDocumentObjects.Add(item);
                        }

                    }

                    importObject.StandardRequirementObjects = new List<StandardRequirementObject>();

                    var doc2 = db.StandardRequirements.Where(s => s.ImporterId == importObject.ImporterId).Include("StandardRequirementType").ToList();

                    if (doc2.Any())
                    {
                        foreach (var standardObj in doc2.Select(item => new StandardRequirementObject
                        {
                            DocumentPath = item.DocumentPath.Replace("~", "").Replace("/", "\\"),
                            DateStr = item.LastUpdated.ToString("dd/MM/yyyy"),
                            StandardRequirementTypeName = item.StandardRequirementType.Name
                        }))
                        {
                            importObject.StandardRequirementObjects.Add(standardObj);
                        }
                    }




                    return importObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public long AddProcessingHistory(ProcessingHistoryObject processingHistory)
        {
            try
            {
                if (processingHistory == null)
                {
                    return -2;
                }

                var processingHistoryEntity = ModelMapper.Map<ProcessingHistoryObject, ProcessingHistory>(processingHistory);
                if (processingHistoryEntity == null || processingHistoryEntity.ApplicationId < 1)
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.ProcessingHistories.Add(processingHistoryEntity);
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

        public long UpdateProcessingHistory(ProcessingHistoryObject processingHistory)
        {
            try
            {
                if (processingHistory == null)
                {
                    return -2;
                }

                var processingHistoryEntity = ModelMapper.Map<ProcessingHistoryObject, ProcessingHistory>(processingHistory);
                if (processingHistoryEntity == null || processingHistoryEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.ProcessingHistories.Attach(processingHistoryEntity);
                    db.Entry(processingHistoryEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return processingHistory.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<ProcessingHistoryObject> GetProcessingHistorys()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var processingHistorys = db.ProcessingHistories.ToList();
                    if (!processingHistorys.Any())
                    {
                        return new List<ProcessingHistoryObject>();
                    }
                    var objList = new List<ProcessingHistoryObject>();
                    processingHistorys.ForEach(app =>
                    {
                        var processingHistoryObject = ModelMapper.Map<ProcessingHistory, ProcessingHistoryObject>(app);
                        if (processingHistoryObject != null && processingHistoryObject.Id > 0)
                        {
                            objList.Add(processingHistoryObject);
                        }
                    });

                    return !objList.Any() ? new List<ProcessingHistoryObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<ProcessingHistoryObject> GetProcessingHistorys(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var processingHistorys =
                            db.ProcessingHistories.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)
.Include("Application")
                                .ToList();
                        if (processingHistorys.Any())
                        {
                            var newList = new List<ProcessingHistoryObject>();
                            processingHistorys.ForEach(app =>
                            {
                                var processingHistoryObject = ModelMapper.Map<ProcessingHistory, ProcessingHistoryObject>(app);
                                if (processingHistoryObject != null && processingHistoryObject.Id > 0)
                                {
                                    processingHistoryObject.ReferenceCode = app.Application.Invoice.ReferenceCode;
                                    processingHistoryObject.DateLeftStr = app.FinishedTime.ToString();
                                    processingHistoryObject.DueTimeStr = app.DueTime.ToString();
                                    processingHistoryObject.AssignedTimeStr = app.AssignedTime.ToString();
                               
                                    var empProfileId = app.EmployeeDesk.EmployeeId;

                                    var pro = db.UserProfiles.Where(a => a.Id == empProfileId).ToList();
                                    if (pro.Any())
                                    {
                                        var personId = pro[0].PersonId;
                                        var person = db.People.Where(p => p.Id == personId).ToList();

                                        if (person.Any())
                                        {
                                            processingHistoryObject.EmployeeName = person[0].FirstName + " " +
                                                                                   person[0].LastName;
                                            processingHistoryObject.CompanyName = person[0].Importer.Name;
                                        }
                                        
                                    }

                                    
                                    newList.Add(processingHistoryObject);
                                }
                            });
                            countG = db.ProcessingHistories.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<ProcessingHistoryObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<ProcessingHistoryObject>();
            }
        }


        public ProcessingHistoryObject GetProcessingHistory(long processingHistoryId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var processingHistorys =
                        db.ProcessingHistories.Where(m => m.Id == processingHistoryId)
                            .ToList();
                    if (!processingHistorys.Any())
                    {
                        return new ProcessingHistoryObject();
                    }

                    var app = processingHistorys[0];
                    var processingHistoryObject = ModelMapper.Map<ProcessingHistory, ProcessingHistoryObject>(app);
                    if (processingHistoryObject == null || processingHistoryObject.Id < 1)
                    {
                        return new ProcessingHistoryObject();
                    }

                    return processingHistoryObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ProcessingHistoryObject();
            }
        }

        public List<ProcessingHistoryObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var processingHistorys =
                        db.ProcessingHistories.Where(m => m.Application.Invoice.ReferenceCode.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (processingHistorys.Any())
                    {
                        var newList = new List<ProcessingHistoryObject>();
                        processingHistorys.ForEach(app =>
                        {
                            var processingHistoryObject = ModelMapper.Map<ProcessingHistory, ProcessingHistoryObject>(app);
                            if (processingHistoryObject != null && processingHistoryObject.Id > 0)
                            {
                                newList.Add(processingHistoryObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<ProcessingHistoryObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProcessingHistoryObject>();
            }
        }

      

        public long DeleteProcessingHistory(long processingHistoryId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.ProcessingHistories.Where(m => m.Id == processingHistoryId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.ProcessingHistories.Remove(item);
                    db.SaveChanges();
                    return 5;

                    //var processingHistory = db.ProcessingHistoryes.Find(processingHistoryId);
                    //db.ProcessingHistoryes.Remove(processingHistory);
                    //db.SaveChanges();
                    //return 5;
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
