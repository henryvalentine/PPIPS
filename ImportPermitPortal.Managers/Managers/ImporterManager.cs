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
   public class ImporterManager
    {
       public long AddImporter(ImporterObject importer)
       {
           try
           {
               if (importer == null)
               {
                   return -2;
               }

               var importerEntity = ModelMapper.Map<ImporterObject, Importer>(importer);
               if (string.IsNullOrEmpty(importerEntity.Name))
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   long importerId = 0;
                   if (db.Importers.Any())
                   {
                       var existing = db.Importers.OrderByDescending(i => i.Id).Take(1).ToList();
                       if (!existing.Any())
                       {
                           importerId = 1;
                       }
                       else
                       {
                           importerId = existing[0].Id + 1;
                       }
                   }
                   else
                   {
                       importerId = 1;
                   }
                   importerEntity.Id = importerId;
                   var returnStatus = db.Importers.Add(importerEntity);
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
       public long AddImporterAndPerson(ImporterObject importer, PersonObject person, out long importerId)
       {
           try
           {
               if (importer == null || person == null)
               {
                   importerId = 0;
                   return -2;
               }
               long personId;
               using (var db = new ImportPermitEntities())
               {
                   var existings = db.Importers.Where(i => i.Name.ToLower() == importer.Name.ToLower() || i.RCNumber.ToLower().Replace("rc", "") == importer.RCNumber.ToLower().Replace("rc", "") || i.TIN.ToLower().Replace("tin", "") == importer.TIN.ToLower().Replace("tin", "")).ToList();
                   if (existings.Any())
                   {
                       importerId = existings[0].Id;
                   }
                   else
                   {
                       importerId = 0;
                   }
                   var impId = importerId;
                   var existingPeople = db.People.Where(i => i.FirstName.ToLower() == person.FirstName.ToLower() && i.LastName.ToLower() == person.LastName.ToLower() && i.ImporterId == impId).ToList();

                   if (existingPeople.Any())
                   {
                       personId = existingPeople[0].Id;
                   }
                   else
                   {
                       personId = 0;
                   }

                   if (importerId > 0 && personId > 0)
                   {
                       return personId;
                   }

                   var importerEntity = ModelMapper.Map<ImporterObject, Importer>(importer);
                   if (importerEntity == null || string.IsNullOrEmpty(importerEntity.Name))
                   {
                       importerId = 0;
                       return -2;
                   }
                   var personEntity = ModelMapper.Map<PersonObject, Person>(person);
                   if (personEntity == null || string.IsNullOrEmpty(importerEntity.Name))
                   {
                       importerId = 0;
                       return -2;
                   }

                   if (importerId < 1)
                   {
                      var importerInfo = db.Importers.Add(importerEntity);
                       db.SaveChanges();
                       importerId = importerInfo.Id;
                   }

                   if (personId < 1)
                   {
                       personEntity.ImporterId = importerId;
                       var prs = db.People.Add(personEntity);
                       db.SaveChanges();
                       personId = prs.Id;
                   }

                   return personId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               importerId = 0;
               return 0;
           }
       }

       public long AddImporterDepotAndPerson(UserProfileObject person)
       {
           try
           {
               if (person == null)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {

                   var importer = new Importer
                   {
                       Name = person.CompanyName,
                       IsActive = true,
                       DateAdded = DateTime.Now.ToString("yyyy/MM/dd")
                   };

                   var returnStatus = db.Importers.Add(importer);
                   db.SaveChanges();

                   var prs = new Person
                   {
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        ImporterId = returnStatus.Id,
                        IsAdmin = true,
                        IsImporter = false,
                   };
                  
                    var depotEntity = new Depot
                   {
                       Name = importer.Name,
                       ImporterId = returnStatus.Id,
                       JettyId = person.JettyId
                   };


                   db.Depots.Add(depotEntity);
                   db.SaveChanges();
                   
                   var prsId =db.People.Add(prs);
                   db.SaveChanges();
                   return prsId.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public long UpdateImporterDepotAndPerson(UserProfileObject person)
       {
           try
           {
               if (person == null)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                    var existing = db.Importers.Where(i => i.Id == person.CompanyId).ToList();
                    if (!existing.Any())
                    {
                        return -2;
                    }
                    var importer = existing[0];

                    var existingDepot = db.Depots.Where(i => i.ImporterId == importer.Id).ToList();
                    if (!existingDepot.Any())
                    {
                        return -2;
                    }

                    var existingp = db.People.Where(i => i.Id == person.PersonId).ToList();
                    if (!existingp.Any())
                    {
                        return -2;
                    }
                  
                   importer.Name = person.CompanyName;
                   importer.IsActive = person.IsActive;

                   var prs = existingp[0];
                   prs.FirstName = person.FirstName;
                   prs.LastName = person.LastName;

                   var depotEntity = existingDepot[0];
                   depotEntity.Name = person.CompanyName;

                   db.Entry(importer).State = EntityState.Modified;
                   db.SaveChanges();

                   db.Entry(prs).State = EntityState.Modified;
                   db.SaveChanges();

                   db.Entry(depotEntity).State = EntityState.Modified;
                   db.SaveChanges();

                   return importer.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public long UpdateImporter(ImporterObject importer)
       {
           try
           {
               if (importer == null)
               {
                   return -2;
               }

               var importerEntity = ModelMapper.Map<ImporterObject, Importer>(importer);
               if (importerEntity == null || importerEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.Importers.Attach(importerEntity);
                   db.Entry(importerEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return importer.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<ImporterObject> GetImporters()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var importers = db.Importers.ToList();
                   if (!importers.Any())
                   {
                        return new List<ImporterObject>();
                   }
                   var objList =  new List<ImporterObject>();
                   importers.ForEach(app =>
                   {
                       var importerObject = ModelMapper.Map<Importer, ImporterObject>(app);
                       if (importerObject != null && importerObject.Id > 0)
                       {
                           objList.Add(importerObject);
                       }
                   });

                   return !objList.Any() ? new List<ImporterObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

        public List<ImporterObject> GetImporters(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var dpr = (long) Dpr.DPR;
                       var importers =
                           db.Importers.Where(m => !m.Banks.Any() && m.Id != dpr).OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize).Include("Structure")
                               .ToList();
                       if (importers.Any())
                       {
                           var newList = new List<ImporterObject>();
                           importers.ForEach(app =>
                           {
                               var importerObject = ModelMapper.Map<Importer, ImporterObject>(app);
                               if (importerObject != null && importerObject.Id > 0)
                               {
                                   if (app.Structure != null)
                                   {
                                       importerObject.StructureName = app.Structure.Name;
                                   }

                                   importerObject.StatusStr = importerObject.IsActive ? "Active" : "Inactive";
                                    
                                   newList.Add(importerObject);
                               }
                           });
                           countG = db.Importers.Count(m => !m.Banks.Any() && m.Id != dpr);
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<ImporterObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<ImporterObject>();
           }
       }
       
       public ImporterObject GetImporter(long companyId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var companies = db.Importers.Where(s => s.Id == companyId).Include("ImporterAddresses").Include("Structure").ToList();
                   if (!companies.Any())
                   {
                       return new ImporterObject();
                   }
                   var j = companies[0];
                   var businessCommencementDateStr = "";
                   var commencementDate = j.BusinessCommencementDate;
                   if (commencementDate != null)
                   {
                       if (commencementDate.Value.Year > 1)
                       {
                           businessCommencementDateStr = ((DateTime) commencementDate).ToString("dd/MM/yyyy");
                       }
                   }
                   else
                   {
                       businessCommencementDateStr = "Not Available";
                   }

                   var importerObject = ModelMapper.Map<Importer, ImporterObject>(j);
                   if (importerObject == null || importerObject.Id < 1)
                   {
                       return new ImporterObject();
                   }
                   importerObject.StatusStr = importerObject.IsActive ? "Active" : "Inactive";
                   importerObject.BusinessCommencementDateStr = businessCommencementDateStr;
                   if (j.ImporterAddresses.Any())
                   {
                       j.ImporterAddresses.ToList().ForEach(ca =>
                       {
                           var addresses = db.Addresses.Where(a => a.AddressId == ca.AddressId).Include("City").ToList();
                           if (addresses.Any())
                           {
                               var address = addresses[0];
                               importerObject.AddressStr = address.AddressLine1 + " " + address.AddressLine2 + " " + address.City.Name;
                           }
                       });
                   }

                   importerObject.AppCountObject = GetBankerCounts(importerObject.Id);
                   return importerObject;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ImporterObject();
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
                       RecertificationCount = permitCount,
                       ProcessingAppCount=  recertificationCount
                   };

               }
           }
           catch (Exception ex)
           {
               return new AppCountObject();
           }
       }

       public ImporterObject GetImporterByLoggedOnUser(string userId, bool isApplicant)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var importers = (from ix in db.AspNetUsers.Where(m => m.Id == userId)
                                    join usp in db.UserProfiles on ix.UserInfo_Id equals usp.Id
                                    join pp in db.People on usp.PersonId equals pp.Id
                                    join cmp in db.Importers on pp.ImporterId equals cmp.Id
                                   
                                    select new ImporterObject
                                    {
                                        Id = cmp.Id,
                                        Name = cmp.Name,
                                        TIN = cmp.TIN,
                                        RCNumber = cmp.RCNumber,
                                        UserProfileObject = new UserProfileObject
                                        {
                                            Id = usp.Id,
                                            UserId = ix.Id,
                                            FirstName = pp.FirstName,
                                            LastName = pp.LastName,
                                            Email = ix.Email,
                                            PhoneNumber = ix.PhoneNumber,
                                            IsActive = usp.IsActive && cmp.IsActive,
                                        }
                                    }).ToList();

                   if (!importers.Any())
                   {
                       return new ImporterObject();
                   }

                   var importer = importers[0];

                   return importer;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ImporterObject();
           }
       }

       public ImporterObject GetAdminImporterUser(string userId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var importers = (from ix in db.AspNetUsers.Where(m => m.Id == userId)
                                    join usp in db.UserProfiles on ix.UserInfo_Id equals usp.Id
                                    join pp in db.People on usp.PersonId equals pp.Id
                                    join cmp in db.Importers on pp.ImporterId equals cmp.Id
                                    select new ImporterObject
                                    {
                                        Id = cmp.Id,
                                        Name = cmp.Name,
                                        TIN = cmp.TIN,
                                        RCNumber = cmp.RCNumber,
                                        UserProfileObject = new UserProfileObject
                                        {
                                            Id = usp.Id,
                                            FirstName = pp.FirstName,
                                            LastName = pp.LastName,
                                            Email = ix.Email,
                                            PhoneNumber = ix.PhoneNumber,
                                            IsActive = usp.IsActive && cmp.IsActive
                                        }
                                    }).ToList();

                   if (!importers.Any())
                   {
                       return new ImporterObject();
                   }

                   return importers[0];
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ImporterObject();
           }
       }
       
       public List<ImporterObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var importers =
                       db.Importers.Where(m => m.Name.ToLower().Trim().Contains(searchCriteria.ToLower().Trim()) || m.RCNumber.ToLower().Trim().Contains(searchCriteria.ToLower().Trim())
                       || m.TIN.ToLower().Trim().Contains(searchCriteria.ToLower().Trim())).Include("Structure")
                       .ToList();

                   if (importers.Any())
                   {
                       var newList = new List<ImporterObject>();
                       importers.ForEach(app =>
                       {
                           var importerObject = ModelMapper.Map<Importer, ImporterObject>(app);
                           if (importerObject != null && importerObject.Id > 0)
                           {
                               if (app.Structure != null)
                               {
                                   importerObject.StructureName = app.Structure.Name;
                               }

                               importerObject.StatusStr = importerObject.IsActive ? "Active" : "Inactive";
                                    
                               newList.Add(importerObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<ImporterObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<ImporterObject>();
           }
       }
       
       public long DeleteImporter(long companyId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.Importers.Where(m => m.Id == companyId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.Importers.Remove(item);
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
