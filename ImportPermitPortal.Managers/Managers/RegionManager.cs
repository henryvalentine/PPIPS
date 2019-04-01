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
    public class RegionManager
    {
        
        public long AddRegion(RegionObject region)
        {
            try
            {
                if (region == null)
                {
                    return -2;
                }

                var regionEntity = ModelMapper.Map<RegionObject, Region>(region);
                if (regionEntity == null || string.IsNullOrEmpty(regionEntity.Name))
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.Regions.Add(regionEntity);
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

        public long UpdateRegion(RegionObject region)
        {
            try
            {
                if (region == null)
                {
                    return -2;
                }

                var regionEntity = ModelMapper.Map<RegionObject, Region>(region);
                if (regionEntity == null || regionEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.Regions.Attach(regionEntity);
                    db.Entry(regionEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return region.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<RegionObject> GetRegions()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var regions = db.Regions.ToList();
                    if (!regions.Any())
                    {
                        return new List<RegionObject>();
                    }
                    var objList = new List<RegionObject>();
                    regions.ForEach(app =>
                    {
                        var regionObject = ModelMapper.Map<Region, RegionObject>(app);
                        if (regionObject != null && regionObject.Id > 0)
                        {
                            objList.Add(regionObject);
                        }
                    });

                    return !objList.Any() ? new List<RegionObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<RegionObject> GetRegions(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var regions =
                            db.Regions.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)

                                .ToList();
                        if (regions.Any())
                        {
                            var newList = new List<RegionObject>();
                            regions.ForEach(app =>
                            {
                                var regionObject = ModelMapper.Map<Region, RegionObject>(app);
                                if (regionObject != null && regionObject.Id > 0)
                                {
                                    newList.Add(regionObject);
                                }
                            });
                            countG = db.Regions.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<RegionObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<RegionObject>();
            }
        }


        public RegionObject GetRegion(long regionId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var regions =
                        db.Regions.Where(m => m.Id == regionId)
                            .ToList();
                    if (!regions.Any())
                    {
                        return new RegionObject();
                    }

                    var app = regions[0];
                    var regionObject = ModelMapper.Map<Region, RegionObject>(app);
                    if (regionObject == null || regionObject.Id < 1)
                    {
                        return new RegionObject();
                    }

                    return regionObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new RegionObject();
            }
        }

        public List<RegionObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var regions =
                        db.Regions.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (regions.Any())
                    {
                        var newList = new List<RegionObject>();
                        regions.ForEach(app =>
                        {
                            var regionObject = ModelMapper.Map<Region, RegionObject>(app);
                            if (regionObject != null && regionObject.Id > 0)
                            {
                                newList.Add(regionObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<RegionObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RegionObject>();
            }
        }

        public long DeleteRegion(long regionId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.Regions.Where(m => m.Id == regionId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.Regions.Remove(item);
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


        public List<RegionObject> GerRegions()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var regions = db.Regions.ToList();
                    if (!regions.Any())
                    {
                        return new List<RegionObject>();
                    }
                    var objList = new List<RegionObject>();
                    regions.ForEach(app =>
                    {
                        var regionObject = ModelMapper.Map<Region, RegionObject>(app);
                        if (regionObject != null && regionObject.Id > 0)
                        {
                            objList.Add(regionObject);
                        }
                    });

                    return !objList.Any() ? new List<RegionObject>() : objList;
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
