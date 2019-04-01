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
    public class NotificationVesselManager
    {

        public long AddNotificationVessel(NotificationVesselObject vessel)
        {
            try
            {
                if (vessel == null)
                {
                    return -2;
                }

                var vesselEntity = ModelMapper.Map<NotificationVesselObject, NotificationVessel>(vessel);
                if (vesselEntity == null || vesselEntity.NotificationId < 1)
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.NotificationVessels.Add(vesselEntity);
                    db.SaveChanges();
                    return returnStatus.NotificationVesselId;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddNotificationVessels(List<NotificationVesselObject> notificationVessels)
        {
            try
            {
                if (!notificationVessels.Any())
                {
                    return -2;
                }

                var returnValue = 0;
                using (var db = new ImportPermitEntities())
                {
                    var mthVessel = notificationVessels.Find(t => t.VesselClassTypeId == (int)VesselClassEnum.Mother_Vessel);
                    if (mthVessel == null || string.IsNullOrEmpty(mthVessel.Name))
                    {
                        return -2;
                    }

                    var shtlVessel = notificationVessels.Find(t => t.VesselClassTypeId == (int)VesselClassEnum.Shuttle_Vessel);
                    if (shtlVessel == null || shtlVessel.VesselId < 1)
                    {
                        return -2;
                    }

                    long mothrVesselId = 0;
                    var existingVessel = db.Vessels.Where(j => j.Name.ToLower().Trim().Replace(" ", "") == mthVessel.Name.ToLower().Trim().Replace(" ", "")).ToList();
                    if (existingVessel.Any())
                    {
                        mothrVesselId = existingVessel[0].VesselId;
                    }

                    var mthVesselEntity = ModelMapper.Map<NotificationVesselObject, NotificationVessel>(mthVessel);
                    if (mthVesselEntity == null || mthVesselEntity.NotificationId < 1)
                    {
                        return -2;
                    }

                    var shtlVesselEntity = ModelMapper.Map<NotificationVesselObject, NotificationVessel>(shtlVessel);
                    if (shtlVesselEntity == null || shtlVesselEntity.NotificationId < 1)
                    {
                        return -2;
                    }
                    
                    if (mothrVesselId < 1)
                    {
                        var mthvessel = new Vessel
                        {
                            Name = mthVessel.Name.Trim(),
                            DateAdded = DateTime.Today
                        };
                        var status = db.Vessels.Add(mthvessel);
                        db.SaveChanges();
                        mthVesselEntity.VesselId = status.VesselId;
                        mthVesselEntity.DateAdded = DateTime.Today;
                        db.NotificationVessels.Add(mthVesselEntity);
                        db.SaveChanges();
                        returnValue += 1;
                    }
                    else
                    {
                        mthVesselEntity.VesselId = mothrVesselId;
                        mthVesselEntity.DateAdded = DateTime.Today;
                        db.NotificationVessels.Add(mthVesselEntity);
                        db.SaveChanges();
                        returnValue += 1;
                    }

                    shtlVesselEntity.DateAdded = DateTime.Today;
                    db.NotificationVessels.Add(shtlVesselEntity);
                    db.SaveChanges();
                    returnValue += 1;

                    return returnValue == notificationVessels.Count ? returnValue : -2;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public long UpdateNotificationVessel(NotificationVesselObject vessel)
        {
            try
            {
                if (vessel == null)
                {
                    return -2;
                }

                var vesselEntity = ModelMapper.Map<NotificationVesselObject, NotificationVessel>(vessel);
                if (vesselEntity == null || vesselEntity.NotificationVesselId < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.NotificationVessels.Attach(vesselEntity);
                    db.Entry(vesselEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return vessel.NotificationVesselId;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateNotificationVessels(List<NotificationVesselObject> oldNotificationVessels, List<NotificationVesselObject> notificationVessels)
        {
            try
            {
                if (!notificationVessels.Any() || !oldNotificationVessels.Any())
                {
                    return -2;
                }

                var returnValue = 0;
                using (var db = new ImportPermitEntities())
                {

                    var mthVessel = notificationVessels.Find(t => t.VesselClassTypeId == (int)VesselClassEnum.Mother_Vessel);
                    if (mthVessel == null || string.IsNullOrEmpty(mthVessel.Name))
                    {
                        return -2;
                    }

                    var oldmthVessel = oldNotificationVessels.Find(t => t.VesselClassTypeId == (int)VesselClassEnum.Mother_Vessel);
                    if (oldmthVessel == null || oldmthVessel.VesselId < 1)
                    {
                        return -2;
                    }

                    oldmthVessel.Name = mthVessel.Name;

                    var shtlVessel = notificationVessels.Find(t => t.VesselClassTypeId == (int)VesselClassEnum.Shuttle_Vessel);
                    if (shtlVessel == null || shtlVessel.VesselId < 1)
                    {
                        return -2;
                    }

                    var oldShtlVessel = oldNotificationVessels.Find(t => t.VesselClassTypeId == (int) VesselClassEnum.Shuttle_Vessel);
                    if (oldShtlVessel == null || oldShtlVessel.VesselId < 1)
                    {
                        return -2;
                    }
                    oldShtlVessel.VesselId = shtlVessel.VesselId;

                    var mthVesselEntity = ModelMapper.Map<NotificationVesselObject, NotificationVessel>(oldmthVessel);
                    if (mthVesselEntity == null || mthVesselEntity.NotificationId < 1)
                    {
                        return -2;
                    }
                    
                    var shtlVesselEntity = ModelMapper.Map<NotificationVesselObject, NotificationVessel>(oldShtlVessel);
                    if (shtlVesselEntity == null || shtlVesselEntity.NotificationId < 1)
                    {
                        return -2;
                    }

                    db.NotificationVessels.Attach(mthVesselEntity);
                    db.Entry(mthVesselEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    returnValue += 1;
                   
                    db.NotificationVessels.Attach(shtlVesselEntity);
                    db.Entry(shtlVesselEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    returnValue += 1;

                    return returnValue == notificationVessels.Count ? returnValue : -2;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<NotificationVesselObject> GetNotificationVessels()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var vessels = (from nv in db.NotificationVessels
                        join v in db.Vessels on nv.VesselId equals v.VesselId
                        select new NotificationVesselObject
                        {
                            NotificationVesselId = nv.NotificationVesselId,
                            VesselId = v.VesselId,
                            NotificationId = nv.NotificationId,
                            VesselClassTypeId = nv.VesselClassTypeId,
                            Name = v.Name
                        }).ToList();

                    if (!vessels.Any())
                    {
                        return new List<NotificationVesselObject>();
                    }

                    vessels.ForEach(app =>
                    {
                        app.VesselClassName = Enum.GetName(typeof (VesselClassEnum), app.VesselClassTypeId);
                    });
                    return vessels;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationVesselObject>();
            }
        }

        public List<NotificationVesselObject> GetNotificationVesselsByNotification(long notificationId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var vessels = (from nv in db.NotificationVessels.Where(k => k.NotificationId == notificationId)
                                   join v in db.Vessels on nv.VesselId equals v.VesselId
                                   select new NotificationVesselObject
                                   {
                                       NotificationVesselId = nv.NotificationVesselId,
                                       VesselId = v.VesselId,
                                       NotificationId = nv.NotificationId,
                                       VesselClassTypeId = nv.VesselClassTypeId,
                                       Name = v.Name
                                   }).ToList();

                    if (!vessels.Any())
                    {
                        return new List<NotificationVesselObject>();
                    }

                    vessels.ForEach(app =>
                    {
                        if (app.VesselClassTypeId == (int)VesselClassEnum.Shuttle_Vessel)
                        {
                            const int refTypeId = (int) RefLicenseTypeEnum.Coastal_Vessel_License_Number;
                            var lics = db.ReferenceLicenses.Where(k => k.ReferenceLicenseTypeId == refTypeId).ToList();
                            if (lics.Any())
                            {
                                app.LicenseNumber = lics[0].LicenceCode;
                            }
                            
                        }
                        app.VesselClassName = Enum.GetName(typeof(VesselClassEnum), app.VesselClassTypeId);
                    });
                    return vessels;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationVesselObject>();
            }
        }

        public List<NotificationVesselObject> GetNotificationVessels(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                     var vessels = (from nv in db.NotificationVessels.OrderByDescending(m => m.NotificationVesselId)
                                .Skip(tpageNumber).Take(tsize)
 
                                join v in db.Vessels on nv.VesselId equals v.VesselId
                                   select new NotificationVesselObject
                                   {
                                        NotificationVesselId = nv.NotificationVesselId,
                                        VesselId = v.VesselId,
                                        NotificationId = nv.NotificationId,
                                        VesselClassTypeId = nv.VesselClassTypeId,
                                        Name = v.Name
                                   }).ToList();

                    if (!vessels.Any())
                    {
                        countG = 0;
                        return new List<NotificationVesselObject>();
                    }

                    vessels.ForEach(app =>
                    {
                        app.VesselClassName = Enum.GetName(typeof(VesselClassEnum), app.VesselClassTypeId);
                    });

                    countG = db.NotificationVessels.Count();
                    return vessels;
                         
                    }

                }
                countG = 0;
                return new List<NotificationVesselObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<NotificationVesselObject>();
            }
        }
        
        public NotificationVesselObject GetNotificationVessel(long notificationVesselId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var vessels = (from nv in db.NotificationVessels.Where(m => m.NotificationVesselId == notificationVesselId)
                                   join v in db.Vessels on nv.VesselId equals v.VesselId
                                   select new NotificationVesselObject
                                   {
                                       NotificationVesselId = nv.NotificationVesselId,
                                       VesselId = v.VesselId,
                                       NotificationId = nv.NotificationId,
                                       VesselClassTypeId = nv.VesselClassTypeId,
                                       Name = v.Name
                                   }).ToList();
                    if (!vessels.Any())
                    {
                        return new NotificationVesselObject();
                    }

                    var vesselObject = vessels[0];
                    vesselObject.VesselClassName = Enum.GetName(typeof(VesselClassEnum), vesselObject.VesselClassTypeId);
                    return vesselObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationVesselObject();
            }
        }

        public List<NotificationVesselObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var vessels = (from v in db.Vessels.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                                   join nv in db.NotificationVessels on v.VesselId equals nv.VesselId
                                   select new NotificationVesselObject
                                   {
                                        NotificationVesselId = nv.NotificationVesselId,
                                        VesselId = v.VesselId,
                                        NotificationId = nv.NotificationId,
                                        VesselClassTypeId = nv.VesselClassTypeId,
                                        Name = v.Name
                                   }).ToList();

                    if (!vessels.Any())
                    {
                        return new List<NotificationVesselObject>();
                    }

                    vessels.ForEach(app =>
                    {
                        app.VesselClassName = Enum.GetName(typeof(VesselClassEnum), app.VesselClassTypeId);
                    });
                    return vessels;
                }
               
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<NotificationVesselObject>();
            }
        }

        public long DeleteNotificationVessel(long vesselId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.NotificationVessels.Where(m => m.NotificationVesselId == vesselId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.NotificationVessels.Remove(item);
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
