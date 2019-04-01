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
    public class UnitOfMeasurementManager
    {
        
        public long AddUnitOfMeasurement(UnitOfMeasurementObject unitOfMeasurement)
        {
            try
            {
                if (unitOfMeasurement == null)
                {
                    return -2;
                }

                var unitOfMeasurementEntity = ModelMapper.Map<UnitOfMeasurementObject, UnitOfMeasurement>(unitOfMeasurement);
                if (unitOfMeasurementEntity == null || string.IsNullOrEmpty(unitOfMeasurementEntity.Name))
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.UnitOfMeasurements.Add(unitOfMeasurementEntity);
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

        public long UpdateUnitOfMeasurement(UnitOfMeasurementObject unitOfMeasurement)
        {
            try
            {
                if (unitOfMeasurement == null)
                {
                    return -2;
                }

                var unitOfMeasurementEntity = ModelMapper.Map<UnitOfMeasurementObject, UnitOfMeasurement>(unitOfMeasurement);
                if (unitOfMeasurementEntity == null || unitOfMeasurementEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.UnitOfMeasurements.Attach(unitOfMeasurementEntity);
                    db.Entry(unitOfMeasurementEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return unitOfMeasurement.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<UnitOfMeasurementObject> GetUnitOfMeasurements()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var unitOfMeasurements = db.UnitOfMeasurements.ToList();
                    if (!unitOfMeasurements.Any())
                    {
                        return new List<UnitOfMeasurementObject>();
                    }
                    var objList = new List<UnitOfMeasurementObject>();
                    unitOfMeasurements.ForEach(app =>
                    {
                        var unitOfMeasurementObject = ModelMapper.Map<UnitOfMeasurement, UnitOfMeasurementObject>(app);
                        if (unitOfMeasurementObject != null && unitOfMeasurementObject.Id > 0)
                        {
                            objList.Add(unitOfMeasurementObject);
                        }
                    });

                    return !objList.Any() ? new List<UnitOfMeasurementObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<UnitOfMeasurementObject> GetUnitOfMeasurements(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var unitOfMeasurements =
                            db.UnitOfMeasurements.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)

                                .ToList();
                        if (unitOfMeasurements.Any())
                        {
                            var newList = new List<UnitOfMeasurementObject>();
                            unitOfMeasurements.ForEach(app =>
                            {
                                var unitOfMeasurementObject = ModelMapper.Map<UnitOfMeasurement, UnitOfMeasurementObject>(app);
                                if (unitOfMeasurementObject != null && unitOfMeasurementObject.Id > 0)
                                {
                                    newList.Add(unitOfMeasurementObject);
                                }
                            });
                            countG = db.UnitOfMeasurements.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<UnitOfMeasurementObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<UnitOfMeasurementObject>();
            }
        }


        public UnitOfMeasurementObject GetUnitOfMeasurement(long unitOfMeasurementId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var unitOfMeasurements =
                        db.UnitOfMeasurements.Where(m => m.Id == unitOfMeasurementId)
                            .ToList();
                    if (!unitOfMeasurements.Any())
                    {
                        return new UnitOfMeasurementObject();
                    }

                    var app = unitOfMeasurements[0];
                    var unitOfMeasurementObject = ModelMapper.Map<UnitOfMeasurement, UnitOfMeasurementObject>(app);
                    if (unitOfMeasurementObject == null || unitOfMeasurementObject.Id < 1)
                    {
                        return new UnitOfMeasurementObject();
                    }

                    return unitOfMeasurementObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new UnitOfMeasurementObject();
            }
        }

        public List<UnitOfMeasurementObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var unitOfMeasurements =
                        db.UnitOfMeasurements.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (unitOfMeasurements.Any())
                    {
                        var newList = new List<UnitOfMeasurementObject>();
                        unitOfMeasurements.ForEach(app =>
                        {
                            var unitOfMeasurementObject = ModelMapper.Map<UnitOfMeasurement, UnitOfMeasurementObject>(app);
                            if (unitOfMeasurementObject != null && unitOfMeasurementObject.Id > 0)
                            {
                                newList.Add(unitOfMeasurementObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<UnitOfMeasurementObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<UnitOfMeasurementObject>();
            }
        }

        public long DeleteUnitOfMeasurement(long unitOfMeasurementId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.UnitOfMeasurements.Where(m => m.Id == unitOfMeasurementId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.UnitOfMeasurements.Remove(item);
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


        public List<UnitOfMeasurementObject> GerUnitOfMeasurements()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var unitOfMeasurements = db.UnitOfMeasurements.ToList();
                    if (!unitOfMeasurements.Any())
                    {
                        return new List<UnitOfMeasurementObject>();
                    }
                    var objList = new List<UnitOfMeasurementObject>();
                    unitOfMeasurements.ForEach(app =>
                    {
                        var unitOfMeasurementObject = ModelMapper.Map<UnitOfMeasurement, UnitOfMeasurementObject>(app);
                        if (unitOfMeasurementObject != null && unitOfMeasurementObject.Id > 0)
                        {
                            objList.Add(unitOfMeasurementObject);
                        }
                    });

                    return !objList.Any() ? new List<UnitOfMeasurementObject>() : objList;
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
