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
    public class FaqCategoryManager
    {
       public long AddFaqCategory(FaqCategoryObject faqCategory)
       {
           try
           {
               if (faqCategory == null)
               {
                   return -2;
               }

               var faqCategoryEntity = ModelMapper.Map<FaqCategoryObject, FaqCategory>(faqCategory);
               if (faqCategoryEntity == null || string.IsNullOrEmpty(faqCategoryEntity.Name))
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.FaqCategories.Add(faqCategoryEntity);
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

       public long UpdateFaqCategory(FaqCategoryObject faqCategory)
       {
           try
           {
               if (faqCategory == null)
               {
                   return -2;
               }

               var faqCategoryEntity = ModelMapper.Map<FaqCategoryObject, FaqCategory>(faqCategory);
               if (faqCategoryEntity == null || faqCategoryEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.FaqCategories.Attach(faqCategoryEntity);
                   db.Entry(faqCategoryEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return faqCategory.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public List<FaqCategoryObject> GetFaqCategories()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var faqCategorys = db.FaqCategories.ToList();
                   if (!faqCategorys.Any())
                   {
                        return new List<FaqCategoryObject>();
                   }
                   var objList =  new List<FaqCategoryObject>();
                   faqCategorys.ForEach(app =>
                   {
                       var faqCategoryObject = ModelMapper.Map<FaqCategory, FaqCategoryObject>(app);
                       if (faqCategoryObject != null && faqCategoryObject.Id > 0)
                       {
                           objList.Add(faqCategoryObject);
                       }
                   });

                   return !objList.Any() ? new List<FaqCategoryObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }
       
        public List<FaqCategoryObject> GetFaqCategories(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var faqCategorys =
                           db.FaqCategories.OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (faqCategorys.Any())
                       {
                           var newList = new List<FaqCategoryObject>();
                           faqCategorys.ForEach(app =>
                           {
                               var faqCategoryObject = ModelMapper.Map<FaqCategory, FaqCategoryObject>(app);
                               if (faqCategoryObject != null && faqCategoryObject.Id > 0)
                               {
                                   newList.Add(faqCategoryObject);
                               }
                           });
                           countG = db.FaqCategories.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<FaqCategoryObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<FaqCategoryObject>();
           }
       }
       
       public FaqCategoryObject GetFaqCategory(long faqCategoryId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var faqCategorys =
                        db.FaqCategories.Where(m => m.Id == faqCategoryId)
                            .ToList();
                    if (!faqCategorys.Any())
                    {
                        return new FaqCategoryObject();
                    }

                    var app = faqCategorys[0];
                    var faqCategoryObject = ModelMapper.Map<FaqCategory, FaqCategoryObject>(app);
                    if (faqCategoryObject == null || faqCategoryObject.Id < 1)
                    {
                        return new FaqCategoryObject();
                    }
                    
                  return faqCategoryObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new FaqCategoryObject();
           }
       }
       public List<FaqCategoryObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var faqCategorys =
                       db.FaqCategories.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (faqCategorys.Any())
                   {
                       var newList = new List<FaqCategoryObject>();
                       faqCategorys.ForEach(app =>
                       {
                           var faqCategoryObject = ModelMapper.Map<FaqCategory, FaqCategoryObject>(app);
                           if (faqCategoryObject != null && faqCategoryObject.Id > 0)
                           {
                               newList.Add(faqCategoryObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<FaqCategoryObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<FaqCategoryObject>();
           }
       }

       public long DeleteFaqCategory(long faqCategoryId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.FaqCategories.Where(m => m.Id == faqCategoryId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.FaqCategories.Remove(item);
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
