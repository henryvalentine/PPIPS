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
    public class ImportClassificationRequirementManager
    {
       public long AddImportClassificationRequirement(List<ImportClassificationRequirementObject> iClassReqs)
       {
           try
           {
               if (iClassReqs == null || !iClassReqs.Any())
               {
                   return -2;
               }
               var successCount = 0;
               using (var db = new ImportPermitEntities())
               {
                   iClassReqs.ToList().ForEach(k =>
                   {
                       if (k.DocumentTypeId > 0 && k.DocumentTypeId > 0)
                       {
                           if (db.ImportClassificationRequirements.Count(m => m.ClassificationId == k.ClassificationId && m.ImportStageId == k.ImportStageId && m.DocumentTypeId == k.DocumentTypeId) > 0)
                           {
                               return;
                           }
                           var iClassReqReqEntity = ModelMapper.Map<ImportClassificationRequirementObject, ImportClassificationRequirement>(k);
                           if (iClassReqReqEntity != null && iClassReqReqEntity.DocumentTypeId > 0 && iClassReqReqEntity.ClassificationId > 0)
                           {
                               db.ImportClassificationRequirements.Add(iClassReqReqEntity);
                               db.SaveChanges();
                               successCount += 1;
                           }
                       }
                   });
                   if (successCount != iClassReqs.Count)
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

       public long UpdateImportClassificationRequirement(ImportClassificationRequirementObject iClassReq, List<ImportClassificationRequirementObject> newReqs)
       {
           try
           {
               if (iClassReq == null || !newReqs.Any())
               {
                   return -2;
               }
              
               using (var db = new ImportPermitEntities())
               {
                   var oldReqs = db.ImportClassificationRequirements.Where(r => r.ClassificationId == iClassReq.ClassificationId && r.ImportStageId == iClassReq.ImportStageId).ToList();
                   if (!oldReqs.Any())
                   {
                       return -2;
                   }
                   var newRequirements = new List<ImportClassificationRequirementObject>();
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
                           if (oldMatch.ClassificationId != newMatch.ClassificationId || oldMatch.ImportStageId != newMatch.ImportStageId)
                           {
                               oldReqs.ForEach(k =>
                               {
                                   db.ImportClassificationRequirements.Remove(k);
                                   db.SaveChanges();
                                   oldReqs.Remove(k);
                               });
                           }
                       }
                       newRequirements.ForEach(k =>
                       {
                           if (!oldReqs.Exists(n => n.DocumentTypeId == k.DocumentTypeId))
                           {
                               if (db.ImportClassificationRequirements.Count(m => m.ClassificationId == iClassReq.Id && m.DocumentTypeId == k.DocumentTypeId) < 1)
                               {
                                   var iClassReqReqEntity = ModelMapper.Map<ImportClassificationRequirementObject, ImportClassificationRequirement>(k);
                                   if (iClassReqReqEntity != null && iClassReqReqEntity.DocumentTypeId > 0 && iClassReqReqEntity.Id > 0)
                                   {
                                       db.ImportClassificationRequirements.Add(iClassReqReqEntity);
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
                                   db.ImportClassificationRequirements.Remove(c);
                                   db.SaveChanges();
                               }
                           });
                       }
                   }
                   return iClassReq.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public List<ImportClassificationRequirementObject> GetImportClassificationRequirements()
        {
           try
           {
               using (var db = new ImportPermitEntities())   
               {
                   var iClassReqs = db.ImportClassificationRequirements.Include("DocumentType").Include("ImportClass").Include("ImportStage").ToList();
                   if (!iClassReqs.Any())
                   {
                        return new List<ImportClassificationRequirementObject>();
                   }
                   var objList =  new List<ImportClassificationRequirementObject>();
                   iClassReqs.ForEach(app =>
                   {
                       var iClassReqObject = ModelMapper.Map<ImportClassificationRequirement, ImportClassificationRequirementObject>(app);
                       if (iClassReqObject != null && iClassReqObject.Id > 0)
                       {
                           objList.Add(iClassReqObject);
                       }
                   });

                   return !objList.Any() ? new List<ImportClassificationRequirementObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }
        
        public List<ImportClassificationRequirementObject> GetImportClassificationRequirements(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var iClassReqs = db.ImportClassificationRequirements
                           .OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize).Include("DocumentType").Include("ImportClass").Include("ImportStage")
                               .ToList();
                       if (iClassReqs.Any())
                       {
                           var newList = new List<ImportClassificationRequirementObject>();
                           iClassReqs.ForEach(app =>
                           {
                               var req = newList.Find(u => u.ClassificationId == app.ClassificationId && u.ImportStageId == app.ImportStageId);
                               if (req != null && req.Id > 0)
                               {
                                   req.Requirements += ", " + app.DocumentType.Name;
                               }
                               else
                               {
                                   var iClassReqObject = ModelMapper.Map<ImportClassificationRequirement, ImportClassificationRequirementObject>(app);
                                   if (iClassReqObject != null && iClassReqObject.Id > 0)
                                   {
                                       iClassReqObject.ImportClassName = app.ImportClass.Name;
                                       iClassReqObject.ImportStageName = app.ImportStage.Name;
                                       iClassReqObject.Requirements = app.DocumentType.Name;
                                       newList.Add(iClassReqObject);
                                   }
                               }

                           });
                           countG = db.ImportClassificationRequirements.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<ImportClassificationRequirementObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ImportClassificationRequirementObject>();
           }
       }
       
       public ImportClassificationRequirementObject GetImportClassificationRequirement(long iClassReqId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var iClassReqOs = db.ImportClassificationRequirements.Where(m => m.Id == iClassReqId).ToList();
                   if (!iClassReqOs.Any())
                   {
                       return new ImportClassificationRequirementObject();
                   }

                   var target = iClassReqOs[0];
                   var iClassReqs = db.ImportClassificationRequirements.Where(
                           m => m.ClassificationId == target.ClassificationId && m.ImportStageId == target.ImportStageId)
                           .Include("DocumentType")
                           .Include("ImportClass")
                           .Include("ImportStage")
                           .ToList();
                   if (!iClassReqs.Any())
                   {
                       return new ImportClassificationRequirementObject();
                   }

                   var targetObject = ModelMapper.Map<ImportClassificationRequirement, ImportClassificationRequirementObject>(target);
                   if (targetObject == null || targetObject.Id < 1)
                   {
                       return new ImportClassificationRequirementObject();
                   }
                   targetObject.ImportClassName = iClassReqs[0].ImportClass.Name;
                   targetObject.ImportStageName = iClassReqs[0].ImportStage.Name;
                   targetObject.DocumentTypeObjects = new List<DocumentTypeObject>();
                   iClassReqs.ForEach(doc =>
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
               return new ImportClassificationRequirementObject();
           }
       }

       public List<ImportClassificationRequirementObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var iClassReqs =
                       db.ImportClassificationRequirements.Where(m => m.DocumentType.Name.ToLower().Trim() == searchCriteria.ToLower().Trim() || m.ImportStage.Name.ToLower().Trim() == searchCriteria.ToLower().Trim()|| m.ImportClass.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .Include("DocumentType").Include("ImportClass").Include("ImportStage")
                       .ToList();

                   if (iClassReqs.Any())
                       {
                           var newList = new List<ImportClassificationRequirementObject>();
                           iClassReqs.ForEach(app =>
                           {
                               var iClassReqObject = ModelMapper.Map<ImportClassificationRequirement, ImportClassificationRequirementObject>(app);
                               if (iClassReqObject != null && iClassReqObject.Id > 0)
                               {
                                   if (newList.Any())
                                   {
                                       var req = newList.Find(u => u.ClassificationId == app.ClassificationId && u.ImportStageId == app.ImportStageId);
                                       if (req != null && req.Id > 0)
                                       {
                                           req.Requirements += ", " + app.DocumentType.Name;
                                       }
                                       
                                   }
                                   else
                                   {
                                       iClassReqObject.ImportClassName = app.ImportClass.Name;
                                       iClassReqObject.ImportStageName = app.ImportStage.Name;
                                       iClassReqObject.Requirements = app.DocumentType.Name;
                                       newList.Add(iClassReqObject);
                                   }
                               }

                           });
                           return newList;
                       }

                   return new List<ImportClassificationRequirementObject>();
                   }
              
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ImportClassificationRequirementObject>();
           }
       }

       public long DeleteImportClassificationRequirement(long iClassReqId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems = db.ImportClassificationRequirements.Where(m => m.Id == iClassReqId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.ImportClassificationRequirements.Remove(item);
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
