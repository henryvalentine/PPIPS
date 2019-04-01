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
   public class InvoiceItemManager
    {

       public long AddInvoiceItem(InvoiceItemObject fee)
       {
           try
           {
               if (fee == null)
               {
                   return -2;
               }

               var invoiceItemEntity = ModelMapper.Map<InvoiceItemObject, InvoiceItem>(fee);
               if (invoiceItemEntity == null || invoiceItemEntity.InvoiceId < 1)
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.InvoiceItems.Add(invoiceItemEntity);
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

       public long UpdateInvoiceItem(InvoiceItemObject fee)
       {
           try
           {
               if (fee == null)
               {
                   return -2;
               }

               var invoiceItemEntity = ModelMapper.Map<InvoiceItemObject, InvoiceItem>(fee);
               if (invoiceItemEntity == null || invoiceItemEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.InvoiceItems.Attach(invoiceItemEntity);
                   db.Entry(invoiceItemEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return fee.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<InvoiceItemObject> GetInvoiceItems()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var fees = db.InvoiceItems.ToList();
                   if (!fees.Any())
                   {
                        return new List<InvoiceItemObject>();
                   }
                   var objList =  new List<InvoiceItemObject>();
                   fees.ForEach(app =>
                   {
                       var feeObject = ModelMapper.Map<InvoiceItem, InvoiceItemObject>(app);
                       if (feeObject != null && feeObject.Id > 0)
                       {
                           objList.Add(feeObject);
                       }
                   });

                   return !objList.Any() ? new List<InvoiceItemObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

      
       public List<InvoiceItemObject> GetInvoiceItems(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var fees =
                           db.InvoiceItems.OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (fees.Any())
                       {
                           var newList = new List<InvoiceItemObject>();
                           fees.ForEach(app =>
                           {
                               var feeObject = ModelMapper.Map<InvoiceItem, InvoiceItemObject>(app);
                               if (feeObject != null && feeObject.Id > 0)
                               {
                                   newList.Add(feeObject);
                               }
                           });
                           countG = db.InvoiceItems.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<InvoiceItemObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<InvoiceItemObject>();
           }
       }

        
       public InvoiceItemObject GetInvoiceItem(long feeId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var fees =
                        db.InvoiceItems.Where(m => m.Id == feeId)
                        .ToList();
                    if (!fees.Any())
                    {
                        return new InvoiceItemObject();
                    }

                    var app = fees[0];
                    var feeObject = ModelMapper.Map<InvoiceItem, InvoiceItemObject>(app);
                    if (feeObject == null || feeObject.Id < 1)
                    {
                        return new InvoiceItemObject();
                    }

                  return feeObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new InvoiceItemObject();
           }
       }

      public long DeleteInvoiceItem(long feeId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.InvoiceItems.Where(m => m.Id == feeId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.InvoiceItems.Remove(item);
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
