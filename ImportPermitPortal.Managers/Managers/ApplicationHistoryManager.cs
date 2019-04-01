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
    public class ImportApplicationHistoryManager
    {
        public long AddImportApplicationHistory(ImportApplicationHistoryObject importApplicationHistory)
        {
            try
            {
                if (importApplicationHistory == null)
                {
                    return -2;
                }

                var importApplicationHistoryEntity = ModelMapper.Map<ImportApplicationHistoryObject, ImportApplicationHistory>(importApplicationHistory);

                if (importApplicationHistoryEntity == null || importApplicationHistoryEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.ImportApplicationHistories.Add(importApplicationHistoryEntity);
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

        public long UpdateImportApplicationHistory(ImportApplicationHistoryObject importApplicationHistory)
        {
            try
            {
                if (importApplicationHistory == null)
                {
                    return -2;
                }

                var importApplicationHistoryEntity = ModelMapper.Map<ImportApplicationHistoryObject, ImportApplicationHistory>(importApplicationHistory);
                if (importApplicationHistoryEntity == null || importApplicationHistoryEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.ImportApplicationHistories.Attach(importApplicationHistoryEntity);
                    db.Entry(importApplicationHistoryEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return importApplicationHistory.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<ImportApplicationHistoryObject> GetImportApplicationHistories()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var histories = db.ImportApplicationHistories.ToList();
                    if (!histories.Any())
                    {
                        return new List<ImportApplicationHistoryObject>();
                    }
                    var objList = new List<ImportApplicationHistoryObject>();
                    histories.ForEach(app =>
                    {
                        var importApplicationHistoryObject = ModelMapper.Map<ImportApplicationHistory, ImportApplicationHistoryObject>(app);
                        if (importApplicationHistoryObject != null && importApplicationHistoryObject.Id > 0)
                        {
                            objList.Add(importApplicationHistoryObject);
                        }
                    });

                    return !objList.Any() ? new List<ImportApplicationHistoryObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
        
        public List<ImportApplicationHistoryObject> GetImportApplicationHistories(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var histories =
                            db.ImportApplicationHistories
                            .OrderByDescending(m => m.Id)
                             .Include("Application")
                            .Skip(tpageNumber).Take(tsize)
                             .ToList();
                        if (histories.Any())
                        {
                            var newList = new List<ImportApplicationHistoryObject>();
                            histories.ForEach(app =>
                            {
                                var application = ModelMapper.Map<Application, ApplicationObject>(app.Application);
                                if (application != null && application.Id > 0)
                                {
                                    var importApplicationHistoryObject = ModelMapper.Map<ImportApplicationHistory, ImportApplicationHistoryObject>(app);
                                    if (importApplicationHistoryObject != null && importApplicationHistoryObject.Id > 0)
                                    {
                                        importApplicationHistoryObject.ApplicationObject = application;
                                        newList.Add(importApplicationHistoryObject);
                                    }
                                }
                                
                            });
                            countG = db.Jetties.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<ImportApplicationHistoryObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<ImportApplicationHistoryObject>();
            }
        }

        public ImportApplicationHistoryObject GetImportApplicationHistory(long importApplicationHistoryId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var histories =
                        db.ImportApplicationHistories.Where(m => m.Id == importApplicationHistoryId).Include("Application")
                            .ToList();
                    if (!histories.Any())
                    {
                        return new ImportApplicationHistoryObject();
                    }

                    var app = histories[0];
                    var application = ModelMapper.Map<Application, ApplicationObject>(app.Application);
                    if (application == null || application.Id < 1)
                    {
                        return new ImportApplicationHistoryObject();
                    }

                    var importApplicationHistoryObject = ModelMapper.Map<ImportApplicationHistory, ImportApplicationHistoryObject>(app);
                    if (importApplicationHistoryObject == null || importApplicationHistoryObject.Id <1)
                    {
                        return new ImportApplicationHistoryObject();
                    }

                    importApplicationHistoryObject.ApplicationObject = application;
                    return importApplicationHistoryObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ImportApplicationHistoryObject();
            }
        }

        public List<ImportApplicationHistoryObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var histories =
                        db.ImportApplicationHistories
                        .Include("Application")
                        
                        .Where(m => m.Event.ToLower() == searchCriteria.ToLower().Trim())
                        .ToList();
                    if (!histories.Any())
                    {
                        return new List<ImportApplicationHistoryObject>();
                    }
                    var newList = new List<ImportApplicationHistoryObject>();
                    histories.ForEach(app =>
                    {
                        var application = ModelMapper.Map<Application, ApplicationObject>(app.Application);
                        if (application != null && application.Id > 0)
                        {
                            var importApplicationHistoryObject = ModelMapper.Map<ImportApplicationHistory, ImportApplicationHistoryObject>(app);
                            if (importApplicationHistoryObject != null && importApplicationHistoryObject.Id > 0)
                            {
                                importApplicationHistoryObject.ApplicationObject = application;
                                newList.Add(importApplicationHistoryObject);
                            }
                        }
                    });
                    return newList;
                }
            }
            catch (Exception ex)
            {
                return new List<ImportApplicationHistoryObject>();
            }
        }
        public long DeleteImportApplicationHistory(long importApplicationHistoryId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.ImportApplicationHistories.Where(m => m.Id == importApplicationHistoryId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.ImportApplicationHistories.Remove(item);
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
