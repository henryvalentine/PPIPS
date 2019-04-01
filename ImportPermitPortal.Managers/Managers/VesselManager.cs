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
    public class VesselManager
    {

        public long AddVessel(VesselObject vessel)
        {
            try
            {
                if (vessel == null)
                {
                    return -2;
                }

                var vesselEntity = ModelMapper.Map<VesselObject, Vessel>(vessel);
                if (vesselEntity == null || string.IsNullOrEmpty(vesselEntity.Name))
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.Vessels.Add(vesselEntity);
                    db.SaveChanges();
                    return returnStatus.VesselId;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddVessel(List<VesselObject> vessels)
        {
            try
            {
                if (vessels == null || !vessels.Any())
                {
                    return -2;
                }
                var count = 0;
                vessels.ForEach(vessel =>
                {
                    var vesselEntity = ModelMapper.Map<VesselObject, Vessel>(vessel);
                    if (vesselEntity != null && !string.IsNullOrEmpty(vesselEntity.Name))
                    {
                        using (var db = new ImportPermitEntities())
                        {
                            var returnStatus = db.Vessels.Add(vesselEntity);
                            db.SaveChanges();
                            count++;
                        } 
                    }
                   
                });

                return count;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        
        public long UpdateVessel(VesselObject vessel)
        {
            try
            {
                if (vessel == null)
                {
                    return -2;
                }

                var vesselEntity = ModelMapper.Map<VesselObject, Vessel>(vessel);
                if (vesselEntity == null || vesselEntity.VesselId < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.Vessels.Attach(vesselEntity);
                    db.Entry(vesselEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return vesselEntity.VesselId;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<VesselObject> GetVessels()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var vessels = db.Vessels.ToList();
                    if (!vessels.Any())
                    {
                        return new List<VesselObject>();
                    }
                    var objList = new List<VesselObject>();
                    vessels.ForEach(app =>
                    {
                        var vesselObject = ModelMapper.Map<Vessel, VesselObject>(app);
                        if (vesselObject != null && vesselObject.VesselId > 0)
                        {
                            vesselObject.CapacityStr = vesselObject.Capacity == null ? "0" : ((float)vesselObject.Capacity).ToString("n1").Replace(".0", "");
                            objList.Add(vesselObject);
                        }
                    });

                    return !objList.Any() ? new List<VesselObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<VesselObject> GetValidShuttleVessels()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var vessels = db.Vessels.Where(m =>m.VesselLicense != null && m.ExpiryDate != null && m.ExpiryDate.Value >= DateTime.Now && m.Status == true).ToList();
                    if (!vessels.Any())
                    {
                        return new List<VesselObject>();
                    }
                    var objList = new List<VesselObject>();
                    vessels.ForEach(app =>
                    {
                        var vesselObject = ModelMapper.Map<Vessel, VesselObject>(app);
                        if (vesselObject != null && vesselObject.VesselId > 0)
                        {
                            vesselObject.CapacityStr = vesselObject.Capacity == null ? "0" : ((float)vesselObject.Capacity).ToString("n1").Replace(".0", "");
                            objList.Add(vesselObject);
                        }
                    });

                    return !objList.Any() ? new List<VesselObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<VesselObject> GetVessels(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var vessels = db.Vessels.Where(m => m.VesselLicense != null).OrderByDescending(m => m.VesselId).Skip(tpageNumber).Take(tsize).ToList();
                        if (vessels.Any())
                        {
                            var newList = new List<VesselObject>();
                            vessels.ForEach(app =>
                            {
                                var vesselObject = ModelMapper.Map<Vessel, VesselObject>(app);
                                if (vesselObject != null && vesselObject.VesselId > 0)
                                {
                                    vesselObject.CapacityStr = vesselObject.Capacity == null ? "0" : ((float)vesselObject.Capacity).ToString("n1").Replace(".0", "");
                                    newList.Add(vesselObject);
                                }
                            });
                            countG = db.Vessels.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<VesselObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<VesselObject>();
            }
        }

        public VesselObject GetVessel(long vesselId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var vessels =
                        db.Vessels.Where(m => m.VesselId == vesselId)
                            .ToList();
                    if (!vessels.Any())
                    {
                        return new VesselObject();
                    }

                    var app = vessels[0];
                    var vesselObject = ModelMapper.Map<Vessel, VesselObject>(app);
                    if (vesselObject == null || vesselObject.VesselId < 1)
                    {
                        return new VesselObject();
                    }
                    vesselObject.IssueDateStr = vesselObject.IssueDate == null ? "" : ((DateTime)vesselObject.IssueDate).ToString("dd/MM/yyyy");
                    vesselObject.ExpiryDateStr = vesselObject.ExpiryDate == null ? "0" : ((DateTime)vesselObject.ExpiryDate).ToString("dd/MM/yyyy");
                    return vesselObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new VesselObject();
            }
        }

        public List<VesselObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var vessels =
                        db.Vessels.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (vessels.Any())
                    {
                        var newList = new List<VesselObject>();
                        vessels.ForEach(app =>
                        {
                            var vesselObject = ModelMapper.Map<Vessel, VesselObject>(app);
                            if (vesselObject != null && vesselObject.VesselId > 0)
                            {
                                vesselObject.CapacityStr = vesselObject.Capacity == null? "0" : ((float)vesselObject.Capacity).ToString("n1").Replace(".0", "");
                                newList.Add(vesselObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<VesselObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<VesselObject>();
            }
        }

        public long DeleteVessel(long vesselId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.Vessels.Where(m => m.VesselId == vesselId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.Vessels.Remove(item);
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
