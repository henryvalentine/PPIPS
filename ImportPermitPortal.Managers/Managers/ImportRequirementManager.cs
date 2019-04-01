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
    public class ImportRequirementManager
    {
       public long AddImportRequirement(List<ImportRequirementObject> importReqs)
       {
           try
           {
               if (importReqs == null || !importReqs.Any())
               {
                   return -2;
               }
               var successCount = 0;
               using (var db = new ImportPermitEntities())
               {
                   importReqs.ToList().ForEach(k =>
                   {
                       if (db.ImportRequirements.Count(m => m.ImportStageId == k.ImportStageId && m.ImportStageId == k.ImportStageId && m.DocumentTypeId == k.DocumentTypeId) < 1)
                       {
                           var importReqReqEntity = ModelMapper.Map<ImportRequirementObject, ImportRequirement>(k);
                           if (importReqReqEntity != null && importReqReqEntity.DocumentTypeId > 0 && importReqReqEntity.ImportStageId > 0)
                           {
                               db.ImportRequirements.Add(importReqReqEntity);
                               db.SaveChanges();
                               successCount += 1;
                           }
                       }
                   });
                   if (successCount != importReqs.Count)
                   {
                       return -4;
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

       public long UpdateImportRequirement(ImportRequirementObject importReq, List<ImportRequirementObject> newReqs)
       {
           try
           {
               if (importReq == null || !newReqs.Any())
               {
                   return -2;
               }
              
               using (var db = new ImportPermitEntities())
               {
                   var oldReqs = db.ImportRequirements.Where(r => r.ImportStageId == importReq.ImportStageId && r.ImportStageId == importReq.ImportStageId).ToList();
                   if (!oldReqs.Any())
                   {
                       return -2;
                   }
                   var newRequirements = new List<ImportRequirementObject>();
                   if (newReqs != null)
                   {
                       newRequirements = newReqs;
                   }

                   if (newRequirements.Any())
                   {
                       if (oldReqs.Any())
                       {
                           var oldMatch = oldReqs[0];
                           var newMatch = newRequirements[0];
                           if (oldMatch.ImportStageId != newMatch.ImportStageId || oldMatch.ImportStageId != newMatch.ImportStageId)
                           {
                               oldReqs.ForEach(k =>
                               {
                                   db.ImportRequirements.Remove(k);
                                   db.SaveChanges();
                                   oldReqs.Remove(k);
                               });
                           }
                       }
                       newRequirements.ForEach(k =>
                       {
                           if (!oldReqs.Exists(n => n.DocumentTypeId == k.DocumentTypeId))
                           {
                               if (db.ImportRequirements.Count(m => m.ImportStageId == importReq.Id && m.DocumentTypeId == k.DocumentTypeId) < 1)
                               {
                                   var importReqReqEntity = ModelMapper.Map<ImportRequirementObject, ImportRequirement>(k);
                                   if (importReqReqEntity != null && importReqReqEntity.DocumentTypeId > 0 && importReqReqEntity.Id > 0)
                                   {
                                       db.ImportRequirements.Add(importReqReqEntity);
                                       db.SaveChanges();
                                   }
                               }
                           }
                       });

                       if (oldReqs.Any())
                       {
                           oldReqs.ForEach(c =>
                           {
                               if (!newRequirements.Exists(n => n.DocumentTypeId == c.DocumentTypeId))
                               {
                                   db.ImportRequirements.Remove(c);
                                   db.SaveChanges();
                               }
                           });
                       }
                   }
                   return importReq.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public List<ImportRequirementObject> GetImportRequirements()
        {
           try
           {
               using (var db = new ImportPermitEntities())   
               {
                   var importReqs = db.ImportRequirements.Include("DocumentType").Include("ImportStage").ToList();
                   if (!importReqs.Any())
                   {
                        return new List<ImportRequirementObject>();
                   }
                   var objList =  new List<ImportRequirementObject>();
                   importReqs.ForEach(app =>
                   {
                       var importReqObject = ModelMapper.Map<ImportRequirement, ImportRequirementObject>(app);
                       if (importReqObject != null && importReqObject.Id > 0)
                       {
                           objList.Add(importReqObject);
                       }
                   });

                   return !objList.Any() ? new List<ImportRequirementObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }
        
        public List<ImportRequirementObject> GetImportRequirements(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var importReqs = db.ImportRequirements
                           .OrderByDescending(m => m.ImportStageId)
                               .Skip(tpageNumber)
                               .Take(tsize).Include("DocumentType").Include("ImportStage")
                               .ToList();

                       if (importReqs.Any())
                       {
                           var newList = new List<ImportRequirementObject>();
                           importReqs.ForEach(app =>
                           {
                               var req = newList.Find(u => u.ImportStageId == app.ImportStageId);
                               if (req != null && req.Id > 0)
                               {
                                   req.Requirements += ", " + app.DocumentType.Name;
                               }
                               else
                               {
                                   var importReqObject = ModelMapper.Map<ImportRequirement, ImportRequirementObject>(app);
                                   if (importReqObject != null && importReqObject.Id > 0)
                                   {
                                       importReqObject.ImportStageName = app.ImportStage.Name;
                                       importReqObject.Requirements = app.DocumentType.Name;
                                       newList.Add(importReqObject);
                                   }
                               }

                           });
                           countG = db.ImportRequirements.Count();
                           if (newList.Count < tsize)
                           {
                               tpageNumber++;
                               var list = GetMoreRequirements(tsize, tpageNumber, newList);
                               return list;
                           }
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<ImportRequirementObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ImportRequirementObject>();
           }
       }

        public List<ImportRequirementObject> GetMoreRequirements(int itemsPerPage, int pageNumber, List<ImportRequirementObject> newList)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var importReqs = db.ImportRequirements
                        .OrderByDescending(m => m.ImportStageId)
                            .Skip((pageNumber) * itemsPerPage)
                            .Take(itemsPerPage).Include("DocumentType").Include("ImportStage")
                            .ToList();

                    if (importReqs.Any())
                    {
                        importReqs.ForEach(app =>
                        {
                            var req = newList.Find(u => u.ImportStageId == app.ImportStageId);
                            if (req != null && req.Id > 0)
                            {
                                req.Requirements += ", " + app.DocumentType.Name;
                            }
                            else
                            {
                                var importReqObject = ModelMapper.Map<ImportRequirement, ImportRequirementObject>(app);
                                if (importReqObject != null && importReqObject.Id > 0)
                                {
                                    importReqObject.ImportStageName = app.ImportStage.Name;
                                    importReqObject.Requirements = app.DocumentType.Name;
                                    newList.Add(importReqObject);
                                }
                            }

                        });
                        return newList;
                    }
                    return new List<ImportRequirementObject>();
                }
            }
            catch (Exception ex)
            {
                return new List<ImportRequirementObject>();
            }
        }
       public ImportRequirementObject GetImportRequirement(long importReqId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var importReqOs = db.ImportRequirements.Where(m => m.Id == importReqId).ToList();
                   if (!importReqOs.Any())
                   {
                       return new ImportRequirementObject();
                   }

                   var target = importReqOs[0];
                   var importReqs = db.ImportRequirements.Where(
                           m => m.ImportStageId == target.ImportStageId && m.ImportStageId == target.ImportStageId)
                           .Include("DocumentType")
                           .Include("ImportStage")
                           .ToList();
                   if (!importReqs.Any())
                   {
                       return new ImportRequirementObject();
                   }

                   var targetObject = ModelMapper.Map<ImportRequirement, ImportRequirementObject>(target);
                   if (targetObject == null || targetObject.Id < 1)
                   {
                       return new ImportRequirementObject();
                   }
                   
                   targetObject.ImportStageName = importReqs[0].ImportStage.Name;
                   targetObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                   importReqs.ForEach(doc =>
                   {
                       targetObject.DocumentTypeObjects.Add(new DocumentTypeObject
                       {
                           DocumentTypeId = doc.DocumentType.DocumentTypeId,
                           Name = doc.DocumentType.Name
                       });
                   });
                   return targetObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ImportRequirementObject();
           }
       }

       public List<ImportRequirementObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var importReqs =
                       db.ImportRequirements.Where(m => m.DocumentType.Name.ToLower().Trim() == searchCriteria.ToLower().Trim() || m.ImportStage.Name.ToLower().Trim() == searchCriteria.ToLower().Trim() || m.DocumentType.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .Include("DocumentType").Include("ImportStage")
                       .ToList();

                   if (importReqs.Any())
                       {
                           var newList = new List<ImportRequirementObject>();
                           importReqs.ForEach(app =>
                           {
                               var importReqObject = ModelMapper.Map<ImportRequirement, ImportRequirementObject>(app);
                               if (importReqObject != null && importReqObject.Id > 0)
                               {
                                   if (newList.Any())
                                   {
                                       var req = newList.Find(u => u.ImportStageId == app.ImportStageId && u.ImportStageId == app.ImportStageId);
                                       if (req != null && req.Id > 0)
                                       {
                                           req.Requirements += ", " + app.DocumentType.Name;
                                       }
                                       
                                   }
                                   else
                                   {
                                       importReqObject.ImportStageName = app.ImportStage.Name;
                                       importReqObject.Requirements = app.DocumentType.Name;
                                       newList.Add(importReqObject);
                                   }
                               }

                           });
                           return newList;
                       }

                   return new List<ImportRequirementObject>();
                   }
              
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ImportRequirementObject>();
           }
       }

       public long DeleteImportRequirement(long importReqId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems = db.ImportRequirements.Where(m => m.Id == importReqId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.ImportRequirements.Remove(item);
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
