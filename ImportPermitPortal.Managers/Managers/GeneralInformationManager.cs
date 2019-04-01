using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
    public class GeneralInformationManager
	{
       public GeneralInformationObject GetCompanyProfileAndAddresses(long companyId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var companies = (from imp in db.Importers.Where(s => s.Id == companyId)
                        .Include("ImporterAddresses")
                        .Include("Structure")
                        join ps in db.People.Where(p => p.IsAdmin == true && p.IsImporter == true) on imp.Id equals ps.ImporterId
                        join us in db.UserProfiles on ps.Id equals us.PersonId
                        join asp in db.AspNetUsers on us.Id equals  asp.UserInfo_Id
                                    select new {imp, asp}).ToList();
                   if (!companies.Any())
                   {
                       return new GeneralInformationObject();
                   }

                   var j = companies[0].imp;
                    var userInfo = companies[0].asp;
                    var businessCommencementDate = new DateTime();
                   var businessCommencementDateStr = "";
                    var commencementDate = j.BusinessCommencementDate;
                    if (commencementDate != null)
                    {
                        if (commencementDate.Value.Year > 1)
                        {
                            businessCommencementDate = ((DateTime)commencementDate);
                            businessCommencementDateStr = ((DateTime)commencementDate).ToString("dd/MM/yyyy");
                        }
                    }
                   
                   var generalInformation = new GeneralInformationObject
                   {
                       CompanyId = j.Id,
                       RCNumber = j.RCNumber,
                       TIN = j.TIN,
                       Email = userInfo.Email,
                       PhoneNumber = userInfo.PhoneNumber,
                       Name = j.Name,
                       StructureName = j.Structure != null ? j.Structure.Name : "",
                       StructureId = j.StructureId ?? 0,
                       LogoPath = j.LogoPath,
                       IsActive = j.IsActive,
                       DateAdded = j.DateAdded,
                       BusinessCommencementDate = businessCommencementDate,
                       ShortNme = j.ShortNme,
                       TotalStaff = j.TotalStaff,
                       TotalExpatriate = j.TotalExpatriate,
                       BusinessCommencementDateStr = businessCommencementDateStr,
                       CompanyAddressObjects = new List<CompanyAddressObj>()
                   };

                   if (j.ImporterAddresses.Any())
                   {
                       j.ImporterAddresses.ToList().ForEach(ca =>
                       {
                           var addresses = db.Addresses.Where(a => a.AddressId == ca.AddressId).Include("City").ToList();
                           if (addresses.Any())
                           {
                               var address = addresses[0];
                               generalInformation.CompanyAddressObjects.Add(new CompanyAddressObj
                               {
                                   AddressId = address.AddressId,
                                   AddressLine1 = address.AddressLine1,
                                   AddressLine2 = address.AddressLine2,
                                   LastUpdated = ca.LastUpdated,
                                   CityId = address.CityId,
                                   CompanyAddressId = ca.ImporterAddressId,
                                   AddressTypeId = ca.AddressTypeId,
                                   IsRegisteredSameAsOperational = ca.IsRegisteredSameAsOperational,
                               });
                           }
                       });
                   }
                   generalInformation.AppCountObject = GetCounts(j.Id);
                   return generalInformation;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new GeneralInformationObject();
           }
       }

        public ItemCountObject GetCounts(long importerId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var appCount = db.Applications.Count(m => m.ImporterId == importerId);
                    var notificationCount = db.Notifications.Count(m => m.ImporterId == importerId);
                    var recertCount = db.Recertifications.Count(m => m.Notification.ImporterId == importerId);
                    var expPermitCount = db.Permits.Where(m => m.ImporterId == importerId).ToList();
                    
                    return new ItemCountObject
                    {
                        ApplicationCount = appCount,
                        NotificationCount = notificationCount,
                        RecertificationCount = recertCount,
                        ExpiringPermitCount = expPermitCount.Count
                    };

                }
            }
            catch (Exception ex)
            {
                return new ItemCountObject();
            }
        }

        public AppCountObject GetBankerCounts(long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var appCount = (db.Applications.Where(k => k.ImporterId == importerId)).Count();

                   var notificationCount = (db.Notifications.Where(k => k.ImporterId == importerId)).Count();

                   var permitCount = (db.Permits.Where(k => k.ImporterId == importerId)).Count();

                   var recertificationCount = (db.Recertifications.Where(k => k.Notification.ImporterId == importerId)).Count();

                   return new AppCountObject
                   {
                       ApprovedAppCount = appCount,
                       SubmittedAppCount = notificationCount,
                       ProcessingAppCount = permitCount,
                       RecertificationCount = recertificationCount
                   };

               }
           }
           catch (Exception ex)
           {
               return new AppCountObject();
           }
       }
       public long ProcessCompanyProfileAndAddresses(GeneralInformationObject info)
       {
           try
           {
               if (info.CompanyId < 1)
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var duplicates = db.Importers.Where(s => s.Name.Trim().ToLower() == info.Name.Trim().ToLower()).ToList();
                   if (!duplicates.Any())
                   {
                       return -3;
                   }

                   var companies = db.Importers.Where(s => s.Id == info.CompanyId).Include("ImporterAddresses").ToList();
                   if (!companies.Any())
                   {
                       return -3;
                   }

                   var j = companies[0];
                   j.RCNumber = info.RCNumber;
                   j.TIN = info.TIN;
                   j.Name = info.Name;
                   j.StructureId = info.StructureId;
                   j.ShortNme = info.ShortNme;
                   j.TotalStaff = info.TotalStaff;
                   j.TotalExpatriate = info.TotalExpatriate;
                   j.BusinessCommencementDate = info.BusinessCommencementDate;
                   db.Entry(j).State = EntityState.Modified;
                   db.SaveChanges();

                   if (info.CompanyAddressObjects.Any())
                   {
                       var existingAddresses = j.ImporterAddresses.ToList();
                       info.CompanyAddressObjects.ForEach(i =>
                       {
                           var existing = existingAddresses.Find(u => u.AddressId == i.AddressId);
                           var add = db.Addresses.Find(i.AddressId);
                           if (existing != null && existing.AddressId > 0 && add != null && add.AddressId > 0)
                           {
                               existing.AddressTypeId = i.AddressTypeId;
                               existing.IsRegisteredSameAsOperational = i.IsRegisteredSameAsOperational;
                               db.Entry(existing).State = EntityState.Modified;
                               db.SaveChanges();

                               add.AddressLine1 = i.AddressLine1;
                               add.CityId = i.CityId;
                               add.LastUpdated = DateTime.Now.ToString("yyyy/MM/dddd");
                               db.Entry(existing).State = EntityState.Modified;
                               db.SaveChanges();
                           }
                           else
                           {
                               var newAdd = new Address
                               {
                                   AddressLine1 = i.AddressLine1,
                                   AddressLine2 = i.AddressLine2,
                                   CityId = i.CityId,
                                   LastUpdated = DateTime.Now.ToString("yyyy/MM/dddd")
                               };

                               var addInfo = db.Addresses.Add(newAdd);
                               db.SaveChanges();

                               var cmAdd = new ImporterAddress
                               {
                                   AddressId = addInfo.AddressId,
                                   AddressTypeId = i.AddressTypeId,
                                   ImporterId = info.CompanyId,
                                   LastUpdated = DateTime.Now.ToString("yyyy/MM/dddd")
                               };

                               db.ImporterAddresses.Add(cmAdd);
                               db.SaveChanges();
                           }
                       });

                   }

                   return j.Id;
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

       public long ProcessCompanyAddresses(List<CompanyAddressObject> addresses, long importerId)
       {
           try
           {
               if (!addresses.Any())
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var count = 0;

                    addresses.ForEach(i =>
                    {
                        var newAdd = new Address
                        {
                            AddressLine1 = i.AddressLine1,
                            AddressLine2 = i.AddressLine2,
                            CityId = i.CityId,
                            LastUpdated = DateTime.Now.ToString("yyyy/MM/dddd")
                        };

                        var addInfo = db.Addresses.Add(newAdd);
                        db.SaveChanges();

                        var cmAdd = new ImporterAddress
                        {
                            AddressId = addInfo.AddressId,
                            AddressTypeId = i.AddressTypeId,
                            ImporterId = importerId,
                            LastUpdated = DateTime.Now.ToString("yyyy/MM/dddd")
                        };

                        db.ImporterAddresses.Add(cmAdd);
                        db.SaveChanges();
                        count++;
                    });

                   return count == addresses.Count() ? 5 : -1;
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

       public string GetCompanyAddress(long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myObj = (from z in db.ImporterAddresses.Where(s => s.ImporterId == importerId)
                                join x in db.Addresses on z.AddressId equals x.AddressId
                                join c in db.Cities on x.CityId equals c.CityId
                                select new { z, x, c }).ToList();
                   if (!myObj.Any())
                   {
                       return "";
                   }

                   var t = myObj[0].z;
                   var r = myObj[0].x;
                   var d = myObj[0].c;
                   return r.AddressLine1 + " " + r.AddressLine2 + ", " + d.Name;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return "";
           }
       }
       public bool DeleteCompanyAddressCheckReferences(long importerAddressId, long importerId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {                   
                    var myObj = db.ImporterAddresses.Where(s => s.ImporterId == importerId && s.ImporterAddressId == importerAddressId).Include("Address").ToList();
                    if (!myObj.Any())
                    {
                        return false;
                    }
                    var ccxx = myObj[0];
                    if (ccxx.AddressTypeId == (int) AddressTypeEnum2.Operational)
                    {
                        var xxvc = db.ImporterAddresses.Where(m => m.ImporterAddressId == ccxx.ImporterAddressId && m.AddressTypeId == (int)AddressTypeEnum2.Registered).ToList();
                        if (!xxvc.Any())
                        {
                            return false;
                        }

                        xxvc[0].IsRegisteredSameAsOperational = true;
                        db.Entry(xxvc[0]).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    db.Addresses.Remove(ccxx.Address);
                    db.ImporterAddresses.Remove(ccxx);
                    var txx = db.SaveChanges();
                    return txx > 0;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return false;
            }
        }
      
       public List<CountryObject> GetCountries()
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var list = new List<CountryObject>();
                   var countries = db.Countries.OrderBy(m => m.Name).Include("Cities").ToList();
                 
                   if (!countries.Any())
                   {
                       return list;
                   }

                   countries.ForEach(c =>
                   {
                       var country = new CountryObject
                       {
                           Id = c.Id,
                           Name = c.Name,
                           CityObjects = new List<CityObject>()
                       };
                       if (c.Cities.Any())
                       {
                           c.Cities.ToList().ForEach(ct =>
                           {
                               country.CityObjects.Add(new CityObject
                               {
                                   CityId = ct.CityId,
                                   Name = ct.Name,
                                   CountryId = ct.CountryId
                               });
                           }); 
                       }

                       list.Add(country);
                   });
                  
                   return list;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<CountryObject>();
           }
       }

       public List<StructureObject> GetStructures()
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var list = new List<StructureObject>();
                   var structures = db.Structures.OrderBy(m => m.Name).ToList();

                   if (!structures.Any())
                   {
                       return list;
                   }

                   structures.ForEach(c =>
                   {
                       list.Add(new StructureObject
                       {
                           StructureId = c.StructureId,
                           Name = c.Name,
                       });
                   });

                   return list;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<StructureObject>();
           }
       }
        
       public List<DocumentTypeObject> GetApplicantUnsuppliedDocumentTypes(long importerId)
       {
           var response = new List<DocumentTypeObject>();
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myObjList = db.StandardRequirementTypes.OrderBy(m => m.Name).ToList();
                   if (!myObjList.Any())
                   {
                       return response;
                   }
                   myObjList.ForEach(n =>
                   {
                       if (db.StandardRequirements.Count(k => k.StandardRequirementTypeId == n.Id && k.ImporterId == importerId) < 1)
                       {
                           response.Add(new DocumentTypeObject
                           {
                               DocumentTypeId = n.Id,
                               Name = n.Name
                           });
                       }
                   });

                   return response;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return response;
           }
       }

       public List<StandardRequirementTypeObject> GetUStandardRequirements(long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myObjList = db.StandardRequirements.Where(m => m.ImporterId == importerId).Include("StandardRequirementType").ToList();
                   
                   var stds = db.StandardRequirementTypes.ToList();
                   if (!stds.Any())
                   {
                       return new List<StandardRequirementTypeObject>();
                   }
                   var newList = new List<StandardRequirementTypeObject>();
                   if (myObjList.Any())
                   {
                       myObjList.ForEach(n =>
                       {
                           var req = n.StandardRequirementType;
                           newList.Add(new StandardRequirementTypeObject
                           {
                               Id = req.Id,
                               Name = req.Name,
                               DocumentPath = n.DocumentPath.Replace("~", string.Empty),
                               StandardRequirementId = n.Id, 
                               IsUploaded = true,
                               ValidFromStr = n.ValidFrom.Year > 1 ? n.ValidFrom.ToString("dd/MM/yyyy") : "",
                               ValidToStr = n.ValidTo != null && n.ValidTo.Value.Year > 1 ? n.ValidTo.Value.ToString("dd/MM/yyyy") : "",
                               ImporterId = n.ImporterId
                           });
                       });
                   }
                   stds.ForEach(n =>
                   {
                       if (newList.All(k => k.Id != n.Id))  
                       {
                           newList.Add(new StandardRequirementTypeObject
                           {
                               Id = n.Id,
                               Name = n.Name,
                               IsUploaded = false
                           });
                       }
                   });
                   return newList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<StandardRequirementTypeObject>();
           }
       }

       public bool CheckAddressAvailability(long importerId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var adds = db.ImporterAddresses.Where(l => l.ImporterId == importerId).ToList();
                   return adds.Any();
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return false;
           }
       }
	}
	
}
