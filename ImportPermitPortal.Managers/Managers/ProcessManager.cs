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
    public class ProcessManager
    {

        public long AddProcess(ProcessObject process)
        {
            try
            {
                if (process == null)
                {
                    return -2;
                }

                var processEntity = ModelMapper.Map<ProcessObject, Process>(process);
                if (processEntity == null || string.IsNullOrEmpty(processEntity.Name))
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.Processes.Add(processEntity);
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

        public long UpdateProcess(ProcessObject process)
        {
            try
            {
                if (process == null)
                {
                    return -2;
                }

                var processEntity = ModelMapper.Map<ProcessObject, Process>(process);
                if (processEntity == null || processEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.Processes.Attach(processEntity);
                    db.Entry(processEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return process.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<ProcessObject> GetProcesss()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var processs = db.Processes.ToList();
                    if (!processs.Any())
                    {
                        return new List<ProcessObject>();
                    }
                    var objList = new List<ProcessObject>();
                    processs.ForEach(app =>
                    {
                        var processObject = ModelMapper.Map<Process, ProcessObject>(app);
                        if (processObject != null && processObject.Id > 0)
                        {
                            objList.Add(processObject);
                        }
                    });

                    return !objList.Any() ? new List<ProcessObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<ProcessObject> GetProcesss(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var processs =
                            db.Processes.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)
.Include("ImportStage")
                                .ToList();
                        if (processs.Any())
                        {
                            var newList = new List<ProcessObject>();
                            processs.ForEach(app =>
                            {
                                var processObject = ModelMapper.Map<Process, ProcessObject>(app);
                                if (processObject != null && processObject.Id > 0)
                                {
                                    processObject.ImportStageName = app.ImportStage.Name;
                                    newList.Add(processObject);
                                }
                            });
                            countG = db.Processes.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<ProcessObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<ProcessObject>();
            }
        }


        public ProcessObject GetProcess(long processId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var processs =
                        db.Processes.Where(m => m.Id == processId)
                            .ToList();
                    if (!processs.Any())
                    {
                        return new ProcessObject();
                    }

                    var app = processs[0];
                    var processObject = ModelMapper.Map<Process, ProcessObject>(app);
                    if (processObject == null || processObject.Id < 1)
                    {
                        return new ProcessObject();
                    }

                    return processObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ProcessObject();
            }
        }

        public List<ProcessObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var processs =
                        db.Processes.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (processs.Any())
                    {
                        var newList = new List<ProcessObject>();
                        processs.ForEach(app =>
                        {
                            var processObject = ModelMapper.Map<Process, ProcessObject>(app);
                            if (processObject != null && processObject.Id > 0)
                            {
                                newList.Add(processObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<ProcessObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProcessObject>();
            }
        }

        public long DeleteProcess(long processId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.Processes.Where(m => m.Id == processId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.Processes.Remove(item);
                    db.SaveChanges();
                    return 5;

                    //var process = db.Processes.Find(processId);
                    //db.Processes.Remove(process);
                    //db.SaveChanges();
                    //return 5;
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
