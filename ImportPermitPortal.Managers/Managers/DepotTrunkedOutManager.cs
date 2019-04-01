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
    public class DepotTrunkedOutManager
    {
        public long AddDepotTrunkedOut(DepotTrunkedOutObject depotTrunkedOut)
        {
            try
            {
                if (depotTrunkedOut == null)
                {
                    return -2;
                }



                using (var db = new ImportPermitEntities())
                {
                    var depotTrunkedOutEntity = new DepotTrunkedOut
                    {
                        DepotId = depotTrunkedOut.DepotId,
                        QuantityTrunkedOutInDepot = depotTrunkedOut.QuantityTrunkedOutInDepot,
                        TrunkedOutDate = depotTrunkedOut.TrunkedOutDate
                    };
                    var returnStatus = db.DepotTrunkedOuts.Add(depotTrunkedOutEntity);

                    //
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

        public long UpdateDepotTrunkedOut(DepotTrunkedOutObject depotTrunkedOut)
        {
            try
            {
                if (depotTrunkedOut == null)
                {
                    return -2;
                }

                var depotTrunkedOutEntity = ModelMapper.Map<DepotTrunkedOutObject, DepotTrunkedOut>(depotTrunkedOut);
                if (depotTrunkedOutEntity == null || depotTrunkedOutEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.DepotTrunkedOuts.Attach(depotTrunkedOutEntity);
                    db.Entry(depotTrunkedOutEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return depotTrunkedOut.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<DepotTrunkedOutObject> GetDepotTrunkedOuts()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var depotTrunkedOuts = db.DepotTrunkedOuts.ToList();
                    if (!depotTrunkedOuts.Any())
                    {
                        return new List<DepotTrunkedOutObject>();
                    }
                    var objList = new List<DepotTrunkedOutObject>();
                    depotTrunkedOuts.ForEach(app =>
                    {
                        var depotTrunkedOutObject = ModelMapper.Map<DepotTrunkedOut, DepotTrunkedOutObject>(app);
                        if (depotTrunkedOutObject != null && depotTrunkedOutObject.Id > 0)
                        {
                            objList.Add(depotTrunkedOutObject);
                        }
                    });

                    return !objList.Any() ? new List<DepotTrunkedOutObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }


        public CalculatorObject Calculator(DepotTrunkedOutObject depotTrunked)
        {
            try
            {
                var obj = new CalculatorObject();
                using (var db = new ImportPermitEntities())
                {
                    var depotTrunkedOut = new DepotTrunkedOut
                    {
                        DepotId = depotTrunked.DepotId,
                        QuantityTrunkedOutInDepot = depotTrunked.QuantityTrunkedOutInDepot,
                        TrunkedOutDate = depotTrunked.TrunkedOutDate
                    };

                    db.DepotTrunkedOuts.Add(depotTrunkedOut);

                    //update the calculator
                    
                    var calc = db.Calculators.FirstOrDefault(c=>c.Id >= 1);
                    if (calc != null)
                    {
                        var quantityInCountry = calc.QuantityInCountry;
                        var currentQuantity = quantityInCountry - depotTrunked.QuantityTrunkedOutInDepot;
                        calc.QuantityInCountry = currentQuantity;
                        calc.Counter = calc.Counter + 1;
                        db.Calculators.Attach(calc);
                        db.Entry(calc).State = EntityState.Modified;
                        obj.QuantityInCountry = currentQuantity.ToString();
                        obj.Good = true;
                        //to get the daily consumption, fetch the first row and last row in the depotTrunkedOut table, minus the dates and get num of days then use it to divide the QuantityInCountry
                        db.SaveChanges();

                        return obj;
                    }
                    obj.Bad = true;
                    return obj;



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

        public List<DepotTrunkedOutObject> GetDepotTrunkedOuts(long portId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var depotTrunkedOuts = db.DepotTrunkedOuts.Where(m => m.Id == portId)
                        .Include("Depot").ToList();
                    if (!depotTrunkedOuts.Any())
                    {
                        return new List<DepotTrunkedOutObject>();
                    }
                    var objList = new List<DepotTrunkedOutObject>();
                    depotTrunkedOuts.ForEach(app =>
                    {
                        var depotTrunkedOutObject = ModelMapper.Map<DepotTrunkedOut, DepotTrunkedOutObject>(app);
                        if (depotTrunkedOutObject != null && depotTrunkedOutObject.Id > 0)
                        {
                            depotTrunkedOutObject.DepotName = app.Depot.Name;
                            objList.Add(depotTrunkedOutObject);
                        }
                    });

                    return !objList.Any() ? new List<DepotTrunkedOutObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
        public List<DepotTrunkedOutObject> GetDepotTrunkedOuts(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var depotTrunkedOuts =
                            db.DepotTrunkedOuts
                            .OrderByDescending(m => m.Id)
                             .Include("Depot")
                             .Skip(tpageNumber).Take(tsize)
                             .ToList();
                        if (depotTrunkedOuts.Any())
                        {
                            var newList = new List<DepotTrunkedOutObject>();
                            depotTrunkedOuts.ForEach(app =>
                            {
                                var depotTrunkedOutObject = ModelMapper.Map<DepotTrunkedOut, DepotTrunkedOutObject>(app);
                                if (depotTrunkedOutObject != null && depotTrunkedOutObject.Id > 0)
                                {
                                    depotTrunkedOutObject.DepotName = app.Depot.Name;
                                    newList.Add(depotTrunkedOutObject);
                                }
                            });
                            countG = db.DepotTrunkedOuts.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<DepotTrunkedOutObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<DepotTrunkedOutObject>();
            }
        }

        public DepotTrunkedOutObject GetDepotTrunkedOut(long depotTrunkedOutId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var depotTrunkedOuts =
                        db.DepotTrunkedOuts.Where(m => m.Id == depotTrunkedOutId)
                            .ToList();
                    if (!depotTrunkedOuts.Any())
                    {
                        return new DepotTrunkedOutObject();
                    }

                    var app = depotTrunkedOuts[0];
                    var depotTrunkedOutObject = ModelMapper.Map<DepotTrunkedOut, DepotTrunkedOutObject>(app);
                    if (depotTrunkedOutObject == null || depotTrunkedOutObject.Id < 1)
                    {
                        return new DepotTrunkedOutObject();
                    }
                    depotTrunkedOutObject.DepotName = app.Depot.Name;
                    return depotTrunkedOutObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new DepotTrunkedOutObject();
            }
        }

        public List<DepotTrunkedOutObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var depotTrunkedOuts =
                        db.DepotTrunkedOuts
                        .Include("Depot")
                        
                        .Where(m => m.Depot.Name.ToLower() == searchCriteria.ToLower().Trim()
                        || m.TrunkedOutDate.ToString().ToLower() == searchCriteria.ToLower().Trim())
                        .ToList();
                    if (!depotTrunkedOuts.Any())
                    {
                        return new List<DepotTrunkedOutObject>();
                    }
                    var newList = new List<DepotTrunkedOutObject>();
                    depotTrunkedOuts.ForEach(app =>
                    {
                        var depotTrunkedOutObject = ModelMapper.Map<DepotTrunkedOut, DepotTrunkedOutObject>(app);
                        if (depotTrunkedOutObject != null && depotTrunkedOutObject.Id > 0)
                        {
                            depotTrunkedOutObject.DepotName = app.Depot.Name;
                            newList.Add(depotTrunkedOutObject);
                        }
                    });
                    return newList;
                }
            }
            catch (Exception ex)
            {
                return new List<DepotTrunkedOutObject>();
            }
        }
        public long DeleteDepotTrunkedOut(long depotTrunkedOutId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.DepotTrunkedOuts.Where(m => m.Id == depotTrunkedOutId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.DepotTrunkedOuts.Remove(item);
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
