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
    public class CalculatorManager
    {
        public CalculatorObject AddCalculator(CalculatorObject calculator)
        {
            try
            {
                var obj = new CalculatorObject();
                using (var db = new ImportPermitEntities())
                {
                    var depotTrunkedOut = new DepotTrunkedOut
                    {
                        
                        QuantityTrunkedOutInDepot = calculator.QuantityTrunkedOutInDepot,
                        TrunkedOutDate = calculator.TrunkedOutDate
                    };

                    db.DepotTrunkedOuts.Add(depotTrunkedOut);

                    //update the calculator

                    var calc = db.Calculators.FirstOrDefault(c => c.Id >= 1);
                    if (calc != null)
                    {
                        var quantityInCountry = calc.QuantityInCountry;
                        var currentQuantity = quantityInCountry - calculator.QuantityTrunkedOutInDepot;
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
                return new CalculatorObject();
            }
        }

        //public long UpdateCalculator(CalculatorObject calculator)
        //{
        //    try
        //    {
        //        if (calculator == null)
        //        {
        //            return -2;
        //        }

        //        var calculatorEntity = ModelMapper.Map<CalculatorObject, Calculator>(calculator);
        //        if (calculatorEntity == null || calculatorEntity.Id < 1)
        //        {
        //            return -2;
        //        }

        //        using (var db = new ImportPermitEntities())
        //        {
        //            db.Calculators.Attach(calculatorEntity);
        //            db.Entry(calculatorEntity).State = EntityState.Modified;
        //            db.SaveChanges();
        //            return calculator.Id;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return 0;
        //    }
        //}

   
        //public List<CalculatorObject> GetLocalCalculators()
        //{
        //    try
        //    {
        //        using (var db = new ImportPermitEntities())
        //        {
        //            const int nigeria = (int) CountryEnum.Nigeria;

        //            var calculators = db.Calculators.Where(c => c.Jetty.Port.Country.Name == "nigeria").ToList();

        //            if (!calculators.Any())
        //            {
        //                return new List<CalculatorObject>();
        //            }

        //            var objList = new List<CalculatorObject>();

        //            calculators.ForEach(app =>
        //            {
                      
                       
        //                    var calculatorObject = ModelMapper.Map<Calculator, CalculatorObject>(app);
        //                    if (calculatorObject != null && calculatorObject.Id > 0)
        //                    {
        //                       objList.Add(calculatorObject);
        //                    }
                        
        //           });

        //            return !objList.Any() ? new List<CalculatorObject>() : objList;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return null;
        //    }
        //}
        //public List<CalculatorObject> GetCalculators()
        //{
        //    try
        //    {

        //        using (var db = new ImportPermitEntities())
        //        {
        //            var calculators = db.Calculators.ToList();

        //            var calculatorList = new List<CalculatorObject>();

        //            //return calculators.Select(item => new CalculatorObject {Name = item.Name, Id = item.Id}).ToList();

        //            foreach (var item in calculators)
        //            {
        //                var calculatorObj = new CalculatorObject();
        //                calculatorObj.Id = item.Id;
        //                calculatorObj.Name = item.Name;

        //                calculatorList.Add(calculatorObj);
        //            }

        //            return calculatorList;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return new List<CalculatorObject>();
        //    }
        //}
        //public List<CalculatorObject> GetCalculators(long userProfileId)
        //{
        //    try
        //    {
        //        using (var db = new ImportPermitEntities())
        //        {
        //            var objList = new List<CalculatorObject>();
        //            //get the inspector's location
        //            int zoneId;
        //            var employee = db.EmployeeDesks.Where(e => e.EmployeeId == userProfileId).ToList();
        //            if (employee.Any())
        //            {
        //                zoneId = employee[0].ZoneId;
        //                //get the jettymapping in the zone
        //                var jettymappings = db.JettyMappings.Where(j => j.ZoneId == zoneId).ToList();

        //                if (jettymappings.Any())
        //                {
        //                    foreach (var item in jettymappings)
        //                    {
        //                        var calculator = db.Calculators.Where(d => d.JettyId == item.JettyId).ToList();
                               
        //                        if (calculator.Any())
        //                        {
        //                            var calculatorObject = new CalculatorObject();
        //                            calculatorObject.Id = calculator[0].Id;
        //                            calculatorObject.Name = calculator[0].Name;
        //                            objList.Add(calculatorObject);
        //                        }
        //                    }
        //                }
        //                return objList;
        //            }

        //            return new List<CalculatorObject>(); 
              
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return null;
        //    }
        //}

        //public UserProfileObject GetCalculatorAdmin(int calculatorId)
        //{
        //    try
        //    {
        //        using (var db = new ImportPermitEntities())
        //        {
                   
        //            var calculatorAdmins = (from dp in db.Calculators.Where(m => m.Id == calculatorId).Include("Jetty")
        //                              join cm in db.Importers on dp.ImporterId equals cm.Id
        //                              join cp in db.People on cm.Id equals cp.ImporterId
        //                              join us in db.UserProfiles.Where(n => n.IsAdmin) on cp.Id equals us.PersonId
        //                              join id in db.AspNetUsers on us.Id equals id.UserInfo_Id

        //                              select new UserProfileObject
        //                              {
        //                                  Id = us.Id,
        //                                  JettyId = dp.Jetty.Id,
        //                                  JettyName = dp.Jetty.Name,
        //                                  CompanyName = cm.Name,
        //                                  CalculatorId = dp.Id,
        //                                  UserId = id.Id,
        //                                  PhoneNumber = id.PhoneNumber,
        //                                  CompanyId = cm.Id,
        //                                  PersonId = cp.Id,
        //                                  FirstName = cp.FirstName,
        //                                  LastName = cp.LastName,
        //                                  Email = id.Email,
        //                                  IsActive = us.IsActive

        //                              }).ToList();

        //            if (calculatorAdmins.Any())
        //            {
        //                var calculatorUser = calculatorAdmins[0];
        //                return calculatorUser;
        //            }

        //            return new UserProfileObject();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return new UserProfileObject();
        //    }
        //}
        
        //public List<CalculatorObject> GetCalculators(int? itemsPerPage, int? pageNumber, out int countG)
        //{
        //    try
        //    {
        //        if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
        //        {
        //            var tpageNumber = (int)pageNumber;
        //            var tsize = (int)itemsPerPage;

        //            using (var db = new ImportPermitEntities())
        //            {
        //                var banks = db.Calculators.OrderByDescending(m => m.Id).Include("Importer").Include("Jetty").Skip((tpageNumber) * tsize).Take(tsize).ToList();

        //                if (!banks.Any())
        //                {
        //                    countG = 0;
        //                    return new List<CalculatorObject>();
        //                }

        //                var newList = new List<CalculatorObject>();
        //                banks.ForEach(h =>
        //                {
        //                    var dpt = new CalculatorObject
        //                    {
        //                        Id = h.Id,
        //                        Name = h.Name,
        //                        JettyId = h.Jetty.Id,
        //                        JettyName = h.Jetty.Name
        //                    };

        //                    var usrs = (from pp in db.People.Where(n => n.ImporterId == h.ImporterId)
        //                                join tt in db.UserProfiles.Include("AspNetUsers") on pp.Id equals tt.PersonId
        //                                where tt.IsAdmin
        //                                select new { tt, pp }).ToList();
        //                    if (usrs.Any())
        //                    {
        //                        var usr = usrs[0].tt;
        //                        var p = usrs[0].pp;
        //                        if (usr != null && usr.Id > 0 && p.Id > 0)
        //                        {
        //                            dpt.UserId = usr.AspNetUsers.ElementAt(0).Id; 
        //                            dpt.LastName = p.FirstName + " " + p.LastName;
        //                            dpt.PhoneNumber = usr.AspNetUsers.ElementAt(0).PhoneNumber;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        dpt.UserId = "";
        //                        dpt.PhoneNumber = "";
        //                        dpt.LastName = "";
        //                    }

        //                    newList.Add(dpt);
        //                });

        //                countG = db.Banks.Count();
        //                return newList;

        //            }

        //        }
        //        countG = 0;
        //        return new List<CalculatorObject>();
        //    }
        //    catch (Exception ex)
        //    {
        //        countG = 0;
        //        return new List<CalculatorObject>();
        //    }
        //}
        
        //public CalculatorObject GetCalculator(long calculatorId)
        //{
        //    try
        //    {
        //        using (var db = new ImportPermitEntities())
        //        {
        //            var calculators =
        //                db.Calculators.Where(m => m.Id == calculatorId)
        //                    .ToList();
        //            if (!calculators.Any())
        //            {
        //                return new CalculatorObject();
        //            }

        //            var app = calculators[0];
        //            var calculatorObject = ModelMapper.Map<Calculator, CalculatorObject>(app);
        //            if (calculatorObject == null || calculatorObject.Id < 1)
        //            {
        //                return new CalculatorObject();
        //            }
        //            calculatorObject.JettyName = app.Jetty.Name;
        //            return calculatorObject;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return new CalculatorObject();
        //    }
        //}

        //public List<CalculatorObject> Search(string searchCriteria)
        //{
        //    try
        //    {
        //        using (var db = new ImportPermitEntities())
        //        {
        //            var calculators =
        //                db.Calculators
        //                .Include("Jetty")
                        
        //                .Where(m => m.Name.ToLower() == searchCriteria.ToLower().Trim()
        //                || m.Jetty.Name.ToLower() == searchCriteria.ToLower().Trim())
        //                .ToList();
        //            if (!calculators.Any())
        //            {
        //                return new List<CalculatorObject>();
        //            }
        //            var newList = new List<CalculatorObject>();
        //            calculators.ForEach(app =>
        //            {
        //                var calculatorObject = ModelMapper.Map<Calculator, CalculatorObject>(app);
        //                if (calculatorObject != null && calculatorObject.Id > 0)
        //                {
        //                    calculatorObject.JettyName = app.Jetty.Name;
        //                    newList.Add(calculatorObject);
        //                }
        //            });
        //            return newList;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new List<CalculatorObject>();
        //    }
        //}
        //public long DeleteCalculator(long calculatorId)
        //{
        //    try
        //    {
        //        using (var db = new ImportPermitEntities())
        //        {
        //            var myItems =
        //                db.Calculators.Where(m => m.Id == calculatorId).ToList();
        //            if (!myItems.Any())
        //            {
        //                return 0;
        //            }

        //            var item = myItems[0];
        //            db.Calculators.Remove(item);
        //            db.SaveChanges();
        //            return 5;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return 0;
        //    }
        //}
    }
}
