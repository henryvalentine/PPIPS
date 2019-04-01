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
    public class ApplicationManager
    {
       public long AddApplication(ApplicationObject importApplication)
       {
           try
           {
               if(importApplication == null)
               {
                   return -2;
               }
               
               var importApplicationEntity = ModelMapper.Map<ApplicationObject, Application>(importApplication);
               if (importApplicationEntity == null || importApplicationEntity.ImporterId < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.Applications.Add(importApplicationEntity);
                   db.SaveChanges();
                   long pId = 0;
                    if (importApplication.PermitId > 0)
                    {
                        var permitApp = new PermitApplication
                        {
                            PermitId = importApplication.PermitId,
                            ApplicationId = returnStatus.Id
                        };
                        var pAppStatus = db.PermitApplications.Add(permitApp);
                        db.SaveChanges();
                        if (pAppStatus.Id < 1)
                        {
                            var pApp = db.Applications.Find(returnStatus.Id);
                            if (pApp == null || pApp.Id < 1)
                            {
                                return -2;
                            }
                            db.Applications.Remove(pApp);
                            db.SaveChanges();
                            return -2;
                        }

                        pId = pAppStatus.Id;
                    }

                    var itemStatus = AddApplicationItems(importApplication.ApplicationItemObjects.ToList(), returnStatus.Id);
                    if (itemStatus < 1)
                    {
                        var app = db.PermitApplications.Find(pId);
                        if (app != null && app.Id > 0)
                        {
                            db.PermitApplications.Remove(app);
                            db.SaveChanges();
                        }

                        var pApp = db.Applications.Find(returnStatus.Id);
                        db.Applications.Remove(pApp);
                        db.SaveChanges();
                        return -2;
                    }
                   return returnStatus.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return -2;
           }
       }

       private long AddApplicationItems(List<ApplicationItemObject> importItems, long applicationId)
       {
           try
           {
               if (!importItems.Any())
               {
                   return -2;
               }

               var successCount = 0;
               using (var db = new ImportPermitEntities())
               {
                   importItems.ForEach(importItem =>
                   {
                       importItem.ApplicationId = applicationId;
                       var importItemEntity = ModelMapper.Map<ApplicationItemObject, ApplicationItem>(importItem);
                       if (importItemEntity != null && importItemEntity.ApplicationId > 0)
                       {
                           var item = db.ApplicationItems.Add(importItemEntity);
                           db.SaveChanges();
                           importItem.ThroughPutObjects.ToList().ForEach(th =>
                           {
                               var throughPut = new ThroughPut
                               {
                                   Id = 0,
                                   ApplicationItemId = item.Id,
                                   DepotId = th.DepotId,
                                   ProductId = th.ProductId,
                                   Quantity = 0,
                                   Comment = "",
                                   IPAddress = ""
                               };

                               db.ThroughPuts.Add(throughPut);
                               db.SaveChanges();
                           });


                           importItem.ApplicationCountryObjects.ToList().ForEach(ct =>
                           {
                               var country = new ApplicationCountry
                               {
                                   Id = 0,
                                   ApplicationItemId = item.Id,
                                   CountryId = ct.CountryId
                               };

                               db.ApplicationCountries.Add(country);
                               db.SaveChanges();
                           });

                           importItem.ProductBankerObjects.ToList().ForEach(b =>
                           {
                               var banker = new ProductBanker
                               {
                                   Id = 0,
                                   ApplicationItemId = item.Id,
                                   BankAccountNumber = b.BankAccountNumber,
                                   BankId = b.BankId
                               };

                               db.ProductBankers.Add(banker);
                               db.SaveChanges();
                           });

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

       public long UpdateApplicationItems(List<ApplicationItemObject> importItems, long applicationId, out  List<DocumentTypeObject> docList)
       {
           try
           {
               if (!importItems.Any())
               {
                   docList = new List<DocumentTypeObject>();
                   return -2;
               }

               var successCount = 0;
               using (var db = new ImportPermitEntities())
               {
                   var existingItems = db.ApplicationItems.Where(a => a.ApplicationId == applicationId).Include("ThroughPuts").Include("ProductBankers").ToList();
                   if (!existingItems.Any())
                   {
                       docList = new List<DocumentTypeObject>();
                       return -2;
                   }
                   var applications = db.Applications.Where(s => s.Id == applicationId).Include("Invoice").ToList();
                   if (!applications.Any())
                   {
                       docList = new List<DocumentTypeObject>();
                       return -2;
                   }
                   var throughPuts = new List<ThroughPut>();
                   var productBankers = new List<ProductBanker>();
                   var newThroughPuts = new List<ThroughPutObject>();
                   var newProductBankers = new List<ProductBankerObject>();

                   existingItems.ForEach(i =>
                   {
                       throughPuts.AddRange(i.ThroughPuts);
                       productBankers.AddRange(i.ProductBankers);
                   });
                   
                   if (!throughPuts.Any() || !productBankers.Any())
                   {
                       docList = new List<DocumentTypeObject>();
                       return -2;
                   }

                   importItems.ForEach(importItem =>
                   {
                       newThroughPuts.AddRange(importItem.ThroughPutObjects);
                       newProductBankers.AddRange(importItem.ProductBankerObjects);

                        importItem.ThroughPutObjects.ToList().ForEach(th =>
                        {
                            var existingThroughPut = throughPuts.Find(t => t.DepotId == th.DepotId);
                            if (existingThroughPut == null || existingThroughPut.Id < 1)
                            {
                                var throughPut = new ThroughPut
                                {
                                    ApplicationItemId = th.ApplicationItemId,
                                    DepotId = th.DepotId,
                                    ProductId = th.ProductId,
                                    Quantity = 0,
                                    Comment = "",
                                    IPAddress = ""
                                };

                                db.ThroughPuts.Add(throughPut);
                                db.SaveChanges();
                            }
                        });

                        importItem.ProductBankerObjects.ToList().ForEach(b =>
                        {
                            var existingBanker = productBankers.Find(t => t.BankId == b.BankId);
                            if (existingBanker == null || existingBanker.Id < 1)
                            {
                                var banker = new ProductBanker
                                {
                                    Id = 0,
                                    ApplicationItemId = b.ApplicationItemId,
                                    BankAccountNumber = b.BankAccountNumber,
                                    BankId = b.BankId
                                };

                                db.ProductBankers.Add(banker);
                                db.SaveChanges();
                            }
                        });

                        successCount += 1;
                       
                   });

                   if (successCount != importItems.Count)
                   {
                       docList = new List<DocumentTypeObject>();
                       return -2;
                   }
                   //applications
                   docList = new List<DocumentTypeObject>();
                   var tempList = new List<DocumentTypeObject>();
                   throughPuts.ForEach(i =>
                   {
                       if (!newThroughPuts.Exists(t => t.DepotId == i.DepotId))
                        {
                            var thr = db.ThroughPuts.Where(d => d.DepotId == i.DepotId && d.ApplicationItemId == i.ApplicationItemId).Include("Document").ToList();
                            if (thr.Any())
                            {
                                var docTypeId = thr[0].Document.DocumentTypeId;
                                var docTypes = db.DocumentTypes.Where(d => d.DocumentTypeId == docTypeId).ToList();
                                if (docTypes.Any())
                                {
                                    var eth = thr[0];
                                    var doc = thr[0].Document;

                                    db.ThroughPuts.Remove(eth);
                                    db.SaveChanges();

                                    db.Documents.Remove(doc);
                                    db.SaveChanges();

                                    tempList.Add(new DocumentTypeObject
                                    {
                                        Name = docTypes[0].Name + "_" + applications[0].Invoice.RRR,
                                        DocumentPath = thr[0].Document.DocumentPath
                                    });
                                }
                                
                            }
                        }
                      
                   });

                   productBankers.ForEach(i =>
                   {
                       if (!newProductBankers.Exists(t => t.BankId == i.BankId))
                       {
                           var thr = db.ProductBankers.Where(d => d.BankId == i.BankId && d.ApplicationItemId == i.ApplicationItemId).Include("Document").ToList();
                          
                           if (thr.Any())
                           {
                               var docTypeId = thr[0].Document.DocumentTypeId;
                               var docTypes = db.DocumentTypes.Where(d => d.DocumentTypeId == docTypeId).ToList();
                               if (docTypes.Any())
                               {
                                   var eth = thr[0];
                                   var doc = thr[0].Document;

                                   db.ProductBankers.Remove(eth);
                                   db.SaveChanges();

                                   db.Documents.Remove(doc);
                                   db.SaveChanges();

                                   tempList.Add(new DocumentTypeObject
                                   {
                                       Name = docTypes[0].Name + "_" + applications[0].Invoice.RRR,
                                       DocumentPath = thr[0].Document.DocumentPath
                                   });
                               }

                           }
                       }
                   });

                   if (tempList.Any())
                   {
                       docList = tempList;
                   }
                   return successCount;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               docList = new List<DocumentTypeObject>();
               return 0;
           }
       }

       public long UpdateApplication(ApplicationObject importApplication)
       {
           try
           {
               if (importApplication == null)
               {
                   return -2;
               }

               var importApplicationEntity = ModelMapper.Map<ApplicationObject, Application>(importApplication);
               if (importApplicationEntity == null || importApplicationEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.Applications.Attach(importApplicationEntity);
                   db.Entry(importApplicationEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   var invStatus = UpdateInvoice(importApplication.InvoiceId, importApplication.ReferenceCode);
                   if (invStatus < 1)
                   {
                       return -2;
                   }
                   return importApplication.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateAppPaymentOption(int paymentTypeId, long applicationId)
       {
           try
           {
               if (paymentTypeId < 1 || applicationId < 1)
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {

                   var apps = db.Applications.Where(i => i.Id == applicationId).Include("Invoice").ToList();
                   if (!apps.Any())
                   {
                       return -2;
                   }

                   var appEntity = apps[0];
                   long invoiceId;
                   var res = long.TryParse(appEntity.Invoice.ReferenceCode, out invoiceId);
                   if (!res || invoiceId < 1)
                   {
                       return -2;
                   }
                   using (var db2 = new PPIPSPaymentEntities())
                   {
                       var invoices = db2.TransactionInvoices.Where(i => i.Id == invoiceId).ToList();
                       if (!invoices.Any())
                       {
                           return -2;
                       }

                       var trInvoice = invoices[0];
                       trInvoice.PaymentMethod = Enum.GetName(typeof (PaymentType), paymentTypeId);
                       db2.Entry(trInvoice).State = EntityState.Modified;
                       db2.SaveChanges();
                   }

                   appEntity.Invoice.PaymentTypeId = paymentTypeId;
                   db.Entry(appEntity.Invoice).State = EntityState.Modified;
                   db.SaveChanges();
                   return appEntity.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public long UpdateInvoice(long invoiceId, string referenceCode)
       {
           try
           {
               if (string.IsNullOrEmpty(referenceCode) || invoiceId < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   var invoices = db.Invoices.Where(h => h.Id == invoiceId).ToList();
                   if (!invoices.Any())
                   {
                       return -2;
                   }

                   invoices[0].ReferenceCode = referenceCode;
                   db.Entry(invoices[0]).State = EntityState.Modified;
                   db.SaveChanges();
                   return invoices[0].Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public List<ApplicationObject> GetApplications()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications = db.Applications.OrderByDescending(i => i.DateApplied).Include("Invoice").ToList();
                   if (!myApplications.Any())
                   {
                        return new List<ApplicationObject>();
                   }
                   var objList =  new List<ApplicationObject>();
                   myApplications.ForEach(app =>
                   {
                       var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                       if (importObject != null && importObject.Id > 0)
                       {
                           importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                           importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                           importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                           var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                           if (name != null)
                               importObject.StatusStr = name.Replace("_", " ");
                          importObject.ReferenceCode = app.Invoice.RRR;
                           objList.Add(importObject);
                       }
                   });

                   return !objList.Any() ? new List<ApplicationObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

       public List<ApplicationItemObject> GetDepotAssignedApplicationItems(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;
                   const int paid = (int)AppStatus.Paid;
                   
                   using (var db = new ImportPermitEntities())
                   {
                       var applicationItems = (from appObj in db.ApplicationItems.Where(k => k.Application.ApplicationStatusCode >= paid)
                                             .Include("Product")
                                             .Include("Application")
                                             .Include("ApplicationCountries").OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                                              join th in db.ThroughPuts.Where(l => l.DocumentId == null)
                                              on appObj.Id equals th.ApplicationItemId
                                              join dp in db.Depots.Where(d => d.ImporterId == importerId)
                                              on th.DepotId equals dp.Id
                                             select appObj).ToList();   

                       if (!applicationItems.Any())
                       {
                           countG = 0;
                          return new List<ApplicationItemObject>();
                       }

                       var newList = new List<ApplicationItemObject>();
                       countG = (from appObj in db.ApplicationItems.Where(k => k.Application.ApplicationStatusCode >= paid)
                                 join th in db.ThroughPuts.Where(l => l.DocumentId == null)
                                 on appObj.Id equals th.ApplicationItemId
                                 join dp in db.Depots.Where(d => d.ImporterId == importerId)
                                 on th.DepotId equals dp.Id
                                 select appObj).Count();
                       
                        applicationItems.ForEach(app =>
                        {
                            var importObject = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(app);
                            if (importObject != null && importObject.Id > 0)
                            {
                                importObject.CountryOfOriginName = "";
                                app.ApplicationCountries.ToList().ForEach(c =>
                                {
                                    var countries = db.Countries.Where(k => k.Id == c.CountryId).ToList();
                                    if (countries.Any())
                                    {
                                        if (string.IsNullOrEmpty(importObject.CountryOfOriginName))
                                        {
                                            importObject.CountryOfOriginName = countries[0].Name;
                                        }
                                        else
                                        {
                                            importObject.CountryOfOriginName += ", " + countries[0].Name;
                                        }
                                    }
                                });

                                importObject.ProductName = app.Product.Name;
                                var importers = db.Importers.Where(a => app.Application.ImporterId == a.Id).ToList();
                                if (!importers.Any())
                                {
                                    return;
                                }
                                if (app.ThroughPuts.Any())
                                {
                                    var dd = app.ThroughPuts.ToList()[0];
                                    var docs = db.Documents.Where(d => d.DocumentId == dd.DocumentId).ToList();
                                    if (!docs.Any())
                                    {
                                        return;
                                    }
                                     
                                    var name = Enum.GetName(typeof (AppStatus), docs[0].Status);
                                    if (name != null)importObject.StatusStr = name.Replace("_", " ");
                                }
                                else
                                {
                                    importObject.StatusStr = AppStatus.Not_Available.ToString().Replace("_", " ");
                                }
                                importObject.ImporterName = importers[0].Name;
                                importObject.EstimatedQuantityStr = importObject.EstimatedQuantity.ToString("n1").Replace(".0", "");
                                importObject.EstimatedValueStr = importObject.EstimatedValue.ToString("n1").Replace(".0", "");
                                newList.Add(importObject);
                            }

                        });
                       return newList.OrderByDescending(k => k.Id).ToList();

                   }

               }
               countG = 0;
               return new List<ApplicationItemObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationItemObject>();
           }
       }

       public List<ApplicationItemObject> GetDepotAssignedApplicationHistory(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;
                   const int paid = (int)AppStatus.Paid;
                   const int pending = (int)AppStatus.Pending;

                   using (var db = new ImportPermitEntities())
                   {
                       var applicationItems = (from appObj in db.ApplicationItems.Where(k => k.Application.ApplicationStatusCode >= paid)
                                             .Include("Product")
                                             .Include("Application")
                                             .Include("ApplicationCountries").OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                                               join th in db.ThroughPuts.Where(l => l.DocumentId != null && l.DocumentId > 0)
                                               on appObj.Id equals th.ApplicationItemId
                                               join dp in db.Depots.Where(d => d.ImporterId == importerId)
                                               on th.DepotId equals dp.Id
                                               select appObj).ToList();

                       if (!applicationItems.Any())
                       {
                           countG = 0;
                           return new List<ApplicationItemObject>();
                       }

                       var newList = new List<ApplicationItemObject>();
                       countG = (from appObj in db.ApplicationItems.Where(k => k.Application.ApplicationStatusCode >= paid)
                                 join th in db.ThroughPuts.Where(l => l.DocumentId != null && l.DocumentId > 0)
                                 on appObj.Id equals th.ApplicationItemId
                                 join dp in db.Depots.Where(d => d.ImporterId == importerId)
                                 on th.DepotId equals dp.Id
                                 select appObj).Count();

                       applicationItems.ForEach(app =>
                       {
                           var importObject = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               importObject.CountryOfOriginName = "";
                               app.ApplicationCountries.ToList().ForEach(c =>
                               {
                                   var countries = db.Countries.Where(k => k.Id == c.CountryId).ToList();
                                   if (countries.Any())
                                   {
                                       if (string.IsNullOrEmpty(importObject.CountryOfOriginName))
                                       {
                                           importObject.CountryOfOriginName = countries[0].Name;
                                       }
                                       else
                                       {
                                           importObject.CountryOfOriginName += ", " + countries[0].Name;
                                       }
                                   }
                               });

                               importObject.ProductName = app.Product.Name;
                               var importers = db.Importers.Where(a => app.Application.ImporterId == a.Id).ToList();
                               if (!importers.Any())
                               {
                                   return;
                               }

                               if (app.ThroughPuts.Any())
                               {
                                   var dd = app.ThroughPuts.ToList()[0];
                                   var docs = db.Documents.Where(d => d.DocumentId == dd.DocumentId).ToList();
                                   if (!docs.Any())
                                   {
                                       return;
                                   }

                                   var name = Enum.GetName(typeof(AppStatus), docs[0].Status);
                                   if (name != null) importObject.StatusStr = name.Replace("_", " ");
                               }
                               else
                               {
                                   importObject.StatusStr = AppStatus.Not_Available.ToString().Replace("_", " ");
                               }
                               importObject.ImporterName = importers[0].Name;
                               importObject.EstimatedQuantityStr = importObject.EstimatedQuantity.ToString("n1").Replace(".0", "");
                               importObject.EstimatedValueStr = importObject.EstimatedValue.ToString("n1").Replace(".0", "");
                               newList.Add(importObject);
                           }

                       });
                       return newList.OrderByDescending(k => k.Id).ToList();

                   }

               }
               countG = 0;
               return new List<ApplicationItemObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationItemObject>();
           }
       }

       public List<ApplicationObject> GetApplications(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var myApplications =
                           db.Applications.Where(m => m.ImporterId == importerId)
                           .OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                               .Include("ApplicationItems")
                               .Include("Invoice")
                                .Include("Importer")
                               .ToList();

                       if (myApplications.Any())
                       {
                           var newList = new List<ApplicationObject>();
                           myApplications.ForEach(app =>
                           {
                               var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                               if (importObject != null && importObject.Id > 0)
                               {
                                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                                   if (name != null)
                                       importObject.StatusStr = name.Replace("_", " ");
                                   importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                                   importObject.ReferenceCode = app.Invoice.RRR;
                                   importObject.ImporterStr = app.Importer.Name;
                                   newList.Add(importObject);
                               }
                           });

                           countG = db.Applications.Count(p => p.ImporterId == importerId);
                           return newList.OrderByDescending(k => k.Id).ToList();
                       }
                   }
                   
               }
               countG = 0;
               return new List<ApplicationObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationObject>();
           }
       }
       
       public List<AppCountObject> GetAdminCounts()
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int submitted = (int)AppStatus.Submitted;
                   const int paid = (int)AppStatus.Paid;
                   const int pending = (int)AppStatus.Pending;
                   const int approved = (int)AppStatus.Approved;
                   const int processing = (int)AppStatus.Processing;
                   const int rejected = (int)AppStatus.Rejected;

                   const int submittedNtf = (int)NotificationStatusEnum.Submitted;
                   const int paidNtf = (int)NotificationStatusEnum.Paid;
                   const int pendingNtf = (int)NotificationStatusEnum.Pending;
                   const int approvedNtf = (int)NotificationStatusEnum.Approved;
                   const int processingNtf = (int)NotificationStatusEnum.Processing;
                   const int rejectedNtf = (int)NotificationStatusEnum.Rejected;

                   const int approvedRct = (int)RecertificationStatusEnum.Approved;
                   const int processingRct = (int)RecertificationStatusEnum.Processing;
                   const int rejectedRct = (int)RecertificationStatusEnum.Rejected;

                   


                   var totalAppCount = db.Applications.Count(a => a.ApplicationStatusCode > pending);
                   var paidAppCount = db.Applications.Count(m => m.ApplicationStatusCode == paid);
                   var submittedAppCount = db.Applications.Count(m => m.ApplicationStatusCode == submitted);
                   var approvedAppCount = db.Applications.Count(m => m.ApplicationStatusCode == approved);
                   var processingAppCount = db.Applications.Count(m => m.ApplicationStatusCode == processing);
                   var rejectedAppCount = db.Applications.Count(m => m.ApplicationStatusCode == rejected);

                   var totalNotificationCount = db.Notifications.Count(a => a.Status > pendingNtf);
                   var paidNotificationCount = db.Notifications.Count(m => m.Status == paidNtf);
                   var submittedNotificationCount = db.Notifications.Count(m => m.Status == submittedNtf);
                   var approvedNotificationCount = db.Notifications.Count(m => m.Status == approvedNtf);
                   var processingNotificationCount = db.Notifications.Count(m => m.Status == processingNtf);
                   var rejectedNotificationCount = db.Notifications.Count(m => m.Status == rejectedNtf);

                   var totalRecertificationCount = db.Recertifications.Count(a => a.Status > pending);
                   const int paidRecertificationCount = 0;
                   const int submittedRecertificationCount = 0;
                   var approvedRecertificationCount = db.Recertifications.Count(m => m.Status == approvedRct);
                   var processingRecertificationCount = db.Recertifications.Count(m => m.Status == processingRct);
                   var rejectedRecertificationCount = db.Recertifications.Count(m => m.Status == rejectedRct);


                   var appList = new List<AppCountObject>();

                   var application = new AppCountObject
                   {
                       TotalCount = totalAppCount,
                       PaidAppCount = paidAppCount,
                       SubmittedAppCount = submittedAppCount,
                       ApprovedAppCount = approvedAppCount,
                       ProcessingAppCount = processingAppCount,
                       RejectedAppCount = rejectedAppCount,
                       TotalAppHref = "AdminApplications/Applications",
                       PaidAppHref = "AdminApplications/PendingSubmission",
                       SubmittedAppCountHref = "AdminApplications/SubmittedApplications",
                       ApprovedAppHref = "AdminApplications/ApprovedApplications",
                       ProcessingAppHref = "AdminApplications/ProcessingApplications",
                       RejectedHref = "AdminApplications/RejectedApplications",
                       ItemName = "Import Application"
                   };

                   appList.Add(application);

                   var notification = new AppCountObject
                   {
                       TotalCount = totalNotificationCount,
                       PaidAppCount = paidNotificationCount,
                       SubmittedAppCount = submittedNotificationCount,
                       ApprovedAppCount = approvedNotificationCount,
                       ProcessingAppCount = processingNotificationCount,
                       RejectedAppCount = rejectedNotificationCount,
                       TotalAppHref = "AdminNotifications/Notifications",
                       PaidAppHref = "AdminNotifications/PendingSubmission",
                       SubmittedAppCountHref = "AdminNotifications/SubmittedNotifications",
                       ApprovedAppHref = "AdminNotifications/ApprovedNotifications",
                       ProcessingAppHref = "AdminNotifications/ProcessingNotifications",
                       RejectedHref = "AdminNotifications/RejectedNotifications",
                       ItemName = "Vessel Arrival Notification"
                   };

                   appList.Add(notification);

                   var recertification = new AppCountObject
                   {
                       TotalCount = totalRecertificationCount,
                       PaidAppCount = paidRecertificationCount,
                       SubmittedAppCount = submittedRecertificationCount,
                       ApprovedAppCount = approvedRecertificationCount,
                       ProcessingAppCount = processingRecertificationCount,
                       RejectedAppCount = rejectedRecertificationCount,
                       TotalAppHref = "#",
                       PaidAppHref = "AdminRecertifications/PaidRecertifications",
                       SubmittedAppCountHref = "AdminRecertifications/SubmittedRecertifications",
                       ApprovedAppHref = "AdminRecertifications/ApprovedRecertifications",
                       ProcessingAppHref = "AdminRecertifications/ProcessingRecertifications",
                       RejectedHref = "AdminRecertifications/RejectedRecertifications",
                       ItemName = "Product Certification"
                   };

                   appList.Add(recertification);

                   return appList;
               }
           }
           catch (Exception ex)
           {
               return new List<AppCountObject>();
           }
       }

       public List<ApplicationObject> GetPaidApplications(int? itemsPerPage, int? pageNumber, out int countG)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;

                   const int paid = (int)AppStatus.Paid;

                   using (var db = new ImportPermitEntities())
                   {
                       var myApplications =
                           db.Applications.Where(m => m.ApplicationStatusCode == paid)
                           .OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                               .Include("ApplicationItems")
                               .Include("Invoice")
                               .Include("Importer")
                               .ToList();

                       if (myApplications.Any())
                       {
                           var newList = new List<ApplicationObject>();
                           myApplications.ForEach(app =>
                           {
                               var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                               if (importObject != null && importObject.Id > 0)
                               {
                                   importObject.ImporterStr = app.Importer.Name;
                                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                                   if (name != null)
                                       importObject.StatusStr = name.Replace("_", " ");
                                   importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                                   importObject.ReferenceCode = app.Invoice.RRR;
                                   newList.Add(importObject);
                               }
                           });

                           countG = db.Applications.Count(m => m.ApplicationStatusCode == paid);
                           return newList.OrderByDescending(k => k.Id).ToList();
                       }
                   }

               }
               countG = 0;
               return new List<ApplicationObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationObject>();
           }
       }

        public List<ApplicationObject> GetApplicationsPendingSubmission(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    const int paid = (int)AppStatus.Paid;

                    using (var db = new ImportPermitEntities())
                    {
                        var myApplications =
                            db.Applications.Where(m => m.ApplicationStatusCode == paid)
                            .OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                                .Include("ApplicationItems")
                                .Include("Invoice")
                                .Include("Importer")
                                .ToList();

                        if (myApplications.Any())
                        {
                            var newList = new List<ApplicationObject>();
                            myApplications.ForEach(app =>
                            {
                                var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                                if (importObject != null && importObject.Id > 0)
                                {
                                    importObject.ImporterStr = app.Importer.Name;
                                    importObject.StatusStr = "Pending Submission";
                                    importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                                    importObject.DerivedValueStr = app.Invoice.AmountPaid.ToString("n1").Replace(".0", "");
                                    importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                                    importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                                    importObject.ReferenceCode = app.Invoice.RRR;
                                    newList.Add(importObject);
                                }
                            });

                           
                            countG = db.Applications.Count(m => m.ApplicationStatusCode == paid);
                            return newList.OrderByDescending(k => k.Id).ToList();
                        }
                    }

                }
                countG = 0;
                return new List<ApplicationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<ApplicationObject>();
            }
        }

        public List<ApplicationObject> GetSubmittedApplications(int? itemsPerPage, int? pageNumber, out int countG)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;

                   const int submitted = (int)AppStatus.Submitted;

                   using (var db = new ImportPermitEntities())
                   {
                       var myApplications =
                           db.Applications.Where(m => m.ApplicationStatusCode == submitted)
                           .OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                               .Include("ApplicationItems")
                               .Include("Invoice")
                                 .Include("Importer")
                               .ToList();

                       if (myApplications.Any())
                       {
                           var newList = new List<ApplicationObject>();
                           myApplications.ForEach(app =>
                           {
                               var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                               if (importObject != null && importObject.Id > 0)
                               {
                                   importObject.ImporterStr = app.Importer.Name;
                                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                                   if (name != null)
                                       importObject.StatusStr = name.Replace("_", " ");
                                   importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                                   importObject.ReferenceCode = app.Invoice.RRR;
                                   newList.Add(importObject);
                               }
                           });

                           countG = db.Applications.Count(m => m.ApplicationStatusCode == submitted);
                           return newList.OrderByDescending(k => k.Id).ToList();
                       }
                   }

               }
               countG = 0;
               return new List<ApplicationObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationObject> GetProcessingApplications(int? itemsPerPage, int? pageNumber, out int countG)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;

                   const int processing = (int)AppStatus.Processing;

                   using (var db = new ImportPermitEntities())
                   {
                       var myApplications =
                           db.Applications.Where(m => m.ApplicationStatusCode == processing)
                           .OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                               .Include("ApplicationItems")
                               .Include("Invoice")
                                 .Include("Importer")
                               .ToList();

                       if (myApplications.Any())
                       {
                           var newList = new List<ApplicationObject>();
                           myApplications.ForEach(app =>
                           {
                               var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                               if (importObject != null && importObject.Id > 0)
                               {
                                   importObject.ImporterStr = app.Importer.Name;
                                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                                   if (name != null)
                                       importObject.StatusStr = name.Replace("_", " ");
                                   importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                                   importObject.ReferenceCode = app.Invoice.RRR;
                                   newList.Add(importObject);
                               }
                           });

                           countG = db.Applications.Count(m => m.ApplicationStatusCode == processing);
                           return newList.OrderByDescending(k => k.Id).ToList();
                       }
                   }

               }
               countG = 0;
               return new List<ApplicationObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationObject> GetApplicationsInVerification(int? itemsPerPage, int? pageNumber, out int countG)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;

                   const int verifying = (int)AppStatus.Verifying;

                   using (var db = new ImportPermitEntities())
                   {
                       var myApplications =
                           db.Applications.Where(m => m.ApplicationStatusCode == verifying)
                           .OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                               .Include("ApplicationItems")
                               .Include("SignOffDocuments")
                               .Include("Invoice")
                                .Include("Importer")
                               .ToList();

                       if (myApplications.Any())
                       {
                           var newList = new List<ApplicationObject>();
                           myApplications.ForEach(app =>
                           {
                               var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                               if (importObject != null && importObject.Id > 0)
                               {
                                   importObject.IsSignOffDocUploaded = app.SignOffDocuments.Any(); 
                                   importObject.ImporterStr = app.Importer.Name;
                                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                                   if (name != null)importObject.StatusStr = name.Replace("_", " ");
                                   importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                                   importObject.ReferenceCode = app.Invoice.RRR;
                                   newList.Add(importObject);
                               }
                           });

                           countG = db.Applications.Count(m => m.ApplicationStatusCode == verifying);
                           return newList.OrderByDescending(k => k.Id).ToList();
                       }
                   }

               }
               countG = 0;
               return new List<ApplicationObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationObject> GetApprovedApplications(int? itemsPerPage, int? pageNumber, out int countG)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;

                   const int approved = (int)AppStatus.Approved;

                   using (var db = new ImportPermitEntities())
                   {
                       var myApplications =
                           db.Applications.Where(m => m.ApplicationStatusCode == approved)
                           .OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                               .Include("ApplicationItems")
                               .Include("Invoice")
                                .Include("Importer")
                               .ToList();

                       if (myApplications.Any())
                       {
                           var newList = new List<ApplicationObject>();
                           myApplications.ForEach(app =>
                           {
                               var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                               if (importObject != null && importObject.Id > 0)
                               {
                                   importObject.ImporterStr = app.Importer.Name;
                                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                                   if (name != null)
                                       importObject.StatusStr = name.Replace("_", " ");
                                   importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                                   importObject.ReferenceCode = app.Invoice.RRR;
                                   newList.Add(importObject);
                               }
                           });

                           countG = db.Applications.Count(m => m.ApplicationStatusCode == approved);
                           return newList.OrderByDescending(k => k.Id).ToList();
                       }
                   }

               }
               countG = 0;
               return new List<ApplicationObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationObject>();
           }
       }

       public AppInfoObject GetApplicationEmployees(long applicationId)
       {
           try
           {
                   const int approved = (int)AppStatus.Verifying;

                   using (var db = new ImportPermitEntities())
                   {
                       var applicationEmployees = (from ap in db.Applications.Where(m => m.ApplicationStatusCode == approved && m.Id == applicationId).Include("Importer").Include("ApplicationItems").Include("Invoice")
                                            join phs in db.ProcessingHistories.Include("Step") on ap.Id equals phs.ApplicationId
                                            join emd in db.EmployeeDesks.Include("UserProfile") on phs.EmployeeId equals emd.Id
                                            join ps in db.People on emd.UserProfile.PersonId equals  ps.Id
                                             select new UserInfoObject
                                             {
                                                 CompanyName = ap.Importer.Name,
                                                 Rrr = ap.Invoice.RRR,
                                                 ActionPerformed = phs.Step.Name,
                                                 Name = ps.LastName + " " + ps.FirstName,
                                                 StartDate = phs.AssignedTime,
                                                 CompletionDate = phs.FinishedTime
                                             }).ToList();
                        
                       if (!applicationEmployees.Any())
                       {
                           return new AppInfoObject();
                       }

                       var appItems = db.ApplicationItems.Where(i => i.ApplicationId == applicationId).ToList();
                       if (!appItems.Any())
                       {
                           return new AppInfoObject();
                       }
                       
                       applicationEmployees.ForEach(app =>
                       {
                           app.StartDateStr = app.StartDate != null && app.StartDate.Value.Year > 1 ? ((DateTime)app.StartDate).ToString("dd/MM/yyyy") : "Not Available";
                           app.CompletionDateStr = app.CompletionDate != null && app.CompletionDate.Value.Year > 1 ? ((DateTime)app.CompletionDate).ToString("dd/MM/yyyy") : "Not Available";

                       });

                       var appInfo = new AppInfoObject
                       {
                           CompanyName = applicationEmployees[0].CompanyName,
                           Rrr = applicationEmployees[0].Rrr,
                           ApplicationItemObjects = new List<ApplicationItemObject>(),
                           UserInfoObjects = new List<UserInfoObject>()
                       };

                       appInfo.UserInfoObjects = applicationEmployees;

                       appItems.ToList().ForEach(u =>
                       {
                           var im = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(u);
                           if (im != null && im.Id > 0)
                           {
                               im.ProductObject = (from pr in db.Products.Where(x => x.ProductId == im.ProductId)
                                                   select new ProductObject
                                                   {
                                                       ProductId = pr.ProductId,
                                                       Code = pr.Code,
                                                       Name = pr.Name
                                                   }).ToList()[0];

                               var bankers = db.ProductBankers.Where(i => i.ApplicationItemId == u.Id).Include("Bank").ToList();
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
                                   im.ThroughPutObjects = new List<ThroughPutObject>();
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

                                           im.ThroughPutObjects.Add(new ThroughPutObject
                                           {
                                               ProductName = im.ProductObject.Name,
                                               ProductCode = im.ProductObject.Code,
                                               Quantity = d.Quantity,
                                               DepotName = d.Depot.Name
                                           });

                                       });
                                   }
                               }

                               im.ProductBankerObjects = new List<ProductBankerObject>();
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
                                   var pdBnk = new ProductBankerObject
                                   {
                                     BankName = string.IsNullOrEmpty(c.BankAccountNumber) ? c.Bank.Name : c.Bank.Name + "(" + c.BankAccountNumber + ")"
                                   };
                                   im.ProductBankerObjects.Add(pdBnk);
                               });

                               appInfo.ApplicationItemObjects.Add(im);
                           }
                       });
                       appInfo.Code = 5;
                       return appInfo;
                   }
           }
           catch (Exception ex)
           {
               return new AppInfoObject();
           }
       }

       public List<ApplicationObject> GetRejectedApplications(int? itemsPerPage, int? pageNumber, out int countG)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;

                   const int rejected = (int)AppStatus.Rejected;

                   using (var db = new ImportPermitEntities())
                   {
                       var myApplications =
                           db.Applications.Where(m => m.ApplicationStatusCode == rejected)
                           .OrderByDescending(m => m.Id).Skip(tpageNumber).Take(tsize)
                               .Include("ApplicationItems")
                               .Include("Invoice")
                                .Include("Importer")
                               .ToList();

                       if (myApplications.Any())
                       {
                           var newList = new List<ApplicationObject>();
                           myApplications.ForEach(app =>
                           {
                               var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                               if (importObject != null && importObject.Id > 0)
                               {
                                   importObject.ImporterStr = app.Importer.Name;
                                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                                   importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                                   importObject.ReferenceCode = app.Invoice.RRR;
                                   newList.Add(importObject);
                               }
                           });

                           countG = db.Applications.Count(m => m.ApplicationStatusCode == rejected);
                           return newList.OrderByDescending(k => k.Id).ToList();
                       }
                   }

               }
               countG = 0;
               return new List<ApplicationObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationObject>();
           }
       }
       
       public List<ApplicationObject> SearchPaidApplications(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int paid = (int)AppStatus.Paid;

                   var myApplications =
                       db.Applications.Where(m => m.ApplicationStatusCode == paid && (m.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || m.Invoice.ReferenceCode.Contains(searchCriteria.Trim()) || m.Importer.Name.Contains(searchCriteria.Trim())))
                        .Include("Invoice")
                         .Include("Importer")
                        .ToList();

                   if (myApplications.Any())
                   {
                       var newList = new List<ApplicationObject>();
                       myApplications.OrderByDescending(g => g.DateApplied).ToList().ForEach(app =>
                       {
                           var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               importObject.ImporterStr = app.Importer.Name;
                               importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                               importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                               importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                               var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                               if (name != null)
                                   importObject.StatusStr = name.Replace("_", " ");
                               importObject.ReferenceCode = app.Invoice.RRR;
                               newList.Add(importObject);
                           }
                       });

                       return newList.OrderByDescending(k => k.Id).ToList();
                   }
               }
               return new List<ApplicationObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationObject>();
           }
       }

        public List<ApplicationObject> SearchApplicationsPendingSubmission(string searchCriteria)
        {
           
           try
            {
                using (var db = new ImportPermitEntities())
                {
                    const int paid = (int)AppStatus.Paid;

                    var myApplications =
                        db.Applications.Where(m => m.ApplicationStatusCode == paid && (m.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || m.Invoice.ReferenceCode.Contains(searchCriteria.Trim()) || m.Importer.Name.Contains(searchCriteria.Trim())))
                         .Include("Invoice")
                          .Include("Importer")
                         .ToList();

                    if (myApplications.Any())
                    {
                        var newList = new List<ApplicationObject>();
                        myApplications.OrderByDescending(g => g.DateApplied).ToList().ForEach(app =>
                        {
                            var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                            if (importObject != null && importObject.Id > 0)
                            {
                                importObject.ImporterStr = app.Importer.Name;
                                importObject.StatusStr = "Pending Submission";
                                importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                                importObject.DerivedValueStr = app.Invoice.AmountPaid.ToString("n1").Replace(".0", "");
                                importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                                importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                                var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                                if (name != null)
                                    importObject.StatusStr = name.Replace("_", " ");
                                importObject.ReferenceCode = app.Invoice.RRR;
                                newList.Add(importObject);
                            }
                        });

                        return newList.OrderByDescending(k => k.Id).ToList();
                    }
                }
                return new List<ApplicationObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ApplicationObject>();
            }
        }


        public List<ApplicationObject> SearchSubmittedApplications(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int submitted = (int)AppStatus.Submitted;

                   var myApplications =
                       db.Applications.Where(m => m.ApplicationStatusCode == submitted && (m.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || m.Invoice.ReferenceCode.Contains(searchCriteria.Trim()) || m.Importer.Name.Contains(searchCriteria.Trim())))
                        .Include("Invoice")
                         .Include("Importer")
                        .ToList();

                   if (myApplications.Any())
                   {
                       var newList = new List<ApplicationObject>();
                       myApplications.OrderByDescending(g => g.DateApplied).ToList().ForEach(app =>
                       {
                           var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               importObject.ImporterStr = app.Importer.Name;
                               importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                               importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                               importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                               var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                               if (name != null)
                                   importObject.StatusStr = name.Replace("_", " ");
                               importObject.ReferenceCode = app.Invoice.RRR;
                               newList.Add(importObject);
                           }
                       });

                       return newList.OrderByDescending(k => k.Id).ToList();
                   }
               }
               return new List<ApplicationObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationObject> SearchProcessingApplications(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int processing = (int)AppStatus.Processing;

                   var myApplications =
                       db.Applications.Where(m => m.ApplicationStatusCode == processing && (m.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || m.Invoice.ReferenceCode.Contains(searchCriteria.Trim()) || m.Importer.Name.Contains(searchCriteria.Trim())))
                        .Include("Invoice")
                         .Include("Importer")
                        .ToList();

                   if (myApplications.Any())
                   {
                       var newList = new List<ApplicationObject>();
                       myApplications.OrderByDescending(g => g.DateApplied).ToList().ForEach(app =>
                       {
                           var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               importObject.ImporterStr = app.Importer.Name;
                               importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                               importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                               importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                               var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                               if (name != null)
                                   importObject.StatusStr = name.Replace("_", " ");
                               importObject.ReferenceCode = app.Invoice.RRR;
                               newList.Add(importObject);
                           }
                       });

                       return newList.OrderByDescending(k => k.Id).ToList();
                   }
               }
               return new List<ApplicationObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationObject> SearchApprovedApplications(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int approved = (int)AppStatus.Approved;

                   var myApplications =
                       db.Applications.Where(m => m.ApplicationStatusCode == approved && (m.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || m.Invoice.ReferenceCode.Contains(searchCriteria.Trim()) || m.Importer.Name.Contains(searchCriteria.Trim())))
                        .Include("Invoice")
                         .Include("Importer")
                        .ToList();

                   if (myApplications.Any())
                   {
                       var newList = new List<ApplicationObject>();
                       myApplications.OrderByDescending(g => g.DateApplied).ToList().ForEach(app =>
                       {
                           var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               importObject.ImporterStr = app.Importer.Name;
                               importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                               importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                               importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                               var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                               if (name != null)
                                   importObject.StatusStr = name.Replace("_", " ");
                               importObject.ReferenceCode = app.Invoice.RRR;
                               newList.Add(importObject);
                           }
                       });

                       return newList.OrderByDescending(k => k.Id).ToList();
                   }
               }
               return new List<ApplicationObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationObject> SearchApplicationsInVerification(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int verifying = (int)AppStatus.Verifying;

                   var myApplications =
                       db.Applications.Where(m => m.ApplicationStatusCode == verifying && (m.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || m.Invoice.ReferenceCode.Contains(searchCriteria.Trim()) || m.Importer.Name.Contains(searchCriteria.Trim())))
                        .Include("Invoice")
                        .Include("SignOffDocuments")
                         .Include("Importer")
                        .ToList();

                   if (myApplications.Any())
                   {
                       var newList = new List<ApplicationObject>();
                       myApplications.OrderByDescending(g => g.DateApplied).ToList().ForEach(app =>
                       {
                           var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               importObject.ImporterStr = app.Importer.Name;
                               importObject.IsSignOffDocUploaded = app.SignOffDocuments.Any();
                               importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                               importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                               importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                               var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                               if (name != null)
                                   importObject.StatusStr = name.Replace("_", " ");
                               importObject.ReferenceCode = app.Invoice.RRR;
                               newList.Add(importObject);
                           }
                       });

                       return newList.OrderByDescending(k => k.Id).ToList();
                   }
               }
               return new List<ApplicationObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationObject> SearchRejectedApplications(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int rejected = (int)AppStatus.Rejected;

                   var myApplications =
                       db.Applications.Where(m => m.ApplicationStatusCode == rejected && (m.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || m.Invoice.ReferenceCode.Contains(searchCriteria.Trim()) || m.Importer.Name.Contains(searchCriteria.Trim())))
                        .Include("Invoice")
                         .Include("Importer")
                        .ToList();

                   if (myApplications.Any())
                   {
                       var newList = new List<ApplicationObject>();
                       myApplications.OrderByDescending(g => g.DateApplied).ToList().ForEach(app =>
                       {
                           var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               importObject.ImporterStr = app.Importer.Name;
                               importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                               importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                               importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                               var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                               if (name != null)
                                   importObject.StatusStr = name.Replace("_", " ");
                               importObject.ReferenceCode = app.Invoice.RRR;
                               newList.Add(importObject);
                           }
                       });

                       return newList.OrderByDescending(k => k.Id).ToList();
                   }
               }
               return new List<ApplicationObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationObject>();
           }
       }
       
       public ItemCountObject GetCounts(long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var expiryDate = DateTime.Today.AddDays(30);
                   var appCount = db.Applications.Count(m => m.ImporterId == importerId);
                   var notificationCount = db.Notifications.Count(m => m.ImporterId == importerId);
                   var recertCount = db.Recertifications.Count(m => m.Notification.ImporterId == importerId);
                   var expPermitCount = db.Permits.Where(m => m.ExpiryDate != null && m.ExpiryDate > DateTime.Today && m.ImporterId == importerId).ToList();

                   var count = 0;
                   if (expPermitCount.Any())
                   {
                       expPermitCount.ForEach(k =>
                       {
                           if (k.ExpiryDate != null && k.ExpiryDate >= DateTime.Today && k.ExpiryDate <= expiryDate)
                           {
                               count++;
                           }
                       });
                   }

                   return new ItemCountObject
                   {
                       ApplicationCount = appCount,
                       NotificationCount = notificationCount,
                       RecertificationCount = recertCount,
                       ExpiringPermitCount = count
                   };

               }
           }
           catch (Exception ex)
           {
               return new ItemCountObject();
           }
       }

       public ItemCountObject GetBankerCounts(long importerId, long userId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int paid = (int)AppStatus.Paid;
                   var appList = (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                                   join pb in db.ProductBankers.Where(x => x.DocumentId == null) on bnk.BankId equals pb.BankId
                                   join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                                   join imp in db.Applications.Where(k => k.ApplicationStatusCode >= paid) on ai.ApplicationId equals imp.Id
                                   select imp).ToList();
                   var newList = new List<Application>();
                   var appCount = 0;
                   if (appList.Any())
                   {
                        appList.ForEach(l =>
                        {
                            if (!newList.Exists(b => b.Id == l.Id))
                            {
                                newList.Add(l);
                                appCount++;
                            }
                        });
                   }

                   var notificationCount = (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                                            join pb in db.ProductBankers.Where(x => x.DocumentId == null) on bnk.BankId equals pb.BankId
                                            join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                                            join app in db.Applications on ai.ApplicationId equals app.Id
                                            join ptApp in db.PermitApplications on app.Id equals ptApp.ApplicationId
                                            join nt in db.Notifications.Where(k => k.Invoice.Status >= paid) on ptApp.PermitId equals nt.PermitId
                                            where (!nt.NotificationBankers.Any(b => b.BankId == bnk.BankId) || !nt.FormMDetails.Any(b => b.BankId == bnk.BankId)) && nt.ProductId == ai.ProductId
                                            select nt).Count();

                   const int rejected = (int)DocStatus.Invalid;
                   var tpCount = 0;
                   if (userId > 0)
                   {
                       tpCount = (db.Documents.Where(d => d.UploadedById == userId && d.Status == rejected)).Count();
                   }
                   else
                   {
                       tpCount = (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                                  join pb in db.ProductBankers.Where(x => x.DocumentId == null) on bnk.BankId equals pb.BankId
                                  join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                                  join app in db.Applications on ai.ApplicationId equals app.Id
                                  join appDoc in db.ApplicationDocuments on app.Id equals appDoc.ApplicationId
                                  join doc in db.Documents.Where(d => d.Status == rejected) on appDoc.DocumentId equals doc.DocumentId
                                  select doc).Count();
                   }

                   return new ItemCountObject
                   {
                       ApplicationCount = appCount,
                       NotificationCount = notificationCount,
                       ExpiringPermitCount = tpCount
                   };

               }
           }
           catch (Exception ex)
           {
               return new ItemCountObject();
           }
       }

       public ItemCountObject GetDepotOwnerCounts(long importerId, long userId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int paid = (int)AppStatus.Paid;
                   var appCount = (from appObj in db.ApplicationItems.Where(k => k.Application.ApplicationStatusCode >= paid)
                                   join th in db.ThroughPuts
                                   on appObj.Id equals th.ApplicationItemId
                                   join dp in db.Depots.Where(d => d.ImporterId == importerId)
                                   on th.DepotId equals dp.Id
                                   select appObj).Count();

                   const int rejected = (int)DocStatus.Invalid;
                   var tpCount = (db.Documents.Where(d => d.UploadedById == userId && d.Status == rejected)).Count();

                   return new ItemCountObject
                   {
                       ApplicationCount = appCount,
                       NotificationCount = tpCount,
                       ExpiringPermitCount = tpCount
                   };

               }
           }
           catch (Exception ex)
           {
               return new ItemCountObject();
           }
       }

       public ItemCountObject GetVerifierCounts()
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int verifying = (int)AppStatus.Verifying;
                   const int approved = (int)AppStatus.Approved;

                   var verifyingAppCount = db.Applications.Count(k => k.ApplicationStatusCode == verifying);
                   var approvedAppCount = db.Applications.Count(k => k.ApplicationStatusCode == approved);
                   
                   return new ItemCountObject
                   {
                       ApprovedApplicationCount = approvedAppCount,
                       VerificationApplicationCount = verifyingAppCount
                   };
                    
               }
           }
           catch (Exception ex)
           {
               return new ItemCountObject();
           }
       }

       public List<ApplicationObject> GetBankAssignedApplications(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;
                   const int paid = (int) AppStatus.Paid;
                   
                   using (var db = new ImportPermitEntities())
                   {
                       var myApplications = (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                                             join pb in db.ProductBankers.Where(x => x.DocumentId == null) on bnk.BankId equals pb.BankId
                                             join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                                             join imp in db.Applications.Where(k => k.ApplicationStatusCode >= paid)
                                             .Include("Importer")
                                            .Include("Invoice")
                                            .Include("ApplicationType")
                                            .OrderByDescending(m => m.Id)
                                            .Skip(tpageNumber).Take(tsize) on ai.ApplicationId equals imp.Id
                                            select imp).ToList();

                        if (myApplications.Any())
                        {
                            var newList = new List<ApplicationObject>();
                            myApplications.ForEach(app =>
                            {
                                var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                                if (importObject != null && importObject.Id > 0 && !newList.Exists(z => z.Id == app.Id))
                                {
                                    importObject.ImporterStr = app.Importer.Name;
                                    importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                                    importObject.AppTypeStr = app.ApplicationType.Name;
                                    importObject.ImportClassName = Enum.GetName(typeof(NotificationClassEnum), importObject.ClassificationId).Replace("_", " ");
                                    importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                                    var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                                    if (name != null) importObject.StatusStr = name.Replace("_", " ");
                                    importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                                    importObject.ReferenceCode = app.Invoice.RRR;
                                    importObject.ImporterStr = app.Importer.Name;
                                    importObject.ApplicationTypeName = app.ApplicationType.Name;
                                    newList.Add(importObject);
                                }
                            });

                           var  countg = 0;
                            var list = (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                                      join pb in db.ProductBankers.Where(x => x.DocumentId == null) on bnk.BankId equals pb.BankId
                                      join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                                      join imp in db.Applications.Where(k => k.ApplicationStatusCode >= paid)
                                      on ai.ApplicationId equals imp.Id
                                      select imp).ToList();

                            if (list.Any())
                            {
                                var countList = new List<Application>();
                                list.ForEach(h =>
                                {
                                    if (!countList.Exists(n => n.Id == h.Id))
                                    {
                                        countList.Add(h);
                                        countg++;
                                    }
                                });
                            }

                            countG = countg;
                            return newList.OrderByDescending(k => k.Id).ToList();
                        }
                   }

               }
               countG = 0;
               return new List<ApplicationObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationObject> GetBankJobHistory(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;
                   
                   using (var db = new ImportPermitEntities())
                   {
                       var myApplications =
                                        (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                                         join pb in db.ProductBankers.Where(x => x.DocumentId != null) on bnk.BankId equals pb.BankId
                                         join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                                         join imp in db.Applications.Include("Invoice").Include("Importer").OrderByDescending(m => m.Id)
                                          .Skip(tpageNumber).Take(tsize) on ai.ApplicationId equals imp.Id
                                           where imp.ApplicationDocuments.Any()
                                         select imp).ToList();

                       if (!myApplications.Any())
                       {
                           countG = 0;
                           return new List<ApplicationObject>();
                       }

                       var newList = new List<ApplicationObject>();
                       countG = (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                                 join pb in db.ProductBankers.Where(x => x.DocumentId != null) on bnk.BankId equals pb.BankId
                                 join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                                 join imp in db.Applications on ai.ApplicationId equals imp.Id
                                 where imp.ApplicationDocuments.Any()
                                 select imp).Count();
                       myApplications.ForEach(app =>
                       {
                           if (!newList.Exists(b => b.Id == app.Id))
                           {
                               var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                               if (importObject != null && importObject.Id > 0)
                               {
                                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                                   importObject.ImporterStr = app.Importer.Name;
                                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                                   if (name != null)importObject.StatusStr = name.Replace("_", " ");
                                   importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                                  importObject.ReferenceCode = app.Invoice.RRR;
                                   if (!newList.Exists(b => b.Id == importObject.Id)) newList.Add(importObject);
                               }
                           }
                       });
                       return newList.OrderByDescending(k => k.Id).ToList();
                   }

               }
               countG = 0;
               return new List<ApplicationObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationObject> GetBankUserJobHistory(int? itemsPerPage, int? pageNumber, out int countG, long userId)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var myApplications =
                                        (from dc in db.Documents.Where(d => d.UploadedById == userId)
                                         join appDoc in db.ApplicationDocuments on dc.DocumentId equals appDoc.DocumentId
                                         join imp in db.Applications.OrderByDescending(m => m.Id)
                                           .Skip(tpageNumber).Take(tsize).Include("Invoice").Include("Importer") on appDoc.ApplicationId equals imp.Id
                                         select imp).ToList();

                       if (!myApplications.Any())
                       {
                           countG = 0;
                           return new List<ApplicationObject>();
                       }

                       var newList = new List<ApplicationObject>();
                       var allAppsCount = (from dc in db.Documents.Where(d => d.UploadedById == userId)
                                           join appDoc in db.ApplicationDocuments on dc.DocumentId equals appDoc.DocumentId
                                           join imp in db.Applications.Take(1) on appDoc.ApplicationId equals imp.Id
                                           select imp).Count();

                       countG = allAppsCount;
                       myApplications.ForEach(app =>
                       {
                           if (!newList.Exists(b => b.Id == app.Id))
                           {
                               var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                               if (importObject != null && importObject.Id > 0)
                               {
                                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                                   importObject.ImporterStr = app.Importer.Name;
                                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                                   importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                                   if (name != null)importObject.StatusStr = name.Replace("_", " ");
                                  importObject.ReferenceCode = app.Invoice.RRR;
                                   if (!newList.Exists(b => b.Id == importObject.Id)) newList.Add(importObject);
                               }
                           }
                       });

                    
                       return newList.OrderByDescending(k => k.Id).ToList();
                   }

               }
               countG = 0;
               return new List<ApplicationObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationObject> GetApplications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var myApplications =
                            db.Applications
                            .OrderByDescending(m => m.Id)
                             .Include("Importer")
                            .Include("Invoice")
                            .Include("ApplicationItems")
                            .Include("ApplicationDocuments")
                            .Include("ProcessingHistories")
                                .Skip(tpageNumber).Take(tsize)
                                .ToList();
                        if (myApplications.Any())
                        {
                            var newList = new List<ApplicationObject>();
                            myApplications.ForEach(app =>
                            {
                                var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                                if (importObject != null && importObject.Id > 0)
                                {
                                    importObject.ImporterStr = app.Importer.Name;
                                    importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");

                                    importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                                    var name = Enum.GetName(typeof (AppStatus), importObject.ApplicationStatusCode);
                                    if (name != null) importObject.StatusStr = name.Replace("_", " ");
                                    importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                                    importObject.ReferenceCode = app.Invoice.RRR;
                                    importObject.ImporterStr = app.Importer.Name;
                                    importObject.ApplicationTypeName = app.ApplicationType.Name;

                                    newList.Add(importObject);
                                }
                            
                             });
                            countG = db.Applications.Count();
                            return newList.OrderByDescending(k => k.Id).ToList();
                        }
                    }

                }
                countG = 0;
                return new List<ApplicationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<ApplicationObject>();
            }
        }

       public List<ApplicationObject> GetAdminApplications(int? itemsPerPage, int? pageNumber, out int countG)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;
                   const int pending = (int)AppStatus.Pending;
                   

                   using (var db = new ImportPermitEntities())
                   {
                       var myApplications =
                           db.Applications.Where(j => j.ApplicationStatusCode > pending)
                           .OrderByDescending(m => m.Id)
                           .Include("Importer")
                           .Include("Invoice")
                           .Include("ApplicationItems")
                           .Include("ApplicationDocuments")
                           .Include("ApplicationType")
                           .Skip(tpageNumber).Take(tsize)
                           .ToList();

                       if (myApplications.Any())
                       {
                           var newList = new List<ApplicationObject>();
                           myApplications.ForEach(app =>
                           {
                               var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                               if (importObject != null && importObject.Id > 0)
                               {
                                   importObject.ImporterStr = app.Importer.Name;
                                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                                   importObject.AppTypeStr = app.ApplicationType.Name;
                                   importObject.ImportClassName = Enum.GetName(typeof(NotificationClassEnum), importObject.ClassificationId).Replace("_", " ");
                                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                                   if (name != null) importObject.StatusStr = name.Replace("_", " ");
                                   importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                                   importObject.ReferenceCode = app.Invoice.RRR;
                                   importObject.ImporterStr = app.Importer.Name;
                                   importObject.ApplicationTypeName = app.ApplicationType.Name;
                                   newList.Add(importObject);
                               }
                           });
                           countG = db.Applications.Count(j => j.ApplicationStatusCode > pending);
                           return newList.OrderByDescending(k => k.Id).ToList();
                       }
                   }

               }
               countG = 0;
               return new List<ApplicationObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ApplicationObject>();
           }
       }

       public List<JobDistributionObject> GetJobDistributions()
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var jobDistributions = (from em in
                                               db.EmployeeDesks.Where(j => j.JobCount > 0)
                                               .Include("UserProfile")
                                           join ps in db.People on em.UserProfile.PersonId equals ps.Id
                                           select new JobDistributionObject
                                           {
                                               EmployeeId = em.Id,
                                               ApplicationCount = em.ApplicationCount,
                                               TotalJobCount = em.JobCount,
                                               NotificationCount = em.NotificationCount,
                                               RecertificationCount = em.RecertificationCount,
                                               EmployeeName = ps.FirstName + " " + ps.LastName
                                           }).ToList();

                   if (!jobDistributions.Any())
                   {
                       return new List<JobDistributionObject>();  
                   }

                   jobDistributions.ForEach(app =>
                   {
                       app.ApplicationCount = app.ApplicationCount != null && app.ApplicationCount > 0 ? app.ApplicationCount : 0;
                       app.TotalJobCount = app.TotalJobCount != null && app.TotalJobCount > 0 ? app.TotalJobCount : 0;
                       app.NotificationCount = app.NotificationCount != null && app.NotificationCount > 0 ? app.NotificationCount : 0;
                       app.RecertificationCount = app.RecertificationCount != null && app.RecertificationCount > 0 ? app.RecertificationCount : 0;

                   });

                   return jobDistributions;
               }
           }
           catch (Exception ex)
           {
               return new List<JobDistributionObject>();
           }
       }

       public ApplicationItemObject GetDepotOwnerApplicationItem(long applicationItemId, long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var applicationItems = (from appItem in db.ApplicationItems.Where(k => k.Id == applicationItemId).Include("ApplicationCountries")
                                           join th in db.ThroughPuts
                                           on appItem.Id equals th.ApplicationItemId
                                           join dp in db.Depots.Where(d => d.ImporterId == importerId)
                                           on th.DepotId equals dp.Id
                                           join imp in db.Importers on appItem.Application.ImporterId equals imp.Id
                                           select new { appItem , imp}).ToList();

                   if (!applicationItems.Any())
                   {
                       return new ApplicationItemObject();
                   }

                   var appObj = applicationItems[0].appItem;
                   var cmp = applicationItems[0].imp;
                   var importObject = new ApplicationItemObject
                   {
                       Id = appObj.Id,
                       ApplicationId = appObj.ApplicationId,
                       ProductId = appObj.ProductId,
                       EstimatedQuantity = appObj.EstimatedQuantity,
                       EstimatedValue = appObj.EstimatedValue,
                       PSFNumber = appObj.PSFNumber,
                       ReferenceLicenseCode = appObj.ReferenceLicenseCode,
                       TotalImportedQuantity = appObj.TotalImportedQuantity,
                       ImportedQuantityValue = appObj.ImportedQuantityValue,
                       OutstandingQuantity = appObj.OutstandingQuantity,
                       DateImported = appObj.DateImported,
                       StorageProviderTypeId = appObj.StorageProviderTypeId,
                       CountryOfOriginName = "",
                       ProductName = appObj.Product.Name,
                       ImporterName = cmp.Name
                        
                   }; 
                    
                    importObject.CountryOfOriginName = "";
                    appObj.ApplicationCountries.ToList().ForEach(c =>
                    {
                        var countries = db.Countries.Where(k => k.Id == c.CountryId).ToList();
                        if (countries.Any())
                        {
                            if (string.IsNullOrEmpty(importObject.CountryOfOriginName))
                            {
                                importObject.CountryOfOriginName = countries[0].Name;
                            }
                            else
                            {
                                importObject.CountryOfOriginName += ", " + countries[0].Name;
                            }
                        }
                    });

                   importObject.EstimatedQuantityStr = importObject.EstimatedQuantity.ToString("n1").Replace(".0", "");
                   importObject.EstimatedValueStr = importObject.EstimatedValue.ToString("n1").Replace(".0", "");
                   if (appObj.ThroughPuts.Any())
                   {
                      var tpObj = appObj.ThroughPuts.ToList()[0];
                       var docs = db.Documents.Where(d => d.DocumentId == tpObj.DocumentId).ToList();
                       if (docs.Any())
                       {
                           return new ApplicationItemObject();
                       }

                       if (tpObj.DocumentId != null)
                           importObject.ThroughPutObjects =  
                               new List<ThroughPutObject>
                               {
                                   new ThroughPutObject
                                   {
                                       Id = tpObj.Id,
                                       ApplicationItemId = tpObj.ApplicationItemId,
                                       DepotId =  tpObj.DepotId,
                                       ProductId  = tpObj.ProductId,
                                       Quantity  = tpObj.Quantity,
                                       Comment  = tpObj.Comment,
                                       DocumentId  = (long) tpObj.DocumentId,
                                       IPAddress  = tpObj.IPAddress,
                                       DocumentPath = docs[0].DocumentPath,
                                       Status = docs[0].Status,
                                       DocumentObject = new DocumentObject
                                       {
                                           ImporterId = docs[0].ImporterId,
                                           DateUploaded = docs[0].DateUploaded,
                                           UploadedById = docs[0].UploadedById,
                                           DocumentPath = docs[0].DocumentPath,
                                           DocumentTypeId = docs[0].DocumentTypeId,
                                           Status = docs[0].Status,
                                           IpAddress = docs[0].IpAddress
                                       }       
                                   }
                               };
                   }
                   else
                   {
                       importObject.ThroughPutObjects = new List<ThroughPutObject>();
                   }
                   return importObject;
               }
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ApplicationItemObject();
           }
       }

       public ApplicationObject GetApplication(long applicationId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var myApplications =
                        db.Applications.Where(m => m.Id == applicationId)
                            .Include("Importer")
                            .Include("ApplicationItems")
                            .Include("ApplicationDocuments")
                            .Include("Invoice")
                            .Include("ProcessTrackings")
                            .Include("ApplicationIssues")
                            .Include("SignOffDocuments")
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
                    if (app.SignOffDocuments.Any())
                    {
                        importObject.SignOffDocumentPath = app.SignOffDocuments.ToList()[0].DocumentPath;
                        importObject.IsSignOffDocUploaded = true;
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
                                feeObject.FeeTypeName = fee.FeeType.Name + " (" + WebUtility.HtmlDecode("&#8358;") + feeObject.Amount.ToString("n0") + "/" + "30,000 MT)";
                                feeObject.Amount = app.Invoice.TotalAmountDue - processingFee.Amount;
                                feeObject.AmountStr = feeObject.Amount.ToString("n0");
                            }
                            else
                            {
                                feeObject.FeeTypeName = fee.FeeType.Name;
                            }
                            feeObject.ImportStageName = fee.ImportStage.Name;
                            objList.Add(feeObject);
                        }
                    });
                    importObject.FeeObjects = objList;
                    
                    importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                  
                    importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                    importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                    importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                   importObject.ReferenceCode = app.Invoice.RRR;
                    var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                    if (name != null)
                        importObject.StatusStr = name.Replace("_", " ");

                    if (app.ApplicationIssues.Any())
                    {
                        importObject.IssuesCount = app.ApplicationIssues.Count;
                        importObject.ApplicationIssueObjects = new List<ApplicationIssueObject>();
                        app.ApplicationIssues.ToList().ForEach(b =>
                        {
                            var issueTypes = db.IssueTypes.Where(q => q.Id == b.IssueTypeId).ToList();
                            if (issueTypes.Any())
                            {
                                importObject.ApplicationIssueObjects.Add(new ApplicationIssueObject
                                {
                                    IssueTypeName = issueTypes[0].Name,
                                    Id = b.Id,
                                    IssueTypeId = b.IssueTypeId,
                                    Description = b.Description,
                                    Status = b.Status,
                                    IssueDate = b.IssueDate,
                                    IssueDateStr = b.IssueDate != null ? ((DateTime)b.IssueDate).ToString("dd/MM/yyyy") : ""
                                }); 
                            }
                        });
                    }
                    
                    importObject.CompanyName = app.Importer.Name;

                    importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                    var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                    if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                    app.ApplicationDocuments.ToList().ForEach(o =>
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
                                    DocumentId = doc.DocumentId,
                                    Uploaded = true,
                                     StatusStr = Enum.GetName(typeof(DocStatus), doc.Status),Status =  doc.Status,
                                    DocumentPath = doc.DocumentPath.Replace("~", ""),
                                    DocumentTypeId = doc.DocumentType.DocumentTypeId,
                                    Name = doc.DocumentType.Name
                                });
                            }
                        }

                    });
                    const int psf = (int)CustomColEnum.Psf;
                    var productIds = new List<long>();
                    var storageTypeIds = new List<long>();

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
                                        var depotName = im.StorageProviderTypeId == (int)StorageProviderTypeEnum.Own_Depot ? d.Depot.Name + "(" + d.Depot.DepotLicense + ")" : d.Depot.Name;

                                        if (string.IsNullOrEmpty(im.DischargeDepotName))
                                        {
                                            im.DischargeDepotName = depotName;
                                        }
                                        else
                                        {
                                            im.DischargeDepotName += ", " + depotName;
                                        }

                                        im.ThroughPutObjects.Add(new ThroughPutObject
                                        {
                                            Id = d.Id,
                                            DepotId = d.DepotId,
                                            ProductName = im.ProductObject.Name,ProductCode = im.ProductObject.Code,
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
                                    im.ProductBankerName = string.IsNullOrEmpty(c.BankAccountNumber) ? c.Bank.Name : c.Bank.Name + "(" + c.BankAccountNumber + ")";
                                }
                                else
                                {
                                    var bankname = string.IsNullOrEmpty(c.BankAccountNumber) ? c.Bank.Name : c.Bank.Name + "(" + c.BankAccountNumber + ")";
                                    im.ProductBankerName += ", " + bankname;
                                }
                                var pdBnk = new ProductBankerObject
                                {
                                    Id = c.Id,
                                    ApplicationItemId = c.ApplicationItemId,
                                    BankId = c.BankId,
                                     BankName = string.IsNullOrEmpty(c.BankAccountNumber) ? c.Bank.Name : c.Bank.Name + "(" + c.BankAccountNumber + ")",
                                    ApplicationId = app.Id,
                                    ImporterId = app.ImporterId,
                                    DocumentId = c.DocumentId,
                                    IsUploaded = c.DocumentId != null ? true : false,
                                    ProductCode = im.ProductObject.Code,
                                    DocumentPath = c.Document != null ? c.Document.DocumentPath.Replace("~", "") : string.Empty,
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
                            productIds.Add(im.ProductId);
                            storageTypeIds.Add(im.StorageProviderTypeId);
                        }
                    });

                   importObject.DocumentTypeObjects = GetApplicationStageDocumentTypes(importObject.DocumentTypeObjects, productIds, importObject.ClassificationId, storageTypeIds);
                  return importObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ApplicationObject();
           }
       }

       public ApplicationObject GetPaidApplication(long invoiceId)
       {
           try
           {
               PaymentReceipt pReceipt;
               using (var db2 = new PPIPSPaymentEntities())
               {
                   var myReceipts = db2.PaymentReceipts.Where(v => v.Id == invoiceId)
                           .Include("TransactionInvoice")
                           .ToList();

                   if (!myReceipts.Any())
                   {
                       return new ApplicationObject();
                   }

                   pReceipt = myReceipts[0];

               }

               if (pReceipt == null || pReceipt.Id < 1)
               {
                   return new ApplicationObject();
               }

               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Applications.Where(m => m.Invoice.RRR == pReceipt.TransactionInvoice.RRR)
                           .Include("Importer")
                           .Include("ApplicationItems")
                           .Include("ApplicationDocuments")
                           .Include("Invoice")
                           .Include("ProcessTrackings")
                           .Include("ApplicationIssues")
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
                               feeObject.FeeTypeName = fee.FeeType.Name + " (" + WebUtility.HtmlDecode("&#8358;") + feeObject.Amount.ToString("n0") + "/" + "30,000 MT)";
                               feeObject.Amount = app.Invoice.TotalAmountDue - processingFee.Amount;
                               feeObject.AmountStr = feeObject.Amount.ToString("n0");
                           }
                           else
                           {
                               feeObject.FeeTypeName = fee.FeeType.Name;
                           }
                           feeObject.ImportStageName = fee.ImportStage.Name;
                           objList.Add(feeObject);
                       }
                   });
                   importObject.FeeObjects = objList;

                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");

                   importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                   importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                   importObject.ReferenceCode = app.Invoice.RRR;
                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                   if (name != null)
                       importObject.StatusStr = name.Replace("_", " ");

                   if (app.ApplicationIssues.Any())
                   {
                       importObject.IssuesCount = app.ApplicationIssues.Count;
                       importObject.ApplicationIssueObjects = new List<ApplicationIssueObject>();
                       app.ApplicationIssues.ToList().ForEach(b =>
                       {
                           var issueTypes = db.IssueTypes.Where(q => q.Id == b.IssueTypeId).ToList();
                           if (issueTypes.Any())
                           {
                               importObject.ApplicationIssueObjects.Add(new ApplicationIssueObject
                               {
                                   IssueTypeName = issueTypes[0].Name,
                                   Id = b.Id,
                                   IssueTypeId = b.IssueTypeId,
                                   Description = b.Description,
                                   Status = b.Status,
                                   IssueDate = b.IssueDate,
                                   IssueDateStr = b.IssueDate != null ? ((DateTime)b.IssueDate).ToString("dd/MM/yyyy") : ""
                               });
                           }
                       });
                   }

                   importObject.CompanyName = app.Importer.Name;

                   importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                   app.ApplicationDocuments.ToList().ForEach(o =>
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
                                   DocumentId = doc.DocumentId,
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
                   const int psf = (int)CustomColEnum.Psf;
                   var productIds = new List<long>();
                   var storageTypeIds = new List<long>();

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
                                       var depotName = im.StorageProviderTypeId == (int)StorageProviderTypeEnum.Own_Depot ? d.Depot.Name + "(" + d.Depot.DepotLicense + ")" : d.Depot.Name;

                                       if (string.IsNullOrEmpty(im.DischargeDepotName))
                                       {
                                           im.DischargeDepotName = depotName;
                                       }
                                       else
                                       {
                                           im.DischargeDepotName += ", " + depotName;
                                       }

                                       im.ThroughPutObjects.Add(new ThroughPutObject
                                       {
                                           Id = d.Id,
                                           DepotId = d.DepotId,
                                            ProductName = im.ProductObject.Name,ProductCode = im.ProductObject.Code,
                                           ProductId = d.ProductId,
                                           Quantity = d.Quantity,
                                           DocumentId = d.DocumentId,
                                           DocumentPath =
                                               d.Document != null && !string.IsNullOrEmpty(d.Document.DocumentPath)
                                                   ? d.Document.DocumentPath.Replace("~", "")
                                                   : string.Empty,
                                           IPAddress = d.IPAddress,
                                           DepotName = d.Depot.Name,
                                           ThName = d.Depot.Name,
                                           Status = d.Document != null ? d.Document.Status : 0,
                                           StatusStr =
                                               d.Document != null
                                                   ? Enum.GetName(typeof (DocStatus), d.Document.Status)
                                                   : string.Empty,
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
                                   im.ProductBankerName = string.IsNullOrEmpty(c.BankAccountNumber) ? c.Bank.Name : c.Bank.Name + "(" + c.BankAccountNumber + ")";
                               }
                               else
                               {
                                   var bankname = string.IsNullOrEmpty(c.BankAccountNumber) ? c.Bank.Name : c.Bank.Name + "(" + c.BankAccountNumber + ")";
                                   im.ProductBankerName += ", " + bankname;
                               }
                               var pdBnk = new ProductBankerObject
                               {
                                   Id = c.Id,
                                   ApplicationItemId = c.ApplicationItemId,
                                   BankId = c.BankId,
                                    BankName = string.IsNullOrEmpty(c.BankAccountNumber) ? c.Bank.Name : c.Bank.Name + "(" + c.BankAccountNumber + ")",
                                   ApplicationId = app.Id,
                                   ImporterId = app.ImporterId,
                                   DocumentId = c.DocumentId,
                                   IsUploaded = c.DocumentId != null ? true : false,
                                   ProductCode = im.ProductObject.Code,
                                   DocumentPath = c.Document != null ? c.Document.DocumentPath.Replace("~", "") : string.Empty,
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
                           productIds.Add(im.ProductId);
                           storageTypeIds.Add(im.StorageProviderTypeId);
                       }
                   });

                   importObject.DocumentTypeObjects = GetApplicationStageDocumentTypes(importObject.DocumentTypeObjects, productIds, importObject.ClassificationId, storageTypeIds);
                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ApplicationObject();
           }
       }

       public ApplicationObject GetSubmittedAppDetails(long applicationId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Applications.Where(m => m.Id == applicationId)
                           .Include("Importer")
                           .Include("ApplicationItems")
                           .Include("ApplicationDocuments")
                           .Include("Invoice")
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
                               feeObject.FeeTypeName = fee.FeeType.Name + " (" + WebUtility.HtmlDecode("&#8358;") + feeObject.Amount.ToString("n0") + "/" + "30,000 MT)";
                               feeObject.Amount = app.Invoice.TotalAmountDue - processingFee.Amount;
                               feeObject.AmountStr = feeObject.Amount.ToString("n0");
                           }
                           else
                           {
                               feeObject.FeeTypeName = fee.FeeType.Name;
                           }
                           feeObject.ImportStageName = fee.ImportStage.Name;
                           objList.Add(feeObject);
                       }
                   });

                   importObject.FeeObjects = objList;

                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");

                   importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                   importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                   importObject.ReferenceCode = app.Invoice.RRR;
                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                   if (name != null)
                       importObject.StatusStr = name.Replace("_", " ");

                   if (app.ApplicationIssues.Any())
                   {
                       importObject.IssuesCount = app.ApplicationIssues.Count;
                       importObject.ApplicationIssueObjects = new List<ApplicationIssueObject>();
                       app.ApplicationIssues.ToList().ForEach(b =>
                       {
                           var issueTypes = db.IssueTypes.Where(q => q.Id == b.IssueTypeId).ToList();
                           if (issueTypes.Any())
                           {
                               importObject.ApplicationIssueObjects.Add(new ApplicationIssueObject
                               {
                                   IssueTypeName = issueTypes[0].Name,
                                   Id = b.Id,
                                   IssueTypeId = b.IssueTypeId,
                                   Description = b.Description,
                                   Status = b.Status,
                                   IssueDate = b.IssueDate,
                                   IssueDateStr = b.IssueDate != null ? ((DateTime)b.IssueDate).ToString("dd/MM/yyyy") : ""
                               });
                           }
                       });
                   }

                   importObject.CompanyName = app.Importer.Name;

                   importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                   app.ApplicationDocuments.ToList().ForEach(o =>
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
                                   DocumentId = doc.DocumentId,
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
                   const int psf = (int)CustomColEnum.Psf;
                   var productIds = new List<long>();
                   var storageTypeIds = new List<long>();

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

                                       im.ThroughPutObjects.Add(new ThroughPutObject
                                       {
                                           Id = d.Id,
                                           DepotId = d.DepotId,
                                            ProductName = im.ProductObject.Name,ProductCode = im.ProductObject.Code,
                                           ProductId = d.ProductId,
                                           Quantity = d.Quantity,
                                           DocumentId = d.DocumentId,
                                           DocumentPath =
                                               d.Document != null && !string.IsNullOrEmpty(d.Document.DocumentPath)
                                                   ? d.Document.DocumentPath.Replace("~", "")
                                                   : string.Empty,
                                           IPAddress = d.IPAddress,
                                           DepotName = d.Depot.Name,
                                           ThName = d.Depot.Name,
                                           Status = d.Document != null ? d.Document.Status : 0,
                                           StatusStr =
                                               d.Document != null
                                                   ? Enum.GetName(typeof(DocStatus), d.Document.Status)
                                                   : string.Empty,
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
                           }

                           im.ProductBankerObjects = new List<ProductBankerObject>();
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
                               var pdBnk = new ProductBankerObject
                               {
                                   Id = c.Id,
                                   ApplicationItemId = c.ApplicationItemId,
                                   BankId = c.BankId,
                                    BankName = string.IsNullOrEmpty(c.BankAccountNumber) ? c.Bank.Name : c.Bank.Name + "(" + c.BankAccountNumber + ")",
                                   ApplicationId = app.Id,
                                   ImporterId = app.ImporterId,
                                   DocumentId = c.DocumentId,
                                   IsUploaded = c.DocumentId != null ? true : false,
                                   ProductCode = im.ProductObject.Code,
                                   DocumentPath = c.Document != null ? c.Document.DocumentPath.Replace("~", "") : string.Empty,
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
                           productIds.Add(im.ProductId);
                           storageTypeIds.Add(im.StorageProviderTypeId);
                       }
                   });

                   importObject.DocumentTypeObjects = GetApplicationStageDocumentTypes(importObject.DocumentTypeObjects, productIds, importObject.ClassificationId, storageTypeIds);
                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ApplicationObject();
           }
       }

       public ApplicationObject GetApplicationDetails(long applicationId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Applications.Where(m => m.Id == applicationId)
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
                   importObject.FeeObjects = objList;

                   var steps = new List<Step>();

                   var processTrackings = app.ProcessingHistories.ToList();

                   if (!processTrackings.Any())
                   {
                       importObject.PercentageCompletion = 0;
                   }

                   var processes = db.Processes.Where(p => p.ImportStageId == appStage).Include("Steps").ToList();

                   if (!processes.Any())
                   {
                       importObject.PercentageCompletion = 0;
                   }

                   processes.ForEach(p =>
                   {
                       steps.AddRange(p.Steps);
                   });

                   if (!steps.Any())
                   {
                       importObject.PercentageCompletion = 0;
                   }

                   if (processTrackings.Count == steps.Count)
                   {
                       importObject.PercentageCompletion = 100;
                   }

                   var percentageCompletion = processTrackings.Count * 100 / steps.Count;
                   importObject.PercentageCompletion = percentageCompletion;

                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                   importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                   importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                   if (name != null)importObject.StatusStr = name.Replace("_", " ");
                   importObject.ReferenceCode = app.Invoice.ReferenceCode;
                   importObject.Rrr = app.Invoice.RRR;
                   importObject.InvoiceId = app.Invoice.Id;
                   importObject.PaymentTypeId = app.Invoice.PaymentTypeId;
                   importObject.FeeObjects = objList;
                   importObject.CompanyName = app.Importer.Name;
                   importObject.PaymentTypeId = app.Invoice.PaymentTypeId;
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                   importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                   app.ApplicationDocuments.ToList().ForEach(o =>
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
                                   Status =  doc.Status,
                                   DocumentPath = doc.DocumentPath.Replace("~", ""),
                                   DocumentTypeId = doc.DocumentType.DocumentTypeId,
                                   Name = doc.DocumentType.Name
                               });
                           }
                       }

                   });
                   
                   const int psf = (int)CustomColEnum.Psf;
                   var productIds = new List<long>();
                   var storageTypeIds = new List<long>();
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

                                       im.ThroughPutObjects.Add(new ThroughPutObject
                                       {
                                           Id = d.Id,
                                           DepotId = d.DepotId,
                                            ProductName = im.ProductObject.Name,ProductCode = im.ProductObject.Code,
                                           ProductId = d.ProductId,
                                           Quantity = d.Quantity,
                                           DocumentId = d.DocumentId,
                                           DocumentPath =
                                               d.Document != null && !string.IsNullOrEmpty(d.Document.DocumentPath)
                                                   ? d.Document.DocumentPath.Replace("~", "")
                                                   : string.Empty,
                                           IPAddress = d.IPAddress,
                                           DepotName = d.Depot.Name,
                                           ThName = d.Depot.Name,
                                           Status = d.Document != null ? d.Document.Status : 0,
                                           StatusStr =
                                               d.Document != null
                                                   ? Enum.GetName(typeof(DocStatus), d.Document.Status)
                                                   : string.Empty,
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
                           }

                           im.ProductBankerObjects = new List<ProductBankerObject>();
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
                               var pdBnk = new ProductBankerObject
                               {
                                   Id = c.Id,
                                   ApplicationItemId = c.ApplicationItemId,
                                   BankId = c.BankId,
                                   BankName = string.IsNullOrEmpty(c.BankAccountNumber) ? c.Bank.Name : c.Bank.Name + "(" + c.BankAccountNumber + ")",
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
                           productIds.Add(im.ProductId);
                           storageTypeIds.Add(im.StorageProviderTypeId);
                       }
                   });

                   importObject.DocumentTypeObjects = GetApplicationStageDocumentTypes(importObject.DocumentTypeObjects, productIds, importObject.ClassificationId, storageTypeIds);
                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ApplicationObject();
           }
       }

       public InvoiceObject GetRrrInfo(long invoiceId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var invoices = db.Invoices.Where(m => m.Id == invoiceId)
                                  .Include("Importer")
                                  .ToList();

                   if (!invoices.Any())
                   {
                       return new InvoiceObject();
                   }

                   var app = invoices[0];

                   var invoiceObject = ModelMapper.Map<Invoice, InvoiceObject>(app);
                   if (invoiceObject == null || invoiceObject.Id < 1)
                   {
                       return new InvoiceObject();
                   }
                  
                   invoiceObject.FeeObjects = new List<FeeObject>();

                   var feeObjects = (from invItem in db.InvoiceItems.Where(k => k.InvoiceId == invoiceObject.Id)
                                             join f in db.Fees on invItem.FeeId equals f.FeeId
                                             select new FeeObject
                                             {
                                                FeeId = f.FeeId,
                                                ImportStageId = f.ImportStageId,
                                                FeeTypeId = f.FeeTypeId,
                                                Amount = f.Amount,
                                                CurrencyCode = f.CurrencyCode,
                                                Name = f.Name,
                                                PrincipalSplit = f.PrincipalSplit,
                                                VendorSplit = f.VendorSplit,
                                                PaymentGatewaySplit = f.PaymentGatewaySplit,
                                                BillableToPrincipal = f.BillableToPrincipal

                                             }).ToList();

                   if (!feeObjects.Any())
                   {
                       return new InvoiceObject();
                   }

                   invoiceObject.FeeObjects = feeObjects;
                   
                   var users = (from p in db.People.Where(p => p.ImporterId == app.ImporterId && p.IsImporter == true).Include("UserProfiles")
                                join usp in db.UserProfiles on p.Id equals usp.PersonId
                                join asp in db.AspNetUsers on usp.Id equals asp.UserInfo_Id
                                select new { usp, asp }).ToList();

                   if (!users.Any())
                   {
                       return new InvoiceObject();
                   }
                   
                   invoiceObject.ImporterName = app.Importer.Name;

                   var ph = users[0].asp.PhoneNumber;
                   var em = users[0].asp.Email;
                   invoiceObject.PhoneNumber = ph; 
                   invoiceObject.Email = em;

                   return invoiceObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new InvoiceObject();
           }
       }

       public ApplicationObject GetApplicationProcesses(long applicationId)
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

                   const int appStage = (int)AppStage.Application;
                   var processes = db.Processes.Where(p => p.ImportStageId == appStage).Include("Steps").ToList();
                   var processTrackings = db.ProcessTrackings.Where(p => p.ApplicationId == applicationId).Include("EmployeeDesk").Include("Step").ToList();
                   var histories = db.ProcessingHistories.Where(p => p.ApplicationId == applicationId).ToList();
                  
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
                   
                   if (processTrackings.Any())
                   {
                       var dtrackingEntity = processTrackings.ToList()[0];
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
                               ApplicationId = dtrackingEntity.ApplicationId,
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
                                var processName= step.Process.Name;
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

       public List<ProcessTrackingObject> GetEmployeeApplicationProcesses(int? itemsPerPage, int? pageNumber, out int countG, long employeeId)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   const int open = (int) OutComeCodeEnum.Open;
                   //const int treated = (int) OutComeCodeEnum.Treated;
                   const int rejected = (int) OutComeCodeEnum.Rejected;
                   const int transfered = (int) OutComeCodeEnum.Transfered;

                   using (var db = new ImportPermitEntities())
                   {
                            var processTrackings = (from p in
                            db.ProcessTrackings.Where(p => p.EmployeeId == employeeId && (p.OutComeCode == open || p.OutComeCode == rejected || p.OutComeCode == transfered))
                            .Include("EmployeeDesk")
                            .Include("Application")
                            .OrderByDescending(m => m.Id)
                            .Skip(tpageNumber).Take(tsize)
                            join ap in db.Applications.Include("Invoice") on p.ApplicationId equals ap.Id
                            join cm in db.Importers on ap.ImporterId equals cm.Id
                            join usp in db.UserProfiles.Include("Person") on p.EmployeeDesk.EmployeeId equals  usp.Id
                            join st in db.Steps.Include("Process") on p.StepId equals st.Id

                                select new ProcessTrackingObject
                                {
                                    Id = p.Id,
                                    ApplicationId = ap.Id,
                                    StepId = p.StepId,
                                    EmployeeId = p.EmployeeDesk.EmployeeId,
                                    StatusId = p.StatusId,
                                    AssignedTime = p.AssignedTime,
                                    DueTime = p.DueTime,
                                    ActualDeliveryDateTime = p.ActualDeliveryDateTime,
                                    StepCode = p.StepCode,
                                    OutComeCode = p.OutComeCode,
                                    EmployeeName = usp.Person.FirstName + " " + usp.Person.LastName,
                                    ProcessName = st.Process.Name,
                                    CurrentStepName = st.Name,
                                    CompanyName = cm.Name,
                                    Rrr = ap.Invoice.RRR

                                }).ToList();

                       if (!processTrackings.Any())
                       {
                           countG = 0;
                           return new List<ProcessTrackingObject>();
                       }

                       if (processTrackings.Any())
                       {
                           processTrackings.ForEach( app =>
                           {
                               app.OutComeCodeStr = app.OutComeCode != null ? Enum.GetName(typeof (OutComeCodeEnum), app.OutComeCode) : "Pending";
                               app.AssignedTimeStr = app.AssignedTime != null ? app.AssignedTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Pending";
                               app.DueTimeStr = app.DueTime != null ? app.DueTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Unavailable";
                               app.ActualDeliveryDateTimeStr = app.ActualDeliveryDateTime != null ? app.ActualDeliveryDateTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Pending";
                           });
                       }
                       countG =  db.ProcessTrackings.Count(p => p.EmployeeId == employeeId && (p.OutComeCode == open || p.OutComeCode == rejected || p.OutComeCode == transfered));
                       return processTrackings;
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

       public List<ProcessTrackingObject> GetEmployeeApplicationProcesses(long employeeId)
       {
           try
           {
               const int open = (int)OutComeCodeEnum.Open;
               const int rejected = (int)OutComeCodeEnum.Rejected;
               const int transfered = (int)OutComeCodeEnum.Transfered;

               using (var db = new ImportPermitEntities())
               {
                   var processTrackings = (from p in
                                               db.ProcessTrackings.Where(p => p.EmployeeId == employeeId)
                                               .Include("EmployeeDesk")
                                               .Include("Application")
                                               .OrderByDescending(m => m.Id)
                                           join ap in db.Applications.Include("Invoice") on p.ApplicationId equals ap.Id
                                           join cm in db.Importers on ap.ImporterId equals cm.Id
                                           join usp in db.UserProfiles.Include("Person") on p.EmployeeDesk.EmployeeId equals usp.Id
                                           join st in db.Steps.Include("Process") on p.StepId equals st.Id

                                           select new ProcessTrackingObject
                                           {
                                               Id = p.Id,
                                               ApplicationId = ap.Id,
                                               StepId = p.StepId,
                                               EmployeeId = p.EmployeeDesk.EmployeeId,
                                               StatusId = p.StatusId,
                                               AssignedTime = p.AssignedTime,
                                               DueTime = p.DueTime,
                                               ActualDeliveryDateTime = p.ActualDeliveryDateTime,
                                               StepCode = p.StepCode,
                                               OutComeCode = p.OutComeCode,
                                               EmployeeName = usp.Person.FirstName + " " + usp.Person.LastName,
                                               ProcessName = st.Process.Name,
                                               CurrentStepName = st.Name,
                                               CompanyName = cm.Name,
                                               Rrr = ap.Invoice.RRR

                                           }).ToList();

                   if (!processTrackings.Any())
                   {
                       return new List<ProcessTrackingObject>();
                   }

                   if (processTrackings.Any())
                   {
                       processTrackings.ForEach(app =>
                       {
                           app.OutComeCodeStr = app.OutComeCode != null ? Enum.GetName(typeof(OutComeCodeEnum), app.OutComeCode) : "Pending";
                           app.AssignedTimeStr = app.AssignedTime != null ? app.AssignedTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Pending";
                           app.DueTimeStr = app.DueTime != null ? app.DueTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Unavailable";
                           app.ActualDeliveryDateTimeStr = app.ActualDeliveryDateTime != null ? app.ActualDeliveryDateTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Pending";
                       });
                   }

                   return processTrackings;
               }
           }
           catch (Exception ex)
           {
               return new List<ProcessTrackingObject>();
           }
       }

       public ApplicationObject GetAppForEdit(long applicationId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Applications.Where(m => m.Id == applicationId)
                           .Include("ApplicationItems")
                            .Include("Invoice")
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

                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                 
                   importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                   importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                   importObject.ReferenceCode = app.Invoice.ReferenceCode;
                   importObject.Rrr = app.Invoice.RRR;
                   importObject.InvoiceId = app.Invoice.Id;
                   importObject.PaymentTypeId = app.Invoice.PaymentTypeId;

                   var importClasss = db.ImportClasses.Where(c => c.Id == app.ClassificationId).ToList();
                   if (!importClasss.Any())
                   {
                       return new ApplicationObject();
                   }

                   importObject.CategoryName = importClasss[0].Name;

                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                   if (name != null)
                       importObject.StatusStr = name.Replace("_", " ");

                   if (!app.ApplicationItems.Any())
                   {
                       return new ApplicationObject();
                   }

                   const int psf = (int)CustomColEnum.Psf;
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
                           if (string.IsNullOrEmpty(im.PSFNumber))
                           {
                               im.PSFNumber = "Not Applicable";
                           }

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
                           
                           im.ProductBankerObjects = new List<ProductBankerObject>();
                           var bankers = db.ProductBankers.Where(i => i.ApplicationItemId == u.Id).Include("Bank").ToList();
                           if (bankers.Any())
                           {
                               im.ProductBankerName = "";
                               bankers.ForEach(t =>
                               {
                                   im.ProductBankerObjects.Add(new ProductBankerObject
                                   {
                                       Id = t.Id,
                                       ApplicationItemId = t.ApplicationItemId,
                                       DocumentId = t.DocumentId,
                                       BankId = t.BankId,
                                       BankObject = new BankObject
                                       {
                                           BankId = t.Bank.BankId,
                                           SortCode = t.Bank.SortCode,
                                           ImporterId = t.Bank.ImporterId,
                                           Name = t.Bank.Name
                                       }
                                   });

                                   if (string.IsNullOrEmpty(im.ProductBankerName))
                                   {
                                       im.ProductBankerName = t.Bank.Name;
                                   }
                                   else
                                   {
                                       im.ProductBankerName += ", " + t.Bank.Name;
                                   }
                               });
                           }

                           var strpType = db.StorageProviderTypes.Where(v => v.Id == im.StorageProviderTypeId).ToList();
                           if (strpType.Any())
                           {
                               im.StorageProviderTypeName = strpType[0].Name;
                           }
                           importObject.ApplicationItemObjects.Add(im);
                       }
                   });
                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ApplicationObject();
           }
       }

       public ApplicationObject GetAppForPayment(long applicationId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Applications.Where(m => m.Id == applicationId)
                           .Include("ApplicationItems")
                            .Include("Invoice")
                             .Include("Importer")
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

                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");

                   importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                   importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                   importObject.CompanyName = app.Importer.Name;
                   importObject.ReferenceCode = app.Invoice.ReferenceCode;
                   importObject.Rrr = app.Invoice.RRR;
                   importObject.InvoiceId = app.Invoice.Id;
                   importObject.PaymentTypeId = app.Invoice.PaymentTypeId;
                   var s = Enum.GetName(typeof (PaymentType), importObject.PaymentTypeId);
                   if (s != null)
                       importObject.PaymentOption = s.Replace("_", "");
                   var importClasss = db.ImportClasses.Where(c => c.Id == app.ClassificationId).ToList();
                   if (!importClasss.Any()) 
                   {
                       return new ApplicationObject();
                   }

                   importObject.CategoryName = importClasss[0].Name;

                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                   if (name != null)
                       importObject.StatusStr = name.Replace("_", " ");

                   if (!app.ApplicationItems.Any())
                   {
                       return new ApplicationObject();
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
                           var bankers = db.ProductBankers.Where(i => i.ApplicationItemId == u.Id).Include("Bank").ToList();
                           if (bankers.Any())
                           {
                               im.ProductBankerName = "";
                               bankers.ForEach(t =>
                               {
                                   if (string.IsNullOrEmpty(im.ProductBankerName))
                                   {
                                       im.ProductBankerName = t.Bank.Name;
                                   }
                                   else
                                   {
                                       im.ProductBankerName += ", " + t.Bank.Name;
                                   }
                               });
                           }

                           var strpType = db.StorageProviderTypes.Where(v => v.Id == im.StorageProviderTypeId).ToList();
                           if (strpType.Any())
                           {
                               im.StorageProviderTypeName = strpType[0].Name;
                           }
                           importObject.ApplicationItemObjects.Add(im);
                       }
                   });
                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ApplicationObject();
           }
       }

       public ApplicationObject GetBankAssignedAppDocuments(long appId, long companyId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var apps = db.Applications.Where(x => x.Id == appId).Include("ApplicationItems")
                                     .Include("ApplicationDocuments").Include("Importer")
                                      .Include("ApplicationType")
                                      .Include("Invoice")
                                     .ToList();

                   if (!apps.Any())
                   {
                       return new ApplicationObject();
                   }

                   var app = apps[0];
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

                   importObject.FeeObjects = objList;

                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");

                   importObject.CompanyName = app.Importer.Name;
                   importObject.ImportClassName = Enum.GetName(typeof(NotificationClassEnum), app.ClassificationId).Replace("_", " ");
                   importObject.AppTypeStr = app.ApplicationType.Name;
                   importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                   importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                   importObject.ReferenceCode = app.Invoice.RRR;
                   if (name != null)importObject.StatusStr = name.Replace("_", " ");
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                   importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                   const int formM = (int)SpecialDocsEnum.Form_M;
                   const int psf = (int)CustomColEnum.Psf;
                   const int refCode = (int)CustomColEnum.Reference_Code;
                   var appDocs = app.ApplicationDocuments.ToList();
                   if (appDocs.Any())
                   {
                       appDocs.ForEach(o =>
                       {
                           var docs = db.Documents.Where(c => c.DocumentId == o.DocumentId).Include("DocumentType").ToList();
                           if (docs.Any())
                           {
                               var doc = docs[0];
                               var dc = new DocumentTypeObject
                               {
                                   IsFormM = doc.DocumentTypeId == formM,
                                    Uploaded = true,  StatusStr = Enum.GetName(typeof(DocStatus), doc.Status),Status =  doc.Status,
                                   DocumentPath = doc.DocumentPath.Replace("~", ""),
                                   DocumentId = doc.DocumentId,
                                   DocumentTypeId = doc.DocumentType.DocumentTypeId,
                                   Name = doc.DocumentType.Name
                               };
                               importObject.DocumentTypeObjects.Add(dc);
                           }

                       });
                   }

                   var productIds = new List<long>();
                   var storageTypeIds = new List<long>();
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
                                                   RequireReferenceCode = pr.ProductColumns.Any() && pr.ProductColumns.Any(v => v.CustomCodeId == refCode),
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

                           im.ProductBankerObjects = new List<ProductBankerObject>();
                           var bankers = db.ProductBankers.Where(i => i.ApplicationItemId == u.Id).Include("Bank").ToList();
                           if (bankers.Any())
                           {
                               im.ProductBankerName = "";
                               bankers.ForEach(t =>
                               {
                                   if (t.Bank.ImporterId == companyId)
                                   {
                                       importObject.BankAccountNumber = !string.IsNullOrEmpty(t.BankAccountNumber) ? t.BankAccountNumber : "Unavailable";
                                   }
                                   im.ProductBankerObjects.Add(new ProductBankerObject
                                   {
                                       Id = t.Id,
                                       ApplicationItemId = t.ApplicationItemId,
                                       DocumentId = t.DocumentId,
                                       BankId = t.BankId,
                                       BankObject = new BankObject
                                       {
                                           BankId = t.Bank.BankId,
                                           SortCode = t.Bank.SortCode,
                                           ImporterId = t.Bank.ImporterId,
                                           Name = t.Bank.Name
                                       }
                                   });

                                   if (string.IsNullOrEmpty(im.ProductBankerName))
                                   {
                                       im.ProductBankerName = t.Bank.Name;
                                   }
                                   else
                                   {
                                       im.ProductBankerName += ", " + t.Bank.Name;
                                   }
                               });
                           }

                           var strpType = db.StorageProviderTypes.Where(v => v.Id == im.StorageProviderTypeId).ToList();
                           if (strpType.Any())
                           {
                               im.StorageProviderTypeName = strpType[0].Name;
                           }
                           importObject.ApplicationItemObjects.Add(im);
                           productIds.Add(im.ProductId);
                           storageTypeIds.Add(im.StorageProviderTypeId);
                       }
                   });

                   
                   importObject.DocumentTypeObjects = GetApplicationStageDocumentTypes(importObject.DocumentTypeObjects, productIds, importObject.ClassificationId, storageTypeIds);

                   return importObject;
               }
              
           }
           catch (Exception ex)
           {
               return new ApplicationObject();
           }
       }

       public int UpdateBankAccounts(List<ProductBankerObject> bankers)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   foreach (var bnk in bankers)
                    {
                        var banks = db.ProductBankers.Where(x => x.Id == bnk.BankId).ToList();
                        if (!banks.Any())
                        {
                            return -2;
                        }
                        var bnkInfo = banks[0];
                        bnkInfo.BankAccountNumber = bnk.BankAccountNumber;
                        db.Entry(bnkInfo).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                   return 5;
               }
               
           }
           catch (Exception ex)
           {
               return -2;
           }
       }
       
       public ApplicationObject GetAppDocuments(long appId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var apps = db.Applications.Where(x => x.Id == appId).Include("ApplicationItems").Include("Invoice").Include("ApplicationDocuments").Include("Importer").ToList();

                   if (!apps.Any())
                   {
                       return new ApplicationObject();
                   }

                   var app = apps[0];
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
                   importObject.FeeObjects = objList;
                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                   importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                   if (name != null)
                       importObject.StatusStr = name.Replace("_", " ");
                  importObject.ReferenceCode = app.Invoice.RRR;
                   importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                   importObject.CompanyName = app.Importer.Name;
                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");

                   importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                   const int formM = (int)SpecialDocsEnum.Form_M;
                   const int psf = (int)CustomColEnum.Psf;
                   const int refCode = (int)CustomColEnum.Reference_Code;
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                   var appDocs = app.ApplicationDocuments.ToList();
                   if (appDocs.Any())
                   {
                       appDocs.ForEach(o =>
                       {
                           var docs = db.Documents.Where(c => c.DocumentId == o.DocumentId).Include("DocumentType").ToList();
                           if (docs.Any())
                           {
                               var doc = docs[0];
                               var dc = new DocumentTypeObject
                               {
                                   IsFormM = doc.DocumentTypeId == formM,
                                    Uploaded = true,  StatusStr = Enum.GetName(typeof(DocStatus), doc.Status),Status =  doc.Status,
                                   DocumentPath = doc.DocumentPath.Replace("~", ""),
                                   DocumentId = doc.DocumentId,
                                   DocumentTypeId = doc.DocumentType.DocumentTypeId,
                                   Name = doc.DocumentType.Name
                               };
                               importObject.DocumentTypeObjects.Add(dc);
                           }

                       });
                   }

                   var productIds = new List<long>();
                   var storageTypeIds = new List<long>();
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
                                                   RequireReferenceCode = pr.ProductColumns.Any() && pr.ProductColumns.Any(v => v.CustomCodeId == refCode),
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


                           im.ProductBankerObjects = new List<ProductBankerObject>();
                           var bankers = db.ProductBankers.Where(i => i.ApplicationItemId == u.Id).Include("Bank").ToList();
                           if (bankers.Any())
                           {
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
                                   var pdBnk = new ProductBankerObject
                                   {
                                       Id = c.Id,
                                       ApplicationItemId = c.ApplicationItemId,
                                       BankId = c.BankId,
                                        BankName = string.IsNullOrEmpty(c.BankAccountNumber) ? c.Bank.Name : c.Bank.Name + "(" + c.BankAccountNumber + ")",
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
                           }

                          var strpType = db.StorageProviderTypes.Where(v => v.Id == im.StorageProviderTypeId).ToList();
                          if (strpType.Any())
                          {
                              im.StorageProviderTypeName = strpType[0].Name;
                          }
                           importObject.ApplicationItemObjects.Add(im);
                           productIds.Add(im.ProductId);
                           storageTypeIds.Add(im.StorageProviderTypeId);
                       }
                   });

                   importObject.DocumentTypeObjects = GetApplicationStageDocumentTypes(importObject.DocumentTypeObjects, productIds, importObject.ClassificationId, storageTypeIds);
                   return importObject.Id < 1 ? new ApplicationObject() : importObject;
               }
               
           }
           catch (Exception ex)
           {
               return new ApplicationObject();
           }
       }

       public ApplicationObject GetAppDocumentsX(long id)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var apps = db.Applications.Where(x => x.Id == id).Include("ApplicationItems")
                                     .Include("ApplicationDocuments").Include("Importer")
                                     .Include("Invoice")
                                     .ToList();

                   if (!apps.Any())
                   {
                       return new ApplicationObject();
                   }

                   var app = apps[0];

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
                   if (name != null)importObject.StatusStr = name.Replace("_", " ");
                   importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                   const int psf = (int)CustomColEnum.Psf;
                   const int refCode = (int)CustomColEnum.Reference_Code;

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
                                        StatusStr = Enum.GetName(typeof(DocStatus), doc.Status),Status =  doc.Status,
                                       DocumentPath = doc.DocumentPath.Replace("~", ""),
                                       DocumentTypeId = doc.DocumentType.DocumentTypeId,
                                       Name = doc.DocumentType.Name
                                   });
                               }
                           }

                       });
                   }
                   
                   var productIds = new List<long>();
                   var storageTypeIds = new List<long>();
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
                                                   RequireReferenceCode = pr.ProductColumns.Any() && pr.ProductColumns.Any(v => v.CustomCodeId == refCode),
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
                           }

                           im.ProductBankerObjects = new List<ProductBankerObject>();
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
                               var pdBnk = new ProductBankerObject
                               {
                                   Id = c.Id,
                                   ApplicationItemId = c.ApplicationItemId,
                                   BankId = c.BankId,
                                   BankName = string.IsNullOrEmpty(c.BankAccountNumber) ? c.Bank.Name : c.Bank.Name + "(" + c.BankAccountNumber + ")",
                                   BankName2 = c.Bank.Name,
                                   BankAccountNumber = c.BankAccountNumber,
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
                           productIds.Add(im.ProductId);
                           storageTypeIds.Add(im.StorageProviderTypeId);
                       }
                   });

                   importObject.DocumentTypeObjects = GetApplicationStageDocumentTypes(importObject.DocumentTypeObjects, productIds, importObject.ClassificationId, storageTypeIds);

                   return importObject;
               }

           }
           catch (Exception ex)
           {
               return new ApplicationObject();
           }
       }

       public List<DocumentTypeObject> GetApplicationStageDocumentTypes(List<DocumentTypeObject> appDocs, List<long> productids, int classId, List<long> storageProviderTypeIds)
       {
           try
           {
               const int formM = (int)SpecialDocsEnum.Form_M;
               const int appStage = (int)AppStage.Application;
               const int applicant = (int)AppRole.Applicant;

                const int pBankerDocId = (int)SpecialDocsEnum.Bank_Reference_Letter;
                const int thphDocId = (int)SpecialDocsEnum.Throughput_agreement;

               var depotOwner = ((int)AppRole.Depot_Owner).ToString();
               var banker = ((int)AppRole.Banker).ToString();
               using (var db = new ImportPermitEntities())
               {
                   productids.ForEach(o =>
                   {
                       var tempList = db.ProductDocumentRequirements.Where(w => w.ProductId == o).Include("DocumentType").ToList();
                       if (tempList.Any())
                       {
                           tempList.ForEach(p =>
                           {
                               var reqDocs = db.DocumentTypeRights.Where(g => g.DocumentTypeId == p.DocumentTypeId).Include("DocumentType").ToList();
                               if (reqDocs.Any())
                               {
                                   var existing = appDocs.Find(z => z.DocumentTypeId == p.DocumentTypeId);
                                   var r = reqDocs[0];
                                   if (r.DocumentTypeId != pBankerDocId && r.DocumentTypeId != thphDocId)
                                   {
                                       if (existing == null || existing.DocumentTypeId < 1)
                                       {
                                           appDocs.Add(new DocumentTypeObject
                                           {
                                               DocumentTypeId = p.DocumentTypeId,
                                               Uploaded = false,
                                               Name = p.DocumentType.Name,
                                               StageId = int.Parse(r.RoleId) == (int) AppRole.Applicant ? 1 : 0,
                                               IsBankDoc = r.RoleId == banker,
                                               IsDepotDoc = r.RoleId == depotOwner
                                           });
                                       }
                                       else
                                       {
                                           existing.StageId = int.Parse(r.RoleId) == applicant ? 1 : 0;
                                           existing.IsBankDoc = r.RoleId == banker;
                                           existing.IsDepotDoc = r.RoleId == depotOwner;
                                       }
                                   }
                               }
                           });
                       }
                   });

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
                               if (r.DocumentTypeId != pBankerDocId && r.DocumentTypeId != thphDocId)
                               {
                                   if (existing == null || existing.DocumentTypeId < 1)
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
                                   else
                                   {
                                       existing.StageId = int.Parse(r.RoleId) == applicant ? 1 : 0;
                                       existing.IsBankDoc = r.RoleId == banker;
                                       existing.IsDepotDoc = r.RoleId == depotOwner;
                                   }
                               }
                           }
                       });
                   }

                   storageProviderTypeIds.ForEach(storageProviderTypeId =>
                   {
                       var storageProviderReqDocs = db.StorageProviderRequirements.Where(i => i.StorageProviderTypeId == storageProviderTypeId).Include("DocumentType").ToList();
                       
                       if (storageProviderReqDocs.Any())
                       {
                           storageProviderReqDocs.ForEach(p =>
                           {
                               var reqDocs = db.DocumentTypeRights.Where(g => g.DocumentTypeId == p.DocumentTypeId).Include("DocumentType").ToList();
                               
                               if (reqDocs.Any())
                               {
                                   var existing = appDocs.Find(z => z.DocumentTypeId == p.DocumentTypeId);
                                   var r = reqDocs[0];
                                   if (r.DocumentTypeId != pBankerDocId && r.DocumentTypeId != thphDocId)
                                   {
                                       if (existing == null || existing.DocumentTypeId < 1)
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
                                       else
                                       {
                                           existing.StageId = int.Parse(r.RoleId) == applicant ? 1 : 0;
                                           existing.IsBankDoc = r.RoleId == banker;
                                           existing.IsDepotDoc = r.RoleId == depotOwner;
                                       }
                                   }
                               }
                           });
                       }
                   });

                   return !appDocs.Any() ? new List<DocumentTypeObject>() : appDocs;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
       }

       public List<int> CheckAppDocs(List<int> appDocs, List<long> productids, int classId, List<long> storageProviderTypeIds)
       {
           try
           {
               const int appStage = (int)AppStage.Application;
               var newList = new List<int>();
               using (var db = new ImportPermitEntities())
               {
                   productids.ForEach(o =>
                   {
                       var tempList = db.ProductDocumentRequirements.Where(w => w.ProductId == o).Include("DocumentType").ToList();
                       if (tempList.Any())
                       {
                           tempList.ForEach(p =>
                           {
                               var reqDocs = db.DocumentTypeRights.Where(g => g.DocumentTypeId == p.DocumentTypeId).Include("DocumentType").ToList();
                               if (reqDocs.Any())
                               {
                                   if (!appDocs.Exists(z => z == p.DocumentTypeId))
                                   {
                                       newList.Add(p.DocumentTypeId);
                                   }
                               }
                           });
                       }
                   });

                   var classificationReqDocs = db.ImportClassificationRequirements.Where(ic => ic.ClassificationId == classId && ic.ImportStageId == appStage).Include("DocumentType").ToList();
                   if (classificationReqDocs.Any())
                   {
                       classificationReqDocs.ForEach(p =>
                       {
                            var reqDocs = db.DocumentTypeRights.Where(g => g.DocumentTypeId == p.DocumentTypeId).Include("DocumentType").ToList();
                           if (reqDocs.Any())
                           {
                               if (!appDocs.Exists(z => z == p.DocumentTypeId))
                               {
                                   newList.Add(p.DocumentTypeId);
                               }
                           }
                       });
                   }

                   var impStage = db.ImportRequirements.Where(ic => ic.ImportStageId == appStage).Include("DocumentType").ToList();
                   if (impStage.Any())
                   {
                       impStage.ForEach(p =>
                       {
                           var reqDocs = db.DocumentTypeRights.Where(g => g.DocumentTypeId == p.DocumentTypeId).Include("DocumentType").ToList();
                           if (reqDocs.Any())
                           {
                               if (!appDocs.Exists(z => z == p.DocumentTypeId))
                               {
                                   newList.Add(p.DocumentTypeId);
                               }
                           }
                       });
                   }

                   storageProviderTypeIds.ForEach(storageProviderTypeId =>
                   {
                       var storageProviderReqDocs = db.StorageProviderRequirements.Where(i => i.StorageProviderTypeId == storageProviderTypeId).Include("DocumentType").ToList();

                       if (storageProviderReqDocs.Any())
                       {
                           storageProviderReqDocs.ForEach(p =>
                           {
                               var reqDocs = db.DocumentTypeRights.Where(g => g.DocumentTypeId == p.DocumentTypeId).Include("DocumentType").ToList();
                               if (reqDocs.Any())
                               {
                                   if (!appDocs.Exists(z => z == p.DocumentTypeId))
                                   {
                                       newList.Add(p.DocumentTypeId);
                                   }
                               }
                           });
                       }
                   });

                   return !newList.Any() ? new List<int>() : newList;
               }

           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<int>();
           }
       }

       public List<DocumentTypeObject> GetNotificationDocumentTypes(List<DocumentTypeObject> appDocs, List<long> productids, int classId, List<long> storageProviderTypeIds)
       {
           try
           {
               const int formM = (int)SpecialDocsEnum.Form_M;
               const int appStage = (int)AppStage.Notification;
               const int applicant = (int)AppRole.Applicant;
               var depotOwner = ((int)AppRole.Depot_Owner).ToString();
               var banker = ((int)AppRole.Banker).ToString();
               using (var db = new ImportPermitEntities())
               {
                   productids.ForEach(o => 
                   {
                       var tempList =
                           db.ProductDocumentRequirements.Where(w => w.ProductId == o).Include("DocumentType").ToList();
                       if (tempList.Any())
                       {
                           tempList.ForEach(p =>
                           {
                               var reqDocs = db.DocumentTypeRights.Where(g => g.DocumentTypeId == p.DocumentTypeId)
                                       .Include("DocumentType")
                                       .ToList();
                               if (reqDocs.Any())
                               {
                                   var existing = appDocs.Find(z => z.DocumentTypeId == p.DocumentTypeId);
                                   var r = reqDocs[0];
                                   if (existing == null || existing.DocumentTypeId < 1)
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
                                   else
                                   {
                                       existing.StageId = int.Parse(r.RoleId) == applicant ? 1 : 0;
                                       existing.IsBankDoc = r.RoleId == banker;
                                       existing.IsDepotDoc = r.RoleId == depotOwner;
                                   }
                               }
                           });
                       }
                   });

                   var classificationReqDocs =
                       db.ImportClassificationRequirements.Where(
                           ic => ic.ClassificationId == classId && ic.ImportStageId == appStage)
                           .Include("DocumentType")
                           .ToList();
                   if (classificationReqDocs.Any())
                   {
                       classificationReqDocs.ForEach(p =>
                       {
                           var reqDocs =
                               db.DocumentTypeRights.Where(g => g.DocumentTypeId == p.DocumentTypeId)
                                   .Include("DocumentType")
                                   .ToList();
                           if (reqDocs.Any())
                           {
                               var existing = appDocs.Find(z => z.DocumentTypeId == p.DocumentTypeId);
                               var r = reqDocs[0];
                               if (existing == null || existing.DocumentTypeId < 1)
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
                               else
                               {
                                   existing.StageId = int.Parse(r.RoleId) == applicant ? 1 : 0;
                                   existing.IsBankDoc = r.RoleId == banker;
                                   existing.IsDepotDoc = r.RoleId == depotOwner;
                               }
                           }
                       });
                   }

                   storageProviderTypeIds.ForEach(storageProviderTypeId =>
                   {
                       var storageProviderReqDocs =
                           db.StorageProviderRequirements.Where(i => i.StorageProviderTypeId == storageProviderTypeId)
                               .Include("DocumentType")
                               .ToList();
                       if (storageProviderReqDocs.Any())
                       {
                           storageProviderReqDocs.ForEach(p =>
                           {
                               var reqDocs =
                                   db.DocumentTypeRights.Where(g => g.DocumentTypeId == p.DocumentTypeId)
                                       .Include("DocumentType")
                                       .ToList();
                               if (reqDocs.Any())
                               {
                                   var existing = appDocs.Find(z => z.DocumentTypeId == p.DocumentTypeId);
                                   var r = reqDocs[0];
                                   if (existing == null || existing.DocumentTypeId < 1)
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
                                   else
                                   {
                                       existing.StageId = int.Parse(r.RoleId) == applicant ? 1 : 0;
                                       existing.IsBankDoc = r.RoleId == banker;
                                       existing.IsDepotDoc = r.RoleId == depotOwner;
                                   }
                               }
                           });
                       }
                   });

                   return !appDocs.Any() ? new List<DocumentTypeObject>() : appDocs;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
       }

       public bool CheckAppSubmit(long id)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var apps = db.Applications.Where(x => x.Id == id).Include("Invoice").Include("ApplicationItems").Include("ApplicationDocuments").ToList();

                   if (!apps.Any())
                   {
                       return false;
                   }


                   var app = apps[0];

                   if (app.ApplicationStatusCode < 1 || app.Invoice.Status < 1)
                   {
                       return false;
                   }
                   var unsupplied = new List<ProductBanker>();
                 
                   var appDocs = app.ApplicationDocuments.ToList();
                   var docObjects = new List<int>();
                   if (appDocs.Any())
                   {
                       appDocs.ForEach(o =>
                       {
                           var docs = db.Documents.Where(c => c.DocumentId == o.DocumentId).Include("DocumentType").ToList();
                           if (docs.Any())
                           {
                               var doc = docs[0];
                               docObjects.Add(doc.DocumentTypeId);
                           }

                       });
                   }
                   else
                   {
                       return false;
                   }

                   var sts = CheckElligibility(app.ImporterId);
                   if (!sts)
                   {
                       return false;
                   }
                   
                   var productIds = new List<long>();
                   var storageTypeIds = new List<long>();
                   var throughputs = new List<ThroughPut>();
                   const int ownDepot = (int) StorageProviderTypeEnum.Own_Depot;
                       
                   app.ApplicationItems.ToList().ForEach(u =>
                   {
                       var dets = u.ProductBankers.Where(j => j.DocumentId == null).ToList();
                       if (dets.Any())
                       {
                           unsupplied.AddRange(dets);
                       }

                       var ports = u.ThroughPuts.ToList();
                       if (!ports.Any()) return;
                       throughputs.AddRange(ports);
                       productIds.Add(u.ProductId);
                       storageTypeIds.Add(u.StorageProviderTypeId);
                   });

                   if (!throughputs.Any())
                   {
                       return false;
                   }

                   if (throughputs.Any(l => l.DocumentId == null && l.ApplicationItem.StorageProviderTypeId != ownDepot))
                   {
                       return false;
                   }

                   if (unsupplied.Any())
                   {
                       return false;
                   }

                   var allDocs = CheckAppDocs(docObjects, productIds, app.ClassificationId, storageTypeIds);

                   return !allDocs.Any();
               }
           }
           catch (Exception ex)
           {
               return false;
           }
       }
       
       public bool CheckElligibility(long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myObjList = db.StandardRequirements.Where(m => m.ImporterId == importerId).ToList();
                   if (!myObjList.Any())
                   {
                       return false;
                   }
                   var stds = db.StandardRequirementTypes.ToList();
                   if (!stds.Any())
                   {
                       return false;
                   }

                   var newList = new List<StandardRequirementTypeObject>();
                  
                   stds.ForEach(n =>
                   {
                       if (myObjList.All(k => k.StandardRequirementTypeId != n.Id))
                       {
                           newList.Add(new StandardRequirementTypeObject
                           {
                               Id = n.Id,
                               Name = n.Name,
                               IsUploaded = false
                           });
                       }
                   });

                   return newList.Count < 1;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return false;
           }
       }

       public ApplicationObject SubmitApp(long id)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var apps = db.Applications.Where(x => x.Id == id).ToList();

                if (!apps.Any())
                {
                    return new ApplicationObject();
                }

                var app = apps[0];
                if (!CheckAppSubmit(id))
                {
                    return new ApplicationObject();
                }
                

                var users = (from p in db.People.Where(p => p.ImporterId == app.ImporterId).Include("UserProfiles")
                             join usp in db.UserProfiles on p.Id equals usp.PersonId
                             join asp in db.AspNetUsers on usp.Id equals asp.UserInfo_Id
                             select new { usp, asp }).ToList();


                if (!users.Any())
                {
                    return new ApplicationObject();
                }

                var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                if (importObject == null || importObject.Id < 1)
                {
                    return new ApplicationObject();
                }

                importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                importObject.CompanyName = app.Importer.Name;
                importObject.ApplicationTypeName = app.ApplicationType.Name;
                importObject.ReferenceCode = app.Invoice.RRR;
                importObject.Rrr = app.Invoice.RRR;
                importObject.AmountPaid = app.Invoice.TotalAmountDue;
                importObject.AmountStr = app.Invoice.TotalAmountDue.ToString("n");
                importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
               
                app.ApplicationItems.ToList().ForEach(u =>
                {
                    var im = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(u);
                    if (im != null && im.Id > 0)
                    {
                        var prd = (from pr in db.Products.Where(x => x.ProductId == im.ProductId)
                                            select new ProductObject
                                            {
                                                ProductId = pr.ProductId,
                                                Code = pr.Code,
                                                Name = pr.Name,
                                                
                                            }).ToList()[0];
                        im.Code = prd.Code;
                        im.EstimatedQuantityStr = im.EstimatedQuantity.ToString("n");
                        im.EstimatedValueStr = im.EstimatedValue.ToString("n");
                        importObject.ApplicationItemObjects.Add(im);

                    }
                });
              
                var usrId = users[0].usp.Id;
                var em = users[0].asp.Email;
                importObject.UserId = usrId;
                importObject.Email = em;

                app.ApplicationStatusCode = (int)AppStatus.Submitted;
                db.Entry(app).State = EntityState.Modified;
                db.SaveChanges();

                return importObject;
             }
           }
           catch (Exception)
           {
               return new ApplicationObject();
           }
       }

       public bool UnSubmitApp(long id)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var apps = db.Applications.Where(x => x.Id == id).ToList();

                   if (!apps.Any())
                   {
                       return false;
                   }

                   var app = apps[0];
                   if (!CheckAppSubmit(id))
                   {
                       return false;
                   }
                   app.ApplicationStatusCode = (int)AppStatus.Paid;
                   db.Entry(app).State = EntityState.Modified;
                   db.SaveChanges();
                   return true;
               }
           }
           catch (Exception ex)
           {
               return false;
           }
       }

       public ApplicationObject GetAppDocuments(string code, long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var apps = (from cf in db.Banks.Where(n => n.ImporterId == importerId)
                               join pb in db.ProductBankers.Where(x => x.DocumentId != null) on cf.BankId equals pb.BankId
                               join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                               join ap in db.Applications.Where(x => x.Invoice.ReferenceCode == code || x.Invoice.RRR == code).Include("ApplicationItems")
                               .Include("Invoice").Include("ApplicationDocuments").Include("Importer") on 
                              ai.ApplicationId equals ap.Id
                              select ap).ToList();

                   if(!apps.Any())
                   {
                       return new ApplicationObject();
                   }

                   var app = apps[0];
                  
                   var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                   if (importObject == null || importObject.Id < 1)
                   {
                       return new ApplicationObject();
                   }

                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                 
                   importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                   importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                   importObject.CompanyName = app.Importer.Name;
                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                   if (name != null)importObject.StatusStr = name.Replace("_", " ");
                  importObject.ReferenceCode = app.Invoice.RRR;
                  
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");

                   importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                   app.ApplicationDocuments.ToList().ForEach(o =>
                   {
                       var docs = db.Documents.Where(c => c.DocumentId == o.DocumentId).Include("DocumentType").ToList();
                       if (docs.Any())
                       {
                           var doc = docs[0];
                           var dc = new DocumentTypeObject
                           {
                               IsFormM = doc.DocumentTypeId == (int) SpecialDocsEnum.Form_M,
                                Uploaded = true,  StatusStr = Enum.GetName(typeof(DocStatus), doc.Status),Status =  doc.Status,
                               DocumentPath = doc.DocumentPath.Replace("~",""),
                               DocumentId = doc.DocumentId,
                               DocumentTypeId = doc.DocumentType.DocumentTypeId,
                               Name = doc.DocumentType.Name
                           };
                           importObject.DocumentTypeObjects.Add(dc);
                       }

                   });

                   const int psf = (int)CustomColEnum.Psf;
                   const int refCode = (int)CustomColEnum.Reference_Code;
                   var productIds = new List<long>();
                   var storageTypeIds = new List<long>();
                   
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
                                                   RequireReferenceCode = pr.ProductColumns.Any() && pr.ProductColumns.Any(v => v.CustomCodeId == refCode),
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

                          var strpType = db.StorageProviderTypes.Where(v => v.Id == im.StorageProviderTypeId).ToList();
                          if (strpType.Any())
                          {
                              im.StorageProviderTypeName = strpType[0].Name;
                          }
                           importObject.ApplicationItemObjects.Add(im);
                           productIds.Add(im.ProductId);
                           storageTypeIds.Add(im.StorageProviderTypeId);
                       }
                   });

                   importObject.DocumentTypeObjects = GetApplicationStageDocumentTypes(importObject.DocumentTypeObjects, productIds, importObject.ClassificationId, storageTypeIds);
                   
                   return importObject;
               }
              
           }
           catch (Exception ex)
           {
               return new ApplicationObject();
           }
       }

       public ApplicationObject GetBankerAppByReference(string code, long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int paidStatus = (int) AppStatus.Paid;
                   var apps = (from cf in db.Banks.Where(n => n.ImporterId == importerId)
                               join pb in db.ProductBankers.Where(x => x.DocumentId == null) on cf.BankId equals pb.BankId
                               join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                               join ap in db.Applications.Where(d => (d.Invoice.RRR == code.Trim() || d.Invoice.ReferenceCode == code.Trim()) && (d.ApplicationStatusCode == paidStatus && d.Invoice.Status == paidStatus))
                               .Include("ApplicationItems")
                               .Include("Invoice")
                               .Include("Importer")
                               on ai.ApplicationId equals ap.Id
                               select new { ap, cf}).ToList();

                   if (!apps.Any())
                   {
                       return new ApplicationObject();
                   }
                   
                   var app = apps[0].ap;
                   var apB = apps[0].cf;
                   var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                   if (importObject == null || importObject.Id < 1)
                   {
                       return new ApplicationObject();
                   }

                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");

                   importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                   importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                   importObject.CompanyName = app.Importer.Name;
                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                   if (name != null) importObject.StatusStr = name.Replace("_", " ");
                  importObject.ReferenceCode = app.Invoice.RRR;
                   importObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");

                   const int psf = (int)CustomColEnum.Psf;
                   const int refCode = (int)CustomColEnum.Reference_Code;
                   
                   app.ApplicationItems.ToList().ForEach(u =>
                   {
                       var im = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(u);
                       if (im != null && im.Id > 0)
                       {
                           var productBankers = db.ProductBankers.Where(a => a.ApplicationItemId == im.Id && a.BankId == apB.BankId).Include("Bank").Include("Document").ToList();
                           var appCountries = db.ApplicationCountries.Where(a => a.ApplicationItemId == im.Id).Include("Country").ToList();
                           var depotList = db.ThroughPuts.Where(a => a.ApplicationItemId == im.Id).Include("Depot").ToList();
                           
                           if (appCountries.Any() && depotList.Any() && productBankers.Any())
                           {
                               im.ProductObject = (from pr in db.Products.Where(x => x.ProductId == im.ProductId).Include("ProductColumns")
                                                   select new ProductObject
                                                   {
                                                       ProductId = pr.ProductId,
                                                       Code = pr.Code,
                                                       Name = pr.Name,
                                                       RequiresPsf = pr.ProductColumns.Any() && pr.ProductColumns.Any(v => v.CustomCodeId == psf),
                                                       RequireReferenceCode = pr.ProductColumns.Any() && pr.ProductColumns.Any(v => v.CustomCodeId == refCode),
                                                       Availability = pr.Availability
                                                   }).ToList()[0];

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

                                im.ProductBankerObjects = new List<ProductBankerObject>();
                                im.ProductBankerName = "";
                                productBankers.ForEach(c =>
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

                                    if (c.Bank.ImporterId == importerId)
                                    {
                                        importObject.BankAccountNumber = !string.IsNullOrEmpty(c.BankAccountNumber) ? c.BankAccountNumber : "Unavailable";
                                    }

                                    var pdBnk = new ProductBankerObject
                                    {
                                        Id = c.Id,
                                        ApplicationItemId = c.ApplicationItemId,
                                        BankId = c.BankId,
                                        DocumentId = c.DocumentId,
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
                           }
                          var strpType = db.StorageProviderTypes.Where(v => v.Id == im.StorageProviderTypeId).ToList();
                          if (strpType.Any())
                          {
                              im.StorageProviderTypeName = strpType[0].Name;
                          }
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

       public NotificationObject GetAppBankerInfo(long productId, long importerId, string permitValue)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int paidStatus = (int)AppStatus.Paid;
                   var apps = (from cf in db.Banks.Where(n => n.ImporterId == importerId)
                               join pb in db.ProductBankers.Where(x => x.DocumentId != null) on cf.BankId equals pb.BankId
                               join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                               join ap in db.Applications.Where(a => a.Invoice.Status >= paidStatus) on ai.ApplicationId equals ap.Id
                               join ptApp in db.PermitApplications on ap.Id equals ptApp.ApplicationId
                               join nt in db.Notifications.Where(x => x.ProductId == productId && x.Permit.PermitValue == permitValue).Include("Product").Include("NotificationDocuments").Include("NotificationBankers") on ptApp.PermitId equals nt.PermitId
                               select nt).ToList();

                   if (!apps.Any())
                   {
                       return new NotificationObject();
                   }

                   var app = apps[0];

                   if (!app.NotificationBankers.Any())
                   {
                       return new NotificationObject();
                   }

                   var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                   if (importObject == null || importObject.Id < 1)
                   {
                       return new NotificationObject();
                   }

                   var appBanker = app.NotificationBankers.ToList()[0];
                   var appBankerObject = ModelMapper.Map<NotificationBanker, NotificationBankerObject>(appBanker);
                   if (appBankerObject == null || appBankerObject.Id < 1)
                   {
                       return new NotificationObject();
                   }

                   var docs = db.Documents.Where(c => c.DocumentId == appBankerObject.AttachedDocumentId).Include("DocumentType").ToList();
                   if (!docs.Any())
                   {
                       return new NotificationObject();
                   }

                   var dc = docs[0];
                   var path = dc.DocumentPath;
                   if (string.IsNullOrEmpty(path))
                   {
                       return new NotificationObject();
                   }

                   appBankerObject.DocumentObject = new DocumentObject
                   {
                       DocumentPath = dc.DocumentPath,
                       DocumentTypeId = dc.DocumentType.DocumentTypeId,
                       DocumentTypeName = dc.DocumentType.Name,
                       DocumentId = dc.DocumentId,
                       Status = dc.Status,
                       StatusStr = Enum.GetName(typeof(DocStatus), dc.Status).Replace("_", "")
                   };

                   appBankerObject.FinancedProductName = app.Product.Name;
                   importObject.NotificationBankerObjects.Add(appBankerObject);
                   
                   return importObject;
               }

           }
           catch (Exception ex)
           {
               return new NotificationObject();
           }
       }

       public ApplicationObject GetApplicationByRef(string code)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Applications.Where(m => m.Invoice.ReferenceCode == code.Trim() || m.Invoice.RRR == code)
                           .Include("Importer")
                           .Include("Invoice")
                           .Include("ApplicationItems")
                           .Include("ApplicationDocuments")
                           .Include("ApplicationType")
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

                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                   importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                   importObject.CompanyName = app.Importer.Name;
                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                   if (name != null)importObject.StatusStr = name.Replace("_", " ");
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                   var appCategory = Enum.GetName(typeof(NotificationClassEnum), importObject.ClassificationId);
                   if (appCategory != null)importObject.CategoryName = appCategory.Replace("_", " ");
                   importObject.ApplicationTypeName = app.ApplicationType.Name;
                  importObject.ReferenceCode = app.Invoice.RRR;

                   importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                   
                   importObject.DocumentTypeObjects = new List<DocumentTypeObject>();

                   app.ApplicationDocuments.ToList().ForEach(o =>
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
                                    StatusStr = Enum.GetName(typeof(DocStatus), doc.Status),Status =  doc.Status,
                                   DocumentPath = doc.DocumentPath.Replace("~", ""),
                                   DocumentTypeId = doc.DocumentType.DocumentTypeId,
                                   Name = doc.DocumentType.Name
                               });
                           }
                       }

                   });

                   const int psf = (int)CustomColEnum.Psf;
                   var productIds = new List<long>();
                   var storageTypeIds = new List<long>();

                   app.ApplicationItems.ToList().ForEach(u =>
                   {
                       var im = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(u);
                       if (im != null && im.Id > 0)
                       {
                           var productBankers = db.ProductBankers.Where(a => a.ApplicationItemId == im.Id).Include("Bank").Include("Document").ToList();
                           var appCountries = db.ApplicationCountries.Where(a => a.ApplicationItemId == im.Id).Include("Country").ToList();
                           var depotList = db.ThroughPuts.Where(a => a.ApplicationItemId == im.Id).Include("Depot").ToList();

                           if (appCountries.Any() && depotList.Any() && productBankers.Any())
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
                           productBankers.ForEach(c =>
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

                               var pdBnk = new ProductBankerObject
                               {
                                   Id = c.Id,
                                   ApplicationItemId = c.ApplicationItemId,
                                   BankId = c.BankId,
                                    BankName = string.IsNullOrEmpty(c.BankAccountNumber) ? c.Bank.Name : c.Bank.Name + "(" + c.BankAccountNumber + ")",
                                   ApplicationId = app.Id,
                                   ImporterId = app.ImporterId,
                                   ProductCode = im.ProductObject.Code,
                                   DocumentId = c.DocumentId,
                                   IsUploaded = c.DocumentId != null,
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
                           }
                           var strpType = db.StorageProviderTypes.Where(v => v.Id == im.StorageProviderTypeId).ToList();
                           if (strpType.Any())
                           {
                               im.StorageProviderTypeName = strpType[0].Name;
                           }
                           importObject.ApplicationItemObjects.Add(im);
                       
                   });

                   importObject.DocumentTypeObjects = GetApplicationStageDocumentTypes(importObject.DocumentTypeObjects, productIds, importObject.ClassificationId, storageTypeIds);
                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ApplicationObject();
           }
       }

       public ApplicationObject GetAppById(long id)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Applications.Where(m => m.Id == id)
                           .Include("Importer")
                           .Include("Invoice")
                           .Include("ApplicationItems")
                           .Include("ApplicationType")
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

                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                   importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                   importObject.CompanyName = app.Importer.Name;
                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                   if (name != null) importObject.StatusStr = name.Replace("_", " ");
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                   importObject.ApplicationTypeName = app.ApplicationType.Name;
                  importObject.ReferenceCode = app.Invoice.RRR;

                   importObject.ApplicationItemObjects = new List<ApplicationItemObject>();

                   const int psf = (int)CustomColEnum.Psf;

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

                           if (string.IsNullOrEmpty(im.PSFNumber))
                           {
                               im.PSFNumber = "Not Applicable";
                           }

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

                           im.ProductBankerObjects = new List<ProductBankerObject>();
                           var bankers = db.ProductBankers.Where(i => i.ApplicationItemId == u.Id).Include("Bank").ToList();
                           if (bankers.Any())
                           {
                               im.ProductBankerName = "";
                               bankers.ForEach(t =>
                               {
                                   im.ProductBankerObjects.Add(new ProductBankerObject
                                   {
                                       Id = t.Id,
                                       ApplicationItemId = t.ApplicationItemId,
                                       DocumentId = t.DocumentId,
                                       BankId = t.BankId,
                                       BankObject = new BankObject
                                       {
                                           BankId = t.Bank.BankId,
                                           SortCode = t.Bank.SortCode,
                                           ImporterId = t.Bank.ImporterId,
                                           Name = t.Bank.Name
                                       }
                                   });

                                   if (string.IsNullOrEmpty(im.ProductBankerName))
                                   {
                                       im.ProductBankerName = t.Bank.Name;
                                   }
                                   else
                                   {
                                       im.ProductBankerName += ", " + t.Bank.Name;
                                   }
                               });
                           }

                           var strpType = db.StorageProviderTypes.Where(v => v.Id == im.StorageProviderTypeId).ToList();
                           if (strpType.Any())
                           {
                               im.StorageProviderTypeName = strpType[0].Name;
                           }
                           importObject.ApplicationItemObjects.Add(im);
                       }
                   });
                   importObject.FeeObjects = GetAppStageFees();
                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ApplicationObject();
           }
       }

       public ApplicationObject GetAppByRef(string code, string rrr)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Applications.Where(m => m.Invoice.ReferenceCode == code.Trim())
                           .Include("Importer")
                           .Include("Invoice")
                           .Include("ApplicationItems")
                           .Include("ApplicationType")
                           .ToList();
                   if (!myApplications.Any())
                   {
                       return new ApplicationObject();
                   }

                 
                   var app = myApplications[0];
                   if (!string.IsNullOrEmpty(rrr))
                   {
                       long orderId;
                       var pRes = long.TryParse(app.Invoice.ReferenceCode, out orderId);
                       if (!pRes || orderId < 1)
                       {
                           return new ApplicationObject();
                       }

                       var invStatus = UpdateInvoceTransactionInvoce(orderId, rrr);
                       if (invStatus < 1)
                       {
                           return new ApplicationObject();
                       }
                       app.Invoice.RRR = rrr;
                       db.Entry(app.Invoice).State = EntityState.Modified;
                       db.SaveChanges();
                   }
                   
                   var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                   if (importObject == null || importObject.Id < 1)
                   {
                       return new ApplicationObject();
                   }

                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                   importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                   importObject.CompanyName = app.Importer.Name;
                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                   if (name != null) importObject.StatusStr = name.Replace("_", " ");
                   importObject.PaymentTypeId = app.Invoice.PaymentTypeId;
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                   importObject.ApplicationTypeName = app.ApplicationType.Name;
                  importObject.ReferenceCode = app.Invoice.RRR;

                   importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                   
                   const int psf = (int)CustomColEnum.Psf;
                  
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

                           if (string.IsNullOrEmpty(im.PSFNumber))
                           {
                               im.PSFNumber = "Not Applicable";
                           }

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

                           im.ProductBankerObjects = new List<ProductBankerObject>();
                           var bankers = db.ProductBankers.Where(i => i.ApplicationItemId == u.Id).Include("Bank").ToList();
                           if (bankers.Any())
                           {
                               im.ProductBankerName = "";
                               bankers.ForEach(t =>
                               {
                                   im.ProductBankerObjects.Add(new ProductBankerObject
                                   {
                                       Id = t.Id,
                                       ApplicationItemId = t.ApplicationItemId,
                                       DocumentId = t.DocumentId,
                                       BankId = t.BankId,
                                       BankObject = new BankObject
                                       {
                                           BankId = t.Bank.BankId,
                                           SortCode = t.Bank.SortCode,
                                           ImporterId = t.Bank.ImporterId,
                                           Name = t.Bank.Name
                                       }
                                   });

                                   if (string.IsNullOrEmpty(im.ProductBankerName))
                                   {
                                       im.ProductBankerName = t.Bank.Name;
                                   }
                                   else
                                   {
                                       im.ProductBankerName += ", " + t.Bank.Name;
                                   }
                               });
                           }

                           var strpType = db.StorageProviderTypes.Where(v => v.Id == im.StorageProviderTypeId).ToList();
                           if (strpType.Any())
                           {
                               im.StorageProviderTypeName = strpType[0].Name;
                           }
                           importObject.ApplicationItemObjects.Add(im);
                       }
                   });
                   importObject.FeeObjects = GetAppStageFees();
                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ApplicationObject();
           }
       }

       public ApplicationObject GetAppByRef(string code)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Applications.Where(m => m.Invoice.ReferenceCode == code.Trim() || m.Invoice.RRR == code)
                           .Include("Importer")
                           .Include("Invoice")
                           .Include("ApplicationItems")
                           .Include("ApplicationType")
                           .ToList();
                   if (!myApplications.Any())
                   {
                       return new ApplicationObject();
                   }


                   var app = myApplications[0];
                   
                    long orderId;
                    var pRes = long.TryParse(app.Invoice.ReferenceCode, out orderId);
                    if (!pRes || orderId < 1)
                    {
                        return new ApplicationObject();
                    }

                    var invStatus = UpdateInvoceTransactionInvoce(orderId);
                    if (invStatus < 1)
                    {
                        return new ApplicationObject();
                    }
                    app.Invoice.AmountPaid = app.Invoice.TotalAmountDue;
                    app.Invoice.Status = (int)PaymentStatusEnum.Paid;
                    db.Entry(app.Invoice).State = EntityState.Modified;
                    db.SaveChanges();

                    app.ApplicationStatusCode = (int)PaymentStatusEnum.Paid;
                    db.Entry(app).State = EntityState.Modified;
                    db.SaveChanges();
                   
                   var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                   if (importObject == null || importObject.Id < 1)
                   {
                       return new ApplicationObject();
                   }

                   importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                   importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                   importObject.AmountPaid = app.Invoice.TotalAmountDue;
                   importObject.CompanyName = app.Importer.Name;
                   importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                   var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                   if (name != null) importObject.StatusStr = name.Replace("_", " ");
                   importObject.PaymentTypeId = app.Invoice.PaymentTypeId;
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                   importObject.ApplicationTypeName = app.ApplicationType.Name;
                   importObject.ReferenceCode = app.Invoice.RRR;
                   importObject.StatusStr = Enum.GetName(typeof(PaymentStatusEnum), app.Invoice.PaymentTypeId);

                   importObject.ApplicationItemObjects = new List<ApplicationItemObject>();

                   const int psf = (int)CustomColEnum.Psf;

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

                           if (string.IsNullOrEmpty(im.PSFNumber))
                           {
                               im.PSFNumber = "Not Applicable";
                           }

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

                           im.ProductBankerObjects = new List<ProductBankerObject>();
                           var bankers = db.ProductBankers.Where(i => i.ApplicationItemId == u.Id).Include("Bank").ToList();
                           if (bankers.Any())
                           {
                               im.ProductBankerName = "";
                               bankers.ForEach(t =>
                               {
                                   im.ProductBankerObjects.Add(new ProductBankerObject
                                   {
                                       Id = t.Id,
                                       ApplicationItemId = t.ApplicationItemId,
                                       DocumentId = t.DocumentId,
                                       BankId = t.BankId,
                                       BankObject = new BankObject
                                       {
                                           BankId = t.Bank.BankId,
                                           SortCode = t.Bank.SortCode,
                                           ImporterId = t.Bank.ImporterId,
                                           Name = t.Bank.Name
                                       }
                                   });

                                   if (string.IsNullOrEmpty(im.ProductBankerName))
                                   {
                                       im.ProductBankerName = t.Bank.Name;
                                   }
                                   else
                                   {
                                       im.ProductBankerName += ", " + t.Bank.Name;
                                   }
                               });
                           }

                           var strpType = db.StorageProviderTypes.Where(v => v.Id == im.StorageProviderTypeId).ToList();
                           if (strpType.Any())
                           {
                               im.StorageProviderTypeName = strpType[0].Name;
                           }
                           importObject.ApplicationItemObjects.Add(im);
                       }
                   });
                   importObject.FeeObjects = GetAppStageFees();
                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ApplicationObject();
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

       public List<FeeObject> GetAppStageFees()
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int appStage = (int)AppStage.Application;
                   var fees = db.Fees.Where(k => k.ImportStageId == appStage).Include("FeeType").Include("ImportStage").ToList();
                   if (!fees.Any())
                   {
                       return new List<FeeObject>();
                   }
                   var priceVolumeThresholds = db.ImportSettings.ToList();
                   if (!priceVolumeThresholds.Any())
                   {
                       return new List<FeeObject>();
                   }

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
               return new List<FeeObject>();
           }
       }

       public long GetApplicationForRenewal(string permitNo, long id)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int expiredStatus = (int) PermitStatusEnum.Expired;
                   const int activeStatus = (int)PermitStatusEnum.Active;
                   var myApplications =  db.Permits.Where(m => m.PermitValue == permitNo.Trim() && m.ImporterId == id && m.PermitStatus >= expiredStatus && m.IssueDate <= DateTime.Today && m.ExpiryDate <= DateTime.Today).ToList();

                   if (!myApplications.Any())  
                   {
                       var stopDate = DateTime.Parse("2015-07-31");
                       if (DateTime.Today <= stopDate)
                       {
                           myApplications = db.Permits.Where(m => m.PermitValue == permitNo.Trim() && m.ImporterId == id).ToList();
                           if (!myApplications.Any())
                           {
                               return 0;
                           }
                       }
                       else
                       {
                           return 0;  
                       }
                       
                   }

                   var app = myApplications[0];

                   return app.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long GetApplicationForInclusion(string permitNo, long id)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                 const int activeStatus = (int) PermitStatusEnum.Active;
                   var myApplications = db.Permits.Where(m => m.PermitValue == permitNo.Trim() && m.ImporterId == id && m.PermitStatus == activeStatus && m.IssueDate <= DateTime.Today && m.ExpiryDate > DateTime.Today).ToList();

                   if (!myApplications.Any())
                   {
                       var stopDate = DateTime.Parse("2015-07-31");
                       if (DateTime.Today <= stopDate)
                       {
                           myApplications =
                               db.Permits.Where(
                                   m =>
                                       m.PermitValue == permitNo.Trim() && m.ImporterId == id &&
                                       m.PermitStatus == activeStatus).ToList();
                           if (!myApplications.Any())
                           {
                               return 0;
                           }
                       }
                       else
                       {
                           return 0;
                       }
                       
                   }

                   var app = myApplications[0];
                   return app.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public List<ProcessTrackingObject> SearchEmployeeAssignedJobProcesses(string searchCriteria, long employeeId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var processTrackings = (from p in
                                               db.ProcessTrackings.Where(p => p.EmployeeId == employeeId)
                                               .Include("EmployeeDesk").OrderByDescending(m => m.Id)
                                               join ap in db.Applications.Where(i => i.Invoice.RRR.Contains(searchCriteria)).Include("Invoice") on p.ApplicationId equals ap.Id
                                               join cm in db.Importers.Where(i => i.Name.Contains(searchCriteria)) on ap.ImporterId equals cm.Id
                                               join usp in db.UserProfiles.Include("Person") on p.EmployeeDesk.EmployeeId equals usp.Id
                                               join st in db.Steps.Include("Process") on p.StepId equals st.Id

                                           select new ProcessTrackingObject
                                           {
                                               Id = p.Id,
                                               ApplicationId = ap.Id,
                                               StepId = p.StepId,
                                               EmployeeId = p.EmployeeDesk.EmployeeId,
                                               StatusId = p.StatusId,
                                               AssignedTime = p.AssignedTime,
                                               DueTime = p.DueTime,
                                               ActualDeliveryDateTime = p.ActualDeliveryDateTime,
                                               StepCode = p.StepCode,
                                               OutComeCode = p.OutComeCode,
                                               EmployeeName = usp.Person.FirstName + " " + usp.Person.LastName,
                                               ProcessName = st.Process.Name,
                                               CurrentStepName = st.Name,
                                               CompanyName = cm.Name,
                                               Rrr = ap.Invoice.RRR

                                           }).ToList();

                   if (!processTrackings.Any())
                   {
                       return new List<ProcessTrackingObject>();
                   }

                   if (processTrackings.Any())
                   {
                       processTrackings.ForEach(app =>
                       {
                           app.OutComeCodeStr = app.OutComeCode != null ? Enum.GetName(typeof(OutComeCodeEnum), app.OutComeCode) : "Pending";
                           app.AssignedTimeStr = app.AssignedTime != null ? app.AssignedTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Pending";
                           app.DueTimeStr = app.DueTime != null ? app.DueTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Unavailable";
                           app.ActualDeliveryDateTimeStr = app.ActualDeliveryDateTime != null ? app.ActualDeliveryDateTime.Value.ToString("yyyy-MM-dd hh:mm: tt") : "Pending";
                       });
                   }
                   
                   return processTrackings;
               } 
               
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ProcessTrackingObject>();
           }
       }

       public List<ApplicationObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int pending = (int)AppStatus.Pending;
                   var myApplications =
                       db.Applications.Where(m => m.ApplicationStatusCode > pending &&  (m.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || m.Invoice.ReferenceCode.Contains(searchCriteria.Trim()) || m.Importer.Name.Contains(searchCriteria.Trim())))
                        .Include("Invoice")
                         .Include("Importer")
                        .ToList();

                   if (myApplications.Any())
                   {
                       var newList = new List<ApplicationObject>();
                       myApplications.OrderByDescending(g => g.DateApplied).ToList().ForEach(app =>
                       {
                           var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               importObject.ImporterStr = app.Importer.Name;
                               importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                               importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                               importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                               var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                               if (name != null)
                                   importObject.StatusStr = name.Replace("_", " ");
                               importObject.ReferenceCode = app.Invoice.RRR;
                               newList.Add(importObject);
                           }
                       });

                       return newList.OrderByDescending(k => k.Id).ToList();
                   }
               }
               return new List<ApplicationObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationObject> SearchByCompany(string searchCriteria, long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                  
                   var myApplications =
                       db.Applications.Where(m => m.ImporterId == importerId && m.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || m.Invoice.ReferenceCode.Contains(searchCriteria.Trim()))
                        .Include("ApplicationItems")
                        .Include("Invoice")
                         .Include("Importer")
                        .ToList();

                   if (myApplications.Any())
                   {
                       var newList = new List<ApplicationObject>();
                       myApplications.OrderByDescending(g => g.DateApplied).ToList().ForEach(app =>
                       {
                           var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                               importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                               importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                               var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                               if (name != null)
                                   importObject.StatusStr = name.Replace("_", " ");
                               importObject.ReferenceCode = app.Invoice.RRR;
                               importObject.ImporterStr = app.Importer.Name;
                               newList.Add(importObject);
                           }
                       });

                       return newList.OrderByDescending(k => k.Id).ToList();
                   }
               }
               return new List<ApplicationObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationObject> SearchByBankAssignedApplications(string searchCriteria, long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                           join pb in db.ProductBankers.Where(x => x.DocumentId == null) on bnk.BankId equals pb.BankId
                           join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                           join imp in
                               db.Applications.Where(c => c.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) ||  c.Importer.Name.Trim().Contains(searchCriteria.Trim()))
                                   .Include("Importer")
                                   .Include("Invoice")
                                   .Include("ApplicationType")
                                   .OrderByDescending(m => m.Id) on ai.ApplicationId equals imp.Id
                           select imp).ToList();

                   if (myApplications.Any())
                   {
                       var newList = new List<ApplicationObject>();
                       myApplications.ForEach(app =>
                       {
                           var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               importObject.ImporterStr = app.Importer.Name;
                               importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                               importObject.AppTypeStr = app.ApplicationType.Name;
                               importObject.ImportClassName = Enum.GetName(typeof (NotificationClassEnum), importObject.ClassificationId).Replace("_", " ");
                               importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                               var name = Enum.GetName(typeof (AppStatus), importObject.ApplicationStatusCode);
                               if (name != null) importObject.StatusStr = name.Replace("_", " ");
                               importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                               importObject.ReferenceCode = app.Invoice.RRR;
                               importObject.ImporterStr = app.Importer.Name;
                               importObject.ApplicationTypeName = app.ApplicationType.Name;
                               newList.Add(importObject);
                           }
                       });

                       return newList.OrderByDescending(k => k.Id).ToList();
                   }
                    return new List<ApplicationObject>();
                }
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationObject> SearchBankJobHistory(string searchCriteria, long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                                    (from bnk in db.Banks.Where(d => d.ImporterId == importerId)
                                     join pb in db.ProductBankers.Where(x => x.DocumentId != null) on bnk.BankId equals pb.BankId
                                     join ai in db.ApplicationItems on pb.ApplicationItemId equals ai.Id
                                     join imp in db.Applications.Where(c => c.Invoice.RRR.Trim().Contains(searchCriteria.Trim()) || c.Invoice.ReferenceCode.Contains(searchCriteria.Trim())).Include("Invoice").Include("Importer")
                                     on ai.ApplicationId equals imp.Id
                                     select imp).ToList();

                   if (!myApplications.Any())
                   {
                       return new List<ApplicationObject>();
                   }

                   var newList = new List<ApplicationObject>();
                   myApplications.OrderByDescending(g => g.DateApplied).ToList().ForEach(app =>
                   {
                       var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                       if (importObject != null && importObject.Id > 0)
                       {
                           importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                           importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                           importObject.ImporterStr = app.Importer.Name;
                           importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                          importObject.ReferenceCode = app.Invoice.RRR;
                           newList.Add(importObject);
                       }
                   });

                   return newList.OrderByDescending(k => k.Id).ToList();
               }

           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationObject> SearchBankUserJobHistory(string searchCriteria, long userId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                                    (from dc in db.Documents.Where(d => d.UploadedById == userId)
                                     join appDoc in db.ApplicationDocuments on dc.DocumentId equals appDoc.DocumentId
                                     join imp in db.Applications.Include("Invoice").Include("Importer") on appDoc.ApplicationId equals imp.Id
                                     where imp.Invoice.RRR.Contains(searchCriteria.Trim()) || imp.Invoice.ReferenceCode.Contains(searchCriteria.Trim())
                                     select imp).ToList();

                   if (!myApplications.Any())
                   {
                       return new List<ApplicationObject>();
                   }

                   var newList = new List<ApplicationObject>();
                   myApplications.OrderByDescending(g => g.DateApplied).ToList().ForEach(app =>
                   {
                       if (!newList.Exists(a => a.Id == app.Id))
                       {
                           var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               importObject.DerivedQuantityStr = importObject.DerivedTotalQUantity.ToString("n1").Replace(".0", "");
                               importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                               importObject.ImporterStr = app.Importer.Name;
                               importObject.LastModifiedStr = app.LastModified.ToString("dd/MM/yyyy");
                               var name = Enum.GetName(typeof(AppStatus), importObject.ApplicationStatusCode);
                               if (name != null)
                                   importObject.StatusStr = name.Replace("_", " ");
                              importObject.ReferenceCode = app.Invoice.RRR;
                               newList.Add(importObject);
                           }
                       }
                   });

                   return newList.OrderByDescending(k => k.Id).ToList();
               }

           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationObject>();
           }
       }

       public List<ApplicationItemObject> SearchDepotOwnerApplicationItems(string searchCriteria, long importerId)
       {
           try
           {
               const int paid = (int)AppStatus.Paid;
               using (var db = new ImportPermitEntities()) 
               {
                   var applicationItems = (from appObj in db.ApplicationItems.Where(k => k.Application.ApplicationStatusCode >= paid && (k.Product.Name.Contains(searchCriteria)))
                                             .Include("Product")
                                             .Include("Application")
                                             .Include("ApplicationCountries")
                                           join th in db.ThroughPuts.Where(l => l.DocumentId == null)
                                           on appObj.Id equals th.ApplicationItemId
                                           join dp in db.Depots.Where(d => d.ImporterId == importerId)
                                           on th.DepotId equals dp.Id where  dp.Name.Contains(searchCriteria)
                                           select appObj).ToList();

                   if (!applicationItems.Any())
                   {
                       return new List<ApplicationItemObject>();
                   }

                   var newList = new List<ApplicationItemObject>();
                   applicationItems.OrderByDescending(g => g.Id).ToList().ForEach(app =>
                   {
                       var importObject = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(app);
                       if (importObject != null && importObject.Id > 0)
                       {
                           var appCountries = db.ApplicationCountries.Where(a => a.ApplicationItemId == app.Id).Include("Country").ToList();
                           var depotList = db.ThroughPuts.Where(a => a.ApplicationItemId == app.Id).Include("Depot").ToList();
                           if (appCountries.Any() && depotList.Any())
                           {
                               importObject.CountryOfOriginName = "";
                               appCountries.ForEach(c =>
                               {
                                   if (string.IsNullOrEmpty(importObject.CountryOfOriginName))
                                   {
                                       importObject.CountryOfOriginName = c.Country.Name;
                                   }
                                   else
                                   {
                                       importObject.CountryOfOriginName += ", " + c.Country.Name;
                                   }
                               });

                               importObject.DischargeDepotName = "";
                               depotList.ForEach(d =>
                               {
                                   var depotName = importObject.StorageProviderTypeId == (int)StorageProviderTypeEnum.Own_Depot ? d.Depot.Name + "(" + d.Depot.DepotLicense + ")" : d.Depot.Name;

                                   if (string.IsNullOrEmpty(importObject.DischargeDepotName))
                                   {
                                       importObject.DischargeDepotName = depotName;
                                   }
                                   else
                                   {
                                       importObject.DischargeDepotName += ", " + depotName;
                                   }
                               });
                           }
                           var importers = db.Importers.Where(a => app.Application.ImporterId == a.Id).ToList();
                           if (!importers.Any())
                           {
                               return;
                           }
                           if (app.ThroughPuts.Any())
                           {
                               var dd = app.ThroughPuts.ToList()[0];
                               var docs = db.Documents.Where(d => d.DocumentId == dd.DocumentId).ToList();
                               if (!docs.Any())
                               {
                                   return;
                               }

                               var name = Enum.GetName(typeof(AppStatus), docs[0].Status);
                               if (name != null) importObject.StatusStr = name.Replace("_", " ");
                           }
                           else
                           {
                               importObject.StatusStr = AppStatus.Not_Available.ToString().Replace("_", " ");
                           }
                           importObject.ImporterName = importers[0].Name;
                           importObject.EstimatedQuantityStr = importObject.EstimatedQuantity.ToString("n1").Replace(".0", "");
                           importObject.EstimatedValueStr = importObject.EstimatedValue.ToString("n1").Replace(".0", "");
                           newList.Add(importObject);
                       }

                   });
                   return newList.OrderByDescending(k => k.Id).ToList();
               }


           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationItemObject>();
           }
       }

       public List<ApplicationItemObject> SearchDepotOwnerApplicationHistory(string searchCriteria, long importerId)
       {
           try
           {
               const int paid = (int)AppStatus.Paid;
               const int pending = (int)AppStatus.Pending;
               using (var db = new ImportPermitEntities())
               {
                   var applicationItems = (from appObj in db.ApplicationItems.Where(k => k.Application.ApplicationStatusCode >= paid && (k.Product.Name.Contains(searchCriteria)))
                                             .Include("Product")
                                             .Include("Application")
                                             .Include("ApplicationCountries")
                                           join th in db.ThroughPuts.Where(l => l.DocumentId != null && l.DocumentId > 0)
                                           on appObj.Id equals th.ApplicationItemId
                                           join dp in db.Depots.Where(d => d.ImporterId == importerId)
                                           on th.DepotId equals dp.Id
                                           where dp.Name.Contains(searchCriteria)
                                           select appObj).ToList();

                   if (!applicationItems.Any())
                   {
                       return new List<ApplicationItemObject>();
                   }

                   var newList = new List<ApplicationItemObject>();
                   applicationItems.OrderByDescending(g => g.Id).ToList().ForEach(app =>
                   {
                       var importObject = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(app);
                       if (importObject != null && importObject.Id > 0)
                       {
                           var appCountries = db.ApplicationCountries.Where(a => a.ApplicationItemId == app.Id).Include("Country").ToList();
                           var depotList = db.ThroughPuts.Where(a => a.ApplicationItemId == app.Id).Include("Depot").ToList();
                           if (appCountries.Any() && depotList.Any())
                           {
                               importObject.CountryOfOriginName = "";
                               appCountries.ForEach(c =>
                               {
                                   if (string.IsNullOrEmpty(importObject.CountryOfOriginName))
                                   {
                                       importObject.CountryOfOriginName = c.Country.Name;
                                   }
                                   else
                                   {
                                       importObject.CountryOfOriginName += ", " + c.Country.Name;
                                   }
                               });

                               importObject.DischargeDepotName = "";
                               depotList.ForEach(d =>
                               {
                                   var depotName = importObject.StorageProviderTypeId == (int)StorageProviderTypeEnum.Own_Depot ? d.Depot.Name + "(" + d.Depot.DepotLicense + ")" : d.Depot.Name;

                                   if (string.IsNullOrEmpty(importObject.DischargeDepotName))
                                   {
                                       importObject.DischargeDepotName = depotName;
                                   }
                                   else
                                   {
                                       importObject.DischargeDepotName += ", " + depotName;
                                   }
                               });
                           }
                           var importers = db.Importers.Where(a => app.Application.ImporterId == a.Id).ToList();
                           if (!importers.Any())
                           {
                               return;
                           }
                           if (app.ThroughPuts.Any())
                           {
                               var dd = app.ThroughPuts.ToList()[0];
                               var docs = db.Documents.Where(d => d.DocumentId == dd.DocumentId).ToList();
                               if (!docs.Any())
                               {
                                   return;
                               }

                               var name = Enum.GetName(typeof(AppStatus), docs[0].Status);
                               if (name != null) importObject.StatusStr = name.Replace("_", " ");
                           }
                           else
                           {
                               importObject.StatusStr = AppStatus.Not_Available.ToString().Replace("_", " ");
                           }
                           importObject.ImporterName = importers[0].Name;
                           importObject.EstimatedQuantityStr = importObject.EstimatedQuantity.ToString("n1").Replace(".0", "");
                           importObject.EstimatedValueStr = importObject.EstimatedValue.ToString("n1").Replace(".0", "");
                           newList.Add(importObject);
                       }

                   });
                   return newList.OrderByDescending(k => k.Id).ToList();
               }
               
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ApplicationItemObject>();
           }
       }

       public long DeleteApplication(long applicationId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Applications.Where(m => m.Id == applicationId).ToList();
                   if (!myApplications.Any())
                   {
                       return 0;
                   }

                   var app = myApplications[0];
                   db.Applications.Remove(app);
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
