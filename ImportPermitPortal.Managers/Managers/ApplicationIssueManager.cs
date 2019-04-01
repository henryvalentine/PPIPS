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
    public class ApplicationIssueManager
    {


        public ApplicationObject GetApplicationFromIssue(long issueId, long userId)
        {


            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var issue = db.ApplicationIssues.Where(i => i.Id == issueId).ToList();
                    if (issue.Any())
                    {
                        var issId = issue[0].ApplicationId;
                        var myApplications =
                            db.Applications.Where(m => m.Id == issId)
                                .Include("Importer")
                                .Include("ApplicationItems")
                                .Include("ApplicationDocuments")
                                .Include("NotificationBankers")
                                .ToList();
                    if (!myApplications.Any())
                    {
                        return new ApplicationObject();
                    }

                    var app = myApplications[0];
                    var importObject = ModelMapper.Map<Application, ApplicationObject>(app);
                    if (importObject == null || importObject.Id < 1)
                    {
                        return new ApplicationObject();
                    }

                    importObject.ReferenceCode = app.Invoice.ReferenceCode;
                    importObject.DateAppliedStr = app.DateApplied.ToString("dd/MM/yyyy");
                    importObject.StatusStr = Enum.GetName(typeof(AppStatus), app.ApplicationStatusCode);
                    importObject.LastModifiedStr = importObject.LastModified.ToString("dd/MM/yyyy");
                    importObject.ImporterStr = app.Importer.Name;
                    importObject.ApplicationTypeName = app.ApplicationType.Name;


                    importObject.ApplicationItemObjects = new List<ApplicationItemObject>();
                    importObject.ApplicationDocumentObjects = new List<ApplicationDocumentObject>();
                    importObject.StandardRequirementObjects = new List<StandardRequirementObject>();

                    var permApp = db.PermitApplications.Where(p => p.ApplicationId == app.Id).ToList();

                    if (permApp.Any())
                    {
                        var permId = permApp[0].PermitId;
                        var perm = db.Permits.Where(m => m.Id == permId).ToList();
                        if (perm.Any())
                        {
                            importObject.PermitStr = perm[0].PermitValue;
                        }
                        else if (!perm.Any())
                        {
                            importObject.PermitStr = "Nil";
                        }

                    }



                    app.ApplicationItems.ToList().ForEach(u =>
                    {
                        var im = ModelMapper.Map<ApplicationItem, ApplicationItemObject>(u);
                        if (im != null && im.ApplicationId > 0)
                        {
                            im.ProductObject = (from pr in db.Products.Where(x => x.ProductId == im.ProductId)
                                select new ProductObject
                                {
                                    ProductId = pr.ProductId,
                                    Code = pr.Code,
                                    Name = pr.Name,
                                    Availability = pr.Availability
                                }).ToList()[0];

                            var appCountries = db.ApplicationCountries.Where(a => a.ApplicationItemId == im.Id).Include("Country").ToList();
                            var depotList = db.ThroughPuts.Where(a => a.ApplicationItemId == im.Id).Include("Depot").ToList();
                            if (appCountries.Any() && depotList.Any())
                            {
                                im.CountryOfOriginName = "";
                                appCountries.ForEach(c =>
                                {
                                    if (string.IsNullOrEmpty(im.CountryOfOriginName))
                                    {
                                        im.CountryOfOriginName = c.Country.Name;
                                    }
                                    else
                                    {
                                        im.CountryOfOriginName += ", " + c.Country.Name;
                                    }
                                });

                                im.DischargeDepotName = "";
                                depotList.ForEach(d =>
                                {
                                    if (string.IsNullOrEmpty(im.DischargeDepotName))
                                    {
                                        im.DischargeDepotName = d.Depot.Name;
                                    }
                                    else
                                    {
                                        im.DischargeDepotName += ", " + d.Depot.Name;
                                    }
                                });
                            }
                            importObject.ApplicationItemObjects.Add(im);

                        }
                    });


                    var doc = (from ad in app.ApplicationDocuments
                        join d in db.Documents on ad.DocumentId equals d.DocumentId

                        select new ApplicationDocumentObject
                        {
                            DocumentTypeName = d.DocumentType.Name,
                            DateUploaded = d.DateUploaded,
                            DocumentPathStr = d.DocumentPath
                        }).ToList();
                    if (doc.Any())
                    {
                        foreach (var item in doc)
                        {
                            item.DateUploadedStr = item.DateUploaded.ToString("dd/MM/yyyy");
                            item.DocumentPathStr = item.DocumentPathStr.Replace("~", "").Replace("/", "\\");
                            importObject.ApplicationDocumentObjects.Add(item);
                        }

                    }

                    importObject.StandardRequirementObjects = new List<StandardRequirementObject>();

                    var doc2 =
                        db.StandardRequirements.Where(s => s.ImporterId == importObject.ImporterId)
                            .Include("StandardRequirementType")
                            .ToList();

                    if (doc2.Any())
                    {
                        foreach (var standardObj in doc2.Select(item => new StandardRequirementObject
                        {
                            DocumentPath = item.DocumentPath.Replace("~", "").Replace("/", "\\"),
                            DateStr = item.LastUpdated.ToString("dd/MM/yyyy"),
                            StandardRequirementTypeName = item.StandardRequirementType.Name
                        }))
                        {
                            importObject.StandardRequirementObjects.Add(standardObj);
                        }
                    }

                        importObject.ApplicationIssueObjects = new List<ApplicationIssueObject>();

                       
                        foreach (var item in issue)
                        {
                            var issueObj = new ApplicationIssueObject();
                            issueObj.IssueTypeName = item.IssueType.Name;
                            issueObj.IssueDateStr = item.IssueDate.ToString();
                            issueObj.Description = item.Description;
                            issueObj.StatusStr = Enum.GetName(typeof(IssueStatus), item.Status);

                            importObject.ApplicationIssueObjects.Add(issueObj);

                        }

                    return importObject;
                }
                    return new ApplicationObject();
            }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public long AddApplicationIssue(ApplicationIssueObject applicationIssue)
        {
            try
            {
                if (applicationIssue == null)
                {
                    return -2;
                }

                var applicationIssueEntity = ModelMapper.Map<ApplicationIssueObject, ApplicationIssue>(applicationIssue);
                if (applicationIssueEntity == null || applicationIssueEntity.ApplicationId < 1)
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.ApplicationIssues.Add(applicationIssueEntity);
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

        public long UpdateApplicationIssue(ApplicationIssueObject applicationIssue)
        {
            try
            {
                if (applicationIssue == null)
                {
                    return -2;
                }

                var applicationIssueEntity = ModelMapper.Map<ApplicationIssueObject, ApplicationIssue>(applicationIssue);
                if (applicationIssueEntity == null || applicationIssueEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.ApplicationIssues.Attach(applicationIssueEntity);
                    db.Entry(applicationIssueEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return applicationIssue.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<ApplicationIssueObject> GetApplicationIssues()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var applicationIssues = db.ApplicationIssues.ToList();
                    if (!applicationIssues.Any())
                    {
                        return new List<ApplicationIssueObject>();
                    }
                    var objList = new List<ApplicationIssueObject>();
                    applicationIssues.ForEach(app =>
                    {
                        var applicationIssueObject = ModelMapper.Map<ApplicationIssue, ApplicationIssueObject>(app);
                        if (applicationIssueObject != null && applicationIssueObject.Id > 0)
                        {
                            objList.Add(applicationIssueObject);
                        }
                    });

                    return !objList.Any() ? new List<ApplicationIssueObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<ApplicationIssueObject> GetApplicationIssues(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var applicationIssues =
                            db.ApplicationIssues.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize).Include("Application")
                                .ToList();
                        if (applicationIssues.Any())
                        {
                            var newList = new List<ApplicationIssueObject>();
                            applicationIssues.ForEach(app =>
                            {
                                var applicationIssueObject = ModelMapper.Map<ApplicationIssue, ApplicationIssueObject>(app);
                                if (applicationIssueObject != null && applicationIssueObject.Id > 0)
                                {
                                    applicationIssueObject.ReferenceCode = app.Application.Invoice.ReferenceCode;
                                    applicationIssueObject.CompanyName = app.Application.Importer.Name;
                                    applicationIssueObject.StatusStr = Enum.GetName(typeof(AppStatus), app.Status);
                                    applicationIssueObject.IssueDateStr = app.IssueDate.Value.ToString("dd/MM/yy");
                                    applicationIssueObject.IssueTypeName = app.IssueType.Name;
                               
                                    //var empProfileId = app.EmployeeDesk.EmployeeId;

                                    //var pro = db.UserProfiles.Where(a => a.Id == empProfileId).ToList();
                                    //if (pro.Any())
                                    //{
                                    //    var personId = pro[0].PersonId;
                                    //    var person = db.People.Where(p => p.Id == personId).ToList();

                                    //    if (person.Any())
                                    //    {
                                    //        applicationIssueObject.EmployeeName = person[0].FirstName + " " +
                                    //                                               person[0].LastName;
                                    //        applicationIssueObject.CompanyName = person[0].Importer.Name;
                                    //    }
                                        
                                    //}

                                    
                                    newList.Add(applicationIssueObject);
                                }
                            });
                            countG = db.ApplicationIssues.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<ApplicationIssueObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<ApplicationIssueObject>();
            }
        }


        public ApplicationIssueObject GetApplicationIssue(long applicationIssueId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var applicationIssues =
                        db.ApplicationIssues.Where(m => m.Id == applicationIssueId)
                            .ToList();
                    if (!applicationIssues.Any())
                    {
                        return new ApplicationIssueObject();
                    }

                    var app = applicationIssues[0];
                    var applicationIssueObject = ModelMapper.Map<ApplicationIssue, ApplicationIssueObject>(app);
                    if (applicationIssueObject == null || applicationIssueObject.Id < 1)
                    {
                        return new ApplicationIssueObject();
                    }

                    return applicationIssueObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationIssueObject();
            }
        }

        public List<ApplicationIssueObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var applicationIssues =
                        db.ApplicationIssues.Where(m => m.Application.Invoice.ReferenceCode.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (applicationIssues.Any())
                    {
                        var newList = new List<ApplicationIssueObject>();
                        applicationIssues.ForEach(app =>
                        {
                            var applicationIssueObject = ModelMapper.Map<ApplicationIssue, ApplicationIssueObject>(app);
                            if (applicationIssueObject != null && applicationIssueObject.Id > 0)
                            {
                                newList.Add(applicationIssueObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<ApplicationIssueObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ApplicationIssueObject>();
            }
        }

        public long DeleteApplicationIssue(long applicationIssueId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.ApplicationIssues.Where(m => m.Id == applicationIssueId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.ApplicationIssues.Remove(item);
                    db.SaveChanges();
                    return 5;

                    //var applicationIssue = db.ApplicationIssuees.Find(applicationIssueId);
                    //db.ApplicationIssuees.Remove(applicationIssue);
                    //db.SaveChanges();
                    //return 5;
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
