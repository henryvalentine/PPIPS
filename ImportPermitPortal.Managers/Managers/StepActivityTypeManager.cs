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
    public class StepActivityTypeManager
    {
        
        public long AddStepActivityType(StepActivityTypeObject stepActivityType)
        {
            try
            {
                if (stepActivityType == null)
                {
                    return -2;
                }

                var stepActivityTypeEntity = ModelMapper.Map<StepActivityTypeObject, StepActivityType>(stepActivityType);
                if (stepActivityTypeEntity == null || string.IsNullOrEmpty(stepActivityTypeEntity.Name))
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.StepActivityTypes.Add(stepActivityTypeEntity);
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

        public long UpdateStepActivityType(StepActivityTypeObject stepActivityType)
        {
            try
            {
                if (stepActivityType == null)
                {
                    return -2;
                }

                var stepActivityTypeEntity = ModelMapper.Map<StepActivityTypeObject, StepActivityType>(stepActivityType);
                if (stepActivityTypeEntity == null || stepActivityTypeEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.StepActivityTypes.Attach(stepActivityTypeEntity);
                    db.Entry(stepActivityTypeEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return stepActivityType.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<StepActivityTypeObject> GetStepActivityTypes()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var stepActivityTypes = db.StepActivityTypes.ToList();
                    if (!stepActivityTypes.Any())
                    {
                        return new List<StepActivityTypeObject>();
                    }
                    var objList = new List<StepActivityTypeObject>();
                    stepActivityTypes.ForEach(app =>
                    {
                        var stepActivityTypeObject = ModelMapper.Map<StepActivityType, StepActivityTypeObject>(app);
                        if (stepActivityTypeObject != null && stepActivityTypeObject.Id > 0)
                        {
                            objList.Add(stepActivityTypeObject);
                        }
                    });

                    return !objList.Any() ? new List<StepActivityTypeObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<StepActivityTypeObject> GetStepActivityTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var stepActivityTypes =
                            db.StepActivityTypes.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)

                                .ToList();
                        if (stepActivityTypes.Any())
                        {
                            var newList = new List<StepActivityTypeObject>();
                            stepActivityTypes.ForEach(app =>
                            {
                                var stepActivityTypeObject = ModelMapper.Map<StepActivityType, StepActivityTypeObject>(app);
                                if (stepActivityTypeObject != null && stepActivityTypeObject.Id > 0)
                                {
                                    newList.Add(stepActivityTypeObject);
                                }
                            });
                            countG = db.StepActivityTypes.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<StepActivityTypeObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<StepActivityTypeObject>();
            }
        }


        public StepActivityTypeObject GetStepActivityType(long stepActivityTypeId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var stepActivityTypes =
                        db.StepActivityTypes.Where(m => m.Id == stepActivityTypeId)
                            .ToList();
                    if (!stepActivityTypes.Any())
                    {
                        return new StepActivityTypeObject();
                    }

                    var app = stepActivityTypes[0];
                    var stepActivityTypeObject = ModelMapper.Map<StepActivityType, StepActivityTypeObject>(app);
                    if (stepActivityTypeObject == null || stepActivityTypeObject.Id < 1)
                    {
                        return new StepActivityTypeObject();
                    }

                    return stepActivityTypeObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new StepActivityTypeObject();
            }
        }

        public List<StepActivityTypeObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var stepActivityTypes =
                        db.StepActivityTypes.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (stepActivityTypes.Any())
                    {
                        var newList = new List<StepActivityTypeObject>();
                        stepActivityTypes.ForEach(app =>
                        {
                            var stepActivityTypeObject = ModelMapper.Map<StepActivityType, StepActivityTypeObject>(app);
                            if (stepActivityTypeObject != null && stepActivityTypeObject.Id > 0)
                            {
                                newList.Add(stepActivityTypeObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<StepActivityTypeObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StepActivityTypeObject>();
            }
        }

        public long DeleteStepActivityType(long stepActivityTypeId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.StepActivityTypes.Where(m => m.Id == stepActivityTypeId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.StepActivityTypes.Remove(item);
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


        public List<StepActivityTypeObject> GerStepActivityTypes()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var stepActivityTypes = db.StepActivityTypes.ToList();
                    if (!stepActivityTypes.Any())
                    {
                        return new List<StepActivityTypeObject>();
                    }
                    var objList = new List<StepActivityTypeObject>();
                    stepActivityTypes.ForEach(app =>
                    {
                        var stepActivityTypeObject = ModelMapper.Map<StepActivityType, StepActivityTypeObject>(app);
                        if (stepActivityTypeObject != null && stepActivityTypeObject.Id > 0)
                        {
                            objList.Add(stepActivityTypeObject);
                        }
                    });

                    return !objList.Any() ? new List<StepActivityTypeObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
    }
}
