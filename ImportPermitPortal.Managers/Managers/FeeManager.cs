using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
   public class FeeManager
    {

       public long AddFee(FeeObject fee)
       {
           try
           {
               if (fee == null)
               {
                   return -2;
               }

               var feeEntity = ModelMapper.Map<FeeObject, Fee>(fee);
               if (feeEntity == null || feeEntity.FeeTypeId < 1)
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.Fees.Add(feeEntity);
                   db.SaveChanges();
                   return returnStatus.FeeId;
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

       public long UpdateFee(FeeObject fee)
       {
           try
           {
               if (fee == null)
               {
                   return -2;
               }

               var feeEntity = ModelMapper.Map<FeeObject, Fee>(fee);
               if (feeEntity == null || feeEntity.FeeId < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.Fees.Attach(feeEntity);
                   db.Entry(feeEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return fee.FeeId;
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
       public List<FeeObject> GetFees()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var fees = db.Fees.ToList();
                   if (!fees.Any())
                   {
                        return new List<FeeObject>();
                   }
                   var objList =  new List<FeeObject>();
                   fees.ForEach(app =>
                   {
                       var feeObject = ModelMapper.Map<Fee, FeeObject>(app);
                       if (feeObject != null && feeObject.FeeId > 0)
                       {
                           feeObject.AmountStr = feeObject.Amount.ToString("n1");
                           objList.Add(feeObject);
                       }
                   });

                   return !objList.Any() ? new List<FeeObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

       public List<FeeObject> GetAppliationStageFees(out CalculationFactor calculationFactor)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int appStage = (int) AppStage.Application;
                   var fees = db.Fees.Where(k => k.ImportStageId == appStage).Include("FeeType").Include("ImportStage").ToList();
                   if (!fees.Any())
                   {
                       calculationFactor = new CalculationFactor();
                       return new List<FeeObject>();
                   }
                   var priceVolumeThresholds = db.ImportSettings.ToList();
                   if (!priceVolumeThresholds.Any())
                   {
                       calculationFactor = new CalculationFactor();
                       return new List<FeeObject>();
                   }

                   var priceVolumeThreshold = priceVolumeThresholds[0].PriceVolumeThreshold;
                   const int statutoryFeeId = (int) FeeTypeEnum.Statutory_Fee;
                   var statutoryFee = fees.Find(m => m.FeeTypeId == statutoryFeeId);
                   if (statutoryFee == null || statutoryFee.FeeId < 1)
                   {
                       calculationFactor = new CalculationFactor();
                       return new List<FeeObject>();
                   }

                   calculationFactor = new CalculationFactor
                   {
                       Fees = statutoryFee.Amount,
                       PriceVolumeThreshold = priceVolumeThreshold
                   };

                   var objList = new List<FeeObject>();
                   fees.ForEach(app =>
                   {
                       var feeObject = ModelMapper.Map<Fee, FeeObject>(app);
                       if (feeObject != null && feeObject.FeeId > 0)
                       {
                           if (feeObject.FeeTypeId == statutoryFeeId)
                           {
                               feeObject.FeeTypeName = app.FeeType.Name ;
                           }
                           else
                           {
                               feeObject.FeeTypeName = app.FeeType.Name;
                           }
                           
                           feeObject.ImportStageName = app.ImportStage.Name;
                           objList.Add(feeObject);
                       }
                   });

                   return !objList.Any() ? new List<FeeObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               calculationFactor = new CalculationFactor();
               return new List<FeeObject>();
           }
       }


       public List<FeeObject> GetNotificationFees(out CalculationFactor calculationFactor)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   const int appStage = (int)AppStage.Notification;
                   var fees = db.Fees.Where(k => k.ImportStageId == appStage).Include("FeeType").Include("ImportStage").ToList().ToList();
                   if (!fees.Any())
                   {
                       calculationFactor = new CalculationFactor();
                       return new List<FeeObject>();
                   }
                   var appSettings = db.ImportSettings.ToList();
                   if (!appSettings.Any())
                   {
                       calculationFactor = new CalculationFactor();
                       return new List<FeeObject>();
                   }

                   var feeSum = 0.0;
                   const int expend = (int) FeeTypeEnum.Expeditionary;
                   var expenditionaryFee = 0.0;
                   fees.ForEach(m =>
                   {
                       if (m.FeeTypeId != expend)
                       {
                           feeSum += m.Amount;
                       }
                       else
                       {
                           expenditionaryFee = m.Amount;
                       }
                   });

                   var appSettingObject = ModelMapper.Map<ImportSetting, ImportSettingObject>(appSettings[0]);
                   if (appSettingObject != null && appSettingObject.Id < 1)
                   {
                       calculationFactor = new CalculationFactor();
                       return new List<FeeObject>();
                   }

                   calculationFactor = new CalculationFactor
                   {
                       Fees = feeSum,
                       ExpenditionaryFee = expenditionaryFee,
                       ImportSettingObject = appSettingObject
                   };
                   
                   var objList = new List<FeeObject>();
                   fees.ForEach(app =>
                   {
                       var feeObject = ModelMapper.Map<Fee, FeeObject>(app);
                       if (feeObject != null && feeObject.FeeId > 0)
                       {
                           feeObject.FeeTypeName = app.FeeType.Name;
                           feeObject.ImportStageName = app.ImportStage.Name;
                           objList.Add(feeObject);
                       }
                   });

                   return !objList.Any() ? new List<FeeObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               calculationFactor = new CalculationFactor();
               return new List<FeeObject>();
           }
       }
       public List<FeeObject> GetFees(int? itemsPerPage, int? pageNumber, out int countG)
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
                           db.Fees.OrderByDescending(m => m.FeeId)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .Include("ImportStage")
                               .Include("FeeType")
                               .ToList();
                       if (fees.Any())
                       {
                           var newList = new List<FeeObject>();
                           fees.ForEach(app =>
                           {
                               var feeObject = ModelMapper.Map<Fee, FeeObject>(app);
                               if (feeObject != null && feeObject.FeeId > 0)
                               {
                                   feeObject.ImportStageName = app.ImportStage.Name;
                                   feeObject.FeeTypeName = app.FeeType.Name;
                                   newList.Add(feeObject);
                               }
                           });
                           countG = db.Fees.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<FeeObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<FeeObject>();
           }
       }
       public FeeObject GetFee(long feeId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var fees =
                        db.Fees.Where(m => m.FeeId == feeId)
                        .Include("ImportStage")
                        .Include("FeeType")
                        .ToList();
                    if (!fees.Any())
                    {
                        return new FeeObject();
                    }

                    var app = fees[0];
                    var feeObject = ModelMapper.Map<Fee, FeeObject>(app);
                    if (feeObject == null || feeObject.FeeId < 1)
                    {
                        return new FeeObject();
                    }

                    feeObject.ImportStageName = app.ImportStage.Name;
                    feeObject.FeeTypeName = app.FeeType.Name;
                  return feeObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new FeeObject();
           }
       }

       public List<FeeObject> Search(string searchCriteria)
       {
           try
           {
                double amount;
               var res = double.TryParse(searchCriteria, out amount);
               using (var db = new ImportPermitEntities())
               {
                   var fees = (from ff in db.Fees.Where(m => res && amount > 0 && m.Amount == amount)
                              join apst in db.ImportStages.Where(n => n.Name.ToLower().Trim() == searchCriteria.ToLower().Trim()) on ff.ImportStageId equals  apst.Id
                              join   c in db.FeeTypes.Where(n => n.Name.ToLower().Trim() == searchCriteria.ToLower().Trim()) on ff.FeeTypeId equals c.FeeTypeId
                               select new FeeObject
                              {
                                  FeeId = ff.FeeId,
                                  FeeTypeId = c.FeeTypeId,
                                  ImportStageId = apst.Id,
                                  Amount = ff.Amount,
                                  FeeTypeName = c.Name,
                                  ImportStageName = apst.Name
                              }).ToList();

                   if (!fees.Any())
                   {
                       return new List<FeeObject>();
                   }
                   return fees;
               }
              
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<FeeObject>();
           }
       }

       public long DeleteFee(long feeId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.Fees.Where(m => m.FeeId == feeId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.Fees.Remove(item);
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

       public long LogPaymentDetails(List<PaymentDistributionSummaryObject> fees)
       {
           try
           {
               if (!fees.Any())
               {
                   return -2;
               }
               var count = 0;
               using (var db = new PPIPSPaymentEntities())
               {
                   foreach (var fee in fees)
                   {
                       var feeEntity = ModelMapper.Map<PaymentDistributionSummaryObject, PaymentDistributionSummary>(fee);
                       if (feeEntity == null || feeEntity.Amount < 1)
                       {
                           return -2;
                       }
                       db.PaymentDistributionSummaries.Add(feeEntity);
                       db.SaveChanges();
                       count++;
                   }
               }  

               if (count != fees.Count)
               {
                   return -2;
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
       
    }
}
