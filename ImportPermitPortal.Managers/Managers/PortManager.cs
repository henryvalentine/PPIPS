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
    public class PortManager
    {
        public long AddPort(PortObject port)
        {
            try
            {
                if (port == null)
                {
                    return -2;
                }

               
                using (var db = new ImportPermitEntities())
                {
                    var portEntity = new Port
                    {
                        Name = port.Name,
                        CountryId = port.CountryId
                    };
                    var returnStatus = db.Ports.Add(portEntity);
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

        public long UpdatePort(PortObject port)
        {
            try
            {
                if (port == null)
                {
                    return -2;
                }

                var portEntity = ModelMapper.Map<PortObject, Port>(port);
                if (portEntity == null || portEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.Ports.Attach(portEntity);
                    db.Entry(portEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return port.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<PortObject> GetPorts()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var ports = db.Ports.ToList();
                    if (!ports.Any())
                    {
                        return new List<PortObject>();
                    }
                    var objList = new List<PortObject>();
                    ports.ForEach(app =>
                    {
                        var portObject = ModelMapper.Map<Port, PortObject>(app);
                        if (portObject != null && portObject.Id > 0)
                        {
                            objList.Add(portObject);
                        }
                    });

                    return !objList.Any() ? new List<PortObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
        
        public List<PortObject> GetPorts(long countryId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var ports = db.Ports.Where(m => m.CountryId == countryId)
                        .Include("Country").ToList();
                    if (!ports.Any())
                    {
                        return new List<PortObject>();
                    }
                    var objList = new List<PortObject>();
                    ports.ForEach(app =>
                    {
                        var portObject = ModelMapper.Map<Port, PortObject>(app);
                        if (portObject != null && portObject.Id > 0)
                        {
                            portObject.CountryName = app.Country.Name;
                            objList.Add(portObject);
                        }
                    });

                    return !objList.Any() ? new List<PortObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
        public List<PortObject> GetPorts(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var ports =
                            db.Ports
                            .OrderByDescending(m => m.Id)
                             .Include("Country")
                             .Skip(tpageNumber).Take(tsize)
                             .Take(tsize)
                             .ToList();
                        if (ports.Any())
                        {
                            var newList = new List<PortObject>();
                            ports.ForEach(app =>
                            {
                                var portObject = ModelMapper.Map<Port, PortObject>(app);
                                if (portObject != null && portObject.Id > 0)
                                {
                                    portObject.CountryName = app.Country.Name;
                                    newList.Add(portObject);
                                }
                            });
                            countG = db.Ports.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<PortObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<PortObject>();
            }
        }

        public PortObject GetPort(long countryId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var ports =
                        db.Ports.Where(m => m.Id == countryId)
                            .ToList();
                    if (!ports.Any())
                    {
                        return new PortObject();
                    }

                    var app = ports[0];
                    var portObject = ModelMapper.Map<Port, PortObject>(app);
                    if (portObject == null || portObject.Id < 1)
                    {
                        return new PortObject();
                    }
                    portObject.CountryName = app.Country.Name;
                    return portObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new PortObject();
            }
        }

        public List<PortObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var ports =
                        db.Ports
                        .Include("Country")

                        .Where(m => m.Name.ToLower() == searchCriteria.ToLower().Trim()
                        || m.Country.Name.ToLower() == searchCriteria.ToLower().Trim())
                        .ToList();
                    if (!ports.Any())
                    {
                        return new List<PortObject>();
                    }
                    var newList = new List<PortObject>();
                    ports.ForEach(app =>
                    {
                        var portObject = ModelMapper.Map<Port, PortObject>(app);
                        if (portObject != null && portObject.CountryId > 0)
                        {
                            portObject.CountryName = app.Country.Name;
                            newList.Add(portObject);
                        }
                    });
                    return newList;
                }
            }
            catch (Exception ex)
            {
                return new List<PortObject>();
            }
        }
        public long DeletePort(long countryId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.Ports.Where(m => m.Id == countryId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.Ports.Remove(item);
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
