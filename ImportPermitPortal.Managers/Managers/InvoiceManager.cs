using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
   public class InvoiceManager
    {

       public ResponseObject AddInvoice(InvoiceObject invoice)
       {
           try
           {
               if (invoice == null)
               {
                   return new ResponseObject();
               }

               var invoiceEntity = ModelMapper.Map<InvoiceObject, Invoice>(invoice);
               if (invoiceEntity == null || invoiceEntity.PaymentTypeId < 1)
               {
                   return new ResponseObject();
               }

               var paymentMethod = "";
               var i = Enum.GetName(typeof(PaymentOption), invoice.PaymentTypeId);
               if (i != null)
               {
                   paymentMethod = i.Replace("8", ",").Replace("_", " ");
               }

               var service = "";
               var j = Enum.GetName(typeof(ServiceDescriptionEnum), invoice.ServiceDescriptionId);
               if (j != null)
               {
                   service = j.Replace("8", ",").Replace("_", " ");
               }

               var transactionInvoice = new TransactionInvoice
               {
                   PaymentMethod = paymentMethod,
                   CustomerId = invoice.ImporterId,
                   ExpiryDate = invoice.ExpiryDate,
                   BookDate = DateTime.Now,
                   TotalAmountDue = invoice.TotalAmountDue,
                   AmountPaid = 0,
                   RRR = invoice.RRR,
                   CurrencyCode = "NGN",
                   Status = (int)InvoiceStatus.Pending,
                   ServiceDescription = service,
                   CreatedDate = DateTime.Now,
                   LastModifiedDate = DateTime.Now,
               };

               var reId = AddTransactionInvoice(transactionInvoice);

               if (reId < 1)
               {
                   return new ResponseObject();
               }

               using (var db = new ImportPermitEntities())
               {
                  invoiceEntity.ReferenceCode = reId.ToString();
                  var returnStatus = db.Invoices.Add(invoiceEntity);
                  db.SaveChanges();

                   var count = AddInvoiceItems(returnStatus.Id, invoice.InvoiceItemObjects.ToList());
                   if (count < 1)
                   {
                       db.Invoices.Remove(invoiceEntity);
                       DeleteTransactionInvoice(reId);
                       db.SaveChanges();
                       return new ResponseObject();
                   }

                   return new ResponseObject { InvoiceId = returnStatus.Id, RefCode = reId.ToString()};
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ResponseObject();
           }
       }

       public ResponseObject AddExpenditionaryInvoice(InvoiceObject invoice, long notificationId)
       {
           try
           {
               if (invoice == null || notificationId < 1)
               {
                   return new ResponseObject();
               }

               var invoiceEntity = ModelMapper.Map<InvoiceObject, Invoice>(invoice);
               if (invoiceEntity == null || invoiceEntity.PaymentTypeId < 1)
               {
                   return new ResponseObject();
               }

               var paymentMethod = "";
               var i = Enum.GetName(typeof(PaymentOption), invoice.PaymentTypeId);
               if (i != null)
               {
                   paymentMethod = i.Replace("8", ",").Replace("_", " ");
               }

               var service = "";
               var j = Enum.GetName(typeof(ServiceDescriptionEnum), invoice.ServiceDescriptionId);
               if (j != null)
               {
                   service = j.Replace("8", ",").Replace("_", " ");
               }

               var transactionInvoice = new TransactionInvoice
               {
                   PaymentMethod = paymentMethod,
                   CustomerId = invoice.ImporterId,
                   ExpiryDate = invoice.ExpiryDate,
                   BookDate = DateTime.Now,
                   TotalAmountDue = invoice.TotalAmountDue,
                   AmountPaid = 0,
                   RRR = invoice.RRR,
                   CurrencyCode = "NGN",
                   Status = (int)InvoiceStatus.Pending,
                   ServiceDescription = service,
                   CreatedDate = DateTime.Now,
                   LastModifiedDate = DateTime.Now,
               };

               var reId = AddTransactionInvoice(transactionInvoice);

               if (reId < 1)
               {
                   return new ResponseObject();
               }

               using (var db = new ImportPermitEntities())
               {
                   invoiceEntity.ReferenceCode = reId.ToString();
                   var returnStatus = db.Invoices.Add(invoiceEntity);
                   db.SaveChanges();

                   var count = AddInvoiceItems(returnStatus.Id, invoice.InvoiceItemObjects.ToList());
                   if (count < 1)
                   {
                       db.Invoices.Remove(invoiceEntity);
                       DeleteTransactionInvoice(reId);
                       db.SaveChanges();
                       return new ResponseObject();
                   }

                   var inexp = new ExpenditionaryInvoice
                   {
                       NotificationId = notificationId,
                       InvoiceId = returnStatus.Id
                   };

                   db.ExpenditionaryInvoices.Add(inexp);
                   db.SaveChanges();
                   return new ResponseObject { InvoiceId = returnStatus.Id, RefCode = reId.ToString() };
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ResponseObject();
           }
       }
       public long AddInvoiceItems(long invoiceId, List<InvoiceItemObject> invoiceItems)
       {
           try
           {
               if (invoiceItems == null)
               {
                   return -2;
               }
               var successCount = 0;
               using (var db = new ImportPermitEntities())
               {
                   invoiceItems.ForEach(v =>
                   {
                       var invoiceItemEntity = ModelMapper.Map<InvoiceItemObject, InvoiceItem>(v);
                       if (invoiceItemEntity == null || invoiceItemEntity.FeeId <= 0) return;
                       invoiceItemEntity.InvoiceId = invoiceId;
                       db.InvoiceItems.Add(invoiceItemEntity);
                       db.SaveChanges();
                       successCount += 1;
                   });
                   if (successCount != invoiceItems.Count) return 0;
                   return 5;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       
       public long AddTransactionInvoice(TransactionInvoice invoice)
       {
           try
           {
               if (invoice == null || invoice.TotalAmountDue < 1)
               {
                   
               }
               using (var db = new PPIPSPaymentEntities())
               {
                   var entity = db.TransactionInvoices.Add(invoice);
                   db.SaveChanges();
                   return entity.Id;
               }

           }
           catch (Exception ex)
           {
               return 0;
           }

       }

       public long UpdateTransactionInvoice(TransactionInvoice invoice)
       {
           try
           {
               if (invoice == null || invoice.TotalAmountDue < 1)
               {
                   return -2;
               }
               using (var db = new PPIPSPaymentEntities())
               {

                   var entities = db.TransactionInvoices.Where(i => i.Id == invoice.Id).ToList();
                   if (!entities.Any())
                   {
                       return -2;
                   }

                   var entity = entities[0];
                   entity.PaymentMethod = invoice.PaymentMethod;
                   entity.ExpiryDate = invoice.ExpiryDate;
                   entity.BookDate = DateTime.Now;
                   entity.TotalAmountDue = invoice.TotalAmountDue;
                   entity.AmountPaid = 0;
                   entity.CurrencyCode = "NGN";
                   entity.Status = (int) InvoiceStatus.Pending;
                   entity.ServiceDescription = invoice.ServiceDescription;
                   entity.LastModifiedDate = DateTime.Now;
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

       public long DeleteTransactionInvoice(long id)
       {
           try
           {
               using (var db = new PPIPSPaymentEntities())
               {
                   var entities = db.TransactionInvoices.Where(i => i.Id == id).ToList();
                   if (!entities.Any())
                   {
                       return 0;
                   }
                   var entity = entities[0];
                   db.TransactionInvoices.Remove(entity);
                   db.SaveChanges();
                   return 5;
               }

           }
           catch (Exception ex)
           {
               return 0;
           }

       }

       public long UpdateInvoiceItems(List<InvoiceItemObject> oldInvoiceItems, List<InvoiceItemObject> invoiceItems)
       {
           try
           {
               if (invoiceItems == null)
               {
                   return -2;
               }
               var successCount = 0;
               using (var db = new ImportPermitEntities())
               {
                   invoiceItems.ForEach(v =>
                   {
                       var existingItem = oldInvoiceItems.Find(l => l.Id == v.Id);
                       if (existingItem == null || existingItem.Id < 1)
                       {
                           var invoiceItemEntity = ModelMapper.Map<InvoiceItemObject, InvoiceItem>(v);
                           if (invoiceItemEntity == null || invoiceItemEntity.FeeId <= 0) return;
                           db.InvoiceItems.Add(invoiceItemEntity);
                           db.SaveChanges();
                           successCount += 1;  
                       }
                   });

                   oldInvoiceItems.ForEach(v =>
                   {
                       var existingItem = invoiceItems.Find(l => l.Id == v.Id);
                       if(existingItem == null || existingItem.Id < 1)
                       {
                           var iic = db.InvoiceItems.Find(v.Id);
                           if(iic != null)
                           {
                               db.InvoiceItems.Remove(iic);
                               db.SaveChanges();
                           }
                       }
                   });

                   if (successCount != invoiceItems.Count) return 0;
                   return 5;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateInvoiceRefCode(InvoiceObject invoice)
        {
            try
            {
                if (invoice == null)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    var invoiceEntity = db.Invoices.Find(invoice.Id);
                    if (invoiceEntity == null || invoiceEntity.Id < 1)
                    {
                        return -2;
                    }
                    invoiceEntity.ReferenceCode = invoice.ReferenceCode;
                    db.Entry(invoiceEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return invoiceEntity.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
       public string UpdateInvoiceGetRefCode(InvoiceObject invoice)
        {
            try
            {
                if (invoice == null)
                {
                    return "";
                }


                using (var db = new ImportPermitEntities())
                {
                    var invoiceEntities = db.Invoices.Where(i => i.Id == invoice.Id).ToList();
                    if (!invoiceEntities.Any())
                    {
                        return "";
                    }

                    var invoiceEntity = invoiceEntities[0];
                    invoiceEntity.PaymentTypeId = invoice.PaymentTypeId;
                    invoiceEntity.TotalAmountDue = invoice.TotalAmountDue;
                    invoiceEntity.IPAddress = invoice.IPAddress;
                    db.Entry(invoiceEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    var updateStatus = UpdateInvoiceItemsWithoutAttach(invoice.InvoiceItemObjects.ToList());
                    if (updateStatus < 1)
                    {
                        return "";
                    }
                    return invoiceEntity.ReferenceCode;
                }
            }
            catch (DbEntityValidationException e)
            {
                var str = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    str += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State) + "\n";
                    str = eve.ValidationErrors.Aggregate(str, (current, ve) => current + (string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage) + " \n"));
                }
                ErrorLogger.LoggError(e.StackTrace, e.Source, str);
                return "";
            }
        }
       public ResponseObject UpdateInvoiceWithouthAttach(InvoiceObject invoice)
       {
           var res = new ResponseObject();
           try
           {
               if (invoice == null)
               {
                   return res;
               }


               using (var db = new ImportPermitEntities())
               {
                   var paymentMethod = "";
                   var pOption = Enum.GetName(typeof(PaymentOption), invoice.PaymentTypeId);
                   if (pOption != null)
                   {
                       paymentMethod = pOption.Replace("8", ",").Replace("_", " ");
                   }

                   var service = "";
                   var j = Enum.GetName(typeof(ServiceDescriptionEnum), invoice.ServiceDescriptionId);
                   if (j != null)
                   {
                       service = j.Replace("8", ",").Replace("_", " ");
                   }

                   var invoiceEntities = db.Invoices.Where(i => i.Id == invoice.Id).ToList();
                   if (!invoiceEntities.Any())
                   {
                       return res;
                   }

                   var invoiceEntity = invoiceEntities[0];

                   long id;
                   var pres = long.TryParse(invoiceEntity.ReferenceCode, out id);
                   if (!pres || id < 1)
                   {
                       return res;
                   }
                   
                   var transactionInvoice = new TransactionInvoice
                   {
                       Id = id,
                       PaymentMethod = paymentMethod,
                       CustomerId = invoice.ImporterId,
                       ExpiryDate = invoice.ExpiryDate,
                       BookDate = DateTime.Now,
                       TotalAmountDue = invoice.TotalAmountDue,
                       AmountPaid = 0,
                       CurrencyCode = "NGN",
                       Status = (int)InvoiceStatus.Pending,
                       ServiceDescription = service,
                       LastModifiedDate = DateTime.Now,
                   };

                   var reId = UpdateTransactionInvoice(transactionInvoice);
                   if (reId < 1)
                   {
                       return new ResponseObject();
                   }
                   
                   invoiceEntity.PaymentTypeId = invoice.PaymentTypeId;
                   invoiceEntity.TotalAmountDue = invoice.TotalAmountDue;
                   invoiceEntity.IPAddress = invoice.IPAddress;
                   db.Entry(invoiceEntity).State = EntityState.Modified;
                   db.SaveChanges();

                   invoice.InvoiceItemObjects.ToList().ForEach(k => { k.InvoiceId = invoiceEntity.Id; });
                   var updateStatus = UpdateInvoiceItemsWithoutAttach(invoice.InvoiceItemObjects.ToList());
                   if (updateStatus < 1)
                   {
                       return res;
                   }

                   return new ResponseObject { InvoiceId = invoiceEntity.Id, RefCode = reId.ToString(), RRR = invoiceEntity.RRR };
               }
           }
           catch (DbEntityValidationException e)
           {
               var str = "";
               foreach (var eve in e.EntityValidationErrors)
               {
                   str += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                       eve.Entry.Entity.GetType().Name, eve.Entry.State) + "\n";
                   str = eve.ValidationErrors.Aggregate(str, (current, ve) => current + (string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage) + " \n"));
               }
               ErrorLogger.LoggError(e.StackTrace, e.Source, str);
               return res;
           }
       }

       public long UpdatePpipsInvoice(double amountDue, string referenceCode)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {

                   var entities = db.Invoices.Where(l => l.ReferenceCode == referenceCode).ToList();
                   if (entities.Any())
                   {
                       return -2;
                   }

                   var entity = entities[0];
                   entity.TotalAmountDue = amountDue;
                   db.Entry(entity).State = EntityState.Modified;
                   db.SaveChanges();
                   return entity.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return -2;
           }
       }
       
       public long UpdateInvoiceRrr(string  refCode, string rrr)
       {
           try
           {
               if (string.IsNullOrEmpty(refCode) || string.IsNullOrEmpty(rrr))
               {
                   return -2;
               }

               long id;
               var pres = long.TryParse(refCode, out id);
               if (!pres || id < 1)
               {
                   return -2;
               }
               var reId = UpdatePpipsInvoiceRrr(id, rrr);
               if (reId < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {

                   var entities = db.Invoices.Where(l => l.ReferenceCode == refCode).ToList();
                   if (!entities.Any())
                   {
                       return -2;
                   }

                   var entity = entities[0];
                   entity.RRR = rrr;
                   db.Entry(entity).State = EntityState.Modified;
                   db.SaveChanges();
                   return 5;
               }
           }
           catch (DbEntityValidationException e)
           {
               var str = "";
               foreach (var eve in e.EntityValidationErrors)
               {
                   str += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                       eve.Entry.Entity.GetType().Name, eve.Entry.State) + "\n";
                   str = eve.ValidationErrors.Aggregate(str, (current, ve) => current + (string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage) + " \n"));
               }
               ErrorLogger.LoggError(e.StackTrace, e.Source, str);
               return -2;
           }
       }

       public long UpdatePpipsInvoiceRrr(long id, string rrr)
       {
           try
           {
               using (var db = new PPIPSPaymentEntities())
               {

                   var entities = db.TransactionInvoices.Where(l => l.Id == id).ToList();
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
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return -2;
           }
       }

       public long UpdateInvoiceItemsWithoutAttach(List<InvoiceItemObject> invoiceItems)
       {
           try
           {
               if (invoiceItems == null)
               {
                   return -2;
               }

               var successCount = 0;
               using (var db = new ImportPermitEntities())
               {
                   var invoiceId = invoiceItems[0].InvoiceId;
                   var appInvoiceItems = db.InvoiceItems.Where(k => k.InvoiceId == invoiceId).ToList();
                   if (!appInvoiceItems.Any())
                   {
                       return -2;
                   }

                   foreach(var v in invoiceItems)
                   {
                       var existingItem = appInvoiceItems.Find(b => b.FeeId == v.FeeId);
                       if (existingItem == null || existingItem.Id < 1)
                       {
                           var invoiceItemEntity = ModelMapper.Map<InvoiceItemObject, InvoiceItem>(v);
                           if (invoiceItemEntity == null || invoiceItemEntity.FeeId <= 0) return -2;
                           db.InvoiceItems.Add(invoiceItemEntity);
                           db.SaveChanges();
                           successCount += 1;
                       }
                       else
                       {
                           existingItem.AmountDue = v.AmountDue;
                           db.Entry(existingItem).State = EntityState.Modified;
                           db.SaveChanges();
                           successCount += 1;
                       }
                   }

                   appInvoiceItems.ForEach(v =>
                   {
                       var existingItem = invoiceItems.Find(l => l.FeeId == v.FeeId);
                       if (existingItem == null || existingItem.FeeId < 1)
                       {
                            db.InvoiceItems.Remove(v);
                            db.SaveChanges();
                       }
                   });

                   if (successCount != invoiceItems.Count) return 0;
                   return 5;
               }
           }
           catch (DbEntityValidationException e)
           {
               var str = "";
               foreach (var eve in e.EntityValidationErrors)
               {
                   str += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                       eve.Entry.Entity.GetType().Name, eve.Entry.State) + "\n";
                   str = eve.ValidationErrors.Aggregate(str, (current, ve) => current + (string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage) + " \n"));
               }
               ErrorLogger.LoggError(e.StackTrace, e.Source, str);
               return 0;
           }
       }

       public long UpdateInvoice(InvoiceObject invoice, List<InvoiceItemObject> newInvoiceItems)
       {
           try
           {
               if (invoice == null)
               {
                   return -2;
               }

               var invoiceEntity = ModelMapper.Map<InvoiceObject, Invoice>(invoice);
               if (invoiceEntity == null || invoiceEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.Invoices.Attach(invoiceEntity);
                   db.Entry(invoiceEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   var updateStatus = UpdateInvoiceItems(newInvoiceItems, invoice.InvoiceItemObjects.ToList());
                   if (updateStatus < 1)
                   {
                       return -4;
                   }
                   return invoice.Id;
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

       public List<InvoiceObject> GetInvoices()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var invoices = db.Invoices.ToList();
                   if (!invoices.Any())
                   {
                        return new List<InvoiceObject>();
                   }
                   var objList =  new List<InvoiceObject>();
                   invoices.ForEach(app =>
                   {
                       var invoiceObject = ModelMapper.Map<Invoice, InvoiceObject>(app);
                       if (invoiceObject != null && invoiceObject.Id > 0)
                       {
                           objList.Add(invoiceObject);
                       }
                   });

                   return !objList.Any() ? new List<InvoiceObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }
       
       public List<InvoiceObject> GetInvoices(int? itemsPerPage, int? pageNumber, out int countG,long importerId)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var myApplications = db.Invoices.Where(m => m.ImporterId == importerId).
                           OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize).ToList();
                       if (!myApplications.Any())
                       {
                           countG = 0;
                           return new List<InvoiceObject>();
                       }

                       var newList = new List<InvoiceObject>();
                       myApplications.ForEach(app =>
                       {
                           var importObject = ModelMapper.Map<Invoice, InvoiceObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               importObject.DateAddedStr = app.DateAdded.ToString("dd/MM/yyyy");
                               importObject.TotalAmountDueStr = app.TotalAmountDue.ToString("n");
                               var name = Enum.GetName(typeof (AppStatus), importObject.Status);
                               if (name != null) importObject.StatusStr = name.Replace("_", " ");
                               var paymentOption = Enum.GetName(typeof (PaymentOption), app.PaymentTypeId);
                               if (paymentOption != null)importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                               var serviceDescription = Enum.GetName(typeof(ServiceDescriptionEnum), app.ServiceDescriptionId);
                               if (serviceDescription != null) importObject.ServiceDescription = serviceDescription.Replace("_", " ");
                               newList.Add(importObject);
                           }
                       });
                       countG = db.Invoices.Count(m => m.ImporterId == importerId);
                       return newList;
                   }
               }
               countG = 0;
               return new List<InvoiceObject>();
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               countG = 0;
               return new List<InvoiceObject>();
           }
       }

       public List<InvoiceObject> GetInvoices(int? itemsPerPage, int? pageNumber, out int countG)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var myApplications = db.Invoices.Include("Importer").
                           OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize).ToList();
                       if (!myApplications.Any())
                       {
                           countG = 0;
                           return new List<InvoiceObject>();
                       }

                       var newList = new List<InvoiceObject>();
                       myApplications.ForEach(app =>
                       {
                           var importObject = ModelMapper.Map<Invoice, InvoiceObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               importObject.DateAddedStr = app.DateAdded.ToString("dd/MM/yyyy");
                               importObject.TotalAmountDueStr = app.TotalAmountDue.ToString("n");
                               var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                               if (name != null) importObject.StatusStr = name.Replace("_", " ");
                               var paymentOption = Enum.GetName(typeof(PaymentOption), app.PaymentTypeId);
                               if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                               importObject.ImporterName = app.Importer.Name;
                               var serviceDescription = Enum.GetName(typeof(ServiceDescriptionEnum), app.ServiceDescriptionId);
                               if (serviceDescription != null) importObject.ServiceDescription = serviceDescription.Replace("_", " ");
                               newList.Add(importObject);
                           }  
                       });

                       countG = db.Invoices.Count();
                       return newList;
                   }
               }
               countG = 0;
               return new List<InvoiceObject>();
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               countG = 0;
               return new List<InvoiceObject>();
           }
       }

       public List<InvoiceObject> GetPaidInvoices(int? itemsPerPage, int? pageNumber, out int countG)
       {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int)pageNumber;
                   var tsize = (int)itemsPerPage;
                   const int paid = (int) PaymentStatusEnum.Paid;
                   using (var db = new ImportPermitEntities())
                   {
                       var myApplications = db.Invoices.Where(v => v.Status == paid).Include("Importer").
                           OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize).ToList();
                       if (!myApplications.Any())
                       {
                           countG = 0;
                           return new List<InvoiceObject>();
                       }

                       var newList = new List<InvoiceObject>();
                       myApplications.ForEach(app =>
                       {
                           var importObject = ModelMapper.Map<Invoice, InvoiceObject>(app);
                           if (importObject != null && importObject.Id > 0)
                           {
                               importObject.DateAddedStr = app.DateAdded.ToString("dd/MM/yyyy");
                               importObject.TotalAmountDueStr = app.TotalAmountDue.ToString("n");
                               var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                               if (name != null) importObject.StatusStr = name.Replace("_", " ");
                               var paymentOption = Enum.GetName(typeof(PaymentOption), app.PaymentTypeId);
                               if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                               importObject.ImporterName = app.Importer.Name;
                               var serviceDescription = Enum.GetName(typeof(ServiceDescriptionEnum), app.ServiceDescriptionId);
                               if (serviceDescription != null) importObject.ServiceDescription = serviceDescription.Replace("_", " ");
                               newList.Add(importObject);
                           }
                       });

                       countG = db.Invoices.Count();
                       return newList;
                   }
               }
               countG = 0;
               return new List<InvoiceObject>();
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               countG = 0;
               return new List<InvoiceObject>();
           }
       }
       public InvoiceObject GetInvoice(long invoiceId, long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Invoices.Where(m => m.Id == invoiceId && m.ImporterId == importerId)
                           .Include("Importer")
                           .Include("Applications")
                           .Include("Notifications")
                           .ToList();
                   if (!myApplications.Any())
                   {
                       return new InvoiceObject();
                   }
                   
                   var app = myApplications[0];

                   var importObject = ModelMapper.Map<Invoice, InvoiceObject>(app);
                   if (importObject == null || importObject.Id < 1)
                   {
                       return new InvoiceObject();
                   }

                   importObject.TotalAmountDueStr = app.TotalAmountDue.ToString("n");
                   var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                   if (name != null) importObject.StatusStr = name.Replace("_", " ");
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("_", " ");
                   var serviceDescription = Enum.GetName(typeof(ServiceDescriptionEnum), app.ServiceDescriptionId);
                   if (serviceDescription != null) importObject.ServiceDescription = serviceDescription.Replace("_", " ");
                   importObject.StatusStr = Enum.GetName(typeof(PaymentStatusEnum), app.Status);

                    importObject.ApplicationObject  = new ApplicationObject();
                    importObject.NotificationObject  = new NotificationObject();

                   if (app.Applications.Any())
                   {
                       var impApp = app.Applications.ToList()[0];
                       var iApp = GetAppApplication(impApp.Id, importerId);
                       importObject.ApplicationObject = iApp;
                   }

                   if (app.Notifications.Any())
                   {
                       var i = app.Notifications.ToList()[0];
                       var inotv = GetNotification(i.Id, importerId);
                       importObject.NotificationObject = inotv;
                   }
                  
                   importObject.InvoiceItemObjects = new List<InvoiceItemObject>();

                    var invoiceItemObjects = (from invItem in db.InvoiceItems.Where(k => k.InvoiceId == importObject.Id)
                    join f in db.Fees on invItem.FeeId equals f.FeeId
                    join ft in db.FeeTypes on f.FeeTypeId equals ft.FeeTypeId
                    select new InvoiceItemObject
                    {
                        FeeTypeName = ft.Name,
                        AmountDue = invItem.AmountDue
                    }).ToList();

                    if (!invoiceItemObjects.Any())
                    {
                        return new InvoiceObject();
                    }

                    importObject.InvoiceItemObjects = invoiceItemObjects;
                  
                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new InvoiceObject();
           }
       }

       public InvoiceObject GetReceipt(long invoiceId, long importerId)
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
                       return new InvoiceObject();
                   }

                   pReceipt = myReceipts[0];

               }

               if (pReceipt == null || pReceipt.Id < 1)
               {
                   return new InvoiceObject();
               }
              
               using (var db = new ImportPermitEntities())
               {
                   var myApplications = db.Invoices.Where(m => m.RRR == pReceipt.TransactionInvoice.RRR && m.ImporterId == importerId && m.Status > 1)
                           .Include("Importer")
                           .Include("Applications")
                           .Include("Notifications")
                            .Include("ExpenditionaryInvoices")
                           .ToList();
                   if (!myApplications.Any())
                   {
                       return new InvoiceObject();
                   }

                   var app = myApplications[0];

                   var importObject = ModelMapper.Map<Invoice, InvoiceObject>(app);
                   if (importObject == null || importObject.Id < 1)
                   {
                       return new InvoiceObject();
                   }
                   importObject.DateAddedStr = pReceipt.DateCreated.ToString("dd-MM-yyyy");
                   importObject.ReceiptNoStr = GenerateReceiptNumber(pReceipt.ReceiptNo);
                   importObject.TotalAmountDueStr = app.TotalAmountDue.ToString("n");
                   var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                   if (name != null) importObject.StatusStr = name.Replace("_", " ");
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("_", " ");
                   var serviceDescription = Enum.GetName(typeof(ServiceDescriptionEnum), app.ServiceDescriptionId);
                   if (serviceDescription != null) importObject.ServiceDescription = serviceDescription.Replace("_", " ");
                   importObject.StatusStr = Enum.GetName(typeof(PaymentStatusEnum), app.Status);

                   importObject.ApplicationObject = new ApplicationObject();
                   importObject.NotificationObject = new NotificationObject();

                   if (app.Applications.Any())
                   {
                       var impApp = app.Applications.ToList()[0];
                       var iApp = GetAppApplication(impApp.Id, importerId);
                       importObject.ApplicationObject = iApp;
                   }

                   if (app.Notifications.Any())
                   {
                       var i = app.Notifications.ToList()[0];
                       var inotv = GetNotification(i.Id, importerId);
                       importObject.NotificationObject = inotv;
                   }

                   importObject.InvoiceItemObjects = new List<InvoiceItemObject>();

                   var invoiceItemObjects = (from invItem in db.InvoiceItems.Where(k => k.InvoiceId == importObject.Id)
                                             join f in db.Fees on invItem.FeeId equals f.FeeId
                                             join ft in db.FeeTypes on f.FeeTypeId equals ft.FeeTypeId
                                             select new InvoiceItemObject
                                             {
                                                 FeeTypeName = ft.Name,
                                                 AmountDue = invItem.AmountDue
                                             }).ToList();

                   if (!invoiceItemObjects.Any())
                   {
                       return new InvoiceObject();
                   }
                   
                   importObject.InvoiceItemObjects = invoiceItemObjects;

                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new InvoiceObject();
           }
       }
        public string GenerateReceiptNumber(long receiptNumber)
        {
            try
            {
                //SYY XXXXXXX
                //2150000004
                //0000004
                var currentDate = DateTime.Now.ToString("yy");
                const int ppipsId = 2;
                const int standardZerosLimit = 7;
                var receiptNumberLength = receiptNumber.ToString().Length;
                var receiptNo = "";
                if (receiptNumberLength >= standardZerosLimit)
                {
                    return receiptNumber.ToString();
                }

                for (var i = receiptNumberLength; i < standardZerosLimit; i++)
                {
                    receiptNo += "0";
                }

                var result = ppipsId + currentDate + receiptNo + receiptNumber;
                return result;
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return "";
            }
        }

        public InvoiceObject GetReceiptInfo(long invoiceId)
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
                       return new InvoiceObject();
                   }

                   pReceipt = myReceipts[0];

               }

               if (pReceipt == null || pReceipt.Id < 1)
               {
                   return new InvoiceObject();
               }

               using (var db = new ImportPermitEntities())
               {
                   var myApplications = db.Invoices.Where(m => m.RRR == pReceipt.TransactionInvoice.RRR && m.Status > 1)
                           .Include("Importer")
                           .Include("Applications")
                           .Include("Notifications")
                            .Include("ExpenditionaryInvoices")
                           .ToList();

                   if (!myApplications.Any())
                   {
                       return new InvoiceObject();
                   }

                   var app = myApplications[0];
                   
                   var importObject = ModelMapper.Map<Invoice, InvoiceObject>(app);
                   if (importObject == null || importObject.Id < 1)
                   {
                       return new InvoiceObject();
                   }

                   importObject.ReceiptNoStr = GenerateReceiptNumber(pReceipt.ReceiptNo);
                   importObject.TotalAmountDueStr = app.TotalAmountDue.ToString("n");
                   var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                   if (name != null) importObject.StatusStr = name.Replace("_", " ");
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("_", " ");
                   var serviceDescription = Enum.GetName(typeof(ServiceDescriptionEnum), app.ServiceDescriptionId);
                   if (serviceDescription != null) importObject.ServiceDescription = serviceDescription.Replace("_", " ");
                   importObject.StatusStr = Enum.GetName(typeof(PaymentStatusEnum), app.Status);
                   importObject.DateAddedStr = pReceipt.DateCreated.ToString("dd-MM-yyyy");
                   importObject.ApplicationObject = new ApplicationObject();
                   importObject.NotificationObject = new NotificationObject();

                   if (app.Applications.Any())
                   {
                       var impApp = app.Applications.ToList()[0];
                       var iApp = GetAppApplication(impApp.Id);
                       importObject.ApplicationObject = iApp;
                   }

                   if (app.Notifications.Any())
                   {
                       var i = app.Notifications.ToList()[0];
                       var inotv = GetNotification(i.Id);
                       importObject.NotificationObject = inotv;
                   }

                   importObject.InvoiceItemObjects = new List<InvoiceItemObject>();

                   var invoiceItemObjects = (from invItem in db.InvoiceItems.Where(k => k.InvoiceId == importObject.Id)
                                             join f in db.Fees on invItem.FeeId equals f.FeeId
                                             join ft in db.FeeTypes on f.FeeTypeId equals ft.FeeTypeId
                                             select new InvoiceItemObject
                                             {
                                                 FeeTypeName = ft.Name,
                                                 AmountDue = invItem.AmountDue
                                             }).ToList();

                   if (!invoiceItemObjects.Any())
                   {
                       return new InvoiceObject();
                   }

                   importObject.InvoiceItemObjects = invoiceItemObjects;

                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new InvoiceObject();
           }
       }

       public InvoiceObject GetReceiptInfo(string rrr)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications = db.Invoices.Where(m => m.RRR == rrr.Trim() && m.Status > 1)
                           .Include("Importer")
                           .Include("Applications")
                           .Include("Notifications")
                            .Include("ExpenditionaryInvoices")
                           .ToList();

                   if (!myApplications.Any())
                   {
                       return new InvoiceObject();
                   }

                   var app = myApplications[0];

                   long receiptId = 0;
                   var res = long.TryParse(app.RRR, out receiptId);
                   if (!res || receiptId < 1)
                   {
                       return new InvoiceObject();
                   }

                   PaymentReceipt pReceipt;
                   using (var db2 = new PPIPSPaymentEntities())
                   {
                       var myReceipts = db2.PaymentReceipts.Where(v => v.TransactionInvoice.Id == receiptId)
                               .Include("TransactionInvoice")
                               .ToList();

                       if (!myReceipts.Any())
                       {
                           return new InvoiceObject();
                       }
                       pReceipt = myReceipts[0];

                   }


                   if (pReceipt == null || pReceipt.Id < 1)
                   {
                       return new InvoiceObject();
                   }

                   var importObject = ModelMapper.Map<Invoice, InvoiceObject>(app);
                   if (importObject == null || importObject.Id < 1)
                   {
                       return new InvoiceObject();
                   }

                   importObject.ReceiptNoStr = GenerateReceiptNumber(pReceipt.ReceiptNo);
                   importObject.TotalAmountDueStr = app.TotalAmountDue.ToString("n");
                   var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                   if (name != null) importObject.StatusStr = name.Replace("_", " ");
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("_", " ");
                   var serviceDescription = Enum.GetName(typeof(ServiceDescriptionEnum), app.ServiceDescriptionId);
                   if (serviceDescription != null) importObject.ServiceDescription = serviceDescription.Replace("_", " ");
                   importObject.StatusStr = Enum.GetName(typeof(PaymentStatusEnum), app.Status);

                   importObject.ApplicationObject = new ApplicationObject();
                   importObject.NotificationObject = new NotificationObject();

                   if (app.Applications.Any())
                   {
                       var impApp = app.Applications.ToList()[0];
                       var iApp = GetAppApplication(impApp.Id);
                       importObject.ApplicationObject = iApp;
                   }

                   if (app.Notifications.Any())
                   {
                       var i = app.Notifications.ToList()[0];
                       var inotv = GetNotification(i.Id);
                       importObject.NotificationObject = inotv;
                   }

                   importObject.InvoiceItemObjects = new List<InvoiceItemObject>();

                   var invoiceItemObjects = (from invItem in db.InvoiceItems.Where(k => k.InvoiceId == importObject.Id)
                                             join f in db.Fees on invItem.FeeId equals f.FeeId
                                             join ft in db.FeeTypes on f.FeeTypeId equals ft.FeeTypeId
                                             select new InvoiceItemObject
                                             {
                                                 FeeTypeName = ft.Name,
                                                 AmountDue = invItem.AmountDue
                                             }).ToList();

                   if (!invoiceItemObjects.Any())
                   {
                       return new InvoiceObject();
                   }

                   importObject.InvoiceItemObjects = invoiceItemObjects;

                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new InvoiceObject();
           }
       }
       public InvoiceObject GetInvoice(long invoiceId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Invoices.Where(m => m.Id == invoiceId)
                           .Include("Importer")
                           .Include("Applications")
                           .Include("Notifications")
                           .ToList();
                   if (!myApplications.Any())
                   {
                       return new InvoiceObject();
                   }

                   var app = myApplications[0];

                   var importObject = ModelMapper.Map<Invoice, InvoiceObject>(app);
                   if (importObject == null || importObject.Id < 1)
                   {
                       return new InvoiceObject();
                   }

                   importObject.TotalAmountDueStr = app.TotalAmountDue.ToString("n");
                   var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                   if (name != null) importObject.StatusStr = name.Replace("_", " ");
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("_", " ");
                   var serviceDescription = Enum.GetName(typeof(ServiceDescriptionEnum), app.ServiceDescriptionId);
                   if (serviceDescription != null) importObject.ServiceDescription = serviceDescription.Replace("_", " ");
                   importObject.StatusStr = Enum.GetName(typeof(PaymentStatusEnum), app.Status);
                   importObject.ImporterName = app.Importer.Name;
                   importObject.ApplicationObject = new ApplicationObject();
                   importObject.NotificationObject = new NotificationObject();

                   if (app.Applications.Any())
                   {
                       var impApp = app.Applications.ToList()[0];
                       var iApp = GetAppApplication(impApp.Id);
                       importObject.ApplicationObject = iApp;
                   }

                   if (app.Notifications.Any())
                   {
                       var i = app.Notifications.ToList()[0];
                       var inotv = GetNotification(i.Id);
                       importObject.NotificationObject = inotv;
                   }

                   importObject.InvoiceItemObjects = new List<InvoiceItemObject>();

                   var invoiceItemObjects = (from invItem in db.InvoiceItems.Where(k => k.InvoiceId == importObject.Id)
                                             join f in db.Fees on invItem.FeeId equals f.FeeId
                                             join ft in db.FeeTypes on f.FeeTypeId equals ft.FeeTypeId
                                             select new InvoiceItemObject
                                             {
                                                 FeeTypeName = ft.Name,
                                                 AmountDue = invItem.AmountDue
                                             }).ToList();

                   if (!invoiceItemObjects.Any())
                   {
                       return new InvoiceObject();
                   }

                   importObject.InvoiceItemObjects = invoiceItemObjects;

                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new InvoiceObject();
           }
       }

       public InvoiceObject VerifyRrr(string rrr)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var invoices = db.Invoices.Where(m => m.RRR.Trim() == rrr.Trim())
                           .Include("Importer")
                           .Include("Applications")
                           .Include("Notifications")
                           .ToList();

                   if (!invoices.Any())
                   {
                       return new InvoiceObject();
                   }

                   var app = invoices[0];

                   var importObject = ModelMapper.Map<Invoice, InvoiceObject>(app);
                   if (importObject == null || importObject.Id < 1)
                   {
                       return new InvoiceObject();
                   }

                   importObject.TotalAmountDueStr = app.TotalAmountDue.ToString("n");
                   var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                   if (name != null) importObject.StatusStr = name.Replace("_", " ");
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("_", " ");
                   var serviceDescription = Enum.GetName(typeof(ServiceDescriptionEnum), app.ServiceDescriptionId);
                   if (serviceDescription != null) importObject.ServiceDescription = serviceDescription.Replace("_", " ");
                   importObject.StatusStr = Enum.GetName(typeof(PaymentStatusEnum), app.Status);
                   importObject.ImporterName = app.Importer.Name;
                   importObject.ApplicationObject = new ApplicationObject();
                   importObject.NotificationObject = new NotificationObject();
                   
                   if (app.Applications.Any())
                   {
                       var impApp = app.Applications.ToList()[0];
                       var iApp = GetAppApplication(impApp.Id);
                       importObject.ApplicationObject = iApp;
                   }

                   if (app.Notifications.Any())
                   {
                       var i = app.Notifications.ToList()[0];
                       var inotv = GetNotification(i.Id);
                       importObject.NotificationObject = inotv;
                   }

                   importObject.InvoiceItemObjects = new List<InvoiceItemObject>();

                   var invoiceItemObjects = (from invItem in db.InvoiceItems.Where(k => k.InvoiceId == importObject.Id)
                                             join f in db.Fees on invItem.FeeId equals f.FeeId
                                             join ft in db.FeeTypes on f.FeeTypeId equals ft.FeeTypeId
                                             select new InvoiceItemObject
                                             {
                                                 FeeTypeName = ft.Name,
                                                 AmountDue = invItem.AmountDue
                                             }).ToList();

                   if (!invoiceItemObjects.Any())
                   {
                       return new InvoiceObject();
                   }

                   importObject.InvoiceItemObjects = invoiceItemObjects;

                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new InvoiceObject();
           }
       }

       public ApplicationObject GetAppApplication(long id, long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Applications.Where(m => m.Id == id && m.ImporterId == importerId)
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
                   importObject.PaymentTypeId = app.Invoice.PaymentTypeId;
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                   importObject.ApplicationTypeName = app.ApplicationType.Name;
                   importObject.ReferenceCode = app.Invoice.ReferenceCode;

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

       public ApplicationObject GetAppApplication(long id)
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
                   importObject.PaymentTypeId = app.Invoice.PaymentTypeId;
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                   importObject.ApplicationTypeName = app.ApplicationType.Name;
                   importObject.ReferenceCode = app.Invoice.ReferenceCode;

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

       public NotificationObject GetNotification(long notificationId, long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Notifications.Where(m => m.Id == notificationId && m.ImporterId == importerId)
                           .Include("Importer")
                           .Include("Invoice")
                           .Include("Permit")
                           .Include("Port")
                           .Include("Product")
                           .Include("Depot")
                           .Include("InvoiceItems")
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

                   importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy");
                   importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                   importObject.ImporterName = app.Importer.Name;
                   importObject.PermitValue = app.Permit.PermitValue;
                   importObject.ProductCode = app.Product.Code;
                   importObject.NotificationClassName = app.ImportClass.Name;

                   importObject.DepotName = app.Depot.Name;
                   var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                   if (name != null) importObject.StatusStr = name.Replace("_", " ");

                   importObject.PaymentTypeId = app.Invoice.PaymentTypeId;
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("_", " ");

                   var cargo = Enum.GetName(typeof(CargoTypeEnum), app.CargoInformationTypeId);
                   if (cargo != null) importObject.CargoTypeName = cargo.Replace("_", " ");

                   importObject.ReferenceCode = app.Invoice.ReferenceCode;
                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new NotificationObject();
           }
       }

       public NotificationObject GetNotification(long notificationId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myApplications =
                       db.Notifications.Where(m => m.Id == notificationId)
                           .Include("Importer")
                           .Include("Invoice")
                           .Include("Permit")
                           .Include("Port")
                           .Include("Product")
                           .Include("Depot")
                            .Include("ImportClass")
                           .Include("InvoiceItems")
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


                   importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy");
                   importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                   importObject.QuantityToDischargeStr = app.QuantityToDischarge.ToString("n");
                   importObject.ImporterName = app.Importer.Name;
                   importObject.PermitValue = app.Permit.PermitValue;
                   importObject.ProductCode = app.Product.Code;
                   importObject.NotificationClassName = app.ImportClass.Name;
                   importObject.Rrr = app.Invoice.RRR;
                   importObject.DepotName = app.Depot.Name;
                   importObject.PortName = app.Port.Name;
                   var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                   if (name != null) importObject.StatusStr = name.Replace("_", " ");

                   importObject.PaymentTypeId = app.Invoice.PaymentTypeId;
                   var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                   if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("_", " ");

                   var cargo = Enum.GetName(typeof(CargoTypeEnum), app.CargoInformationTypeId);
                   if (cargo != null) importObject.CargoTypeName = cargo.Replace("_", " ");

                   importObject.ReferenceCode = app.Invoice.ReferenceCode;
                   return importObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new NotificationObject();
           }
       }

       public List<InvoiceObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var invoices =
                       db.Invoices.Where(m => m.ReferenceCode.Contains(searchCriteria) || m.Importer.Name.ToLower().Contains(searchCriteria.ToLower()) || m.RRR.Contains(searchCriteria)).Include("Importer").ToList();
                   if (!invoices.Any())
                    {
                        return new List<InvoiceObject>();
                    }

                   var newList = new List<InvoiceObject>();
                   invoices.ForEach(app =>
                   {
                       var importObject = ModelMapper.Map<Invoice, InvoiceObject>(app);
                       if (importObject != null && importObject.Id > 0)
                       {
                           importObject.DateAddedStr = app.DateAdded.ToString("dd/MM/yyyy");
                           importObject.TotalAmountDueStr = app.TotalAmountDue.ToString("n");
                           var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                           if (name != null) importObject.StatusStr = name.Replace("_", " ");
                           var paymentOption = Enum.GetName(typeof(PaymentOption), app.PaymentTypeId);
                           if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                           importObject.ImporterName = app.Importer.Name;
                           var serviceDescription = Enum.GetName(typeof(ServiceDescriptionEnum), app.ServiceDescriptionId);
                           if (serviceDescription != null) importObject.ServiceDescription = serviceDescription.Replace("_", " ");
                           newList.Add(importObject);
                       }
                   });

                   if (!invoices.Any())
                   {
                       return new List<InvoiceObject>();
                   }
                   return newList;
               }

           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<InvoiceObject>();
           }
       }

       public List<InvoiceObject> Search(string searchCriteria, long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var invoices = db.Invoices.Where(m => m.ReferenceCode == searchCriteria && m.ImporterId == importerId).ToList();
                   if (!invoices.Any())
                       {
                           return new List<InvoiceObject>();
                       }

                   var newList = new List<InvoiceObject>();
                   invoices.ForEach(app =>
                   {
                       var importObject = ModelMapper.Map<Invoice, InvoiceObject>(app);
                       if (importObject != null && importObject.Id > 0)
                       {
                           importObject.DateAddedStr = app.DateAdded.ToString("dd/MM/yyyy");
                           importObject.TotalAmountDueStr = app.TotalAmountDue.ToString("n");
                           var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                           if (name != null) importObject.StatusStr = name.Replace("_", " ");
                           var paymentOption = Enum.GetName(typeof(PaymentOption), app.PaymentTypeId);
                           if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                           var serviceDescription = Enum.GetName(typeof(ServiceDescriptionEnum), app.ServiceDescriptionId);
                           if (serviceDescription != null) importObject.ServiceDescription = serviceDescription.Replace("_", " ");
                           newList.Add(importObject);
                       }
                   });

                   if (!invoices.Any())
                   {
                       return new List<InvoiceObject>();
                   }
                   return newList;
               }

           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<InvoiceObject>();
           }
       }

       public List<InvoiceObject> SearchPaidInvoice(string searchCriteria)
       {
           try
           {
               const int paid = (int)PaymentStatusEnum.Paid;
               using (var db = new ImportPermitEntities())
               {
                   var invoices =
                       db.Invoices.Where(m => m.Status >= paid && (m.ReferenceCode == searchCriteria || m.Importer.Name.ToLower().Contains(searchCriteria.ToLower()))).Include("Importer").ToList();
                   if (!invoices.Any())
                   {
                       return new List<InvoiceObject>();
                   }

                   var newList = new List<InvoiceObject>();
                   invoices.ForEach(app =>
                   {
                       var importObject = ModelMapper.Map<Invoice, InvoiceObject>(app);
                       if (importObject != null && importObject.Id > 0)
                       {
                           importObject.DateAddedStr = app.DateAdded.ToString("dd/MM/yyyy");
                           importObject.TotalAmountDueStr = app.TotalAmountDue.ToString("n");
                           var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                           if (name != null) importObject.StatusStr = name.Replace("_", " ");
                           var paymentOption = Enum.GetName(typeof(PaymentOption), app.PaymentTypeId);
                           if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("8", ",").Replace("_", " ");
                           importObject.ImporterName = app.Importer.Name;
                           var serviceDescription = Enum.GetName(typeof(ServiceDescriptionEnum), app.ServiceDescriptionId);
                           if (serviceDescription != null) importObject.ServiceDescription = serviceDescription.Replace("_", " ");
                           newList.Add(importObject);
                       }
                   });

                   if (!invoices.Any())
                   {
                       return new List<InvoiceObject>();
                   }
                   return newList;
               }

           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<InvoiceObject>();
           }
       }

       public long DeleteInvoice(long invoiceId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems = db.Invoices.Where(m => m.Id == invoiceId).Include("InvoiceItems").ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }
                   var item = myItems[0];
                   if (!item.InvoiceItems.Any())
                   {
                       return 0;
                   }

                   foreach (var o in item.InvoiceItems)
                   {
                       db.InvoiceItems.Remove(o);
                       db.SaveChanges();
                   }
                  
                   db.Invoices.Remove(item);
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






