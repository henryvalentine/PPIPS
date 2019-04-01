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
    public class ProcessTrackingManager
    {

        public long AddProcessTracking(ProcessTrackingObject processTracking)
        {
            try
            {
                if (processTracking == null)
                {
                    return -2;
                }

                var processTrackingEntity = ModelMapper.Map<ProcessTrackingObject, ProcessTracking>(processTracking);
                if (processTrackingEntity == null || processTrackingEntity.EmployeeId < 1)
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.ProcessTrackings.Add(processTrackingEntity);
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

        public long UpdateProcessTracking(ProcessTrackingObject processTracking)
        {
            try
            {
                if (processTracking == null)
                {
                    return -2;
                }

                var processTrackingEntity = ModelMapper.Map<ProcessTrackingObject, ProcessTracking>(processTracking);
                if (processTrackingEntity == null || processTrackingEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.ProcessTrackings.Attach(processTrackingEntity);
                    db.Entry(processTrackingEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return processTracking.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<ProcessTrackingObject> GetProcessTrackings()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var processTrackings = db.ProcessTrackings.ToList();
                    if (!processTrackings.Any())
                    {
                        return new List<ProcessTrackingObject>();
                    }
                    var objList = new List<ProcessTrackingObject>();
                    processTrackings.ForEach(app =>
                    {
                        var processTrackingObject = ModelMapper.Map<ProcessTracking, ProcessTrackingObject>(app);
                        if (processTrackingObject != null && processTrackingObject.Id > 0)
                        {
                            objList.Add(processTrackingObject);
                        }
                    });

                    return !objList.Any() ? new List<ProcessTrackingObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<ProcessTrackingObject> GetProcessTrackings(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var processTrackings = (from ptr in db.ProcessTrackings.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize).Include("Application")
                                                    join inv in db.Invoices on ptr.Application.InvoiceId equals inv.Id
                                                    select new ProcessTrackingObject
                                                    {
                                                        Id = ptr.Id,
                                                        ApplicationId = ptr.ApplicationId,
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

                        if (processTrackings.Any())
                        {
                           countG = 0;
                           return new List<ProcessTrackingObject>();
                        }
                        countG = db.ProcessTrackings.Count();
                        return processTrackings;
                    }

                }
                countG = 0;
                return new List<ProcessTrackingObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<ProcessTrackingObject>();
            }
        }


        public ProcessTrackingObject GetProcessTracking(long processTrackingId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var processTrackings =
                        db.ProcessTrackings.Where(m => m.Id == processTrackingId)
                            .ToList();
                    if (!processTrackings.Any())
                    {
                        return new ProcessTrackingObject();
                    }

                    var app = processTrackings[0];
                    var processTrackingObject = ModelMapper.Map<ProcessTracking, ProcessTrackingObject>(app);
                    if (processTrackingObject == null || processTrackingObject.Id < 1)
                    {
                        return new ProcessTrackingObject();
                    }

                    return processTrackingObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ProcessTrackingObject();
            }
        }

        public List<ProcessTrackingObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var processTrackings = (from ptr in db.ProcessTrackings.Where(m => m.Application.Invoice.ReferenceCode.Contains(searchCriteria)).Include("Application")
                                            join inv in db.Invoices on ptr.Application.InvoiceId equals inv.Id
                                            select new ProcessTrackingObject
                                            {
                                                Id = ptr.Id,
                                                ApplicationId = ptr.ApplicationId,
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

                    if (processTrackings.Any())
                    {
                        return new List<ProcessTrackingObject>();
                    }
                    
                    return processTrackings;
                }
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProcessTrackingObject>();
            }
        }

        public long DeleteProcessTracking(long processTrackingId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.ProcessTrackings.Where(m => m.Id == processTrackingId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.ProcessTrackings.Remove(item);
                    db.SaveChanges();
                    return 5;

                    //var processTracking = db.ProcessTrackinges.Find(processTrackingId);
                    //db.ProcessTrackinges.Remove(processTracking);
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
