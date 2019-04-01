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
    public class RecertificationHistoryManager
    {


        public ApplicationObject GetApplicationFromHistory(long historyId, long userId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var history = db.RecertificationHistories.Find(historyId);

                    var myApplications =
                        db.Applications.Where(m => m.Id == history.RecertificationId)
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
                    importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                    importObject.ApplicationDocumentObjects = new List<ApplicationDocumentObject>();


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


                            importObject.ApplicationItemObjects.Add(im);

                        }
                    });


                    var doc = (from ad in app.ApplicationDocuments
                               join d in db.Documents.Include("DocumentType") on ad.DocumentId equals d.DocumentId

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

                    //check the activity type of the employee
                    var checkEmployee = db.EmployeeDesks.Where(e => e.EmployeeId == userId).ToList();

                    if (checkEmployee.Any())
                    {
                        var actId = checkEmployee[0].ActivityTypeId;
                        var checkActivity =
                            db.StepActivityTypes.Where(c => c.Id == actId).ToList();
                        var activity = "";
                        if (checkActivity.Any())
                        {
                            activity = checkActivity[0].Name;

                        }
                        importObject.Activity = activity;
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

        public long AddRecertificationHistory(RecertificationHistoryObject recertificationHistory)
        {
            try
            {
                if (recertificationHistory == null)
                {
                    return -2;
                }

                var recertificationHistoryEntity = ModelMapper.Map<RecertificationHistoryObject, RecertificationHistory>(recertificationHistory);
                if (recertificationHistoryEntity == null || recertificationHistoryEntity.RecertificationId < 1)
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.RecertificationHistories.Add(recertificationHistoryEntity);
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

        public long UpdateRecertificationHistory(RecertificationHistoryObject recertificationHistory)
        {
            try
            {
                if (recertificationHistory == null)
                {
                    return -2;
                }

                var recertificationHistoryEntity = ModelMapper.Map<RecertificationHistoryObject, RecertificationHistory>(recertificationHistory);
                if (recertificationHistoryEntity == null || recertificationHistoryEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.RecertificationHistories.Attach(recertificationHistoryEntity);
                    db.Entry(recertificationHistoryEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return recertificationHistory.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<RecertificationHistoryObject> GetRecertificationHistorys()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var recertificationHistorys = db.RecertificationHistories.ToList();
                    if (!recertificationHistorys.Any())
                    {
                        return new List<RecertificationHistoryObject>();
                    }
                    var objList = new List<RecertificationHistoryObject>();
                    recertificationHistorys.ForEach(app =>
                    {
                        var recertificationHistoryObject = ModelMapper.Map<RecertificationHistory, RecertificationHistoryObject>(app);
                        if (recertificationHistoryObject != null && recertificationHistoryObject.Id > 0)
                        {
                            objList.Add(recertificationHistoryObject);
                        }
                    });

                    return !objList.Any() ? new List<RecertificationHistoryObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<RecertificationHistoryObject> GetRecertificationHistorys(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var recertificationHistorys =
                            db.RecertificationHistories.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)
.Include("Application")
                                .ToList();
                        if (recertificationHistorys.Any())
                        {
                            var newList = new List<RecertificationHistoryObject>();
                            recertificationHistorys.ForEach(app =>
                            {
                                var recertificationHistoryObject = ModelMapper.Map<RecertificationHistory, RecertificationHistoryObject>(app);
                                if (recertificationHistoryObject != null && recertificationHistoryObject.Id > 0)
                                {
                                    recertificationHistoryObject.ReferenceCode = app.Recertification.Notification.Invoice.ReferenceCode;
                                    recertificationHistoryObject.DateLeftStr = app.FinishedTime.ToString();
                                    recertificationHistoryObject.DueTimeStr = app.DueTime.ToString();
                                    recertificationHistoryObject.AssignedTimeStr = app.AssignedTime.ToString();
                               
                                    var empProfileId = app.EmployeeDesk.EmployeeId;

                                    var pro = db.UserProfiles.Where(a => a.Id == empProfileId).ToList();
                                    if (pro.Any())
                                    {
                                        var personId = pro[0].PersonId;
                                        var person = db.People.Where(p => p.Id == personId).ToList();

                                        if (person.Any())
                                        {
                                            recertificationHistoryObject.EmployeeName = person[0].FirstName + " " +
                                                                                   person[0].LastName;
                                            recertificationHistoryObject.CompanyName = person[0].Importer.Name;
                                        }
                                        
                                    }

                                    
                                    newList.Add(recertificationHistoryObject);
                                }
                            });
                            countG = db.RecertificationHistories.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<RecertificationHistoryObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<RecertificationHistoryObject>();
            }
        }


        public RecertificationHistoryObject GetRecertificationHistory(long recertificationHistoryId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var recertificationHistorys =
                        db.RecertificationHistories.Where(m => m.Id == recertificationHistoryId)
                            .ToList();
                    if (!recertificationHistorys.Any())
                    {
                        return new RecertificationHistoryObject();
                    }

                    var app = recertificationHistorys[0];
                    var recertificationHistoryObject = ModelMapper.Map<RecertificationHistory, RecertificationHistoryObject>(app);
                    if (recertificationHistoryObject == null || recertificationHistoryObject.Id < 1)
                    {
                        return new RecertificationHistoryObject();
                    }

                    return recertificationHistoryObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new RecertificationHistoryObject();
            }
        }

        public List<RecertificationHistoryObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var recertificationHistorys =
                        db.RecertificationHistories.Where(m => m.Recertification.Notification.Invoice.ReferenceCode.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (recertificationHistorys.Any())
                    {
                        var newList = new List<RecertificationHistoryObject>();
                        recertificationHistorys.ForEach(app =>
                        {
                            var recertificationHistoryObject = ModelMapper.Map<RecertificationHistory, RecertificationHistoryObject>(app);
                            if (recertificationHistoryObject != null && recertificationHistoryObject.Id > 0)
                            {
                                newList.Add(recertificationHistoryObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<RecertificationHistoryObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RecertificationHistoryObject>();
            }
        }

        public long DeleteRecertificationHistory(long recertificationHistoryId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.RecertificationHistories.Where(m => m.Id == recertificationHistoryId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.RecertificationHistories.Remove(item);
                    db.SaveChanges();
                    return 5;

                    //var recertificationHistory = db.RecertificationHistoryes.Find(recertificationHistoryId);
                    //db.RecertificationHistoryes.Remove(recertificationHistory);
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
