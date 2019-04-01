using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
    public class EmployeeProfileManager
    {

        public ApplicationObject GetApplicationAdmin(long Id)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    

                    var myApplications =
                        db.Applications.Where(m => m.Id == Id)
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
                            item.DocumentPathStr = item.DocumentPathStr.Replace("~", "").Replace("//", string.Empty);
                            importObject.ApplicationDocumentObjects.Add(item);
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


        public NotificationObject GetNotificationAdmin(long Id)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {


                    var myApplications =
                        db.Notifications.Where(m => m.Id == Id)
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
                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString();
                    importObject.DischargeDateStr = importObject.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.QuantityToDischargeStr = importObject.QuantityToDischarge.ToString();
                    importObject.ProductName = app.Product.Name;
                    importObject.DepotName = app.Depot.Name;

                    importObject.DischargeDepotId = app.DischargeDepotId;

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
                            item.DocumentPathStr = item.DocumentPathStr.Replace("~", "").Replace("//", string.Empty);
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
        public ApplicationObject GetApplication(long trackId, long userId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var track = db.ProcessTrackings.Find(trackId);

                    var myApplications =
                        db.Applications.Where(m => m.Id == track.ApplicationId)
                            .Include("Importer")
                            .Include("Invoice")
                            .Include("ApplicationItems")
                            .Include("ApplicationDocuments")
                            .Include("ProcessingHistories")
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

                    const int appStage = (int)AppStage.Application;
                    var fees = db.Fees.Where(k => k.ImportStageId == appStage).Include("FeeType").Include("ImportStage").ToList();
                    if (!fees.Any())
                    {
                        return new ApplicationObject();
                    }
                    var objList = new List<FeeObject>();
                    const int statutoryFeeId = (int)FeeTypeEnum.Statutory_Fee;
                    const int processingFeeId = (int)FeeTypeEnum.Processing_Fee;
                    var statutoryFee = fees.Find(m => m.FeeTypeId == statutoryFeeId);
                    var processingFee = fees.Find(m => m.FeeTypeId == processingFeeId);
                    if (statutoryFee == null || statutoryFee.FeeId < 1)
                    {
                        return new ApplicationObject();
                    }
                    fees.ForEach(fee =>
                    {
                        var feeObject = ModelMapper.Map<Fee, FeeObject>(fee);
                        if (feeObject != null && feeObject.FeeId > 0)
                        {
                            if (feeObject.FeeTypeId == statutoryFeeId)
                            {
                                feeObject.FeeTypeName = fee.FeeType.Name + " (" + WebUtility.HtmlDecode("&#8358;") + feeObject.Amount.ToString("n1") + "/" + "30,000 MT)";
                                feeObject.Amount = app.Invoice.TotalAmountDue - processingFee.Amount;
                                feeObject.AmountStr = feeObject.Amount.ToString("n1");
                            }
                            else
                            {
                                feeObject.FeeTypeName = fee.FeeType.Name;
                            }
                            feeObject.ImportStageName = fee.ImportStage.Name;
                            objList.Add(feeObject);
                        }
                    });

                    importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                    importObject.ReferenceCode = app.Invoice.ReferenceCode;
                    importObject.Rrr = app.Invoice.RRR;
                    importObject.InvoiceId = app.Invoice.Id;
                    importObject.PaymentTypeId = app.Invoice.PaymentTypeId;
                    importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                    importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                    importObject.CompanyName = app.Importer.Name;
                    importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                    var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                    if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                    importObject.FeeObjects = objList;
                    var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                    if (name != null) importObject.StatusStr = name.Replace("_", " ");
                    importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                    const int psf = (int)CustomColEnum.Psf;
                    importObject.ApplicationTypeName = app.ApplicationType.Name;
                    

                    importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                    importObject.ApplicationDocumentObjects = new List<ApplicationDocumentObject>();

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
                        if (im != null && im.Id > 0)
                        {
                            im.ProductObject = (from pr in db.Products.Where(x => x.ProductId == im.ProductId).Include("ProductColumns")
                                                select new ProductObject
                                                {
                                                    ProductId = pr.ProductId,
                                                    Code = pr.Code,
                                                    Name = pr.Name,
                                                    RequiresPsf = pr.ProductColumns.Any() && pr.ProductColumns.Any(v => v.CustomCodeId == psf),
                                                    Availability = pr.Availability
                                                }).ToList()[0];

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
                                im.ThroughPutObjects = new List<ThroughPutObject>();
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

                                    im.ThroughPutObjects.Add(new ThroughPutObject
                                    {
                                        Id = d.Id,
                                        DepotId = d.DepotId,
                                        ProductName = im.ProductObject.Name,
                                        ProductCode = im.ProductObject.Code,
                                        ProductId = d.ProductId,
                                        Quantity = d.Quantity,
                                        DocumentId = d.DocumentId,
                                        DocumentPath = d.Document != null && !string.IsNullOrEmpty(d.Document.DocumentPath) ? d.Document.DocumentPath.Replace("~", "") : string.Empty,
                                        IPAddress = d.IPAddress,
                                        DepotName = d.Depot.Name,
                                        ThName = d.Depot.Name,
                                        Status = d.Document != null ? d.Document.Status : 0,
                                        StatusStr = d.Document != null ? Enum.GetName(typeof(DocStatus), d.Document.Status) : string.Empty,
                                        Comment = d.Comment,
                                        ApplicationItemId = d.ApplicationItemId,
                                        DepotObject = new DepotObject
                                        {
                                            Id = d.Depot.Id,
                                            Name = d.Depot.Name,
                                            DepotLicense = d.Depot.DepotLicense,
                                            IssueDate = d.Depot.IssueDate,
                                            ExpiryDate = d.Depot.ExpiryDate,
                                            ImporterId = d.Depot.ImporterId
                                        }
                                    });

                                });
                            }

                            im.ProductBankerObjects = new List<ProductBankerObject>();
                            im.ProductBankerName = "";
                            bankers.ForEach(c =>
                            {
                                if (string.IsNullOrEmpty(im.ProductBankerName))
                                {
                                    im.ProductBankerName = c.Bank.Name;
                                }
                                else
                                {
                                    im.ProductBankerName += ", " + c.Bank.Name;
                                }
                                var pdBnk = new ProductBankerObject
                                {
                                    Id = c.Id,
                                    ApplicationItemId = c.ApplicationItemId,
                                    BankId = c.BankId,
                                    BankName = c.Bank.Name,
                                    ApplicationId = app.Id,
                                    ImporterId = app.ImporterId,
                                    DocumentId = c.DocumentId,
                                    IsUploaded = c.DocumentId != null ? true : false,
                                    ProductCode = im.ProductObject.Code,
                                    DocumentPath = c.Document != null ? c.Document.DocumentPath : string.Empty,
                                    DocumentStatus = c.Document != null ? Enum.GetName(typeof(DocStatus), c.Document.Status) : string.Empty
                                };

                                if (c.Document != null && c.DocumentId > 0)
                                {
                                    pdBnk.DocumentObject = new DocumentObject
                                    {
                                        DocumentId = c.Document.DocumentId,
                                        DocumentTypeId = c.Document.DocumentTypeId,
                                        DocumentPath = c.Document.DocumentPath,
                                        Status = c.Document.Status,
                                        StatusStr = Enum.GetName(typeof(DocStatus), c.Document.Status)
                                    };
                                }

                                im.ProductBankerObjects.Add(pdBnk);
                            });

                            var strpType = db.StorageProviderTypes.Where(v => v.Id == im.StorageProviderTypeId).ToList();
                            if (strpType.Any())
                            {
                                im.StorageProviderTypeName = strpType[0].Name;
                            }
                            importObject.ApplicationItemObjects.Add(im);
                        }
                    });

                    var appDocs = app.ApplicationDocuments.ToList();
                    if (appDocs.Any())
                    {
                        appDocs.ForEach(o =>
                        {
                            const int pBankerDocId = (int)SpecialDocsEnum.Bank_Reference_Letter;
                            const int thphDocId = (int)SpecialDocsEnum.Throughput_agreement;
                            var docs = db.Documents.Where(c => c.DocumentId == o.DocumentId).Include("DocumentType").ToList();
                            if (docs.Any())
                            {
                                var doc = docs[0];
                                if (doc.DocumentTypeId != pBankerDocId && doc.DocumentTypeId != thphDocId)
                                {
                                    importObject.DocumentTypeObjects.Add(new DocumentTypeObject
                                    {
                                        Uploaded = true,
                                        StatusStr = Enum.GetName(typeof(DocStatus), doc.Status),
                                        Status = doc.Status,
                                        DocumentPath = doc.DocumentPath.Replace("~", ""),
                                        DocumentTypeId = doc.DocumentType.DocumentTypeId,
                                        Name = doc.DocumentType.Name
                                    });
                                }
                            }

                        });
                    }

                    //check the activity type of the employee
                    var checkEmployee = db.EmployeeDesks.Where(e => e.EmployeeId == userId).ToList();

                    if (checkEmployee.Any())
                    {
                        var actId = checkEmployee[0].ActivityTypeId;
                        var checkActivity = db.StepActivityTypes.Where(c => c.Id == actId).ToList();
                        var activity = "";
                        if (checkActivity.Any())
                        {
                            activity = checkActivity[0].Name;

                        }
                        importObject.Activity = activity;
                    }

                    //get application history
                    importObject.ProcessingHistoryObjects = new List<ProcessingHistoryObject>();

                    var his = db.ProcessingHistories.Where(h => h.ApplicationId == importObject.Id).ToList();

                    if (his.Any())
                    {
                        foreach (var item in his)
                        {
                            var hisObj = new ProcessingHistoryObject();
                            var empId = item.EmployeeId;
                            var emp = db.EmployeeDesks.Where(e => e.Id == empId).ToList();
                            if (emp.Any())
                            {
                                var proId = emp[0].EmployeeId;
                                var pro = db.UserProfiles.Where(p => p.Id == proId).ToList();
                                if (pro.Any())
                                {
                                    var personId = pro[0].PersonId;
                                    var person = db.People.Where(r => r.Id == personId).ToList();
                                    if (person.Any())
                                    {
                                        hisObj.EmployeeName = person[0].FirstName;
                                        hisObj.StepName = item.Step.Name;
                                        hisObj.AssignedTimeStr = item.AssignedTime.ToString();
                                        hisObj.ActualDeliveryDateTimeStr = item.FinishedTime.ToString();
                                        hisObj.Remarks = item.Remarks;
                                        importObject.ProcessingHistoryObjects.Add(hisObj);
                                    }
                                }
                            }

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
		
        public ResponseObject GetDashboard(long userId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == userId).ToList();

                    if (employeeDesk.Any())
                    {
                        var res = new ResponseObject();
                        res.applications = employeeDesk[0].ApplicationCount.ToString();
                        res.notifications = employeeDesk[0].NotificationCount.ToString();
                        res.recertifications = employeeDesk[0].RecertificationCount.ToString();

                        return res;
                    }

                    return new ResponseObject();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ResponseObject();
            }
        }



        public RecertificationObject GetRecertification(long trackId, long userId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var track = db.RecertificationProcesses.Find(trackId);
                  
                    var docs = new List<DocumentObject>();

                    var recertifications =
                        db.Recertifications.Where(m => m.Id == track.RecertificationId)
                                              
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
                return new RecertificationObject();
            }
        }



        public ApplicationObject GetApplicationFromHistory(long historyId, long userId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var history = db.ProcessingHistories.Find(historyId);

                    var myApplications =
                        db.Applications.Where(m => m.Id == history.ApplicationId)
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
                            item.DocumentPathStr = item.DocumentPathStr.Replace("~", "").Replace("//", string.Empty);
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

        public NotificationObject GetNotification(long trackId, long userId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var track = db.NotificationInspectionQueues.Find(trackId);

                    var myApplications =
                        db.Notifications.Where(m => m.Id == track.NotificationId)
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

                    importObject.QuantityOnVesselStr = importObject.QuantityOnVessel.ToString("n1");
                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("n1");

                    importObject.DischargeDateStr = importObject.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.QuantityToDischargeStr = importObject.QuantityToDischarge.ToString("n1");
                    importObject.SequenceNumber = (int)track.Step.SequenceNumber;
                    importObject.DepotName = app.Depot.Name;
                    importObject.ReferenceCode = app.Invoice.ReferenceCode;
                    importObject.CountryName = app.Port.Country.Name;
                    importObject.ImporterName = app.Importer.Name;
                   
                    importObject.NotificationDocumentObjects = new List<NotificationDocumentObject>();
                    importObject.ProductObject = new ProductObject();
                    importObject.NotificationInspectionObjects = new List<NotificationInspectionObject>();
                 
                    
                    var product = (from p in db.Products where p.ProductId == importObject.ProductId
                               

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
                  
                  

                    //var doc = (from ad in app.NotificationDocuments
                    //           join d in db.Documents on ad.DocumentId equals d.DocumentId

                    //           select new NotificationDocumentObject()
                    //           {
                    //               DocumentId = d.DocumentId,
                    //               DocumentTypeName = d.DocumentType.Name,
                    //               DateUploaded = d.DateUploaded,
                    //               DocumentPathStr = d.DocumentPath,
                    //               IsValid = d.IsValid
                    //           }).ToList();


                    //if (doc.Any())
                    //{
                    //    foreach (var item in doc)
                    //    {
                    //        item.DateUploadedStr = item.DateUploaded.ToString("dd/MM/yyyy");
                    //        item.DocumentPathStr = item.DocumentPathStr.Replace("~", "").Replace("//", string.Empty);
                    //        importObject.NotificationDocumentObjects.Add(item);
                    //    }

                    //}

                    var notDocObjs = new List<NotificationDocumentObject>();
                    var doc = db.NotificationDocuments.Where(o => o.NotificationId == importObject.Id).ToList();

                    foreach (var item in doc)
                    {
                        var normalDoc = db.Documents.Where(m => m.DocumentId == item.DocumentId).ToList();
                        if (normalDoc.Any())
                        {
                    
                            var notDocObj = new NotificationDocumentObject();
                            notDocObj.DocumentId = normalDoc[0].DocumentId;
                            notDocObj.DateUploadedStr = normalDoc[0].DateUploaded.ToString("dd/MM/yyyy");
                            notDocObj.DocumentTypeName = normalDoc[0].DocumentType.Name;
                            notDocObj.DocumentPathStr = normalDoc[0].DocumentPath.Replace("~", "").Replace("//", string.Empty); 
                            notDocObj.IsValid = normalDoc[0].IsValid;

                            notDocObjs.Add(notDocObj);
                        }
                    }

                    importObject.NotificationDocumentObjects = notDocObjs;


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
                            var zoneId = checkEmployee[0].ZoneId;

                            if (activity == "TeamLead")
                            {
                                importObject.EmployeeDeskObjects = new List<EmployeeDeskObject>();
                               
                                var inspectors =
                                    db.EmployeeDesks.Where(
                                        i => i.ZoneId == zoneId && i.StepActivityType.Name == "Inspection").ToList();

                                foreach (var item in inspectors)
                                {
                                    var emp = new EmployeeDeskObject();
                                    var pro = db.UserProfiles.Find(item.EmployeeId);
                                    var person = db.People.Find(pro.PersonId);
                                    emp.FirstName = person.FirstName;
                                    emp.Id = item.Id;

                                    importObject.EmployeeDeskObjects.Add(emp);
                                }
                            }
                            
                        }
                        importObject.Activity = activity;
                    }
                    //check whether vessel report is saved
                    var vesselReport = db.NotificationInspections.Where(n => n.NotificationId == track.NotificationId);
                    if (vesselReport.Any())
                    {
                        importObject.IsVesselReportSaved = true;
                    }

                    ////check whether checklist is accessed for the first time by the employee
                    //var checkFirst = db.NotificationCheckListOutcomes.Where(n => n.NotificationId == track.NotificationId);
                    //if (!checkFirst.Any())
                    //{
                    //    importObject.IsCheckListFirstTime = true;
                    //}
                    ////check whether checklist is stored
                    //var checkStored = db.NotificationCheckListOutcomes.Where(n => n.NotificationId == track.NotificationId);
                    //if (checkStored.Any())
                    //{
                    //    importObject.IsCheckListStored = true;
                    //}

                    ////check whether checklist is verified
                    //var checkS = db.NotificationCheckListOutcomes.Where(n => n.NotificationId == track.NotificationId && n.Status == (int)EnumCheckListOutComeStatus.Saved);
                    //if (checkS.Any())
                    //{
                    //    importObject.IsReportCheckListSaved = true;
                    //}

                    //check whether recertification is stored
                    var recertificationS = db.RecertificationResults.Where(n => n.NotificationId == track.NotificationId && n.SubmittionStatus == (int)EnumCheckListOutComeStatus.Saved);
                    if (recertificationS.Any())
                    {
                        importObject.IsRecertificationSaved = true;
                    }

                    //check whether recertification is submitted
                    var recertification = db.RecertificationResults.Where(n => n.NotificationId == track.NotificationId && n.SubmittionStatus == (int)EnumCheckListOutComeStatus.Submitted);
                    if (recertification.Any())
                    {
                        importObject.IsRecertificationSubmitted = true;
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


        public NotificationObject GetNotificationBack(long Id, long userId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                   

                    var myApplications =
                        db.Notifications.Where(m => m.Id == Id)
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
                    importObject.AmountDueStr = app.Invoice.TotalAmountDue.ToString("n1");
                    importObject.DischargeDateStr = importObject.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.QuantityToDischargeStr = importObject.QuantityToDischarge.ToString();

                    var track = db.NotificationInspectionQueues.Where(t=>t.NotificationId == Id).ToList();
                    if (track.Any())
                    {
                        importObject.SequenceNumber = (int)track[0].Step.SequenceNumber;
                    }

                    importObject.DepotName = app.Depot.Name;
                    importObject.ReferenceCode = app.Invoice.ReferenceCode;
                    importObject.CountryName = app.Port.Country.Name;
                    importObject.ImporterName = app.Importer.Name;

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



                 

                    var notDocObjs = new List<NotificationDocumentObject>();
                    var doc = db.NotificationDocuments.Where(o => o.NotificationId == importObject.Id).ToList();

                    foreach (var item in doc)
                    {
                        var normalDoc = db.Documents.Where(m => m.DocumentId == item.DocumentId).ToList();
                        if (normalDoc.Any())
                        {

                            var notDocObj = new NotificationDocumentObject();
                            notDocObj.DocumentId = normalDoc[0].DocumentId;
                            notDocObj.DateUploadedStr = normalDoc[0].DateUploaded.ToString("dd/MM/yyyy");
                            notDocObj.DocumentTypeName = normalDoc[0].DocumentType.Name;
                            notDocObj.DocumentPathStr = normalDoc[0].DocumentPath;
                            notDocObj.IsValid = normalDoc[0].IsValid;

                            notDocObjs.Add(notDocObj);
                        }
                    }

                    importObject.NotificationDocumentObjects = notDocObjs;


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

                    //check whether checklist is accessed for the first time by the employee
                    var checkFirst = db.NotificationCheckListOutcomes.Where(n => n.NotificationId == Id);
                    if (!checkFirst.Any())
                    {
                        importObject.IsCheckListFirstTime = true;
                    }
                    //check whether checklist is stored
                    var checkStored = db.NotificationCheckListOutcomes.Where(n => n.NotificationId == Id);
                    if (checkStored.Any())
                    {
                        importObject.IsCheckListStored = true;
                    }

                    //check whether checklist is verified
                    var checkS = db.NotificationCheckListOutcomes.Where(n => n.NotificationId == Id && n.Status == (int)EnumCheckListOutComeStatus.Saved);
                    if (checkS.Any())
                    {
                        importObject.IsReportCheckListSaved = true;
                    }

                    //check whether recertification is stored
                    var recertificationS = db.RecertificationResults.Where(n => n.NotificationId == Id && n.SubmittionStatus == (int)EnumCheckListOutComeStatus.Saved);
                    if (recertificationS.Any())
                    {
                        importObject.IsRecertificationSaved = true;
                    }

                    //check whether recertification is submitted
                    var recertification = db.RecertificationResults.Where(n => n.NotificationId == Id && n.SubmittionStatus == (int)EnumCheckListOutComeStatus.Submitted);
                    if (recertification.Any())
                    {
                        importObject.IsRecertificationSubmitted = true;
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

     

     

        public ApplicationObject GetPreviousApplicationTasks(long id, string userId)
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
                if (processTrackingEntity == null || string.IsNullOrEmpty(processTrackingEntity.Application.Invoice.ReferenceCode))
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

        public List<ProcessTrackingObject> GetEmployeeProfiles(int? itemsPerPage, int? pageNumber, out int countG, string userId)
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
                            db.ProcessTrackings.Where(m => m.EmployeeId.Equals(employeeId) && m.StatusId == (int)AppStatus.Processing).OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)
.Include("Application")
                                .ToList();
                        if (processTrackings.Any())
                        {
                            var newList = new List<ProcessTrackingObject>();
                            //var processTrackingObject = new ProcessTrackingObject();
                            processTrackings.ForEach(app =>
                            {
                                var processTrackingObject = ModelMapper.Map<ProcessTracking, ProcessTrackingObject>(app);
                                if (processTrackingObject != null && processTrackingObject.Id > 0)
                                {
                                    processTrackingObject.ReferenceCode = app.Application.Invoice.RRR;
                                    processTrackingObject.AssignedTimeStr = app.AssignedTime.ToString();


                                    TimeSpan time = new TimeSpan(0, Convert.ToInt32(app.Step.ExpectedDeliveryDuration), 0, 0);
                                    var hr = Convert.ToInt32(app.Step.ExpectedDeliveryDuration);
                                    var combined = app.AssignedTime.Value.AddHours(hr);


                                    processTrackingObject.DueTimeStr = combined.ToString();
                                    processTrackingObject.CompanyName = app.Application.Importer.Name;
                                    newList.Add(processTrackingObject);
                                }
                            });

                            //foreach (var item in processTrackings)
                            //{
                            //    processTrackingObject.ReferenceCode = item.Application.Invoice.ReferenceCode;
                            //    processTrackingObject.AssignedTimeStr = item.AssignedTime.ToString();


                            //    TimeSpan time = new TimeSpan(0, Convert.ToInt32(item.Step.ExpectedDeliveryDuration), 0, 0);
                            //    var hr = Convert.ToInt32(item.Step.ExpectedDeliveryDuration);
                            //    var combined = item.AssignedTime.Value.AddHours(hr);


                            //    processTrackingObject.DueTimeStr = combined.ToString();
                            //    processTrackingObject.CompanyName = item.Application.Importer.Name;
                            //    newList.Add(processTrackingObject);
                            //}
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


        public List<NotificationInspectionQueueObject> GetNotificationTrackProfiles(int? itemsPerPage, int? pageNumber, out int countG, string userId)
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
                                db.NotificationInspectionQueues.Where(m => m.EmployeeId.Equals(employeeId) && m.StepCode != 0 && m.StepCode != -1).OrderByDescending(m => m.Id)
                                    .Skip(tpageNumber).Take(tsize)
                                    .Include("Notification")
                                    .ToList();
                            if (processTrackings.Any())
                            {
                                var newList = new List<NotificationInspectionQueueObject>();
                                processTrackings.ForEach(app =>
                                {
                                    var processTrackingObject = ModelMapper.Map<NotificationInspectionQueue, NotificationInspectionQueueObject>(app);
                                    if (processTrackingObject != null && processTrackingObject.Id > 0)
                                    {
                                        processTrackingObject.ReferenceCode = app.Notification.Invoice.RRR;
                                        processTrackingObject.AssignedTimeStr = app.AssignedTime.ToString();

                                        TimeSpan time = new TimeSpan(0, Convert.ToInt32(app.Step.ExpectedDeliveryDuration), 0, 0);
                                        var hr = Convert.ToInt32(app.Step.ExpectedDeliveryDuration);
                                        var combined = app.AssignedTime.Value.AddHours(hr);

                                        processTrackingObject.DueTimeStr = combined.ToString();
                                        processTrackingObject.CompanyName = app.Notification.Importer.Name;
                                       
                                        newList.Add(processTrackingObject);
                                    }
                                });
                                countG = db.NotificationInspectionQueues.Count();
                                return newList;
                            }
                        }
                    }

                }
                countG = 0;
                return new List<NotificationInspectionQueueObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationInspectionQueueObject>();
            }
        }


        public List<RecertificationProcessObject> GetRecertificationTrackProfiles(int? itemsPerPage, int? pageNumber, out int countG, string userId)
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
                                db.RecertificationProcesses.Where(m => m.EmployeeId == employeeId && m.StatusId == (int)RecertificationStatusEnum.Processing).OrderByDescending(m => m.Id)
                                    .Skip(tpageNumber).Take(tsize)
    .Include("Recertification")
                                    .ToList();
                            if (processTrackings.Any())
                            {
                                var newList = new List<RecertificationProcessObject>();
                                processTrackings.ForEach(app =>
                                {
                                    var processTrackingObject = ModelMapper.Map<RecertificationProcess, RecertificationProcessObject>(app);
                                    if (processTrackingObject != null && processTrackingObject.Id > 0)
                                    {
                                        processTrackingObject.ReferenceCode = app.Recertification.Notification.Invoice.ReferenceCode;
                                        processTrackingObject.AssignedTimeStr = app.AssignedTime.ToString();

                                        TimeSpan time = new TimeSpan(0, Convert.ToInt32(app.Step.ExpectedDeliveryDuration), 0, 0);
                                        var hr = Convert.ToInt32(app.Step.ExpectedDeliveryDuration);
                                        var combined = app.AssignedTime.Value.AddHours(hr);

                                        processTrackingObject.DueTimeStr = combined.ToString();
                                        processTrackingObject.CompanyName =
                                            app.Recertification.Notification.Importer.Name;
                                        newList.Add(processTrackingObject);
                                    }
                                });
                                countG = db.RecertificationProcesses.Count();
                                return newList;
                            }
                        }
                    }

                }
                countG = 0;
                return new List<RecertificationProcessObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<RecertificationProcessObject>();
            }
        }



        public List<ProcessingHistoryObject> GetPreviousJobsProfiles(int? itemsPerPage, int? pageNumber, out int countG, string userId)
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
                                        processTrackingObject.ReferenceCode = app.Application.Invoice.ReferenceCode;
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
                return new List<ProcessingHistoryObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<ProcessingHistoryObject>();
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
                    var processTrackings =
                        db.ProcessTrackings.Where(m => m.Application.Invoice.ReferenceCode.ToLower().Trim() == searchCriteria.ToLower().Trim())
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

        public NotificationObject GetNotificationFromDetail(long Id, long userId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {


                    var myApplications =
                        db.Notifications.Where(m => m.Id == Id)
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
                    importObject.ProductName = app.Product.Name;
                    importObject.DepotName = app.Depot.Name;

                    importObject.DischargeDepotId = app.DischargeDepotId;

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
                            item.DocumentPathStr = item.DocumentPathStr.Replace("~", "").Replace("//", string.Empty);
                            importObject.NotificationDocumentObjects.Add(item);
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

                    //check whether checklist is accessed for the first time by the employee
                    var checkFirst = db.NotificationCheckListOutcomes.Where(n => n.NotificationId == Id);
                    if (!checkFirst.Any())
                    {
                        importObject.IsCheckListFirstTime = true;
                    }
                    //check whether checklist is stored
                    var checkStored = db.NotificationCheckListOutcomes.Where(n => n.NotificationId == Id);
                    if (checkStored.Any())
                    {
                        importObject.IsCheckListStored = true;
                    }

                    //check whether checklist is verified
                    var checkS = db.NotificationCheckListOutcomes.Where(n => n.NotificationId == Id && n.Status == (int)EnumCheckListOutComeStatus.Saved);
                    if (checkS.Any())
                    {
                        importObject.IsReportCheckListSaved = true;
                    }

                    //check whether recertification is stored
                    var recertificationS = db.RecertificationResults.Where(n => n.NotificationId == Id && n.SubmittionStatus == (int)EnumCheckListOutComeStatus.Saved);
                    if (recertificationS.Any())
                    {
                        importObject.IsRecertificationSaved = true;
                    }


                    //check whether recertification is submitted
                    var recertification = db.RecertificationResults.Where(n => n.NotificationId == Id && n.SubmittionStatus == (int)EnumCheckListOutComeStatus.Submitted);
                    if (recertification.Any())
                    {
                        importObject.IsRecertificationSubmitted = true;
                    }

                    //check whether vessel report is stored
                    var vesselReportSaved = db.NotificationInspections.Where(n => n.NotificationId == Id && n.SubmittionStatus == (int)EnumCheckListOutComeStatus.Saved);
                    if (vesselReportSaved.Any())
                    {
                        importObject.IsVesselReportSaved = true;
                    }

                    //check whether vessel report is submitted
                    var vesselReportSubmitted = db.NotificationInspections.Where(n => n.NotificationId == Id && n.SubmittionStatus == (int)EnumCheckListOutComeStatus.Submitted);
                    if (vesselReportSubmitted.Any())
                    {
                        importObject.IsVesselReportSubmitted = true;
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
