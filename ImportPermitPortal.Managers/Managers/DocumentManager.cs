using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
   public class DocumentManager
    {

       public long AddDocument(DocumentObject document)
       {
           try
           {
               if (document == null)
               {
                   return -2;
               }

               var documentEntity = ModelMapper.Map<DocumentObject, Document>(document);
               if (documentEntity == null || string.IsNullOrEmpty(documentEntity.DocumentPath))
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var imApp = db.Documents.Add(documentEntity);
                   db.SaveChanges();


                   var app = new ApplicationDocument
                   {
                       DocumentId = imApp.DocumentId,
                       ApplicationId = document.ApplicationId
                   };

                   var dx = db.ApplicationDocuments.Add(app);
                   db.SaveChanges();

                   if (dx.ApplicationDocumentId < 1)
                   {
                       db.Documents.Remove(imApp);
                       return 0;
                   }

                   return imApp.DocumentId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long AddProductBankerDocument(DocumentObject document)
       {
           try
           {
               if (document == null)
               {
                   return -2;
               }

               var documentEntity = ModelMapper.Map<DocumentObject, Document>(document);
               if (documentEntity == null || string.IsNullOrEmpty(documentEntity.DocumentPath))
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var productBankers = db.ProductBankers.Where(b => b.ApplicationItemId == document.ApplicationItemId && b.BankId == document.BankId).ToList();
                   if (!productBankers.Any())
                   {
                       return -2;
                   }


                   var imApp = db.Documents.Add(documentEntity);
                   db.SaveChanges();

                   var bankerEntity = productBankers[0];
                   bankerEntity.DocumentId = imApp.DocumentId;
                   db.Entry(bankerEntity).State = EntityState.Modified;
                   db.SaveChanges();

                   var app = new ApplicationDocument
                   {
                       DocumentId = imApp.DocumentId,
                       ApplicationId = document.ApplicationId
                   };

                   var dx = db.ApplicationDocuments.Add(app);
                   db.SaveChanges();

                   if (dx.ApplicationDocumentId < 1)
                   {
                       db.Documents.Remove(imApp);
                       return 0;
                   }

                   return imApp.DocumentId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long AddSignDocument(SignOffDocumentObject document)
       {
           try
           {
               if (document == null)
               {
                   return -2;
               }

               var documentEntity = ModelMapper.Map<SignOffDocumentObject, SignOffDocument>(document);
               if (documentEntity == null || string.IsNullOrEmpty(documentEntity.DocumentPath) || documentEntity.ApplicationId < 1)
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var imApp = db.SignOffDocuments.Add(documentEntity);
                   db.SaveChanges();
                   return imApp.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long AddNotificationDocument(DocumentObject document)
       {
           try
           {
               if (document == null)
               {
                   return -2;
               }

               var documentEntity = ModelMapper.Map<DocumentObject, Document>(document);
               if (documentEntity == null || string.IsNullOrEmpty(documentEntity.DocumentPath))
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var ntDoc = db.Documents.Add(documentEntity);
                   db.SaveChanges();


                   var app = new NotificationDocument
                   {
                       DocumentId = ntDoc.DocumentId,
                       NotificationId = document.NotificationId,
                       DateAttached = document.DateUploaded,
                       Comment = document.Comment
                   };

                   var dx = db.NotificationDocuments.Add(app);
                   db.SaveChanges();

                   if (dx.Id < 1)
                   {
                       db.Documents.Remove(ntDoc);
                       return 0;
                   }

                   return ntDoc.DocumentId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long SaveNotificationDocumentUpdateDepot(DocumentObject document)
       {
           try
           {
               if (document == null)
               {
                   return -2;
               }

               var documentEntity = ModelMapper.Map<DocumentObject, Document>(document);
               if (documentEntity == null || string.IsNullOrEmpty(documentEntity.DocumentPath))
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var ntDoc = db.Documents.Add(documentEntity);
                   db.SaveChanges();
                   var notifications = db.Notifications.Where(x => x.Id == document.NotificationId).ToList();

                   if (!notifications.Any())
                   {
                       return -2;
                   }
                   
                   var app = new NotificationDocument
                   {
                       DocumentId = ntDoc.DocumentId,
                       NotificationId = document.NotificationId,
                       DateAttached = document.DateUploaded,
                       Comment = document.Comment
                   };

                   var dx = db.NotificationDocuments.Add(app);
                   db.SaveChanges();

                   if (dx.Id < 1)
                   {
                       db.Documents.Remove(ntDoc);
                       return 0;
                   }

                   var notification = notifications[0];
                   notification.DischargeDepotId = document.DepotId;
                   db.Entry(notification).State = EntityState.Modified;
                   db.SaveChanges();

                   return ntDoc.DocumentId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public long AddFormM(FormMDetailObject formM)
       {
           try
           {
               if (formM == null)
               {
                   return -2;
               }

               var formMEntity = ModelMapper.Map<FormMDetailObject, FormMDetail>(formM);
               if (formMEntity == null )
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                    var documentEntity = ModelMapper.Map<DocumentObject, Document>(formM.DocumentObject);
                    if (string.IsNullOrEmpty(documentEntity.DocumentPath))
                    {
                        return -2;
                    }
                    var ntDoc = db.Documents.Add(documentEntity);
                    db.SaveChanges();


                    var app = new NotificationDocument
                    {
                        DocumentId = ntDoc.DocumentId,
                        NotificationId = formM.DocumentObject.NotificationId,
                        DateAttached = formM.DocumentObject.DateUploaded,
                        Comment = formM.DocumentObject.Comment
                    };

                    db.NotificationDocuments.Add(app);
                    db.SaveChanges();

                    formMEntity.AttachedDocumentId = ntDoc.DocumentId;
                   var imApp = db.FormMDetails.Add(formMEntity);
                   db.SaveChanges();
                   
                   return imApp.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateDocument(DocumentObject document)
       {
           try
           {
               if (document == null)
               {
                   return -2;
               }

               var documentEntity = ModelMapper.Map<DocumentObject, Document>(document);
               if (documentEntity == null || documentEntity.DocumentId < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.Documents.Attach(documentEntity);
                   db.Entry(documentEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return document.DocumentId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateBankerDocument(DocumentObject document)
       {
           try
           {
               if (document == null)
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   var documentEntities = db.Documents.Where(d => d.DocumentId == document.DocumentId).ToList();
                   if (!documentEntities.Any())
                   {
                       return -2;
                   }
                   var documentEntity = documentEntities[0];

                   documentEntity.DateUploaded = document.DateUploaded;
                   documentEntity.UploadedById = document.UploadedById;
                   documentEntity.DocumentPath = document.DocumentPath;
                   db.Entry(documentEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return documentEntity.DocumentId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateFormM(FormMDetailObject formM)
       {
           try
           {
               if (formM == null)
               {
                   return -2;
               }

               var formMEntity = ModelMapper.Map<FormMDetailObject, FormMDetail>(formM);
               if (formMEntity == null)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                    var oldFormMs = db.FormMDetails.Where(v => v.Id == formM.Id).ToList();
                    if (!oldFormMs.Any())
                    {
                        return -2;
                    }

                    var docs = db.Documents.Where(v => v.DocumentId == formM.AttachedDocumentId).ToList();
                   if (!docs.Any())
                   {
                       return -2;
                   }
                   var doc = docs[0];
                   if (doc.DocumentPath != formM.DocumentPath)
                   {
                        doc.DocumentPath = formM.DocumentPath;
                        doc.DateUploaded = formM.DateAttached;
                        db.Entry(doc).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                   var formMDetail = oldFormMs[0];
                   formMDetail.FormMReference = formM.FormMReference;
                   formMDetail.Quantity = formM.Quantity;
                   formMDetail.LetterOfCreditNo = formM.LetterOfCreditNo;
                   formMDetail.AttachedDocumentId = formM.AttachedDocumentId;
                   formMDetail.DateAttached = formM.DateAttached;
                   db.Entry(formMEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return formMEntity.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<DocumentObject> GetDocuments()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var documents = db.Documents.ToList();
                   if (!documents.Any())
                   {
                        return new List<DocumentObject>();
                   }
                   var objList =  new List<DocumentObject>();
                   documents.ForEach(app =>
                   {
                       var documentObject = ModelMapper.Map<Document, DocumentObject>(app);
                       if (documentObject != null && documentObject.DocumentId > 0)
                       {
                           objList.Add(documentObject);
                       }
                   });

                   return !objList.Any() ? new List<DocumentObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

       public List<DocumentObject> GetDocuments(long appId, long ImporterId, bool isBanker)
        {
           try
           {
                   using (var db = new ImportPermitEntities())
                   {
                       List<DocumentObject> myDocuments;
                       if (appId > 0)
                       {
                           myDocuments = (from ap in db.ApplicationDocuments.Where(x => x.ApplicationId == appId)
                                          join d in db.Documents.Where(m => m.ImporterId == ImporterId).Include("DocumentType").Include("Importer") on ap.DocumentId equals d.DocumentId
                                          join app in db.Applications on ap.ApplicationId equals app.Id
                                          select new DocumentObject
                                          {
                                              DocumentId = d.DocumentId,
                                              DocumentTypeId = d.DocumentTypeId,
                                              ImporterId = d.ImporterId,
                                              DateUploaded = d.DateUploaded,
                                              Status = d.Status,
                                              UploadedById = d.UploadedById,
                                              DocumentPath = d.DocumentPath,
                                              CompanyName = d.Importer.Name,
                                              DocumentTypeName = d.DocumentType.Name,
                                          }).ToList();
                       }
                       else
                       {
                           if (isBanker)
                           {
                               myDocuments = (from ap in db.ApplicationDocuments
                                              join d in db.Documents.Include("DocumentType").Include("Importer") on ap.DocumentId equals d.DocumentId
                                              join app in db.Applications on ap.ApplicationId equals app.Id
                                              select new DocumentObject
                                              {
                                                  DocumentId = d.DocumentId,
                                                  DocumentTypeId = d.DocumentTypeId,
                                                  ImporterId = d.ImporterId,
                                                  DateUploaded = d.DateUploaded,
                                                  Status = d.Status,
                                                  UploadedById = d.UploadedById,
                                                  DocumentPath = d.DocumentPath,
                                                  CompanyName = d.Importer.Name,
                                                  DocumentTypeName = d.DocumentType.Name,
                                              }).ToList();
                           }
                           else
                           {
                               myDocuments = (from ap in db.ApplicationDocuments
                                              join d in db.Documents.Where(m => m.ImporterId == ImporterId).Include("DocumentType").Include("Importer") on ap.DocumentId equals d.DocumentId
                                              join app in db.Applications on ap.ApplicationId equals app.Id
                                              select new DocumentObject
                                              {
                                                  DocumentId = d.DocumentId,
                                                  DocumentTypeId = d.DocumentTypeId,
                                                  ImporterId = d.ImporterId,
                                                  DateUploaded = d.DateUploaded,
                                                  Status = d.Status,
                                                  UploadedById = d.UploadedById,
                                                  DocumentPath = d.DocumentPath,
                                                  CompanyName = d.Importer.Name,
                                                  DocumentTypeName = d.DocumentType.Name,
                                              }).ToList();
                           }
                       }
                       

                       if (!myDocuments.Any())
                       {
                            return new List<DocumentObject>();
                       }

                       myDocuments.ForEach(m =>
                       {
                           m.DateUploadedStr = m.DateUploaded.ToString("dd/MM/yyyy");
                           m.StatusStr = Enum.GetName(typeof(DocStatus), m.Status);
                       });
                   }
               return new List<DocumentObject>();
           }
           catch (Exception ex)
           {
              return new List<DocumentObject>();
           }
       }

       public List<DocumentObject> GetDocuments(long ImporterId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                  var  myDocuments = (from ap in db.ApplicationDocuments
                                      join d in db.Documents.Where(m => m.ImporterId == ImporterId).Include("DocumentType").Include("Importer") on ap.DocumentId equals d.DocumentId
                                      join app in db.Applications on ap.ApplicationId equals app.Id
                                      select new DocumentObject
                                      {
                                          DocumentId = d.DocumentId,
                                          DocumentTypeId = d.DocumentTypeId,
                                          ImporterId = d.ImporterId,
                                          DateUploaded = d.DateUploaded,
                                          Status = d.Status,
                                          UploadedById = d.UploadedById,
                                          DocumentPath = d.DocumentPath,
                                          DocumentTypeName = d.DocumentType.Name,
                                          CompanyName = d.Importer.Name,
                                          ApplicationId = app.Id
                                      }).ToList();
                  
                   

                   if (!myDocuments.Any())
                   {
                       return new List<DocumentObject>();
                   }

                   myDocuments.ForEach(m =>
                   {
                       m.DateUploadedStr = m.DateUploaded.ToString("dd/MM/yyyy");
                       m.StatusStr = Enum.GetName(typeof(DocStatus), m.Status);
                   });
               }
               return new List<DocumentObject>();
           }
           catch (Exception ex)
           {
               return new List<DocumentObject>();
           }
       }
       public DocumentObject GetDocument(long documentId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var documents = (from d in db.Documents.Where(m => m.DocumentId == documentId).Include("DocumentType").Include("Importer")
                                      join ap in db.ApplicationDocuments on d.DocumentId equals ap.DocumentId
                                      join app in db.Applications on ap.ApplicationId equals app.Id
                                      select new DocumentObject
                                      {
                                          DocumentId = d.DocumentId,
                                          DocumentTypeId = d.DocumentTypeId,
                                          ImporterId = d.ImporterId,
                                          DateUploaded = d.DateUploaded,
                                          Status = d.Status,
                                          UploadedById = d.UploadedById,
                                          DocumentPath = d.DocumentPath,
                                          DocumentTypeName = d.DocumentType.Name,
                                          CompanyName = d.Importer.Name,
                                          ApplicationId = app.Id
                                      }).ToList();
                    if (!documents.Any())
                    {
                        return new DocumentObject();
                    }
                    var doc = documents[0];
                    doc.DateUploadedStr = doc.DateUploaded.ToString("dd/MM/yyyy");
                    doc.StatusStr = Enum.GetName(typeof(DocStatus), doc.Status);
                    return doc;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new DocumentObject();
           }
       }

       public FormMDetailObject GetFormMByProduct(long notificationId)
       {
           try
           { 
               using (var db = new ImportPermitEntities())
               {
                   var documents = (from d in db.FormMDetails.Where(m => m.NotificationId == notificationId).Include("Document").Include("Notification")
                                    select new FormMDetailObject
                                    {
                                        Id  =  d.Id,
                                        NotificationId = d.NotificationId,
                                        DateIssued  =  d.DateIssued,
                                        FormMReference  =  d.FormMReference,
                                        Quantity  =  d.Quantity,
                                        LetterOfCreditNo  =  d.LetterOfCreditNo,
                                        AttachedDocumentId  =  d.AttachedDocumentId,
                                        DateAttached  =  d.DateAttached,
                                        DocumentObject = new DocumentObject
                                        {
                                            DocumentId = d.Document.DocumentId,
                                            DocumentTypeId = d.Document.DocumentTypeId,
                                            ImporterId = d.Document.ImporterId,
                                            DateUploaded = d.Document.DateUploaded,
                                            Status = d.Document.Status,
                                            UploadedById = d.Document.UploadedById,
                                            DocumentPath = d.Document.DocumentPath,
                                        }
                                    }).ToList();
                   if (!documents.Any())
                   {
                       return new FormMDetailObject();
                   }

                   var doc = documents[0];
                   var products = db.Notifications.Where(n => n.Id == doc.NotificationId).Include("Product").ToList();
                   if (!products.Any())
                   {
                       return new FormMDetailObject();
                   }
                   var product = products[0].Product;
                   doc.ProductObject = new ProductObject
                   {
                       ProductId = product.ProductId,
                       Code = product.Code,
                       Name = product.Name,
                       Availability = product.Availability,
                   };
                   doc.DateAttachedStr = doc.DateAttached.ToString("dd/MM/yyyy");
                   doc.DateIssuedStr = doc.DateIssued.ToString("dd/MM/yyyy");
                   return doc;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new FormMDetailObject();
           }
       }
       public List<DocumentObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var documents =
                       (from ap in db.ApplicationDocuments
                           join d in
                               db.Documents.Where(
                                   m => m.DateUploaded.ToString(CultureInfo.InvariantCulture) == searchCriteria)
                                   .Include("DocumentType").Include("Importer") on ap.DocumentId equals d.DocumentId
                           join app in db.Applications on ap.ApplicationId equals app.Id
                           select new DocumentObject
                           {
                               DocumentId = d.DocumentId,
                               DocumentTypeId = d.DocumentTypeId,
                               ImporterId = d.ImporterId,
                               DateUploaded = d.DateUploaded,
                               Status = d.Status,
                               UploadedById = d.UploadedById,
                               DocumentPath = d.DocumentPath,
                               DocumentTypeName = d.DocumentType.Name,
                               CompanyName = d.Importer.Name,
                               ApplicationId = app.Id
                           }).ToList();

                   if (!documents.Any())
                   {
                      return new List<DocumentObject>();
                   }

                   documents.ForEach(m =>
                   {
                       m.DateUploadedStr = m.DateUploaded.ToString("dd/MM/yyyy");
                       m.StatusStr = Enum.GetName(typeof(DocStatus), m.Status);
                   });
                   return documents;
               }
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<DocumentObject>();
           }
       }

       public long DeleteDocument(long documentId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.Documents.Where(m => m.DocumentId == documentId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.Documents.Remove(item);
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
