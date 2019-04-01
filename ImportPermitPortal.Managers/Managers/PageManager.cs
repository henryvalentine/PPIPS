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
    public class PageManager
    {
        public long AddPage(PageObject page) 
        {
            try
            {
                if (page == null)
                {
                    return -2;
                }

                var pageEntity = ModelMapper.Map<PageObject, Page>(page);

                if (pageEntity == null || pageEntity.LastedUpdatedById < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.Pages.Add(pageEntity);
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

        public long UpdatePage(PageObject page)
        {
            try
            {
                if (page == null)
                {
                    return -2;
                }

                var pageEntity = ModelMapper.Map<PageObject, Page>(page);
                if (pageEntity == null || pageEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.Pages.Attach(pageEntity);
                    db.Entry(pageEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return page.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<PageObject> GetPages()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var countries = db.Pages.ToList();
                    if (!countries.Any())
                    {
                        return new List<PageObject>();
                    }
                    var objList = new List<PageObject>();
                    countries.ForEach(app =>
                    {
                        var pageObject = ModelMapper.Map<Page, PageObject>(app);
                        if (pageObject != null && pageObject.Id > 0)
                        {
                            objList.Add(pageObject);
                        }
                    });

                    return !objList.Any() ? new List<PageObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
        
        public List<PageObject> GetPages(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var pages = (from pg in db.Pages.OrderByDescending(g => g.Id).Skip(tpageNumber).Take(tsize) 
                        select new PageObject
                        {
                        Id = pg.Id,
                        Alias = pg.Alias,
                        Title = pg.Title,
                        PageContent = pg.PageContent,
                        IsActive = pg.IsActive,
                        LastUpdated = pg.LastUpdated,
                        LastedUpdatedById = pg.LastedUpdatedById,

                        }).ToList();

                        if (!pages.Any())
                        {
                            countG = 0;
                            return new List<PageObject>();
                        }

                        countG = db.Pages.Count();
                        return pages;
                    }

                }
                countG = 0;
                return new List<PageObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<PageObject>();
            }
        }

        public PageObject GetPage(long pageId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var pages = (from pg in db.Pages.Where(g => g.Id == pageId)
                                 select new PageObject
                                 {
                                     Id = pg.Id,
                                     Alias = pg.Alias,
                                     Title = pg.Title,
                                     PageContent = pg.PageContent,
                                     IsActive = pg.IsActive,
                                     LastUpdated = pg.LastUpdated,
                                     LastedUpdatedById = pg.LastedUpdatedById,

                                 }).ToList();

                    if (!pages.Any())
                    {
                        return new PageObject();
                    }

                    return pages[0];
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new PageObject();
            }
        }

        public List<PageObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var pages = (from pg in db.Pages.Where(g => g.Title.ToLower().Contains(searchCriteria.ToLower()))
                                 select new PageObject
                                 {
                                     Id = pg.Id,
                                     Alias = pg.Alias,
                                     Title = pg.Title,
                                     PageContent = pg.PageContent,
                                     IsActive = pg.IsActive,
                                     LastUpdated = pg.LastUpdated,
                                     LastedUpdatedById = pg.LastedUpdatedById,

                                 }).ToList();

                    if (!pages.Any())
                    {
                        return new List<PageObject>();
                    }
                    return pages;
                }
            }
            catch (Exception ex)
            {
                return new List<PageObject>();
            }
        }
        public long DeletePage(long pageId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.Pages.Where(m => m.Id == pageId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.Pages.Remove(item);
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
