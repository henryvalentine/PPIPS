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
    public class RecertificationProcessManager
    {

        public long AddRecertificationProcess(RecertificationProcessObject recertificationProcess)
        {
            try
            {
                if (recertificationProcess == null)
                {
                    return -2;
                }

                var recertificationProcessEntity = ModelMapper.Map<RecertificationProcessObject, RecertificationProcess>(recertificationProcess);
                if (recertificationProcessEntity == null || recertificationProcessEntity.EmployeeId < 1)
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.RecertificationProcesses.Add(recertificationProcessEntity);
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

        public long UpdateRecertificationProcess(RecertificationProcessObject recertificationProcess)
        {
            try
            {
                if (recertificationProcess == null)
                {
                    return -2;
                }

                var recertificationProcessEntity = ModelMapper.Map<RecertificationProcessObject, RecertificationProcess>(recertificationProcess);
                if (recertificationProcessEntity == null || recertificationProcessEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.RecertificationProcesses.Attach(recertificationProcessEntity);
                    db.Entry(recertificationProcessEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return recertificationProcess.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<RecertificationProcessObject> GetRecertificationProcesss()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var recertificationProcesss = db.RecertificationProcesses.ToList();
                    if (!recertificationProcesss.Any())
                    {
                        return new List<RecertificationProcessObject>();
                    }
                    var objList = new List<RecertificationProcessObject>();
                    recertificationProcesss.ForEach(app =>
                    {
                        var recertificationProcessObject = ModelMapper.Map<RecertificationProcess, RecertificationProcessObject>(app);
                        if (recertificationProcessObject != null && recertificationProcessObject.Id > 0)
                        {
                            objList.Add(recertificationProcessObject);
                        }
                    });

                    return !objList.Any() ? new List<RecertificationProcessObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<RecertificationProcessObject> GetRecertificationProcesss(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var recertificationProcesss = (from ptr in db.RecertificationProcesses.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)
.Include("Recertification")
                                                    join inv in db.Invoices on ptr.Recertification.Notification.InvoiceId equals inv.Id
                                                    select new RecertificationProcessObject
                                                    {
                                                        Id = ptr.Id,
                                                        RecertificationId = ptr.RecertificationId,
                                                        StepId = ptr.StepId,
                                                        EmployeeId = ptr.EmployeeId,
                                                        StatusId = ptr.StatusId,
                                                        AssignedTime = ptr.AssignedTime,
                                                        DueTime = ptr.DueTime,
                                                        ActualDeliveryDateTime = ptr.ActualDeliveryDateTime,
                                                        StepCode = ptr.StepCode,
                                                        OutComeCode = ptr.OutComeCode,
                                                        ReferenceCode = inv.ReferenceCode
                                                    }).ToList();

                        if (recertificationProcesss.Any())
                        {
                           countG = 0;
                           return new List<RecertificationProcessObject>();
                        }
                        countG = db.RecertificationProcesses.Count();
                        return recertificationProcesss;
                    }

                }
                countG = 0;
                return new List<RecertificationProcessObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<RecertificationProcessObject>();
            }
        }


        public RecertificationProcessObject GetRecertificationProcess(long recertificationProcessId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var recertificationProcesss =
                        db.RecertificationProcesses.Where(m => m.Id == recertificationProcessId)
                            .ToList();
                    if (!recertificationProcesss.Any())
                    {
                        return new RecertificationProcessObject();
                    }

                    var app = recertificationProcesss[0];
                    var recertificationProcessObject = ModelMapper.Map<RecertificationProcess, RecertificationProcessObject>(app);
                    if (recertificationProcessObject == null || recertificationProcessObject.Id < 1)
                    {
                        return new RecertificationProcessObject();
                    }

                    return recertificationProcessObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new RecertificationProcessObject();
            }
        }

        public List<RecertificationProcessObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var recertificationProcesss = (from ptr in db.RecertificationProcesses.Where(m => m.Recertification.Notification.Invoice.ReferenceCode.Contains(searchCriteria)).Include("Application")
                                            join inv in db.Invoices on ptr.Recertification.Notification.InvoiceId equals inv.Id
                                            select new RecertificationProcessObject
                                            {
                                                Id = ptr.Id,
                                                RecertificationId = ptr.RecertificationId,
                                                StepId = ptr.StepId,
                                                EmployeeId = ptr.EmployeeId,
                                                StatusId = ptr.StatusId,
                                                AssignedTime = ptr.AssignedTime,
                                                DueTime = ptr.DueTime,
                                                ActualDeliveryDateTime = ptr.ActualDeliveryDateTime,
                                                StepCode = ptr.StepCode,
                                                OutComeCode = ptr.OutComeCode,
                                                ReferenceCode = inv.ReferenceCode
                                            }).ToList();

                    if (recertificationProcesss.Any())
                    {
                        return new List<RecertificationProcessObject>();
                    }
                    
                    return recertificationProcesss;
                }
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RecertificationProcessObject>();
            }
        }

        public long DeleteRecertificationProcess(long recertificationProcessId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.RecertificationProcesses.Where(m => m.Id == recertificationProcessId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.RecertificationProcesses.Remove(item);
                    db.SaveChanges();
                    return 5;

                    //var recertificationProcess = db.RecertificationProcesses.Find(recertificationProcessId);
                    //db.RecertificationProcesses.Remove(recertificationProcess);
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
