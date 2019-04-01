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
   public class DocumentTypeManager
    {
       public long AddDocumentType(DocumentTypeObject documentType)
       {
           try
           {
               if (documentType == null)
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   if (db.DocumentTypes.Count(g =>g.Name.ToLower().Trim().Replace(" ", "") == documentType.Name.ToLower().Trim().Replace(" ", "")) > 0)
                   {
                       return -3;
                   }
                   var documentTypeEntity = ModelMapper.Map<DocumentTypeObject, DocumentType>(documentType);
                   if (documentTypeEntity == null || string.IsNullOrEmpty(documentTypeEntity.Name))
                   {
                       return -2;
                   }
                   var returnStatus = db.DocumentTypes.Add(documentTypeEntity);
                   db.SaveChanges();
                   return returnStatus.DocumentTypeId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateDocumentType(DocumentTypeObject documentType)
       {
           try
           {
               if (documentType == null)
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   if (db.DocumentTypes.Count(g => g.Name.ToLower().Trim().Replace(" ", "") == documentType.Name.ToLower().Trim().Replace(" ", "") && g.DocumentTypeId != documentType.DocumentTypeId) > 0)
                   {
                       return -3;
                   }
                   var documentTypeEntity = ModelMapper.Map<DocumentTypeObject, DocumentType>(documentType);
                   if (documentTypeEntity == null || documentTypeEntity.DocumentTypeId < 1)
                   {
                       return -2;
                   }
                   db.DocumentTypes.Attach(documentTypeEntity);
                   db.Entry(documentTypeEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return documentType.DocumentTypeId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<DocumentTypeObject> GetDocumentTypes()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var documentTypes = db.DocumentTypes.ToList();
                   if (!documentTypes.Any())
                   {
                        return new List<DocumentTypeObject>();
                   }
                   var objList =  new List<DocumentTypeObject>();
                   documentTypes.ForEach(app =>
                   {
                       var documentTypeObject = ModelMapper.Map<DocumentType, DocumentTypeObject>(app);
                       if (documentTypeObject != null && documentTypeObject.DocumentTypeId > 0)
                       {
                           if (!objList.Exists(v => v.DocumentTypeId == documentTypeObject.DocumentTypeId))
                           {
                               objList.Add(documentTypeObject);
                           }
                       }
                   });

                   return !objList.Any() ? new List<DocumentTypeObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

       public List<DocumentTypeObject> GetApplicationStageDocumentTypes(RequirementProp requirementProp)
       {
           try
           { 
               using (var db = new ImportPermitEntities())
               {
                   var docs = new List<DocumentTypeObject>();
                   requirementProp.ProductIds.ForEach(o =>
                   {
                       var tempList = db.ProductDocumentRequirements.Where(w => w.ProductId == o).Include("DocumentType").ToList();
                       if (tempList.Any())
                       {
                          tempList.ForEach(p =>
                          {
                              if (!docs.Exists(z => z.DocumentTypeId == p.DocumentTypeId))
                              {
                                  docs.Add(new DocumentTypeObject
                                  {
                                      DocumentTypeId = p.DocumentTypeId,
                                      Name = p.DocumentType.Name
                                  });
                              }
                          });
                       }
                   });

                   const int appStage = (int)AppStage.Application;
                   var classificationReqDocs = db.ImportClassificationRequirements.Where(ic => ic.ClassificationId == requirementProp.ImportClassId && ic.ImportStageId == appStage).Include("DocumentType").ToList();
                   if (classificationReqDocs.Any())
                   {
                       classificationReqDocs.ForEach(r =>
                       {
                           docs.Add(new DocumentTypeObject
                           {
                               DocumentTypeId = r.DocumentTypeId,
                               Name = r.DocumentType.Name
                           });
                       });
                   }

                   requirementProp.StorageProviderTypeIds.ForEach(o =>
                   {
                       var storageProviderReqDocs = db.StorageProviderRequirements.Where(i => i.StorageProviderTypeId == o).Include("DocumentType").ToList();
                       if (storageProviderReqDocs.Any())
                       {
                           var p = storageProviderReqDocs[0];
                           if (!docs.Exists(z => z.DocumentTypeId == p.DocumentTypeId))
                           {
                               docs.Add(new DocumentTypeObject
                               {
                                   DocumentTypeId = p.DocumentTypeId,
                                   Name = p.DocumentType.Name
                               });
                           }
                       }
                   });

                   return !docs.Any() ? new List<DocumentTypeObject>() : docs;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
       }

       public List<DocumentTypeObject> GetNotificationStageDocumentTypes(int importClassId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var docs = new List<DocumentTypeObject>();
                   const int notificationStage = (int)AppStage.Notification;
                   var notificationReqDocs = db.ImportClassificationRequirements.Where(ic => ic.ClassificationId == importClassId && ic.ImportStageId == notificationStage).Include("DocumentType").ToList();
                   if (notificationReqDocs.Any())
                   {
                       notificationReqDocs.ForEach(r =>
                       {
                           docs.Add(new DocumentTypeObject
                           {
                               DocumentTypeId = r.DocumentTypeId,
                               Name = r.DocumentType.Name
                           });
                       });
                   }
                   return !docs.Any() ? new List<DocumentTypeObject>() : docs;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
       }

       public List<DocumentTypeObject> GetDocumentTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var documentTypes =
                           db.DocumentTypes.OrderByDescending(m => m.DocumentTypeId)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (documentTypes.Any())
                       {
                           var newList = new List<DocumentTypeObject>();
                           documentTypes.ForEach(app =>
                           {
                               var documentTypeObject = ModelMapper.Map<DocumentType, DocumentTypeObject>(app);
                               if (documentTypeObject != null && documentTypeObject.DocumentTypeId > 0)
                               {
                                   if (!newList.Exists(v => v.DocumentTypeId == documentTypeObject.DocumentTypeId))
                                   {
                                       newList.Add(documentTypeObject);
                                   }
                               }
                           });
                           countG = db.DocumentTypes.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<DocumentTypeObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<DocumentTypeObject>();
           }
       }
       
       public DocumentTypeObject GetDocumentType(long documentTypeId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var documentTypes =
                        db.DocumentTypes.Where(m => m.DocumentTypeId == documentTypeId)
                            .ToList();
                    if (!documentTypes.Any())
                    {
                        return new DocumentTypeObject();
                    }

                    var app = documentTypes[0];
                    var documentTypeObject = ModelMapper.Map<DocumentType, DocumentTypeObject>(app);
                    if (documentTypeObject == null || documentTypeObject.DocumentTypeId < 1)
                    {
                        return new DocumentTypeObject();
                    }
                    
                  return documentTypeObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new DocumentTypeObject();
           }
       }

       public List<DocumentTypeObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var documentTypes =
                       db.DocumentTypes.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (documentTypes.Any())
                   {
                       var newList = new List<DocumentTypeObject>();
                       documentTypes.ForEach(app =>
                       {
                           var documentTypeObject = ModelMapper.Map<DocumentType, DocumentTypeObject>(app);
                           if (documentTypeObject != null && documentTypeObject.DocumentTypeId > 0)
                           {
                               if (!newList.Exists(v => v.DocumentTypeId == documentTypeObject.DocumentTypeId))
                               {
                                   newList.Add(documentTypeObject);
                               }
                           }
                       });

                       return newList;
                   }
               }
               return new List<DocumentTypeObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<DocumentTypeObject>();
           }
       }

       public long DeleteDocumentType(long documentTypeId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.DocumentTypes.Where(m => m.DocumentTypeId == documentTypeId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.DocumentTypes.Remove(item);
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
