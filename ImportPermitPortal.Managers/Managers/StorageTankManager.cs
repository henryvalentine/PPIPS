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
    public class StorageTankManager
    {
        
        public long AddStorageTank(StorageTankObject storageTank)
        {
            try
            {
                if (storageTank == null)
                {
                    return -2;
                }

                var storageTankEntity = new StorageTank
                {
                    DepotId = storageTank.DepotId,
                    ProductId = storageTank.ProductId,
                    TankNo = storageTank.TankNo,
                    Capacity = storageTank.Capacity,
                    UoMId = storageTank.UoMId
                };
              
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.StorageTanks.Add(storageTankEntity);
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

        public long UpdateStorageTank(StorageTankObject storageTank)
        {
            try
            {
                if (storageTank == null)
                {
                    return -2;
                }

                var storageTankEntity = ModelMapper.Map<StorageTankObject, StorageTank>(storageTank);
                if (storageTankEntity == null || storageTankEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.StorageTanks.Attach(storageTankEntity);
                    db.Entry(storageTankEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return storageTank.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<StorageTankObject> GetStorageTanks(long id)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var notification = db.Notifications.Where(n => n.Id == id).ToList();

                    if (notification.Any())
                    {
                       var depotId = notification[0].DischargeDepotId;

                       var storageTanks = db.StorageTanks.Where(t=>t.DepotId == depotId).ToList();
                       if (!storageTanks.Any())
                       {
                           return new List<StorageTankObject>();
                       }

                       var objList = new List<StorageTankObject>();
                       storageTanks.ForEach(app =>
                       {
                           var storageTankObject = ModelMapper.Map<StorageTank, StorageTankObject>(app);
                           if (storageTankObject != null && storageTankObject.Id > 0)
                           {
                               objList.Add(storageTankObject);
                           }
                       });

                       return !objList.Any() ? new List<StorageTankObject>() : objList;
                    }

                    return new List<StorageTankObject>();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<StorageTankObject> GetStorageTanks(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var storageTanks =
                            db.StorageTanks.OrderByDescending(m => m.Id).Include("Product").Include("Depot").Include("UnitOfMeasurement")
                                .Skip(tpageNumber).Take(tsize)

                                .ToList();
                        if (storageTanks.Any())
                        {
                            var newList = new List<StorageTankObject>();
                            storageTanks.ForEach(app =>
                            {
                                var storageTankObject = ModelMapper.Map<StorageTank, StorageTankObject>(app);
                                if (storageTankObject != null && storageTankObject.Id > 0)
                                {
                                    storageTankObject.ProductName = app.Product.Name;
                                    storageTankObject.DepotName = app.Depot.Name;
                                    storageTankObject.Measurement = app.UnitOfMeasurement.Name;
                                    newList.Add(storageTankObject);
                                }
                            });
                            countG = db.StorageTanks.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<StorageTankObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<StorageTankObject>();
            }
        }


        public StorageTankObject GetStorageTank(long storageTankId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var storageTanks =
                        db.StorageTanks.Where(m => m.Id == storageTankId)
                            .ToList();
                    if (!storageTanks.Any())
                    {
                        return new StorageTankObject();
                    }

                    var app = storageTanks[0];
                    var storageTankObject = ModelMapper.Map<StorageTank, StorageTankObject>(app);
                    if (storageTankObject == null || storageTankObject.Id < 1)
                    {
                        return new StorageTankObject();
                    }

                    return storageTankObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new StorageTankObject();
            }
        }

        public List<StorageTankObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var storageTanks =
                        db.StorageTanks.Where(m => m.TankNo.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (storageTanks.Any())
                    {
                        var newList = new List<StorageTankObject>();
                        storageTanks.ForEach(app =>
                        {
                            var storageTankObject = ModelMapper.Map<StorageTank, StorageTankObject>(app);
                            if (storageTankObject != null && storageTankObject.Id > 0)
                            {
                                newList.Add(storageTankObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<StorageTankObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StorageTankObject>();
            }
        }

        public long DeleteStorageTank(long storageTankId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.StorageTanks.Where(m => m.Id == storageTankId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.StorageTanks.Remove(item);
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


        public List<StorageTankObject> GerStorageTanks()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var storageTanks = db.StorageTanks.ToList();
                    if (!storageTanks.Any())
                    {
                        return new List<StorageTankObject>();
                    }
                    var objList = new List<StorageTankObject>();
                    storageTanks.ForEach(app =>
                    {
                        var storageTankObject = ModelMapper.Map<StorageTank, StorageTankObject>(app);
                        if (storageTankObject != null && storageTankObject.Id > 0)
                        {
                            objList.Add(storageTankObject);
                        }
                    });

                    return !objList.Any() ? new List<StorageTankObject>() : objList;
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
