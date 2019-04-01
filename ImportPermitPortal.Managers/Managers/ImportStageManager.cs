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
    public class ImportStageManager
    {

        public long AddImportStage(ImportStageObject importStage)
        {
            try
            {
                if (importStage == null)
                {
                    return -2;
                }

                var importStageEntity = ModelMapper.Map<ImportStageObject, ImportStage>(importStage);
                if (importStageEntity == null || string.IsNullOrEmpty(importStageEntity.Name))
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.ImportStages.Add(importStageEntity);
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

        public long UpdateImportStage(ImportStageObject importStage)
        {
            try
            {
                if (importStage == null)
                {
                    return -2;
                }

                var importStageEntity = ModelMapper.Map<ImportStageObject, ImportStage>(importStage);
                if (importStageEntity == null || importStageEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.ImportStages.Attach(importStageEntity);
                    db.Entry(importStageEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return importStage.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<ImportStageObject> GetImportStages()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var importStages = db.ImportStages.ToList();
                    if (!importStages.Any())
                    {
                        return new List<ImportStageObject>();
                    }
                    var objList = new List<ImportStageObject>();
                    importStages.ForEach(app =>
                    {
                        var importStageObject = ModelMapper.Map<ImportStage, ImportStageObject>(app);
                        if (importStageObject != null && importStageObject.Id > 0)
                        {
                            objList.Add(importStageObject);
                        }
                    });

                    return !objList.Any() ? new List<ImportStageObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<ImportStageObject> GetStagesWithProcesses()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var applicationStages = db.ImportStages.Include("Processes").ToList();


                    if (!applicationStages.Any())
                    {
                        return new List<ImportStageObject>();
                    }

                    var objList = new List<ImportStageObject>();

                    applicationStages.ForEach(app =>
                    {
                        var applicationStageObject = ModelMapper.Map<ImportStage, ImportStageObject>(app);
                        if (applicationStageObject != null && applicationStageObject.Id > 0)
                        {
                            applicationStageObject.ProcessObjects = new List<ProcessObject>();

                            applicationStageObject.StepObjects = new List<StepObject>();

                            var appprocesses = app.Processes.ToList();
                            //


                            appprocesses.ForEach(o =>
                            {
                                var processObject = ModelMapper.Map<Process, ProcessObject>(o);
                                if (processObject != null && processObject.ImportStageId > 0)
                                {

                                    applicationStageObject.ProcessObjects.Add(processObject);
                                }
                            });


                            var stepObjects =
                                db.Steps.Where(
                                    s =>
                                        s.Process.ImportStageId.Equals(
                                            applicationStageObject.Id));
                            if (stepObjects.Any())
                            {
                                foreach (var item in stepObjects)
                                {
                                    var stepEntity = ModelMapper.Map<Step, StepObject>(item);
                                    applicationStageObject.StepObjects.Add(stepEntity);
                                }
                            }

                            objList.Add(applicationStageObject);
                        }
                    });



                    return !objList.Any() ? new List<ImportStageObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
        public List<ImportStageObject> GetImportStages(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var importStages =
                            db.ImportStages.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)

                                .ToList();
                        if (importStages.Any())
                        {
                            var newList = new List<ImportStageObject>();
                            importStages.ForEach(app =>
                            {
                                var importStageObject = ModelMapper.Map<ImportStage, ImportStageObject>(app);
                                if (importStageObject != null && importStageObject.Id > 0)
                                {
                                    newList.Add(importStageObject);
                                }
                            });
                            countG = db.ImportStages.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<ImportStageObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<ImportStageObject>();
            }
        }
        
        public ImportStageObject GetImportStage(long importStageId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var importStages =
                        db.ImportStages.Where(m => m.Id == importStageId)
                            .ToList();
                    if (!importStages.Any())
                    {
                        return new ImportStageObject();
                    }

                    var app = importStages[0];
                    var importStageObject = ModelMapper.Map<ImportStage, ImportStageObject>(app);
                    if (importStageObject == null || importStageObject.Id < 1)
                    {
                        return new ImportStageObject();
                    }

                    return importStageObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ImportStageObject();
            }
        }

        public List<ImportStageObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var importStages =
                        db.ImportStages.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (importStages.Any())
                    {
                        var newList = new List<ImportStageObject>();
                        importStages.ForEach(app =>
                        {
                            var importStageObject = ModelMapper.Map<ImportStage, ImportStageObject>(app);
                            if (importStageObject != null && importStageObject.Id > 0)
                            {
                                newList.Add(importStageObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<ImportStageObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ImportStageObject>();
            }
        }

        public long DeleteImportStage(long importStageId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.ImportStages.Where(m => m.Id == importStageId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.ImportStages.Remove(item);
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
