using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
    public class RecertificationManager
    {
        public long AddRecertification(RecertificationObject recertification)
        {
            try
            {
                if (recertification == null)
                {
                    return -2;
                }

              

                using (var db = new ImportPermitEntities())
                {
                    var not = db.Notifications.Where(n => n.Id == recertification.Id).ToList();
                    var recert = new Recertification();

                    if (not.Any())
                    {
                        recert.NotificationId = not[0].Id;
                        recert.DateApplied = DateTime.Now;
                        recert.Status = (int)RecertificationStatusEnum.Processing;
                        recert.ReferenceCode = not[0].Invoice.ReferenceCode;
                        recert.LastModified = DateTime.Now;
                        db.Recertifications.Add(recert);
                        //change the status of the notification
                        not[0].Status = (int)NotificationStatusEnum.Recertifying;
                        db.Notifications.Attach(not[0]);
                        db.Entry(not[0]).State = EntityState.Modified;
                         db.SaveChanges();
                        //add to process tracking
                        AssignRecertificationToEmployee(recert.Id);
                        return 1;
                    }

                    return 0;
                    
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }


        private bool AssignRecertificationToEmployee(long recertificationId)
        {
            try
            {

                using (var db = new ImportPermitEntities())
                {

                    var recertification = db.Recertifications.Find(recertificationId);
                    //make sure it is paid for

                    const int appStage = (int)AppStage.Recertification;

                    //get the step with a sequence of one in application stage
                    var firstStep = db.Steps.Where(s => s.SequenceNumber == 1 && s.Process.ImportStageId == appStage).ToList();

                    if (firstStep.Any())
                    {
                        //get the group the step belong to
                        //var group = db.Groups.Find(firstStep[0].GroupId);

                        var groupId = firstStep[0].GroupId;
                        var activityTypeId = firstStep[0].ActivityTypeId;
                        var stepId = firstStep[0].Id;
                        //get the employees in that group
                        var emp = (from e in db.EmployeeDesks

                                   where e.GroupId.Equals(groupId) && e.ActivityTypeId.Equals(activityTypeId)
                                   orderby e.JobCount ascending
                                   select new EmployeeStepObject
                                   {
                                       EmployeeDeskId = e.Id,

                                       JobCount = e.JobCount

                                   }).ToList().First();


                        if (emp != null)
                        {
                            var track = new RecertificationProcess();
                            track.RecertificationId = recertificationId;
                            track.EmployeeId = emp.EmployeeDeskId;
                            track.StepId = stepId;
                            track.AssignedTime = DateTime.Now;
                            track.StepCode = 1;
                            track.StatusId = (int)RecertificationStatusEnum.Processing;

                            db.RecertificationProcesses.Add(track);

                            //update employee job count

                            var empId = emp.EmployeeDeskId;

                            var employeeDesk = db.EmployeeDesks.Find(empId);

                            employeeDesk.JobCount = employeeDesk.JobCount + 1;
                            employeeDesk.RecertificationCount = employeeDesk.RecertificationCount + 1;

                            db.EmployeeDesks.Attach(employeeDesk);
                            db.Entry(employeeDesk).State = EntityState.Modified;

                           
                            db.SaveChanges();

                            return true;
                        }


                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return false;
            }
        }



        public long UpdateRecertification(RecertificationObject recertification)
        {
            try
            {
                if (recertification == null)
                {
                    return -2;
                }

                var recertificationEntity = ModelMapper.Map<RecertificationObject, Recertification>(recertification);
                if (recertificationEntity == null || recertificationEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.Recertifications.Attach(recertificationEntity);
                    db.Entry(recertificationEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return recertification.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

   
        //public List<RecertificationObject> GetLocalRecertifications()
        //{
        //    try
        //    {
        //        using (var db = new ImportPermitEntities())
        //        {
        //            const int nigeria = (int) CountryEnum.Nigeria;

        //            var recertifications = db.Recertifications.Where(c => c.Jetty.Port.Country.Name == "nigeria").ToList();

        //            if (!recertifications.Any())
        //            {
        //                return new List<RecertificationObject>();
        //            }

        //            var objList = new List<RecertificationObject>();

        //            recertifications.ForEach(app =>
        //            {
                      
                       
        //                    var recertificationObject = ModelMapper.Map<Recertification, RecertificationObject>(app);
        //                    if (recertificationObject != null && recertificationObject.Id > 0)
        //                    {
        //                       objList.Add(recertificationObject);
        //                    }
                        
        //           });

        //            return !objList.Any() ? new List<RecertificationObject>() : objList;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return null;
        //    }
        //}
        public List<RecertificationObject> GetRecertifications()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var objList = new List<RecertificationObject>();
                    

                    var recertifications = db.Recertifications.ToList();

                    foreach (var item in recertifications)
                    {
                        var recertificationObj = new RecertificationObject();
                        recertificationObj.ReferenceCode = item.ReferenceCode;
                        recertificationObj.Id = item.Id;
                        recertificationObj.StatusStr = Enum.GetName(typeof(RecertificationStatusEnum), item.Status);

                        objList.Add(recertificationObj);
                    }

                    return objList;

                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RecertificationObject>();
            }
        }
        public List<RecertificationObject> GetRecertifications(long userProfileId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var objList = new List<RecertificationObject>();
                    //get the inspector's location
                    int zoneId;
                    var employee = db.EmployeeDesks.Where(e => e.EmployeeId == userProfileId).ToList();
                    if (employee.Any())
                    {
                        zoneId = employee[0].ZoneId;
                        //get the jettymapping in the zone
                        var jettymappings = db.JettyMappings.Where(j => j.ZoneId == zoneId).ToList();

                       
                        return objList;
                    }

                    return new List<RecertificationObject>(); 
              
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
        public List<RecertificationObject> GetRecertifications(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var recertifications =
                            db.Recertifications.Where(r=>r.Notification.ImporterId == importerId)
                            .OrderByDescending(m => m.Id)
                             .Include("Notification")
                             .Skip(tpageNumber).Take(tsize)
                             .Take(tsize)
                             .ToList();
                        if (recertifications.Any())
                        {
                            var newList = new List<RecertificationObject>();
                            recertifications.ForEach(app =>
                            {
                                var recertificationObject = ModelMapper.Map<Recertification, RecertificationObject>(app);
                                if (recertificationObject != null && recertificationObject.Id > 0)
                                {
                                    recertificationObject.StatusStr = Enum.GetName(typeof(RecertificationStatusEnum), app.Status);
                                    
                                    newList.Add(recertificationObject);
                                }
                            });
                            countG = db.Recertifications.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<RecertificationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<RecertificationObject>();
            }
        }

        public List<RecertificationObject> GetAdminRecertifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var recertifications =
                            db.Recertifications
                            .OrderByDescending(m => m.Id)
                             .Include("Notification")
                             .Skip(tpageNumber).Take(tsize)
                             .Take(tsize)
                             .ToList();
                        if (recertifications.Any())
                        {
                            var newList = new List<RecertificationObject>();
                            recertifications.ForEach(app =>
                            {
                                var recertificationObject = ModelMapper.Map<Recertification, RecertificationObject>(app);
                                if (recertificationObject != null && recertificationObject.Id > 0)
                                {
                                    recertificationObject.StatusStr = Enum.GetName(typeof(RecertificationStatusEnum), app.Status);

                                    newList.Add(recertificationObject);
                                }
                            });
                            countG = db.Recertifications.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<RecertificationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<RecertificationObject>();
            }
        }

        public List<RecertificationObject> GetUserRecertifications(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var recertifications =
                           db.Recertifications.Where(r => r.Notification.ImporterId == importerId)
                            .OrderByDescending(m => m.Id)
                             .Include("Notification")
                             .Skip(tpageNumber).Take(tsize)
                             .Take(tsize)
                             .ToList();
                        if (recertifications.Any())
                        {
                            var newList = new List<RecertificationObject>();
                            recertifications.ForEach(app =>
                            {
                                var recertificationObject = ModelMapper.Map<Recertification, RecertificationObject>(app);
                                if (recertificationObject != null && recertificationObject.Id > 0)
                                {
                                    recertificationObject.StatusStr = Enum.GetName(typeof(RecertificationStatusEnum), app.Status);

                                    newList.Add(recertificationObject);
                                }
                            });
                            countG = db.Recertifications.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<RecertificationObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<RecertificationObject>();
            }
        }

        public RecertificationObject GetRecertification(long recertificationId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var recertifications =
                        db.Recertifications.Where(m => m.Id == recertificationId)
                            .ToList();
                    if (!recertifications.Any())
                    {
                        return new RecertificationObject();
                    }

                    var app = recertifications[0];
                    var recertificationObject = ModelMapper.Map<Recertification, RecertificationObject>(app);
                    if (recertificationObject == null || recertificationObject.Id < 1)
                    {
                        return new RecertificationObject();
                    }

                    recertificationObject.Company = app.Notification.Importer.Name;
                    recertificationObject.DateStr = app.DateApplied.ToString();
                    recertificationObject.StatusStr = Enum.GetName(typeof(RecertificationStatusEnum), app.Status);


                    var notDocObjs = new List<NotificationDocumentObject>();
                    var doc = db.NotificationDocuments.Where(o => o.NotificationId == app.NotificationId).ToList();

                    foreach (var item in doc)
                    {
                        var normalDoc = db.Documents.Where(m => m.DocumentId == item.DocumentId).ToList();
                        if (normalDoc.Any())
                        {

                            var notDocObj = new NotificationDocumentObject();
                            notDocObj.DocumentId = normalDoc[0].DocumentId;
                            notDocObj.DateUploadedStr = normalDoc[0].DateUploaded.ToString("dd/MM/yyyy");
                            notDocObj.DocumentTypeName = normalDoc[0].DocumentType.Name;
                            notDocObj.DocumentPathStr = normalDoc[0].DocumentPath.Replace("~", "").Replace("/", "\\");
                            notDocObjs.Add(notDocObj);
                        }
                    }

                    recertificationObject.NotificationDocumentObjects = notDocObjs;
             
                    return recertificationObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new RecertificationObject();
            }
        }

        public List<DocumentObject> GetRecertificationDocs(long recertificationId)
        {
            try
            {
                var docs = new List<DocumentObject>();

                using (var db = new ImportPermitEntities())
                {
                    var recertifications =
                        db.Recertifications.Where(m => m.Id == recertificationId)
                            .ToList();
                    if (recertifications.Any())
                    {
                          //get the importer Id
                        var importerId = recertifications[0].Notification.ImporterId;

                        //get the required docs
                        var requiredDocs =
                            db.ImportRequirements.Where(r => r.ImportStageId == (int) AppStage.Recertification).ToList();
                        if (requiredDocs.Any())
                        {
                            foreach (var item in requiredDocs)
                            {
                                var doc = new DocumentObject();
                                var docEntity =
                                    db.Documents.Where(
                                        d => d.DocumentTypeId == item.DocumentTypeId && d.ImporterId == importerId)
                                        .ToList();
                                if (docEntity.Any())
                                {
                                    doc.DocumentTypeName = docEntity[0].DocumentType.Name;
                                    doc.DateUploadedStr = docEntity[0].DateUploaded.ToString("dd/MM/yyyy");
                                    doc.DocumentPath = docEntity[0].DocumentPath;

                                    docs.Add(doc);
                                }
                                
                            }

                            return docs;
                        }
                        return new List<DocumentObject>();
                    }
                  
                   return new List<DocumentObject>();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentObject>();
            }
        }


        public List<RecertificationObject> Search(string searchCriteria, long id)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var recertifications =
                        db.Recertifications
                        .Include("Jetty")

                        .Where(m => m.Notification.ImporterId == id && (m.ReferenceCode.ToLower() == searchCriteria.ToLower().Trim()
                        || m.DateApplied.ToString().ToLower() == searchCriteria.ToLower().Trim()))
                        .ToList();
                    if (!recertifications.Any())
                    {
                        return new List<RecertificationObject>();
                    }
                    var newList = new List<RecertificationObject>();
                    recertifications.ForEach(app =>
                    {
                        var recertificationObject = ModelMapper.Map<Recertification, RecertificationObject>(app);
                        if (recertificationObject != null && recertificationObject.Id > 0)
                        {
                          
                            newList.Add(recertificationObject);
                        }
                    });
                    return newList;
                }
            }
            catch (Exception ex)
            {
                return new List<RecertificationObject>();
            }
        }

        public List<RecertificationObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var recertifications =
                        db.Recertifications
                        .Include("Jetty")

                        .Where(m => m.ReferenceCode.ToLower() == searchCriteria.ToLower().Trim()
                        || m.DateApplied.ToString().ToLower() == searchCriteria.ToLower().Trim())
                        .ToList();
                    if (!recertifications.Any())
                    {
                        return new List<RecertificationObject>();
                    }
                    var newList = new List<RecertificationObject>();
                    recertifications.ForEach(app =>
                    {
                        var recertificationObject = ModelMapper.Map<Recertification, RecertificationObject>(app);
                        if (recertificationObject != null && recertificationObject.Id > 0)
                        {

                            newList.Add(recertificationObject);
                        }
                    });
                    return newList;
                }
            }
            catch (Exception ex)
            {
                return new List<RecertificationObject>();
            }
        }
        public long DeleteRecertification(long recertificationId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.Recertifications.Where(m => m.Id == recertificationId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.Recertifications.Remove(item);
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
