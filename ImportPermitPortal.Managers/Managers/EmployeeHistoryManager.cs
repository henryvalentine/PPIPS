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
    public class EmployeeHistoryManager
    {
        public ApplicationObject GetApplication(long historyId)
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

        public NotificationObject GetNotification(long historyId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var history = db.NotificationHistories.Find(historyId);


                    var myApplications =
                        db.Notifications.Where(m => m.Id == history.NotificationId)
                            .Include("Importer")
                            .Include("Permit")
                            .Include("Product")
                            .ToList();
                    if (!myApplications.Any())
                    {
                        return new NotificationObject();
                    }

                    var app = myApplications[0];
                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new NotificationObject();
                    }

                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                    importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum), app.Status);
                    importObject.ArrivalDateStr = importObject.ArrivalDate.ToString("dd/MM/yyyy");
                    importObject.QuantityOnVesselStr = importObject.QuantityOnVessel.ToString();
                    importObject.AmountDueStr = importObject.AmountDue.ToString();
                    importObject.DischargeDateStr = importObject.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.QuantityToDischargeStr = importObject.QuantityToDischarge.ToString();

                    importObject.DepotName = app.Depot.Name;

                    importObject.ReferenceCode = app.Invoice.ReferenceCode;

                    importObject.NotificationDocumentObjects = new List<NotificationDocumentObject>();
                    importObject.ProductObject = new ProductObject();
                    importObject.NotificationInspectionObjects = new List<NotificationInspectionObject>();



                    var product = (from p in db.Products
                                   where p.ProductId == importObject.ProductId


                                   select new ProductObject()
                                   {
                                       ProductId = p.ProductId,
                                       Code = p.Code,
                                       Name = p.Name
                                   }).ToList();
                    if (product.Any())
                    {
                        importObject.ProductObject = product[0];

                    }


                   


                    var inspection = (from i in db.NotificationInspections
                                      where i.NotificationId == importObject.Id


                                      select new NotificationInspectionObject()
                                      {
                                          InspectionDate = i.InspectionDate,
                                          InspectorComment = i.InspectorComment
                                      }


                                   ).ToList();
                    if (inspection.Any())
                    {
                        foreach (var item in inspection)
                        {
                            if (item.InspectionDate != null)
                            {
                                item.InspectionDateStr = item.InspectionDate.Value.ToString("dd/MM/yyyy");
                            }

                            importObject.NotificationInspectionObjects.Add(item);
                            importObject.IsReportSubmitted = true;
                        }



                    }



                    var doc = (from ad in app.NotificationDocuments
                               join d in db.Documents on ad.DocumentId equals d.DocumentId

                               select new NotificationDocumentObject()
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
                            importObject.NotificationDocumentObjects.Add(item);
                        }

                    }
                   


                    return importObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public RecertificationObject GetRecertification(long historyId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var history = db.RecertificationHistories.Find(historyId);

                    var docs = new List<DocumentObject>();

                    var recertifications =
                        db.Recertifications.Where(m => m.Id == history.RecertificationId)

                            .ToList();

                    if (!recertifications.Any())
                    {
                        return new RecertificationObject();
                    }

                    var app = recertifications[0];
                    var importObject = ModelMapper.Map<Recertification, RecertificationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new RecertificationObject();
                    }

                    //get the importer Id
                    var importerId = app.Notification.ImporterId;

                    //get the required docs
                    var requiredDocs =
                        db.ImportRequirements.Where(r => r.ImportStageId == (int)AppStage.Recertification).ToList();
                    if (requiredDocs.Any())
                    {
                        foreach (var item in requiredDocs)
                        {
                            var doc = new DocumentObject();
                            var docEntity =
                                db.Documents.Where(
                                    d => d.DocumentTypeId == item.DocumentTypeId && d.ImporterId == importerId)
                                    .ToList();
                            if (docEntity.Any())
                            {
                                doc.DocumentTypeName = docEntity[0].DocumentType.Name;
                                doc.DateUploadedStr = docEntity[0].DateUploaded.ToString("dd/MM/yyyy");
                                doc.DocumentPath = docEntity[0].DocumentPath;

                                docs.Add(doc);
                            }

                        }

                        importObject.DocumentObjects = docs;


                    }


                    importObject.ReferenceCode = app.Notification.Invoice.ReferenceCode;

                    return importObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new RecertificationObject();
            }
        }


        public ApplicationObject PreviousTasks(long id, string userId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(userId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();
                    if (employeeDesk.Any())
                    {
                        var employeeId = employeeDesk[0].Id;

                        var myApplication =
                            db.Applications.Find(id);
                            

                    if (myApplication == null)
                    {
                        return new ApplicationObject();
                    }

                    var app = myApplication;
                    var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new ApplicationObject();
                    }

                    importObject.ProcessingHistoryObjects = new List<ProcessingHistoryObject>();

                  


                        var history = (from h in db.ProcessingHistories where h.EmployeeId.Equals(employeeId)
                                   

                                   select new ProcessingHistoryObject()
                                   {

                                       AssignedTime = h.AssignedTime,

                                       DueTime = h.DueTime,
                                       FinishedTime = h.FinishedTime,
                                       Remarks = h.Remarks,
                                       OutComeCode = h.OutComeCode
                                   }).ToList();
                        if (history.Any())
                        {
                            foreach (var item in history)
                            {
                                item.AssignedTimeStr = item.AssignedTime.ToString();
                                item.DueTimeStr = item.DueTime.ToString();
                                item.ActualDeliveryDateTimeStr = item.FinishedTime.ToString();

                                importObject.ProcessingHistoryObjects.Add(item);
                            }
                            return importObject;
                        }

                        return new ApplicationObject();
                       
                    }

                    return new ApplicationObject();
                }

               
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }


       
        public long AddProcessTracking(ProcessTrackingObject processTracking)
        {
            try
            {
                if (processTracking == null)
                {
                    return -2;
                }

                var processTrackingEntity = ModelMapper.Map<ProcessTrackingObject, ProcessTracking>(processTracking);
                if (processTrackingEntity == null || processTrackingEntity.EmployeeId < 1)
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.ProcessTrackings.Add(processTrackingEntity);
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

        public long UpdateProcessTracking(ProcessTrackingObject processTracking)
        {
            try
            {
                if (processTracking == null)
                {
                    return -2;
                }

                var processTrackingEntity = ModelMapper.Map<ProcessTrackingObject, ProcessTracking>(processTracking);
                if (processTrackingEntity == null || processTrackingEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.ProcessTrackings.Attach(processTrackingEntity);
                    db.Entry(processTrackingEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return processTracking.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<ProcessTrackingObject> GetProcessTrackings()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var processTrackings = db.ProcessTrackings.ToList();
                    if (!processTrackings.Any())
                    {
                        return new List<ProcessTrackingObject>();
                    }
                    var objList = new List<ProcessTrackingObject>();
                    processTrackings.ForEach(app =>
                    {
                        var processTrackingObject = ModelMapper.Map<ProcessTracking, ProcessTrackingObject>(app);
                        if (processTrackingObject != null && processTrackingObject.Id > 0)
                        {
                            objList.Add(processTrackingObject);
                        }
                    });

                    return !objList.Any() ? new List<ProcessTrackingObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<ProcessTrackingObject> GetEmployeeHistorys(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {

                        //get the id of the userprofile table
                        var registeredGuys = db.AspNetUsers.Find(userId);
                        var profileId = registeredGuys.UserProfile.Id;

                        //get the employee id on employee desk table
                        var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                        if (employeeDesk.Any())
                        {
                            var employeeId = employeeDesk[0].Id;

                        var processTrackings =
                            db.ProcessTrackings.Where(m=>m.EmployeeId.Equals(employeeId) && m.StepCode != 0).OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)
.Include("Application")
                                .ToList();
                        if (processTrackings.Any())
                        {
                            var newList = new List<ProcessTrackingObject>();
                            processTrackings.ForEach(app =>
                            {
                                var processTrackingObject = ModelMapper.Map<ProcessTracking, ProcessTrackingObject>(app);
                                if (processTrackingObject != null && processTrackingObject.Id > 0)
                                {
                                    processTrackingObject.AssignedTimeStr = app.AssignedTime.ToString();
                                    processTrackingObject.DueTimeStr = app.DueTime.ToString();
                                    newList.Add(processTrackingObject);
                                }
                            });
                            countG = db.ProcessTrackings.Count();
                            return newList;
                        }
                    }
                }

                }
                countG = 0;
                return new List<ProcessTrackingObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<ProcessTrackingObject>();
            }
        }


        public List<ProcessingHistoryObject> GetPreviousJobsHistorys(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {

                        //get the id of the userprofile table
                        var registeredGuys = db.AspNetUsers.Find(userId);
                        var profileId = registeredGuys.UserProfile.Id;

                        //get the employee id on employee desk table
                        var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                        if (employeeDesk.Any())
                        {
                            var employeeId = employeeDesk[0].Id;

                            var processTrackings =
                                db.ProcessingHistories.Where(m => m.EmployeeId.Equals(employeeId)).OrderByDescending(m => m.Id)
                                    .Skip(tpageNumber).Take(tsize)
    .Include("Application")
                                    .ToList();
                            if (processTrackings.Any())
                            {
                                var newList = new List<ProcessingHistoryObject>();
                                processTrackings.ForEach(app =>
                                {
                                    var processTrackingObject = ModelMapper.Map<ProcessingHistory, ProcessingHistoryObject>(app);
                                    if (processTrackingObject != null && processTrackingObject.Id > 0)
                                    {
                                        processTrackingObject.CompanyName = app.Application.Importer.Name;
                                        processTrackingObject.ReferenceCode = app.Application.Invoice.ReferenceCode;
                                        processTrackingObject.AssignedTimeStr = app.AssignedTime.ToString();
                                       
                                        processTrackingObject.ActualDeliveryDateTimeStr = app.FinishedTime.ToString();
                                        newList.Add(processTrackingObject);
                                    }
                                });
                                countG = db.ProcessingHistories.Count();
                                return newList;
                            }
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

        public List<NotificationHistoryObject> GetPreviousNotificationJobsHistorys(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {

                        //get the id of the userprofile table
                        var registeredGuys = db.AspNetUsers.Find(userId);
                        var profileId = registeredGuys.UserProfile.Id;

                        //get the employee id on employee desk table
                        var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                        if (employeeDesk.Any())
                        {
                            var employeeId = employeeDesk[0].Id;

                            var processTrackings =
                                db.NotificationHistories.Where(m => m.EmployeeId.Equals(employeeId)).OrderByDescending(m => m.Id)
                                    .Skip(tpageNumber).Take(tsize)
                                    .Include("Notification")
                                    .ToList();
                            if (processTrackings.Any())
                            {
                                var newList = new List<NotificationHistoryObject>();
                                //processTrackings.ForEach(app =>
                                //{
                                //    var processTrackingObject = ModelMapper.Map<NotificationHistory, NotificationHistoryObject>(app);
                                //    if (processTrackingObject != null && processTrackingObject.Id > 0)
                                //    {
                                //        processTrackingObject.CompanyName = app.Notification.Importer.Name;
                                //        processTrackingObject.ReferenceCode = app.Notification.Invoice.ReferenceCode;
                                //        processTrackingObject.AssignedTimeStr = app.AssignedTime.ToString();

                                //        processTrackingObject.ActualDeliveryDateTimeStr = app.FinishedTime.ToString();
                                //        newList.Add(processTrackingObject);
                                //    }
                                //});

                                foreach (var item in processTrackings)
                                {
                                    var obj = new NotificationHistoryObject();
                                    obj.Id = item.Id;
                                    obj.CompanyName = item.Notification.Importer.Name;
                                    obj.ReferenceCode = item.Notification.Invoice.RRR;
                                    obj.AssignedTimeStr = item.AssignedTime.ToString();
                                    obj.ActualDeliveryDateTimeStr = item.FinishedTime.ToString();
                                    obj.Remarks = item.Remarks;
                                    newList.Add(obj);
                                }
                                countG = db.NotificationHistories.Count();
                                return newList;
                            }
                        }
                    }

                }
                countG = 0;
                return new List<NotificationHistoryObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationHistoryObject>();
            }
        }

        public List<RecertificationHistoryObject> GetPreviousRecertificationJobsHistorys(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {

                        //get the id of the userprofile table
                        var registeredGuys = db.AspNetUsers.Find(userId);
                        var profileId = registeredGuys.UserProfile.Id;

                        //get the employee id on employee desk table
                        var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                        if (employeeDesk.Any())
                        {
                            var employeeId = employeeDesk[0].Id;

                            var processTrackings =
                                db.RecertificationHistories.Where(m => m.EmployeeId.Equals(employeeId)).OrderByDescending(m => m.Id)
                                    .Skip(tpageNumber).Take(tsize)
    .Include("Recertification")
                                    .ToList();
                            if (processTrackings.Any())
                            {
                                var newList = new List<RecertificationHistoryObject>();
                                processTrackings.ForEach(app =>
                                {
                                    var processTrackingObject = ModelMapper.Map<RecertificationHistory, RecertificationHistoryObject>(app);
                                    if (processTrackingObject != null && processTrackingObject.Id > 0)
                                    {
                                        processTrackingObject.CompanyName = app.Recertification.Notification.Importer.Name;
                                        processTrackingObject.ReferenceCode = app.Recertification.Notification.Invoice.ReferenceCode;
                                        processTrackingObject.AssignedTimeStr = app.AssignedTime.ToString();

                                        processTrackingObject.ActualDeliveryDateTimeStr = app.FinishedTime.ToString();
                                        newList.Add(processTrackingObject);
                                    }
                                });
                                countG = db.RecertificationHistories.Count();
                                return newList;
                            }
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

        public ProcessTrackingObject GetProcessTracking(long processTrackingId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var processTrackings =
                        db.ProcessTrackings.Where(m => m.Id == processTrackingId)
                            .ToList();
                    if (!processTrackings.Any())
                    {
                        return new ProcessTrackingObject();
                    }

                    var app = processTrackings[0];
                    var processTrackingObject = ModelMapper.Map<ProcessTracking, ProcessTrackingObject>(app);
                    if (processTrackingObject == null || processTrackingObject.Id < 1)
                    {
                        return new ProcessTrackingObject();
                    }

                    return processTrackingObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ProcessTrackingObject();
            }
        }

        public List<ProcessTrackingObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var processTrackings = db.ProcessTrackings.Where(m => m.Application.Invoice.ReferenceCode.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (processTrackings.Any())
                    {
                        var newList = new List<ProcessTrackingObject>();
                        processTrackings.ForEach(app =>
                        {
                            var processTrackingObject = ModelMapper.Map<ProcessTracking, ProcessTrackingObject>(app);
                            if (processTrackingObject != null && processTrackingObject.Id > 0)
                            {
                                newList.Add(processTrackingObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<ProcessTrackingObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProcessTrackingObject>();
            }
        }

        public long DeleteProcessTracking(long processTrackingId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.ProcessTrackings.Where(m => m.Id == processTrackingId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.ProcessTrackings.Remove(item);
                    db.SaveChanges();
                    return 5;

                    //var processTracking = db.ProcessTrackinges.Find(processTrackingId);
                    //db.ProcessTrackinges.Remove(processTracking);
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
