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
    public class ZoneManager
    {
        
        public long AddZone(ZoneObject zone)
        {
            try
            {
                if (zone == null)
                {
                    return -2;
                }

                var zoneEntity = ModelMapper.Map<ZoneObject, Zone>(zone);
                if (zoneEntity == null || string.IsNullOrEmpty(zoneEntity.Name))
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.Zones.Add(zoneEntity);
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

        public long UpdateZone(ZoneObject zone)
        {
            try
            {
                if (zone == null)
                {
                    return -2;
                }

                var zoneEntity = ModelMapper.Map<ZoneObject, Zone>(zone);
                if (zoneEntity == null || zoneEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.Zones.Attach(zoneEntity);
                    db.Entry(zoneEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return zone.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<ZoneObject> GetZones()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var zones = db.Zones.ToList();
                    if (!zones.Any())
                    {
                        return new List<ZoneObject>();
                    }
                    var objList = new List<ZoneObject>();
                    zones.ForEach(app =>
                    {
                        var zoneObject = ModelMapper.Map<Zone, ZoneObject>(app);
                        if (zoneObject != null && zoneObject.Id > 0)
                        {
                            objList.Add(zoneObject);
                        }
                    });

                    return !objList.Any() ? new List<ZoneObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<ZoneObject> GetZones(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var zones =
                            db.Zones.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)

                                .ToList();
                        if (zones.Any())
                        {
                            var newList = new List<ZoneObject>();
                            zones.ForEach(app =>
                            {
                                var zoneObject = ModelMapper.Map<Zone, ZoneObject>(app);
                                if (zoneObject != null && zoneObject.Id > 0)
                                {
                                    newList.Add(zoneObject);
                                }
                            });
                            countG = db.Zones.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<ZoneObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<ZoneObject>();
            }
        }


        public ZoneObject GetZone(long zoneId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var zones =
                        db.Zones.Where(m => m.Id == zoneId)
                            .ToList();
                    if (!zones.Any())
                    {
                        return new ZoneObject();
                    }

                    var app = zones[0];
                    var zoneObject = ModelMapper.Map<Zone, ZoneObject>(app);
                    if (zoneObject == null || zoneObject.Id < 1)
                    {
                        return new ZoneObject();
                    }

                    return zoneObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ZoneObject();
            }
        }

        public List<ZoneObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var zones =
                        db.Zones.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (zones.Any())
                    {
                        var newList = new List<ZoneObject>();
                        zones.ForEach(app =>
                        {
                            var zoneObject = ModelMapper.Map<Zone, ZoneObject>(app);
                            if (zoneObject != null && zoneObject.Id > 0)
                            {
                                newList.Add(zoneObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<ZoneObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ZoneObject>();
            }
        }

        public long DeleteZone(long zoneId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.Zones.Where(m => m.Id == zoneId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.Zones.Remove(item);
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


        public List<ZoneObject> GerZones()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var zones = db.Zones.ToList();
                    if (!zones.Any())
                    {
                        return new List<ZoneObject>();
                    }
                    var objList = new List<ZoneObject>();
                    zones.ForEach(app =>
                    {
                        var zoneObject = ModelMapper.Map<Zone, ZoneObject>(app);
                        if (zoneObject != null && zoneObject.Id > 0)
                        {
                            objList.Add(zoneObject);
                        }
                    });

                    return !objList.Any() ? new List<ZoneObject>() : objList;
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
