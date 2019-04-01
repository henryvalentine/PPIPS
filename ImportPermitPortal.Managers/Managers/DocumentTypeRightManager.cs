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
   public class DocumentTypeRightManager
    {

       public long AddDocumentTypeRight(DocumentTypeRightObject documentTypeRight)
       {
           try
           {
               if (documentTypeRight == null)
               {
                   return -2;
               }

               var documentTypeRightEntity = ModelMapper.Map<DocumentTypeRightObject, DocumentTypeRight>(documentTypeRight);
               if (documentTypeRightEntity == null || documentTypeRightEntity.DocumentTypeId < 1)
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.DocumentTypeRights.Add(documentTypeRightEntity);
                   db.SaveChanges();
                   return returnStatus.DocumentTypeRightId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateDocumentTypeRight(DocumentTypeRightObject documentTypeRight)
       {
           try
           {
               if (documentTypeRight == null)
               {
                   return -2;
               }

               var documentTypeRightEntity = ModelMapper.Map<DocumentTypeRightObject, DocumentTypeRight>(documentTypeRight);
               if (documentTypeRightEntity == null || documentTypeRightEntity.DocumentTypeRightId < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.DocumentTypeRights.Attach(documentTypeRightEntity);
                   db.Entry(documentTypeRightEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return documentTypeRight.DocumentTypeRightId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long AddDocumentTypeRights(List<DocumentTypeRightObject> documentTypeRights)
       {
           try
           {
               if (!documentTypeRights.Any())
               {
                   return -2;
               }

               var successCount = 0;
               
               using (var db = new ImportPermitEntities())
               {
                   documentTypeRights.ForEach(g =>
                   {
                       if (db.DocumentTypeRights.Count(m => m.DocumentTypeId == g.DocumentTypeId && m.RoleId == g.RoleId) < 1)
                       {
                           var docReqEntity = ModelMapper.Map<DocumentTypeRightObject, DocumentTypeRight>(g);
                           if (docReqEntity != null && docReqEntity.DocumentTypeId > 0 && !string.IsNullOrEmpty(docReqEntity.RoleId))
                           {
                               db.DocumentTypeRights.Add(docReqEntity);
                               db.SaveChanges();
                               successCount += 1;
                           }
                       }
                   });
                   if (successCount > 0 && successCount != documentTypeRights.Count)
                   {
                       return 2;
                   }
                   return 5;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateDocumentTypeRights(List<DocumentTypeRightObject> documentTypeRights, List<DocumentTypeRightObject> newReqs)
       {
           try
           {
               if (!documentTypeRights.Any() || !newReqs.Any())
               {
                   return -2;
               }
               var successCount = 0;
               using (var db = new ImportPermitEntities())
               {
                    newReqs.ForEach(k =>
                    {
                        if (!documentTypeRights.Exists(n => n.DocumentTypeId == k.DocumentTypeId))
                        {
                            if (db.DocumentTypeRights.Count(m => m.DocumentTypeId == k.DocumentTypeId && m.RoleId == k.RoleId) < 1)
                            {
                                var docReqEntity = ModelMapper.Map<DocumentTypeRightObject, DocumentTypeRight>(k);
                                if (docReqEntity != null && docReqEntity.DocumentTypeId > 0 &&
                                    !string.IsNullOrEmpty(docReqEntity.RoleId))
                                {
                                    db.DocumentTypeRights.Add(docReqEntity);
                                    db.SaveChanges();
                                    successCount += 1;
                                }
                            }
                        }
                        else
                        {
                            successCount += 1;
                        }
                    });
                    if (documentTypeRights.Any())
                    {
                        documentTypeRights.ForEach(c =>
                        {
                            if (!newReqs.Exists(n => n.DocumentTypeId == c.DocumentTypeId))
                            {
                                var reqsToRemove = db.DocumentTypeRights.Where(m => m.DocumentTypeId == c.DocumentTypeId).ToList();
                                if (reqsToRemove.Any())
                                {
                                    var item = reqsToRemove[0];
                                    db.DocumentTypeRights.Remove(item);
                                    db.SaveChanges();
                                }
                            }
                        });
                    }
                   
                   if (successCount > 0 && successCount != documentTypeRights.Count)
                   {
                       return 2;
                   }
                   return 5;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       
       public List<DocumentTypeRightObject> GetDocumentTypeRightsByRole(string roleId)
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var documentTypeRights = db.DocumentTypeRights.Where(m => m.RoleId == roleId)
                        .Include("DocumentType")
                        .Include("AspNetRole")
                        .ToList();
                   if (!documentTypeRights.Any())
                   {
                        return new List<DocumentTypeRightObject>();
                   }
                   var objList =  new List<DocumentTypeRightObject>();
                   documentTypeRights.ForEach(app =>
                   {
                       var documentTypeRightObject = ModelMapper.Map<DocumentTypeRight, DocumentTypeRightObject>(app);
                       if (documentTypeRightObject != null && documentTypeRightObject.DocumentTypeRightId > 0)
                       {
                           documentTypeRightObject.RoleName = app.AspNetRole.Name;
                           documentTypeRightObject.DocumentTypeName = app.DocumentType.Name;
                           objList.Add(documentTypeRightObject);
                       }
                   });

                   return !objList.Any() ? new List<DocumentTypeRightObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

       public List<DocumentTypeRightObject> GetDocumentRightsByRole(string roleId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var documentTypeRights = db.DocumentTypeRights.Where(m => m.RoleId == roleId)
                        .Include("DocumentType")
                        .Include("AspNetRole")
                        .ToList();
                   if (!documentTypeRights.Any())
                   {
                       return new List<DocumentTypeRightObject>();
                   }
                   var objList = new List<DocumentTypeRightObject>();
                   documentTypeRights.ForEach(app =>
                   {
                       var documentTypeRightObject = ModelMapper.Map<DocumentTypeRight, DocumentTypeRightObject>(app);
                       if (documentTypeRightObject != null && documentTypeRightObject.DocumentTypeRightId > 0)
                       {
                           documentTypeRightObject.DocumentTypeName = app.DocumentType.Name;
                           documentTypeRightObject.RoleName = app.AspNetRole.Name.Replace("_", " ");
                           objList.Add(documentTypeRightObject);
                       }
                   });

                   return !objList.Any() ? new List<DocumentTypeRightObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
       }

       public List<DocumentTypeObject> GetDocumentRightsByRoles()
       {
           try
           {
               var banker = ((int)AppRole.Banker).ToString();
               var applicant = ((int)AppRole.Applicant).ToString();
               var depotOwner = ((int)AppRole.Depot_Owner).ToString();

               using (var db = new ImportPermitEntities())
               {
                   var documentTypeRights = db.DocumentTypeRights.Where(m => m.RoleId == banker || m.RoleId == applicant || m.RoleId == depotOwner)
                        .Include("DocumentType")
                        .ToList();
                   if (!documentTypeRights.Any())
                   {
                       return new List<DocumentTypeObject>();
                   }
                   var objList = new List<DocumentTypeObject>();
                   documentTypeRights.ForEach(app =>
                   {
                       var doc = new DocumentTypeObject()
                       {
                           Name = app.DocumentType.Name,
                           DocumentTypeId = app.DocumentType.DocumentTypeId
                       };

                       objList.Add(doc);
                       
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

       public List<DocumentTypeRightObject> GetDocumentTypeRightsByDocumentType(int documentTypeId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var documentTypeRights = db.DocumentTypeRights.Where(m => m.DocumentTypeId == documentTypeId)
                        .Include("DocumentType")
                        .Include("AspNetRole")
                        .ToList();
                   if (!documentTypeRights.Any())
                   {
                       return new List<DocumentTypeRightObject>();
                   }
                   var objList = new List<DocumentTypeRightObject>();
                   documentTypeRights.ForEach(app =>
                   {
                       var documentTypeRightObject = ModelMapper.Map<DocumentTypeRight, DocumentTypeRightObject>(app);
                       if (documentTypeRightObject != null && documentTypeRightObject.DocumentTypeRightId > 0)
                       {
                           documentTypeRightObject.RoleName = app.AspNetRole.Name;
                           documentTypeRightObject.DocumentTypeName = app.DocumentType.Name;
                           objList.Add(documentTypeRightObject);
                       }
                   });

                   return !objList.Any() ? new List<DocumentTypeRightObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
       }

        public List<DocumentTypeRightObject> GetDocumentTypeRights(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var documentTypeRights =
                           db.DocumentTypeRights
                           .Include("DocumentType")
                        .Include("AspNetRole").OrderByDescending(m => m.DocumentTypeRightId)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (documentTypeRights.Any())
                       {
                           var newList = new List<DocumentTypeRightObject>();
                           documentTypeRights.ForEach(k =>
                           {
                               var req = newList.Find(m => m.RoleId == k.RoleId);

                               if (req != null && req.DocumentTypeRightId > 0)
                               {
                                   req.DocumentTypeName += ", " + k.DocumentType.Name;
                               }
                               else
                               {
                                   var documentTypeRightObject = ModelMapper.Map<DocumentTypeRight, DocumentTypeRightObject>(k);
                                   if (documentTypeRightObject != null && documentTypeRightObject.DocumentTypeRightId > 0)
                                   {
                                       documentTypeRightObject.RoleName = k.AspNetRole.Name.Replace("_", " ");
                                       documentTypeRightObject.DocumentTypeName = k.DocumentType.Name;
                                       newList.Add(documentTypeRightObject);
                                   }
                               }
                               
                           });
                           countG = db.DocumentTypeRights.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<DocumentTypeRightObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<DocumentTypeRightObject>();
           }
       }
       
        public DocumentTypeRightObject GetDocumentTypeRight(long documentTypeRightId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var documentTypeRights =
                        db.DocumentTypeRights.Where(m => m.DocumentTypeRightId == documentTypeRightId)
                        .Include("DocumentType")
                        .Include("AspNetRole")
                        .ToList();
                    if (!documentTypeRights.Any())
                    {
                        return new DocumentTypeRightObject();
                    }

                    var app = documentTypeRights[0];
                    var documentTypeRightObject = ModelMapper.Map<DocumentTypeRight, DocumentTypeRightObject>(app);
                    if (documentTypeRightObject == null || documentTypeRightObject.DocumentTypeRightId < 1)
                    {
                        return new DocumentTypeRightObject();
                    }

                  documentTypeRightObject.DocumentTypeName = app.DocumentType.Name;
                  documentTypeRightObject.RoleName = app.AspNetRole.Name.Replace("_", " ");
                  return documentTypeRightObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new DocumentTypeRightObject();
           }
       }
        public List<DocumentTypeRightObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var docTypeRights =
                        db.DocumentTypeRights
                        .Include("DocumentType")
                        .Include("AspNetRole")
                        .Where(m => m.DocumentType.Name.ToLower() == searchCriteria.ToLower().Trim()
                        || m.AspNetRole.Name.ToLower() == searchCriteria.ToLower().Trim())
                        .ToList();
                    if (docTypeRights.Any())
                    {
                        var newList = new List<DocumentTypeRightObject>();
                        docTypeRights.ForEach(k =>
                        {
                            var req = newList.Find(m => m.RoleId == k.RoleId);

                            if (req != null && req.DocumentTypeRightId > 0)
                            {
                                req.DocumentTypeName += ", " + k.DocumentType.Name;
                            }
                            else
                            {
                                var documentTypeRightObject = ModelMapper.Map<DocumentTypeRight, DocumentTypeRightObject>(k);
                                if (documentTypeRightObject != null && documentTypeRightObject.DocumentTypeRightId > 0)
                                {
                                    documentTypeRightObject.RoleName = k.AspNetRole.Name.Replace("_", " ");
                                    documentTypeRightObject.DocumentTypeName = k.DocumentType.Name;
                                    newList.Add(documentTypeRightObject);
                                }
                            }

                        });
                       
                        return newList;
                    }
                    return new List<DocumentTypeRightObject>();
                }
            }
            catch (Exception ex)
            {
                return new List<DocumentTypeRightObject>();
            }
        }

        public List<AspNetRoleObject> GetRoles()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var roles = db.AspNetRoles.Where(l => l.Id == "1" || l.Id == "2" || l.Id == "6" || l.Id == "7").ToList();

                    if (!roles.Any())
                    {
                        return new List<AspNetRoleObject>();
                    }
                    var newList = new List<AspNetRoleObject>();
                    roles.ForEach(app =>
                    {
                        var roleObject = ModelMapper.Map<AspNetRole, AspNetRoleObject>(app);
                        if (roleObject != null && !string.IsNullOrEmpty(roleObject.Id))
                        {
                            roleObject.Name = roleObject.Name.Replace("_", " ");
                            newList.Add(roleObject);
                        }
                    });

                    return newList;
                }
            }
            catch (Exception ex)
            {
                return new List<AspNetRoleObject>();
            }
        }

        public long DeleteDocumentTypeRight(long documentTypeRightId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.DocumentTypeRights.Where(m => m.DocumentTypeRightId == documentTypeRightId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.DocumentTypeRights.Remove(item);
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
