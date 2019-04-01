using System;
using System.Data.Entity;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
   public class FormMDetailManager
    {
       public long AddFormMDetail(FormMDetailObject formMDetail)
       {
           try
           {
               if (formMDetail == null)
               {
                   return -2;
               }

               var formMDetailEntity = ModelMapper.Map<FormMDetailObject, FormMDetail>(formMDetail);
               if (formMDetailEntity == null || formMDetailEntity.NotificationId < 1)
               {
                   return -2;
               }
               
               using (var db = new ImportPermitEntities())
               {
                   if (db.FormMDetails.Count(m => m.NotificationId == formMDetail.NotificationId) > 0)
                   {
                       return -3;
                   }
                   var returnStatus = db.FormMDetails.Add(formMDetailEntity);
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

       public long UpdateFormMDetail(FormMDetailObject formMDetail)
       {
           try
           {
               if (formMDetail == null)
               {
                   return -2;
               }

               var formMDetailEntity = ModelMapper.Map<FormMDetailObject, FormMDetail>(formMDetail);
               if (formMDetailEntity == null || formMDetailEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   if (db.FormMDetails.Count(m => m.NotificationId == formMDetail.NotificationId && m.Id != formMDetail.Id) > 0)
                   {
                       return -3;
                   }
                   db.FormMDetails.Attach(formMDetailEntity);
                   db.Entry(formMDetailEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return formMDetail.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
      
       public FormMDetailObject GetFormMDetail(long formMDetailId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var formMDetails =
                        db.FormMDetails.Where(m => m.Id == formMDetailId)
                        .Include("Document")
                        .Include("Notification")
                        .Include("Product")
                        .ToList();

                    if (!formMDetails.Any())  
                    {
                        return new FormMDetailObject();
                    }

                    var app = formMDetails[0];
                    var formMDetailObject = ModelMapper.Map<FormMDetail, FormMDetailObject>(app);
                    if (formMDetailObject == null || formMDetailObject.Id < 1)
                    {
                        return new FormMDetailObject();
                    }

                    formMDetailObject.ProductName = db.Products.Where(v => v.ProductId == app.Notification.ProductId).ToList()[0].Name;
                    formMDetailObject.DocumentTypeName = db.DocumentTypes.Where(d => d.DocumentTypeId == app.Document.DocumentTypeId).ToList()[0].Name;
                    formMDetailObject.DateAttachedStr = app.DateAttached.ToString("dd/MM/yyyy");
                    formMDetailObject.DateIssuedStr = app.DateIssued.ToString("dd/MM/yyyy");
                  return formMDetailObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new FormMDetailObject();
           }
       }

       public long DeleteFormMDetail(long formMDetailId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.FormMDetails.Where(m => m.Id == formMDetailId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.FormMDetails.Remove(item);
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
