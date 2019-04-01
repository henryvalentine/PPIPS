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
   public class ThroughPutManager
    {

       public long AddThroughPut(ThroughPutObject throughPut)
       {
           try
           {
               if (throughPut == null)
               {
                   return -2;
               }

               var documentEntity = ModelMapper.Map<DocumentObject, Document>(throughPut.DocumentObject);
               if (documentEntity == null || documentEntity.DocumentTypeId < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   var throughPutEntities = (from th in db.ThroughPuts.Where(th => th.Id == throughPut.Id)
                       join appItem in db.ApplicationItems on th.ApplicationItemId equals appItem.Id
                       join app in db.Applications on appItem.ApplicationId equals app.Id
                       select new {th, app}).ToList();

                   if (!throughPutEntities.Any())
                   {
                       return -2;
                   }

                   var throughPutEntity = throughPutEntities[0].th;
                   var appEntity = throughPutEntities[0].app;

                   var docEntity = db.Documents.Add(documentEntity);
                   db.SaveChanges();

                   var appDoc = new ApplicationDocument
                   {
                       DocumentId = docEntity.DocumentId,
                       ApplicationId = appEntity.Id
                   };

                   db.ApplicationDocuments.Add(appDoc);
                   db.SaveChanges();
                   
                  
                   throughPutEntity.DocumentId = docEntity.DocumentId;
                   throughPutEntity.Quantity = throughPut.Quantity;
                   throughPutEntity.IPAddress = throughPut.IPAddress;
                   db.Entry(throughPutEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return throughPutEntity.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       


       public long UpdateThroughPut(ThroughPutObject throughPut)
       {
           try
           {
               if (throughPut == null)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   if (throughPut.DocumentObject != null && throughPut.DocumentObject.DocumentId > 0)
                   {
                       var documentEntities = db.Documents.Where(th => th.DocumentId == throughPut.DocumentObject.DocumentId).ToList();
                       if (!documentEntities.Any())
                       {
                           return -2;
                       }

                       var documentEntity = documentEntities[0];
                       documentEntity.UploadedById = throughPut.DocumentObject.UploadedById;
                       documentEntity.IpAddress = throughPut.DocumentObject.IpAddress;
                       documentEntity.Status = (int)AppStatus.Pending;
                       documentEntity.DateUploaded = DateTime.Now;
                       documentEntity.DocumentPath = throughPut.DocumentObject.DocumentPath;
                       db.Entry(documentEntity).State = EntityState.Modified;
                       db.SaveChanges();
                   }
                   
                   var throughPutEntities = db.ThroughPuts.Where(th => th.Id == throughPut.Id).ToList();
                   if (!throughPutEntities.Any())
                   {
                       return -2;
                   }

                   var throughPutEntity = throughPutEntities[0];
                   throughPutEntity.Quantity = throughPut.Quantity;
                   throughPutEntity.IPAddress = throughPut.IPAddress;
                   db.Entry(throughPutEntity).State = EntityState.Modified;
                   db.SaveChanges();

                   return throughPutEntity.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public ThroughPutObject GetThroughPut(long thId)
       {
           try
           {
               if (thId < 1)
               {
                   return new ThroughPutObject();
               }
               using (var db = new ImportPermitEntities())
               {
                   var docs = db.ThroughPuts.Where(m => m.Id == thId).Include("Document").Include("Product").ToList();
                   if (!docs.Any())
                   {
                        return new ThroughPutObject();
                   }
                   var doc = docs[0];
                   var throughPutObj = ModelMapper.Map<ThroughPut, ThroughPutObject>(doc);
                   if (throughPutObj == null || throughPutObj.ApplicationItemId < 1)
                   {
                        return new ThroughPutObject();
                   }

                   return throughPutObj;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ThroughPutObject();
           }
       }
       public long DeleteThroughPut(long throughPutId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.ThroughPuts.Where(m => m.Id == throughPutId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.ThroughPuts.Remove(item);
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
