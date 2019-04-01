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
    public class CountryManager
    {
        public long AddCountry(CountryObject country)
        {
            try
            {
                if (country == null)
                {
                    return -2;
                }

                var countryEntity = ModelMapper.Map<CountryObject, Country>(country);

                if (countryEntity == null)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    if (db.Countries.Count(f => f.Name.ToLower().Trim() == country.Name.ToLower().Trim()) > 0)
                    {
                        return -3;
                    }
                    var returnStatus = db.Countries.Add(countryEntity);
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

        public long UpdateCountry(CountryObject country)
        {
            try
            {
                if (country == null)
                {
                    return -2;
                }

                var countryEntity = ModelMapper.Map<CountryObject, Country>(country);
                if (countryEntity == null || countryEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    if (db.Countries.Count(f => f.Name.ToLower().Trim() == country.Name.ToLower().Trim() && f.Id != country.Id) > 0)
                    {
                        return -3;
                    }

                    db.Countries.Attach(countryEntity);
                    db.Entry(countryEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return country.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<CountryObject> GetCountries()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var countries = db.Countries.ToList();
                    if (!countries.Any())
                    {
                        return new List<CountryObject>();
                    }
                    var objList = new List<CountryObject>();
                    countries.ForEach(app =>
                    {
                        var countryObject = ModelMapper.Map<Country, CountryObject>(app);
                        if (countryObject != null && countryObject.Id > 0)
                        {
                            var coo = countryObject.Name;
                            objList.Add(countryObject);
                        }
                    });

                    return !objList.Any() ? new List<CountryObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<CountryObject> GetCountriesByRegion(int regionId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var countries = db.Countries.Where(m => m.RegionId == regionId)
                        .Include("Region").ToList();
                    if (!countries.Any())
                    {
                        return new List<CountryObject>();
                    }
                    var objList = new List<CountryObject>();
                    countries.ForEach(app =>
                    {
                        var countryObject = ModelMapper.Map<Country, CountryObject>(app);
                        if (countryObject != null && countryObject.Id > 0)
                        {
                            countryObject.RegionName = app.Region.Name;
                            objList.Add(countryObject);
                        }
                    });

                    return !objList.Any() ? new List<CountryObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<CountryObject> GetCountriesWithPorts()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var countries = db.Countries
                        .Include("Ports").ToList();
                    if (!countries.Any())
                    {
                        return new List<CountryObject>();
                    }
                    var objList = new List<CountryObject>();
                    countries.ForEach(app =>
                    {
                        var countryObject = ModelMapper.Map<Country, CountryObject>(app);
                        if (countryObject != null && countryObject.Id > 0)
                        {
                            var ports = app.Ports.ToList();
                            countryObject.PortObjects = new List<PortObject>();
                            ports.ForEach(pt =>
                            {
                                var portObject = ModelMapper.Map<Port, PortObject>(pt);
                                if (portObject != null && portObject.Id > 0)
                                {
                                    countryObject.PortObjects.Add(portObject);

                                }
                            });

                            objList.Add(countryObject);
                        }
                    });

                    return !objList.Any() ? new List<CountryObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<CountryObject> GetCountries(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var countries =
                            db.Countries
                            .OrderByDescending(m => m.Id)
                             .Include("Region")
                              .Skip(tpageNumber).Take(tsize)
                             .ToList();
                        if (countries.Any())
                        {
                            var newList = new List<CountryObject>();
                            countries.ForEach(app =>
                            {
                                var countryObject = ModelMapper.Map<Country, CountryObject>(app);
                                if (countryObject != null && countryObject.Id > 0)
                                {
                                    countryObject.RegionName = app.Region.Name;
                                    
                                    newList.Add(countryObject);
                                }
                            });
                            countG = db.Countries.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<CountryObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<CountryObject>();
            }
        }

        public CountryObject GetCountry(long countryId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var countries =
                        db.Countries.Where(m => m.Id == countryId)
                            .ToList();
                    if (!countries.Any())
                    {
                        return new CountryObject();
                    }

                    var app = countries[0];
                    var countryObject = ModelMapper.Map<Country, CountryObject>(app);
                    if (countryObject == null || countryObject.Id < 1)
                    {
                        return new CountryObject();
                    }
                    countryObject.RegionName = app.Region.Name;
                    return countryObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new CountryObject();
            }
        }

        public List<CountryObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var countries =
                        db.Countries
                        .Include("Region")
                        
                        .Where(m => m.Name.ToLower() == searchCriteria.ToLower().Trim()
                        || m.Region.Name.ToLower() == searchCriteria.ToLower().Trim())
                        .ToList();
                    if (!countries.Any())
                    {
                        return new List<CountryObject>();
                    }
                    var newList = new List<CountryObject>();
                    countries.ForEach(app =>
                    {
                        var countryObject = ModelMapper.Map<Country, CountryObject>(app);
                        if (countryObject != null && countryObject.RegionId > 0)
                        {
                            countryObject.RegionName = app.Region.Name;
                            newList.Add(countryObject);
                        }
                    });
                    return newList;
                }
            }
            catch (Exception ex)
            {
                return new List<CountryObject>();
            }
        }
        public long DeleteCountry(long countryId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.Countries.Where(m => m.Id == countryId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.Countries.Remove(item);
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
