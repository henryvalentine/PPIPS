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
    public class JettyMappingManager
    {
        public long AddJettyMapping(JettyMappingObject jettyMapping)
        {
            try
            {
                if (jettyMapping == null)
                {
                    return -2;
                }
    
                using (var db = new ImportPermitEntities())
                {

                    var jettyMappingEntity = new JettyMapping
                    {
                        JettyId = jettyMapping.JettyId,
                        ZoneId = jettyMapping.ZoneId
                    };

                    var returnStatus = db.JettyMappings.Add(jettyMappingEntity);
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

        public long UpdateJettyMapping(JettyMappingObject jettyMapping)
        {
            try
            {
                if (jettyMapping == null)
                {
                    return -2;
                }

                var jettyMappingEntity = ModelMapper.Map<JettyMappingObject, JettyMapping>(jettyMapping);
                if (jettyMappingEntity == null || jettyMappingEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.JettyMappings.Attach(jettyMappingEntity);
                    db.Entry(jettyMappingEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return jettyMapping.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

     
        public List<JettyMappingObject> GetJettyMappings()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var jettyMappings = db.JettyMappings.Include("Jetty").Include("Zone").ToList();
                    if (!jettyMappings.Any())
                    {
                        return new List<JettyMappingObject>();
                    }
                    var objList = new List<JettyMappingObject>();
                    jettyMappings.ForEach(app =>
                    {
                        var jettyMappingObject = ModelMapper.Map<JettyMapping, JettyMappingObject>(app);
                        if (jettyMappingObject != null && jettyMappingObject.Id > 0)
                        {
                            jettyMappingObject.JettyName = app.Jetty.Name;
                            jettyMappingObject.ZoneName = app.Zone.Name;
                            objList.Add(jettyMappingObject);
                        }
                    });

                    return !objList.Any() ? new List<JettyMappingObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<JettyMappingObject> GetJettyMappings(long zoneId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var jettyMappings = db.JettyMappings.Where(m => m.ZoneId == zoneId)
                        .Include("Jetty").Include("Zone").ToList();
                    if (!jettyMappings.Any())
                    {
                        return new List<JettyMappingObject>();
                    }
                    var objList = new List<JettyMappingObject>();
                    jettyMappings.ForEach(app =>
                    {
                        var jettyMappingObject = ModelMapper.Map<JettyMapping, JettyMappingObject>(app);
                        if (jettyMappingObject != null && jettyMappingObject.Id > 0)
                        {
                            jettyMappingObject.JettyName = app.Jetty.Name;
                            jettyMappingObject.ZoneName = app.Zone.Name;
                            objList.Add(jettyMappingObject);
                        }
                    });

                    return !objList.Any() ? new List<JettyMappingObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
        public List<JettyMappingObject> GetJettyMappings(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var jettyMappings =
                            db.JettyMappings
                            .OrderByDescending(m => m.Id)
                             .Include("Jetty").Include("Zone")
                             .Skip(tpageNumber).Take(tsize)
                             .ToList();
                        if (jettyMappings.Any())
                        {
                            var newList = new List<JettyMappingObject>();
                            jettyMappings.ForEach(app =>
                            {
                                var jettyMappingObject = ModelMapper.Map<JettyMapping, JettyMappingObject>(app);
                                if (jettyMappingObject != null && jettyMappingObject.Id > 0)
                                {
                                    jettyMappingObject.JettyName = app.Jetty.Name;
                                    jettyMappingObject.ZoneName = app.Zone.Name;
                                    newList.Add(jettyMappingObject);
                                }
                            });
                            countG = db.Jetties.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<JettyMappingObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<JettyMappingObject>();
            }
        }

        public JettyMappingObject GetJettyMapping(long jettyMappingId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var jettyMappings =
                        db.JettyMappings.Where(m => m.Id == jettyMappingId)
                        .Include("Jetty").Include("Zone")
                            .ToList();
                    if (!jettyMappings.Any())
                    {
                        return new JettyMappingObject();
                    }

                    var app = jettyMappings[0];
                    var jettyMappingObject = ModelMapper.Map<JettyMapping, JettyMappingObject>(app);
                    if (jettyMappingObject == null || jettyMappingObject.Id < 1)
                    {
                        return new JettyMappingObject();
                    }

                    jettyMappingObject.JettyName = app.Jetty.Name;
                    jettyMappingObject.ZoneName = app.Zone.Name;
                    return jettyMappingObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new JettyMappingObject();
            }
        }

        public List<JettyMappingObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var jettyMappings =
                        db.JettyMappings
                       .Include("Jetty").Include("Zone")
                        
                        .Where(m => m.Zone.Name.ToLower() == searchCriteria.ToLower().Trim()
                        || m.Zone.Name.ToLower() == searchCriteria.ToLower().Trim())
                        .ToList();
                    if (!jettyMappings.Any())
                    {
                        return new List<JettyMappingObject>();
                    }
                    var newList = new List<JettyMappingObject>();
                    jettyMappings.ForEach(app =>
                    {
                        var jettyMappingObject = ModelMapper.Map<JettyMapping, JettyMappingObject>(app);
                        if (jettyMappingObject != null && jettyMappingObject.ZoneId > 0)
                        {
                            jettyMappingObject.JettyName = app.Jetty.Name;
                            jettyMappingObject.ZoneName = app.Zone.Name;
                            newList.Add(jettyMappingObject);
                        }
                    });
                    return newList;
                }
            }
            catch (Exception ex)
            {
                return new List<JettyMappingObject>();
            }
        }
        public long DeleteJettyMapping(long jettyMappingId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.JettyMappings.Where(m => m.Id == jettyMappingId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.JettyMappings.Remove(item);
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
