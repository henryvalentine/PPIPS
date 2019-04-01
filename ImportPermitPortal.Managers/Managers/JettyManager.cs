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
    public class JettyManager
    {
        public long AddJetty(JettyObject jetty)
        {
            try
            {
                if (jetty == null)
                {
                    return -2;
                }

              

                using (var db = new ImportPermitEntities())
                {
                    var jettyEntity = new Jetty
                    {
                        Name = jetty.Name,
                        PortId = jetty.PortId
                    };
                    var returnStatus = db.Jetties.Add(jettyEntity);
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

        public long UpdateJetty(JettyObject jetty)
        {
            try
            {
                if (jetty == null)
                {
                    return -2;
                }

                var jettyEntity = ModelMapper.Map<JettyObject, Jetty>(jetty);
                if (jettyEntity == null || jettyEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.Jetties.Attach(jettyEntity);
                    db.Entry(jettyEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return jetty.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<JettyObject> GetJetties()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var jettys = db.Jetties.ToList();
                    if (!jettys.Any())
                    {
                        return new List<JettyObject>();
                    }
                    var objList = new List<JettyObject>();
                    jettys.ForEach(app =>
                    {
                        var jettyObject = ModelMapper.Map<Jetty, JettyObject>(app);
                        if (jettyObject != null && jettyObject.Id > 0)
                        {
                            objList.Add(jettyObject);
                        }
                    });

                    return !objList.Any() ? new List<JettyObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<DepotObject> GetDepots()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    //const int nigeria = (int) CountryEnum.Nigeria;

                    var depotList = db.Depots.ToList();

                    if (!depotList.Any())
                    {
                        return new List<DepotObject>();
                    }

                    var objList = new List<DepotObject>();

                    depotList.ForEach(app =>
                    {
                        var depot = ModelMapper.Map<Depot, DepotObject>(app);
                        if (depot != null && depot.Id > 0)
                        {
                            objList.Add(depot);
                        }
                   });

                    return !objList.Any() ? new List<DepotObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DepotObject>();
            }
        }

        public List<JettyObject> GetJetties(long portId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var jettys = db.Jetties.Where(m => m.PortId == portId)
                        .Include("Port").ToList();
                    if (!jettys.Any())
                    {
                        return new List<JettyObject>();
                    }
                    var objList = new List<JettyObject>();
                    jettys.ForEach(app =>
                    {
                        var jettyObject = ModelMapper.Map<Jetty, JettyObject>(app);
                        if (jettyObject != null && jettyObject.Id > 0)
                        {
                            jettyObject.PortName = app.Port.Name;
                            objList.Add(jettyObject);
                        }
                    });

                    return !objList.Any() ? new List<JettyObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
        public List<JettyObject> GetJetties(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var jettys =
                            db.Jetties
                            .OrderByDescending(m => m.Id)
                             .Include("Port")
                             .Skip(tpageNumber).Take(tsize)
                             .ToList();
                        if (jettys.Any())
                        {
                            var newList = new List<JettyObject>();
                            jettys.ForEach(app =>
                            {
                                var jettyObject = ModelMapper.Map<Jetty, JettyObject>(app);
                                if (jettyObject != null && jettyObject.Id > 0)
                                {
                                    jettyObject.PortName = app.Port.Name;
                                    newList.Add(jettyObject);
                                }
                            });
                            countG = db.Jetties.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<JettyObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<JettyObject>();
            }
        }

        public JettyObject GetJetty(long jettyId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var jettys =
                        db.Jetties.Where(m => m.Id == jettyId)
                            .ToList();
                    if (!jettys.Any())
                    {
                        return new JettyObject();
                    }

                    var app = jettys[0];
                    var jettyObject = ModelMapper.Map<Jetty, JettyObject>(app);
                    if (jettyObject == null || jettyObject.Id < 1)
                    {
                        return new JettyObject();
                    }
                    jettyObject.PortName = app.Port.Name;
                    return jettyObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new JettyObject();
            }
        }
        public int GetJettyIdByName(string jettyName)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var jettys = db.Jetties.Where(m => m.Name.ToLower().Replace(" ", string.Empty) == jettyName.ToLower().Replace(" ", string.Empty)).ToList();
                    if (!jettys.Any())
                    {
                        return 0;
                    }

                    var app = jettys[0];
                    return app.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<JettyObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var jettys =
                        db.Jetties
                        .Include("Port")
                        
                        .Where(m => m.Name.ToLower() == searchCriteria.ToLower().Trim()
                        || m.Port.Name.ToLower() == searchCriteria.ToLower().Trim())
                        .ToList();
                    if (!jettys.Any())
                    {
                        return new List<JettyObject>();
                    }
                    var newList = new List<JettyObject>();
                    jettys.ForEach(app =>
                    {
                        var jettyObject = ModelMapper.Map<Jetty, JettyObject>(app);
                        if (jettyObject != null && jettyObject.PortId > 0)
                        {
                            jettyObject.PortName = app.Port.Name;
                            newList.Add(jettyObject);
                        }
                    });
                    return newList;
                }
            }
            catch (Exception ex)
            {
                return new List<JettyObject>();
            }
        }
        public long DeleteJetty(long jettyId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.Jetties.Where(m => m.Id == jettyId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.Jetties.Remove(item);
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
