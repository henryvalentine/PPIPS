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
   public class FaqManager
    {
       public long AddFaq(FaqObject faq)
       {
           try
           {
               if (faq == null)
               {
                   return -2;
               }

               var faqEntity = ModelMapper.Map<FaqObject, Faq>(faq);
               if (faqEntity == null || faqEntity.CategoryId < 1)
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.Faqs.Add(faqEntity);
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

       public long UpdateFaq(FaqObject faq)
       {
           try
           {
               if (faq == null)
               {
                   return -2;
               }

               var faqEntity = ModelMapper.Map<FaqObject, Faq>(faq);
               if (faqEntity == null || faqEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.Faqs.Attach(faqEntity);
                   db.Entry(faqEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return faq.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       
        public List<FaqObject> GetFaqs(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var faqs =
                           db.Faqs.OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (faqs.Any())
                       {
                           var newList = new List<FaqObject>();
                           faqs.ForEach(app =>
                           {
                               var faqObject = ModelMapper.Map<Faq, FaqObject>(app);
                               if (faqObject != null && faqObject.Id > 0)
                               {
                                   newList.Add(faqObject);
                               }
                           });
                           countG = db.Faqs.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<FaqObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<FaqObject>();
           }
       }

        public List<FaqObject> GetFaqs()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var faqs = db.Faqs.ToList();
                    if (faqs.Any())
                    {
                        var newList = new List<FaqObject>();
                        faqs.ForEach(app =>
                        {
                            var faqObject = ModelMapper.Map<Faq, FaqObject>(app);
                            if (faqObject != null && faqObject.Id > 0)
                            {
                                newList.Add(faqObject);
                            }
                        });
                       
                        return newList;
                    }
                    return new List<FaqObject>();
                }
            }
            catch (Exception ex)
            {
                return new List<FaqObject>();
            }
        }
       public FaqObject GetFaq(long faqId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var faqs =
                        db.Faqs.Where(m => m.Id == faqId)
                            .ToList();
                    if (!faqs.Any())
                    {
                        return new FaqObject();
                    }

                    var app = faqs[0];
                    var faqObject = ModelMapper.Map<Faq, FaqObject>(app);
                    if (faqObject == null || faqObject.Id < 1)
                    {
                        return new FaqObject();
                    }
                    
                  return faqObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new FaqObject();
           }
       }
       public List<FaqObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var faqs =
                       db.Faqs.Where(m => m.Answer.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (faqs.Any())
                   {
                       var newList = new List<FaqObject>();
                       faqs.ForEach(app =>
                       {
                           var faqObject = ModelMapper.Map<Faq, FaqObject>(app);
                           if (faqObject != null && faqObject.Id > 0)
                           {
                               newList.Add(faqObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<FaqObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<FaqObject>();
           }
       }

       public long DeleteFaq(long faqId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.Faqs.Where(m => m.Id == faqId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.Faqs.Remove(item);
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
