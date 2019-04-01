using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
    public class StandardRequirementManager
	{
        public List<StandardRequirementObject> GetStandardRequirements(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var myObjList =
                            db.StandardRequirements.Where(m => m.ImporterId == importerId)
                            .OrderByDescending(m => m.Id)
                            .Skip(tpageNumber).Take(tsize)
                            .Take(tsize)
                        .Include("StandardRequirementType")
                        .ToList();
                        if (myObjList.Any())
                        {
                            countG = db.StandardRequirements.Count(m => m.ImporterId == importerId); 
                            var newList = new List<StandardRequirementObject>();
                            myObjList.ForEach(m =>
                            {
                                var sReq = ModelMapper.Map<StandardRequirement, StandardRequirementObject>(m);
                                if (sReq != null && sReq.Id > 0)
                                {
                                    sReq.ValidToStr = m.ValidTo != null ? ((DateTime) m.ValidTo).ToString("yyyy/MM/dd"): "";
                                    sReq.ValidFromStr = m.ValidFrom.ToString("yyyy/MM/dd");
                                    newList.Add(sReq);
                                }
                            });
                            
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<StandardRequirementObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<StandardRequirementObject>();
            }
        }

        public List<StandardRequirementObject> Search(string searchCriteria, long companyId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var reqs = (from ff in db.StandardRequirements.Where(m => m.ImporterId == companyId).Include("StandardRequirementType")
                                join apst in db.StandardRequirementTypes.Where(n => n.Name.ToLower().Trim() == searchCriteria.ToLower().Trim()) on ff.StandardRequirementTypeId equals apst.Id
                                select new StandardRequirementObject
                                {
                                    Id = ff.Id,
                                    StandardRequirementTypeName = ff.StandardRequirementType.Name,
                                    Title = ff.Title,
                                    ValidFrom = ff.ValidFrom,
                                    ValidTo = ff.ValidTo,
                                    DocumentPath = ff.DocumentPath
                                }).ToList();

                    if (!reqs.Any())
                    {
                        return new List<StandardRequirementObject>();
                    }
                    return reqs;
                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StandardRequirementObject>();
            }
        }

        public long GetCompanyDocumentCount(long importerId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    return db.StandardRequirements.Count(m => m.ImporterId == importerId);
                }
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddStandardRequirement(StandardRequirementObject sReq)
        {
            try
            {
                if (sReq == null)
                {
                    return -2;
                }
                
                using (var db = new ImportPermitEntities())
                {
                    if (db.StandardRequirements.Count(m => m.StandardRequirementTypeId == sReq.StandardRequirementTypeId && m.ImporterId == sReq.ImporterId && m.ValidFrom == sReq.ValidFrom && m.ValidTo == sReq.ValidTo) > 0)
                    {
                        return -3;
                    }
                    var sReqTypeEntity = ModelMapper.Map<StandardRequirementObject, StandardRequirement>(sReq);
                    if (sReqTypeEntity == null || sReq.StandardRequirementTypeId < 1)
                    {
                        return -2;
                    }
                    var processedCompany = db.StandardRequirements.Add(sReqTypeEntity);
                    db.SaveChanges();
                    return processedCompany.Id;
                }
            }
            catch (DbEntityValidationException e)
            {
                var str = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    str += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State) + "\n";
                    str = eve.ValidationErrors.Aggregate(str, (current, ve) => current + (string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage) + " \n"));
                }
                ErrorLogger.LoggError(e.StackTrace, e.Source, str);
                return 0;
            }
        }

        public int UpdateStandardRequirement(StandardRequirementObject sReq)
        {
            try
            {
                if (sReq == null)
                { return -2; }
                using (var db = new ImportPermitEntities())
                {
                    if (db.StandardRequirements.Count(m => m.StandardRequirementTypeId == sReq.StandardRequirementTypeId && m.ImporterId == sReq.ImporterId && m.ValidFrom == sReq.ValidFrom && m.ValidTo == sReq.ValidTo && m.Id != sReq.Id) > 0)
                    {
                        return -3;
                    }
                    var sReqTypeEntity = ModelMapper.Map<StandardRequirementObject, StandardRequirement>(sReq);
                    if (sReqTypeEntity == null || sReq.StandardRequirementTypeId < 1)
                    {
                        return -2;
                    }
                    db.StandardRequirements.Attach(sReqTypeEntity);
                    db.Entry(sReqTypeEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return 5;
                }
            }
            catch (DbEntityValidationException e)
            {
                var str = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    str += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State) + "\n";
                    str = eve.ValidationErrors.Aggregate(str, (current, ve) => current + (string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage) + " \n"));
                }
                ErrorLogger.LoggError(e.StackTrace, e.Source, str);
                return 0;
            }
        }

        public StandardRequirementObject GetStandardRequirementByImporter(long importerId, long sReqId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myObj = db.StandardRequirements.Where(s => s.ImporterId == importerId && s.Id == sReqId).ToList();
                    if (!myObj.Any())
                    {
                        return new StandardRequirementObject();
                    }
                    var m = myObj[0];
                    var sReq = ModelMapper.Map<StandardRequirement, StandardRequirementObject>(m);
                    if (sReq == null || sReq.Id < 1)
                    {
                        return new StandardRequirementObject();
                    }
                    sReq.ValidToStr = m.ValidTo != null ? ((DateTime)m.ValidTo).ToString("yyyy/MM/dd") : "";
                    sReq.ValidFromStr = m.ValidFrom.ToString("yyyy/MM/dd");
                    return sReq;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new StandardRequirementObject();
            }
        }
        
        public StandardRequirementObject GetStandardRequirement(long sReqId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myObj = db.StandardRequirements.Where(s => s.Id == sReqId).ToList();
                    if (!myObj.Any())
                    {
                        return new StandardRequirementObject();
                    }
                    var m = myObj[0];
                    var sReq = ModelMapper.Map<StandardRequirement, StandardRequirementObject>(m);
                    if (sReq == null || sReq.Id < 1)
                    {
                        return new StandardRequirementObject();
                    }
                    sReq.ValidToStr = m.ValidTo != null ? ((DateTime)m.ValidTo).ToString("yyyy/MM/dd") : "";
                    sReq.ValidFromStr = m.ValidFrom.ToString("yyyy/MM/dd");
                    return sReq;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new StandardRequirementObject();
            }
        }
        public string DeleteStandardRequirement(long sReqId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myObj = db.StandardRequirements.Where(s => s.Id == sReqId).ToList();
                    if (!myObj.Any())
                    {
                        return string.Empty;
                    }
                    db.StandardRequirements.Remove(myObj[0]);
                    var txx = db.SaveChanges();
                    return myObj[0].DocumentPath;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return string.Empty;
            }
        }
        public List<StandardRequirementObject> SearchLibraries(string searchCriteria, long companyId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var lwer = searchCriteria.ToLower().Trim();
                    var myObjList =
                        db.StandardRequirements.Where(m => m.ImporterId == companyId && m.Title.ToLower().Contains(lwer) || m.StandardRequirementType.Name.ToLower().Trim().Contains(lwer))
                        .OrderByDescending(m => m.Id)
                        .Include("StandardRequirementType")
                        .ToList();
                    if (!myObjList.Any())
                    {
                        return new List<StandardRequirementObject>();
                    }
                    var docLib = new List<StandardRequirementObject>();
                    myObjList.ForEach(m => docLib.Add(new StandardRequirementObject
                    {
                        Id = m.Id,
                        StandardRequirementTypeName = m.StandardRequirementType.Name,
                        Title = m.Title,
                        ValidFrom = m.ValidFrom,
                        ValidToStr = m.ValidTo != null ?((DateTime)m.ValidTo).ToString("yyyy/MM/dd") : "",
                        ValidFromStr = m.ValidFrom.ToString("yyyy/MM/dd"),
                        ValidTo = m.ValidTo,
                        DocumentPath = m.DocumentPath
                    }));
                    return docLib.OrderByDescending(m => m.StandardRequirementTypeName).ToList();
                }
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StandardRequirementObject>();
            }
        }
	}
	
}
