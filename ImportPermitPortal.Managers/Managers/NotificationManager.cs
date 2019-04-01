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
    public class NotificationManager
    {
        public long AddNotification(NotificationObject notification)
        {
            try
            {
                if (notification == null)
                {
                    return -2;
                }
                
                using (var db = new ImportPermitEntities())
                {
                    var portId = 0;
                    var existingPorts = db.Ports.Where(k => k.Name.ToLower().Trim() == notification.PortName.ToLower().Trim()).ToList();
                    if (existingPorts.Any())
                    {
                        portId = existingPorts[0].Id;
                    }
                    else
                    {
                        var port = new Port
                        {
                            CountryId = notification.CountryId,
                            Name = notification.PortName
                        };

                        var portEntity = db.Ports.Add(port);
                        db.SaveChanges();
                        portId = portEntity.Id;
                    }

                    if (portId < 1)
                    {
                        return -2;
                    }

                    const int status = (int) AppStatus.Pending;
                    if (db.Notifications.Count(j => j.ProductId == notification.ProductId && j.DischargeDate == notification.DischargeDate &&
                        j.ArrivalDate == notification.ArrivalDate && j.Status == status && j.ImporterId == notification.ImporterId) > 0)
                    {
                        return -3;
                    }
                    var notificationEntity = ModelMapper.Map<NotificationObject, Notification>(notification);
                    if (notificationEntity == null || notificationEntity.ClassificationId < 1)
                    {
                        return -2;
                    }

                    notificationEntity.PortOfOriginId = portId;
                    var returnStatus = db.Notifications.Add(notificationEntity);
                    db.SaveChanges();
                    return returnStatus.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return -2;
            }
        }

        public long UpdateNotification(NotificationObject notification)
        {
            try
            {
                if (notification == null)
                {
                    return -2;
                }
                
                using (var db = new ImportPermitEntities())
                {
                    var portId = 0;
                    var existingPorts = db.Ports.Where(k => k.Name.ToLower().Trim() == notification.PortName.ToLower().Trim()).ToList();
                    if (existingPorts.Any())
                    {
                        portId = existingPorts[0].Id;
                    }
                    else
                    {
                        var port = new Port
                        {
                            CountryId = notification.CountryId,
                            Name = notification.PortName
                        };

                        var portEntity = db.Ports.Add(port);
                        db.SaveChanges();
                        portId = portEntity.Id;
                    }

                    if (portId < 1)
                    {
                        return -2;
                    }
                    const int status = (int)NotificationStatusEnum.Pending;
                    if (db.Notifications.Count(j => j.ProductId == notification.ProductId && j.DischargeDate == notification.DischargeDate &&
                        j.ArrivalDate == notification.ArrivalDate && j.Status == status && j.ImporterId == notification.ImporterId && j.Id != notification.Id) > 0)
                    {
                        return -3;
                    }

                    var notificationEntity = ModelMapper.Map<NotificationObject, Notification>(notification);
                    if (notificationEntity == null || notificationEntity.Id < 1)
                    {
                        return -2;
                    }

                    notificationEntity.PortOfOriginId = portId;
                    db.Notifications.Attach(notificationEntity);
                    db.Entry(notificationEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return notification.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public NotificationObject GetNotification(long notificationId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myNotifications =
                        db.Notifications
                            .Where(z => z.Id == notificationId)
                            .Include("Depot")
                            .Include("Permit")
                            .Include("Port")
                            .Include("Invoice")
                            .Include("Product")
                            .Include("NotificationDocuments")
                            .ToList();

                    if (!myNotifications.Any())
                    {
                        return new NotificationObject();
                    }
                    var app = myNotifications[0];
                    var countries = db.Countries.Where(o => o.Id == app.Port.CountryId).ToList();
                    if (!countries.Any())
                    {
                        return new NotificationObject();
                    }
                    var country = countries[0];
                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new NotificationObject();
                    }

                    const int appStage = (int)AppStage.Notification;
                    var fees = db.Fees.Where(k => k.ImportStageId == appStage).Include("FeeType").Include("ImportStage").ToList();
                    if (!fees.Any())
                    {
                        return new NotificationObject();
                    }
                    var objList = new List<FeeObject>();
                    fees.ForEach(fee =>
                    {
                        var feeObject = ModelMapper.Map<Fee, FeeObject>(fee);
                        if (feeObject != null && feeObject.FeeId > 0)
                        {
                            feeObject.FeeTypeName = fee.FeeType.Name;
                            feeObject.ImportStageName = fee.ImportStage.Name;
                            objList.Add(feeObject);
                        }
                    });
                    importObject.FeeObjects = objList;

                    var expenditionaryFees = db.ExpenditionaryInvoices.Where(e => e.NotificationId == app.Id).Include("Invoice").ToList();
                    if (expenditionaryFees.Any())
                    {
                        expenditionaryFees.ForEach(r =>
                        {
                            var invoiceItems = db.InvoiceItems.Where(i => i.InvoiceId == r.InvoiceId).Include("Fee").ToList();
                            if (invoiceItems.Any())
                            {
                                var fee = invoiceItems[0].Fee;
                                var existing = importObject.FeeObjects.Find(f => f.FeeId == fee.FeeId);
                                if (existing != null && existing.FeeId > 0)
                                {
                                    existing.FeeTypeName = existing.FeeTypeName + " X2 ";
                                    existing.Amount += fee.Amount;
                                }
                            }
                        });
                      
                    }

                    importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                    app.NotificationDocuments.ToList().ForEach(o =>
                    {
                        var docs = db.Documents.Where(c => c.DocumentId == o.DocumentId).Include("DocumentType").ToList();
                        if (docs.Any())
                        {
                            var doc = docs[0];
                            var dc = new DocumentTypeObject
                            {
                                IsFormM = doc.DocumentTypeId == (int)SpecialDocsEnum.Form_M,
                                Uploaded = true,
                                DocumentPath = doc.DocumentPath.Replace("~", ""),
                                DocumentId = doc.DocumentId,
                                DocumentTypeId = doc.DocumentType.DocumentTypeId,
                                Name = doc.DocumentType.Name
                            };
                            importObject.DocumentTypeObjects.Add(dc);
                        }

                    });

                    importObject.RegionId = country.RegionId;
                    importObject.DischargeDepotId = app.Depot.Id;
                    
                    importObject.StatusStr = importObject.Status == (int) NotificationStatusEnum.Paid ? "Pending Submission" : Enum.GetName(typeof(NotificationStatusEnum), importObject.Status).Replace("_", " ");

                    importObject.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum), importObject.ClassificationId).Replace("_", " ");
                    importObject.CargoTypeName = Enum.GetName(typeof(CargoTypeEnum), importObject.CargoInformationTypeId).Replace("_", " ");
                    importObject.ProductCode = app.Product.Code;
                    importObject.CountryCode = country.CountryCode;
                    importObject.CountryName = country.Name;
                    importObject.PortName = app.Port.Name;
                    importObject.CountryId = app.Port.CountryId;
                    importObject.DepotName = app.Depot.Name;
                    importObject.PermitValue = app.Permit.PermitValue;
                    importObject.ProductName = app.Product.Name;
                    importObject.QuantityToDischargeStr = app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                    importObject.AmountDueStr = importObject.FeeObjects.Sum(f => f.Amount).ToString("N");
                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                    //importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.ReferenceCode = app.Invoice.ReferenceCode;
                    importObject.Rrr = app.Invoice.RRR;
                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                    importObject.DocumentTypeObjects = GetNotificationDocumentTypes(importObject.DocumentTypeObjects, importObject.ClassificationId);
                    return importObject;
                } 
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public ApplicationObject GetNotificationProcesses(long notificationId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var importObject = new ApplicationObject
                    {
                        ProcessTrackingObjects = new List<ProcessTrackingObject>(),
                        Processes = new List<ProcessObject>(),
                        ProcessingHistoryObjects = new List<ProcessingHistoryObject>(),
                        Steps = new List<StepObject>()
                    };

                    const int appStage = (int)AppStage.Notification;
                    var processes = db.Processes.Where(p => p.ImportStageId == appStage).Include("Steps").ToList();
                    var notificationQueues = db.NotificationInspectionQueues.Where(p => p.NotificationId == notificationId).Include("EmployeeDesk").Include("Step").ToList();
                    var histories = db.NotificationHistories.Where(p => p.NotificationId == notificationId).ToList();

                    if (!processes.Any())
                    {
                        return new ApplicationObject();
                    }

                    var steps = new List<Step>();
                    processes.ForEach(p =>
                    {
                        steps.AddRange(p.Steps.ToList());
                    });

                    importObject.CurrentEmployeeDesk = new EmployeeDeskObject();

                    if (notificationQueues.Any())
                    {
                        var dtrackingEntity = notificationQueues.ToList()[0];
                        var employees = (from em in db.EmployeeDesks.Where(e => e.Id == dtrackingEntity.EmployeeId)
                                         join pp in db.People on em.UserProfile.PersonId equals pp.Id
                                         select pp).ToList();

                        if (!employees.Any())
                        {
                            return new ApplicationObject();
                        }

                        var currentDek = employees[0];
                        var currentProcess = "";

                        var currentStep = steps.Find(s => s.Id == dtrackingEntity.StepId);
                        if (currentStep == null || currentStep.Id < 1)
                        {
                            return new ApplicationObject();
                        }

                        currentProcess = currentStep.Process.Name;

                        if (string.IsNullOrEmpty(currentStep.Name) || string.IsNullOrEmpty(currentProcess))
                        {
                            return new ApplicationObject();
                        }

                        importObject.ProcessTrackingObjects = new List<ProcessTrackingObject>
                       {
                           new ProcessTrackingObject
                           {
                               Id = dtrackingEntity.Id,
                               ApplicationId = dtrackingEntity.NotificationId,
                               StepId = dtrackingEntity.StepId,
                               EmployeeId  = dtrackingEntity.EmployeeId,
                               StatusId  = dtrackingEntity.StatusId,
                               AssignedTime  = dtrackingEntity.AssignedTime,
                               DueTime  = dtrackingEntity.DueTime,
                               ActualDeliveryDateTime  = dtrackingEntity.ActualDeliveryDateTime,
                               StepCode = dtrackingEntity.StepCode,
                               OutComeCode = dtrackingEntity.OutComeCode,
                               EmployeeName = currentDek.FirstName + " " + currentDek.LastName,
                               OutComeCodeStr = dtrackingEntity.OutComeCode != null ? Enum.GetName(typeof(OutComeCodeEnum), dtrackingEntity.OutComeCode) : "Pending",
                               ProcessName = currentProcess,
                               CurrentStepName = currentStep.Name,
                               AssignedTimeStr = dtrackingEntity.AssignedTime != null ? dtrackingEntity.AssignedTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Pending",
                               DueTimeStr = dtrackingEntity.DueTime != null ? dtrackingEntity.DueTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Unavailable",
                               ActualDeliveryDateTimeStr = dtrackingEntity.ActualDeliveryDateTime != null ? dtrackingEntity.ActualDeliveryDateTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Pending"
                       }};


                    }

                    if (histories.Any())
                    {
                        histories.ForEach(h =>
                        {
                            var employees = (from em in db.EmployeeDesks.Where(e => e.Id == h.EmployeeId)
                                             join pp in db.People on em.UserProfile.PersonId equals pp.Id
                                             select pp).ToList();

                            var step = steps.Find(s => s.Id == h.StepId);
                            if (step != null && step.Id > 0 && employees.Any())
                            {
                                var processName = step.Process.Name;
                                var employee = employees[0];
                                importObject.ProcessingHistoryObjects.Add(new ProcessingHistoryObject
                                {
                                    StepName = step.Name,
                                    EmployeeName = employee.FirstName + " " + employee.LastName,
                                    AssignedTimeStr = h.AssignedTime != null ? h.AssignedTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Pending",
                                    DueTimeStr = h.DueTime != null ? h.DueTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Unavailable",
                                    FinishedTimeStr = h.FinishedTime != null ? h.FinishedTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Pending",
                                    ProcessName = processName,
                                    OutComeCodeStr = h.OutComeCode != null ? Enum.GetName(typeof(OutComeCodeEnum), h.OutComeCode) : "Pending"
                                });
                            }
                        });
                    }

                    return importObject;
                }
            }
            catch (Exception ex)
            {
                return new ApplicationObject();
            }
        }

        public List<PermitObject> GetApplicantsValidPermits(long importerId)
        {
            try
            {
                const int activePermit = (int)PermitStatusEnum.Active;
                using (var db = new ImportPermitEntities())
                {
                    var myPermits = 
                        ( from p in db.Permits.Where(z => z.ImporterId == importerId && z.PermitStatus == activePermit)
                          join ptApp in db.PermitApplications on p.Id equals ptApp.PermitId
                          join ap in db.Applications on ptApp.ApplicationId equals ap.Id
                          join apI in db.ApplicationItems.Where(n => n.OutstandingQuantity > 0)
                          on ap.Id equals apI.ApplicationId
                          select p).ToList();
                    //n => (n.ImportedQuantityValue + n.OutstandingQuantity) < n.EstimatedQuantity
                    if (!myPermits.Any())
                    {
                        return new List<PermitObject>();
                    }

                    var newList = new List<PermitObject>();
                    myPermits.ForEach(o =>
                    {
                        if (!newList.Exists(p => p.PermitValue == o.PermitValue))
                        {
                            var permitObject = ModelMapper.Map<Permit, PermitObject>(o);
                            if (permitObject == null || permitObject.Id < 1)
                            {
                                return;
                            }
                            newList.Add(permitObject);
                        }
                        
                    });

                    return newList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<PermitObject>();
            }
        }

        public NotificationRequirementDetails CheckNotificationSubmit(long id)
        {
            var ntDet = new NotificationRequirementDetails { UnsuppliedDocuments = new List<string>(), ExpenditionaryFee = new FeeObject(), Code = -3, Error = "Internal server error. Notification details could not be validated. Please try again later." };
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notifications = db.Notifications.Where(x => x.Id == id)
                        .Include("NotificationDocuments")
                        .Include("NotificationBankers")
                        .Include("NotificationVessels")
                        .Include("FormMDetails")
                         .Include("Permit")
                         .Include("Invoice")
                        .ToList();
                    
                    if (!notifications.Any())
                    {
                        return ntDet;
                    }

                    var notification = notifications[0];

                    var notificationItems = (from nt in db.Notifications.Where(x => x.Id == notification.Id)
                                join ptApp in db.PermitApplications on nt.PermitId equals ptApp.PermitId
                                join ap in db.Applications on ptApp.ApplicationId equals ap.Id
                                join ai in db.ApplicationItems.Where(c => c.ProductId == notification.ProductId).Include("ProductBankers") on ap.Id equals ai.ApplicationId
                                select ai).ToList();

                    if (!notificationItems.Any())
                    {
                        return ntDet;
                    }

                    var invoiceItems = db.InvoiceItems.Where(i => i.InvoiceId == notification.Id).Include("Fee").ToList();

                    if (!invoiceItems.Any())
                    {
                        return ntDet;
                    }

                    var expenditionaryFee = CheckRulesViolation(notification.DischargeDate);

                    if (expenditionaryFee.ErrorCode < 1)
                    {
                        ntDet.Error = expenditionaryFee.Error;
                        return ntDet;
                    }

                    var expenditionaryInvoices = db.ExpenditionaryInvoices.Where(i => i.NotificationId == notification.Id).Include("Invoice").ToList();
                    
                    if (expenditionaryInvoices.Any())
                    {
                        var unpaid = expenditionaryInvoices.Find(a => a.Invoice.Status < 2 || (!a.Invoice.AmountPaid.Equals(a.Invoice.TotalAmountDue)));

                        if (unpaid != null && unpaid.Id > 0)
                        {
                            ntDet.PaymentTypeId = unpaid.Invoice.PaymentTypeId;
                            expenditionaryFee.ErrorCode = 5;
                        }
                    }

                    var suppliedDocs = notification.NotificationDocuments.ToList();
                    var formMs = notification.FormMDetails.ToList();
                    var availableBankers = notification.NotificationBankers.ToList();
                    var bankers = new List<ProductBanker>();
                    notificationItems.ForEach(v =>
                    {
                        bankers.AddRange(v.ProductBankers);
                    });

                    if (!bankers.Any())
                    {
                        ntDet.Code = -5;
                        ntDet.Error = "The Notification could not be verified. Please try again later.";
                        return ntDet;
                    }
                   
                    var telex = SpecialDocsEnum.Telex_Copy.ToString().Replace("_", " ");
                    var formM = SpecialDocsEnum.Form_M.ToString().Replace("_", " ");
                    bankers.ForEach(b =>
                    {
                        var bankList = db.Banks.Where(c => c.BankId == b.BankId).ToList();
                        if (!bankList.Any())
                        {
                            return;
                        }
                        if (!availableBankers.Exists(h => h.BankId == b.BankId))
                        {
                            var item = telex + " (" + bankList[0].Name + ")";
                            ntDet.UnsuppliedDocuments.Add(item);
                        }

                        if (!formMs.Exists(h => h.BankId == b.BankId))
                        {
                            var item = formM + " (" + bankList[0].Name + ")";
                            ntDet.UnsuppliedDocuments.Add(item);
                        }
                    });

                    var ntDocs = new List<Document>();
                    if (!suppliedDocs.Any())
                    {
                        var sDocs = GetUnsuppliedDocumentTypes(new List<Document>(), notification.ClassificationId);
                        if (sDocs.Any())
                        {
                            ntDet.UnsuppliedDocuments.AddRange(sDocs);
                        }
                        else
                        {
                            ntDet.Code = -5;
                            ntDet.Error = "The Notification could not be verified. Please try again later.";
                            return ntDet;
                        }
                    }
                    else
                    {
                        suppliedDocs.ForEach(a =>
                        {
                            var docs = db.Documents.Where(m => m.DocumentId == a.DocumentId).Include("DocumentType").ToList();
                            if (docs.Any())
                            {
                                var doc = docs[0];
                                ntDocs.Add(doc);
                            }
                        });
                    }

                    ntDet.IsVessselsProvided = notification.NotificationVessels.ToList().Any();
                    
                    var vv = GetUnsuppliedDocumentTypes(ntDocs, notification.ClassificationId);
                    if (vv.Any())
                    {
                        ntDet.UnsuppliedDocuments.AddRange(vv);
                    }

                    //var extraExpenditionaryCriteria = DateTime.Today - notification.DischargeDate;
                     //&& extraExpenditionaryCriteria.Days < expenditionaryFee.VesselDischargeLeadTime
                    
                    if (invoiceItems.All(f => f.Fee.FeeTypeId != expenditionaryFee.FeeTypeId))
                    {
                        if (expenditionaryFee.ErrorCode > 2)
                        {
                            ntDet.IsExpenditionaryApplicable = true;
                            ntDet.PaymentTypeId = notification.Invoice.PaymentTypeId;
                            ntDet.Error = ntDet.Error = "This Notification has attracted an Expenditionary fee of " + expenditionaryFee.AmountStr + " which is yet to be paid."; ;
                            ntDet.ExpenditionaryFee = expenditionaryFee;
                            ntDet.Code = 5;
                        }
                    }
                       
                   
                    if (expenditionaryFee.ErrorCode == 2  && !ntDet.UnsuppliedDocuments.Any())
                    {
                        ntDet.IsExpenditionaryApplicable = false;
                        ntDet.Code = 10;
                    }
                    if (expenditionaryFee.ErrorCode == 2 && ntDet.UnsuppliedDocuments.Any())
                    {
                        ntDet.IsExpenditionaryApplicable = false;
                        ntDet.Code = 5;
                    }
                    
                   
                    ntDet.NotificationId = notification.Id;
                    return ntDet;
                }
            }
            catch (Exception ex)
            {
                return ntDet;
            }
        }

        public bool UpdateDischargeDepot(long notificationId, int depotId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notifications = db.Notifications.Where(x => x.Id == notificationId).ToList();

                    if (!notifications.Any())
                    {
                        return false;
                    }
                    
                    var notification = notifications[0];
                    notification.DischargeDepotId = depotId;
                    db.Entry(notification).State = EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool CheckNotificationSubmit2(long id)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notifications = db.Notifications.Where(x => x.Id == id)
                        .Include("NotificationDocuments")
                        .Include("NotificationBankers")
                        .Include("NotificationVessels")
                        .Include("FormMDetails")
                         .Include("Permit")
                        .ToList();

                    var ntBs = (from nt in db.Notifications.Where(x => x.Id == id)
                                join ptApp in db.PermitApplications on nt.PermitId equals ptApp.PermitId
                                join ap in db.Applications on ptApp.ApplicationId equals ap.Id
                                join ai in db.ApplicationItems.Include("ProductBankers") on ap.Id equals ai.ApplicationId
                                select ai).ToList();

                    if (!notifications.Any())
                    {
                        return false;
                    }

                    if (!ntBs.Any())
                    {
                        return false;
                    }

                    var notification = notifications[0];

                    var expenditionaryFee = CheckRulesViolation(notification.DischargeDate);

                    if (expenditionaryFee.ErrorCode < 1)
                    {
                        return false;
                    }

                    var response = new NotificationRequirementDetails();
                    var suppliedDocs = notification.NotificationDocuments.ToList();
                    var formMs = notification.FormMDetails.ToList();
                    var availableBankers = notification.NotificationBankers.ToList();
                    var bankers = new List<ProductBanker>();
                    ntBs.ForEach(v =>
                    {
                        bankers.AddRange(v.ProductBankers);
                    });

                    if (!bankers.Any())
                    {
                        return false;
                    }

                    var telex = SpecialDocsEnum.Telex_Copy.ToString().Replace("_", " ");
                    var formM = SpecialDocsEnum.Form_M.ToString();

                    bankers.ForEach(b =>
                    {
                        if (!availableBankers.Exists(h => h.BankId == b.BankId))
                        {
                            var item = telex + " (" + b.Bank.Name + ")";
                            response.UnsuppliedDocuments.Add(item);
                        }

                        if (!formMs.Exists(h => h.BankId == b.BankId))
                        {
                            var item = formM + " (" + b.Bank.Name + ")";
                            response.UnsuppliedDocuments.Add(item);
                        }
                    });

                    if (!suppliedDocs.Any())
                    {
                        return false;
                    }

                    if (!notification.NotificationVessels.Any())
                    {
                        return false;
                    }
                    
                    var ntDocs = new List<Document>();
                    suppliedDocs.ForEach(a =>
                    {
                        var docs = db.Documents.Where(m => m.DocumentId == a.DocumentId).Include("DocumentType").ToList();
                        if (docs.Any())
                        {
                            var doc = docs[0];
                            ntDocs.Add(doc);
                        }
                    });

                    var vv = GetUnsuppliedDocumentTypes(ntDocs, notification.ClassificationId);
                    if (vv.Any())
                    {
                        return false;
                    }

                    if (expenditionaryFee.ErrorCode < 1)
                    {
                        return false;
                    }

                    if (expenditionaryFee.ErrorCode > 0 && expenditionaryFee.ErrorCode != 2)
                    {
                        return false;
                    }

                    if (!response.UnsuppliedDocuments.Any())
                    {
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public FeeObject CheckRulesViolation(DateTime dischargeDate)
        {
            try
            {
                const int expenditionary = (int) FeeTypeEnum.Expeditionary;
               
                using (var db = new ImportPermitEntities())
                {
                    var appSettings = db.ImportSettings.ToList();
                    if (!appSettings.Any())
                    {
                        return new FeeObject { ErrorCode = -1, Error = "The Notification Fees details could not be verified. Please try again later."};
                    }

                    var expenditionaryFees = db.Fees.Where(x => x.FeeTypeId == expenditionary).Include("FeeType").ToList();

                    if (!expenditionaryFees.Any())
                    {
                        return new FeeObject { ErrorCode = -1, Error = "The Notification Fees details could not be verified. Please try again later." };
                    }

                    var expff = expenditionaryFees[0];

                    var fee = new FeeObject
                    {
                        FeeId = expff.FeeId,
                        FeeTypeId = expff.FeeTypeId,
                        Amount = expff.Amount,
                        Name = expff.Name,
                        CurrencyCode = expff.CurrencyCode,
                        AmountStr = WebUtility.HtmlDecode("&#8358;") + expff.Amount.ToString("n0"),
                        PrincipalSplit = expff.PrincipalSplit,
                        VendorSplit = expff.VendorSplit,
                        PaymentGatewaySplit = expff.PaymentGatewaySplit,
                        BillableToPrincipal = expff.BillableToPrincipal,
                        FeeTypeName = expff.FeeType.Name,
                        VesselDischargeLeadTime = appSettings[0].VesselDischargeLeadTime
                    };
                    

                    if (dischargeDate == DateTime.Today)
                    {
                        fee.ErrorCode = 5;
                        return fee;
                    }

                    var daysLapse = 0;

                    if (dischargeDate > DateTime.Today)
                    {
                        daysLapse = (dischargeDate - DateTime.Today).Days;
                    }

                    if (dischargeDate < DateTime.Today)
                    {
                        daysLapse = (DateTime.Today - dischargeDate).Days;
                    }

                    if (daysLapse >= appSettings[0].VesselDischargeLeadTime)
                    {
                        fee.ErrorCode = 2;
                    }

                    if (daysLapse < appSettings[0].VesselDischargeLeadTime)
                    {
                        fee.ErrorCode = 5;
                    }

                    return fee;
                }
            }
            catch (Exception ex)
            {
                return new FeeObject { ErrorCode = -1, Error = "The Notification Fees details could not be verified. Please try again later." };
            }
        }

        public NotificationRequirementDetails SubmitNotification(long id)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notifications = db.Notifications.Where(x => x.Id == id).ToList();

                    if (!notifications.Any())
                    {
                        return new NotificationRequirementDetails { Code = -3, Error = "Internal server error. Notification details could not be validated. Please try again later." };
                    }

                    var notification = notifications[0];

                    var ntfDetails = CheckNotificationSubmit(notification.Id);

                    if (ntfDetails.Code > 0 && ntfDetails.Code > 5 && !ntfDetails.UnsuppliedDocuments.Any() && ntfDetails.ExpenditionaryFee.Amount < 1 && ntfDetails.ExpenditionaryFee.FeeId < 1)
                    {
                        notification.Status = (int) NotificationStatusEnum.Submitted;
                        db.Entry(notification).State = EntityState.Modified;
                        db.SaveChanges();
                        return ntfDetails;
                    }
                    return ntfDetails;
                }
            }
            catch (Exception ex)
            {
                return new NotificationRequirementDetails { Code = -3, Error = "Internal server error. Notification details could not be validated. Please try again later." };
            }
        }

        public bool UnSubmitNotification(long id)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notifications = db.Notifications.Where(x => x.Id == id).ToList();

                    if (!notifications.Any())
                    {
                        return false;
                    }

                    var notification = notifications[0];
                    notification.Status = (int)NotificationStatusEnum.Paid;
                    db.Entry(notification).State = EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool CheckAppBankerInfo(long notificationId, string permitValue)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var apps = (from nt in db.Notifications.Where(x => x.Id == notificationId).Include("NotificationBankers")
                                join ptApp in db.PermitApplications.Where(p => p.Permit.PermitValue == permitValue) on nt.PermitId equals ptApp.PermitId
                                join ap in db.Applications on ptApp.ApplicationId equals ap.Id
                                select nt).ToList();

                    if (!apps.Any())
                    {
                        return false;
                    }
                    var bankers = apps[0].NotificationBankers.ToList();
                    if (!bankers.Any())
                    {
                        return false;
                    }
                    return true;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public NotificationObject GetNotificationForEdit(long notificationId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myNotifications =
                        db.Notifications
                            .Where(z => z.Id == notificationId)
                            .Include("Depot")
                            .Include("Permit")
                            .Include("Port")
                            .Include("Invoice")
                            .Include("ImportClass")
                            .Include("Product")
                            .Include("NotificationDocuments")
                            .ToList();

                    if (!myNotifications.Any())
                    {
                        return new NotificationObject();
                    }
                    
                    var app = myNotifications[0];
                    
                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new NotificationObject();
                    }

                    var throughputList = (from pt in db.Permits.Where(p => p.Id == app.PermitId)
                                            join ptAp in db.PermitApplications on pt.Id equals ptAp.PermitId
                                            join ai in db.ApplicationItems on ptAp.ApplicationId equals ai.ApplicationId
                                            join th in db.ThroughPuts on ai.Id equals th.ApplicationItemId
                                            select th).ToList();

                    if (!throughputList.Any())
                    {
                        return new NotificationObject();
                    }
                    
                    importObject.SelectedDepotList = new List<int>();
                    throughputList.ForEach(d =>
                    {
                        importObject.SelectedDepotList.Add(d.DepotId);
                    });
                    
                    importObject.FeeObjects = new List<FeeObject>();

                    var fees = (from invItem in db.InvoiceItems.Where(k => k.InvoiceId == app.Invoice.Id)
                                              join f in db.Fees on invItem.FeeId equals f.FeeId
                                              join ft in db.FeeTypes on f.FeeTypeId equals ft.FeeTypeId
                                              select new FeeObject
                                              {
                                                  FeeTypeName = ft.Name,
                                                  FeeId = f.FeeId,
                                                  Amount = f.Amount,
                                                  FeeTypeId = ft.FeeTypeId

                                              }).ToList();

                    if (!fees.Any())
                    {
                        return new NotificationObject();
                    }

                    fees.ForEach(f =>
                    {
                        f.AmountStr = f.Amount.ToString("n1");
                    });

                    importObject.FeeObjects = fees;

                    long appliedQuantity = 0;
                    var country = "";
                    var products = new List<ApplicationItemObject>();

                    var ims = (from pt in db.Permits.Where(k => k.Id == app.PermitId)
                               join ptApp in db.PermitApplications on pt.Id equals ptApp.PermitId
                               join ap in db.Applications on ptApp.ApplicationId equals ap.Id
                               join it in db.ApplicationItems.Include("ApplicationCountries").Include("ThroughPuts") on ap.Id equals it.ApplicationId
                               select it).ToList();

                    if (!ims.Any())
                    {
                        return new NotificationObject();
                    }

                    ims.ForEach(it =>
                    {
                       
                        var depot = "";
                        var throughputs = new List<ThroughPutObject>();
                         var countries = new List<ApplicationCountryObject>();

                        if (importObject.ProductId == it.ProductId)
                        {
                            appliedQuantity = it.EstimatedQuantity;
                        }

                        it.ThroughPuts.ToList().ForEach(t =>
                        {
                            var dp = db.Depots.Find(t.DepotId);
                            if (dp == null || dp.Id < 1)
                            {
                                return;
                            }
                           
                            if (string.IsNullOrEmpty(depot))
                            {
                                depot = dp.Name;
                            }
                            else
                            {
                                depot += ", " + dp.Name;
                            }

                            var throughput = new ThroughPutObject
                            {
                                Id = t.Id,
                                ApplicationItemId = t.Id,
                                DepotId = t.DepotId,
                                ProductId = t.ProductId,
                                Quantity = t.Quantity,
                                Comment = t.Comment,
                                DocumentId = t.DocumentId,
                                IPAddress = t.IPAddress,
                                DepotObject = new DepotObject()
                            };

                            var depotItem = new DepotObject
                            {
                                Id = dp.Id,
                                Name = dp.Name,
                                JettyId = dp.JettyId,
                                DepotLicense = dp.DepotLicense,
                                ImporterId = dp.ImporterId,
                                IssueDate = dp.IssueDate,
                                ExpiryDate = dp.ExpiryDate
                            };

                            throughput.DepotObject = depotItem;

                            throughputs.Add(throughput);
                            
                        });

                        it.ApplicationCountries.ToList().ForEach(ct =>
                        {
                            var ctr = db.Countries.Find(ct.CountryId);
                            if (ctr == null || ctr.Id < 1)
                            {
                                return;
                            }

                            if (string.IsNullOrEmpty(depot))
                            {
                                country = ctr.Name;
                            }
                            else
                            {
                                country += ", " + ctr.Name;
                            }

                            var appCountry = new ApplicationCountryObject
                            {
                                Id = ct.Id,
                                ApplicationItemId = ct.ApplicationItemId,
                                CountryId = ct.CountryId,
                                CountryObject = new CountryObject
                                {
                                    Id = ctr.Id,
                                    Name = ctr.Name,
                                    CountryCode = ctr.CountryCode,
                                    RegionId = ctr.RegionId
                                }
                            };
                            countries.Add(appCountry);
                            
                        });

                        products.Add(new ApplicationItemObject
                        {
                            Id = it.Id,
                            ApplicationId = it.ApplicationId,
                            ProductId = it.ProductId,
                            EstimatedQuantity = it.EstimatedQuantity,
                            EstimatedValue = it.EstimatedValue,
                            PSFNumber = it.PSFNumber,
                            TotalImportedQuantity = it.TotalImportedQuantity,
                            Code = it.Product.Code,
                            Name = it.Product.Name,
                            Availability = it.Product.Availability,
                            CountryOfOriginName = country,
                            DischargeDepotName = depot,
                            ThroughPutObjects = throughputs,
                            ApplicationCountryObjects = countries
                        });
                    });
                    importObject.AmountDue = app.Invoice.TotalAmountDue;
                    importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                    app.NotificationDocuments.ToList().ForEach(o =>
                    {
                        var docs = db.Documents.Where(c => c.DocumentId == o.DocumentId).Include("DocumentType").ToList();
                        if (docs.Any())
                        {
                            var doc = docs[0];
                            var dc = new DocumentTypeObject
                            {
                                IsFormM = doc.DocumentTypeId == (int)SpecialDocsEnum.Form_M,
                                Uploaded = true,
                                DocumentPath = doc.DocumentPath.Replace("~", ""),
                                DocumentId = doc.DocumentId,
                                DocumentTypeId = doc.DocumentType.DocumentTypeId,
                                Name = doc.DocumentType.Name
                            };
                            importObject.DocumentTypeObjects.Add(dc);
                        }

                    });

                    importObject.ImportClassName = app.ImportClass.Name;
                    importObject.ApplicationItemObjects = products;
                    importObject.DischargeDepotId = app.Depot.Id;
                    importObject.PaymentTypeId = app.Invoice.PaymentTypeId;
                    importObject.StatusStr = Enum.GetName(typeof (NotificationStatusEnum), importObject.Status);
                    var s = Enum.GetName(typeof (CargoTypeEnum), importObject.CargoInformationTypeId);
                    if (s != null)importObject.CargoTypeName = s.Replace("_", " ");
                    importObject.ProductCode = app.Product.Code;
                    importObject.CountryName = country;
                    importObject.PortName = app.Port.Name;
                    importObject.CountryId = app.Port.CountryId;
                    importObject.ApplicationQuantity = appliedQuantity;
                    importObject.DepotName = app.Depot.Name;
                    importObject.PermitValue = app.Permit.PermitValue;
                    importObject.ProductName = app.Product.Name;
                    importObject.QuantityToDischargeStr = app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1").Replace(".0", "");
                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                    importObject.ReferenceCode = app.Invoice.ReferenceCode;
                    importObject.Rrr = app.Invoice.RRR;
                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                    importObject.DocumentTypeObjects = GetNotificationDocumentTypes(importObject.DocumentTypeObjects, importObject.ClassificationId);
                    return importObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public NotificationObject SearchBankNotifications(string referenceCode, long importerId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    const int paidStatus = (int)NotificationStatusEnum.Paid;
                    var myNotifications =
                        (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                         join pb in db.ProductBankers on bnk.BankId equals pb.BankId
                         join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                         join ap in db.Applications on ai.ApplicationId equals ap.Id
                         join ptApp in db.PermitApplications on ap.Id equals ptApp.ApplicationId
                         join nt in db.Notifications.Where(j => (j.Invoice.ReferenceCode.Trim() == referenceCode.Trim() || j.Invoice.RRR.Trim() == referenceCode.Trim()) && j.Invoice.Status >= paidStatus)
                         .Include("Depot")
                        .Include("Permit")
                        .Include("Port")
                        .Include("Importer")
                        .Include("FormMDetails")
                        .Include("Invoice")
                        .Include("Product")
                        .Include("NotificationDocuments")
                         .Include("NotificationBankers")
                        on ptApp.PermitId equals nt.PermitId
                         where (!nt.NotificationBankers.Any(b => b.BankId == bnk.BankId) || !nt.FormMDetails.Any()) && nt.ProductId == ai.ProductId
                         select new { nt, bnk, ap.Id, ai}).ToList();

                    if (!myNotifications.Any())
                    {
                        return new NotificationObject();
                    }
                    
                    var app = myNotifications[0].nt;
                    var bnkr = myNotifications[0].bnk;
                    var id = myNotifications[0].Id;
                    var item = myNotifications[0].ai;
                    var countries = db.Countries.Where(o => o.Id == app.Port.CountryId).ToList();

                    if (!countries.Any())
                    {
                        return new NotificationObject();
                    }
                    
                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new NotificationObject();
                    }

                    importObject.BankId = bnkr.BankId;
                    importObject.ApplicationId = id;
                    importObject.ImporterName = app.Importer.Name;
                    importObject.FormMDetailObjects = new List<FormMDetailObject>();
                    importObject.NotificationBankerObjects = new List<NotificationBankerObject>();
                    
                    if (app.FormMDetails != null && app.FormMDetails.Any())
                    {
                        var formMDetail = app.FormMDetails.ToList()[0];
                        var document = db.Documents.Where(d => d.DocumentId == formMDetail.AttachedDocumentId).Include("DocumentType").ToList();
                        if (document.Any())
                        {
                            var doc = document[0];
                            importObject.FormMDetailObjects.Add(new FormMDetailObject
                            {
                                Id = formMDetail.Id,
                                NotificationId = formMDetail.NotificationId,
                                DateIssued = formMDetail.DateIssued,
                                FormMReference = formMDetail.FormMReference,
                                Quantity = formMDetail.Quantity,
                                LetterOfCreditNo = formMDetail.LetterOfCreditNo,
                                AttachedDocumentId = doc.DocumentId,
                                DateAttached = formMDetail.DateAttached,
                                DateIssuedStr = formMDetail.DateIssued.ToString("dd/MM/yyyy"),
                                DocumentTypeName = doc.DocumentType.Name,
                                DocumentPath = doc.DocumentPath
                            });
                        }
                    }

                    if (app.NotificationBankers != null && app.NotificationBankers.Any())
                    {
                        var banker = app.NotificationBankers.ToList()[0];
                        var document = db.Documents.Where(d => d.DocumentId == banker.AttachedDocumentId).Include("DocumentType").ToList();
                        if (document.Any())
                        {
                            var doc = document[0];
                            importObject.NotificationBankerObjects.Add(new NotificationBankerObject
                            {
                                Id = banker.Id,
                                NotificationId = banker.NotificationId,
                                BankId = banker.BankId,
                                DateAdded = banker.DateAdded,
                                FinancedQuantity = banker.FinancedQuantity,
                                TransactionAmount = banker.TransactionAmount,
                                ActualQuantity = banker.ActualQuantity,
                                AttachedDocumentId = banker.AttachedDocumentId,
                                ProductId = banker.ProductId,
                                LastUpdateBy = banker.LastUpdateBy,
                                ApprovedBy = banker.ApprovedBy,
                                IpAddress = banker.IpAddress,
                                FinLetterPath = doc.DocumentPath
                            });
                        }
                    }

                    importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                    const int formM = (int)SpecialDocsEnum.Form_M;
                    const int telex = (int)SpecialDocsEnum.Telex_Copy;
                    var appDocs = app.NotificationDocuments.ToList();
                    if (appDocs.Any())
                    {
                        appDocs.ForEach(o =>
                        {
                            var docs = db.Documents.Where(c => c.DocumentId == o.DocumentId).Include("DocumentType").ToList();
                            if (docs.Any())
                            {
                                var doc = docs[0];
                                if (doc.DocumentTypeId != formM && doc.DocumentTypeId != telex)
                                {
                                    importObject.DocumentTypeObjects.Add(new DocumentTypeObject
                                    {
                                        DocumentId = o.DocumentId,
                                        Uploaded = true,
                                        IsFormM = doc.DocumentTypeId == formM,
                                        DocumentPath = doc.DocumentPath.Replace("~", ""),
                                        DocumentTypeId = doc.DocumentType.DocumentTypeId,
                                        Name = doc.DocumentType.Name
                                    });
                                }
                            }

                        });
                    }
                    
                    importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum), importObject.Status);
                    var name = Enum.GetName(typeof(NotificationClassEnum), importObject.ClassificationId);
                    if (name != null)importObject.NotificationTypeName = name.Replace("_", " ");
                    var s = Enum.GetName(typeof(CargoTypeEnum), importObject.CargoInformationTypeId);
                    if (s != null)importObject.CargoTypeName = s.Replace("_", " ");
                    importObject.ProductCode = app.Product.Code;
                    importObject.CountryName = countries[0].Name;
                    importObject.PortName = app.Port.Name;
                    importObject.Rrr = app.Invoice.RRR;
                    importObject.ReferenceCode = app.Invoice.ReferenceCode;
                    importObject.CountryId = app.Port.CountryId;
                    importObject.ApplicationQuantity = item.EstimatedQuantity;
                    importObject.OutStandingQuantity = item.OutstandingQuantity;
                    importObject.DepotName = app.Depot.Name;
                    importObject.PermitValue = app.Permit.PermitValue;
                    importObject.ProductName = app.Product.Name;
                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0 ? app.Permit.PermitValue : "Not Available";
                    importObject.QuantityToDischargeStr = app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1").Replace(".0", "");
                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                    importObject.DocumentTypeObjects = GetNotificationDocumentTypes(importObject.DocumentTypeObjects, importObject.ClassificationId);
                    return importObject;
                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public NotificationObject GetBankNotifications(long notificationId, long importerId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    const int paidStatus = (int)NotificationStatusEnum.Paid;
                    var myNotifications =
                        (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                         join pb in db.ProductBankers on bnk.BankId equals pb.BankId
                         join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                         join ap in db.Applications on ai.ApplicationId equals ap.Id
                         join ptApp in db.PermitApplications on ap.Id equals ptApp.ApplicationId
                         join nt in db.Notifications.Where(j => j.Id == notificationId && j.Invoice.Status >= paidStatus)
                         .Include("Depot")
                        .Include("Permit")
                        .Include("Port")
                        .Include("Importer")
                        .Include("FormMDetails")
                        .Include("Invoice")
                        .Include("Product")
                        .Include("NotificationDocuments")
                         .Include("NotificationBankers")
                        on ptApp.PermitId equals nt.PermitId
                         where (!nt.NotificationBankers.Any(b => b.BankId == bnk.BankId) || !nt.FormMDetails.Any()) && nt.ProductId == ai.ProductId
                         select new { nt, bnk, ap.Id, ai }).ToList();

                    if (!myNotifications.Any())
                    {
                        return new NotificationObject();
                    }

                    var app = myNotifications[0].nt;
                    var bnkr = myNotifications[0].bnk;
                    var id = myNotifications[0].Id;
                    var item = myNotifications[0].ai;
                    var countries = db.Countries.Where(o => o.Id == app.Port.CountryId).ToList();

                    if (!countries.Any())
                    {
                        return new NotificationObject();
                    }

                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new NotificationObject();
                    }

                    importObject.BankId = bnkr.BankId;
                    importObject.ApplicationId = id;
                    importObject.ImporterName = app.Importer.Name;
                    importObject.FormMDetailObjects = new List<FormMDetailObject>();
                    importObject.NotificationBankerObjects = new List<NotificationBankerObject>();

                    if (app.FormMDetails != null && app.FormMDetails.Any())
                    {
                        var formMDetail = app.FormMDetails.ToList()[0];
                        var document = db.Documents.Where(d => d.DocumentId == formMDetail.AttachedDocumentId).Include("DocumentType").ToList();
                        if (document.Any())
                        {
                            var doc = document[0];
                            importObject.FormMDetailObjects.Add(new FormMDetailObject
                            {
                                Id = formMDetail.Id,
                                NotificationId = formMDetail.NotificationId,
                                DateIssued = formMDetail.DateIssued,
                                FormMReference = formMDetail.FormMReference,
                                Quantity = formMDetail.Quantity,
                                LetterOfCreditNo = formMDetail.LetterOfCreditNo,
                                AttachedDocumentId = doc.DocumentId,
                                DateAttached = formMDetail.DateAttached,
                                DateIssuedStr = formMDetail.DateIssued.ToString("dd/MM/yyyy"),
                                DocumentTypeName = doc.DocumentType.Name,
                                DocumentPath = doc.DocumentPath
                            });
                        }
                    }

                    if (app.NotificationBankers != null && app.NotificationBankers.Any())
                    {
                        var banker = app.NotificationBankers.ToList()[0];
                        var document = db.Documents.Where(d => d.DocumentId == banker.AttachedDocumentId).Include("DocumentType").ToList();
                        if (document.Any())
                        {
                            var doc = document[0];
                            importObject.NotificationBankerObjects.Add(new NotificationBankerObject
                            {
                                Id = banker.Id,
                                NotificationId = banker.NotificationId,
                                BankId = banker.BankId,
                                DateAdded = banker.DateAdded,
                                FinancedQuantity = banker.FinancedQuantity,
                                TransactionAmount = banker.TransactionAmount,
                                ActualQuantity = banker.ActualQuantity,
                                AttachedDocumentId = banker.AttachedDocumentId,
                                ProductId = banker.ProductId,
                                LastUpdateBy = banker.LastUpdateBy,
                                ApprovedBy = banker.ApprovedBy,
                                IpAddress = banker.IpAddress,
                                FinLetterPath = doc.DocumentPath
                            });
                        }
                    }

                    importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                    const int formM = (int)SpecialDocsEnum.Form_M;
                    const int telex = (int)SpecialDocsEnum.Telex_Copy;
                    var appDocs = app.NotificationDocuments.ToList();
                    if (appDocs.Any())
                    {
                        appDocs.ForEach(o =>
                        {
                            var docs = db.Documents.Where(c => c.DocumentId == o.DocumentId).Include("DocumentType").ToList();
                            if (docs.Any())
                            {
                                var doc = docs[0];
                                if (doc.DocumentTypeId != formM && doc.DocumentTypeId != telex)
                                {
                                    importObject.DocumentTypeObjects.Add(new DocumentTypeObject
                                    {
                                        DocumentId = o.DocumentId,
                                        Uploaded = true,
                                        IsFormM = doc.DocumentTypeId == formM,
                                        DocumentPath = doc.DocumentPath.Replace("~", ""),
                                        DocumentTypeId = doc.DocumentType.DocumentTypeId,
                                        Name = doc.DocumentType.Name
                                    });
                                }
                            }

                        });
                    }

                    importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum), importObject.Status);
                    var name = Enum.GetName(typeof(NotificationClassEnum), importObject.ClassificationId);
                    if (name != null) importObject.NotificationTypeName = name.Replace("_", " ");
                    var s = Enum.GetName(typeof(CargoTypeEnum), importObject.CargoInformationTypeId);
                    if (s != null) importObject.CargoTypeName = s.Replace("_", " ");
                    importObject.ProductCode = app.Product.Code;
                    importObject.CountryName = countries[0].Name;
                    importObject.PortName = app.Port.Name;
                    importObject.Rrr = app.Invoice.RRR;
                    importObject.ReferenceCode = app.Invoice.ReferenceCode;
                    importObject.CountryId = app.Port.CountryId;
                    importObject.ApplicationQuantity = item.EstimatedQuantity;
                    importObject.OutStandingQuantity = item.OutstandingQuantity;
                    importObject.DepotName = app.Depot.Name;
                    importObject.PermitValue = app.Permit.PermitValue;
                    importObject.ProductName = app.Product.Name;
                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0 ? app.Permit.PermitValue : "Not Available";
                    importObject.QuantityToDischargeStr = app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1").Replace(".0", "");
                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                    importObject.DocumentTypeObjects = GetNotificationDocumentTypes(importObject.DocumentTypeObjects, importObject.ClassificationId);
                    return importObject;
                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public NotificationObject SearchBankProcessedNotifications(string referenceCode, long importerId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    const int paidStatus = (int)NotificationStatusEnum.Paid;
                    var myNotifications =
                        (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                         join pb in db.ProductBankers on bnk.BankId equals pb.BankId
                         join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                         join ap in db.Applications on ai.ApplicationId equals ap.Id
                         join ptApp in db.PermitApplications on ap.Id equals ptApp.ApplicationId
                         join nt in db.Notifications.Where(j => (j.Invoice.ReferenceCode.Trim() == referenceCode.Trim() || j.Invoice.RRR.Trim() == referenceCode.Trim()) && j.Invoice.Status >= paidStatus)
                         .Include("Depot")
                        .Include("Permit")
                        .Include("Port")
                        .Include("Importer")
                        .Include("FormMDetails")
                        .Include("Invoice")
                        .Include("Product")
                        .Include("NotificationDocuments")
                         .Include("NotificationBankers")
                        on ptApp.PermitId equals nt.PermitId
                         where nt.ProductId == ai.ProductId
                         select new { nt, bnk, ap.Id, ai }).ToList();

                    if (!myNotifications.Any())
                    {
                        return new NotificationObject();
                    }

                    var app = myNotifications[0].nt;
                    var bnkr = myNotifications[0].bnk;
                    var id = myNotifications[0].Id;
                    var item = myNotifications[0].ai;
                    var countries = db.Countries.Where(o => o.Id == app.Port.CountryId).ToList();

                    if (!countries.Any())
                    {
                        return new NotificationObject();
                    }

                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new NotificationObject();
                    }

                    importObject.BankId = bnkr.BankId;
                    importObject.ApplicationId = id;
                    importObject.ImporterName = app.Importer.Name;
                    importObject.FormMDetailObjects = new List<FormMDetailObject>();
                    importObject.NotificationBankerObjects = new List<NotificationBankerObject>();

                    if (app.FormMDetails != null && app.FormMDetails.Any())
                    {
                        var formMDetails = app.FormMDetails.Where(b => b.BankId == bnkr.BankId).ToList();
                        if (formMDetails.Any())
                        {
                            var formMDetail = formMDetails[0];
                            var document = db.Documents.Where(d => d.DocumentId == formMDetail.AttachedDocumentId).Include("DocumentType").ToList();
                            if (document.Any())
                            {
                                var doc = document[0];
                                importObject.FormMDetailObjects.Add(new FormMDetailObject
                                {
                                    Id = formMDetail.Id,
                                    NotificationId = formMDetail.NotificationId,
                                    DateIssued = formMDetail.DateIssued,
                                    FormMReference = formMDetail.FormMReference,
                                    Quantity = formMDetail.Quantity,
                                    LetterOfCreditNo = formMDetail.LetterOfCreditNo,
                                    AttachedDocumentId = doc.DocumentId,
                                    DateAttached = formMDetail.DateAttached,
                                    DateIssuedStr = formMDetail.DateIssued.ToString("dd/MM/yyyy"),
                                    DocumentTypeName = doc.DocumentType.Name,
                                    DocumentPath = doc.DocumentPath
                                });
                            }
                        }
                    }

                    if (app.NotificationBankers != null && app.NotificationBankers.Any())
                    {
                        var bankers = app.NotificationBankers.Where(b => b.BankId == bnkr.BankId).ToList();
                        if (bankers.Any())
                        {
                            var banker = bankers[0];
                            var document = db.Documents.Where(d => d.DocumentId == banker.AttachedDocumentId).Include("DocumentType").ToList();
                            if (document.Any())
                            {
                                var doc = document[0];
                                importObject.NotificationBankerObjects.Add(new NotificationBankerObject
                                {
                                    Id = banker.Id,
                                    NotificationId = banker.NotificationId,
                                    BankId = banker.BankId,
                                    DateAdded = banker.DateAdded,
                                    FinancedQuantity = banker.FinancedQuantity,
                                    TransactionAmount = banker.TransactionAmount,
                                    ActualQuantity = banker.ActualQuantity,
                                    AttachedDocumentId = banker.AttachedDocumentId,
                                    ProductId = banker.ProductId,
                                    LastUpdateBy = banker.LastUpdateBy,
                                    ApprovedBy = banker.ApprovedBy,
                                    IpAddress = banker.IpAddress,
                                    FinLetterPath = doc.DocumentPath
                                });
                            }
                        }
                    }

                    importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                    const int formM = (int)SpecialDocsEnum.Form_M;
                    const int telex = (int)SpecialDocsEnum.Telex_Copy;
                    var appDocs = app.NotificationDocuments.ToList();
                    if (appDocs.Any())
                    {
                        appDocs.ForEach(o =>
                        {
                            var docs = db.Documents.Where(c => c.DocumentId == o.DocumentId).Include("DocumentType").ToList();
                            if (docs.Any())
                            {
                                var doc = docs[0];
                                if (doc.DocumentTypeId != formM && doc.DocumentTypeId != telex)
                                {
                                    importObject.DocumentTypeObjects.Add(new DocumentTypeObject
                                    {
                                        DocumentId = o.DocumentId,
                                        Uploaded = true,
                                        IsFormM = doc.DocumentTypeId == formM,
                                        DocumentPath = doc.DocumentPath.Replace("~", ""),
                                        DocumentTypeId = doc.DocumentType.DocumentTypeId,
                                        Name = doc.DocumentType.Name
                                    });
                                }
                            }

                        });
                    }

                    importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum), importObject.Status);
                    var name = Enum.GetName(typeof(NotificationClassEnum), importObject.ClassificationId);
                    if (name != null) importObject.NotificationTypeName = name.Replace("_", " ");
                    var s = Enum.GetName(typeof(CargoTypeEnum), importObject.CargoInformationTypeId);
                    if (s != null) importObject.CargoTypeName = s.Replace("_", " ");
                    importObject.ProductCode = app.Product.Code;
                    importObject.CountryName = countries[0].Name;
                    importObject.PortName = app.Port.Name;
                    importObject.Rrr = app.Invoice.RRR;
                    importObject.ReferenceCode = app.Invoice.ReferenceCode;
                    importObject.CountryId = app.Port.CountryId;
                    importObject.ApplicationQuantity = item.EstimatedQuantity;
                    importObject.OutStandingQuantity = item.OutstandingQuantity;
                    importObject.DepotName = app.Depot.Name;
                    importObject.PermitValue = app.Permit.PermitValue;
                    importObject.ProductName = app.Product.Name;
                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0 ? app.Permit.PermitValue : "Not Available";
                    importObject.QuantityToDischargeStr = app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1").Replace(".0", "");
                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                    importObject.DocumentTypeObjects = GetNotificationDocumentTypes(importObject.DocumentTypeObjects, importObject.ClassificationId);
                    return importObject;
                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }
        
         public NotificationObject GetBankProcessedNotifications(long notificationId, long importerId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    const int paidStatus = (int)NotificationStatusEnum.Paid;
                    var myNotifications =
                        (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                         join pb in db.ProductBankers on bnk.BankId equals pb.BankId
                         join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                         join ap in db.Applications on ai.ApplicationId equals ap.Id
                         join ptApp in db.PermitApplications on ap.Id equals ptApp.ApplicationId
                         join nt in db.Notifications.Where(j => j.Id == notificationId && j.Invoice.Status >= paidStatus)
                         .Include("Depot")
                        .Include("Permit")
                        .Include("Port")
                        .Include("Importer")
                        .Include("FormMDetails")
                        .Include("Invoice")
                        .Include("Product")
                        .Include("NotificationDocuments")
                         .Include("NotificationBankers")
                        on ptApp.PermitId equals nt.PermitId
                         where nt.ProductId == ai.ProductId
                         select new { nt, bnk, ap.Id, ai }).ToList();

                    if (!myNotifications.Any())
                    {
                        return new NotificationObject();
                    }

                    var app = myNotifications[0].nt;
                    var bnkr = myNotifications[0].bnk;
                    var id = myNotifications[0].Id;
                    var item = myNotifications[0].ai;
                    var countries = db.Countries.Where(o => o.Id == app.Port.CountryId).ToList();

                    if (!countries.Any())
                    {
                        return new NotificationObject();
                    }

                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new NotificationObject();
                    }

                    importObject.BankId = bnkr.BankId;
                    importObject.ApplicationId = id;
                    importObject.ImporterName = app.Importer.Name;
                    importObject.FormMDetailObjects = new List<FormMDetailObject>();
                    importObject.NotificationBankerObjects = new List<NotificationBankerObject>();

                    if (app.FormMDetails != null && app.FormMDetails.Any())
                    {
                        var formMDetails = app.FormMDetails.Where(b => b.BankId == bnkr.BankId).ToList();
                        if (formMDetails.Any())
                        {
                            var formMDetail = formMDetails[0];
                            var document = db.Documents.Where(d => d.DocumentId == formMDetail.AttachedDocumentId).Include("DocumentType").ToList();
                            if (document.Any())
                            {
                                var doc = document[0];
                                importObject.FormMDetailObjects.Add(new FormMDetailObject
                                {
                                    Id = formMDetail.Id,
                                    NotificationId = formMDetail.NotificationId,
                                    DateIssued = formMDetail.DateIssued,
                                    FormMReference = formMDetail.FormMReference,
                                    Quantity = formMDetail.Quantity,
                                    LetterOfCreditNo = formMDetail.LetterOfCreditNo,
                                    AttachedDocumentId = doc.DocumentId,
                                    DateAttached = formMDetail.DateAttached,
                                    DateIssuedStr = formMDetail.DateIssued.ToString("dd/MM/yyyy"),
                                    DocumentTypeName = doc.DocumentType.Name,
                                    DocumentPath = doc.DocumentPath
                                });
                            }
                        }
                    }

                    if (app.NotificationBankers != null && app.NotificationBankers.Any())
                    {
                        var bankers = app.NotificationBankers.Where(b => b.BankId == bnkr.BankId).ToList();
                        if (bankers.Any())
                        {
                            var banker = bankers[0];
                            var document = db.Documents.Where(d => d.DocumentId == banker.AttachedDocumentId).Include("DocumentType").ToList();
                            if (document.Any())
                            {
                                var doc = document[0];
                                importObject.NotificationBankerObjects.Add(new NotificationBankerObject
                                {
                                    Id = banker.Id,
                                    NotificationId = banker.NotificationId,
                                    BankId = banker.BankId,
                                    DateAdded = banker.DateAdded,
                                    FinancedQuantity = banker.FinancedQuantity,
                                    TransactionAmount = banker.TransactionAmount,
                                    ActualQuantity = banker.ActualQuantity,
                                    AttachedDocumentId = banker.AttachedDocumentId,
                                    ProductId = banker.ProductId,
                                    LastUpdateBy = banker.LastUpdateBy,
                                    ApprovedBy = banker.ApprovedBy,
                                    IpAddress = banker.IpAddress,
                                    FinLetterPath = doc.DocumentPath
                                });
                            }
                        }
                    }

                    importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                    const int formM = (int)SpecialDocsEnum.Form_M;
                    const int telex = (int)SpecialDocsEnum.Telex_Copy;
                    var appDocs = app.NotificationDocuments.ToList();
                    if (appDocs.Any())
                    {
                        appDocs.ForEach(o =>
                        {
                            var docs = db.Documents.Where(c => c.DocumentId == o.DocumentId).Include("DocumentType").ToList();
                            if (docs.Any())
                            {
                                var doc = docs[0];
                                if (doc.DocumentTypeId != formM && doc.DocumentTypeId != telex)
                                {
                                    importObject.DocumentTypeObjects.Add(new DocumentTypeObject
                                    {
                                        DocumentId = o.DocumentId,
                                        Uploaded = true,
                                        IsFormM = doc.DocumentTypeId == formM,
                                        DocumentPath = doc.DocumentPath.Replace("~", ""),
                                        DocumentTypeId = doc.DocumentType.DocumentTypeId,
                                        Name = doc.DocumentType.Name
                                    });
                                }
                            }

                        });
                    }

                    importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum), importObject.Status);
                    var name = Enum.GetName(typeof(NotificationClassEnum), importObject.ClassificationId);
                    if (name != null) importObject.NotificationTypeName = name.Replace("_", " ");
                    var s = Enum.GetName(typeof(CargoTypeEnum), importObject.CargoInformationTypeId);
                    if (s != null) importObject.CargoTypeName = s.Replace("_", " ");
                    importObject.ProductCode = app.Product.Code;
                    importObject.CountryName = countries[0].Name;
                    importObject.PortName = app.Port.Name;
                    importObject.Rrr = app.Invoice.RRR;
                    importObject.ReferenceCode = app.Invoice.ReferenceCode;
                    importObject.CountryId = app.Port.CountryId;
                    importObject.ApplicationQuantity = item.EstimatedQuantity;
                    importObject.OutStandingQuantity = item.OutstandingQuantity;
                    importObject.DepotName = app.Depot.Name;
                    importObject.PermitValue = app.Permit.PermitValue;
                    importObject.ProductName = app.Product.Name;
                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0 ? app.Permit.PermitValue : "Not Available";
                    importObject.QuantityToDischargeStr = app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1").Replace(".0", "");
                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                    importObject.DocumentTypeObjects = GetNotificationDocumentTypes(importObject.DocumentTypeObjects, importObject.ClassificationId);
                    return importObject;
                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }
        
        public List<DocumentTypeObject> GetNotificationDocumentTypes(List<DocumentTypeObject> appDocs, int classId)
        {
            try
            {
                const int formM = (int)SpecialDocsEnum.Form_M;
                const int telex = (int)SpecialDocsEnum.Telex_Copy;
                const int appStage = (int)AppStage.Notification;
                const int applicant = (int)AppRole.Applicant;
                var depotOwner = ((int)AppRole.Depot_Owner).ToString();
                var banker = ((int)AppRole.Banker).ToString();

                using (var db = new ImportPermitEntities())
                {
                    var classificationReqDocs = db.ImportClassificationRequirements.Where(ic => ic.ClassificationId == classId && ic.ImportStageId == appStage).Include("DocumentType").ToList();
                    if (classificationReqDocs.Any())
                    {
                        classificationReqDocs.ForEach(p =>
                        {
                            var reqDocs = db.DocumentTypeRights.Where(g => g.DocumentTypeId == p.DocumentTypeId).Include("DocumentType").ToList();
                            if (reqDocs.Any())
                            {
                                var existing = appDocs.Find(z => z.DocumentTypeId == p.DocumentTypeId);
                                var r = reqDocs[0];
                                if (existing == null || existing.DocumentTypeId < 1)
                                {
                                    if (r.DocumentTypeId != formM && r.DocumentTypeId != telex)
                                    {
                                        appDocs.Add(new DocumentTypeObject
                                        {
                                            DocumentTypeId = p.DocumentTypeId,
                                            Uploaded = false,
                                            Name = p.DocumentType.Name,
                                            IsFormM = p.DocumentTypeId == formM,
                                            StageId = int.Parse(r.RoleId) == (int) AppRole.Applicant ? 1 : 0,
                                            IsBankDoc = r.RoleId == banker,
                                            IsDepotDoc = r.RoleId == depotOwner
                                        });
                                    }
                                }
                                else
                                {
                                    existing.StageId = int.Parse(r.RoleId) == applicant ? 1 : 0;
                                    existing.IsBankDoc = r.RoleId == banker;
                                    existing.IsDepotDoc = r.RoleId == depotOwner;
                                }
                            }
                        });
                    }
                    
                    return !appDocs.Any() ? new List<DocumentTypeObject>() : appDocs;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<string> GetUnsuppliedDocumentTypes(List<Document> appDocs, int classId)
        {
            try
            {
                const int telex = (int)SpecialDocsEnum.Telex_Copy;
                const int formM = (int)SpecialDocsEnum.Form_M;
                const int appStage = (int)AppStage.Notification;
                var unsuppliedDocuments = new List<string>();
                using (var db = new ImportPermitEntities())
                {
                    var classificationReqDocs = db.ImportClassificationRequirements.Where(ic => ic.ClassificationId == classId && ic.ImportStageId == appStage).Include("DocumentType").ToList();
                    
                    if (classificationReqDocs.Any())
                    {
                        classificationReqDocs.ForEach(p =>
                        {
                            if (p.DocumentTypeId != telex && p.DocumentTypeId != formM)
                            {
                                var reqDocs = db.DocumentTypeRights.Where(g => g.DocumentTypeId == p.DocumentTypeId).Include("DocumentType").ToList();
                                if (reqDocs.Any())
                                {
                                    var existing = appDocs.Find(z => z.DocumentTypeId == p.DocumentTypeId);
                                    if (existing == null || existing.DocumentTypeId < 1)
                                    {
                                        unsuppliedDocuments.Add(p.DocumentType.Name);
                                    }
                                }
                            }
                        });
                    }

                    return !unsuppliedDocuments.Any() ? new List<string>() : unsuppliedDocuments;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<int> CheckNtDocs(List<int> appDocs, int classId)
        {
            try
            {
                var newList = new List<int>();
                const int appStage = (int)AppStage.Notification;
                using (var db = new ImportPermitEntities())
                {
                    var classificationReqDocs = db.ImportClassificationRequirements.Where(ic => ic.ClassificationId == classId && ic.ImportStageId == appStage).ToList();
                    if (classificationReqDocs.Any())
                    {
                        classificationReqDocs.ForEach(p =>
                        {
                            var reqDocs = db.DocumentTypeRights.Where(g => g.DocumentTypeId == p.DocumentTypeId).ToList();
                            if (reqDocs.Any())
                            {
                                var existing = appDocs.Find(z => z == p.DocumentTypeId);
                                if (existing < 1)
                                {
                                    newList.Add(p.DocumentTypeId);
                                }
                            }
                        });
                    }

                    var impStage = db.ImportRequirements.Where(ic => ic.ImportStageId == appStage).ToList();
                    if (impStage.Any())
                    {
                        impStage.ForEach(p =>
                        {
                            var reqDocs = db.DocumentTypeRights.Where(g => g.DocumentTypeId == p.DocumentTypeId).ToList();
                            if (reqDocs.Any())
                            {
                                var existing = appDocs.Find(z => z == p.DocumentTypeId);
                                if (existing < 1)
                                {
                                    newList.Add(p.DocumentTypeId);
                                }
                            }
                        });
                    }

                    return !newList.Any() ? new List<int>() : newList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<int>();
            }
        }

        public NotificationObject GetBankNotification(long notificationId, long importerId)
        {

            const int paid = (int)NotificationStatusEnum.Paid;
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myNotifications =
                        (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                         join pb in db.ProductBankers on bnk.BankId equals pb.BankId
                         join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                         join ap in db.Applications on ai.ApplicationId equals ap.Id
                         join ptApp in db.PermitApplications on ap.Id equals ptApp.ApplicationId
                         join nt in db.Notifications.Where(j => j.Id == notificationId && j.Invoice.Status == paid && j.NotificationDocuments.Any())
                         .Include("Depot")
                         .Include("Permit")
                         .Include("Port")
                         .Include("Invoice")
                         .Include("Product")
                         .Include("NotificationDocuments")
                         on ptApp.PermitId equals nt.PermitId
                          where !nt.NotificationBankers.Any(b => b.BankId == bnk.BankId)
                         select nt).ToList();

                    if (!myNotifications.Any())
                    {
                        return new NotificationObject();
                    }

                    long appliedQuantity = 0;
                    var country = "";
                    var app = myNotifications[0];

                    var products = new List<ApplicationItemObject>();
                    
                   var ims = (from pt in db.Permits.Where(k => k.Id == app.PermitId)
                                join ptApp in db.PermitApplications on pt.Id equals ptApp.PermitId
                                join ap in db.Applications on ptApp.ApplicationId equals ap.Id
                                join it in db.ApplicationItems.Include("ApplicationCountries").Include("ThroughPuts") on ap.Id equals it.ApplicationId
                                select it).ToList();

                    if (!ims.Any())
                    {
                        return new NotificationObject();
                    }

                    ims.ForEach(it =>
                    {
                        if (app.ProductId == it.ProductId)
                        {
                            appliedQuantity = it.EstimatedQuantity;
                        }
                        var depot = "";
                        it.ThroughPuts.ToList().ForEach(t =>
                        {
                            var dp = db.Depots.Find(t.DepotId);
                            if (dp != null && dp.Id > 0)
                            {
                                if (string.IsNullOrEmpty(depot))
                                {
                                    depot = dp.Name;
                                }
                                else
                                {
                                    depot += ", " + dp.Name;
                                }
                            }
                           
                        });

                        it.ApplicationCountries.ToList().ForEach(ct =>
                        {
                            var ctr = db.Countries.Find(ct.CountryId);
                            if (ctr != null && ctr.Id > 0)
                            {
                                if (string.IsNullOrEmpty(depot))
                                {
                                    country = ctr.Name;
                                }
                                else
                                {
                                    country += ", " + ctr.Name;
                                }
                            }
                        });

                        products.Add( new ApplicationItemObject
                                        {
                                            Id = it.Id,
                                            ApplicationId = it.ApplicationId,
                                            ProductId = it.ProductId,
                                            EstimatedQuantity = it.EstimatedQuantity,
                                            EstimatedValue = it.EstimatedValue,
                                            PSFNumber = it.PSFNumber,
                                            TotalImportedQuantity = it.TotalImportedQuantity,
                                            Code = it.Product.Code,
                                            Name = it.Product.Name,
                                            Availability = it.Product.Availability,
                                            CountryOfOriginName = country,
                                            DischargeDepotName = depot
                                        });
                    });
                    
                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new NotificationObject();
                    }

                    importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                    const int formM = (int)SpecialDocsEnum.Form_M;
                    var appDocs = app.NotificationDocuments.ToList();
                    if (appDocs.Any())
                    {
                        appDocs.ForEach(o =>
                        {
                            var docs =
                                db.Documents.Where(c => c.DocumentId == o.DocumentId).Include("DocumentType").ToList();
                            if (docs.Any())
                            {
                                var doc = docs[0];
                                importObject.DocumentTypeObjects.Add(new DocumentTypeObject
                                {
                                    DocumentId = o.DocumentId,
                                    Uploaded = true,
                                    IsFormM = doc.DocumentTypeId == formM,
                                    DocumentPath = doc.DocumentPath.Replace("~", ""),
                                    DocumentTypeId = doc.DocumentType.DocumentTypeId,
                                    Name = doc.DocumentType.Name
                                });
                            }

                        });
                    }
                    
                    importObject.ApplicationItemObjects = products;
                    importObject.DischargeDepotId = app.Depot.Id;
                    importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum), importObject.Status);
                    var name = Enum.GetName(typeof(NotificationClassEnum), importObject.ClassificationId);
                    if (name != null)
                        importObject.NotificationTypeName = name.Replace("_", " ");
                    var s = Enum.GetName(typeof(CargoTypeEnum), importObject.CargoInformationTypeId);
                    if (s != null)
                        importObject.CargoTypeName = s.Replace("_", " ");
                    importObject.ProductCode = app.Product.Code;
                    importObject.CountryName = country;
                    importObject.PortName = app.Port.Name;
                    importObject.CountryId = app.Port.CountryId;
                    importObject.ApplicationQuantity = appliedQuantity;
                    importObject.DepotName = app.Depot.Name;
                    importObject.PermitValue = app.Permit.PermitValue;
                    importObject.ProductName = app.Product.Name;
                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0 ? app.Permit.PermitValue : "Not Available";
                    importObject.QuantityToDischargeStr = app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1").Replace(".0", "");
                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                    ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.ReferenceCode = app.Invoice.ReferenceCode;
                    importObject.Rrr = app.Invoice.RRR;
                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                    importObject.DocumentTypeObjects = GetNotificationDocumentTypes(importObject.DocumentTypeObjects, importObject.ClassificationId);
                    return importObject;
                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }
        
        public List<NotificationObject> GetNotifications()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myNotifications = db.Notifications.ToList();
                    if (!myNotifications.Any())
                    {
                        return new List<NotificationObject>();
                    }
                    var objList = new List<NotificationObject>();
                    myNotifications.ForEach(app =>
                    {
                        var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                        if (importObject != null && importObject.Id > 0)
                        {
                            importObject.ReferenceCode = app.Invoice.ReferenceCode;
                            objList.Add(importObject);
                        }
                    });

                    return !objList.Any() ? new List<NotificationObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<NotificationObject> GetCompletedNotifications(long id)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    const int approved = (int) NotificationStatusEnum.Approved;

                    var myNotifications = db.Notifications.Where(n => n.ImporterId == id && n.Status == approved).ToList();

                    if (!myNotifications.Any())
                    {
                        return new List<NotificationObject>();
                    }
                    var objList = new List<NotificationObject>();
                    myNotifications.ForEach(app =>
                    {
                        var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                        if (importObject != null && importObject.Id > 0)
                        {
                            importObject.ReferenceCode = app.Invoice.ReferenceCode;
                            importObject.Code = app.Product.Name + "--" + app.DischargeDate.ToString("dd/MM/yyyy") +"--Ref No"+ "(" +
                                                app.Invoice.ReferenceCode + ")";
                            objList.Add(importObject);
                        }
                    });

                    return !objList.Any() ? new List<NotificationObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<NotificationObject> GetPreviousNotification(long permitId, long productId, out double tolerancePercentage)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var initialNotifications = db.Notifications.Where(d => d.PermitId == permitId && d.ProductId == productId).ToList();
                    if (!initialNotifications.Any())
                    {
                        tolerancePercentage = 0;
                        return new List<NotificationObject>();
                    }
                    var settings = db.ImportSettings.ToList();
                    if (!settings.Any())
                    {
                        tolerancePercentage = 0;
                        return new List<NotificationObject>();
                    }
                    var objList = new List<NotificationObject>();

                    initialNotifications.ForEach(app =>
                    {
                        var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                        if (importObject != null && importObject.Id > 0)
                        {
                            objList.Add(importObject);
                        }
                    });
                    tolerancePercentage = settings[0].DischargeQuantityTolerance;
                    return !objList.Any() ? new List<NotificationObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                tolerancePercentage = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> GetEmployeeNotifications(int? itemsPerPage, int? pageNumber, out int countG, long employeeId)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int) pageNumber;
                    var tsize = (int) itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var myNotifications =
                            (from ni in db.NotificationInspectionQueues.Where(e => e.EmployeeId == employeeId)
                                join nn in db.Notifications
                                    .OrderByDescending(m => m.Id)
                                    .Skip(tpageNumber)
                                    .Take(tsize)
                                    .Include("Depot")
                                    .Include("Permit")
                                    .Include("Port").Include("Invoice")
                                    .Include("Product") on ni.Id equals nn.Id
                                select nn)
                                .ToList();
                        if (myNotifications.Any())
                        {
                            var newList = new List<NotificationObject>();
                            myNotifications.ForEach(app =>
                            {
                                var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                                if (importObject != null && importObject.Id > 0)
                                {
                                    importObject.StatusStr = Enum.GetName(typeof (NotificationStatusEnum),
                                        importObject.Status);
                                    importObject.NotificationTypeName = Enum.GetName(typeof (NotificationClassEnum),
                                        importObject.ClassificationId);
                                    importObject.CargoTypeName = Enum.GetName(typeof (CargoTypeEnum),
                                        importObject.CargoInformationTypeId);
                                    importObject.PortName = app.Port.Name;
                                    importObject.DepotName = app.Depot.Name;
                                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                        ? app.Permit.PermitValue
                                        : "Not Available";
                                    importObject.ProductName = app.Product.Code;
                                    importObject.QuantityToDischargeStr =
                                        app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                    ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                        .Replace(".0", "");
                                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                    importObject.ReferenceCode = app.Invoice.RRR;
                                    newList.Add(importObject);
                                }
                            });

                            var count =
                                (from ni in db.NotificationInspectionQueues.Where(e => e.EmployeeId == employeeId)
                                    join nn in db.Notifications on ni.Id equals nn.Id
                                    select nn).ToList();
                            countG = count.Count;
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<NotificationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> GetCompanyNotifications(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int) pageNumber;
                    var tsize = (int) itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var myNotifications = db.Notifications.Where(x => x.ImporterId == importerId)
                            .OrderByDescending(m => m.Id)
                            .Skip(tpageNumber)
                            .Take(tsize)
                            .Include("Depot")
                            .Include("Permit")
                            .Include("Port").Include("Invoice")
                            .Include("Product")
                            .ToList();
                        if (myNotifications.Any())
                        {
                            var newList = new List<NotificationObject>();
                            myNotifications.ForEach(app =>
                            {
                                var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                                if (importObject != null && importObject.Id > 0)
                                {
                                    importObject.StatusStr = Enum.GetName(typeof (NotificationStatusEnum),
                                        importObject.Status);
                                    importObject.NotificationTypeName = Enum.GetName(typeof (NotificationClassEnum),
                                        importObject.ClassificationId);
                                    importObject.CargoTypeName = Enum.GetName(typeof (CargoTypeEnum),
                                        importObject.CargoInformationTypeId);
                                    importObject.PortName = app.Port.Name;
                                    importObject.DepotName = app.Depot.Name;
                                    importObject.PermitValue = app.Permit.PermitValue;
                                    importObject.ProductName = app.Product.Code;
                                    importObject.QuantityToDischargeStr =
                                        app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                        ? app.Permit.PermitValue
                                        : "Not Available";
                                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                    ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                        .Replace(".0", "");
                                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                    importObject.ReferenceCode = app.Invoice.RRR;
                                    newList.Add(importObject);
                                }
                            });

                            countG = db.Notifications.Count(x => x.ImporterId == importerId);
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<NotificationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> GetNotifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int) pageNumber;
                    var tsize = (int) itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var myNotifications = db.Notifications
                            .OrderByDescending(m => m.Id)
                            .Skip(tpageNumber)
                            .Take(tsize)
                            .Include("Depot")
                            .Include("Permit")
                            .Include("Port")
                            .Include("Product")
                            .Include("Invoice")
                             .Include("Importer")
                            .ToList();
                        if (myNotifications.Any())
                        {
                            var newList = new List<NotificationObject>();
                            myNotifications.ForEach(app =>
                            {
                                var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                                if (importObject != null && importObject.Id > 0)
                                {
                                    importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum),
                                        importObject.Status);
                                    importObject.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum),
                                        importObject.ClassificationId);
                                    importObject.CargoTypeName = Enum.GetName(typeof(CargoTypeEnum),
                                        importObject.CargoInformationTypeId);
                                    importObject.PortName = app.Port.Name;
                                    importObject.DepotName = app.Depot.Name;
                                    importObject.ImporterName = app.Importer.Name;
                                    importObject.PermitValue = app.Permit.PermitValue;
                                    importObject.ProductName = app.Product.Code;
                                    importObject.QuantityToDischargeStr =
                                        app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                        .Replace(".0", "");
                                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                        ? app.Permit.PermitValue
                                        : "Not Available";
                                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                    ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                    importObject.ReferenceCode = app.Invoice.RRR;
                                    newList.Add(importObject);
                                }
                            });

                            countG = db.Notifications.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<NotificationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myNotifications =
                        (from nf in db.Notifications.Where(d => d.Invoice.ReferenceCode.Contains(searchCriteria) || d.Invoice.RRR.Trim().Contains(searchCriteria)).Include("Invoice")
                         join jt in db.Depots.Where(d => d.Name.ToLower().Trim().Contains(searchCriteria.ToLower().Trim())) on nf.DischargeDepotId equals jt.Id
                            join pt in db.Permits on nf.PermitId equals pt.Id
                            join prt in db.Ports on nf.PortOfOriginId equals prt.Id
                            join pd in db.Products.Where(c => c.Name.ToLower().Trim().Contains(searchCriteria))
                                on nf.ProductId equals pd.ProductId
                            select new NotificationObject
                            {
                                Id = nf.Id,
                                ReferenceCode = nf.Invoice.RRR,
                                ClassificationId = nf.ClassificationId,
                                PermitId = nf.PermitId,
                                PortOfOriginId = nf.PortOfOriginId,
                                DischargeDepotId = nf.DischargeDepotId,
                                PermitValue = pt != null && pt.Id > 0 ? pt.PermitValue : "Not Available",
                                ProductId = nf.ProductId,
                                QuantityToDischarge = nf.QuantityToDischarge,
                                CargoInformationTypeId = nf.CargoInformationTypeId,
                                ArrivalDate = nf.ArrivalDate,
                                DischargeDate = nf.DischargeDate,
                                AmountDue = nf.Invoice.TotalAmountDue,
                                DateCreated = nf.DateCreated,
                                PortName = prt.Name,
                                DepotName = jt.Name,
                                ProductName = pd.Code
                            }).ToList();

                    if (!myNotifications.Any())
                    {
                        return new List<NotificationObject>();
                    }
                    myNotifications.ForEach(v =>
                    {
                        v.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum), v.ClassificationId);
                        v.QuantityToDischargeStr = v.QuantityToDischarge.ToString("n1").Replace(".0", "");
                        v.QuantityOnVesselStr = v.QuantityOnVessel.ToString("n1").Replace(".0", "");
                        v.AmountDueStr = v.AmountDue.ToString("N");
                        v.CargoTypeName = Enum.GetName(typeof (CargoTypeEnum), v.CargoInformationTypeId);
                        v.ArrivalDateStr = v.ArrivalDate.ToString("dd/MM/yyyy") + " - " + v.DischargeDate.ToString("dd/MM/yyyy");
                        //v.DischargeDateStr = v.DischargeDate.ToString("dd/MM/yyyy");
                        v.DateCreatedStr = v.DateCreated.ToString("dd/MM/yyyy");
                        v.StatusStr = Enum.GetName(typeof (NotificationStatusEnum), v.Status);
                    });

                    return myNotifications;
                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }

        public ImportSettingObject GetImportSettings()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var appsettings = db.ImportSettings.ToList();

                    if (!appsettings.Any())
                    {
                        return new ImportSettingObject();
                    }
                    var setting = appsettings[0];
                    return new ImportSettingObject
                    {
                        Id = setting.Id,
                        ApplicationExpiry = setting.ApplicationExpiry,
                        ApplicationLifeCycle = setting.ApplicationLifeCycle,
                        PriceVolumeThreshold = setting.PriceVolumeThreshold,
                        VesselArrivalLeadTime = setting.VesselArrivalLeadTime,
                        VesselDischargeLeadTime = setting.VesselDischargeLeadTime,
                        DischargeQuantityTolerance = setting.DischargeQuantityTolerance,
                        PermitExpiryTolerance = setting.PermitExpiryTolerance,
                        PermitValidity = setting.PermitValidity,
                        MessageLifeSpan = setting.MessageLifeSpan,
                    };
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ImportSettingObject();
            }
        }

        public ApplicationObject GetApplicationByPermitNumber(string permitValue, long importerId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var appsettings = db.ImportSettings.ToList();

                    if (!appsettings.Any())
                    {
                        return new ApplicationObject();
                    }

                    var myNotifications = (from pt in db.Permits.Where(d => d.PermitValue == permitValue)
                                           join ptApp in db.PermitApplications on pt.Id equals ptApp.PermitId
                                           join imp in db.Applications.Include("ApplicationItems") on ptApp.ApplicationId equals imp.Id
                                           join ic in db.ImportClasses on imp.ClassificationId equals ic.Id
                            where imp.ImporterId == importerId
                            select new {imp, pt, ic}).ToList();

                    if (!myNotifications.Any())
                    {
                        return new ApplicationObject();
                    }

                    var app = myNotifications[0].imp;
                    var prt = myNotifications[0].pt;
                    var cl = myNotifications[0].ic;
                    var importObject = ModelMapper.Map<Application, ApplicationObject>(app); 
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new ApplicationObject();
                    }

                    importObject.PermitId = prt.Id;
                    importObject.ImportClassName = cl.Name;
                    importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                    if (!app.ApplicationItems.Any())
                    {
                        return new ApplicationObject();
                    }

                    var appsetting = appsettings[0];

                    const int psf = (int) CustomColEnum.Psf;
                    var items = app.ApplicationItems.ToList();
                    foreach(var u in items)
                    {
                        var im = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(u);
                        if (im != null && im.ApplicationId > 0)
                        {
                            im.ProductObject = new ProductObject();
                            var products = db.Products.Where(x => x.ProductId == im.ProductId).Include("ProductColumns").ToList();
                            if (!products.Any())
                            {
                                return new ApplicationObject();
                            }
                            
                            var product = products[0];
                           
                            im.ProductObject = new ProductObject
                            {
                                ProductId = product.ProductId,
                                Code = product.Code,
                                RequiresPsf = product.ProductColumns.Any() && product.ProductColumns.Any(v => v.CustomCodeId == psf),
                                Availability = product.Availability,
                                Name = product.Name
                            };

                            im.ApplicationCountryObjects = new List<ApplicationCountryObject>();
                            im.ThroughPutObjects = new List<ThroughPutObject>();

                            var appCountries = db.ApplicationCountries.Where(a => a.ApplicationItemId == im.Id).Include("Country").ToList();
                            var depotList = db.ThroughPuts.Where(a => a.ApplicationItemId == im.Id).Include("Depot").ToList();

                            if (appCountries.Any() && depotList.Any())
                            {
                                appCountries.ForEach(c =>
                                {
                                    im.ApplicationCountryObjects.Add(new ApplicationCountryObject
                                    {
                                        Id = c.Id,
                                        CountryId = c.CountryId,
                                        Name = c.Country.Name,
                                        ApplicationItemId = c.ApplicationItemId
                                    });
                                });

                                depotList.ForEach(d =>
                                {
                                    im.ThroughPutObjects.Add(new ThroughPutObject
                                    {
                                        Id = d.Id,
                                        DepotId = d.DepotId,
                                        DepotName = d.Depot.Name,
                                        ProductId = d.ProductId,
                                        Quantity = d.Quantity,
                                        DocumentId = d.DocumentId,
                                        IPAddress = d.IPAddress,
                                        Comment = d.Comment,
                                        ApplicationItemId = d.ApplicationItemId
                                    });
                                });
                            }
                            
                            im.ProductObject.Code = u.TotalImportedQuantity < 1 ? im.ProductObject.Code + " - (Balance Volume(MT) : " + u.EstimatedQuantity.ToString("N") + ")" : im.ProductObject.Code + " - (Balance Volume(MT) : " + GetVolumeBalance(appsetting.DischargeQuantityTolerance, u.EstimatedQuantity, u.TotalImportedQuantity) + ")";
                            if (string.IsNullOrEmpty(im.PSFNumber))
                            {
                                im.PSFNumber = "Not Applicable";
                            }

                            importObject.ApplicationItemObjects.Add(im);
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

        public NotificationObject GetNotificationByRef(string code, string rrr)
        {
            try 
            {
                using (var db = new ImportPermitEntities())
                {
                    var myApplications =
                        db.Notifications.Where(m => m.Invoice.ReferenceCode == code.Trim() || m.Invoice.RRR == code.Trim())
                            .Include("Importer")
                            .Include("Invoice")
                            .Include("Permit")
                            .Include("Port")
                            .Include("Product")
                            .Include("Depot")
                            .ToList();

                    if (!myApplications.Any())
                    {
                        return new NotificationObject();
                    }

                    var app = myApplications[0];
                    if (!string.IsNullOrEmpty(rrr))
                    {
                        long orderId;
                        var pRes = long.TryParse(app.Invoice.ReferenceCode, out orderId);
                        if (!pRes || orderId < 1)
                        {
                            return new NotificationObject();
                        }

                        var invStatus = UpdateInvoceTransactionInvoce(orderId, rrr);
                        if (invStatus < 1)
                        {
                            return new NotificationObject();
                        }
                        app.Invoice.RRR = rrr;
                        db.Entry(app.Invoice).State = EntityState.Modified;
                        db.SaveChanges(); 
                    }

                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new NotificationObject();
                    }

                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                    ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.ImporterName = app.Importer.Name;
                    importObject.PermitValue = app.Permit.PermitValue;
                    importObject.ProductCode = app.Product.Code;
                    importObject.NotificationClassName = app.ImportClass.Name;

                    importObject.DepotName = app.Depot.Name;
                    var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                    if (name != null) importObject.StatusStr = name.Replace("_", " ");

                    importObject.PaymentTypeId = app.Invoice.PaymentTypeId;
                    var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                    if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");

                    var cargo = Enum.GetName(typeof(CargoTypeEnum), app.CargoInformationTypeId);
                    if (cargo != null) importObject.CargoTypeName = cargo.Replace("_", " ");

                    importObject.ReferenceCode = app.Invoice.ReferenceCode;

                     var notificationProps =  new NotificationRequirementProp
                    {
                        NotificationClassId = app.ClassificationId,
                        CargoInformationTypeId = app.CargoInformationTypeId,
                        ArrivalDate = app.ArrivalDate,
                        DischargeDate = app.DischargeDate
                    };

                     importObject.FeeObjects = ComputeAmountDue(notificationProps);
                    return importObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public long UpdateInvoceTransactionInvoce(long orderId, string rrr)
        {
            try
            {
                using (var db = new PPIPSPaymentEntities())
                {
                    var entities = db.TransactionInvoices.Where(n => n.Id == orderId).ToList();
                    if (!entities.Any())
                    {
                        return -2;
                    }
                    var entity = entities[0];
                    entity.RRR = rrr;
                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();
                    return entity.Id;
                }

            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public long UpdateInvoceTransactionInvoce(long orderId)
        {
            try
            {
                using (var db = new PPIPSPaymentEntities())
                {
                    var entities = db.TransactionInvoices.Where(n => n.Id == orderId).ToList();
                    if (!entities.Any())
                    {
                        return -2;
                    }
                    var entity = entities[0];
                    entity.AmountPaid = entity.TotalAmountDue;
                    entity.Status = (int)PaymentStatusEnum.Paid;
                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();
                    return entity.Id;
                }

            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public NotificationObject GetNotificationByRef(string code)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myApplications =
                        db.Notifications.Where(m => m.Invoice.ReferenceCode == code.Trim() || m.Invoice.RRR == code.Trim())
                            .Include("Importer")
                            .Include("Invoice")
                            .Include("Permit")
                            .Include("Port")
                            .Include("Product")
                            .Include("Depot")
                            .ToList();

                    if (!myApplications.Any())
                    {
                        return new NotificationObject();
                    }

                    var app = myApplications[0];
                   
                    long orderId;
                    var pRes = long.TryParse(app.Invoice.ReferenceCode, out orderId);
                    if (!pRes || orderId < 1)
                    {
                        return new NotificationObject();
                    }

                    var invStatus = UpdateInvoceTransactionInvoce(orderId);
                    if (invStatus < 1)
                    {
                        return new NotificationObject();
                    }
                    app.Invoice.AmountPaid = app.Invoice.TotalAmountDue;
                    app.Invoice.Status = (int)PaymentStatusEnum.Paid;
                    db.Entry(app.Invoice).State = EntityState.Modified;
                    db.SaveChanges();
                    

                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new NotificationObject();
                    }

                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                    ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.ImporterName = app.Importer.Name;
                    importObject.PermitValue = app.Permit.PermitValue;
                    importObject.ProductCode = app.Product.Code;
                    importObject.NotificationClassName = app.ImportClass.Name;
                    importObject.AmountPaid = app.Invoice.TotalAmountDue;

                    importObject.DepotName = app.Depot.Name;
                    var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                    if (name != null) importObject.StatusStr = name.Replace("_", " ");

                    importObject.PaymentTypeId = app.Invoice.PaymentTypeId;
                    var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                    if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");

                    var cargo = Enum.GetName(typeof(CargoTypeEnum), app.CargoInformationTypeId);
                    if (cargo != null) importObject.CargoTypeName = cargo.Replace("_", " ");

                    importObject.ReferenceCode = app.Invoice.ReferenceCode;
                    importObject.StatusStr = Enum.GetName(typeof(PaymentStatusEnum), app.Invoice.PaymentTypeId);

                    var notificationProps = new NotificationRequirementProp
                    {
                        NotificationClassId = app.ClassificationId,
                        CargoInformationTypeId = app.CargoInformationTypeId,
                        ArrivalDate = app.ArrivalDate,
                        DischargeDate = app.DischargeDate
                    };

                    importObject.FeeObjects = ComputeAmountDue(notificationProps);
                    return importObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }
        
        public List<FeeObject> ComputeAmountDue(NotificationRequirementProp notificationProps)
        {
            var gVal = new GenericValidator();
            try
            {
                CalculationFactor calculationFactor;
                var appFees = GetNotificationFees(out calculationFactor);
                if (!appFees.Any() || calculationFactor == null || calculationFactor.ImportSettingObject == null || calculationFactor.ImportSettingObject.Id < 1)
                {
                    return new List<FeeObject>();
                }

                var daysLapse = notificationProps.DischargeDate - DateTime.Today;
                if (daysLapse.Days < calculationFactor.ImportSettingObject.VesselDischargeLeadTime)
                {
                    calculationFactor.ExpenditionaryFeeApplicable = true;
                }
               
                if (!calculationFactor.ExpenditionaryFeeApplicable)
                {
                    const int expenditionaryFee = (int)FeeTypeEnum.Expeditionary;
                    appFees.Remove(appFees.Find(o => o.FeeTypeId == expenditionaryFee));
                }

                return appFees;

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Requesting processing failed. Please try again later.";
                return new List<FeeObject>(); ;
            }
        }

        public List<FeeObject> GetNotificationFees(out CalculationFactor calculationFactor)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    const int appStage = (int)AppStage.Notification;
                    var fees = db.Fees.Where(k => k.ImportStageId == appStage).Include("FeeType").ToList().ToList();
                    if (!fees.Any())
                    {
                        calculationFactor = new CalculationFactor();
                        return new List<FeeObject>();
                    }
                    var appSettings = db.ImportSettings.ToList();
                    if (!appSettings.Any())
                    {
                        calculationFactor = new CalculationFactor();
                        return new List<FeeObject>();
                    }

                    var feeSum = 0.0;
                    const int expend = (int)FeeTypeEnum.Expeditionary;
                    var expenditionaryFee = 0.0;
                    fees.ForEach(m =>
                    {
                        if (m.FeeTypeId != expend)
                        {
                            feeSum += m.Amount;
                        }
                        else
                        {
                            expenditionaryFee = m.Amount;
                        }
                    });

                    var appSettingObject = ModelMapper.Map<ImportSetting, ImportSettingObject>(appSettings[0]);
                    if (appSettingObject != null && appSettingObject.Id < 1)
                    {
                        calculationFactor = new CalculationFactor();
                        return new List<FeeObject>();
                    }

                    calculationFactor = new CalculationFactor
                    {
                        Fees = feeSum,
                        ExpenditionaryFee = expenditionaryFee,
                        ImportSettingObject = appSettingObject
                    };

                    var objList = new List<FeeObject>();
                    fees.ForEach(app =>
                    {
                        var feeObject = ModelMapper.Map<Fee, FeeObject>(app);
                        if (feeObject != null && feeObject.FeeId > 0)
                        {
                            feeObject.FeeTypeName = app.FeeType.Name;
                            feeObject.ImportStageName = app.ImportStage.Name;
                            objList.Add(feeObject);
                        }
                    });

                    return !objList.Any() ? new List<FeeObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                calculationFactor = new CalculationFactor();
                return new List<FeeObject>();
            }
        }

        public List<NotificationObject> SearchEmployeeNotifications(string searchCriteria, long emplyeeId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myNotifications =
                        (from ni in db.NotificationInspectionQueues.Where(e => e.EmployeeId == emplyeeId)
                         join nf in db.Notifications.Where(d => d.Invoice.ReferenceCode.Contains(searchCriteria) || d.Invoice.RRR.Contains(searchCriteria)).Include("Invoice") on
                                ni.Id equals nf.Id
                            join jt in
                                db.Depots.Where(d => d.Name.ToLower().Trim().Contains(searchCriteria.ToLower().Trim()))
                                on nf.DischargeDepotId equals jt.Id
                            join pt in db.Permits on nf.PermitId equals pt.Id
                            join prt in db.Ports on nf.PortOfOriginId equals prt.Id
                            join pd in db.Products.Where(c => c.Name.ToLower().Trim().Contains(searchCriteria))
                                on nf.ProductId equals pd.ProductId
                            select new NotificationObject
                            {
                                Id = nf.Id,
                                ReferenceCode = nf.Invoice.ReferenceCode,
                                ClassificationId = nf.ClassificationId,
                                PermitId = nf.PermitId,
                                PortOfOriginId = nf.PortOfOriginId,
                                DischargeDepotId = nf.DischargeDepotId,
                                ProductId = nf.ProductId,
                                QuantityToDischarge = nf.QuantityToDischarge,
                                CargoInformationTypeId = nf.CargoInformationTypeId,
                                ArrivalDate = nf.ArrivalDate,
                                DischargeDate = nf.DischargeDate,
                                AmountDue = nf.Invoice.TotalAmountDue,
                                DateCreated = nf.DateCreated,
                                PortName = prt.Name,
                                DepotName = jt.Name,
                                PermitValue = pt != null && pt.Id > 0 ? pt.PermitValue : "Not Available",
                                ProductName = pd.Code
                            }).ToList();
                    //DateAssigned  InspectionDate OutComeCodeTypeId
                    if (!myNotifications.Any())
                    {
                        return new List<NotificationObject>();
                    }
                    myNotifications.ForEach(v =>
                    {
                        v.AmountDueStr = v.AmountDue.ToString("N");
                        v.QuantityToDischargeStr = v.QuantityToDischarge.ToString("n1").Replace(".0", "");
                        v.QuantityOnVesselStr = v.QuantityOnVessel.ToString("n1").Replace(".0", "");
                        v.NotificationTypeName = Enum.GetName(typeof (NotificationClassEnum), v.ClassificationId);
                        v.CargoTypeName = Enum.GetName(typeof (CargoTypeEnum), v.CargoInformationTypeId);
                        v.ArrivalDateStr = v.ArrivalDate.ToString("dd/MM/yyyy") + " - " + v.DischargeDate.ToString("dd/MM/yyyy");
                        //v.DischargeDateStr = v.DischargeDate.ToString("dd/MM/yyyy");
                        v.DateCreatedStr = v.DateCreated.ToString("dd/MM/yyyy");
                    });

                    return myNotifications;
                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> SearchCompanyNotifications(string searchCriteria, long importerId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myNotifications =
                        (from nf in
                            db.Notifications.Where(
                                d =>
                                    d.ImporterId == importerId &&
                                    (d.Invoice.ReferenceCode.Contains(searchCriteria) || d.Invoice.RRR.Contains(searchCriteria) ||
                                     !d.Invoice.ReferenceCode.Contains(searchCriteria)) || !d.Invoice.RRR.Contains(searchCriteria)).Include("Invoice")
                            join jt in db.Depots.Where(d => d.Name.ToLower().Trim().Contains(searchCriteria.ToLower().Trim())) on nf.DischargeDepotId equals jt.Id
                            join pt in db.Permits on nf.PermitId equals pt.Id
                            join prt in db.Ports on nf.PortOfOriginId equals prt.Id
                            join pd in db.Products.Where(c => c.Name.ToLower().Trim().Contains(searchCriteria)) on nf.ProductId equals pd.ProductId
                            select new NotificationObject
                            {
                                Id = nf.Id,
                                ReferenceCode = nf.Invoice.ReferenceCode,
                                ClassificationId = nf.ClassificationId,
                                PermitId = nf.PermitId,
                                PortOfOriginId = nf.PortOfOriginId,
                                DischargeDepotId = nf.DischargeDepotId,
                                ProductId = nf.ProductId,
                                QuantityToDischarge = nf.QuantityToDischarge,
                                CargoInformationTypeId = nf.CargoInformationTypeId,
                                ArrivalDate = nf.ArrivalDate,
                                DischargeDate = nf.DischargeDate,
                                AmountDue = nf.Invoice.TotalAmountDue,
                                DateCreated = nf.DateCreated,
                                PortName = prt.Name,
                                DepotName = jt.Name,
                                PermitValue = pt != null && pt.Id > 0 ? pt.PermitValue : "Not Available",
                                ProductName = pd.Code
                            }).ToList();

                    if (!myNotifications.Any())
                    {
                        return new List<NotificationObject>();
                    }
                    myNotifications.ForEach(v =>
                    {
                        v.AmountDueStr = v.AmountDue.ToString("N");
                        v.StatusStr = Enum.GetName(typeof (NotificationStatusEnum), v.Status);
                        v.QuantityToDischargeStr = v.QuantityToDischarge.ToString("n1").Replace(".0", "");
                        v.QuantityOnVesselStr = v.QuantityOnVessel.ToString("n1").Replace(".0", "");
                        v.NotificationTypeName = Enum.GetName(typeof (NotificationClassEnum), v.ClassificationId);
                        v.CargoTypeName = Enum.GetName(typeof (CargoTypeEnum), v.CargoInformationTypeId);
                        v.ArrivalDateStr = v.ArrivalDate.ToString("dd/MM/yyyy") + " - " + v.DischargeDate.ToString("dd/MM/yyyy");
                        //v.DischargeDateStr = v.DischargeDate.ToString("dd/MM/yyyy");
                        v.DateCreatedStr = v.DateCreated.ToString("dd/MM/yyyy");
                    });

                    return myNotifications;
                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }
        
        public long DeleteNotification(long applicationId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myNotifications =
                        db.Notifications.Where(m => m.Id == applicationId).ToList();
                    if (!myNotifications.Any())
                    {
                        return 0;
                    }

                    var app = myNotifications[0];
                    db.Notifications.Remove(app);
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

        public string GetVolumeBalance(long dischargeQuantityTolerance, long estimatedQuantity, double? quantityImported)
        {
            try
            {
                const float percentageLimit = 100;
                if (quantityImported != null)
                {
                    var balaceVolume = ((dischargeQuantityTolerance*estimatedQuantity)/percentageLimit) -
                                       (double) quantityImported;
                    return balaceVolume.ToString("N");
                }
                return "";
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return "";
            }
        }

        public List<NotificationObject> GetBankAssignedNotifications(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int) pageNumber;
                    var tsize = (int) itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                         const int paid = (int)NotificationStatusEnum.Paid;
                        var myNotifications = (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                                               join pb in db.ProductBankers on bnk.BankId equals pb.BankId
                                               join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                                               join app in db.Applications on ai.ApplicationId equals app.Id
                                               join ptApp in db.PermitApplications on app.Id equals ptApp.ApplicationId
                                               join nt in db.Notifications.Where(x => x.Status == paid).OrderByDescending(m => m.Id)
                                                .Skip(tpageNumber)
                                                .Take(tsize)
                                                .Include("Depot")
                                                .Include("Permit")
                                                .Include("Importer")
                                                .Include("Port")
                                                .Include("Invoice")
                                                .Include("Product")
                                                .Include("NotificationDocuments")
                                                on ptApp.PermitId equals nt.PermitId
                                               where (!nt.NotificationBankers.Any(b => b.BankId == bnk.BankId) || !nt.FormMDetails.Any(b => b.BankId == bnk.BankId)) && nt.ProductId == ai.ProductId
                                               select nt).ToList();

                        if (myNotifications.Any())
                        {
                            var newList = new List<NotificationObject>();
                            myNotifications.ForEach(app =>
                            {
                                if (!newList.Exists(b => b.Id == app.Id))
                                {
                                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                                    if (importObject != null && importObject.Id > 0)
                                    {
                                        importObject.StatusStr = Enum.GetName(typeof (NotificationStatusEnum),
                                            importObject.Status);
                                        importObject.ImporterName = app.Importer.Name;
                                        importObject.NotificationTypeName = Enum.GetName(typeof (NotificationClassEnum),
                                            importObject.ClassificationId);
                                        importObject.CargoTypeName = Enum.GetName(typeof (CargoTypeEnum),
                                            importObject.CargoInformationTypeId);
                                        importObject.PortName = app.Port.Name;
                                        importObject.DepotName = app.Depot.Name;
                                        importObject.PermitValue = app.Permit.PermitValue;
                                        importObject.ProductName = app.Product.Code;
                                        importObject.QuantityToDischargeStr =
                                            app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                        importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                        importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                            ? app.Permit.PermitValue
                                            : "Not Available";
                                        importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                        ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                        importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                            .Replace(".0", "");
                                        importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                        importObject.ReferenceCode = app.Invoice.RRR;
                                        newList.Add(importObject);

                                    }
                                }
                            });

                            countG = (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                                      join pb in db.ProductBankers.Where(x => x.DocumentId == null) on bnk.BankId equals pb.BankId
                                      join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                                      join app in db.Applications on ai.ApplicationId equals app.Id
                                      join ptApp in db.PermitApplications on app.Id equals ptApp.ApplicationId
                                      join nt in db.Notifications.Where(x => x.Status == paid) on ptApp.PermitId equals nt.PermitId where (!nt.NotificationBankers.Any(b => b.BankId == bnk.BankId) || !nt.FormMDetails.Any()) && nt.ProductId == ai.ProductId
                                      select nt).ToList().Count;

                            return newList;
                        }

                        countG = 0;
                        return new List<NotificationObject>();
                    }

                }
                countG = 0;
                return new List<NotificationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> GetBankNotificationHistory(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    const int paid = (int)NotificationStatusEnum.Paid;
                    var tpageNumber = (int) pageNumber;
                    var tsize = (int) itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var myNotifications = (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                                               join pb in db.ProductBankers on bnk.BankId equals pb.BankId
                                               join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                                               join app in db.Applications on ai.ApplicationId equals app.Id
                                               join ptApp in db.PermitApplications on app.Id equals ptApp.ApplicationId
                                               join nt in db.Notifications.Where(n => n.Invoice.Status == paid && n.NotificationDocuments.Any()).OrderByDescending(m => m.Id)

                                .Skip(tpageNumber)
                                .Take(tsize)
                                .Include("Depot")
                                .Include("Permit")
                                .Include("Port")
                                .Include("Invoice")
                                .Include("Product")
                                on ptApp.PermitId equals nt.PermitId
                                where nt.NotificationBankers.Any(k => k.BankId == bnk.BankId) && nt.FormMDetails.Any(b => b.BankId == bnk.BankId)
                                               select nt).ToList();

                        if (myNotifications.Any())
                        {
                            var newList = new List<NotificationObject>();
                            myNotifications.ForEach(app =>
                            {
                                if (!newList.Exists(b => b.Id == app.Id))
                                {
                                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                                    if (importObject != null && importObject.Id > 0)
                                    {
                                        importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum),
                                            importObject.Status);
                                        importObject.ImporterName = app.Importer.Name;
                                        importObject.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum),
                                            importObject.ClassificationId);
                                        importObject.CargoTypeName = Enum.GetName(typeof(CargoTypeEnum),
                                            importObject.CargoInformationTypeId);
                                        importObject.PortName = app.Port.Name;
                                        importObject.DepotName = app.Depot.Name;
                                        importObject.PermitValue = app.Permit.PermitValue;
                                        importObject.ProductName = app.Product.Code;
                                        importObject.QuantityToDischargeStr =
                                            app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                        importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                        importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                            ? app.Permit.PermitValue
                                            : "Not Available";
                                        importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                        ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                        importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                            .Replace(".0", "");
                                        importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                        importObject.ReferenceCode = app.Invoice.RRR;
                                        newList.Add(importObject);

                                    }
                                }
                            });
                            countG = (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                                      join pb in db.ProductBankers on bnk.BankId equals pb.BankId
                                      join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                                      join app in db.Applications on ai.ApplicationId equals app.Id
                                      join ptApp in db.PermitApplications on app.Id equals ptApp.ApplicationId
                                      join nt in db.Notifications.Where(n => n.Invoice.Status == paid && n.NotificationDocuments.Any()) on ptApp.PermitId equals nt.PermitId
                                      where nt.NotificationBankers.Any(b => b.BankId == bnk.BankId)
                                select nt).ToList().Count;
                            return newList;
                        }

                        countG = 0;
                        return new List<NotificationObject>();
                    }

                }
                countG = 0;
                return new List<NotificationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> GetBankUserNotificationHistory(int? itemsPerPage, int? pageNumber, out int countG, long userId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                    {
                        var tpageNumber = (int) pageNumber;
                        var tsize = (int) itemsPerPage;

                        var myNotifications =
                            (from dc in db.Documents.Where(d => d.UploadedById == userId)
                                join ntDoc in db.NotificationDocuments on dc.DocumentId equals ntDoc.DocumentId
                                join nt in db.Notifications.OrderByDescending(m => m.Id)
                                    .Skip(tpageNumber).Take(tsize)
                                    .Take(tsize)
                                    .Include("Invoice")
                                    .Include("Depot")
                                    .Include("Permit")
                                    .Include("Port")
                                    .Include("Invoice")
                                    .Include("Product")
                                    on ntDoc.NotificationId equals nt.Id
                                select nt).ToList();

                        if (myNotifications.Any())
                        {
                            var newList = new List<NotificationObject>();
                            myNotifications.ForEach(app =>
                            {
                                if (!newList.Exists(b => b.Id == app.Id))
                                {
                                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                                    if (importObject != null && importObject.Id > 0)
                                    {
                                        importObject.StatusStr = Enum.GetName(typeof (NotificationStatusEnum),
                                            importObject.Status);
                                        importObject.ImporterName = app.Importer.Name;
                                        importObject.NotificationTypeName = Enum.GetName(typeof (NotificationClassEnum),
                                            importObject.ClassificationId);
                                        importObject.CargoTypeName = Enum.GetName(typeof (CargoTypeEnum),
                                            importObject.CargoInformationTypeId);
                                        importObject.PortName = app.Port.Name;
                                        importObject.DepotName = app.Depot.Name;
                                        importObject.PermitValue = app.Permit.PermitValue;
                                        importObject.ProductName = app.Product.Code;
                                        importObject.QuantityToDischargeStr =
                                            app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                        importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                        importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                            ? app.Permit.PermitValue
                                            : "Not Available";
                                        importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                        ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                        importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                            .Replace(".0", "");
                                        importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                        importObject.ReferenceCode = app.Invoice.RRR;
                                        newList.Add(importObject);

                                    }
                                }
                            });
                            countG = (from dc in db.Documents.Where(d => d.UploadedById == userId)
                                join ntDoc in db.NotificationDocuments on dc.DocumentId equals ntDoc.DocumentId
                                join nt in db.Notifications
                                    on ntDoc.Id equals nt.Id
                                select nt).ToList().Count;
                            return newList;
                        }
                        countG = 0;
                        return new List<NotificationObject>();
                    }
                    countG = 0;
                    return new List<NotificationObject>();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> SearchBankAssignedNotifications(string searchCriteria, long importerId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    const int paid = (int)NotificationStatusEnum.Paid;
                    var myNotifications =
                        (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                         join pb in db.ProductBankers on bnk.BankId equals pb.BankId
                         join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                         join app in db.Applications on ai.ApplicationId equals app.Id
                         join ptApp in db.PermitApplications on app.Id equals ptApp.ApplicationId
                         join nt in db.Notifications.Where(j => j.Invoice.Status == paid && (j.Invoice.ReferenceCode.Trim().Contains(searchCriteria.Trim()) || j.Invoice.RRR.Contains(searchCriteria) || j.Importer.Name.Contains(searchCriteria) || j.Product.Name.Trim().Contains(searchCriteria.Trim()))).Include("Invoice")
                         .Include("Depot").Include("Port").Include("Product").Include("Importer").Include("NotificationDocuments") on ptApp.PermitId equals nt.PermitId
                         where !nt.NotificationBankers.Any(b => b.BankId == bnk.BankId) && nt.FormMDetails.Any(b => b.BankId == bnk.BankId)
                            select new NotificationObject
                            {
                                Id = nt.Id,
                                ReferenceCode = nt.Invoice.RRR,
                                ClassificationId = nt.ClassificationId,
                                PermitId = nt.PermitId,
                                ImporterName = nt.Importer.Name,
                                PortOfOriginId = nt.PortOfOriginId,
                                DischargeDepotId = nt.DischargeDepotId,
                                ProductId = nt.Product.ProductId,
                                QuantityToDischarge = nt.QuantityToDischarge,
                                QuantityOnVessel = nt.QuantityOnVessel,
                                CargoInformationTypeId = nt.CargoInformationTypeId,
                                ArrivalDate = nt.ArrivalDate,
                                ProductCode = nt.Product.Code,
                                DischargeDate = nt.DischargeDate,
                                AmountDue = nt.Invoice.TotalAmountDue,
                                DateCreated = nt.DateCreated,
                                ImporterId = nt.ImporterId,
                                Status = nt.Status,
                                PortName = nt.Port.Name,
                                DepotName = nt.Depot.Name,
                                PermitValue = nt.Permit != null && nt.Permit.Id > 0 ? nt.Permit.PermitValue : "Not Available"

                            }).ToList();

                    if (!myNotifications.Any())
                    {
                        return new List<NotificationObject>();
                    }

                    var newList =  new List<NotificationObject>();

                    myNotifications.ForEach(app =>
                    {
                        if (!newList.Exists(b => b.Id == app.Id))
                        {
                            app.StatusStr = Enum.GetName(typeof (NotificationStatusEnum), app.Status);
                            app.NotificationTypeName = Enum.GetName(typeof (NotificationClassEnum), app.ClassificationId);
                            app.CargoTypeName = Enum.GetName(typeof (CargoTypeEnum), app.CargoInformationTypeId);
                            app.QuantityToDischargeStr = app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                            app.AmountDueStr = app.AmountDue.ToString("N");
                            app.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                            //app.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                            app.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1").Replace(".0", "");
                            app.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                            newList.Add(app);
                        }
                    });
                    return myNotifications;
                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> SearchBankNotificationHistory(string searchCriteria, long importerId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myNotifications =
                        (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                            join pb in db.ProductBankers on bnk.BankId equals pb.BankId
                            join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                            join app in db.Applications on ai.Id equals app.Id
                            join ptApp in db.PermitApplications on app.Id equals ptApp.ApplicationId
                            join nt in
                                db.Notifications.Where(
                                    j =>
                                        j.NotificationDocuments.Any() &&
                                        (j.Invoice.ReferenceCode.Trim().Contains(searchCriteria.Trim()) ||
                                         j.Invoice.RRR.Contains(searchCriteria) ||
                                         j.Importer.Name.Contains(searchCriteria) ||
                                         j.Product.Name.Trim().Contains(searchCriteria.Trim()))).Include("Invoice")
                                    .Include("Depot")
                                    .Include("Port")
                                    .Include("Product")
                                    .Include("NotificationDocuments") on ptApp.PermitId equals nt.PermitId
                          where nt.NotificationBankers.Any(b => b.BankId == bnk.BankId) && nt.FormMDetails.Any(b => b.BankId == bnk.BankId)
                            select nt).ToList();

                    if (myNotifications.Any())
                    {
                        var newList = new List<NotificationObject>();
                        myNotifications.ForEach(app =>
                        {
                            if (!newList.Exists(b => b.Id == app.Id))
                            {
                                var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                                if (importObject != null && importObject.Id > 0)
                                {
                                    importObject.StatusStr = Enum.GetName(typeof (NotificationStatusEnum),
                                        importObject.Status);
                                    importObject.ImporterName = app.Importer.Name;
                                    importObject.NotificationTypeName = Enum.GetName(typeof (NotificationClassEnum),
                                        importObject.ClassificationId);
                                    importObject.CargoTypeName = Enum.GetName(typeof (CargoTypeEnum),
                                        importObject.CargoInformationTypeId);
                                    importObject.PortName = app.Port.Name;
                                    importObject.DepotName = app.Depot.Name;
                                    importObject.PermitValue = app.Permit.PermitValue;
                                    importObject.ProductName = app.Product.Code;
                                    importObject.QuantityToDischargeStr =
                                        app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                        ? app.Permit.PermitValue
                                        : "Not Available";
                                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                    ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                        .Replace(".0", "");
                                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                    importObject.ReferenceCode = app.Invoice.RRR;
                                    newList.Add(importObject);

                                }
                            }
                        });
                        return newList;
                    }
                    return new List<NotificationObject>();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> SearchBankUserNotificationHistory(string searchCriteria, long userId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myNotifications =
                            (from dc in db.Documents.Where(d => d.UploadedById == userId)
                            join ntDoc in db.NotificationDocuments on dc.DocumentId equals ntDoc.DocumentId
                            join nt in db.Notifications.Where(j => j.Invoice.ReferenceCode.Trim().Contains(searchCriteria.Trim()) || j.Invoice.RRR.Contains(searchCriteria) || j.Importer.Name.Contains(searchCriteria) || j.Product.Name.Trim().Contains(searchCriteria.Trim()))
                           .Include("Invoice")
                            .Include("Depot")
                            .Include("Permit")
                            .Include("Port")
                            .Include("Product")
                            on ntDoc.Id equals nt.Id
                          select nt).ToList();

                        if (myNotifications.Any())
                        {
                            var newList = new List<NotificationObject>();
                            myNotifications.ForEach(app =>
                            {
                                if (!newList.Exists(b => b.Id == app.Id))
                                {
                                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                                    if (importObject != null && importObject.Id > 0)
                                    {
                                        importObject.StatusStr = Enum.GetName(typeof (NotificationStatusEnum),
                                            importObject.Status);
                                        importObject.ImporterName = app.Importer.Name;
                                        importObject.NotificationTypeName = Enum.GetName(typeof (NotificationClassEnum),
                                            importObject.ClassificationId);
                                        importObject.CargoTypeName = Enum.GetName(typeof (CargoTypeEnum),
                                            importObject.CargoInformationTypeId);
                                        importObject.PortName = app.Port.Name;
                                        importObject.DepotName = app.Depot.Name;
                                        importObject.PermitValue = app.Permit.PermitValue;
                                        importObject.ProductName = app.Product.Code;
                                        importObject.QuantityToDischargeStr =
                                            app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                        importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                        importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                            ? app.Permit.PermitValue
                                            : "Not Available";
                                        importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                        ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                        importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                            .Replace(".0", "");
                                        importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                        importObject.ReferenceCode = app.Invoice.RRR;
                                        newList.Add(importObject);

                                    }
                                }
                            });
                            return newList;
                        }
                    return new List<NotificationObject>();
                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> GetPaidNotifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    const int paid = (int)NotificationStatusEnum.Paid;

                    using (var db = new ImportPermitEntities())
                    {
                        var myNotifications = db.Notifications.Where(m => m.Status == paid)
                            .OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                                
                                .Include("Invoice")
                                .Include("Importer")
                                .Include("Depot")
                                .Include("Permit")
                                .Include("Port")
                                .Include("Product")
                                .ToList();

                        if (myNotifications.Any())
                        {
                            var newList = new List<NotificationObject>();
                            myNotifications.ForEach(app =>
                            {
                                var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                                if (importObject != null && importObject.Id > 0)
                                {
                                    importObject.StatusStr = "Pending Submission";
                                        //Enum.GetName(typeof(NotificationStatusEnum),importObject.Status);
                                    importObject.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum),
                                        importObject.ClassificationId);
                                    importObject.CargoTypeName = Enum.GetName(typeof(CargoTypeEnum),
                                        importObject.CargoInformationTypeId);
                                    importObject.PortName = app.Port.Name;
                                    importObject.DepotName = app.Depot.Name;
                                    importObject.ImporterName = app.Importer.Name;
                                    importObject.PermitValue = app.Permit.PermitValue;
                                    importObject.ProductName = app.Product.Code;
                                    importObject.QuantityToDischargeStr =
                                        app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                        .Replace(".0", "");
                                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                        ? app.Permit.PermitValue
                                        : "Not Available";
                                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                    ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                    importObject.ReferenceCode = app.Invoice.RRR;
                                    newList.Add(importObject);
                                }
                            });

                            countG = db.Notifications.Count(m => m.Status == paid);
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<NotificationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> GetSubmittedNotifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    const int submitted = (int)NotificationStatusEnum.Submitted;

                    using (var db = new ImportPermitEntities())
                    {
                        var myNotifications =
                            db.Notifications.Where(m => m.Status == submitted)
                            .OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                                
                                .Include("Invoice")
                                  .Include("Importer")
                                .Include("Depot")
                                .Include("Permit")
                                .Include("Port")
                                .Include("Product")
                                .ToList();

                        if (myNotifications.Any())
                        {
                            var newList = new List<NotificationObject>();
                            myNotifications.ForEach(app =>
                            {
                                var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                                if (importObject != null && importObject.Id > 0)
                                {
                                    importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum),
                                        importObject.Status);
                                    importObject.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum),
                                        importObject.ClassificationId);
                                    importObject.CargoTypeName = Enum.GetName(typeof(CargoTypeEnum),
                                        importObject.CargoInformationTypeId);
                                    importObject.PortName = app.Port.Name;
                                    importObject.DepotName = app.Depot.Name;
                                    importObject.ImporterName = app.Importer.Name;
                                    importObject.PermitValue = app.Permit.PermitValue;
                                    importObject.ProductName = app.Product.Code;
                                    importObject.QuantityToDischargeStr =
                                        app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                        .Replace(".0", "");
                                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                        ? app.Permit.PermitValue
                                        : "Not Available";
                                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                    ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                    importObject.ReferenceCode = app.Invoice.RRR;
                                    newList.Add(importObject);
                                }
                            });

                            countG = db.Notifications.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<NotificationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> GetProcessingNotifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    const int processing = (int)NotificationStatusEnum.Processing;

                    using (var db = new ImportPermitEntities())
                    {
                        var myNotifications =
                            db.Notifications.Where(m => m.Status == processing)
                            .OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                                
                                .Include("Invoice")
                                  .Include("Importer")
                                  .Include("Depot")
                                .Include("Permit")
                                .Include("Port")
                                .Include("Product")
                                .ToList();

                        if (myNotifications.Any())
                        {
                            var newList = new List<NotificationObject>();
                            myNotifications.ForEach(app =>
                            {
                                var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                                if (importObject != null && importObject.Id > 0)
                                {
                                    importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum),
                                        importObject.Status);
                                    importObject.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum),
                                        importObject.ClassificationId);
                                    importObject.CargoTypeName = Enum.GetName(typeof(CargoTypeEnum),
                                        importObject.CargoInformationTypeId);
                                    importObject.PortName = app.Port.Name;
                                    importObject.DepotName = app.Depot.Name;
                                    importObject.ImporterName = app.Importer.Name;
                                    importObject.PermitValue = app.Permit.PermitValue;
                                    importObject.ProductName = app.Product.Code;
                                    importObject.QuantityToDischargeStr =
                                        app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                        .Replace(".0", "");
                                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                        ? app.Permit.PermitValue
                                        : "Not Available";
                                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                    ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                    importObject.ReferenceCode = app.Invoice.RRR;
                                    newList.Add(importObject);
                                }
                            });

                            countG = db.Notifications.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<NotificationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> GetApprovedNotifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    const int approved = (int)NotificationStatusEnum.Approved;

                    using (var db = new ImportPermitEntities())
                    {
                        var myNotifications =
                            db.Notifications.Where(m => m.Status == approved)
                            .OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                                
                                .Include("Invoice")
                                 .Include("Importer")
                                .Include("Depot")
                                .Include("Permit")
                                .Include("Port")
                                .Include("Product")
                                .ToList();

                        if (myNotifications.Any())
                        {
                            var newList = new List<NotificationObject>();
                            myNotifications.ForEach(app =>
                            {
                                var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                                if (importObject != null && importObject.Id > 0)
                                {
                                    importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum), importObject.Status).Replace("_", " ");
                                    importObject.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum),
                                        importObject.ClassificationId);
                                    importObject.CargoTypeName = Enum.GetName(typeof(CargoTypeEnum),
                                        importObject.CargoInformationTypeId);
                                    importObject.PortName = app.Port.Name;
                                    importObject.DepotName = app.Depot.Name;
                                    importObject.ImporterName = app.Importer.Name;
                                    importObject.PermitValue = app.Permit.PermitValue;
                                    importObject.ProductName = app.Product.Code;
                                    importObject.QuantityToDischargeStr =
                                        app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                        .Replace(".0", "");
                                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                        ? app.Permit.PermitValue
                                        : "Not Available";
                                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                    ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                    importObject.ReferenceCode = app.Invoice.RRR;
                                    newList.Add(importObject);
                                }
                            });

                            countG = db.Notifications.Count(m => m.Status == approved);
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<NotificationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> GetRejectedNotifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    const int rejected = (int)NotificationStatusEnum.Rejected;

                    using (var db = new ImportPermitEntities())
                    {
                        var myNotifications =
                            db.Notifications.Where(m => m.Status == rejected)
                            .OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                                
                                .Include("Invoice")
                                 .Include("Importer")
                                .Include("Depot")
                                .Include("Permit")
                                .Include("Port")
                                .Include("Product")
                                .ToList();

                        if (myNotifications.Any())
                        {
                            var newList = new List<NotificationObject>();
                            myNotifications.ForEach(app =>
                            {
                                var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                                if (importObject != null && importObject.Id > 0)
                                {
                                    importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum), importObject.Status).Replace("_", " ");
                                    importObject.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum),
                                        importObject.ClassificationId);
                                    importObject.CargoTypeName = Enum.GetName(typeof(CargoTypeEnum),
                                        importObject.CargoInformationTypeId);
                                    importObject.PortName = app.Port.Name;
                                    importObject.DepotName = app.Depot.Name;
                                    importObject.ImporterName = app.Importer.Name;
                                    importObject.PermitValue = app.Permit.PermitValue;
                                    importObject.ProductName = app.Product.Code;
                                    importObject.QuantityToDischargeStr =
                                        app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                        .Replace(".0", "");
                                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                        ? app.Permit.PermitValue
                                        : "Not Available";
                                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                    ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                    importObject.ReferenceCode = app.Invoice.RRR;
                                    newList.Add(importObject);
                                }
                            });

                            countG = db.Notifications.Count(m => m.Status == rejected);
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<NotificationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> SearchPaidNotifications(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    const int paid = (int)NotificationStatusEnum.Paid;

                    var myNotifications =
                        db.Notifications.Where(m => m.Status == paid && (m.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || m.Invoice.ReferenceCode.Contains(searchCriteria.Trim()) || m.Importer.Name.Contains(searchCriteria.Trim())))
                         .Include("Invoice")
                          .Include("Importer")
                           .Include("Depot")
                            .Include("Permit")
                            .Include("Port")
                            .Include("Product")
                            .ToList();

                        if (myNotifications.Any())
                        {
                            var newList = new List<NotificationObject>();
                            myNotifications.ForEach(app =>
                            {
                                var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                                if (importObject != null && importObject.Id > 0)
                                {
                                    importObject.StatusStr = "Pending Submission";
                                    //Enum.GetName(typeof(NotificationStatusEnum),importObject.Status);
                                    importObject.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum),
                                        importObject.ClassificationId);
                                    importObject.CargoTypeName = Enum.GetName(typeof(CargoTypeEnum),
                                        importObject.CargoInformationTypeId);
                                    importObject.PortName = app.Port.Name;
                                    importObject.DepotName = app.Depot.Name;
                                    importObject.ImporterName = app.Importer.Name;
                                    importObject.PermitValue = app.Permit.PermitValue;
                                    importObject.ProductName = app.Product.Code;
                                    importObject.QuantityToDischargeStr =
                                        app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                    importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                        .Replace(".0", "");
                                    importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                        ? app.Permit.PermitValue
                                        : "Not Available";
                                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                    ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                    importObject.ReferenceCode = app.Invoice.RRR;
                                    newList.Add(importObject);
                                }
                            });

                            return newList;
                        }
                    return new List<NotificationObject>();
                    }
              }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> SearchSubmittedNotifications(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    const int submitted = (int)NotificationStatusEnum.Submitted;

                    var myNotifications =
                        db.Notifications.Where(m => m.Status == submitted && (m.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || m.Invoice.ReferenceCode.Contains(searchCriteria.Trim()) || m.Importer.Name.Contains(searchCriteria.Trim())))
                       .Include("Invoice")
                          .Include("Importer")
                           .Include("Depot")
                            .Include("Permit")
                            .Include("Port")
                            .Include("Product")
                            .ToList();

                    if (myNotifications.Any())
                    {
                        var newList = new List<NotificationObject>();
                        myNotifications.ForEach(app =>
                        {
                            var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                            if (importObject != null && importObject.Id > 0)
                            {
                                importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum), importObject.Status).Replace("_", " ");
                                importObject.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum),
                                    importObject.ClassificationId);
                                importObject.CargoTypeName = Enum.GetName(typeof(CargoTypeEnum),
                                    importObject.CargoInformationTypeId);
                                importObject.PortName = app.Port.Name;
                                importObject.DepotName = app.Depot.Name;
                                importObject.ImporterName = app.Importer.Name;
                                importObject.PermitValue = app.Permit.PermitValue;
                                importObject.ProductName = app.Product.Code;
                                importObject.QuantityToDischargeStr =
                                    app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                    .Replace(".0", "");
                                importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                    ? app.Permit.PermitValue
                                    : "Not Available";
                                importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                importObject.ReferenceCode = app.Invoice.RRR;
                                newList.Add(importObject);
                            }
                        });

                        return newList;
                    }
                    return new List<NotificationObject>();
                }
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> SearchProcessingNotifications(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    const int processing = (int)NotificationStatusEnum.Processing;

                    var myNotifications =
                        db.Notifications.Where(m => m.Status == processing && (m.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || m.Invoice.ReferenceCode.Contains(searchCriteria.Trim()) || m.Importer.Name.Contains(searchCriteria.Trim())))
                        .Include("Invoice")
                          .Include("Importer")
                           .Include("Depot")
                            .Include("Permit")
                            .Include("Port")
                            .Include("Product")
                            .ToList();

                    if (myNotifications.Any())
                    {
                        var newList = new List<NotificationObject>();
                        myNotifications.ForEach(app =>
                        {
                            var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                            if (importObject != null && importObject.Id > 0)
                            {
                                importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum), importObject.Status).Replace("_", " ");
                                importObject.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum),
                                    importObject.ClassificationId);
                                importObject.CargoTypeName = Enum.GetName(typeof(CargoTypeEnum),
                                    importObject.CargoInformationTypeId);
                                importObject.PortName = app.Port.Name;
                                importObject.DepotName = app.Depot.Name;
                                importObject.ImporterName = app.Importer.Name;
                                importObject.PermitValue = app.Permit.PermitValue;
                                importObject.ProductName = app.Product.Code;
                                importObject.QuantityToDischargeStr =
                                    app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                    .Replace(".0", "");
                                importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                    ? app.Permit.PermitValue
                                    : "Not Available";
                                importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                importObject.ReferenceCode = app.Invoice.RRR;
                                newList.Add(importObject);
                            }
                        });

                        return newList;
                    }
                    return new List<NotificationObject>();
                }
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> SearchApprovedNotifications(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    const int approved = (int)NotificationStatusEnum.Approved;

                    var myNotifications =
                        db.Notifications.Where(m => m.Status == approved && (m.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || m.Invoice.ReferenceCode.Contains(searchCriteria.Trim()) || m.Importer.Name.Contains(searchCriteria.Trim())))
                       .Include("Invoice")
                          .Include("Importer")
                           .Include("Depot")
                            .Include("Permit")
                            .Include("Port")
                            .Include("Product")
                            .ToList();

                    if (myNotifications.Any())
                    {
                        var newList = new List<NotificationObject>();
                        myNotifications.ForEach(app =>
                        {
                            var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                            if (importObject != null && importObject.Id > 0)
                            {
                                importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum), importObject.Status).Replace("_", " ");
                                importObject.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum),
                                    importObject.ClassificationId);
                                importObject.CargoTypeName = Enum.GetName(typeof(CargoTypeEnum),
                                    importObject.CargoInformationTypeId);
                                importObject.PortName = app.Port.Name;
                                importObject.DepotName = app.Depot.Name;
                                importObject.ImporterName = app.Importer.Name;
                                importObject.PermitValue = app.Permit.PermitValue;
                                importObject.ProductName = app.Product.Code;
                                importObject.QuantityToDischargeStr =
                                    app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                    .Replace(".0", "");
                                importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                    ? app.Permit.PermitValue
                                    : "Not Available";
                                importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                importObject.ReferenceCode = app.Invoice.RRR;
                                newList.Add(importObject);
                            }
                        });
                        return newList;
                    }
                    return new List<NotificationObject>();
                }
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }

        public List<NotificationObject> SearchRejectedNotifications(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    const int rejected = (int)NotificationStatusEnum.Rejected;

                    var myNotifications =
                        db.Notifications.Where(m => m.Status == rejected && (m.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || m.Invoice.ReferenceCode.Contains(searchCriteria.Trim()) || m.Importer.Name.Contains(searchCriteria.Trim())))
                        .Include("Invoice")
                          .Include("Importer")
                           .Include("Depot")
                            .Include("Permit")
                            .Include("Port")
                            .Include("Product")
                            .ToList();

                    if (myNotifications.Any())
                    {
                        var newList = new List<NotificationObject>();
                        myNotifications.ForEach(app =>
                        {
                            var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                            if (importObject != null && importObject.Id > 0)
                            {
                                importObject.StatusStr = Enum.GetName(typeof(NotificationStatusEnum), importObject.Status).Replace("_", " ");
                                importObject.NotificationTypeName = Enum.GetName(typeof(NotificationClassEnum),
                                    importObject.ClassificationId);
                                importObject.CargoTypeName = Enum.GetName(typeof(CargoTypeEnum),
                                    importObject.CargoInformationTypeId);
                                importObject.PortName = app.Port.Name;
                                importObject.DepotName = app.Depot.Name;
                                importObject.ImporterName = app.Importer.Name;
                                importObject.PermitValue = app.Permit.PermitValue;
                                importObject.ProductName = app.Product.Code;
                                importObject.QuantityToDischargeStr =
                                    app.QuantityToDischarge.ToString("n1").Replace(".0", "");
                                importObject.QuantityOnVesselStr = app.QuantityOnVessel.ToString("n1")
                                    .Replace(".0", "");
                                importObject.PermitValue = app.Permit != null && app.Permit.Id > 0
                                    ? app.Permit.PermitValue
                                    : "Not Available";
                                importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy") + " - " + app.DischargeDate.ToString("dd/MM/yyyy");
                                importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("N");
                                ////importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                                importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                                importObject.ReferenceCode = app.Invoice.RRR;
                                newList.Add(importObject);
                            }
                        });

                        return newList;
                    }
                    return new List<NotificationObject>();
                }
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationObject>();
            }
        }
       
    }
}
