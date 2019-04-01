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
    public class DepotOwnerManager
    {
        
        public long AddDepotOwner(DepotOwnerObject depotOwner)
        {
            try
            {
                if (depotOwner == null)
                {
                    return -2;
                }

                var depotOwnerEntity = ModelMapper.Map<DepotOwnerObject, DepotOwner>(depotOwner);
                if (depotOwnerEntity == null || string.IsNullOrEmpty(depotOwnerEntity.Name))
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.DepotOwners.Add(depotOwnerEntity);
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

        public long UpdateDepotOwner(DepotOwnerObject depotOwner)
        {
            try
            {
                if (depotOwner == null)
                {
                    return -2;
                }

                var depotOwnerEntity = ModelMapper.Map<DepotOwnerObject, DepotOwner>(depotOwner);
                if (depotOwnerEntity == null || depotOwnerEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.DepotOwners.Attach(depotOwnerEntity);
                    db.Entry(depotOwnerEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return depotOwner.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<DepotOwnerObject> GetDepotOwners()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var depotOwners = db.DepotOwners.ToList();
                    if (!depotOwners.Any())
                    {
                        return new List<DepotOwnerObject>();
                    }
                    var objList = new List<DepotOwnerObject>();
                    depotOwners.ForEach(app =>
                    {
                        var depotOwnerObject = ModelMapper.Map<DepotOwner, DepotOwnerObject>(app);
                        if (depotOwnerObject != null && depotOwnerObject.Id > 0)
                        {
                            objList.Add(depotOwnerObject);
                        }
                    });

                    return !objList.Any() ? new List<DepotOwnerObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<DepotOwnerObject> GetDepotOwners(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var depotOwners =
                            db.DepotOwners.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)

                                .ToList();
                        if (depotOwners.Any())
                        {
                            var newList = new List<DepotOwnerObject>();
                            depotOwners.ForEach(app =>
                            {
                                var depotOwnerObject = ModelMapper.Map<DepotOwner, DepotOwnerObject>(app);
                                if (depotOwnerObject != null && depotOwnerObject.Id > 0)
                                {
                                    newList.Add(depotOwnerObject);
                                }
                            });
                            countG = db.DepotOwners.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<DepotOwnerObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<DepotOwnerObject>();
            }
        }


        public DepotOwnerObject GetDepotOwner(long depotOwnerId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var depotOwners =
                        db.DepotOwners.Where(m => m.Id == depotOwnerId)
                            .ToList();
                    if (!depotOwners.Any())
                    {
                        return new DepotOwnerObject();
                    }

                    var app = depotOwners[0];
                    var depotOwnerObject = ModelMapper.Map<DepotOwner, DepotOwnerObject>(app);
                    if (depotOwnerObject == null || depotOwnerObject.Id < 1)
                    {
                        return new DepotOwnerObject();
                    }

                    return depotOwnerObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new DepotOwnerObject();
            }
        }

        public List<DepotOwnerObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var depotOwners =
                        db.DepotOwners.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (depotOwners.Any())
                    {
                        var newList = new List<DepotOwnerObject>();
                        depotOwners.ForEach(app =>
                        {
                            var depotOwnerObject = ModelMapper.Map<DepotOwner, DepotOwnerObject>(app);
                            if (depotOwnerObject != null && depotOwnerObject.Id > 0)
                            {
                                newList.Add(depotOwnerObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<DepotOwnerObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DepotOwnerObject>();
            }
        }

        public long DeleteDepotOwner(long depotOwnerId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.DepotOwners.Where(m => m.Id == depotOwnerId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.DepotOwners.Remove(item);
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


        public List<DepotOwnerObject> GerDepotOwners()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var depotOwners = db.DepotOwners.ToList();
                    if (!depotOwners.Any())
                    {
                        return new List<DepotOwnerObject>();
                    }
                    var objList = new List<DepotOwnerObject>();
                    depotOwners.ForEach(app =>
                    {
                        var depotOwnerObject = ModelMapper.Map<DepotOwner, DepotOwnerObject>(app);
                        if (depotOwnerObject != null && depotOwnerObject.Id > 0)
                        {
                            objList.Add(depotOwnerObject);
                        }
                    });

                    return !objList.Any() ? new List<DepotOwnerObject>() : objList;
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
