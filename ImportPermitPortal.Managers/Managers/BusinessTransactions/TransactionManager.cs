using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Net;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.BusinessTransactions
{
    
    public class TransactionManager
    {

        public long AddTransactionInvoice(TransactionInvoiceObject invoice)
        {
            try
            {
                var invoiceEntity = ModelMapper.Map<TransactionInvoiceObject, TransactionInvoice>(invoice);
                if (invoiceEntity == null || invoiceEntity.TotalAmountDue < 1)
                {
                    return 0;
                }

                using (var db = new PPIPSPaymentEntities())
                {
                    var entity = db.TransactionInvoices.Add(invoiceEntity);
                    db.SaveChanges();
                    return entity.Id;
                }

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.InnerException.Message);
               return 0;
            }

        }

        public long AddReceiptAndLog(PaymentLogObject payLog, long transactionInvoiceId) 
        {
            try
            {
                var payLogEntity = ModelMapper.Map<PaymentLogObject, PaymentLog>(payLog);
                if (payLogEntity == null || payLogEntity.PaymentAmount < 1)
                {
                    return 0;
                }

                using (var db = new PPIPSPaymentEntities())
                {
                    payLogEntity.PaymentStatus = PaymentStatusEnum.Paid.ToString();
                    var pLogEntity = db.PaymentLogs.Add(payLogEntity);
                    db.SaveChanges();

                    var receipt = new PaymentReceipt
                    {
                        ReceiptNo = 0,
                        TransactionInvoiceId = transactionInvoiceId,
                        DateCreated = DateTime.Now
                    };

                    var rcpt = db.PaymentReceipts.Add(receipt);
                    db.SaveChanges();

                    rcpt.ReceiptNo = rcpt.Id;
                    db.Entry(rcpt).State = EntityState.Modified;
                    db.SaveChanges();
                    return pLogEntity.Id;
                }

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.InnerException.Message);
                return 0;
            }

        }

        public InvoiceObject AddBankPayment(PaymentLogObject payLog)
        {
            try
            {
                using (var db = new PPIPSPaymentEntities())
                {
                    if (payLog.PaymentAmount < 1)
                    {
                        return new InvoiceObject();
                    }

                    long orderId;
                    var pRes = long.TryParse(payLog.PaymentReference, out orderId);
                    if (!pRes || orderId < 1)
                    {
                        return new InvoiceObject();
                    }

                    var entities = db.TransactionInvoices.Where(l => l.Id == orderId).ToList();
                    if (!entities.Any())
                    {
                        return new InvoiceObject();
                    }

                    var entity = entities[0];

                    const int paid = (int) PaymentStatusEnum.Paid;
                    if (entity.Status == paid && entity.TotalAmountDue.Equals(payLog.PaymentAmount))
                    {
                        var logs = db.PaymentReceipts.Where(r => r.TransactionInvoiceId == orderId).ToList();
                        
                        if (!logs.Any())
                        {
                           var tt = AddReceiptAndLog(payLog, orderId);
                           if (tt < 1)
                           {
                               return new InvoiceObject();
                           }

                           entity.AmountPaid = payLog.PaymentAmount;
                           entity.Status = (int)PaymentStatusEnum.Paid;
                           entity.LastModifiedDate = DateTime.Now;
                           entity.CurrencyCode = payLog.PaymentCurrency;
                           db.Entry(entity).State = EntityState.Modified;
                           db.SaveChanges();
                        }

                        var invoice = GetUpdateInvoice(payLog.PaymentReference);
                        if (invoice == null || invoice.Id < 1)
                        {
                            return new InvoiceObject();
                        }

                        return invoice;
                    }

                    if (!entity.TotalAmountDue.Equals(payLog.PaymentAmount))
                    {
                        var unclearedPayment = new UnclearedPayment
                        {
                            ReferenceCode = payLog.PaymentReference,
                            Amount = payLog.PaymentAmount,
                            CompanyId = payLog.CustomerName,
                            DateAssigned = DateTime.Now
                        };
                        db.UnclearedPayments.Add(unclearedPayment);
                        db.SaveChanges();
                        return new InvoiceObject();
                    }

                    var logStatus = UpdatePaymentDetails(entity.RRR);
                    if (logStatus < 1)
                    {
                        return new InvoiceObject();
                    }

                    var payLogEntity = ModelMapper.Map<PaymentLogObject, PaymentLog>(payLog);
                    if (payLogEntity == null || payLogEntity.PaymentAmount < 1)
                    {
                        return new InvoiceObject();
                    }

                    payLogEntity.PaymentStatus = PaymentStatusEnum.Paid.ToString();
                    var pLogEntity = db.PaymentLogs.Add(payLogEntity);
                    db.SaveChanges();

                    var receipt = new PaymentReceipt
                    {
                        ReceiptNo = 0,
                        TransactionInvoiceId = entity.Id,
                        DateCreated = DateTime.Now
                    };

                    var rcpt = db.PaymentReceipts.Add(receipt);
                    db.SaveChanges();
                    var cc = db.PaymentReceipts.OrderByDescending(r => r.ReceiptNo).Take(1).ToList()[0];
                    rcpt.ReceiptNo = cc.ReceiptNo + 1;
                    db.Entry(rcpt).State = EntityState.Modified;
                    db.SaveChanges();

                    entity.AmountPaid = payLog.PaymentAmount;
                    entity.Status = (int) PaymentStatusEnum.Paid;
                    entity.LastModifiedDate = DateTime.Now;
                    entity.CurrencyCode = payLog.PaymentCurrency;
                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();

                    //update PPIPS Invoice
                    var invStatus = UpdatePpipsInvoiceGetAppInfo(payLog.PaymentAmount, payLog.PaymentReference);

                    //if PPIPS Invoice update fails
                    if (invStatus.Id < 1)
                    {
                        //Revert all changes to maintain consistency
                        UndoUpdatedPpipsInvoice(payLog.PaymentReference);
                        db.PaymentLogs.Remove(pLogEntity);
                        db.SaveChanges();
                        db.PaymentReceipts.Remove(rcpt);
                        db.SaveChanges();
                        
                        entity.AmountPaid = 0;
                        entity.Status = (int)PaymentStatusEnum.Pending;
                        entity.CurrencyCode = "";
                        db.Entry(entity).State = EntityState.Modified;
                        db.SaveChanges();
                        return new InvoiceObject();
                    }
                    
                    return invStatus;
                }


            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.InnerException.Message);
                return new InvoiceObject();
            }
        }
        
        public long AddTransactionPayment(PaymentLogObject payLog)
        {
            try
            {
                using (var db = new PPIPSPaymentEntities())
                {
                    if (payLog.PaymentAmount < 1)
                    {
                        return -2;
                    }

                    long orderId;
                    var pRes = long.TryParse(payLog.PaymentReference, out orderId);
                    if (!pRes || orderId < 1)
                    {
                        return -2;
                    }

                    var entities = db.TransactionInvoices.Where(l => l.Id == orderId).ToList();
                    if (!entities.Any())
                    {
                        return -2;
                    }

                    var entity = entities[0];

                    if (!entity.TotalAmountDue.Equals(payLog.PaymentAmount))
                    {
                        var unclearedPayment = new UnclearedPayment
                        {
                            ReferenceCode = payLog.PaymentReference,
                            Amount = payLog.PaymentAmount,
                            CompanyId = payLog.CustomerName,
                            DateAssigned = DateTime.Now
                        };
                        db.UnclearedPayments.Add(unclearedPayment);
                        db.SaveChanges();
                        return -3;
                    }

                    var logStatus = UpdatePaymentDetails(entity.RRR);
                    if (logStatus < 1)
                    {
                        return -2;
                    }
                    var payLogEntity = ModelMapper.Map<PaymentLogObject, PaymentLog>(payLog);
                    if (payLogEntity == null || payLogEntity.PaymentAmount < 1)
                    {
                        return -2;
                    }

                    payLogEntity.PaymentStatus = PaymentStatusEnum.Paid.ToString();
                    db.PaymentLogs.Add(payLogEntity);
                    db.SaveChanges();

                    var receipt = new PaymentReceipt
                    {
                        ReceiptNo = 0,
                        TransactionInvoiceId = entity.Id,
                        DateCreated = DateTime.Now
                    };

                    var rcpt = db.PaymentReceipts.Add(receipt);
                    db.SaveChanges();

                    var cc = db.PaymentReceipts.OrderByDescending(r => r.ReceiptNo).Take(1).ToList()[0];
                    rcpt.ReceiptNo = cc.ReceiptNo + 1;
                    db.Entry(rcpt).State = EntityState.Modified;
                    db.SaveChanges();

                    entity.AmountPaid = entity.TotalAmountDue;
                    entity.Status = (int)PaymentStatusEnum.Paid;
                    entity.LastModifiedDate = DateTime.Now;
                    entity.CurrencyCode = payLog.PaymentCurrency;
                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();
                    return entity.Id;
                }

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.InnerException.Message);
                return -2;
            }
        }

        public InvoiceObject GetUpdateInvoice(string referenceCode)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var entities = db.Invoices.Where(l => l.ReferenceCode == referenceCode).Include("Importer").ToList();
                    if (!entities.Any())
                    {
                        return new InvoiceObject();
                    }

                    var entity = entities[0];

                    var importObject = new InvoiceObject { FeeObjects = new List<FeeObject>() };
                    var fees = (from i in db.InvoiceItems.Where(f => f.InvoiceId == entity.Id)
                                join f in db.Fees on i.FeeId equals f.FeeId
                                join ft in db.FeeTypes on f.FeeTypeId equals ft.FeeTypeId
                                select new FeeObject
                                {
                                    Amount = f.Amount,
                                    FeeId = f.FeeId,
                                    FeeTypeName = ft.Name,
                                    FeeTypeId = ft.FeeTypeId
                                }).ToList();

                    if (!fees.Any())
                    {
                        return new InvoiceObject();
                    }

                    var users = (from p in db.People.Where(p => p.ImporterId == entity.ImporterId).Include("UserProfiles")
                                 join usp in db.UserProfiles on p.Id equals usp.PersonId
                                 join asp in db.AspNetUsers on usp.Id equals asp.UserInfo_Id
                                 select new { usp, asp }).ToList();

                    if (!users.Any())
                    {
                        return new InvoiceObject();
                    }

                    importObject.Id = entity.Id;
                    importObject.DateAppliedStr = entity.DateAdded.ToString("dd/MM/yyyy");
                    importObject.CompanyName = entity.Importer.Name;
                    importObject.ReferenceCode = entity.ReferenceCode;
                    importObject.RRR = entity.RRR;
                    var id = users[0].usp.Id;
                    var em = users[0].asp.Email;
                    importObject.UserId = id;
                    importObject.Email = em;
                    return importObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new InvoiceObject();
            }
        }

        public InvoiceObject UpdatePpipsInvoiceGetAppInfo(double amountPaid, string referenceCode)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var entities = db.Invoices.Where(l => l.ReferenceCode == referenceCode).Include("Importer").Include("Applications").Include("Notifications").Include("ExpenditionaryInvoices").ToList();
                    if (!entities.Any())
                    {
                        return new InvoiceObject();
                    }

                    var entity = entities[0];
                    var importObject = new InvoiceObject
                    {
                        FeeObjects = new List<FeeObject>(),
                        Bankers = new List<BankObject>(),
                        ImporterName = entity.Importer.Name
                    };
                    
                    if (entity.Applications != null)
                    {
                        if (entity.Applications.Any())
                        {
                            var app = entity.Applications.ToList()[0];
                            importObject.Bankers = GetBankersForApplication(app);

                        }
                    }
                    
                    if (entity.Notifications != null)
                    {
                        if (entity.Notifications.Any())
                        {
                            var ntf = entity.Notifications.ToList()[0];
                            importObject.Bankers = GetBankersForNotification(ntf);
                        }
                    }

                    if (entity.ExpenditionaryInvoices != null)
                    {
                        if (entity.ExpenditionaryInvoices.Any())
                        {
                            var exp = entity.ExpenditionaryInvoices.ToList()[0];

                            var ntfs = db.Notifications.Where(h => h.Id == exp.NotificationId).ToList();

                            if (ntfs.Any())
                            {
                                var ntf = ntfs[0];
                                ntf.Status = (int)PaymentStatusEnum.Paid;
                                db.Entry(ntf).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }

                    entity.AmountPaid = amountPaid;
                    entity.Status = (int)PaymentStatusEnum.Paid;
                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();

                    
                    var fees = (from i in db.InvoiceItems.Where(f => f.InvoiceId == entity.Id)
                                join f in db.Fees on i.FeeId equals f.FeeId
                                join ft in db.FeeTypes on f.FeeTypeId equals ft.FeeTypeId
                                select new FeeObject
                                {
                                    Amount = f.Amount,
                                    FeeId = f.FeeId,
                                    FeeTypeName = ft.Name,
                                    FeeTypeId = ft.FeeTypeId
                                }).ToList();

                    if (!fees.Any())
                    {
                        return new InvoiceObject();
                    }

                    var users = (from p in db.People.Where(p => p.ImporterId == entity.ImporterId).Include("UserProfiles")
                                 join usp in db.UserProfiles on p.Id equals usp.PersonId
                                 join asp in db.AspNetUsers on usp.Id equals asp.UserInfo_Id
                                 select new { usp, asp }).ToList();

                    if (!users.Any())
                    {
                        return new InvoiceObject();
                    }

                    importObject.Id = entity.Id;
                    importObject.DateAppliedStr = entity.DateAdded.ToString("yyyy-MM-dd");
                    importObject.CompanyName = entity.Importer.Name;
                    importObject.ReferenceCode = entity.ReferenceCode;
                    importObject.RRR = entity.RRR;
                    var id = users[0].usp.Id;
                    var em = users[0].asp.Email;
                    importObject.UserId = id;
                    importObject.Email = em;
                    importObject.PhoneNumber = users[0].asp.PhoneNumber;
                    return importObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new InvoiceObject();
            }
        }

       
        public List<BankObject> GetBankersForApplication(Application app)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var appItems = db.ApplicationItems.Where(k => k.ApplicationId == app.Id).ToList();
                    var bankersList = new List<BankObject>();
                    if (appItems.Any())
                    {
                        appItems.ForEach(u =>
                        {
                            var bankers = db.ProductBankers.Where(i => i.ApplicationItemId == u.Id).Include("Bank").ToList();
                            if (appItems.Any())
                            {
                                bankers.ForEach(c =>
                                {
                                    var exs = bankersList.Find(b => b.BankId == c.BankId);

                                    if (exs == null || exs.BankId < 1)
                                    {
                                        var emailIds = (from im in db.Importers.Where(i => i.Id == c.Bank.ImporterId)
                                                        join p in db.People on im.Id equals p.ImporterId
                                                        join usp in db.UserProfiles.Where(o => o.Person.IsAdmin == true)
                                                        on p.Id equals usp.PersonId
                                                        join asp in db.AspNetUsers on usp.Id equals asp.UserInfo_Id
                                                        select new { asp.Email, usp.Id }).ToList();

                                        if (emailIds.Any())
                                        {
                                            var tkk = emailIds[0].Email;
                                            var rrt = emailIds[0].Id;

                                            var pdBnk = new BankObject
                                            {
                                                BankId = c.BankId,
                                                Name = c.Bank.Name,
                                                ProductCode = u.Product.Code,
                                                Email = emailIds.Any() ? tkk : null,
                                                NotificationEmail = !string.IsNullOrEmpty(c.Bank.NotificationEmail) ? c.Bank.NotificationEmail : null,
                                                UserProfileId = rrt
                                            };

                                            bankersList.Add(pdBnk);
                                        }
                                    }
                                    else
                                    {
                                        exs.ProductCode += ", " + u.Product.Code;
                                    }
                                });

                            }
                        });
                    }

                   return bankersList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<BankObject>();
            }
        }

        public List<BankObject> GetBankersForNotification(Notification ntf)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {

                    var bankers = (from p in db.Permits.Where(p => p.Notifications.Any(n => n.Id == ntf.Id))
                                   join pa in db.PermitApplications on p.Id equals pa.PermitId
                                   join apItem in db.ApplicationItems.Where(o => o.ProductId == ntf.ProductId).Include("Product") on pa.ApplicationId equals apItem.ApplicationId
                                   join pb in db.ProductBankers.Include("Bank") on apItem.Id equals pb.ApplicationItemId
                                   select new BankObject
                                   {
                                       BankId = pb.Bank.BankId,
                                       ImporterId = pb.Bank.ImporterId,
                                       Name = pb.Bank.Name,
                                       ProductCode = apItem.Product.Code,
                                       NotificationEmail = pb.Bank.NotificationEmail
                                   }).ToList();


                    bankers.ForEach(c =>
                    {

                        var emailIds = (from im in db.Importers.Where(i => i.Id == c.ImporterId)
                                        join p in db.People on im.Id equals p.ImporterId
                                        join usp in db.UserProfiles.Where(o => o.Person.IsAdmin == true)
                                        on p.Id equals usp.PersonId
                                        join asp in db.AspNetUsers on usp.Id equals asp.UserInfo_Id
                                        select new { asp.Email, usp.Id }).ToList();

                        if (emailIds.Any())
                        {
                            var tkk = emailIds[0].Email;
                            var rrt = emailIds[0].Id;

                            c.Email = emailIds.Any() ? tkk : null;
                            c.NotificationEmail = !string.IsNullOrEmpty(c.NotificationEmail) ? c.NotificationEmail : null;
                            c.UserProfileId = rrt;
                        }

                    });


                    return bankers;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<BankObject>();
            }
        }

        public bool UndoUpdatedPpipsInvoice(string referenceCode)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var entities = db.Invoices.Where(l => l.ReferenceCode == referenceCode).Include("Importer").Include("Applications").Include("Notifications").Include("ExpenditionaryInvoices").ToList();
                    if (!entities.Any())
                    {
                        return false;
                    }

                    var entity = entities[0];

                    if (entity.Applications.Any())
                    {
                        var app = entity.Applications.ToList()[0];
                        if (app != null && app.Id > 0)
                        {
                            app.ApplicationStatusCode = (int)PaymentStatusEnum.Pending;
                            db.Entry(app).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    if (entity.Notifications.Any())
                    {
                        var ntf = entity.Notifications.ToList()[0];
                        if (ntf != null && ntf.Id > 0)
                        {
                            ntf.Status = (int)PaymentStatusEnum.Pending;
                            db.Entry(ntf).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    if (entity.ExpenditionaryInvoices.Any())
                    {
                        var exp = entity.ExpenditionaryInvoices.ToList()[0];

                        var ntfs = db.Notifications.Where(h => h.Id == exp.NotificationId).ToList();

                        if (ntfs.Any())
                        {
                            var ntf = ntfs[0];
                            ntf.Status = (int)PaymentStatusEnum.Pending;
                            db.Entry(ntf).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    entity.AmountPaid = 0;
                    entity.Status = (int)PaymentStatusEnum.Pending;
                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return false;
            }
        }

        public ApplicationObject AddApplicationWebPayment(string code)  
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
                    var users = (from p in db.People.Where(p => p.ImporterId == app.ImporterId).Include("UserProfiles") 
                                join usp in db.UserProfiles on p.Id equals usp.PersonId
                                join asp in db.AspNetUsers on usp.Id equals asp.UserInfo_Id
                                select new { usp, asp}).ToList();

                    if (!users.Any())
                    {
                        return new ApplicationObject();
                    }

                    var pType = Enum.GetName(typeof (PaymentOption), app.Invoice.PaymentTypeId);
                    var opt = "";
                    if (pType != null)
                    {
                        opt = pType.Replace("_", " ");
                    }
                    
                    var desc = Enum.GetName(typeof (ServiceDescriptionEnum), app.Invoice.ServiceDescriptionId);
                    var descStr = "";
                    if (desc != null)
                    {
                        descStr = desc.Replace("_", " ");
                    }

                    if (app.Invoice.Status < 2 && app.ApplicationStatusCode < 2)
                    {
                        var payLog = new PaymentLogObject
                        {
                            Type = true,
                            PaymentReference = app.Invoice.ReferenceCode,
                            PaymentAmount = app.Invoice.TotalAmountDue,
                            PaymentDate = DateTime.Today.ToString(CultureInfo.InvariantCulture),
                            PaymentMethod = opt,
                            CustomerName = app.Importer.Name,
                            Rrr = app.Invoice.RRR
                        };

                        var tt = AddTransactionPayment(payLog);
                        if (tt < 1)
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
                    importObject.ReferenceCode = app.Invoice.ReferenceCode;
                    importObject.Rrr = app.Invoice.RRR;
                    importObject.AmountPaid = app.Invoice.TotalAmountDue;
                    importObject.ServiceDescription = descStr;
                    importObject.ApplicationStatusCode = (int)PaymentStatusEnum.Paid;
                    importObject.StatusStr = PaymentStatusEnum.Paid.ToString();
                    
                    importObject.ApplicationItemObjects = new List<ApplicationItemObject>();

                    importObject.FeeObjects = new List<FeeObject>();
                    var fees = (from i in db.InvoiceItems.Where(f => f.InvoiceId == importObject.InvoiceId)
                                join f in db.Fees on i.FeeId equals f.FeeId
                                join ft in db.FeeTypes on f.FeeTypeId equals ft.FeeTypeId
                                select new FeeObject
                                {
                                    Amount = f.Amount,
                                    FeeId = f.FeeId,
                                    FeeTypeName = ft.Name,
                                    FeeTypeId = ft.FeeTypeId

                                }).ToList();
                    if (!fees.Any())
                    {
                        return new ApplicationObject();
                    }
                    
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
                        if (fee.FeeTypeId == statutoryFeeId)
                        {
                            fee.FeeTypeName = fee.FeeTypeName + " (" + WebUtility.HtmlDecode("&#8358;") + fee.Amount.ToString("n0") + "/" + "30,000 MT)";
                            fee.Amount = app.Invoice.TotalAmountDue - processingFee.Amount;
                            fee.AmountStr = fee.Amount.ToString("n0");
                        }
                        else
                        {
                            fee.AmountStr = fee.Amount.ToString("n0");
                        }
                        
                    });


                    importObject.FeeObjects = fees;
                    importObject.Bankers = new List<BankObject>();
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
                            im.EstimatedQuantityStr = im.EstimatedQuantity.ToString("n");
                            im.EstimatedValueStr = im.EstimatedValue.ToString("n");
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

                            importObject.ApplicationItemObjects.Add(im);
                        }
                    });

                    importObject.Bankers = GetBankersForApplication(app);
                    var id = users[0].usp.Id;
                    var em = users[0].asp.Email;
                    importObject.UserId = id;
                    importObject.Email = em;
                    return importObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.InnerException.Message);
                return new ApplicationObject();
            }
        }

        public InvoiceObject InsertPayment(string rrr, string orderId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {

                    var entities = db.Invoices.Where(l => l.ReferenceCode == orderId && l.RRR == rrr).Include("Importer").Include("Applications").Include("Notifications").Include("ExpenditionaryInvoices").ToList();
                    if (!entities.Any())
                    {
                        return new InvoiceObject();
                    }

                    var entity = entities[0];

                    var pType = Enum.GetName(typeof(PaymentOption), entity.PaymentTypeId);
                    var opt = "";
                    if (pType != null)
                    {
                        opt = pType.Replace("_", " ");
                    }

                    var desc = Enum.GetName(typeof(ServiceDescriptionEnum), entity.ServiceDescriptionId);
                    var descStr = "";
                    if (desc != null)
                    {
                        descStr = desc.Replace("_", " ");
                    }

                    if (entity.Status < 2)
                    {
                        var payLog = new PaymentLogObject
                        {
                            Type = true,
                            PaymentReference = entity.ReferenceCode,
                            PaymentAmount = entity.TotalAmountDue,
                            PaymentDate = DateTime.Today.ToString(CultureInfo.InvariantCulture),
                            PaymentMethod = opt,
                            CustomerName = entity.Importer.Name,
                            Rrr = entity.RRR
                        };

                        var tt = AddTransactionPayment(payLog);
                        if (tt < 1)
                        {
                            return new InvoiceObject();
                        }
                    }

                    if (entity.Applications != null)
                    {
                        if (entity.Applications.Any())
                        {
                            var app = entity.Applications.ToList()[0];
                            if (app != null && app.Id > 0)
                            {
                                app.ApplicationStatusCode = (int)PaymentStatusEnum.Paid;
                                db.Entry(app).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }

                    if (entity.Notifications != null)
                    {
                        if (entity.Notifications.Any())
                        {
                            var ntf = entity.Notifications.ToList()[0];
                            if (ntf != null && ntf.Id > 0)
                            {
                                ntf.Status = (int)PaymentStatusEnum.Paid;
                                db.Entry(ntf).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }

                    if (entity.ExpenditionaryInvoices != null)
                    {
                        if (entity.ExpenditionaryInvoices.Any())
                        {
                            var exp = entity.ExpenditionaryInvoices.ToList()[0];

                            var ntfs = db.Notifications.Where(h => h.Id == exp.NotificationId).ToList();

                            if (ntfs.Any())
                            {
                                var ntf = ntfs[0];
                                ntf.Status = (int)PaymentStatusEnum.Paid;
                                db.Entry(ntf).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }

                    entity.AmountPaid = entity.TotalAmountDue;
                    entity.Status = (int)PaymentStatusEnum.Paid;
                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();

                    var users = (from p in db.People.Where(p => p.ImporterId == entity.ImporterId).Include("UserProfiles")
                                 join usp in db.UserProfiles on p.Id equals usp.PersonId
                                 join asp in db.AspNetUsers on usp.Id equals asp.UserInfo_Id
                                 select new { usp, asp }).ToList();

                    if (!users.Any())
                    {
                        return new InvoiceObject();
                    }

                    var importObject = ModelMapper.Map<Invoice, InvoiceObject>(entity);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new InvoiceObject();
                    }


                    importObject.CompanyName = entity.Importer.Name;
                    importObject.PaymentTypeId = entity.PaymentTypeId;
                    importObject.ReferenceCode = entity.ReferenceCode;
                    importObject.RRR = entity.RRR;
                    importObject.ServiceDescription = descStr;
                    importObject.Status = (int)PaymentStatusEnum.Paid;
                    importObject.StatusStr = PaymentStatusEnum.Paid.ToString();
                    importObject.ApplicationItemObjects = new List<ApplicationItemObject>();

                    importObject.FeeObjects = new List<FeeObject>();
                    var fees = (from i in db.InvoiceItems.Where(f => f.InvoiceId == importObject.Id)
                                join f in db.Fees on i.FeeId equals f.FeeId
                                join ft in db.FeeTypes on f.FeeTypeId equals ft.FeeTypeId
                                select new FeeObject
                                {
                                    Amount = f.Amount,
                                    FeeId = f.FeeId,
                                    FeeTypeName = ft.Name,
                                    FeeTypeId = ft.FeeTypeId

                                }).ToList();

                    if (!fees.Any())
                    {
                        return new InvoiceObject();
                    }

                    const int statutoryFeeId = (int)FeeTypeEnum.Statutory_Fee;
                    const int processingFeeId = (int)FeeTypeEnum.Processing_Fee;
                    var statutoryFee = fees.Find(m => m.FeeTypeId == statutoryFeeId);
                    var processingFee = fees.Find(m => m.FeeTypeId == processingFeeId);
                    if (statutoryFee == null || statutoryFee.FeeId < 1)
                    {
                        return new InvoiceObject();
                    }
                    fees.ForEach(fee =>
                    {
                        if (fee.FeeTypeId == statutoryFeeId)
                        {
                            fee.FeeTypeName = fee.FeeTypeName + " (" + WebUtility.HtmlDecode("&#8358;") + fee.Amount.ToString("n0") + "/" + "30,000 MT)";
                            fee.Amount = entity.TotalAmountDue - processingFee.Amount;
                            fee.AmountStr = fee.Amount.ToString("n0");
                        }
                        else
                        {
                            fee.AmountStr = fee.Amount.ToString("n0");
                        }

                    });


                    importObject.FeeObjects = fees;
                    var id = users[0].usp.Id;
                    var em = users[0].asp.Email;
                    importObject.UserId = id;
                    importObject.Email = em;
                    return importObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.InnerException.Message);
                return new InvoiceObject();
            }
        }
        public NotificationObject AddNotificationWebPayment(string code)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myApplications = db.Notifications.Where(m => m.Invoice.ReferenceCode == code.Trim())
                           .Include("Importer")
                           .Include("Invoice")
                           .Include("Permit")
                           .Include("Port")
                           .Include("Product")
                           .Include("Depot")
                           .ToList();
                    if (!myApplications.Any())
                    {
                        return new NotificationObject();
                    }

                    var app = myApplications[0];
                    var pType = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                    var opt = "";
                    if (pType != null)
                    {
                        opt = pType.Replace("_", " ");
                    }

                    var desc = Enum.GetName(typeof(ServiceDescriptionEnum), app.Invoice.ServiceDescriptionId);
                    var descStr = "";
                    if (desc != null)
                    {
                        descStr = desc.Replace("_", " ");
                    }

                    if (app.Status < 2 && app.Invoice.Status < 1)
                    {
                        var payLog = new PaymentLogObject
                        {
                            Type = true,
                            PaymentReference = app.Invoice.ReferenceCode,
                            PaymentAmount = app.Invoice.TotalAmountDue,
                            PaymentDate = DateTime.Today.ToString(CultureInfo.InvariantCulture),
                            PaymentMethod = opt,
                            CustomerName = app.Importer.Name,
                            Rrr = app.Invoice.RRR
                        };

                        var tt = AddTransactionPayment(payLog);
                        if (tt < 1)
                        {
                            return new NotificationObject();
                        }

                        app.Invoice.AmountPaid = app.Invoice.TotalAmountDue;
                        app.Invoice.Status = (int)PaymentStatusEnum.Paid;
                        db.Entry(app.Invoice).State = EntityState.Modified;
                        db.SaveChanges();

                        app.Status = (int)PaymentStatusEnum.Paid;
                        db.Entry(app).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var importObject = ModelMapper.Map<Notification, NotificationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new NotificationObject();
                    }
                    
                    importObject.ArrivalDateStr = app.ArrivalDate.ToString("dd/MM/yyyy");
                    importObject.DischargeDateStr = app.DischargeDate.ToString("dd/MM/yyyy");
                    importObject.DateCreatedStr = app.DateCreated.ToString("dd/MM/yyyy");
                    importObject.ImporterName = app.Importer.Name;
                    importObject.PermitValue = app.Permit.PermitValue;
                    importObject.AmountPaidStr = app.Invoice.TotalAmountDue.ToString("n1");
                    importObject.QuantityOnVesselStr = importObject.QuantityOnVessel.ToString("n1");
                    importObject.QuantityToDischargeStr = importObject.QuantityToDischarge.ToString("n1");
                    importObject.ProductCode = app.Product.Code;
                    importObject.NotificationClassName = app.ImportClass.Name;
                    importObject.ServiceDescription = descStr;
                    importObject.DepotName = app.Depot.Name;
                    importObject.PortName = app.Port.Name;
                    var name = Enum.GetName(typeof(AppStatus), importObject.Status);
                    if (name != null) importObject.StatusStr = name.Replace("_", " ");
                    var cargo = Enum.GetName(typeof(CargoTypeEnum), app.CargoInformationTypeId);
                    if (cargo != null) importObject.CargoTypeName = cargo.Replace("_", " ");
                    importObject.PaymentTypeId = app.Invoice.PaymentTypeId;
                    var paymentOption = Enum.GetName(typeof(PaymentOption), app.Invoice.PaymentTypeId);
                    if (paymentOption != null) importObject.PaymentOption = paymentOption.Replace("_", " ");
                    importObject.Rrr = app.Invoice.RRR;
                    importObject.ReferenceCode = app.Invoice.ReferenceCode;

                    var fees = (from i in db.InvoiceItems.Where(f => f.InvoiceId == importObject.InvoiceId)
                          join f in db.Fees on i.FeeId equals f.FeeId
                          join ft in db.FeeTypes on f.FeeTypeId equals ft.FeeTypeId
                          select new FeeObject
                          {
                              Amount = i.AmountDue,
                              FeeTypeName = ft.Name,
                              FeeTypeId = ft.FeeTypeId

                          }).ToList();
                    if (!fees.Any())
                    {
                        return new NotificationObject();
                    }

                    importObject.FeeObjects = new List<FeeObject>();
                    fees.ForEach(o =>
                    {
                        o.AmountStr = o.Amount.ToString("n");
                    });
                    importObject.FeeObjects = fees;

                    var users = (from p in db.People.Where(p => p.ImporterId == app.ImporterId).Include("UserProfiles")
                                 join usp in db.UserProfiles on p.Id equals usp.PersonId
                                 join asp in db.AspNetUsers on usp.Id equals asp.UserInfo_Id
                                 select new { usp, asp }).ToList();

                    if (!users.Any())
                    {
                        return new NotificationObject();
                    }

                    var im = app.Product;
                    importObject.ProductObject = new ProductObject {Code = im.Code, Name = im.Name};
                    importObject.Bankers = GetBankersForNotification(app);
                    var id = users[0].usp.Id;
                    var em = users[0].asp.Email;
                    importObject.UserId = id;
                    importObject.Email = em; 

                    return importObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.InnerException.Message);
                return new NotificationObject();
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

        public long CheckRrrValidity(string rrrCode, string paymentDate)
        {
            try
            {
                using (var db = new PPIPSPaymentEntities())
                {

                    var entities = db.TransactionInvoices.Where(l => l.RRR == rrrCode).ToList();
                    if (entities.Any())
                    {
                        return -2;
                    }

                    var entity = entities[0];
                    if (entity.ExpiryDate != null)
                    {
                        DateTime date;
                        var dateres = DateTime.TryParse(entity.ExpiryDate.ToString(), out date);
                        if (!dateres || (date > entity.ExpiryDate))
                        {
                            return -4;
                        }

                        if (date  <= entity.ExpiryDate)
                        {
                            var dd = (entity.ExpiryDate - date).Value.Days;
                            if (dd < 7)
                            {
                                return -4;
                            }
                        }
                        
                        return 5;
                    }

                    return -2;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return -2;
            }
        }

        public long UpdatePaymentDetails(string rrr)
        {
            try
            {
                if (string.IsNullOrEmpty(rrr))
                {
                    return -2;
                }

                using (var db = new PPIPSPaymentEntities())
                {
                   var summaries = db.PaymentDistributionSummaries.Where(r => r.PaymentReference == rrr).ToList();
                    if (!summaries.Any())
                    {
                        return -2;
                    }
                    foreach (var fee in summaries)
                    {
                        fee.PaymentDate = DateTime.Today;
                        fee.Status = true;
                        db.Entry(fee).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    
                }
                return 5;
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

        public PaymentReceiptObject GetMyReceipt(long receiptId, long importerId)
        {
            try
            {
                using (var db = new PPIPSPaymentEntities())
                {
                    var myReceipts = db.PaymentReceipts.Where(v => v.Id == receiptId && v.TransactionInvoice.CustomerId == importerId).Include("TransactionInvoice").ToList();
                    if (!myReceipts.Any())
                    {
                        return new PaymentReceiptObject();
                    }
                    var receipt = myReceipts[0];
                   
                    var receiptObject = ModelMapper.Map<PaymentReceipt, PaymentReceiptObject>(receipt);
                    if (receiptObject != null && receiptObject.Id > 0)
                    {
                        using (var db2 = new ImportPermitEntities())
                        {
                            var customers = db2.Importers.Where(i => i.Id == receipt.TransactionInvoice.CustomerId).ToList();
                            if (customers.Any())
                            {
                                receiptObject.DateAddedStr = receipt.DateCreated.ToString("dd/MM/yyyy");
                                receiptObject.TotalAmountDueStr = receipt.TransactionInvoice.AmountPaid.ToString("n");
                                receiptObject.TotalAmountDue = receipt.TransactionInvoice.AmountPaid;
                                receiptObject.PaymentOption = receipt.TransactionInvoice.PaymentMethod.Replace("8", ",").Replace("_", " ");
                                receiptObject.ServiceDescription = receipt.TransactionInvoice.ServiceDescription.Replace("_", " ");
                                receiptObject.ImporterName = customers[0].Name;
                                receiptObject.RRR = receipt.TransactionInvoice.RRR;
                                receiptObject.ReceiptNoStr = GenerateReceiptNumber(receiptObject.ReceiptNo);
                                receiptObject.Number = "2" + DateTime.Now.Year +  DateTime.Now.Month + receipt.TransactionInvoice.Id;
                                return receiptObject;
                            }

                        }
                    }
                    
                }
                return new PaymentReceiptObject();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new PaymentReceiptObject();
            }
        }

        public List<PaymentReceiptObject> GetMyReceipts(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;
                    using (var db = new PPIPSPaymentEntities())
                    {
                        var myReceipts = db.PaymentReceipts.Where(v => v.TransactionInvoice.CustomerId == importerId).Include("TransactionInvoice").
                            OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize).ToList();
                        if (!myReceipts.Any())
                        {
                            countG = 0;
                            return new List<PaymentReceiptObject>();
                        }

                        var newList = new List<PaymentReceiptObject>();
                        myReceipts.ForEach(receipt =>
                        {
                            var receiptObject = ModelMapper.Map<PaymentReceipt, PaymentReceiptObject>(receipt);
                            if (receiptObject != null && receiptObject.Id > 0)
                            {
                                receiptObject.DateAddedStr = receipt.DateCreated.ToString("dd/MM/yyyy");
                                receiptObject.TotalAmountDueStr = receipt.TransactionInvoice.AmountPaid.ToString("n");
                                receiptObject.PaymentOption = receipt.TransactionInvoice.PaymentMethod.Replace("8", ",").Replace("_", " ");
                                receiptObject.ServiceDescription = receipt.TransactionInvoice.ServiceDescription.Replace("_", " ");
                                receiptObject.RRR = receipt.TransactionInvoice.RRR;
                                receiptObject.ReceiptNoStr = GenerateReceiptNumber(receiptObject.ReceiptNo);
                                newList.Add(receiptObject);
                            }
                        });

                        countG = db.PaymentReceipts.Count(v => v.TransactionInvoice.CustomerId == importerId);
                        return newList;
                    }
                }
                countG = 0;
                return new List<PaymentReceiptObject>();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<PaymentReceiptObject>();
            }
        }

        public List<PaymentReceiptObject> GetReceipts(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;
                    using (var db = new PPIPSPaymentEntities())
                    {
                        var myReceipts = db.PaymentReceipts.Include("TransactionInvoice").
                            OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize).ToList();
                        if (!myReceipts.Any())
                        {
                            countG = 0;
                            return new List<PaymentReceiptObject>();
                        }

                        var newList = new List<PaymentReceiptObject>();
                        myReceipts.ForEach(receipt =>
                        {
                            var receiptObject = ModelMapper.Map<PaymentReceipt, PaymentReceiptObject>(receipt);
                            if (receiptObject != null && receiptObject.Id > 0)
                            {
                                receiptObject.DateAddedStr = receipt.DateCreated.ToString("dd/MM/yyyy");
                                receiptObject.TotalAmountDueStr = receipt.TransactionInvoice.TotalAmountDue.ToString("n");
                                receiptObject.TotalAmountPaidStr =  receipt.TransactionInvoice.AmountPaid.ToString("n");
                                receiptObject.PaymentOption = receipt.TransactionInvoice.PaymentMethod.Replace("8", ",").Replace("_", " ");
                                receiptObject.ServiceDescription = receipt.TransactionInvoice.ServiceDescription.Replace("_", " ");
                                receiptObject.RRR = receipt.TransactionInvoice.RRR;
                                receiptObject.ReceiptNoStr = GenerateReceiptNumber(receiptObject.ReceiptNo);
                                using (var dbe = new ImportPermitEntities())
                                {
                                    var customers = dbe.Importers.Where(i => i.Id == receipt.TransactionInvoice.CustomerId).ToList();
                                    if (customers.Any())
                                    {
                                        receiptObject.ImporterName = customers[0].Name;
                                        newList.Add(receiptObject);
                                    }
                                }
                               
                            }
                        });

                        countG = db.PaymentReceipts.Count();
                        return newList;
                    }
                }
                countG = 0;
                return new List<PaymentReceiptObject>();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<PaymentReceiptObject>();
            }
        }
        
        public List<PaymentReceiptObject> GetReceiptSummary(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var db = new PPIPSPaymentEntities())
                {
                    var myReceipts = db.PaymentReceipts.Where(v => v.DateCreated >= startDate && v.DateCreated <= endDate).Include("TransactionInvoice").
                                     OrderByDescending(m => m.Id).ToList();

                    if (!myReceipts.Any())
                    {
                        return new List<PaymentReceiptObject>();
                    }

                    var newList = new List<PaymentReceiptObject>();
                    myReceipts.ForEach(receipt =>
                    {
                        var receiptObject = ModelMapper.Map<PaymentReceipt, PaymentReceiptObject>(receipt);
                        if (receiptObject != null && receiptObject.Id > 0)
                        {
                            receiptObject.DateAddedStr = receipt.DateCreated.ToString("dd/MM/yyyy");
                            receiptObject.TotalAmountDueStr = receipt.TransactionInvoice.AmountPaid.ToString("n");
                            receiptObject.PaymentOption = receipt.TransactionInvoice.PaymentMethod.Replace("8", ",").Replace("_", " ");
                            receiptObject.ServiceDescription = receipt.TransactionInvoice.ServiceDescription.Replace("_", " ");
                            receiptObject.RRR = receipt.TransactionInvoice.RRR;
                            newList.Add(receiptObject);
                        }
                    });

                    return newList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<PaymentReceiptObject>();
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

        public List<PaymentReceiptObject> SearchMyReceipts(string searchCriteria, long importerId)
        {
            try
            {
                using (var db = new PPIPSPaymentEntities())
                {
                    var myReceipts = db.PaymentReceipts.Where(m => m.TransactionInvoice.RRR.Contains(searchCriteria.ToLower()) && m.TransactionInvoice.CustomerId == importerId).Include("TransactionInvoice").ToList();
                    if (!myReceipts.Any())
                    {
                        return new List<PaymentReceiptObject>();
                    }

                    var newList = new List<PaymentReceiptObject>();
                    myReceipts.ForEach(receipt =>
                    {
                        var receiptObject = ModelMapper.Map<PaymentReceipt, PaymentReceiptObject>(receipt);
                        if (receiptObject != null && receiptObject.Id > 0)
                        {
                            receiptObject.DateAddedStr = receipt.DateCreated.ToString("dd/MM/yyyy");
                            receiptObject.TotalAmountDueStr = receipt.TransactionInvoice.AmountPaid.ToString("n");
                            receiptObject.PaymentOption = receipt.TransactionInvoice.PaymentMethod.Replace("8", ",").Replace("_", " ");
                            receiptObject.ServiceDescription = receipt.TransactionInvoice.ServiceDescription.Replace("_", " ");
                            receiptObject.RRR = receipt.TransactionInvoice.RRR;
                            newList.Add(receiptObject);
                        }
                    });

                    if (!myReceipts.Any())
                    {
                        return new List<PaymentReceiptObject>();
                    }
                    return newList;
                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<PaymentReceiptObject>();
            }
        }

        public List<PaymentReceiptObject> SearchReceipts(string searchCriteria)
        {
            try
            {
                using (var db = new PPIPSPaymentEntities())
                {
                    var myReceipts = db.PaymentReceipts.Where(m => m.TransactionInvoice.RRR.Contains(searchCriteria.ToLower())).Include("TransactionInvoice").ToList();
                    if (!myReceipts.Any())
                    {
                        return new List<PaymentReceiptObject>();
                    }

                    var newList = new List<PaymentReceiptObject>();
                    myReceipts.ForEach(receipt =>
                    {
                        var receiptObject = ModelMapper.Map<PaymentReceipt, PaymentReceiptObject>(receipt);
                        if (receiptObject != null && receiptObject.Id > 0)
                        {
                            receiptObject.DateAddedStr = receipt.DateCreated.ToString("dd/MM/yyyy");
                            receiptObject.TotalAmountDueStr = receipt.TransactionInvoice.TotalAmountDue.ToString("n");
                            receiptObject.TotalAmountPaidStr = receipt.TransactionInvoice.AmountPaid.ToString("n");
                            receiptObject.PaymentOption = receipt.TransactionInvoice.PaymentMethod.Replace("8", ",").Replace("_", " ");
                            receiptObject.ServiceDescription = receipt.TransactionInvoice.ServiceDescription.Replace("_", " ");
                            receiptObject.RRR = receipt.TransactionInvoice.RRR;
                            receiptObject.ReceiptNoStr = GenerateReceiptNumber(receiptObject.ReceiptNo);
                            using (var dbe = new ImportPermitEntities())
                            {
                                var customers = dbe.Importers.Where(i => i.Id == receipt.TransactionInvoice.CustomerId).ToList();
                                if (customers.Any())
                                {
                                    receiptObject.ImporterName = customers[0].Name;
                                    newList.Add(receiptObject);
                                }
                            }

                        }
                    });

                    if (!myReceipts.Any())
                    {
                        return new List<PaymentReceiptObject>();
                    }
                    return newList;
                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<PaymentReceiptObject>();
            }
        }
    }
}
