using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using ImportPermitPortal.BizObjects;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.EF.Model;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;
using Microsoft.AspNet.Identity;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using WebGrease.Css.Extensions;
using System.Security.AccessControl;
using System.Security.Principal;
using Mandrill;
using Mandrill.Model;
using System.Threading.Tasks;


namespace ImportPermitPortal.Controllers
{
    [Authorize(Roles = "Verifier,Employee,DownstreamDirector,Applicant,Super_Admin,Support")]
    public class EmployeeProfileController : Controller
    {

        [HttpGet]
        public ActionResult DirectEmployee()
        {
            try
            {
                //get the id of logged in user
                var userId = User.Identity.GetUserId();

                using (var db = new ImportPermitEntities())
                {

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(userId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                    if (employeeDesk.Any())
                    {
                        var employeeId = employeeDesk[0].Id;

                        var appTrack = db.ProcessTrackings.Where(p => p.EmployeeId == employeeId);
                        var noteTrack = db.NotificationInspectionQueues.Where(n => n.EmployeeId == employeeId);

                        if (appTrack.Any())
                        {
                            return Redirect("http://ppips2.shopkeeper.ng/nge.html#EmployeeProfile/EmployeeProfile");
                        }
                        else if (noteTrack.Any())
                        {
                            return
                                Redirect(
                                    "http://ppips2.shopkeeper.ng/nge.html#EmployeeNotificationTrack/EmployeeNotificationTrack");

                        }
                    }
                    return Redirect("http://ppips2.shopkeeper.ng/nge.html#DirectEmployee/DirectEmployee");
                }

            }
            catch (Exception)
            {
                return Redirect("http://ppips2.shopkeeper.ng/nge.html#DirectEmployee/DirectEmployee");

            }
        }
        public ActionResult Dashboard()
        {
            var rep = new Reporter();
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    //get the id of logged in user
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    var emp = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();
                    if (emp.Any())
                    {
                        rep.ApplicationCount = emp[0].ApplicationCount != null ? (int)emp[0].ApplicationCount : 0;
                        rep.NotificationCount = emp[0].NotificationCount != null? (int)emp[0].NotificationCount : 0 ;
                        rep.RecertificationCount = emp[0].RecertificationCount != null ? (int)emp[0].RecertificationCount : 0;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }
                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetProcessTrackingObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ProcessTrackingObject> filteredParentMenuObjects;
                var countG = 0;

                //get the id of logged in user
                var userId = User.Identity.GetUserId();


                var pagedParentMenuObjects = GetEmployeeProfiles(param.iDisplayLength, param.iDisplayStart, out countG,
                    userId);


                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ProcessTrackingServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ProcessTrackingObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ProcessTrackingObject, string> orderingFunction =
                    (c => sortColumnIndex == 1 ? c.ReferenceCode : c.DueTimeStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "asc"
                    ? filteredParentMenuObjects.OrderBy(orderingFunction)
                    : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

              
                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.ReferenceCode, c.CompanyName, c.AssignedTimeStr, c.DueTimeStr };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = countG,
                    iTotalDisplayRecords = countG,
                    aaData = result
                },
                    JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<ProcessTrackingObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetEmployeeNotificationObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<NotificationInspectionQueueObject> filteredParentMenuObjects;
                var countG = 0;

                //get the id of logged in user
                var userId = User.Identity.GetUserId();


                var pagedParentMenuObjects = GetNotificationTrackProfiles(param.iDisplayLength, param.iDisplayStart,
                    out countG,
                    userId);


                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new NotificationInspectionQueueServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<NotificationInspectionQueueObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<NotificationInspectionQueueObject, string> orderingFunction =
                    (c => sortColumnIndex == 1 ? c.ReferenceCode : c.AssignedTimeStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "asc"
                    ? filteredParentMenuObjects.OrderBy(orderingFunction)
                    : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;


                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.ReferenceCode, c.CompanyName, c.AssignedTimeStr, c.DueTimeStr };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = countG,
                    iTotalDisplayRecords = countG,
                    aaData = result
                },
                    JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<NotificationInspectionQueueObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetEmployeeRecertificationObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<RecertificationProcessObject> filteredParentMenuObjects;
                var countG = 0;

                //get the id of logged in user
                var userId = User.Identity.GetUserId();


                var pagedParentMenuObjects = GetRecertificationTrackProfiles(param.iDisplayLength, param.iDisplayStart,
                     out countG,
                     userId);


                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new RecertificationProcessServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<RecertificationProcessObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<RecertificationProcessObject, string> orderingFunction =
                    (c => sortColumnIndex == 1 ? c.ReferenceCode : c.AssignedTimeStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "asc"
                    ? filteredParentMenuObjects.OrderBy(orderingFunction)
                    : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;


                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.ReferenceCode, c.CompanyName, c.AssignedTimeStr, c.DueTimeStr };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = countG,
                    iTotalDisplayRecords = countG,
                    aaData = result
                },
                    JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<RecertificationProcessObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetPreviousJobs(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ProcessingHistoryObject> filteredParentMenuObjects;
                var countG = 0;

                //get the id of logged in user
                var userId = User.Identity.GetUserId();


                var pagedParentMenuObjects = GetPreviousJobsProfiles(param.iDisplayLength, param.iDisplayStart,
                    out countG,
                    userId);


                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ProcessingHistoryServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ProcessingHistoryObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ProcessingHistoryObject, string> orderingFunction =
                    (c => sortColumnIndex == 1 ? c.ReferenceCode : c.AssignedTimeStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "asc"
                    ? filteredParentMenuObjects.OrderBy(orderingFunction)
                    : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[]
                    {
                        Convert.ToString(c.Id),
                        c.ReferenceCode, c.AssignedTimeStr,
                        c.DueTimeStr
                    };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = countG,
                    iTotalDisplayRecords = countG,
                    aaData = result
                },
                    JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<ProcessingHistoryObject>(), JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult AddIssue(EmployeeProfileObject employeeProfile)
        {
            var rep = new Reporter();
            try
            {


                if (employeeProfile == null || employeeProfile.ApplicationId < 1)
                {
                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }



                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                    long employeeId;

                    if (employeeDesk.Any())
                    {
                        employeeId = employeeDesk[0].Id;


                        //update the tracking stepcode to enable us get the next step
                        var track =
                            db.ProcessTrackings.Where(
                                p =>
                                    p.ApplicationId == employeeProfile.ApplicationId &&
                                    p.EmployeeId == employeeId).ToList();

                        if (track.Any())
                        {
                            var importapplication = db.Applications.Find(employeeProfile.ApplicationId);
                            importapplication.ApplicationStatusCode = (int)AppStatus.Rejected;
                            track[0].StepCode = -1;
                            track[0].ActualDeliveryDateTime = DateTime.Now;

                            track[0].StatusId = (int)AppStatus.Rejected;



                            //add a history of work done 
                            ProcessingHistory prohis = new ProcessingHistory();
                            prohis.ApplicationId = importapplication.Id;
                            prohis.AssignedTime = track[0].AssignedTime;
                            prohis.FinishedTime = DateTime.Now;
                            prohis.EmployeeId = employeeId;
                            prohis.StepId = track[0].StepId;
                            prohis.OutComeCode = (int)AppStatus.Rejected;
                            prohis.Remarks = employeeProfile.Description;


                            db.ProcessingHistories.Add(prohis);


                            db.Entry(importapplication).State = EntityState.Modified;
                            db.Entry(track[0]).State = EntityState.Modified;


                            var issue = new ApplicationIssue();
                            issue.ApplicationId = employeeProfile.ApplicationId;
                            issue.IssueTypeId = employeeProfile.IssueTypeId;
                            issue.Description = employeeProfile.Description;
                            issue.Status = (int)IssueStatus.Open;
                            issue.IssueDate = DateTime.Now;
                            db.ApplicationIssues.Add(issue);
                            db.SaveChanges();
                            rep.IsAccepted = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }
                        rep.IsError = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }
                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddRecertificationIssue(EmployeeProfileObject employeeProfile)
        {
            var rep = new Reporter();
            try
            {


                if (employeeProfile == null || employeeProfile.ApplicationId < 1)
                {
                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }



                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId.Equals(profileId)).ToList();

                    long employeeId;

                    if (employeeDesk.Any())
                    {
                        employeeId = employeeDesk[0].Id;


                        //update the tracking stepcode to enable us get the next step
                        var track =
                            db.RecertificationProcesses.Where(
                                p =>
                                    p.RecertificationId == employeeProfile.ApplicationId &&
                                    p.EmployeeId == employeeId).ToList();

                        if (track.Any())
                        {
                            var recertification = db.Recertifications.Find(employeeProfile.ApplicationId);
                            recertification.Status = (int)RecertificationStatusEnum.Rejected;
                            track[0].StepCode = -1;
                            track[0].ActualDeliveryDateTime = DateTime.Now;

                            track[0].StatusId = (int)RecertificationStatusEnum.Rejected;



                            //add a history of work done 
                            var prohis = new RecertificationHistory();
                            prohis.RecertificationId = recertification.Id;
                            prohis.AssignedTime = track[0].AssignedTime;
                            prohis.FinishedTime = DateTime.Now;
                            prohis.EmployeeId = employeeId;
                            prohis.StepId = track[0].StepId;
                            prohis.OutComeCode = (int)RecertificationStatusEnum.Rejected;
                            prohis.Remarks = employeeProfile.Description;


                            db.RecertificationHistories.Add(prohis);


                            db.Entry(recertification).State = EntityState.Modified;
                            db.Entry(track[0]).State = EntityState.Modified;


                            var issue = new RecertificationIssue();
                            issue.RecertificationId = employeeProfile.ApplicationId;
                            issue.IssueTypeId = employeeProfile.IssueTypeId;
                            issue.Description = employeeProfile.Description;
                            issue.Status = (int)IssueStatus.Open;
                            issue.IssueDate = DateTime.Now;
                            db.RecertificationIssues.Add(issue);
                            db.SaveChanges();
                            rep.IsAccepted = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }
                        rep.IsError = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }
                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddNotificationIssue(EmployeeProfileObject employeeProfile)
        {
            var rep = new Reporter();
            try
            {

                if (employeeProfile == null || employeeProfile.ApplicationId < 1)
                {
                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }



                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                    long employeeId;

                    if (employeeDesk.Any())
                    {
                        employeeId = employeeDesk[0].Id;


                        //update the tracking stepcode to enable us get the next step
                        var track =
                            db.NotificationInspectionQueues.Where(
                                p =>
                                    p.NotificationId == employeeProfile.ApplicationId &&
                                    p.EmployeeId == employeeId).ToList();

                        if (track.Any())
                        {
                            var notification = db.Notifications.Find(employeeProfile.ApplicationId);
                            notification.Status = (int)NotificationStatusEnum.Rejected;
                            track[0].StepCode = -1;
                            track[0].ActualDeliveryDateTime = DateTime.Now;

                            track[0].OutComeCode = (int)NotificationStatusEnum.Rejected;



                            //add a history of work done 
                            var prohis = new NotificationHistory();
                            prohis.NotificationId = notification.Id;
                            prohis.AssignedTime = track[0].AssignedTime;
                            prohis.FinishedTime = DateTime.Now;
                            prohis.EmployeeId = employeeId;
                            prohis.StepId = track[0].StepId;
                            prohis.OutComeCode = (int)NotificationStatusEnum.Rejected;
                            prohis.Remarks = employeeProfile.Description;


                            db.NotificationHistories.Add(prohis);


                            db.Entry(notification).State = EntityState.Modified;
                            db.Entry(track[0]).State = EntityState.Modified;


                            var issue = new NotificationIssue();
                            issue.NotificationId = employeeProfile.ApplicationId;
                            issue.IssueTypeId = employeeProfile.IssueTypeId;
                            issue.Description = employeeProfile.Description;
                            issue.Status = (int)IssueStatus.Open;
                            issue.IssueDate = DateTime.Now;
                            db.NotificationIssues.Add(issue);
                            db.SaveChanges();
                            rep.IsAccepted = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }
                        rep.IsError = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }
                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        public Reporter AcceptNotification(long id)
        {
            var rep = new Reporter();
            try
            {

                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                    long employeeId;

                    if (employeeDesk.Any())
                    {
                        employeeId = employeeDesk[0].Id;


                        var notification = db.Notifications.Where(n => n.Id == id).ToList();


                        if (notification.Any())
                        {

                            var notId = notification[0].Id;

                            //update the tracking stepcode to enable us get the next step
                            var track =
                                db.NotificationInspectionQueues.Where(
                                    p => p.NotificationId == notId && p.EmployeeId == employeeId).ToList();

                            if (track.Any())
                            {

                                //get the sequence of the current step

                                var stepnow = db.Steps.Find(track[0].StepId);

                                var nextStepSequence = stepnow.SequenceNumber + 1;

                                //find out if this is the last step
                                var stepnext = db.Steps.Where(s => s.SequenceNumber == nextStepSequence && s.Process.ImportStageId == (int)AppStage.Notification).ToList();

                                if (stepnext.Any())
                                {
                                    var fsx = (int)nextStepSequence;

                                    //call the assign method
                                    var res = new WorkFlowServices().AssignNotificationToEmployeeInTrack(id, fsx,
                                        track[0].Id);

                                    if (res)
                                    {

                                        //add a history of work done 
                                        var prohis = new NotificationHistory();
                                        prohis.NotificationId = id;
                                        prohis.AssignedTime = track[0].AssignedTime;
                                        prohis.EmployeeId = employeeId;
                                        prohis.FinishedTime = DateTime.Now;
                                        prohis.StepId = track[0].StepId;
                                        prohis.Remarks = "done";
                                        prohis.OutComeCode = (int)NotificationStatusEnum.Processing;

                                        db.NotificationHistories.Add(prohis);

                                        //update employee job count

                                        employeeDesk[0].JobCount = employeeDesk[0].JobCount - 1;

                                        var employee = employeeDesk[0];

                                        db.EmployeeDesks.Attach(employee);
                                        db.Entry(employee).State = EntityState.Modified;

                                        db.SaveChanges();


                                        rep.IsAccepted = true;
                                        //return Json(rep, JsonRequestBehavior.AllowGet);
                                        return rep;

                                    }

                                }


                                else
                                {
                                    //generate the document
                                    //update employee job count

                                    employeeDesk[0].JobCount = employeeDesk[0].JobCount - 1;

                                    var employee = employeeDesk[0];

                                    db.EmployeeDesks.Attach(employee);
                                    db.Entry(employee).State = EntityState.Modified;

                                    track[0].StepCode = 0;
                                    track[0].OutComeCode = (int)NotificationStatusEnum.Approved;


                                    db.NotificationInspectionQueues.Attach(track[0]);
                                    db.Entry(track[0]).State = EntityState.Modified;

                                    //change application status
                                    notification[0].Status = (int)NotificationStatusEnum.Approved;
                                    db.Notifications.Attach(notification[0]);
                                    db.Entry(notification[0]).State = EntityState.Modified;


                                    db.SaveChanges();

                                    rep.IsCertificateGenerated = true;
                                    //return Json(rep, JsonRequestBehavior.AllowGet);
                                    return rep;
                                }



                                rep.IsNull = true;
                                //return Json(rep, JsonRequestBehavior.AllowGet);
                                return rep;
                            }

                            rep.IsError = true;
                            //return Json(rep, JsonRequestBehavior.AllowGet);
                            return rep;
                        }
                        rep.IsError = true;
                        //return Json(rep, JsonRequestBehavior.AllowGet);
                        return rep;
                    }
                    rep.NoEmployee = true;
                    //return Json(rep, JsonRequestBehavior.AllowGet);
                    return rep;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                //return Json(rep, JsonRequestBehavior.AllowGet);
                return rep;
            }
        }


        [HttpPost]
        public ActionResult ProcessDialogReview(EmployeeProfileObject employeeProfile)
        {
            var rep = new Reporter();
            try
            {


                if (employeeProfile == null)
                {
                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }


                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                    long employeeId;

                    if (employeeDesk.Any())
                    {
                        employeeId = employeeDesk[0].Id;


                        var doc = db.NotificationDocuments.Where(o => o.NotificationId == employeeProfile.ApplicationId).ToList();

                        if (doc.Any())
                        {

                            foreach (var item in doc)
                            {
                                var normalDoc = db.Documents.Where(m => m.DocumentId == item.DocumentId).ToList();
                                if (normalDoc.Any())
                                {

                                    if (normalDoc[0].IsValid == "False" || normalDoc[0].IsValid == null)
                                    {
                                        rep.IsValid = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }


                                }
                            }
                        }


                        var notification =
                            db.Notifications.Where(n => n.Id == employeeProfile.ApplicationId).ToList();



                        if (notification.Any())
                        {

                            var notId = notification[0].Id;
                          
                            //update the tracking stepcode to enable us get the next step
                            var track =
                                db.NotificationInspectionQueues.Where(
                                    p => p.NotificationId == notId && p.EmployeeId == employeeId).ToList();

                            if (track.Any())
                            {




                                //get the sequence of the current step

                                var stepnow = db.Steps.Find(track[0].StepId);

                                var nextStepSequence = stepnow.SequenceNumber + 1;

                                //find out if this is the last step
                                var stepnext = db.Steps.Where(s => s.SequenceNumber == nextStepSequence && s.Process.ImportStageId == (int)AppStage.Notification).ToList();

                                if (stepnext.Any())
                                {
                                    var fsx = (int)nextStepSequence;

                                    //call the assign method
                                    var res =
                                        new WorkFlowServices().AssignNotificationToEmployeeInTrack(
                                            employeeProfile.ApplicationId, fsx, track[0].Id);

                                    if (res)
                                    {

                                        //add a history of work done 
                                        var prohis = new NotificationHistory();
                                        prohis.NotificationId = employeeProfile.ApplicationId;
                                        prohis.AssignedTime = track[0].AssignedTime;
                                        prohis.EmployeeId = employeeId;
                                        prohis.FinishedTime = DateTime.Now;
                                        prohis.StepId = track[0].StepId;
                                        prohis.OutComeCode = (int)NotificationStatusEnum.Processing;
                                        prohis.Remarks = employeeProfile.Description;

                                        db.NotificationHistories.Add(prohis);

                                        //update employee job count

                                        employeeDesk[0].JobCount = employeeDesk[0].JobCount - 1;
                                        employeeDesk[0].NotificationCount = employeeDesk[0].NotificationCount - 1;

                                        var employee = employeeDesk[0];

                                        db.EmployeeDesks.Attach(employee);
                                        db.Entry(employee).State = EntityState.Modified;

                                        db.SaveChanges();


                                        rep.IsAccepted = true;
                                        return Json(rep, JsonRequestBehavior.AllowGet);


                                    }
                                    rep.NoEmployee = true;
                                    return Json(rep, JsonRequestBehavior.AllowGet);

                                }


                                if (!stepnext.Any())
                                {
                                    //generate the document
                                    //update employee job count

                                    employeeDesk[0].JobCount = employeeDesk[0].JobCount - 1;
                                    employeeDesk[0].NotificationCount = employeeDesk[0].NotificationCount - 1;

                                    var employee = employeeDesk[0];

                                    db.EmployeeDesks.Attach(employee);
                                    db.Entry(employee).State = EntityState.Modified;

                                    track[0].StepCode = 0;
                                    track[0].StatusId = (int)NotificationStatusEnum.Approved;



                                    db.NotificationInspectionQueues.Attach(track[0]);
                                    db.Entry(track[0]).State = EntityState.Modified;

                                    
                                    //change application status
                                    notification[0].Status = (int)NotificationStatusEnum.Approved;
                                    db.Notifications.Attach(notification[0]);
                                    db.Entry(notification[0]).State = EntityState.Modified;


                                 

                                    //add a history of work done 
                                    var prohis = new NotificationHistory();
                                    prohis.NotificationId = employeeProfile.ApplicationId;
                                    prohis.AssignedTime = track[0].AssignedTime;
                                    prohis.EmployeeId = employeeId;
                                    prohis.FinishedTime = DateTime.Now;
                                    prohis.StepId = track[0].StepId;
                                    prohis.OutComeCode = (int)NotificationStatusEnum.Processing;
                                    prohis.Remarks = employeeProfile.Description;

                                    db.NotificationHistories.Add(prohis);

                                    db.SaveChanges();

                                    rep.IsCertificateGenerated = true;
                                    return Json(rep, JsonRequestBehavior.AllowGet);

                                }



                                rep.IsNull = true;
                                return Json(rep, JsonRequestBehavior.AllowGet);

                            }

                            rep.IsError = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);

                        }
                        rep.IsError = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);

                    }
                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);

            }
        }


        [HttpPost]
        public ActionResult ProcessNotificationDialogApprove(EmployeeProfileObject employeeProfile)
        {
            var rep = new Reporter();
            try
            {


                if (employeeProfile == null)
                {
                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }


                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                    long employeeId;

                    if (employeeDesk.Any())
                    {
                        employeeId = employeeDesk[0].Id;


                        var doc = db.NotificationDocuments.Where(o => o.NotificationId == employeeProfile.ApplicationId).ToList();

                        if (doc.Any())
                        {

                            foreach (var item in doc)
                            {
                                var normalDoc = db.Documents.Where(m => m.DocumentId == item.DocumentId).ToList();
                                if (normalDoc.Any())
                                {

                                    if (normalDoc[0].IsValid == "False" || normalDoc[0].IsValid == null)
                                    {
                                        rep.IsValid = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }


                                }
                            }
                        }


                        var notification =
                            db.Notifications.Where(n => n.Id == employeeProfile.ApplicationId).ToList();



                        if (notification.Any())
                        {

                            var notId = notification[0].Id;

                            //update the tracking stepcode to enable us get the next step
                            var track =
                                db.NotificationInspectionQueues.Where(
                                    p => p.NotificationId == notId && p.EmployeeId == employeeId).ToList();

                            if (track.Any())
                            {




                                //get the sequence of the current step

                                var stepnow = db.Steps.Find(track[0].StepId);

                                var nextStepSequence = stepnow.SequenceNumber + 1;

                                //find out if this is the last step
                                var stepnext = db.Steps.Where(s => s.SequenceNumber == nextStepSequence && s.Process.ImportStageId == (int)AppStage.Notification).ToList();

                                if (stepnext.Any())
                                {
                                    var fsx = (int)nextStepSequence;

                                    //call the assign method
                                    var res =
                                        new WorkFlowServices().AssignNotificationToEmployeeInTrack(
                                            employeeProfile.ApplicationId, fsx, track[0].Id);

                                    if (res)
                                    {

                                        //add a history of work done 
                                        var prohis = new NotificationHistory();
                                        prohis.NotificationId = employeeProfile.ApplicationId;
                                        prohis.AssignedTime = track[0].AssignedTime;
                                        prohis.EmployeeId = employeeId;
                                        prohis.FinishedTime = DateTime.Now;
                                        prohis.StepId = track[0].StepId;
                                        prohis.OutComeCode = (int)NotificationStatusEnum.Processing;
                                        prohis.Remarks = employeeProfile.Description;

                                        db.NotificationHistories.Add(prohis);

                                        //update employee job count

                                        employeeDesk[0].JobCount = employeeDesk[0].JobCount - 1;
                                        employeeDesk[0].NotificationCount = employeeDesk[0].NotificationCount - 1;

                                        var employee = employeeDesk[0];

                                        db.EmployeeDesks.Attach(employee);
                                        db.Entry(employee).State = EntityState.Modified;

                                        db.SaveChanges();


                                        rep.IsAccepted = true;
                                        return Json(rep, JsonRequestBehavior.AllowGet);


                                    }
                                    rep.NoEmployee = true;
                                    return Json(rep, JsonRequestBehavior.AllowGet);

                                }


                                if (!stepnext.Any())
                                {
                                    //generate the document
                                    //update employee job count

                                    employeeDesk[0].JobCount = employeeDesk[0].JobCount - 1;
                                    employeeDesk[0].NotificationCount = employeeDesk[0].NotificationCount - 1;

                                    var employee = employeeDesk[0];

                                    db.EmployeeDesks.Attach(employee);
                                    db.Entry(employee).State = EntityState.Modified;

                                    track[0].StepCode = 0;
                                    track[0].StatusId = (int)NotificationStatusEnum.Approved;



                                    db.NotificationInspectionQueues.Attach(track[0]);
                                    db.Entry(track[0]).State = EntityState.Modified;


                                    //change application status
                                    notification[0].Status = (int)NotificationStatusEnum.Approved;
                                    db.Notifications.Attach(notification[0]);
                                    db.Entry(notification[0]).State = EntityState.Modified;




                                    //add a history of work done 
                                    var prohis = new NotificationHistory();
                                    prohis.NotificationId = employeeProfile.ApplicationId;
                                    prohis.AssignedTime = track[0].AssignedTime;
                                    prohis.EmployeeId = employeeId;
                                    prohis.FinishedTime = DateTime.Now;
                                    prohis.StepId = track[0].StepId;
                                    prohis.OutComeCode = (int)NotificationStatusEnum.Processing;
                                    prohis.Remarks = employeeProfile.Description;

                                    db.NotificationHistories.Add(prohis);

                                    db.SaveChanges();

                                    rep.IsCertificateGenerated = true;
                                    return Json(rep, JsonRequestBehavior.AllowGet);

                                }



                                rep.IsNull = true;
                                return Json(rep, JsonRequestBehavior.AllowGet);

                            }

                            rep.IsError = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);

                        }
                        rep.IsError = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);

                    }
                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);

            }
        }


      


        [HttpGet]
        public ActionResult GetDoc(long id)
        {
            var rep = new Reporter();
            try
            {

                using (var db = new ImportPermitEntities())
                {

                        var doc = db.Documents.Where(d => d.DocumentId == id).ToList();
                    if (doc.Any())
                    {
                        rep.Id = doc[0].DocumentId;
                        rep.DocPath = doc[0].DocumentPath.Replace("~", ""); 

                        var notDoc = db.NotificationDocuments.Where(n => n.DocumentId == id).ToList();
                        if (notDoc.Any())
                        {
                            rep.notid = notDoc[0].NotificationId;
                            rep.IsAccepted = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }
                        
                    }

                    rep.IsError = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult GetStandardDoc(long id)
        {
            var rep = new Reporter();
            try
            {

                using (var db = new ImportPermitEntities())
                {

                    var doc = db.StandardRequirements.Where(d => d.Id == id).ToList();
                    if (doc.Any())
                    {
                        rep.Id = doc[0].Id;
                        rep.DocPath = doc[0].DocumentPath.Replace("~", "");

                        rep.IsAccepted = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);

                    }

                    rep.IsError = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


       


        [HttpGet]
        public ActionResult ValidDocument(long id)
        {
            var rep = new Reporter();
            try
            {

                using (var db = new ImportPermitEntities())
                {
                    
                        var doc = db.Documents.Where(d => d.DocumentId == id).ToList();
                        if (doc.Any())
                        {
                            var docId = doc[0].DocumentId;
                           
                            doc[0].IsValid = "True";
                            db.Documents.Attach(doc[0]);
                            db.Entry(doc[0]).State = EntityState.Modified;

                            var notDoc = db.NotificationDocuments.Where(n => n.DocumentId == docId).ToList();
                            if (notDoc.Any())
                            {
                                var notId = notDoc[0].NotificationId;

                                var track =
                                    db.NotificationInspectionQueues.Where(r => r.NotificationId == notId).ToList();

                                if (track.Any())
                                {
                                    db.SaveChanges();
                                    rep.IsValid = true;
                                    rep.TrackId = track[0].Id;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }
                            }

                        }


                        rep.IsError = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);


                   
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult InValidDocument(long id)
        {
            var rep = new Reporter();
            try
            {

                using (var db = new ImportPermitEntities())
                {

                    var doc = db.Documents.Where(d => d.DocumentId == id).ToList();
                    if (doc.Any())
                    {
                        var docId = doc[0].DocumentId;

                        doc[0].IsValid = "False";
                        db.Documents.Attach(doc[0]);
                        db.Entry(doc[0]).State = EntityState.Modified;

                        var notDoc = db.NotificationDocuments.Where(n => n.DocumentId == docId).ToList();
                        if (notDoc.Any())
                        {
                            var notId = notDoc[0].NotificationId;

                            var track =
                                db.NotificationInspectionQueues.Where(r => r.NotificationId == notId).ToList();

                            if (track.Any())
                            {
                                db.SaveChanges();
                                rep.IsValid = true;
                                rep.TrackId = track[0].Id;
                                return Json(rep, JsonRequestBehavior.AllowGet);
                            }
                        }

                    }


                    rep.IsError = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);



                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult AddNotificationReportCheckList(NotificationCheckListObject employeeProfile)
        {
            var rep = new Reporter();
            try
            {


                if (employeeProfile == null)
                {
                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }



                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                    long employeeId;

                    if (employeeDesk.Any())
                    {
                        employeeId = employeeDesk[0].Id;

                        var checkOutcome =
                            db.NotificationCheckListOutcomes.Where(
                                c =>
                                    c.NotificationCheckListId == employeeProfile.Id &&
                                    c.NotificationId == employeeProfile.NotificationId && c.EmployeeId == employeeId)
                                .ToList();

                        if (checkOutcome.Any())
                        {
                            checkOutcome[0].Value = employeeProfile.MySelection;

                            db.NotificationCheckListOutcomes.Attach(checkOutcome[0]);
                            db.Entry(checkOutcome[0]).State = EntityState.Modified;
                            db.SaveChanges();

                        }







                        rep.IsAccepted = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);


                    }
                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult IsCheckListComplete(long id)
        {
            var rep = new Reporter();
            try
            {

                using (var db = new ImportPermitEntities())
                {
                    var notification = db.Notifications.Where(n => n.Id == id).ToList();

                    if (notification.Any())
                    {
                        
                        
                        var doc = db.NotificationDocuments.Where(o => o.NotificationId == id).ToList();

                        if (doc.Any())
                        {

                            foreach (var item in doc)
                            {
                                var normalDoc = db.Documents.Where(m => m.DocumentId == item.DocumentId).ToList();
                                if (normalDoc.Any())
                                {

                                    if (normalDoc[0].IsValid == "False" || normalDoc[0].IsValid == null)
                                    {
                                        rep.IsValid = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }


                                }
                            }
                        }
                        rep.IsAccepted = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);

                    }

                   

                    rep.IsError = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult IsVesselReportComplete(long id)
        {
            var rep = new Reporter();
            try
            {

                using (var db = new ImportPermitEntities())
                {
                    var notification = db.Notifications.Where(n => n.Id == id).ToList();

                    if (notification.Any())
                    {


                        var notInspec = db.NotificationInspections.Where(o => o.NotificationId == id).ToList();

                        if (notInspec.Any())
                        {
                            if (string.IsNullOrEmpty(notInspec[0].QuantityOnVessel.ToString()))
                            {
                                rep.IsNull = true;
                                return Json(rep, JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(notInspec[0].QuantityOnBillOfLading.ToString()))
                            {
                                rep.IsNull = true;
                                return Json(rep, JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(notInspec[0].InspectionDate.ToString()))
                            {
                                rep.IsNull = true;
                                return Json(rep, JsonRequestBehavior.AllowGet);
                            }

                            var recert = db.RecertificationResults.Where(r => r.NotificationId == id).ToList();
                            if (recert.Any())
                            {
                                if (string.IsNullOrEmpty(recert[0].Density.ToString()))
                                {
                                    rep.IsNull = true;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }
                                if (string.IsNullOrEmpty(recert[0].DischargeApproval))
                                {
                                    rep.IsNull = true;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }

                                rep.IsAccepted = true;
                                return Json(rep, JsonRequestBehavior.AllowGet);
                                
                            }
                            rep.IsNull = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }


                        rep.IsNull = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }

                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult ContinueCheckListLater(long id)
        {
            var rep = new Reporter();
            try
            {

                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId.Equals(profileId)).ToList();

                    long employeeId;

                    if (employeeDesk.Any())
                    {
                        employeeId = employeeDesk[0].Id;

                        var checkOutcome =
                            db.NotificationCheckListOutcomes.Where(
                                c =>
                                    c.NotificationId == id && c.EmployeeId == employeeId).ToList();

                        if (checkOutcome.Any())
                        {
                            foreach (var item in checkOutcome)
                            {
                                item.Status = (int)EnumCheckListOutComeStatus.Verified;
                                db.NotificationCheckListOutcomes.Attach(item);
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                            }



                        }

                        rep.IsAccepted = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);


                    }
                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult SaveCheckList(long id)
        {
            var rep = new Reporter();
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId.Equals(profileId)).ToList();

                    long employeeId;

                    if (employeeDesk.Any())
                    {
                        employeeId = employeeDesk[0].Id;

                        var checkOutcome =
                            db.NotificationCheckListOutcomes.Where(
                                c =>
                                    c.NotificationId == id && c.EmployeeId == employeeId).ToList();

                        if (checkOutcome.Any())
                        {
                            foreach (var item in checkOutcome)
                            {
                                item.Status = (int)EnumCheckListOutComeStatus.Saved;
                                item.Date = DateTime.Now;
                                db.NotificationCheckListOutcomes.Attach(item);
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }

                        rep.IsAccepted = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);


                    }
                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        public async Task<ActionResult> AcceptApp(EmployeeProfileObject employeeProfile)
        {
            var rep = new Reporter();

            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var importapplication = db.Applications.Find(employeeProfile.ApplicationId);

                    //update the application statuscode to enable us get the next step
                    if (importapplication == null)
                    {
                        rep.IsNull = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }

                    //get the id of logged in user
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                    //var employeeDesk = db.EmployeeDesks.FirstOrDefault(e => e.EmployeeId.Equals(3));

                    long employeeId;

                    if (employeeDesk.Any())
                    {
                        employeeId = employeeDesk[0].Id;
                        //employeeId = employeeDesk.Id;

                        //update the tracking stepcode to enable us get the next step
                        var track =
                            db.ProcessTrackings.Where(p => p.ApplicationId == employeeProfile.ApplicationId && p.EmployeeId == employeeId).ToList();

                        //get the sequence of the current step

                        var stepnow = db.Steps.Find(track[0].StepId);

                        var nextStepSequence = stepnow.SequenceNumber + 1;

                        //find out if this is the last step
                        var stepnext = db.Steps.Where(s => s.SequenceNumber == nextStepSequence && s.Process.ImportStageId == (int)AppStage.Application).ToList();

                        if (stepnext.Any() && nextStepSequence <= 5)
                        {
                            var fsx = (int)nextStepSequence;

                            //call the assign method
                            var assignProId = new WorkFlowServices().AssignApplicationToEmployeeInTrack(employeeProfile.ApplicationId, fsx, track[0].Id);

                            if (assignProId > 0)
                            {
                                //add a history of work done 
                                var prohis = new ProcessingHistory
                                {
                                    ApplicationId = employeeProfile.ApplicationId,
                                    AssignedTime = track[0].AssignedTime,
                                    EmployeeId = employeeId,
                                    FinishedTime = DateTime.Now,
                                    StepId = track[0].StepId,
                                    OutComeCode = (int) AppStatus.Processing,
                                    Remarks = employeeProfile.Description
                                };


                                db.ProcessingHistories.Add(prohis);

                                //update employee job count

                                employeeDesk[0].JobCount = employeeDesk[0].JobCount - 1;
                                employeeDesk[0].ApplicationCount = employeeDesk[0].ApplicationCount - 1;

                                var employee = employeeDesk[0];

                                db.EmployeeDesks.Attach(employee);
                                db.Entry(employee).State = EntityState.Modified;

                                db.SaveChanges();


                                rep.IsAccepted = true;

                                var userPro = db.UserProfiles.Find(assignProId);

                                var name = userPro.Person.FirstName;

                                var email = "";
                                var reg = db.AspNetUsers.Where(p => p.UserInfo_Id == assignProId).ToList();

                                if (reg.Any())
                                {
                                    email = reg[0].Email;
                                }

                                var mailObj = new EmployeeMailObject
                                {
                                    UserId = assignProId,
                                    Email = email,
                                    Name = name
                                };

                                try
                                {
                                    await SendEmployeeMail(mailObj);
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }
                                catch (SmtpException ex)
                                {
                                    ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);

                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }
                                                             

                            }


                        }

                        else
                        {
                            //Build permit

                            track[0].ActualDeliveryDateTime = DateTime.Now;

                            track[0].StatusId = (int)AppStatus.Verifying;

                            //stepcode of 100 means the applicatication id finished
                            track[0].StepCode = 0;

                            var finalTrack = track[0];

                            db.Entry(finalTrack).State = EntityState.Modified;

                            //add a history of work done 
                            var prohis = new ProcessingHistory
                            {
                                ApplicationId = employeeProfile.ApplicationId,
                                AssignedTime = track[0].AssignedTime,
                                EmployeeId = employeeId,
                                FinishedTime = DateTime.Now,
                                StepId = track[0].StepId,
                                OutComeCode = (int) AppStatus.Verifying,
                                Remarks = employeeProfile.Description
                            };

                            db.ProcessingHistories.Add(prohis);
                            employeeDesk[0].JobCount = employeeDesk[0].JobCount - 1;
                            employeeDesk[0].ApplicationCount = employeeDesk[0].ApplicationCount - 1;

                            var employee = employeeDesk[0];

                            db.EmployeeDesks.Attach(employee);
                            db.Entry(employee).State = EntityState.Modified;

                            //change application status
                            importapplication.ApplicationStatusCode = (int)AppStatus.Verifying;
                            db.Applications.Attach(importapplication);
                            db.Entry(importapplication).State = EntityState.Modified;
                            db.SaveChanges();

                            rep.IsFinal = true;
                            rep.NoEmployee = false;
                            return Json(rep, JsonRequestBehavior.AllowGet);

                        }
                    }

                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);

                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }

        }

        public async Task<ActionResult> SignOff(long applicationId)
        {
            var rep = new GenericValidator();

            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var importapplications = db.Applications.Where(a => a.Id == applicationId).Include("Importer").Include("Invoice").ToList();

                    if (!importapplications.Any())
                    {
                        rep.Code = -1;
                        rep.Error = "Application information could not be retrieved. Please try again later";
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }

                    //change application status so that Applicant can see and print permit
                    var app = importapplications[0];
                    app.ApplicationStatusCode = (int)AppStatus.Approved;
                    db.Entry(app).State = EntityState.Modified;
                    db.SaveChanges();

                    int permid;
                    var all = from l in db.Permits
                              orderby l.PermitNo descending
                              select new DatabaseObject
                              {
                                  Id = l.Id,
                                  PermitPermitNo = l.PermitNo
                              };


                    if (all.Any())
                    {
                        permid = all.First().PermitPermitNo + 1;
                    }
                    else
                    {
                        permid = 1;
                    }
                    string permNum = "";
                    var count = permid.ToString().Length;
                    if (count == 1)
                    {
                        permNum = "00000" + permid;
                    }
                    else if (count == 2)
                    {
                        permNum = "0000" + permid;
                    }
                    else if (count == 3)
                    {
                        permNum = "000" + permid;
                    }
                    else if (count == 4)
                    {
                        permNum = "00" + permid;
                    }
                    else if (count == 5)
                    {
                        permNum = "0" + permid;
                    }
                    else if (count >= 6)
                    {
                        permNum = permid.ToString();
                    }

                    var year = DateTime.Now.Year;
                    var rc = app.Importer.RCNumber;
                    var thenum = "DPR/PPIPS/" + rc + "/" + year + permNum;

                    var perm = new Permit
                    {
                        IssueDate = DateTime.Now,
                        PermitNo = permid,
                        PermitValue = thenum,
                        DateAdded = DateTime.Now
                    };


                    var time = new TimeSpan(90, 0, 0, 0);
                    var combined = DateTime.Now.Add(time);

                    perm.ExpiryDate = combined;
                    perm.ImporterId = app.ImporterId;

                    perm.PermitStatus = (int)PermitStatusEnum.Active;

                   var permit = db.Permits.Add(perm);
                    db.SaveChanges();

                    //create the permitApplication
                    var permitAppInfo = new PermitApplication
                    {
                        PermitId = perm.Id,
                        ApplicationId = app.Id
                    };

                    db.PermitApplications.Add(permitAppInfo);
                    db.SaveChanges();

                    //var pdfres = GetPdfNow(employeeProfile.ApplicationId, radno, thenum, company, combined);

                    //Notify Applicant by email

                    var importerId = app.Importer.Id;
                    var users = ( from p in db.People.Where(p => p.ImporterId == importerId)
                                 join usp in db.UserProfiles on p.Id equals usp.PersonId
                                 join aus in db.AspNetUsers on usp.Id equals aus.UserInfo_Id
                                 select new UserProfileObject
                                 {
                                     Email = aus.Email,
                                     PhoneNumber = aus.PhoneNumber,
                                     Id = usp.Id
                                 }).ToList();
                    
                    if (!users.Any())
                    {
                        rep.Code = -1;
                        rep.Error = "Application information could not be retrieved. Please try again later";
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }

                    var user = users[0];
                    //var permitApp = app.PermitApplications.ToList()[0];
                    //var permits = db.Permits.Where(p => p.Id == permitApp.PermitId).ToList();
                    //if (!permits.Any())
                    //{
                    //    rep.Code = -1;
                    //    rep.Error = "Application information could not be retrieved. Please try again later";
                    //    return Json(rep, JsonRequestBehavior.AllowGet);
                    //}

                    var mailObj = new PermitMailObject
                    {
                        ImporterName = app.Importer.Name,
                        PermitNumber = permit.PermitValue,
                        UserId = user.Id,
                        ReferenceNumber = app.Invoice.ReferenceCode,
                        Email = user.Email, 
                        Rrr = app.Invoice.RRR,
                        PhoneNumber = user.PhoneNumber
                    };

                    try
                    {
                        await SendPermitMail(mailObj);
                        rep.Code = 5;
                        rep.Error = "Application has been Signed off and a mail has been sent to notify the Applicant.";
                        
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }
                    catch (SmtpException ex)
                    {
                        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);

                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }
                    
                }

            }
            catch (Exception ex)
            {
                rep.Code = -1;
                rep.Error = "Application information could not be retrieved. Please try again later";
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(rep, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GeneratePermit(long permitId)
        {
            var gVal = new GenericValidator();

            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var permitApps = db.PermitApplications.Where(p => p.PermitId == permitId).Include("Application").Include("Permit").ToList();

                    if (!permitApps.Any())
                    {
                        gVal.Code = -1;
                        gVal.Error = "Permit could not be generated.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    var permitApp = permitApps[0];
                    var importers = db.Importers.Where(i => i.Id == permitApp.Application.ImporterId).ToList();
                    if (!importers.Any())
                    {
                        gVal.Code = -1;
                        gVal.Error = "Permit could not be generated.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    var company = importers[0].Name;
                    var permitNumber = permitApp.Permit.PermitValue;

                    var time = new TimeSpan(90, 0, 0, 0);
                    var combined = DateTime.Now.Add(time);

                    var permitPath = GetPdfNow(permitApp.Application.Id, importers[0].Id, permitNumber, company, combined);

                    if (string.IsNullOrEmpty(permitPath))
                    {
                        gVal.Code = -1;
                        gVal.Error = "Permit could not be generated.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }
                    
                    gVal.Code = 5;
                    gVal.Error = "Permit was successfully generated.";
                    gVal.Path = permitPath;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "Permit could not be generated.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

        }

        private async Task SendPermitMail(PermitMailObject model)
        {
            try
            {
                if (model == null)
                {
                    return;
                }

                var msgBody = "";
                var msg = new MessageTemplateServices().GetMessageTemp((int)MessageEventEnum.New_Permit);
                if (msg.Id < 1)
                {
                    return;
                }

                var emMs = new MessageObject
                {
                    UserId = model.UserId,
                    MessageTemplateId = msg.Id,
                    Status = (int)MessageStatus.Pending,
                    DateSent = DateTime.Now,
                    MessageBody = msg.MessageContent
                };

                var sta = new MessageServices().AddMessagePerm(emMs);
                if (sta < 1)
                {
                    return;
                }

                if (Request.Url != null)
                {
                    msgBody = "Hi" + " " + model.ImporterName + "<br/><br/>";
                    msg.Subject = msg.Subject.Replace("{ reference code}", model.Rrr).Replace("{Permit No}", model.PermitNumber);
                    msg.MessageContent = msg.MessageContent.Replace("{Permit No}", model.PermitNumber).Replace("\n", "<br/>");


                    msgBody += msg.MessageContent;
                    msgBody += "<br/><br/>" + msg.Footer.Replace("\n", "<br/>");
                }
                
                if (Request.Url != null)
                {

                    var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                    var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                    var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                    var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                    if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                    {
                        return;
                    }

                    #region Using Mandrill
                    var api = new MandrillApi(apiKey);
                    var receipint = new List<MandrillMailAddress> { new MandrillMailAddress(model.Email) };
                    var message = new MandrillMessage()
                    {
                        AutoHtml = true,
                        To = receipint,
                        FromEmail = settings.Smtp.From,
                        FromName = appName,
                        Subject = msg.Subject,
                        Html = msgBody
                    };

                    var result = await api.Messages.SendAsync(message);

                    if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                    {
                        emMs.Status = (int)MessageStatus.Failed;
                    }
                    else
                    {
                        emMs.Status = (int)MessageStatus.Sent;
                    }

                    #endregion
                    ServiceHelper.SendSmsNotification(model.PhoneNumber, msg.Subject);
                    emMs.Id = sta;
                    emMs.MessageBody = msgBody;
                    var tts = new MessageServices().UpdateMessage(emMs);
                    if (tts < 1)
                    {
                        return;
                    }
                    
                }
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
            }
        }

        private async Task<bool> SendEmployeeMail(EmployeeMailObject model)
        {
            try
            {
                if (model == null)
                {
                    return false;
                }

                var msgBody = "";
                var msg = new MessageTemplateServices().GetMessageTemp((int)MessageEventEnum.Employee_NewJob);
                if (msg.Id < 1)
                {
                    return false;
                }

                var emMs = new MessageObject
                {

                    UserId = model.UserId,
                    MessageTemplateId = msg.Id,
                    Status = (int)MessageStatus.Pending,
                    DateSent = DateTime.Now,
                    MessageBody = msg.MessageContent
                };

                var sta = new MessageServices().AddMessagePerm(emMs);
                if (sta < 1)
                {
                    return false;
                }

                if (Request.Url != null)
                {
                    msgBody = "Hi" + " " + model.Name + "<br/><br/>";
                    msg.Subject = msg.Subject;
                  
                    msgBody += msg.MessageContent;
                    msgBody += "<br/><br/>" + msg.Footer.Replace("\n", "<br/>");
                }

                if (Request.Url != null)
                {

                    var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                    var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                    var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                    var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                    if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                    {
                        return false;
                    }

                    #region Using Mandrill
                    var api = new MandrillApi(apiKey);
                    var receipint = new List<MandrillMailAddress> { new MandrillMailAddress(model.Email) };
                    var message = new MandrillMessage()
                    {
                        AutoHtml = true,
                        To = receipint,
                        FromEmail = settings.Smtp.From,
                        FromName = appName,
                        Subject = msg.Subject,
                        Html = msgBody
                    };

                    var result = await api.Messages.SendAsync(message);

                    if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                    {
                        emMs.Status = (int)MessageStatus.Failed;
                    }
                    else
                    {
                        emMs.Status = (int)MessageStatus.Sent;
                    }
                    #endregion

                   
                    emMs.Id = sta;
                    emMs.MessageBody = msgBody;
                    var tts = new MessageServices().UpdateMessage(emMs);
                    if (tts < 1)
                    {
                        return false;
                    }

                    return true;

                }

                return false;
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                return false;
            }
        }

        public ActionResult AcceptRecertification(EmployeeProfileObject employeeProfile)
        {
            var rep = new Reporter();

            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var recertification = db.Recertifications.Find(employeeProfile.ApplicationId);


                    if (recertification == null)
                    {
                        rep.IsNull = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }

                    //get the id of logged in user
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                    //var employeeDesk = db.EmployeeDesks.FirstOrDefault(e => e.EmployeeId.Equals(3));

                    long employeeId;

                    if (employeeDesk.Any())
                    {
                        employeeId = employeeDesk[0].Id;
                        //employeeId = employeeDesk.Id;

                        //update the tracking stepcode to enable us get the next step
                        var track =
                            db.RecertificationProcesses.Where(p => p.RecertificationId == employeeProfile.ApplicationId && p.EmployeeId == employeeId).ToList();

                        //get the sequence of the current step

                        var stepnow = db.Steps.Find(track[0].StepId);

                        var nextStepSequence = stepnow.SequenceNumber + 1;

                        //find out if this is the last step
                        var stepnext = db.Steps.Where(s => s.SequenceNumber == nextStepSequence && s.Process.ImportStageId == (int)AppStage.Recertification).ToList();

                        if (stepnext.Any())
                        {

                            var fsx = (int)nextStepSequence;

                            //call the assign method
                            var res = new WorkFlowServices().AssignRecertificationToEmployeeInTrack(employeeProfile.ApplicationId, fsx, track[0].Id);

                            if (res)
                            {
                                //add a history of work done 
                                var prohis = new RecertificationHistory();
                                prohis.RecertificationId = employeeProfile.ApplicationId;
                                prohis.AssignedTime = track[0].AssignedTime;
                                prohis.EmployeeId = employeeId;
                                prohis.FinishedTime = DateTime.Now;
                                prohis.StepId = track[0].StepId;
                                prohis.OutComeCode = (int)RecertificationStatusEnum.Processing;
                                prohis.Remarks = employeeProfile.Description;


                                db.RecertificationHistories.Add(prohis);

                                //update employee job count

                                employeeDesk[0].JobCount = employeeDesk[0].JobCount - 1;
                                employeeDesk[0].RecertificationCount = employeeDesk[0].RecertificationCount - 1;

                                var employee = employeeDesk[0];

                                db.EmployeeDesks.Attach(employee);
                                db.Entry(employee).State = EntityState.Modified;

                                db.SaveChanges();


                                rep.IsAccepted = true;
                                return Json(rep, JsonRequestBehavior.AllowGet);

                            }
                        }

                        else
                        {
                            //generate permit

                            track[0].ActualDeliveryDateTime = DateTime.Now;

                            //stepcode of 0 means the applicatication id finished
                            track[0].StepCode = 0;
                            track[0].StatusId = (int)RecertificationStatusEnum.Approved;

                            var finalTrack = track[0];

                            db.Entry(finalTrack).State = EntityState.Modified;

                            //add a history of work done 
                            var prohis = new RecertificationHistory();
                            prohis.RecertificationId = employeeProfile.ApplicationId;
                            prohis.AssignedTime = track[0].AssignedTime;
                            prohis.EmployeeId = employeeId;
                            prohis.FinishedTime = DateTime.Now;
                            prohis.StepId = track[0].StepId;
                            prohis.OutComeCode = (int)RecertificationStatusEnum.Approved;
                            prohis.Remarks = employeeProfile.Description;

                            db.RecertificationHistories.Add(prohis);


                            //update employee job count

                            employeeDesk[0].JobCount = employeeDesk[0].JobCount - 1;
                            employeeDesk[0].RecertificationCount = employeeDesk[0].RecertificationCount - 1;

                            var employee = employeeDesk[0];

                            db.EmployeeDesks.Attach(employee);
                            db.Entry(employee).State = EntityState.Modified;

                            //send email

                            //change recertification status
                            recertification.Status = (int)RecertificationStatusEnum.Approved;
                            db.Recertifications.Attach(recertification);
                            db.Entry(recertification).State = EntityState.Modified;
                            db.SaveChanges();

                            rep.IsFinal = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);


                        }
                    }


                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);

                }

            }


            catch (Exception ex)
            {

                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpGet]
        public ActionResult MarkCheckList(long id)
        {
            var rep = new List<NotificationCheckListObject>();
            try
            {




                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId.Equals(profileId)).ToList();



                    if (employeeDesk.Any())
                    {
                        long employeeId = employeeDesk[0].Id;
                        var list = db.NotificationCheckLists.ToList();

                        foreach (var item in list)
                        {
                            var ob = new NotificationCheckListObject();

                            var outcome =
                                db.NotificationCheckListOutcomes.Where(
                                    o => o.NotificationCheckListId == item.Id && o.EmployeeId == employeeId).ToList();
                            if (outcome.Any())
                            {
                                ob.CheckListItem = item.CheckListItem;
                                ob.Id = item.Id;
                                if (outcome[0].Value == "Yes")
                                {
                                    ob.YesSelection = "Yes";
                                }
                                else
                                {
                                    ob.NoSelection = "No";
                                }

                                rep.Add(ob);

                            }
                            if (!outcome.Any())
                            {
                                ob.CheckListItem = item.CheckListItem;
                                ob.Id = item.Id;


                                ob.NoSelection = "No";


                                rep.Add(ob);

                                var newOutcome = new NotificationCheckListOutcome();
                                newOutcome.NotificationId = id;
                                newOutcome.NotificationCheckListId = item.Id;
                                newOutcome.Value = "No";
                                newOutcome.EmployeeId = employeeId;
                                newOutcome.Status = (int)EnumCheckListOutComeStatus.Verified;
                                db.NotificationCheckListOutcomes.Add(newOutcome);

                            }







                        }
                        //var clist = (from o in db.NotificationCheckListOutcomes
                        //    join c in db.NotificationCheckLists on o.NotificationCheckListId equals c.Id into oc
                        //    where o.NotificationId == id && o.EmployeeId == employeeId
                        //    select new NotificationCheckListObject
                        //    {
                        //        CheckListItem = o.c
                        //    }).ToList();



                        db.SaveChanges();

                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }
                    rep[0].NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep[0].IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult PrintCheckList(long id)
        {
            var way = new PObject();
            try
            {

                Random rad = new Random();
                var radno = rad.Next();

                PdfDocument pdf = new PdfDocument();

                //Next step is to create a an Empty page.

                PdfPage pp = pdf.AddPage();


                pp.Size = PageSize.A4;



                string path = Path.Combine(Server.MapPath("~/InspectionDoc"), "Checklist.pdf");

                //Then create an XGraphics Object

                XGraphics gfx = XGraphics.FromPdfPage(pp);

                XImage image = XImage.FromFile(path);
                gfx.DrawImage(image, 0, 0);




                //XFont font = new XFont("Calibri", 12, XFontStyle.Regular);
                //XFont font2 = new XFont("Calibri", 8, XFontStyle.Regular);






                MigraDoc.DocumentObjectModel.Document doc = new MigraDoc.DocumentObjectModel.Document();



                Section section = doc.AddSection();

                var table = section.AddTable();



                //table = section.AddTable();
                table.Style = "Table";

                table.Borders.Width = 0;
                table.Borders.Left.Width = 0;
                table.Borders.Right.Width = 0;
                table.Rows.LeftIndent = 0;

                Column column = table.AddColumn("2cm");
                column.Format.Alignment = ParagraphAlignment.Center;

                column = table.AddColumn("12cm");
                column.Format.Alignment = ParagraphAlignment.Center;


                column = table.AddColumn("2cm");
                column.Format.Alignment = ParagraphAlignment.Center;


                using (var db = new ImportPermitEntities())
                {


                    var notdoc = db.NotificationDocuments.Where(o => o.NotificationId == id).ToList();
                    if (notdoc.Any())
                    {

                        var num = notdoc.Count();
                        var i = 1;
                        foreach (var item in notdoc)
                    {
                        var normalDoc = db.Documents.Where(m => m.DocumentId == item.DocumentId).ToList();
                        if (normalDoc.Any())
                        {

                            if (normalDoc[0].IsValid == "True" && i <= num)
                            {
                                Row row = table.AddRow();
                                row.Format.Font.Bold = true;
                                row.Format.Font.Size = 14;
                                row = table.AddRow();
                                row.Cells[0].AddParagraph(i.ToString());
                                row.Cells[0].Format.Alignment = ParagraphAlignment.Left;
                                row.Cells[1].AddParagraph(normalDoc[0].DocumentType.Name);
                                row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                                row.Cells[2].AddParagraph("True");
                                row.Cells[2].Format.Alignment = ParagraphAlignment.Left;

                                i = i + 1;
                            }
                            else if (normalDoc[0].IsValid == "False" && i <= num)
                            {
                                Row row = table.AddRow();
                                row.Format.Font.Bold = true;
                                row.Format.Font.Size = 14;
                                row = table.AddRow();
                                row.Cells[0].AddParagraph(i.ToString());
                                row.Cells[0].Format.Alignment = ParagraphAlignment.Left;
                                row.Cells[1].AddParagraph(normalDoc[0].DocumentType.Name);
                                row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                                row.Cells[2].AddParagraph("False");
                                row.Cells[2].Format.Alignment = ParagraphAlignment.Left;

                                i = i + 1;
                            }
                            
                        }
                    }

                
                        const bool unicode = false;
                        const PdfFontEmbedding embedding = PdfFontEmbedding.Always;

                        PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);

                        // Associate the MigraDoc document with a renderer

                        pdfRenderer.Document = doc;



                        // Layout and render document to PDF

                        pdfRenderer.RenderDocument();



                        var pathtable = Path.Combine(Server.MapPath("~/InspectionDoc"), "table.pdf");

                        pdfRenderer.PdfDocument.Save(pathtable);



                        //XImage imagetable2 = XImage.FromFile(pathtable2);



                        XImage imagetable = XImage.FromFile(Path.Combine(Server.MapPath("~/InspectionDoc"), "table.pdf"));
                        //gfx.DrawImage(imagetable, 0, 210);
                        gfx.DrawImage(imagetable, 20, 150, 550, 500);

                        //string path2 = Path.Combine(Server.MapPath("~/InspectionDoc"), "Checklist2.pdf");

                        string path2 = Path.Combine(Server.MapPath("~/TempDoc"), "Checklist2" + radno + ".pdf");

                        pdf.Save(path2);

                        way.SmallPath = @"\TempDoc\\" + "Checklist2" + radno + ".pdf";
                        way.BigPath = Path.Combine(Server.MapPath("~/TempDoc"), "Checklist2" + radno + ".pdf");


                        return Json(way, JsonRequestBehavior.AllowGet);



                    }

                    way.NoEmployee = true;
                    return Json(way, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                way.Error = true;
                return Json(way, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteFile(string way)
        {
            var rep = new Reporter();
            try
            {


                if ((System.IO.File.Exists(way)))
                {
                    System.IO.File.Delete(way);
                    rep.IsAccepted = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);

                }

                rep.IsAccepted = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult StoreRecertification(RecertificationResultObject employeeProfile)
        {
            var rep = new Reporter();
            try
            {


                if (employeeProfile == null)
                {
                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }




                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId.Equals(profileId)).ToList();



                    if (employeeDesk.Any())
                    {
                        long employeeId = employeeDesk[0].Id;

                        var cert =
                            db.RecertificationResults.Where(r => r.NotificationId == employeeProfile.NotificationId)
                                .ToList();
                        if (cert.Any())
                        {
                            cert[0].Benzene = employeeProfile.Benzene;
                            cert[0].InitialBoilingPoint = employeeProfile.InitialBoilingPoint;
                            cert[0].FinalBoilingPoint = employeeProfile.FinalBoilingPoint;
                            cert[0].MSEP = employeeProfile.MSEP;

                            cert[0].LimitVariance = employeeProfile.LimitVariance;
                            cert[0].REIDVapourPressure = employeeProfile.REIDVapourPressure;
                            cert[0].ResearchOctaneNumber = employeeProfile.ResearchOctaneNumber;
                            cert[0].TotalSulphur = employeeProfile.TotalSulphur;
                            cert[0].TotalAcidity = employeeProfile.TotalAcidity;
                          

                            cert[0].CaptainName = employeeProfile.CaptainName;
                            cert[0].DoctorTest = employeeProfile.DoctorTest;
                            cert[0].DieselIndex = employeeProfile.DieselIndex;
                            cert[0].Colour = employeeProfile.Colour;
                            cert[0].ProductColour = employeeProfile.ProductColour;

                            cert[0].Ethanol = employeeProfile.Ethanol;
                            cert[0].Flashpoint = employeeProfile.Flashpoint;
                            cert[0].FreezingPoint = employeeProfile.FreezingPoint;
                            cert[0].Density = employeeProfile.Density;
                            cert[0].DischargeApproval = employeeProfile.DischargeApproval;
                            cert[0].Spec = employeeProfile.Spec;
                            cert[0].RecommendationId = employeeProfile.RecommendationId;

                            db.RecertificationResults.Attach(cert[0]);
                            db.Entry(cert[0]).State = EntityState.Modified;
                            db.SaveChanges();

                            rep.Id = employeeProfile.NotificationId;
                            rep.IsAccepted = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }
                        if (!cert.Any())
                        {
                            var newcert = new RecertificationResult();

                            newcert.Benzene = employeeProfile.Benzene;
                            newcert.InitialBoilingPoint = employeeProfile.InitialBoilingPoint;
                            newcert.FinalBoilingPoint = employeeProfile.FinalBoilingPoint;
                            newcert.MSEP = employeeProfile.MSEP;
                            newcert.NotificationId = employeeProfile.NotificationId;
                            newcert.LimitVariance = employeeProfile.LimitVariance;
                            newcert.REIDVapourPressure = employeeProfile.REIDVapourPressure;
                            newcert.ResearchOctaneNumber = employeeProfile.ResearchOctaneNumber;
                            newcert.TotalSulphur = employeeProfile.TotalSulphur;
                            newcert.TotalAcidity = employeeProfile.TotalAcidity;
                            newcert.Spec = employeeProfile.Spec;
                            newcert.EmployeeId = employeeId;
                            newcert.CaptainName = employeeProfile.CaptainName;
                            newcert.DoctorTest = employeeProfile.DoctorTest;
                            newcert.DieselIndex = employeeProfile.DieselIndex;
                            newcert.ProductColour = employeeProfile.ProductColour;
                            newcert.RecommendationId = employeeProfile.RecommendationId;
                            
                            newcert.Ethanol = employeeProfile.Ethanol;
                            newcert.Flashpoint = employeeProfile.Flashpoint;
                            newcert.FreezingPoint = employeeProfile.FreezingPoint;
                            newcert.Density = employeeProfile.Density;
                            newcert.DischargeApproval = employeeProfile.DischargeApproval;
                            newcert.SubmittionStatus = (int)EnumCheckListOutComeStatus.Saved;

                            db.RecertificationResults.Add(newcert);
                            db.SaveChanges();

                            rep.Id = employeeProfile.NotificationId;
                            rep.IsAccepted = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);

                        }

                    }
                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }

   
        [HttpGet]
        public ActionResult RetrieveStoredRecertification(long id)
        {

            try
            {

                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId.Equals(profileId)).ToList();


                    if (employeeDesk.Any())
                    {
                        long employeeId = employeeDesk[0].Id;

                        var cert =
                            db.RecertificationResults.Where(
                                r =>
                                    r.NotificationId == id &&
                                    r.SubmittionStatus == (int)EnumCheckListOutComeStatus.Saved).ToList();

                        if (cert.Any())
                        {

                            var newcert = new RecertificationResultObject();

                            newcert.Benzene = cert[0].Benzene;
                            newcert.InitialBoilingPoint = cert[0].InitialBoilingPoint;
                            newcert.FinalBoilingPoint = cert[0].FinalBoilingPoint;
                            newcert.MSEP = cert[0].MSEP;
                            newcert.NotificationId = cert[0].NotificationId;
                            newcert.LimitVariance = cert[0].LimitVariance;
                            newcert.REIDVapourPressure = cert[0].REIDVapourPressure;
                            newcert.ResearchOctaneNumber = cert[0].ResearchOctaneNumber;
                            newcert.TotalSulphur = cert[0].TotalSulphur;
                            newcert.TotalAcidity = cert[0].TotalAcidity;
                            newcert.Spec = cert[0].Spec;
                            newcert.RecommendationId = cert[0].RecommendationId;
                            newcert.EmployeeId = employeeId;
                            newcert.CaptainName = cert[0].CaptainName;
                            newcert.DoctorTest = cert[0].DoctorTest;
                            newcert.DieselIndex = cert[0].DieselIndex;
                            newcert.ProductColour = cert[0].ProductColour;
                            newcert.Ethanol = cert[0].Ethanol;
                            newcert.Flashpoint = cert[0].Flashpoint;
                            newcert.FreezingPoint = cert[0].FreezingPoint;
                            newcert.Density = cert[0].Density;
                            newcert.DischargeApproval = cert[0].DischargeApproval;
                            newcert.SubmittionStatus = (int)EnumCheckListOutComeStatus.Submitted;
                            return Json(newcert, JsonRequestBehavior.AllowGet);
                        }



                    }

                    return Json(new RecertificationResultObject(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);

                return Json(new RecertificationResultObject(), JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult SubmitRecertification(RecertificationResultObject employeeProfile)
        {
            var rep = new Reporter();
            try
            {


                if (employeeProfile == null)
                {
                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }




                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId.Equals(profileId)).ToList();


                    if (employeeDesk.Any())
                    {
                        long employeeId = employeeDesk[0].Id;

                        var cert =
                            db.RecertificationResults.Where(r => r.NotificationId == employeeProfile.NotificationId)
                                .ToList();
                        if (cert.Any())
                        {
                            cert[0].Benzene = employeeProfile.Benzene;
                            cert[0].InitialBoilingPoint = employeeProfile.InitialBoilingPoint;
                            cert[0].MSEP = employeeProfile.MSEP;
                            cert[0].NotificationId = employeeProfile.NotificationId;
                            cert[0].LimitVariance = employeeProfile.LimitVariance;
                            cert[0].REIDVapourPressure = employeeProfile.REIDVapourPressure;
                            cert[0].ResearchOctaneNumber = employeeProfile.ResearchOctaneNumber;
                            cert[0].TotalSulphur = employeeProfile.TotalSulphur;
                            cert[0].Spec = employeeProfile.Spec;
                            cert[0].EmployeeId = employeeId;
                            cert[0].CaptainName = employeeProfile.CaptainName;
                            cert[0].DoctorTest = employeeProfile.DoctorTest;
                            cert[0].DieselIndex = employeeProfile.DieselIndex;
                            cert[0].DischargeApproval = employeeProfile.DischargeApproval;
                            cert[0].Ethanol = employeeProfile.Ethanol;
                            cert[0].Flashpoint = employeeProfile.Flashpoint;
                            cert[0].FreezingPoint = employeeProfile.FreezingPoint;
                            cert[0].Density = employeeProfile.Density;
                            cert[0].SubmittionStatus = (int)EnumCheckListOutComeStatus.Submitted;
                            db.RecertificationResults.Attach(cert[0]);
                            db.Entry(cert[0]).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            var newcert = new RecertificationResult
                            {
                                Benzene = employeeProfile.Benzene,
                                InitialBoilingPoint = employeeProfile.InitialBoilingPoint,
                                MSEP = employeeProfile.MSEP,
                                NotificationId = employeeProfile.NotificationId,
                                LimitVariance = employeeProfile.LimitVariance,
                                REIDVapourPressure = employeeProfile.REIDVapourPressure,
                                ResearchOctaneNumber = employeeProfile.ResearchOctaneNumber,
                                TotalSulphur = employeeProfile.TotalSulphur,
                                Spec = employeeProfile.Spec,
                                EmployeeId = employeeId,
                                CaptainName = employeeProfile.CaptainName,
                                DoctorTest = employeeProfile.DoctorTest,
                                DieselIndex = employeeProfile.DieselIndex,
                                DischargeApproval = employeeProfile.DischargeApproval,
                                Ethanol = employeeProfile.Ethanol,
                                Flashpoint = employeeProfile.Flashpoint,
                                FreezingPoint = employeeProfile.FreezingPoint,
                                Density = employeeProfile.Density,
                                SubmittionStatus = (int)EnumCheckListOutComeStatus.Submitted
                            };


                            db.RecertificationResults.Add(newcert);
                            db.SaveChanges();

                        }



                        rep.IsAccepted = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);


                    }
                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public ActionResult EditTank(NotificationDischageDataObject employeeProfile)
        {

            try
            {
                var dischargeDataObject = new NotificationDischageDataObject();

                if (employeeProfile == null)
                {
                    dischargeDataObject.IsNull = true;
                    return Json(dischargeDataObject, JsonRequestBehavior.AllowGet);
                }


                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId.Equals(profileId)).ToList();


                    if (employeeDesk.Any())
                    {
                        long employeeId = employeeDesk[0].Id;

                        //get notification discharge data if exist

                        var dischargeData =
                            db.NotificationDischageDatas.Where(
                                d => 
                                     d.Id == employeeProfile.TankId)
                                .ToList();
                        if (dischargeData.Any())
                        {
                            var tankId = dischargeData[0].TankId;

                            //get the before parameters
                            var parameterBeforeObject = new DischargeParameterBeforeObject();

                            var parameterBefore =
                                db.DischargeParameterBefores.Where(
                                    p =>
                                        p.NotificationId == employeeProfile.NotificationId &&
                                        p.TankId == tankId).ToList();
                            if (parameterBefore.Any())
                            {


                                parameterBeforeObject.EquivVolInM_1515C = parameterBefore[0].EquivVolInM_1515C;
                                parameterBeforeObject.NetVolOfOil_TkTc = parameterBefore[0].NetVolOfOil_TkTc;

                                parameterBeforeObject.NetVol_1515C = parameterBefore[0].NetVol_1515C;
                                parameterBeforeObject.SG_515C = parameterBefore[0].SG_515C;

                                parameterBeforeObject.TankGuage = parameterBefore[0].TankGuage;
                                parameterBeforeObject.TankTC = parameterBefore[0].TankTC;

                                parameterBeforeObject.VolCorrFactor = parameterBefore[0].VolCorrFactor;
                                parameterBeforeObject.VolOfWaterLTRS = parameterBefore[0].VolOfWaterLTRS;

                                parameterBeforeObject.SGtC_Lab = parameterBefore[0].SGtC_Lab;
                                parameterBeforeObject.CrossVol_TkPcLTRS = parameterBefore[0].CrossVol_TkPcLTRS;


                            }



                            //get the after parameters
                            var parameterAfterObject = new DischargeParameterAfterObject();

                            var parameterAfter =
                                db.DischargeParameterAfters.Where(
                                    p =>
                                        p.NotificationId == employeeProfile.NotificationId &&
                                        p.TankId == tankId).ToList();
                            if (parameterAfter.Any())
                            {


                                parameterAfterObject.EquivVolInM_1515C = parameterAfter[0].EquivVolInM_1515C;
                                parameterAfterObject.NetVolOfOil_TkTc = parameterAfter[0].NetVolOfOil_TkTc;

                                parameterAfterObject.NetVol_1515C = parameterAfter[0].NetVol_1515C;
                                parameterAfterObject.SG_515C = parameterAfter[0].SG_515C;

                                parameterAfterObject.TankGuage = parameterAfter[0].TankGuage;
                                parameterAfterObject.TankTC = parameterAfter[0].TankTC;

                                parameterAfterObject.VolCorrFactor = parameterAfter[0].VolCorrFactor;
                                parameterAfterObject.VolOfWaterLTRS = parameterAfter[0].VolOfWaterLTRS;

                                parameterAfterObject.SGtC_Lab = parameterAfter[0].SGtC_Lab;
                                parameterAfterObject.CrossVol_TkPcLTRS = parameterAfter[0].CrossVol_TkPcLTRS;


                            }

                            //get the tank
                            var tank = db.StorageTanks.Find(tankId);


                            //add parameters to discharge data object
                            dischargeDataObject.IsDischargeDataCreated = true;
                            dischargeDataObject.TankName = tank.TankNo;
                            dischargeDataObject.DischargeParameterBeforeObject = parameterBeforeObject;
                            dischargeDataObject.DischargeParameterAfterObject = parameterAfterObject;


                            return Json(dischargeDataObject, JsonRequestBehavior.AllowGet);
                        }

                    }

                    return Json(new NotificationDischageDataObject(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new NotificationDischageDataObject(), JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult AssignInspectorJob(EmployeeProfileObject employeeProfile)
        {

            var rep = new Reporter();
            try
            {


                if (employeeProfile == null)
                {
                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }


                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                    long employeeId;

                    if (employeeDesk.Any())
                    {
                        employeeId = employeeDesk[0].Id;


                        var notification =
                            db.Notifications.Where(n => n.Id == employeeProfile.ApplicationId).ToList();



                        if (notification.Any())
                        {

                            var notId = notification[0].Id;

                            //update the tracking stepcode to enable us get the next step
                            var track =
                                db.NotificationInspectionQueues.Where(
                                    p => p.NotificationId == notId && p.EmployeeId == employeeId).ToList();

                            if (track.Any())
                            {

                                //get the sequence of the current step

                                var stepnow = db.Steps.Find(track[0].StepId);

                             

                                var nextStepSequence = stepnow.SequenceNumber + 1;

                                //find out if this is the last step
                                var stepnext = db.Steps.Where(s => s.SequenceNumber == nextStepSequence && s.Process.ImportStageId == (int)AppStage.Notification).ToList();

                                if (stepnext.Any())
                                {
                                    var fsx = (int)nextStepSequence;
                                    var stepId = stepnext[0].Id;

                                 //assign job
                                    track[0].EmployeeId = employeeProfile.EmployeeId;
                                    track[0].StepId = stepId;
                                    track[0].AssignedTime = DateTime.Now;
                                    track[0].StepCode = fsx;

                                    var tracker = track[0];

                                    db.NotificationInspectionQueues.Attach(tracker);
                                    db.Entry(tracker).State = EntityState.Modified;


                                    //update employee job count

                                    var employeeDeskAssign = db.EmployeeDesks.Find(employeeProfile.EmployeeId);

                                    employeeDeskAssign.JobCount = employeeDeskAssign.JobCount + 1;
                                    employeeDeskAssign.NotificationCount = employeeDeskAssign.NotificationCount + 1;

                                    db.EmployeeDesks.Attach(employeeDeskAssign);
                                    db.Entry(employeeDeskAssign).State = EntityState.Modified;

                                
                                        ////add a history of work done 
                                        //var prohis = new NotificationHistory();
                                        //prohis.NotificationId = employeeProfile.ApplicationId;
                                        //prohis.AssignedTime = track[0].AssignedTime;
                                        //prohis.EmployeeId = employeeId;
                                        //prohis.FinishedTime = DateTime.Now;
                                        //prohis.StepId = track[0].StepId;
                                        //prohis.OutComeCode = (int)NotificationStatusEnum.Processing;
                                        //prohis.Remarks = employeeProfile.Description;

                                        //db.NotificationHistories.Add(prohis);

                                        //update employee job count

                                        employeeDesk[0].JobCount = employeeDesk[0].JobCount - 1;
                                        employeeDesk[0].NotificationCount = employeeDesk[0].NotificationCount - 1;

                                        var employee = employeeDesk[0];

                                        db.EmployeeDesks.Attach(employee);
                                        db.Entry(employee).State = EntityState.Modified;

                                        db.SaveChanges();


                                        rep.IsAccepted = true;
                                        return Json(rep, JsonRequestBehavior.AllowGet);

                                }


                                else
                                {
                                    //generate the document
                                    //update employee job count

                                    employeeDesk[0].JobCount = employeeDesk[0].JobCount - 1;
                                    employeeDesk[0].NotificationCount = employeeDesk[0].NotificationCount - 1;

                                    var employee = employeeDesk[0];

                                    db.EmployeeDesks.Attach(employee);
                                    db.Entry(employee).State = EntityState.Modified;

                                    track[0].StepCode = 0;
                                    track[0].StatusId = (int)NotificationStatusEnum.Approved;



                                    db.NotificationInspectionQueues.Attach(track[0]);
                                    db.Entry(track[0]).State = EntityState.Modified;


                                    //change application status
                                    notification[0].Status = (int)NotificationStatusEnum.Approved;
                                    db.Notifications.Attach(notification[0]);
                                    db.Entry(notification[0]).State = EntityState.Modified;




                                    //add a history of work done 
                                    var prohis = new NotificationHistory();
                                    prohis.NotificationId = employeeProfile.ApplicationId;
                                    prohis.AssignedTime = track[0].AssignedTime;
                                    prohis.EmployeeId = employeeId;
                                    prohis.FinishedTime = DateTime.Now;
                                    prohis.StepId = track[0].StepId;
                                    prohis.OutComeCode = (int)NotificationStatusEnum.Processing;
                                    prohis.Remarks = employeeProfile.Description;

                                    db.NotificationHistories.Add(prohis);

                                    db.SaveChanges();

                                    rep.IsCertificateGenerated = true;
                                    return Json(rep, JsonRequestBehavior.AllowGet);

                                }


                            }

                            rep.IsError = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);

                        }
                        rep.IsError = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);

                    }
                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);

            }
        }


      





       

        [HttpPost]
        public ActionResult SaveDischargeData(ParameterObject employeeProfile)
        {
            var rep = new Reporter();
            double? density = 0;
            rep.notid = employeeProfile.NotificationId;
            try
            {
                if (employeeProfile.NotificationId < 1)
                {
                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }

                using (var db = new ImportPermitEntities())
                {


                    //get the notification
                    var notification = db.Notifications.Where(n => n.Id == employeeProfile.NotificationId).ToList();
                    if (notification.Any())
                    {
                        var depotId = notification[0].DischargeDepotId;


                        var cert =
                            db.RecertificationResults.Where(r => r.NotificationId == employeeProfile.NotificationId)
                                .ToList();
                        if (cert.Any())
                        {
                            density = cert[0].Density;

                        }


                        var notificationVessel =
                            db.NotificationVessels.Where(n => n.NotificationId == employeeProfile.NotificationId)
                                .ToList();
                        if (notificationVessel.Any())
                        {
                            var vesselId = notificationVessel[0].VesselId;


                            //get the tankno

                            long tankId = 0;

                            var tank = db.StorageTanks.Where(s => s.TankNo.Equals(employeeProfile.TankNo)).ToList();

                            if (tank.Any())
                            {
                                tankId = tank[0].Id;
                            }
                            else
                            {
                                var newTank = new StorageTank();
                                newTank.TankNo = employeeProfile.TankNo;
                                newTank.DepotId = depotId;
                                newTank.ProductId = notification[0].ProductId;
                                newTank.UoMId = (int)EnumMeasurement.m3;
                                db.StorageTanks.Add(newTank);
                                db.SaveChanges();
                                tankId = newTank.Id;
                            }

                            //get the density

                         
                                    //save new  parameter before
                                    var newParameterBefore = new DischargeParameterBefore();
                                    newParameterBefore.NotificationId = employeeProfile.NotificationId;
                                    newParameterBefore.TankId = tankId;

                                    newParameterBefore.TankGuage = employeeProfile.TankGuageBefore;
                                    newParameterBefore.TankTC = employeeProfile.TankTCBefore;

                                    newParameterBefore.CrossVol_TkPcLTRS = employeeProfile.CrossVol_TkPcLTRSBefore;
                                    newParameterBefore.SGtC_Lab = employeeProfile.SGtC_LabBefore;
                                    newParameterBefore.VolOfWaterLTRS = employeeProfile.VolOfWaterLTRSBefore;
                                    newParameterBefore.VolCorrFactor = employeeProfile.VolCorrFactorBefore;
                                    newParameterBefore.SG_515C = density;

                                    newParameterBefore.NetVolOfOil_TkTc = employeeProfile.CrossVol_TkPcLTRSBefore - employeeProfile.VolOfWaterLTRSBefore;

                                    newParameterBefore.NetVol_1515C = newParameterBefore.NetVolOfOil_TkTc * employeeProfile.VolCorrFactorBefore;

                                    newParameterBefore.EquivVolInM_1515C = newParameterBefore.NetVol_1515C * density;
                                                                    

                                    //save new parameter after
                                    var newParameterAfter = new DischargeParameterAfter();
                                    newParameterAfter.NotificationId = employeeProfile.NotificationId;
                                    newParameterAfter.TankId = tankId;


                                    newParameterAfter.TankGuage = employeeProfile.TankGuageAfter;
                                    newParameterAfter.TankTC = employeeProfile.TankTCAfter;

                                    newParameterAfter.CrossVol_TkPcLTRS = employeeProfile.CrossVol_TkPcLTRSAfter;
                                    newParameterAfter.SGtC_Lab = employeeProfile.SGtC_LabAfter;
                                    newParameterAfter.VolOfWaterLTRS = employeeProfile.VolOfWaterLTRSAfter;
                                    newParameterAfter.VolCorrFactor = employeeProfile.VolCorrFactorAfter;
                                    newParameterAfter.SG_515C = density;

                                    newParameterAfter.NetVolOfOil_TkTc = employeeProfile.CrossVol_TkPcLTRSAfter - employeeProfile.VolOfWaterLTRSAfter;

                                    newParameterAfter.NetVol_1515C = newParameterAfter.NetVolOfOil_TkTc * employeeProfile.VolCorrFactorAfter;

                                    newParameterAfter.EquivVolInM_1515C = newParameterAfter.NetVol_1515C * density;
                                                                    

                                    db.DischargeParameterBefores.Add(newParameterBefore);
                                    db.DischargeParameterAfters.Add(newParameterAfter);
                                    db.SaveChanges();

                                    //save new  discharge data
                                    var newDischargeData = new NotificationDischageData();
                                    newDischargeData.NotificationId = employeeProfile.NotificationId;
                                    newDischargeData.DepotId = depotId;
                                    newDischargeData.VesselId = vesselId;
                                    newDischargeData.TankId = tankId;
                                    newDischargeData.DischargeParameterBeforeId = newParameterBefore.Id;
                                    newDischargeData.DischargeParameterAfterId = newParameterAfter.Id;

                                    db.NotificationDischageDatas.Add(newDischargeData);
                                    db.SaveChanges();

                                    //return all the discharge data objects

                                    //var datas = ReturnDischargeDatas(employeeProfile.NotificationId);
                            rep.IsAccepted = true;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                
                          
                        }
                        rep.IsError = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }
                    rep.IsError = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult UpdateDischargeData(ParameterObject employeeProfile)
        {
            var rep = new Reporter();

            rep.notid = employeeProfile.NotificationId;
            try
            {
                if (employeeProfile.NotificationId < 1)
                {
                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }

                using (var db = new ImportPermitEntities())
                {


                    //get the notification
                    var tank = db.StorageTanks.Where(n => n.TankNo.Equals(employeeProfile.TankNo)).ToList();
                    if (tank.Any())
                    {

                        var tankId = tank[0].Id;


                        var parameterBefore =
                            db.DischargeParameterBefores.Where(
                                p =>
                                    p.NotificationId == employeeProfile.NotificationId &&
                                    p.TankId == tankId).ToList();
                        if (parameterBefore.Any())
                        {

                            parameterBefore[0].TankGuage = employeeProfile.TankGuageBefore;

                            parameterBefore[0].TankTC = employeeProfile.TankTCBefore;

                            parameterBefore[0].CrossVol_TkPcLTRS =
                                employeeProfile.CrossVol_TkPcLTRSBefore;


                            parameterBefore[0].SGtC_Lab = employeeProfile.SGtC_LabBefore;

                            parameterBefore[0].VolOfWaterLTRS =
                                employeeProfile.VolOfWaterLTRSBefore;



                            parameterBefore[0].VolCorrFactor =
                                employeeProfile.VolCorrFactorBefore;

                            parameterBefore[0].NetVolOfOil_TkTc =
                                parameterBefore[0].CrossVol_TkPcLTRS -
                                parameterBefore[0].VolOfWaterLTRS;


                            parameterBefore[0].NetVol_1515C =
                                parameterBefore[0].NetVolOfOil_TkTc*
                                parameterBefore[0].VolCorrFactor;



                            parameterBefore[0].EquivVolInM_1515C =
                                parameterBefore[0].NetVol_1515C*parameterBefore[0].SG_515C;


                            db.DischargeParameterBefores.Attach(parameterBefore[0]);
                            db.Entry(parameterBefore[0]).State = EntityState.Modified;



                            var parameterAfter =
                                db.DischargeParameterAfters.Where(
                                    p =>
                                        p.NotificationId == employeeProfile.NotificationId &&
                                        p.TankId == tankId).ToList();

                            if (parameterAfter.Any())
                            {

                                parameterAfter[0].TankGuage =
                                    employeeProfile.TankGuageAfter;

                                parameterAfter[0].TankTC = employeeProfile.TankTCAfter;

                                parameterAfter[0].CrossVol_TkPcLTRS =
                                    employeeProfile.CrossVol_TkPcLTRSAfter;

                                parameterAfter[0].SGtC_Lab = employeeProfile.SGtC_LabAfter;

                                parameterAfter[0].VolOfWaterLTRS =
                                    employeeProfile.VolOfWaterLTRSAfter;



                                parameterAfter[0].VolCorrFactor =
                                    employeeProfile.VolCorrFactorAfter;

                                parameterAfter[0].NetVolOfOil_TkTc =
                                    parameterAfter[0].CrossVol_TkPcLTRS -
                                    parameterAfter[0].VolOfWaterLTRS;



                                parameterAfter[0].NetVol_1515C =
                                    parameterAfter[0].NetVolOfOil_TkTc*
                                    parameterAfter[0].VolCorrFactor;



                                parameterAfter[0].EquivVolInM_1515C =
                                    parameterAfter[0].NetVol_1515C*parameterAfter[0].SG_515C;


                                //paramBeforeObj.NetVolBal =
                                //    parameterAfterObj.NetVol_1515C -
                                //    paramBeforeObj.NetVol_1515C;


                                //paramBeforeObj.EquivVolBal =
                                //  parameterAfterObj.EquivVolInM_1515C -
                                //  paramBeforeObj.EquivVolInM_1515C;



                                db.DischargeParameterAfters.Attach(parameterAfter[0]);
                                db.Entry(parameterAfter[0]).State = EntityState.Modified;





                                db.SaveChanges();

                                //return all the discharge data objects

                                //var datas = ReturnDischargeDatas(employeeProfile.NotificationId);
                                rep.IsAccepted = true;
                                return Json(rep, JsonRequestBehavior.AllowGet);


                            }
                            rep.IsError = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }
                        rep.IsError = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }
                    rep.IsError = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult PrintDischargeData11(ParameterObject employeeProfile)
        {

            var way = new PObject();
            double? netVolTrack = 0;
            double vesselQuantity = 0;
            double? density = 0;
            
            try
            {
                Random rad = new Random();
                var radno = rad.Next();

                PdfDocument pdf = new PdfDocument();

                //Next step is to create a an Empty page.

                PdfPage pp = pdf.AddPage();

                pp.Size = PageSize.A4;
                pp.Orientation = PageOrientation.Landscape;

                string path = Path.Combine(Server.MapPath("~/InspectionDoc"), "discharge.pdf");

                //Then create an XGraphics Object

                XGraphics gfx = XGraphics.FromPdfPage(pp);

                XImage image = XImage.FromFile(path);
                gfx.DrawImage(image, 0, 0);




                XFont font = new XFont("Calibri", 10, XFontStyle.Regular);

                using (var db = new ImportPermitEntities())
                {
                    List<NotificationDischageData> dischargeList = new List<NotificationDischageData>();
                    List<DischargeParameterAfter> afterList = new List<DischargeParameterAfter>();
                    List<DischargeParameterBefore> beforeList = new List<DischargeParameterBefore>();

                    //get  the notification object
                    var notification = db.Notifications.Where(n => n.Id == employeeProfile.NotificationId).ToList();

                    var depotId = 0;
                   
                    if (notification.Any())
                    {
                        depotId = notification[0].DischargeDepotId;

                        
                        //get product
                        gfx.DrawString(notification[0].Product.Name, font, XBrushes.Black,
                            new XRect(280, 133, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //get arrival date
                        gfx.DrawString(notification[0].ArrivalDate.ToString("dd/MM/yy"), font, XBrushes.Black,
                            new XRect(280, 142, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                          //get vessel inspection
                        var vesselInspection =
                          db.NotificationInspections.Where(
                              r =>
                                  r.NotificationId == employeeProfile.NotificationId &&
                                  r.SubmittionStatus == (int)EnumCheckListOutComeStatus.Saved).ToList();
                        if (vesselInspection.Any())
                        {
                            gfx.DrawString(vesselInspection[0].DischargeCommencementDate.ToString(), font, XBrushes.Black,
                         new XRect(280, 152, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                            gfx.DrawString(vesselInspection[0].DischargeCompletionDate.ToString(), font, XBrushes.Black,
                        new XRect(280, 162, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                            
                        }

                     
                        //vessel arrival quantity
                        gfx.DrawString(notification[0].QuantityToDischarge.ToString(), font, XBrushes.Black,
                            new XRect(335, 483, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        vesselQuantity = notification[0].QuantityToDischarge;
                     
                        //get the consignee
                        gfx.DrawString(notification[0].Importer.Name, font, XBrushes.Black,
                            new XRect(335, 524, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }

                    //get tanks in the depot
                    var tanks = db.StorageTanks.Where(t => t.DepotId == depotId).ToList();

                    if (tanks.Any())
                    {
                        //get all the dischargedatas for the tanks in the depot that was filled
                        foreach (var item in tanks)
                        {
                            var dischargeData =
                           db.NotificationDischageDatas.Where(
                               d => d.NotificationId == employeeProfile.NotificationId &&
                                    d.TankId == item.Id)
                               .ToList();
                            if (dischargeData.Any())
                            {
                                dischargeList.Add(dischargeData[0]);
                            }

                            var parameterBefore =
                          db.DischargeParameterBefores.Where(
                              p =>
                                  p.NotificationId == employeeProfile.NotificationId &&
                                  p.TankId == item.Id).ToList();
                            if (parameterBefore.Any())
                            {
                                beforeList.Add(parameterBefore[0]);
                            }

                            var parameterAfter =
                          db.DischargeParameterAfters.Where(
                              p =>
                                  p.NotificationId == employeeProfile.NotificationId &&
                                  p.TankId == item.Id).ToList();

                            if (parameterAfter.Any())
                            {
                                afterList.Add(parameterAfter[0]);
                            }
                        }

                    }
                    else
                    {
                        way.NoCert = true;
                        return Json(way, JsonRequestBehavior.AllowGet);
                    }

                    var count = dischargeList.Count;

                  //get the density from recertification result
                    
                        var cert =
                            db.RecertificationResults.Where(r => r.NotificationId == employeeProfile.NotificationId)
                                .ToList();
                    if (cert.Any())
                    {
                        density = cert[0].Density;
                        
                    }

                    //first tank
                    if (1 <= count)
                    {
                        gfx.DrawString(dischargeList[0].StorageTank.TankNo, font, XBrushes.Black,
                            new XRect(103, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(afterList[0].TankGuage.ToString(), font, XBrushes.Black,
                            new XRect(163, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].TankGuage.ToString(), font, XBrushes.Black,
                            new XRect(163, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(afterList[0].TankTC.ToString(), font, XBrushes.Black,
                            new XRect(200, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].TankTC.ToString(), font, XBrushes.Black,
                            new XRect(200, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);

                        gfx.DrawString(afterList[0].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black,
                            new XRect(230, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black,
                            new XRect(230, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(afterList[0].SGtC_Lab.ToString(), font, XBrushes.Black,
                            new XRect(280, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].SGtC_Lab.ToString(), font, XBrushes.Black,
                            new XRect(280, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(afterList[0].VolOfWaterLTRS.ToString(), font, XBrushes.Black,
                            new XRect(335, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].VolOfWaterLTRS.ToString(), font, XBrushes.Black,
                            new XRect(335, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[0].CrossVol_TkPcLTRS - afterList[0].VolOfWaterLTRS;
                        var netVolBefore = beforeList[0].CrossVol_TkPcLTRS - beforeList[0].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black,
                            new XRect(385, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black,
                            new XRect(385, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);

                        //net vol
                        var volAfter = netOilVolAfter * afterList[0].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[0].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                       
                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;

                        //Density
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black,
                            new XRect(450, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black,
                            new XRect(450, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol corr factor
                        gfx.DrawString(afterList[0].VolCorrFactor.ToString(), font, XBrushes.Black,
                            new XRect(505, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].VolCorrFactor.ToString(), font, XBrushes.Black,
                            new XRect(505, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black,
                            new XRect(575, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black,
                            new XRect(575, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black,
                          new XRect(575, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;


                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black,
                            new XRect(638, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black,
                            new XRect(638, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black,
                          new XRect(638, 238, pp.Width.Point, pp.Height.Point),
                          XStringFormats.TopLeft);

                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 210, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    //2nd tank

                    if (2 <= count)
                    {
                        gfx.DrawString(dischargeList[1].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 250, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[1].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[1].CrossVol_TkPcLTRS - afterList[1].VolOfWaterLTRS;
                        var netVolBefore = beforeList[1].CrossVol_TkPcLTRS - beforeList[1].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 278, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 277, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 278, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                       

                        //net vol
                        var volAfter = netOilVolAfter * afterList[1].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[1].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;


                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 250, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 278, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 277, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 278, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                    }

                    //3rd tank
                    if (3 <= count)
                    {
                        gfx.DrawString(dischargeList[2].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 297, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[2].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                       
                        //vol of oil

                        var netOilVolAfter = afterList[2].CrossVol_TkPcLTRS - afterList[2].VolOfWaterLTRS;
                        var netVolBefore = beforeList[2].CrossVol_TkPcLTRS - beforeList[2].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                       

                        //net vol
                        var volAfter = netOilVolAfter * afterList[2].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[2].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;


                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 297, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }



                    //4th tank
                    if (4 <= count)
                    {
                        gfx.DrawString(dischargeList[3].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 344, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[3].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[3].CrossVol_TkPcLTRS - afterList[3].VolOfWaterLTRS;
                        var netVolBefore = beforeList[3].CrossVol_TkPcLTRS - beforeList[3].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        //net vol
                        var volAfter = netOilVolAfter * afterList[3].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[3].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;

                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 344, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }


                    //5th tank
                    if (5 <= count)
                    {
                        gfx.DrawString(dischargeList[4].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 384, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[4].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[4].CrossVol_TkPcLTRS - afterList[4].VolOfWaterLTRS;
                        var netVolBefore = beforeList[4].CrossVol_TkPcLTRS - beforeList[4].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        //net vol
                        var volAfter = netOilVolAfter * afterList[4].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[4].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;

                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 384, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }


                    //6th tank
                    if (6 <= count)
                    {
                        gfx.DrawString(dischargeList[5].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 426, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[5].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[5].CrossVol_TkPcLTRS - afterList[5].VolOfWaterLTRS;
                        var netVolBefore = beforeList[5].CrossVol_TkPcLTRS - beforeList[5].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        //net vol
                        var volAfter = netOilVolAfter * afterList[5].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[5].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;


                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 426, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }


                    //quantity received into shore tank
                    gfx.DrawString(netVolTrack.ToString(), font, XBrushes.Black,
                        new XRect(335, 496, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                    var shorepercent = (vesselQuantity - netVolTrack)/100;
                    gfx.DrawString(shorepercent.ToString(), font, XBrushes.Black,
                        new XRect(335, 511, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    //get the depot
                    var depot = db.Depots.Where(d => d.Id == depotId).ToList();

                    var portName = "";
                    if (depot.Any())
                    {
                        gfx.DrawString(depot[0].Name, font, XBrushes.Black,
                          new XRect(280, 110, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        var jettyId = depot[0].JettyId;

                        var jetty = db.Jetties.Find(jettyId);
                        portName = jetty.Port.Name;

                        var jettyMapping = db.JettyMappings.Where(j => j.JettyId == jettyId).ToList();

                        if (jettyMapping.Any())
                        {
                            //get the zone
                            gfx.DrawString(jettyMapping[0].Zone.Name, font, XBrushes.Black, new XRect(575, 75, pp.Width.Point, pp.Height.Point),
                         XStringFormats.TopLeft);
                        }

                    }

                    var permNum = "";

                    var numbe = employeeProfile.NotificationId;

                    var counting = numbe.ToString().Length;
                    if (counting == 1)
                    {
                        permNum = "00000" + numbe;
                    }
                    else if (counting == 2)
                    {
                        permNum = "0000" + numbe;
                    }
                    else if (counting == 3)
                    {
                        permNum = "000" + numbe;
                    }
                    else if (counting == 4)
                    {
                        permNum = "00" + numbe;
                    }
                    else if (counting == 5)
                    {
                        permNum = "0" + numbe;
                    }
                    else if (counting >= 6)
                    {
                        permNum = numbe.ToString();
                    }

                    var coq = "DPR/DS/PPIPS/" + portName + "/" + permNum;
                    //serial number
                    gfx.DrawString("CoQ Number:"+ " " + coq, font, XBrushes.Black, new XRect(560, 50, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);

                    //get the vessel
                    var notificationVessel =
                        db.NotificationVessels.Where(v => v.NotificationId == employeeProfile.NotificationId).ToList();

                    if (notificationVessel.Any())
                    {
                        var vesselId = notificationVessel[0].VesselId;

                        var vessel = db.Vessels.Where(v => v.VesselId == vesselId).ToList();

                        if (vessel.Any())
                        {
                            gfx.DrawString(vessel[0].Name, font, XBrushes.Black,
                           new XRect(280, 122, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        }
                    }

                    string path2 = Path.Combine(Server.MapPath("~/TempDoc"), "discharge2" + radno + ".pdf");


                    pdf.Save(path2);

                    way.SmallPath = @"\TempDoc\\" + "discharge2" + radno + ".pdf";
                    way.BigPath = Path.Combine(Server.MapPath("~/TempDoc"), "discharge2" + radno + ".pdf");


                    return Json(way, JsonRequestBehavior.AllowGet);


                }
            }
            catch (Exception ex)
            {
                way.Error = true;
                return Json(way, JsonRequestBehavior.AllowGet);

            }
        }


        [HttpPost]
        public ActionResult PrintDischargeData(ParameterObject employeeProfile)
        {
            var id = employeeProfile.NotificationId;

            var way = new PObject();
            double? netVolTrack = 0;
            double vesselQuantity = 0;
            double? density = 0;

            try
            {
                Random rad = new Random();
                var radno = rad.Next();

                PdfDocument pdf = new PdfDocument();

                //Next step is to create a an Empty page.

                PdfPage pp = pdf.AddPage();

                pp.Size = PageSize.A4;
                pp.Orientation = PageOrientation.Landscape;

                string path = Path.Combine(Server.MapPath("~/InspectionDoc"), "discharge.pdf");

                //Then create an XGraphics Object

                XGraphics gfx = XGraphics.FromPdfPage(pp);

                XImage image = XImage.FromFile(path);
                gfx.DrawImage(image, 0, 0);




                XFont font = new XFont("Calibri", 10, XFontStyle.Regular);

                using (var db = new ImportPermitEntities())
                {
                    List<NotificationDischageData> dischargeList = new List<NotificationDischageData>();
                    List<DischargeParameterAfter> afterList = new List<DischargeParameterAfter>();
                    List<DischargeParameterBefore> beforeList = new List<DischargeParameterBefore>();

                    //get  the notification object
                    var notification = db.Notifications.Where(n => n.Id == id).ToList();

                    var depotId = 0;

                    if (notification.Any())
                    {
                        depotId = notification[0].DischargeDepotId;


                        //get product
                        gfx.DrawString(notification[0].Product.Name, font, XBrushes.Black,
                            new XRect(280, 133, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //get arrival date
                        gfx.DrawString(notification[0].ArrivalDate.ToString("dd/MM/yy"), font, XBrushes.Black,
                            new XRect(280, 142, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //get vessel inspection
                        var vesselInspection =
                          db.NotificationInspections.Where(
                              r =>
                                  r.NotificationId == id &&
                                  r.SubmittionStatus == (int)EnumCheckListOutComeStatus.Submitted).ToList();
                        if (vesselInspection.Any())
                        {
                            gfx.DrawString(vesselInspection[0].DischargeCommencementDate.ToString(), font, XBrushes.Black,
                         new XRect(280, 152, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                            gfx.DrawString(vesselInspection[0].DischargeCompletionDate.ToString(), font, XBrushes.Black,
                        new XRect(280, 162, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        }


                        //vessel arrival quantity
                        gfx.DrawString(notification[0].QuantityToDischarge.ToString(), font, XBrushes.Black,
                            new XRect(335, 483, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        vesselQuantity = notification[0].QuantityToDischarge;

                        //get the consignee
                        gfx.DrawString(notification[0].Importer.Name, font, XBrushes.Black,
                            new XRect(335, 524, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }

                    //get tanks in the depot
                    var tanks = db.StorageTanks.Where(t => t.DepotId == depotId).ToList();

                    if (tanks.Any())
                    {
                        //get all the dischargedatas for the tanks in the depot that was filled
                        foreach (var item in tanks)
                        {
                            var dischargeData =
                           db.NotificationDischageDatas.Where(
                               d => d.NotificationId == id &&
                                    d.TankId == item.Id)
                               .ToList();
                            if (dischargeData.Any())
                            {
                                dischargeList.Add(dischargeData[0]);
                            }

                            var parameterBefore =
                          db.DischargeParameterBefores.Where(
                              p =>
                                  p.NotificationId == id &&
                                  p.TankId == item.Id).ToList();
                            if (parameterBefore.Any())
                            {
                                beforeList.Add(parameterBefore[0]);
                            }

                            var parameterAfter =
                          db.DischargeParameterAfters.Where(
                              p =>
                                  p.NotificationId == id &&
                                  p.TankId == item.Id).ToList();

                            if (parameterAfter.Any())
                            {
                                afterList.Add(parameterAfter[0]);
                            }
                        }

                    }
                    else
                    {
                        way.NoCert = true;
                        return Json(way, JsonRequestBehavior.AllowGet);
                    }

                    var count = dischargeList.Count;

                    //get the density from recertification result

                    var cert =
                        db.RecertificationResults.Where(r => r.NotificationId == id)
                            .ToList();
                    if (cert.Any())
                    {
                        density = cert[0].Density;

                    }

                    //first tank
                    if (1 <= count)
                    {
                        gfx.DrawString(dischargeList[0].StorageTank.TankNo, font, XBrushes.Black,
                            new XRect(103, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(afterList[0].TankGuage.ToString(), font, XBrushes.Black,
                            new XRect(163, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].TankGuage.ToString(), font, XBrushes.Black,
                            new XRect(163, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(afterList[0].TankTC.ToString(), font, XBrushes.Black,
                            new XRect(200, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].TankTC.ToString(), font, XBrushes.Black,
                            new XRect(200, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);

                        gfx.DrawString(afterList[0].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black,
                            new XRect(230, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black,
                            new XRect(230, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(afterList[0].SGtC_Lab.ToString(), font, XBrushes.Black,
                            new XRect(280, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].SGtC_Lab.ToString(), font, XBrushes.Black,
                            new XRect(280, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(afterList[0].VolOfWaterLTRS.ToString(), font, XBrushes.Black,
                            new XRect(335, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].VolOfWaterLTRS.ToString(), font, XBrushes.Black,
                            new XRect(335, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[0].CrossVol_TkPcLTRS - afterList[0].VolOfWaterLTRS;
                        var netVolBefore = beforeList[0].CrossVol_TkPcLTRS - beforeList[0].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black,
                            new XRect(385, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black,
                            new XRect(385, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);

                        //net vol
                        var volAfter = netOilVolAfter * afterList[0].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[0].VolCorrFactor;
                        var netVol = volAfter - volBefore;


                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;

                        //Density
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black,
                            new XRect(450, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black,
                            new XRect(450, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol corr factor
                        gfx.DrawString(afterList[0].VolCorrFactor.ToString(), font, XBrushes.Black,
                            new XRect(505, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].VolCorrFactor.ToString(), font, XBrushes.Black,
                            new XRect(505, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black,
                            new XRect(575, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black,
                            new XRect(575, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black,
                          new XRect(575, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;


                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black,
                            new XRect(638, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black,
                            new XRect(638, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black,
                          new XRect(638, 238, pp.Width.Point, pp.Height.Point),
                          XStringFormats.TopLeft);

                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 210, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    //2nd tank

                    if (2 <= count)
                    {
                        gfx.DrawString(dischargeList[1].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 250, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[1].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[1].CrossVol_TkPcLTRS - afterList[1].VolOfWaterLTRS;
                        var netVolBefore = beforeList[1].CrossVol_TkPcLTRS - beforeList[1].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 278, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 277, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 278, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                        //net vol
                        var volAfter = netOilVolAfter * afterList[1].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[1].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;


                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 250, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 278, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 277, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 278, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                    }

                    //3rd tank
                    if (3 <= count)
                    {
                        gfx.DrawString(dischargeList[2].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 297, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[2].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        //vol of oil

                        var netOilVolAfter = afterList[2].CrossVol_TkPcLTRS - afterList[2].VolOfWaterLTRS;
                        var netVolBefore = beforeList[2].CrossVol_TkPcLTRS - beforeList[2].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);




                        //net vol
                        var volAfter = netOilVolAfter * afterList[2].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[2].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;


                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 297, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }



                    //4th tank
                    if (4 <= count)
                    {
                        gfx.DrawString(dischargeList[3].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 344, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[3].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[3].CrossVol_TkPcLTRS - afterList[3].VolOfWaterLTRS;
                        var netVolBefore = beforeList[3].CrossVol_TkPcLTRS - beforeList[3].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        //net vol
                        var volAfter = netOilVolAfter * afterList[3].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[3].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;

                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 344, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }


                    //5th tank
                    if (5 <= count)
                    {
                        gfx.DrawString(dischargeList[4].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 384, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[4].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[4].CrossVol_TkPcLTRS - afterList[4].VolOfWaterLTRS;
                        var netVolBefore = beforeList[4].CrossVol_TkPcLTRS - beforeList[4].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        //net vol
                        var volAfter = netOilVolAfter * afterList[4].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[4].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;

                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 384, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }


                    //6th tank
                    if (6 <= count)
                    {
                        gfx.DrawString(dischargeList[5].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 426, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[5].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[5].CrossVol_TkPcLTRS - afterList[5].VolOfWaterLTRS;
                        var netVolBefore = beforeList[5].CrossVol_TkPcLTRS - beforeList[5].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        //net vol
                        var volAfter = netOilVolAfter * afterList[5].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[5].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;


                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 426, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }


                    //quantity received into shore tank
                    gfx.DrawString(netVolTrack.ToString(), font, XBrushes.Black,
                        new XRect(335, 496, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                    var shorepercent = (vesselQuantity - netVolTrack) / 100;
                    gfx.DrawString(shorepercent.ToString(), font, XBrushes.Black,
                        new XRect(335, 511, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    //get the depot
                    var depot = db.Depots.Where(d => d.Id == depotId).ToList();

                    var portName = "";
                    if (depot.Any())
                    {
                        gfx.DrawString(depot[0].Name, font, XBrushes.Black,
                          new XRect(280, 110, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        var jettyId = depot[0].JettyId;

                        var jetty = db.Jetties.Find(jettyId);
                        portName = jetty.Port.Name;

                        var jettyMapping = db.JettyMappings.Where(j => j.JettyId == jettyId).ToList();

                        if (jettyMapping.Any())
                        {
                            //get the zone
                            gfx.DrawString(jettyMapping[0].Zone.Name, font, XBrushes.Black, new XRect(575, 75, pp.Width.Point, pp.Height.Point),
                         XStringFormats.TopLeft);
                        }

                    }

                    var permNum = "";

                    var numbe = id;

                    var counting = numbe.ToString().Length;
                    if (counting == 1)
                    {
                        permNum = "00000" + numbe;
                    }
                    else if (counting == 2)
                    {
                        permNum = "0000" + numbe;
                    }
                    else if (counting == 3)
                    {
                        permNum = "000" + numbe;
                    }
                    else if (counting == 4)
                    {
                        permNum = "00" + numbe;
                    }
                    else if (counting == 5)
                    {
                        permNum = "0" + numbe;
                    }
                    else if (counting >= 6)
                    {
                        permNum = numbe.ToString();
                    }

                    var coq = "DPR/DS/PPIPS/" + portName + "/" + permNum;
                    //serial number
                    gfx.DrawString("CoQ Number:" + " " + coq, font, XBrushes.Black, new XRect(560, 50, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);

                    //get the vessel
                    var notificationVessel =
                        db.NotificationVessels.Where(v => v.NotificationId == id).ToList();

                    if (notificationVessel.Any())
                    {
                        var vesselId = notificationVessel[0].VesselId;

                        var vessel = db.Vessels.Where(v => v.VesselId == vesselId).ToList();

                        if (vessel.Any())
                        {
                            gfx.DrawString(vessel[0].Name, font, XBrushes.Black,
                           new XRect(280, 122, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        }
                    }



                    string path2 = Path.Combine(Server.MapPath("~/TempDoc"), "discharge2" + radno + ".pdf");


                    pdf.Save(path2);

                    way.SmallPath = @"\TempDoc\\" + "discharge2" + radno + ".pdf";
                    way.BigPath = Path.Combine(Server.MapPath("~/TempDoc"), "discharge2" + radno + ".pdf");


                    return Json(way, JsonRequestBehavior.AllowGet);


                }
            }
            catch (Exception ex)
            {
                way.Error = true;
                return Json(way, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public ActionResult SaveTankFile(HttpPostedFileBase file, long notificationId)
        {
            var rep = new Reporter();
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    //get the id of logged in user
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                   
                    if (file != null)
                    {
                        var notification = db.Notifications.Where(n => n.Id == notificationId).ToList();

                        if (notification.Any())
                        {
                            //get the employee id on employee desk table
                            var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                            if (employeeDesk.Any())
                            {
                                var doc = new EF.Model.Document();
                                doc.DocumentTypeId = (int)SpecialDocsEnum.Dry_Tank_Certificate;
                                doc.ImporterId = notification[0].ImporterId;
                                doc.UploadedById = employeeDesk[0].Id;
                                doc.DateUploaded = DateTime.Now;
                                doc.Status = (int) DocStatus.Valid;
                                var pic = Path.GetFileName(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/ImportDocuments"), pic);

                                // file is uploaded
                                file.SaveAs(path);
                                doc.DocumentPath = @"\ImportDocuments\" + file.FileName;
                                db.Documents.Add(doc);
                                db.SaveChanges();

                                var notDoc = new NotificationDocument();
                                notDoc.NotificationId = notificationId;
                                notDoc.DocumentId = doc.DocumentId;
                                notDoc.DateAttached = DateTime.Now;
                                db.NotificationDocuments.Add(notDoc);
                                db.SaveChanges();
                                rep.IsAccepted = true;
                                return Json(rep, JsonRequestBehavior.AllowGet);
                            }
                            rep.IsError = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }
                        rep.IsError = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }
                    rep.IsError = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                   
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);

                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult SaveTankFile2(HttpPostedFileBase file, long notificationId)
        {
            var rep = new Reporter();
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    //get the id of logged in user
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;


                    if (file != null)
                    {
                        var notification = db.Notifications.Where(n => n.Id == notificationId).ToList();

                        if (notification.Any())
                        {
                            //get the employee id on employee desk table
                            var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId == profileId).ToList();

                            if (employeeDesk.Any())
                            {
                                var doc = new EF.Model.Document();
                                doc.DocumentTypeId = (int)SpecialDocsEnum.ROB;
                                doc.ImporterId = notification[0].ImporterId;
                                doc.UploadedById = employeeDesk[0].Id;
                                doc.DateUploaded = DateTime.Now;
                                doc.Status = (int)DocStatus.Valid;
                                var pic = Path.GetFileName(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/ImportDocuments"), pic);

                                // file is uploaded
                                file.SaveAs(path);
                                doc.DocumentPath = @"\ImportDocuments\" + file.FileName;
                                db.Documents.Add(doc);
                                db.SaveChanges();

                                var notDoc = new NotificationDocument();
                                notDoc.NotificationId = notificationId;
                                notDoc.DocumentId = doc.DocumentId;
                                notDoc.DateAttached = DateTime.Now;
                                db.NotificationDocuments.Add(notDoc);
                                db.SaveChanges();
                                rep.IsAccepted = true;
                                return Json(rep, JsonRequestBehavior.AllowGet);
                            }
                            rep.IsError = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }
                        rep.IsError = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }
                    rep.IsError = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);

                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveVesselReport(NotificationInspectionObject employeeProfile)
        {
            var rep = new Reporter();
            try
            {


                if (employeeProfile == null)
                {
                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }


                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId.Equals(profileId)).ToList();

                    var notification = db.Notifications.Find(employeeProfile.NotificationId);

                    if (employeeDesk.Any())
                    {
                        long employeeId = employeeDesk[0].Id;

                        var cert =
                            db.NotificationInspections.Where(r => r.NotificationId == employeeProfile.NotificationId)
                                .ToList();
                        if (cert.Any())
                        {
                            cert[0].NotificationId = employeeProfile.NotificationId;
                            cert[0].QuantityOnVessel = employeeProfile.QuantityOnVessel;
                            //cert[0].ProductId = employeeProfile.ProductId;

                            cert[0].QuantityDischarged = employeeProfile.QuantityDischarged;
                            //cert[0].DepotId = employeeProfile.DepotId;
                            cert[0].EmployeeId = employeeId;
                            cert[0].StatusId = employeeProfile.StatusId;
                            cert[0].RecommendationId = employeeProfile.RecommendationId;

                            if (employeeProfile.InspectionDate != null)
                            {
                                cert[0].InspectionDate = employeeProfile.InspectionDate;

                                
                            }
                          

                          
                            cert[0].InspectorComment = employeeProfile.InspectorComment;

                            if (employeeProfile.DischargeCommencementDate != null)
                            {

                                cert[0].DischargeCommencementDate = employeeProfile.DischargeCommencementDate;

                                
                            }
                            

                            if (employeeProfile.DischargeCompletionDate != null)
                            {
                                cert[0].DischargeCompletionDate = employeeProfile.DischargeCompletionDate;

                               
                            }
                          
                           
                           
                            cert[0].QuantityOnBillOfLading = employeeProfile.QuantityOnBillOfLading;
                            cert[0].QuantityAfterSTS = employeeProfile.QuantityAfterSTS;
                            cert[0].LoadPortCoQAvailable = employeeProfile.LoadPortCoQAvailable;
                            cert[0].InspectionSubmittionDate = employeeProfile.InspectionSubmittionDate;

                            if (employeeProfile.VesselArrivalDate != null)
                            {
                                cert[0].VesselArrivalDate = employeeProfile.VesselArrivalDate;

                               
                            }
                           
                            


                            db.NotificationInspections.Attach(cert[0]);
                            db.Entry(cert[0]).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            var newcert = new NotificationInspection();


                            newcert.NotificationId = employeeProfile.NotificationId;
                            newcert.QuantityOnVessel = employeeProfile.QuantityOnVessel;
                            newcert.ProductId = notification.ProductId;

                            newcert.QuantityDischarged = employeeProfile.QuantityDischarged;
                            newcert.DepotId = notification.DischargeDepotId;
                            newcert.EmployeeId = employeeProfile.EmployeeId;
                            newcert.StatusId = employeeProfile.StatusId;
                            newcert.RecommendationId = employeeProfile.RecommendationId;

                            newcert.InspectionDate = employeeProfile.InspectionDate;
                            newcert.InspectorComment = employeeProfile.InspectorComment;
                            newcert.DischargeCommencementDate = employeeProfile.DischargeCommencementDate;

                            newcert.DischargeCompletionDate = employeeProfile.DischargeCompletionDate;
                            newcert.SubmittionStatus = (int)EnumCheckListOutComeStatus.Saved;
                            newcert.QuantityOnBillOfLading = employeeProfile.QuantityOnBillOfLading;
                            newcert.QuantityAfterSTS = employeeProfile.QuantityAfterSTS;
                            newcert.LoadPortCoQAvailable = employeeProfile.LoadPortCoQAvailable;
                            newcert.InspectionSubmittionDate = employeeProfile.InspectionSubmittionDate;
                            newcert.VesselArrivalDate = employeeProfile.VesselArrivalDate;



                            db.NotificationInspections.Add(newcert);
                            db.SaveChanges();

                        }



                        rep.IsAccepted = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);


                    }
                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult RetrieveSavedVesselReport(long id)
        {

            try
            {

                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;

                    //get the employee id on employee desk table
                    var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId.Equals(profileId)).ToList();

                    var notification = db.Notifications.Find(id);

                    if (employeeDesk.Any())
                    {
                        long employeeId = employeeDesk[0].Id;

                        var cert =
                            db.NotificationInspections.Where(
                                r =>
                                    r.NotificationId == id &&
                                    r.SubmittionStatus == (int)EnumCheckListOutComeStatus.Saved).ToList();

                        if (cert.Any())
                        {

                            var newcert = new NotificationInspectionObject();

                            newcert.NotificationId = cert[0].NotificationId;
                            newcert.QuantityOnVessel = cert[0].QuantityOnVessel;
                            newcert.ProductId = cert[0].ProductId;
                            newcert.ProductName = notification.Product.Name;

                            newcert.QuantityDischarged = cert[0].QuantityDischarged;
                            newcert.DepotId = cert[0].DepotId;
                            newcert.DepotName = notification.Depot.Name;
                            newcert.EmployeeId = cert[0].EmployeeId;
                            newcert.StatusId = cert[0].StatusId;
                            newcert.RecommendationId = cert[0].RecommendationId;

                            if (cert[0].InspectionDate != null)
                            {
                                newcert.InspectionDateStr = cert[0].InspectionDate.Value.ToString("dd/MM/yyyy");

                            }
                            newcert.InspectorComment = cert[0].InspectorComment;

                            if (cert[0].DischargeCommencementDate != null)
                            {
                                newcert.DischargeCommencementDateStr = cert[0].DischargeCommencementDate.Value.ToString("dd/MM/yyyy");

                            }

                            if (cert[0].DischargeCompletionDate != null)
                            {
                                newcert.DischargeCompletionDateStr = cert[0].DischargeCompletionDate.Value.ToString("dd/MM/yyyy");

                            }

                           
                            newcert.QuantityOnBillOfLading = cert[0].QuantityOnBillOfLading;
                            newcert.QuantityAfterSTS = cert[0].QuantityAfterSTS;
                            newcert.LoadPortCoQAvailable = cert[0].LoadPortCoQAvailable;
                            newcert.InspectionSubmittionDate = cert[0].InspectionSubmittionDate;

                            if (cert[0].VesselArrivalDate != null)
                            {
                                newcert.VesselArrivalDateStr = cert[0].VesselArrivalDate.Value.ToString("dd/MM/yyyy");

                            }
                           

                            return Json(newcert, JsonRequestBehavior.AllowGet);
                        }



                    }

                    return Json(new NotificationInspectionObject(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);

                return Json(new NotificationInspectionObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public string GetPdfNow(long id, long importerId, string permitValue, string companyName, DateTime dateGenerated)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var appItems = (from t in db.ApplicationItems.Include("Application")
                                where t.ApplicationId == id
                                orderby t.EstimatedValue descending

                                select t).ToList();

                    if (!appItems.Any())
                    {
                        return null;
                    }
                    var items = new List<ApplicationItemObject>();
                    appItems.ForEach(t =>
                    {
                        var im = new ApplicationItemObject()
                        {
                            Id = t.Id,
                            ProductName = t.Product.Name,
                            EstimatedQuantity = t.EstimatedQuantity,
                            EstimatedValue = t.EstimatedValue
                        };
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

                            items.Add(im);
                        }
                    });


                    var application = appItems[0].Application;

                    var address = new GeneralInformationServices().GetImporterAddress(application.ImporterId);
                    if (string.IsNullOrEmpty(address))
                    {
                        return null;
                    }

                    PdfDocument pdf = new PdfDocument();

                    //Next step is to create a an Empty page.

                    PdfPage pp = pdf.AddPage();


                    pp.Size = PageSize.A4;


                    string path = Path.Combine(Server.MapPath("~/PermDoc"), "perm.pdf");

                    //Then create an XGraphics Object

                    XGraphics gfx = XGraphics.FromPdfPage(pp);

                    XImage image = XImage.FromFile(path);
                    gfx.DrawImage(image, 0, 0);


                    XFont font = new XFont("Calibri", 12, XFontStyle.Regular);
                    XFont font2 = new XFont("Calibri", 8, XFontStyle.Regular);


                    gfx.DrawString(permitValue, font, XBrushes.Black,
                        new XRect(362, 162, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                    gfx.DrawString(application.Importer.Name, font, XBrushes.Black,
                        new XRect(80, 188, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    gfx.DrawString(address, font, XBrushes.Black,
                        new XRect(92, 217, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                    MigraDoc.DocumentObjectModel.Document doc = new MigraDoc.DocumentObjectModel.Document();


                    Section section = doc.AddSection();

                    var table = section.AddTable();

                    //table = section.AddTable();
                    table.Style = "Table";

                    table.Borders.Width = 0.25;
                    table.Borders.Left.Width = 0.5;
                    table.Borders.Right.Width = 0.5;
                    table.Rows.LeftIndent = 0;

                    Column column = table.AddColumn("4cm");
                    column.Format.Alignment = ParagraphAlignment.Center;


                    column = table.AddColumn("4cm");
                    column.Format.Alignment = ParagraphAlignment.Center;

                    column = table.AddColumn("4cm");
                    column.Format.Alignment = ParagraphAlignment.Center;

                    column = table.AddColumn("4cm");
                    column.Format.Alignment = ParagraphAlignment.Center;

                    // Create the header of the table
                    Row row = table.AddRow();
                    //row = table.AddRow();
                    row.HeadingFormat = true;
                    row.Format.Alignment = ParagraphAlignment.Center;
                    //row.Format.Font.Bold = true;
                    row.Format.Font.Size = 12;

                    row.Cells[0].AddParagraph("Type of Petroleum Product:");
                    row.Cells[0].Format.Alignment = ParagraphAlignment.Left;


                    row.Cells[1].AddParagraph("Country of origin:");
                    row.Cells[1].Format.Alignment = ParagraphAlignment.Left;

                    row.Cells[2].AddParagraph("Quantity/Weight (Metric Tones):");
                    row.Cells[2].Format.Alignment = ParagraphAlignment.Left;

                    row.Cells[3].AddParagraph("Estimated Value($):");
                    row.Cells[3].Format.Alignment = ParagraphAlignment.Left;

                    double total = 0;

                    if (items.Any() && items.Count() <= 7)
                    {
                        foreach (var item in items.ToList())
                        {
                            row = table.AddRow();
                            row.Format.Font.Size = 12;
                            row.Cells[0].AddParagraph(item.ProductName);
                            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

                            total = total + item.EstimatedValue;

                            row.Cells[1].AddParagraph(item.CountryOfOriginName);
                            row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                            row.Cells[2].AddParagraph(item.EstimatedQuantity.ToString("n0"));
                            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
                            row.Cells[3].AddParagraph(item.EstimatedValue.ToString("n0"));
                            row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
                        }
                        row = table.AddRow();
                        row.Format.Font.Bold = true;
                        row.Cells[2].AddParagraph("Total");
                        row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
                        row.Cells[3].AddParagraph(total.ToString("n0"));
                        row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
                    }
                    else if (items.Any() && items.Count() > 7)
                    {


                        foreach (var item in items.ToList())
                        {
                            row = table.AddRow();
                            row.Format.Font.Size = 8;
                            row.Cells[0].AddParagraph(item.ProductName);
                            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

                            total = total + item.EstimatedValue;

                            row.Cells[1].AddParagraph(item.CountryOfOriginName);
                            row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                            row.Cells[2].AddParagraph(item.EstimatedQuantity.ToString("n0"));
                            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
                            row.Cells[3].AddParagraph(item.EstimatedValue.ToString("n0"));
                            row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
                        }
                        row = table.AddRow();
                        row.Format.Font.Bold = true;
                        row.Format.Font.Size = 12;
                        row.Cells[2].AddParagraph("Total");
                        row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
                        row.Cells[3].AddParagraph(total.ToString("n0"));
                        row.Cells[3].Format.Alignment = ParagraphAlignment.Left;

                    }


                    //convert total amount to words

                    LogicObject log = new LogicObject();

                    var amtWords = log.ChangeToWords(total.ToString(CultureInfo.InvariantCulture), true);


                    const bool unicode = false;
                    const PdfFontEmbedding embedding = PdfFontEmbedding.Always;

                    PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);

                    // Associate the MigraDoc document with a renderer

                    pdfRenderer.Document = doc;


                    // Layout and render document to PDF

                    pdfRenderer.RenderDocument();

                    var pathtable = Path.Combine(Server.MapPath("~/PermDoc"), "table.pdf");

                    pdfRenderer.PdfDocument.Save(pathtable);


                    XImage imagetable = XImage.FromFile(Path.Combine(Server.MapPath("~/PermDoc"), "table.pdf"));
                    //gfx.DrawImage(imagetable, 0, 210);
                    gfx.DrawImage(imagetable, 20, 250, 550, 500);


                    gfx.DrawString(amtWords, font, XBrushes.Black, new XRect(150, 512, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);

                    var fee = 
                        db.Fees.Where(
                            f =>
                                f.ImportStageId == (int)AppStage.Application &&
                                f.FeeTypeId == (int)FeeTypeEnum.Processing_Fee).ToList();
                    if (fee.Any())
                    {
                        var feeAmount = fee[0].Amount.ToString("n0");
                        gfx.DrawString("N" + feeAmount, font, XBrushes.Black, new XRect(255, 550, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }

                    var fee2 =
                        db.Fees.Where(
                            f =>
                                f.ImportStageId == (int)AppStage.Application &&
                                f.FeeTypeId == (int)FeeTypeEnum.Statutory_Fee).ToList();
                    if (fee.Any())
                    {
                        var feeAmount = fee2[0].Amount.ToString("n0");
                        gfx.DrawString("N" + feeAmount, font, XBrushes.Black, new XRect(235, 590, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                    }

                    gfx.DrawString(DateTime.Now.ToString("MMMM dd yyyy"), font, XBrushes.Black,new XRect(120, 630, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                    
                    gfx.DrawString(dateGenerated.ToString("MMMM dd yyyy"), font, XBrushes.Black,new XRect(390, 630, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                    
                    var path2 = SaveTempPermit(pdf, importerId);
                    if (string.IsNullOrEmpty(path2))
                    {
                        return null;
                    }

                    return path2;
                }
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }


        }

        private static string GenerateUniqueName()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        private string SaveTempPermit(PdfDocument pdf, long importerId)
        {
            try
            {
                var fileName = GenerateUniqueName() + ".pdf";
                var path = HostingEnvironment.MapPath("~/PermDoc/" + importerId.ToString(CultureInfo.InvariantCulture));
                if (string.IsNullOrEmpty(path))
                {
                    return "";
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    var dInfo = new DirectoryInfo(path);
                    var dSecurity = dInfo.GetAccessControl();
                    dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                    dInfo.SetAccessControl(dSecurity);
                }

                var dir = new DirectoryInfo(path);
                var files = dir.GetFiles();
                if (files.Any())
                {
                    files.ForEach(j =>
                    {
                        System.IO.File.Delete(j.FullName);
                    });
                }

                var newPath = Path.Combine(path, fileName);
                pdf.Save(newPath);
                return PhysicalToVirtualPathMapper.MapPath(newPath).Replace("~", "");
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return string.Empty;
            }
        }

        public bool GetRecertPdf(string permitNo, string companyName, string motherVessel, string shuttleVessel, string product, string jetty, string commence, string finish, long notId)
        {
            try
            {
                var r = new Random();
                var radno = r.Next();
                using (var db = new ImportPermitEntities())
                {

                    var items = from t in db.NotificationInspections
                                where t.NotificationId == notId
                              

                                select new NotificationInspectionObject()
                                {
                                    Id = t.Id,
                                    DepotName = t.Depot.Name,
                                    QuantityDischargedStr = t.QuantityDischarged.ToString(),
                                    QuantityDischarged = t.QuantityDischarged
                                   
                                };



                    var notification = db.Notifications.Find(notId);
                    var permitId = notification.PermitId;

                    var permVal = "";
                    var permit = db.Permits.Where(p => p.Id == permitId).ToList();
                    if (permit.Any())
                    {
                        permVal = permit[0].PermitValue;
                    }

                    PdfDocument pdf = new PdfDocument();

                    //Next step is to create a an Empty page.

                    PdfPage pp = pdf.AddPage();


                    pp.Size = PageSize.A4;




                    string path = Path.Combine(Server.MapPath("~/PermDoc"), "recert.pdf");

                    //Then create an XGraphics Object

                    XGraphics gfx = XGraphics.FromPdfPage(pp);

                    XImage image = XImage.FromFile(path);
                    gfx.DrawImage(image, 0, 0);




                    XFont font = new XFont("Calibri", 12, XFontStyle.Regular);
                    XFont font2 = new XFont("Calibri", 8, XFontStyle.Regular);


                    gfx.DrawString("refNo", font, XBrushes.Black,
                        new XRect(352, 182, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                    gfx.DrawString(DateTime.Now.ToString("dd/MM/yyyy"), font, XBrushes.Black,
                        new XRect(352, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    gfx.DrawString(permVal, font, XBrushes.Black,
                        new XRect(342, 410, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                    MigraDoc.DocumentObjectModel.Document doc = new MigraDoc.DocumentObjectModel.Document();



                    Section section = doc.AddSection();

                    var table = section.AddTable();



                    //table = section.AddTable();
                    table.Style = "Table";

                    table.Borders.Width = 0;
                    table.Borders.Left.Width = 0;
                    table.Borders.Right.Width = 0;
                    table.Rows.LeftIndent = 0;

                    Column column = table.AddColumn("16cm");
                    column.Format.Alignment = ParagraphAlignment.Center;


                  
                    // Create the header of the table
                    Row row = table.AddRow();
                    
                   
                    row.Cells[0].AddParagraph("We hereby certify that" +" "+ companyName +" "+ "through"+ " "+ motherVessel + "(" +
                                              shuttleVessel + ")"+" "+" delivered the following quantity of"+" " + product +" "+
                                              "in the facility of the listed company at " + jetty + ","+" " +" from " + commence +
                                              "-" + finish);

                    row.Cells[0].Format.Alignment = ParagraphAlignment.Left;


                    row = table.AddRow();
                    row = table.AddRow();


                    row = table.AddRow();

                    //get bank name
                   
                    var permApp = db.PermitApplications.Where(p => p.PermitId == permitId).ToList();

                    long appId = 0;
                    if (permApp.Any())
                    {
                        appId = permApp[0].ApplicationId;
                       
                    }

                    var application = db.Applications.Where(a => a.Id == appId).Include("ApplicationItems").ToList();

                    var bank = "";
                    if (application.Any())
                    {
                        var aps = application[0].ApplicationItems.ToList();

                        aps.ForEach(n =>
                        {
                            var bankers = db.ProductBankers.Where(s => s.ApplicationItemId == n.Id).Include("Bank").ToList();
                            if (bankers.Any())
                            {
                                bankers.ForEach(q =>
                                {
                                    if (string.IsNullOrEmpty(bank))
                                    {
                                        bank = q.Bank.Name;
                                    }
                                    else
                                    {
                                        bank += ", " + q.Bank.Name;
                                    }
                                });
                            }
                        });
                        
                    }
                 

                    row.Cells[0].AddParagraph(bank +" "+"is/are responsible for the banking transaction in respect of this importation.");


                    row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

                    row = table.AddRow();
                    row = table.AddRow();

                    row = table.AddRow();

                    row.Cells[0].AddParagraph(
                        "We also certify that the product delivered met specifications stipulated under the guidelines and was therefore accepted.");

                    row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

                    row = table.AddRow();
                    row = table.AddRow();

                    //add another table

                    var table2 = section.AddTable();

                    //table = section.AddTable();
                    table2.Style = "Table";

                    table2.Borders.Width = 0.25;
                    table2.Borders.Left.Width = 0.5;
                    table2.Borders.Right.Width = 0.5;
                    table2.Rows.LeftIndent = 0;

                    Column column2 = table2.AddColumn("2cm");
                    column2.Format.Alignment = ParagraphAlignment.Center;


                    column2 = table2.AddColumn("8cm");
                    column2.Format.Alignment = ParagraphAlignment.Center;

                    column2 = table2.AddColumn("6cm");
                    column2.Format.Alignment = ParagraphAlignment.Center;

                  


                    // Create the header of the table
                    Row row2 = table2.AddRow();
                    //row = table.AddRow();
                    row2.HeadingFormat = true;
                    row2.Format.Alignment = ParagraphAlignment.Center;
                    row2.Format.Font.Bold = true;
                    row2.Format.Font.Size = 12;

                    row2.Cells[0].AddParagraph("S/A:");
                    row2.Cells[0].Format.Alignment = ParagraphAlignment.Left;


                    row2.Cells[1].AddParagraph("COMPANY'S FACILITY:");
                    row2.Cells[1].Format.Alignment = ParagraphAlignment.Left;

                    row2.Cells[2].AddParagraph("QTY. DISCHARGED IN M/T:");
                    row2.Cells[2].Format.Alignment = ParagraphAlignment.Left;




                    double? total = 0;
                 
                    var i = 1;

                    foreach (var item in items)
                    {

                        row2 = table2.AddRow();
                        row2.Format.Font.Bold = true;
                       
                        row2.Cells[0].AddParagraph(i.ToString());
                        row2.Cells[0].Format.Alignment = ParagraphAlignment.Left;
                        row2.Cells[1].AddParagraph(item.DepotName);
                        row2.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                        row2.Cells[2].AddParagraph(item.QuantityDischargedStr);
                        row2.Cells[2].Format.Alignment = ParagraphAlignment.Left;

                        total = total + item.QuantityDischarged;

                        i = i + 1;
                    }

                    row2 = table2.AddRow();
                    row2.Format.Font.Bold = true;
                    row2.Cells[1].AddParagraph("Total");
                    row2.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                    row2.Cells[2].AddParagraph(Convert.ToString(total));
                    row2.Cells[2].Format.Alignment = ParagraphAlignment.Left;

                    

                    const bool unicode = false;
                    const PdfFontEmbedding embedding = PdfFontEmbedding.Always;

                    PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);

                    // Associate the MigraDoc document with a renderer

                    pdfRenderer.Document = doc;



                    // Layout and render document to PDF

                    pdfRenderer.RenderDocument();



                    var pathtable = Path.Combine(Server.MapPath("~/PermDoc"), "recertTable.pdf");

                    pdfRenderer.PdfDocument.Save(pathtable);


                    XImage imagetable = XImage.FromFile(Path.Combine(Server.MapPath("~/PermDoc"), "recertTable.pdf"));

                    //gfx.DrawImage(imagetable, 0, 210);
                    gfx.DrawImage(imagetable, 20, 400, 550, 500);


                    var sign = XImage.FromFile(Path.Combine(Server.MapPath("~/PermDoc"), "director.png"));

                    //gfx.DrawImage(imagetable, 0, 210);
                    gfx.DrawImage(sign, 30, 705, 35, 37);


                    string path2 = Path.Combine(Server.MapPath("~/PermDoc"), "recert" + radno + ".pdf");


                    pdf.Save(path2);


                    return true;

                }
            }

            catch (Exception ex)
            {
                return false;
            }


        }

      
        public string PrintRecertification(int id)
        {
            try
            {

                using (var db = new ImportPermitEntities())
                {

                    var cert = db.RecertificationResults.Where(r => r.NotificationId == id).ToList();

                    if (cert.Any())
                    {


                        PdfDocument pdf = new PdfDocument();

                        //Next step is to create a an Empty page.

                        PdfPage pp = pdf.AddPage();



                        string path = Path.Combine(Server.MapPath("~/InspectionDoc"), "VesselReport.pdf");

                        //Then create an XGraphics Object

                        XGraphics gfx = XGraphics.FromPdfPage(pp);

                        XImage image = XImage.FromFile(path);
                        gfx.DrawImage(image, 0, 0);
                        XFont font = new XFont("Calibri", 10, XFontStyle.Regular);

                        //get notification
                        var notification = db.Notifications.Where(n => n.Id == id).ToList();
                        if (notification.Any())
                        {
                            //get product
                            gfx.DrawString(notification[0].Product.Name, font, XBrushes.Black, new XRect(200, 245, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                            var depotId = notification[0].DischargeDepotId;
                            //get the depot
                            var depot = db.Depots.Where(d => d.Id == depotId).ToList();

                            if (depot.Any())
                            {
                                gfx.DrawString(depot[0].Name, font, XBrushes.Black, new XRect(230, 337, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                                var jettyId = depot[0].JettyId;

                                var jetty = db.Jetties.Where(j => j.Id == jettyId).ToList();

                                //get the jetty
                                if (jetty.Any())
                                {
                                    gfx.DrawString(jetty[0].Name, font, XBrushes.Black, new XRect(80, 200, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                                    //get the port
                                    var portId = jetty[0].PortId;

                                    var port = db.Ports.Where(p => p.Id == portId).ToList();

                                    if (port.Any())
                                    {
                                        gfx.DrawString(port[0].Name, font, XBrushes.Black, new XRect(430, 245, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                                    }
                                }

                                //get permitId if any
                                var permNo = notification[0].Permit.PermitValue;

                                if (String.IsNullOrEmpty(permNo))
                                {

                                    gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 265, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                                }
                                else
                                {
                                    gfx.DrawString(permNo, font, XBrushes.Black, new XRect(230, 265, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                                }
                            }

                            //get the consognee
                            gfx.DrawString(notification[0].Importer.Name, font, XBrushes.Black, new XRect(215, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        }

                      

                        //get the vessel
                        var notificationVessel =
                            db.NotificationVessels.Where(v => v.NotificationId == id).ToList();

                        if (notificationVessel.Any())
                        {
                            var vesselId = notificationVessel[0].VesselId;

                            var vessel = db.Vessels.Where(v => v.VesselId == vesselId).ToList();

                            if (vessel.Any())
                            {

                                gfx.DrawString(vessel[0].Name, font, XBrushes.Black, new XRect(200, 230, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                            }
                        }



                        gfx.DrawString("allfast", font, XBrushes.Black, new XRect(430, 230, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        //get vessel inspection
                        var vesselInspection =
                          db.NotificationInspections.Where(
                              r =>
                                  r.NotificationId == id &&
                                  r.SubmittionStatus == (int)EnumCheckListOutComeStatus.Submitted).ToList();
                        if (vesselInspection.Any())
                        {
                            //bill of lading
                            gfx.DrawString(vesselInspection[0].QuantityOnBillOfLading.ToString(), font, XBrushes.Black, new XRect(230, 282, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                            //sts
                            gfx.DrawString(vesselInspection[0].QuantityAfterSTS.ToString(), font, XBrushes.Black, new XRect(230, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                            //arrival quantity
                            gfx.DrawString(vesselInspection[0].QuantityDischarged.ToString(), font, XBrushes.Black, new XRect(230, 317, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                            //recertification date
                            if (vesselInspection[0].InspectionDate != null)
                            {
                                gfx.DrawString(vesselInspection[0].InspectionDate.Value.ToString("dd/MM/yyyy"), font, XBrushes.Black, new XRect(370, 200, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                            }

                            //load port quality cert
                            if (vesselInspection[0].LoadPortCoQAvailable == "1")
                            {
                                gfx.DrawString("Yes", font, XBrushes.Black, new XRect(290, 377, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                            }
                            else if (vesselInspection[0].LoadPortCoQAvailable == "2")
                            {
                                gfx.DrawString("No", font, XBrushes.Black, new XRect(290, 377, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                            }
                            else if (vesselInspection[0].LoadPortCoQAvailable == "")
                            {
                                gfx.DrawString("", font, XBrushes.Black, new XRect(290, 377, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                            }

                           

                        }


                        //account
                        gfx.DrawString("", font, XBrushes.Black, new XRect(415, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                       
                        //density
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(250, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //flaspoint
                        gfx.DrawString(cert[0].Flashpoint.ToString(), font, XBrushes.Black, new XRect(180, 430, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //colour
                        if (cert[0].ProductColour != null)
                        {
                            var cId = cert[0].ProductColour;
                            var colour = db.ProductColours.Find(cId);
                            gfx.DrawString(colour.Name, font, XBrushes.Black, new XRect(350, 430, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        }
                        //1st boiling
                        gfx.DrawString(cert[0].InitialBoilingPoint.ToString(), font, XBrushes.Black, new XRect(210, 447, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //final boiling point
                        gfx.DrawString(cert[0].FinalBoilingPoint.ToString(), font, XBrushes.Black, new XRect(430, 447, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //sulphur
                        gfx.DrawString(cert[0].TotalSulphur.ToString(), font, XBrushes.Black, new XRect(230, 467, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //acid
                        gfx.DrawString(cert[0].TotalAcidity.ToString(), font, XBrushes.Black, new XRect(400, 467, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //RON
                        gfx.DrawString(cert[0].ResearchOctaneNumber.ToString(), font, XBrushes.Black, new XRect(250, 485, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //RVP
                        gfx.DrawString(cert[0].REIDVapourPressure.ToString(), font, XBrushes.Black, new XRect(450, 485, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //benzene
                        gfx.DrawString(cert[0].Benzene.ToString(), font, XBrushes.Black, new XRect(160, 505, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //ethanol
                        gfx.DrawString(cert[0].Ethanol.ToString(), font, XBrushes.Black, new XRect(300, 505, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //diesel
                        gfx.DrawString(cert[0].DieselIndex.ToString(), font, XBrushes.Black, new XRect(450, 505, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //freezepoint
                        gfx.DrawString(cert[0].FreezingPoint.ToString(), font, XBrushes.Black, new XRect(170, 525, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //msep
                        gfx.DrawString(cert[0].MSEP.ToString(), font, XBrushes.Black, new XRect(300, 525, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //doctor test
                        gfx.DrawString(cert[0].DoctorTest.ToString(), font, XBrushes.Black, new XRect(450, 525, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //limit variance
                        if (cert[0].LimitVariance == "1")
                        {
                            //with limit variance
                            gfx.DrawString("Yes", font, XBrushes.Black, new XRect(430, 557, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                            //off limit variance
                            gfx.DrawString("No", font, XBrushes.Black, new XRect(175, 560, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        }
                        else if (cert[0].LimitVariance == "2")
                        {
                            //with limit variance
                            gfx.DrawString("No", font, XBrushes.Black, new XRect(430, 557, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                            //off limit variance
                            gfx.DrawString("Yes", font, XBrushes.Black, new XRect(175, 560, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        }

                        //spec
                        if (cert[0].Spec == "11")
                        {
                            //on spec
                            gfx.DrawString("Yes", font, XBrushes.Black, new XRect(320, 580, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                            //off spec
                            gfx.DrawString("No", font, XBrushes.Black, new XRect(430, 580, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        }
                        else if (cert[0].Spec == "22")
                        {
                            //on spec
                            gfx.DrawString("No", font, XBrushes.Black, new XRect(320, 580, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                            //off spec
                            gfx.DrawString("Yes", font, XBrushes.Black, new XRect(430, 580, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        }

                        //approval
                        if (cert[0].DischargeApproval == "111")
                        {
                            gfx.DrawString("Granted", font, XBrushes.Black, new XRect(370, 595, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        }
                        else if (cert[0].DischargeApproval == "222")
                        {
                            gfx.DrawString("Not Granted", font, XBrushes.Black, new XRect(370, 595, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        }


                        //captain
                        if (cert[0].CaptainName != null)
                        {
                           gfx.DrawString(cert[0].CaptainName, font, XBrushes.Black, new XRect(370, 640, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        }

                        string path2 = Path.Combine(Server.MapPath("~/InspectionDoc"), "VesselReport2.pdf");


                        pdf.Save(path2);

                        const string way = @"\InspectionDoc\\" + "VesselReport2.pdf";

                        return way;

                    }


                    return "No";

                }
            }
            catch (Exception ex)
            {
                //return Json(ex.Message, JsonRequestBehavior.AllowGet);
                return ex.Message;

            }


        }

        [HttpGet]
        public ActionResult SubmitReport(long id)
        {
            var rep = new Reporter();
            try
            {

                using (var db = new ImportPermitEntities())
                {

                    var notification = db.Notifications.Where(n => n.Id == id).ToList();

                    if (notification.Any())
                    {
                        var depotId = notification[0].DischargeDepotId;
                        //check if checklist is submitted
                        var doc = db.NotificationDocuments.Where(o => o.NotificationId == id).ToList();

                        if (doc.Any())
                        {

                            foreach (var item in doc)
                            {
                                var normalDoc = db.Documents.Where(m => m.DocumentId == item.DocumentId).ToList();
                                if (normalDoc.Any())
                                {
                                    var norm = normalDoc[0];

                                    if (norm.DocumentTypeId == (int)SpecialDocsEnum.Dry_Tank_Certificate || norm.DocumentTypeId == (int)SpecialDocsEnum.ROB)
                                    {
                                        norm.IsValid = "True";
                                        
                                    }

                                    if (norm.IsValid == "False" || norm.IsValid == null)
                                    {
                                        rep.IsChecklistSaved = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }

                                   
                                }
                               
                            }
                            rep.IsChecklistSaved = true;

                            //check if vessel report is submitted
                           
                            var notInspec = db.NotificationInspections.Where(o => o.NotificationId == id).ToList();
                          
                            if (notInspec.Any())
                            {
                               
                                var dischargedate = Convert.ToDateTime(notInspec[0].DischargeCompletionDate);

                                if (string.IsNullOrEmpty(notInspec[0].QuantityOnVessel.ToString()))
                                {
                                    rep.IsVesselReport = false;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }
                                if (string.IsNullOrEmpty(notInspec[0].QuantityOnBillOfLading.ToString()))
                                {
                                    rep.IsVesselReport = false;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }
                                if (string.IsNullOrEmpty(notInspec[0].InspectionDate.ToString()))
                                {
                                    rep.IsVesselReport = false;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }

                                rep.IsVesselReport = true;

                                var recert = db.RecertificationResults.Where(r => r.NotificationId == id).ToList();
                                if (recert.Any())
                                {
                                    if (string.IsNullOrEmpty(recert[0].Density.ToString()))
                                    {
                                        rep.IsRecertificationSaved = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }
                                    if (string.IsNullOrEmpty(recert[0].DischargeApproval))
                                    {
                                        rep.IsRecertificationSaved = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }

                                    rep.IsRecertificationSaved = true;


                                    //check if discharge data is submitted
                                    var discharge = db.NotificationDischageDatas.Where(d => d.NotificationId == id).ToList();

                                    if (discharge.Any())
                                    {
                                        rep.IsDischargeDataSaved = true;

                                      

                                          var dischargeListObj = new List<NotificationDischageDataObject>();
                                       

                                        //get tanks in the depot
                                        var tanks = db.StorageTanks.Where(t => t.DepotId == depotId).ToList();

                                        if (tanks.Any())
                                        {
                                            //get all the dischargedatas for the tanks in the depot that was filled
                                            foreach (var item in tanks)
                                            {
                                                var dischargeData =
                                               db.NotificationDischageDatas.Where(
                                                   d => d.NotificationId == id &&
                                                        d.TankId == item.Id)
                                                   .ToList();
                                                if (dischargeData.Any())
                                                {
                                                    var dischargeObj = new NotificationDischageDataObject();

                                                    dischargeObj.TankName = dischargeData[0].StorageTank.TankNo;
                                                    dischargeObj.NotificationId = dischargeData[0].NotificationId;


                                                    //get the density from recertification result

                                                    var cert =
                                                        db.RecertificationResults.Where(r => r.NotificationId == id)
                                                            .ToList();

                                                    if (cert.Any())
                                                    {
                                                        var density = cert[0].Density;

                                                        var parameterBefore =
                                                            db.DischargeParameterBefores.Where(
                                                                p =>
                                                                    p.NotificationId == id &&
                                                                    p.TankId == item.Id).ToList();
                                                        if (parameterBefore.Any())
                                                        {
                                                            var paramBeforeObj = new DischargeParameterBeforeObject();
                                                            paramBeforeObj.TankGuage = parameterBefore[0].TankGuage;

                                                            paramBeforeObj.TankTC = parameterBefore[0].TankTC;

                                                            paramBeforeObj.CrossVol_TkPcLTRS =
                                                                parameterBefore[0].CrossVol_TkPcLTRS;


                                                            paramBeforeObj.SGtC_Lab = parameterBefore[0].SGtC_Lab;

                                                            paramBeforeObj.VolOfWaterLTRS =
                                                                parameterBefore[0].VolOfWaterLTRS;

                                                           

                                                            paramBeforeObj.VolCorrFactor =
                                                                parameterBefore[0].VolCorrFactor;


                                                            paramBeforeObj.SG_515C =
                                                               parameterBefore[0].SG_515C;


                                                            paramBeforeObj.NetVolOfOil_TkTc =
                                                                paramBeforeObj.CrossVol_TkPcLTRS -
                                                                paramBeforeObj.VolOfWaterLTRS;

                                                            paramBeforeObj.NetVol_1515C =
                                                                paramBeforeObj.NetVolOfOil_TkTc*
                                                                paramBeforeObj.VolCorrFactor;

                                                            paramBeforeObj.EquivVolInM_1515C =
                                                                paramBeforeObj.NetVol_1515C*density;




                                                            var parameterAfter =
                                                                db.DischargeParameterAfters.Where(
                                                                    p =>
                                                                        p.NotificationId == id &&
                                                                        p.TankId == item.Id).ToList();

                                                            if (parameterAfter.Any())
                                                            {
                                                                var parameterAfterObj =
                                                                    new DischargeParameterAfterObject();
                                                                parameterAfterObj.TankGuage =
                                                                    parameterAfter[0].TankGuage;

                                                                parameterAfterObj.TankTC = parameterAfter[0].TankTC;

                                                                parameterAfterObj.CrossVol_TkPcLTRS =
                                                                    parameterAfter[0].CrossVol_TkPcLTRS;

                                                                parameterAfterObj.SGtC_Lab = parameterAfter[0].SGtC_Lab;

                                                                parameterAfterObj.VolOfWaterLTRS =
                                                                    parameterAfter[0].VolOfWaterLTRS;

                                                               

                                                                parameterAfterObj.VolCorrFactor =
                                                                    parameterAfter[0].VolCorrFactor;

                                                                parameterAfterObj.NetVolOfOil_TkTc =
                                                                    parameterAfterObj.CrossVol_TkPcLTRS -
                                                                    paramBeforeObj.VolOfWaterLTRS;

                                                                parameterAfterObj.SG_515C =
                                                                  parameterAfter[0].SG_515C;

                                                                parameterAfterObj.NetVol_1515C =
                                                                    parameterAfterObj.NetVolOfOil_TkTc*
                                                                    parameterAfterObj.VolCorrFactor;



                                                                parameterAfterObj.EquivVolInM_1515C =
                                                               parameterAfterObj.NetVol_1515C * density;


                                                                paramBeforeObj.NetVolBal =
                                                                    parameterAfterObj.NetVol_1515C -
                                                                    paramBeforeObj.NetVol_1515C;


                                                                paramBeforeObj.EquivVolBal =
                                                                  parameterAfterObj.EquivVolInM_1515C -
                                                                  paramBeforeObj.EquivVolInM_1515C;

                                                                                                                           
                                                                dischargeObj.DischargeParameterBeforeObject =
                                                                    paramBeforeObj;

                                                                dischargeObj.DischargeParameterAfterObject =
                                                                    parameterAfterObj;

                                                                dischargeListObj.Add(dischargeObj);




                                                            }

                                                        }
                                                    }
                                                }

                                               
                                            }
                                            return Json(dischargeListObj, JsonRequestBehavior.AllowGet);
                                        }


                                        //double? total = 0;
                                        //var paramsBefore = db.DischargeParameterBefores.Where(d => d.NotificationId == id && d.StorageTank.Product.Code.Equals("PMS")).ToList();

                                        //if (paramsBefore.Any())
                                        //{
                                        //    foreach (var item in paramsBefore)
                                        //    {
                                        //        var netVolOfOil = item.CrossVol_TkPcLTRS - item.VolOfWaterLTRS;
                                        //        var volBefore = netVolOfOil * item.VolCorrFactor;

                                        //        var paramAfter =
                                        //            db.DischargeParameterAfters.Where(
                                        //                p => p.NotificationId == id && p.TankId == item.TankId).ToList();

                                        //        if (paramAfter.Any())
                                        //        {
                                        //            var netVolofOilA = paramAfter[0].CrossVol_TkPcLTRS - paramAfter[0].VolOfWaterLTRS;
                                        //            var volAfter = netVolofOilA * paramAfter[0].VolCorrFactor;
                                        //            var netVol = volAfter - volBefore;

                                        //            total = total + netVol;
                                        //        }
                                        //    }


                                        //    var calc = db.Calculators.Where(c => c.Id >= 1).ToList();
                                        //    if (calc.Any())
                                        //    {
                                        //        calc[0].QuantityInCountry = calc[0].QuantityInCountry + Convert.ToDouble(total);
                                        //        db.Calculators.Attach(calc[0]);
                                        //        db.Entry(calc[0]).State = EntityState.Modified;
                                        //        db.SaveChanges();
                                        //    }
                                        //    else
                                        //    {
                                        //        var newcalc = new Calculator();
                                        //        newcalc.QuantityInCountry = Convert.ToDouble(total);
                                        //        newcalc.Counter = 0;
                                        //        db.Calculators.Add(newcalc);
                                        //        db.SaveChanges();
                                        //    }

                                        //    //update discharge depot 
                                        //    var depotDischarge = new DepotDischarge();
                                        //    depotDischarge.DepotId = notification[0].DischargeDepotId;
                                        //    depotDischarge.QuantityDischargedInDepot = Convert.ToDouble(total);
                                        //    depotDischarge.DischargedDate = dischargedate;
                                        //    db.DepotDischarges.Add(depotDischarge);
                                        //    db.SaveChanges();

                                        //}


                                      

                                        ////move notification to the next level
                                        //if (rep.IsChecklistSaved &&
                                        //    rep.IsDischargeDataSaved && rep.IsVesselReport)
                                        //{
                                        //    var returnRep = AcceptNotification(id);

                                        //    //update the datas
                                        //    notInspec[0].SubmittionStatus = (int)EnumCheckListOutComeStatus.Submitted;
                                        //    db.NotificationInspections.Attach(notInspec[0]);
                                        //    db.Entry(notInspec[0]).State = EntityState.Modified;


                                        //    recert[0].SubmittionStatus = (int)EnumCheckListOutComeStatus.Submitted;
                                        //    db.RecertificationResults.Attach(recert[0]);
                                        //    db.Entry(recert[0]).State = EntityState.Modified;

                                        //    rep.IsAccepted = returnRep.IsAccepted;
                                        //    rep.IsNull = returnRep.IsNull;
                                        //    rep.IsError = returnRep.IsError;
                                        //    rep.IsCertificateGenerated = returnRep.IsCertificateGenerated;
                                            
                                        //    db.SaveChanges();
                                        //    return Json(rep, JsonRequestBehavior.AllowGet);
                                        //}

                                        db.SaveChanges();
                                        return Json(rep, JsonRequestBehavior.AllowGet);

                                    }
                                   

                                }
                                rep.IsRecertificationSaved = false;
                                return Json(rep, JsonRequestBehavior.AllowGet);
                            }
                            rep.IsVesselReport = false;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }

                        rep.IsChecklistSaved = false;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }

                    rep.IsError = true;
                    return Json(rep, JsonRequestBehavior.AllowGet); 

                      
                        

                      

                    }
                  
                }
            
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult ShowTanks(long id)
        {
            var rep = new Reporter();
            rep.notid = id;
            try
            {

                using (var db = new ImportPermitEntities())
                {

                    var notification = db.Notifications.Where(n => n.Id == id).ToList();

                    if (notification.Any())
                    {
                        var depotId = notification[0].DischargeDepotId;
                        //check if checklist is submitted
                        var doc = db.NotificationDocuments.Where(o => o.NotificationId == id).ToList();

                        if (doc.Any())
                        {

                            foreach (var item in doc)
                            {
                                var normalDoc = db.Documents.Where(m => m.DocumentId == item.DocumentId).ToList();
                                if (normalDoc.Any())
                                {
                                    var norm = normalDoc[0];

                                    if (norm.DocumentTypeId == (int)SpecialDocsEnum.Dry_Tank_Certificate || norm.DocumentTypeId == (int)SpecialDocsEnum.ROB)
                                    {
                                        norm.IsValid = "True";

                                    }

                                    if (norm.IsValid == "False" || norm.IsValid == null)
                                    {
                                        rep.IsChecklistSaved = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }


                                }

                            }
                            rep.IsChecklistSaved = true;

                            //check if vessel report is submitted

                            var notInspec = db.NotificationInspections.Where(o => o.NotificationId == id).ToList();

                            if (notInspec.Any())
                            {

                                var dischargedate = Convert.ToDateTime(notInspec[0].DischargeCompletionDate);

                                if (string.IsNullOrEmpty(notInspec[0].QuantityOnVessel.ToString()))
                                {
                                    rep.IsVesselReport = false;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }
                                if (string.IsNullOrEmpty(notInspec[0].QuantityOnBillOfLading.ToString()))
                                {
                                    rep.IsVesselReport = false;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }
                                if (string.IsNullOrEmpty(notInspec[0].InspectionDate.ToString()))
                                {
                                    rep.IsVesselReport = false;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }

                                rep.IsVesselReport = true;

                                var recert = db.RecertificationResults.Where(r => r.NotificationId == id).ToList();
                                if (recert.Any())
                                {
                                    if (string.IsNullOrEmpty(recert[0].Density.ToString()))
                                    {
                                        rep.IsRecertificationSaved = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }
                                    if (string.IsNullOrEmpty(recert[0].DischargeApproval))
                                    {
                                        rep.IsRecertificationSaved = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }

                                    rep.IsRecertificationSaved = true;


                                    //check if discharge data is submitted
                                    var discharge = db.NotificationDischageDatas.Where(d => d.NotificationId == id).ToList();

                                    if (discharge.Any())
                                    {
                                        rep.IsDischargeDataSaved = true;



                                        var dischargeListObj = new List<NotificationDischageDataObject>();


                                        //get tanks in the depot
                                        var tanks = db.StorageTanks.Where(t => t.DepotId == depotId).ToList();

                                        if (tanks.Any())
                                        {
                                            //get all the dischargedatas for the tanks in the depot that was filled
                                            foreach (var item in tanks)
                                            {
                                                var dischargeData =
                                               db.NotificationDischageDatas.Where(
                                                   d => d.NotificationId == id &&
                                                        d.TankId == item.Id)
                                                   .ToList();
                                                if (dischargeData.Any())
                                                {
                                                    var dischargeObj = new NotificationDischageDataObject();

                                                    dischargeObj.TankName = dischargeData[0].StorageTank.TankNo;
                                                    dischargeObj.NotificationId = dischargeData[0].NotificationId;
                                                    dischargeObj.Id = dischargeData[0].Id;


                                                    //get the density from recertification result

                                                    var cert =
                                                        db.RecertificationResults.Where(r => r.NotificationId == id)
                                                            .ToList();

                                                    if (cert.Any())
                                                    {
                                                        var density = cert[0].Density;
                                                        
                                                        var parameterBefore =
                                                            db.DischargeParameterBefores.Where(
                                                                p =>
                                                                    p.NotificationId == id &&
                                                                    p.TankId == item.Id).ToList();
                                                        if (parameterBefore.Any())
                                                        {
                                                            var paramBeforeObj = new DischargeParameterBeforeObject();
                                                            paramBeforeObj.TankGuage = parameterBefore[0].TankGuage;

                                                            paramBeforeObj.TankTC = parameterBefore[0].TankTC;

                                                            paramBeforeObj.CrossVol_TkPcLTRS =
                                                                parameterBefore[0].CrossVol_TkPcLTRS;


                                                            paramBeforeObj.SGtC_Lab = parameterBefore[0].SGtC_Lab;

                                                            paramBeforeObj.VolOfWaterLTRS =
                                                                parameterBefore[0].VolOfWaterLTRS;



                                                            paramBeforeObj.VolCorrFactor =
                                                                parameterBefore[0].VolCorrFactor;


                                                            paramBeforeObj.SG_515C =
                                                               parameterBefore[0].SG_515C;


                                                            paramBeforeObj.NetVolOfOil_TkTc =
                                                                paramBeforeObj.CrossVol_TkPcLTRS -
                                                                paramBeforeObj.VolOfWaterLTRS;

                                                            paramBeforeObj.NetVol_1515C =
                                                                paramBeforeObj.NetVolOfOil_TkTc *
                                                                paramBeforeObj.VolCorrFactor;

                                                            paramBeforeObj.EquivVolInM_1515C =
                                                                paramBeforeObj.NetVol_1515C * density;




                                                            var parameterAfter =
                                                                db.DischargeParameterAfters.Where(
                                                                    p =>
                                                                        p.NotificationId == id &&
                                                                        p.TankId == item.Id).ToList();

                                                            if (parameterAfter.Any())
                                                            {
                                                                var parameterAfterObj =
                                                                    new DischargeParameterAfterObject();
                                                                parameterAfterObj.TankGuage =
                                                                    parameterAfter[0].TankGuage;

                                                                parameterAfterObj.TankTC = parameterAfter[0].TankTC;

                                                                parameterAfterObj.CrossVol_TkPcLTRS =
                                                                    parameterAfter[0].CrossVol_TkPcLTRS;

                                                                parameterAfterObj.SGtC_Lab = parameterAfter[0].SGtC_Lab;

                                                                parameterAfterObj.VolOfWaterLTRS =
                                                                    parameterAfter[0].VolOfWaterLTRS;



                                                                parameterAfterObj.VolCorrFactor =
                                                                    parameterAfter[0].VolCorrFactor;

                                                                parameterAfterObj.NetVolOfOil_TkTc =
                                                                    parameterAfterObj.CrossVol_TkPcLTRS -
                                                                    paramBeforeObj.VolOfWaterLTRS;

                                                                parameterAfterObj.SG_515C =
                                                                  parameterAfter[0].SG_515C;

                                                                parameterAfterObj.NetVol_1515C =
                                                                    parameterAfterObj.NetVolOfOil_TkTc *
                                                                    parameterAfterObj.VolCorrFactor;



                                                                parameterAfterObj.EquivVolInM_1515C =
                                                               parameterAfterObj.NetVol_1515C * density;


                                                                paramBeforeObj.NetVolBal =
                                                                    parameterAfterObj.NetVol_1515C -
                                                                    paramBeforeObj.NetVol_1515C;


                                                                paramBeforeObj.EquivVolBal =
                                                                  parameterAfterObj.EquivVolInM_1515C -
                                                                  paramBeforeObj.EquivVolInM_1515C;


                                                                dischargeObj.DischargeParameterBeforeObject =
                                                                    paramBeforeObj;

                                                                dischargeObj.DischargeParameterAfterObject =
                                                                    parameterAfterObj;

                                                                dischargeListObj.Add(dischargeObj);




                                                            }

                                                        }
                                                    }
                                                }


                                            }
                                            return Json(dischargeListObj, JsonRequestBehavior.AllowGet);
                                        }


                                        rep.IsDischargeDataSaved = false;
                                      
                                        return Json(rep, JsonRequestBehavior.AllowGet);

                                    }
                                    rep.IsDischargeDataSaved = false;

                                    return Json(rep, JsonRequestBehavior.AllowGet);

                                }
                                rep.IsRecertificationSaved = false;

                                return Json(rep, JsonRequestBehavior.AllowGet);
                            }
                            rep.IsVesselReport = false;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }

                        rep.IsChecklistSaved = false;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }

                    rep.IsError = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);






                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ShowSavedTanks(ParameterObject employeeProfile)
        {
            var rep = new Reporter();
            var id = employeeProfile.NotificationId;
            try
            {

                using (var db = new ImportPermitEntities())
                {

                    var notification = db.Notifications.Where(n => n.Id == id).ToList();

                    if (notification.Any())
                    {
                        var depotId = notification[0].DischargeDepotId;
                        //check if checklist is submitted
                        var doc = db.NotificationDocuments.Where(o => o.NotificationId == id).ToList();

                        if (doc.Any())
                        {

                            foreach (var item in doc)
                            {
                                var normalDoc = db.Documents.Where(m => m.DocumentId == item.DocumentId).ToList();
                                if (normalDoc.Any())
                                {

                                    if (normalDoc[0].IsValid == "False" || normalDoc[0].IsValid == null)
                                    {
                                        rep.IsChecklistSaved = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }


                                }

                            }
                            rep.IsChecklistSaved = true;

                            //check if vessel report is submitted

                            var notInspec = db.NotificationInspections.Where(o => o.NotificationId == id).ToList();

                            if (notInspec.Any())
                            {

                                var dischargedate = Convert.ToDateTime(notInspec[0].DischargeCompletionDate);

                                if (string.IsNullOrEmpty(notInspec[0].QuantityOnVessel.ToString()))
                                {
                                    rep.IsVesselReport = false;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }
                                if (string.IsNullOrEmpty(notInspec[0].QuantityOnBillOfLading.ToString()))
                                {
                                    rep.IsVesselReport = false;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }
                                if (string.IsNullOrEmpty(notInspec[0].InspectionDate.ToString()))
                                {
                                    rep.IsVesselReport = false;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }

                                rep.IsVesselReport = true;

                                var recert = db.RecertificationResults.Where(r => r.NotificationId == id).ToList();
                                if (recert.Any())
                                {
                                    if (string.IsNullOrEmpty(recert[0].Density.ToString()))
                                    {
                                        rep.IsRecertificationSaved = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }
                                    if (string.IsNullOrEmpty(recert[0].DischargeApproval))
                                    {
                                        rep.IsRecertificationSaved = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }

                                    rep.IsRecertificationSaved = true;

                                    //add tank parameters
                                    var notificationVessel =
                            db.NotificationVessels.Where(n => n.NotificationId == employeeProfile.NotificationId)
                                .ToList();
                                    if (notificationVessel.Any())
                                    {
                                        var vesselId = notificationVessel[0].VesselId;


                                        //get the tankno

                                        long tankId = 0;

                                        var tank = db.StorageTanks.Where(s => s.TankNo.Equals(employeeProfile.TankNo)).ToList();

                                        if (tank.Any())
                                        {
                                            tankId = tank[0].Id;
                                        }
                                        else
                                        {
                                            var newTank = new StorageTank();
                                            newTank.TankNo = employeeProfile.TankNo;
                                            newTank.DepotId = depotId;
                                            newTank.ProductId = notification[0].ProductId;
                                            newTank.UoMId = (int)EnumMeasurement.m3;
                                            db.StorageTanks.Add(newTank);
                                            db.SaveChanges();
                                            tankId = newTank.Id;
                                        }

                                        //save new  parameter before
                                        var newParameterBefore = new DischargeParameterBefore();
                                        newParameterBefore.NotificationId = employeeProfile.NotificationId;
                                        newParameterBefore.TankId = tankId;

                                        newParameterBefore.CrossVol_TkPcLTRS = employeeProfile.CrossVol_TkPcLTRSBefore;

                                        newParameterBefore.EquivVolInM_1515C = employeeProfile.EquivVolInM_1515CBefore;
                                        newParameterBefore.NetVolOfOil_TkTc = employeeProfile.NetVolOfOil_TkTcBefore;

                                        newParameterBefore.NetVol_1515C = employeeProfile.NetVol_1515CBefore;
                                        newParameterBefore.SG_515C = employeeProfile.SG_515CBefore;

                                        newParameterBefore.TankGuage = employeeProfile.TankGuageBefore;
                                        newParameterBefore.TankTC = employeeProfile.TankTCBefore;

                                        newParameterBefore.VolCorrFactor = employeeProfile.VolCorrFactorBefore;
                                        newParameterBefore.VolOfWaterLTRS = employeeProfile.VolOfWaterLTRSBefore;

                                        newParameterBefore.SGtC_Lab = employeeProfile.SGtC_LabBefore;
                                        newParameterBefore.CrossVol_TkPcLTRS = employeeProfile.CrossVol_TkPcLTRSBefore;

                                        //save new parameter after
                                        var newParameterAfter = new DischargeParameterAfter();
                                        newParameterAfter.NotificationId = employeeProfile.NotificationId;
                                        newParameterAfter.TankId = tankId;

                                        newParameterAfter.CrossVol_TkPcLTRS = employeeProfile.CrossVol_TkPcLTRSAfter;

                                        newParameterAfter.EquivVolInM_1515C = employeeProfile.EquivVolInM_1515CAfter;
                                        newParameterAfter.NetVolOfOil_TkTc = employeeProfile.NetVolOfOil_TkTcAfter;

                                        newParameterAfter.NetVol_1515C = employeeProfile.NetVol_1515CAfter;
                                        newParameterAfter.SG_515C = employeeProfile.SG_515CAfter;

                                        newParameterAfter.TankGuage = employeeProfile.TankGuageAfter;
                                        newParameterAfter.TankTC = employeeProfile.TankTCAfter;

                                        newParameterAfter.VolCorrFactor = employeeProfile.VolCorrFactorAfter;
                                        newParameterAfter.VolOfWaterLTRS = employeeProfile.VolOfWaterLTRSAfter;

                                        newParameterAfter.SGtC_Lab = employeeProfile.SGtC_LabAfter;
                                        newParameterAfter.CrossVol_TkPcLTRS = employeeProfile.CrossVol_TkPcLTRSAfter;


                                        db.DischargeParameterBefores.Add(newParameterBefore);
                                        db.DischargeParameterAfters.Add(newParameterAfter);
                                        db.SaveChanges();

                                        //save new  discharge data
                                        var newDischargeData = new NotificationDischageData();
                                        newDischargeData.NotificationId = employeeProfile.NotificationId;
                                        newDischargeData.DepotId = depotId;
                                        newDischargeData.VesselId = vesselId;
                                        newDischargeData.TankId = tankId;
                                        newDischargeData.DischargeParameterBeforeId = newParameterBefore.Id;
                                        newDischargeData.DischargeParameterAfterId = newParameterAfter.Id;

                                        db.NotificationDischageDatas.Add(newDischargeData);
                                        db.SaveChanges();

                                      


                                    }


                                    //check if discharge data is submitted
                                    var discharge = db.NotificationDischageDatas.Where(d => d.NotificationId == id).ToList();

                                    if (discharge.Any())
                                    {
                                        rep.IsDischargeDataSaved = true;



                                        var dischargeListObj = new List<NotificationDischageDataObject>();


                                        //get tanks in the depot
                                        var tanks = db.StorageTanks.Where(t => t.DepotId == depotId).ToList();

                                        if (tanks.Any())
                                        {
                                            //get all the dischargedatas for the tanks in the depot that was filled
                                            foreach (var item in tanks)
                                            {
                                                var dischargeData =
                                               db.NotificationDischageDatas.Where(
                                                   d => d.NotificationId == id &&
                                                        d.TankId == item.Id)
                                                   .ToList();
                                                if (dischargeData.Any())
                                                {
                                                    var dischargeObj = new NotificationDischageDataObject();

                                                    dischargeObj.TankName = dischargeData[0].StorageTank.TankNo;
                                                    dischargeObj.NotificationId = dischargeData[0].NotificationId;


                                                    //get the density from recertification result

                                                    var cert =
                                                        db.RecertificationResults.Where(r => r.NotificationId == id)
                                                            .ToList();

                                                    if (cert.Any())
                                                    {
                                                        var density = cert[0].Density;

                                                        var parameterBefore =
                                                            db.DischargeParameterBefores.Where(
                                                                p =>
                                                                    p.NotificationId == id &&
                                                                    p.TankId == item.Id).ToList();
                                                        if (parameterBefore.Any())
                                                        {
                                                            var paramBeforeObj = new DischargeParameterBeforeObject();
                                                            paramBeforeObj.TankGuage = parameterBefore[0].TankGuage;

                                                            paramBeforeObj.TankTC = parameterBefore[0].TankTC;

                                                            paramBeforeObj.CrossVol_TkPcLTRS =
                                                                parameterBefore[0].CrossVol_TkPcLTRS;


                                                            paramBeforeObj.SGtC_Lab = parameterBefore[0].SGtC_Lab;

                                                            paramBeforeObj.VolOfWaterLTRS =
                                                                parameterBefore[0].VolOfWaterLTRS;



                                                            paramBeforeObj.VolCorrFactor =
                                                                parameterBefore[0].VolCorrFactor;


                                                            paramBeforeObj.SG_515C =
                                                               parameterBefore[0].SG_515C;


                                                            paramBeforeObj.NetVolOfOil_TkTc =
                                                                paramBeforeObj.CrossVol_TkPcLTRS -
                                                                paramBeforeObj.VolOfWaterLTRS;

                                                            paramBeforeObj.NetVol_1515C =
                                                                paramBeforeObj.NetVolOfOil_TkTc *
                                                                paramBeforeObj.VolCorrFactor;

                                                            paramBeforeObj.EquivVolInM_1515C =
                                                                paramBeforeObj.NetVol_1515C * density;




                                                            var parameterAfter =
                                                                db.DischargeParameterAfters.Where(
                                                                    p =>
                                                                        p.NotificationId == id &&
                                                                        p.TankId == item.Id).ToList();

                                                            if (parameterAfter.Any())
                                                            {
                                                                var parameterAfterObj =
                                                                    new DischargeParameterAfterObject();
                                                                parameterAfterObj.TankGuage =
                                                                    parameterAfter[0].TankGuage;

                                                                parameterAfterObj.TankTC = parameterAfter[0].TankTC;

                                                                parameterAfterObj.CrossVol_TkPcLTRS =
                                                                    parameterAfter[0].CrossVol_TkPcLTRS;

                                                                parameterAfterObj.SGtC_Lab = parameterAfter[0].SGtC_Lab;

                                                                parameterAfterObj.VolOfWaterLTRS =
                                                                    parameterAfter[0].VolOfWaterLTRS;



                                                                parameterAfterObj.VolCorrFactor =
                                                                    parameterAfter[0].VolCorrFactor;

                                                                parameterAfterObj.NetVolOfOil_TkTc =
                                                                    parameterAfterObj.CrossVol_TkPcLTRS -
                                                                    paramBeforeObj.VolOfWaterLTRS;

                                                                parameterAfterObj.SG_515C =
                                                                  parameterAfter[0].SG_515C;

                                                                parameterAfterObj.NetVol_1515C =
                                                                    parameterAfterObj.NetVolOfOil_TkTc *
                                                                    parameterAfterObj.VolCorrFactor;



                                                                parameterAfterObj.EquivVolInM_1515C =
                                                               parameterAfterObj.NetVol_1515C * density;


                                                                paramBeforeObj.NetVolBal =
                                                                    parameterAfterObj.NetVol_1515C -
                                                                    paramBeforeObj.NetVol_1515C;


                                                                paramBeforeObj.EquivVolBal =
                                                                  parameterAfterObj.EquivVolInM_1515C -
                                                                  paramBeforeObj.EquivVolInM_1515C;


                                                                dischargeObj.DischargeParameterBeforeObject =
                                                                    paramBeforeObj;

                                                                dischargeObj.DischargeParameterAfterObject =
                                                                    parameterAfterObj;

                                                                dischargeListObj.Add(dischargeObj);




                                                            }

                                                        }
                                                    }
                                                }


                                            }
                                            return Json(dischargeListObj, JsonRequestBehavior.AllowGet);
                                        }


                                        rep.IsDischargeDataSaved = false;

                                        return Json(rep, JsonRequestBehavior.AllowGet);

                                    }
                                    rep.IsDischargeDataSaved = false;

                                    return Json(rep, JsonRequestBehavior.AllowGet);

                                }
                                rep.IsRecertificationSaved = false;

                                return Json(rep, JsonRequestBehavior.AllowGet);
                            }
                            rep.IsVesselReport = false;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }

                        rep.IsChecklistSaved = false;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }

                    rep.IsError = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);






                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult SubmitJettyReport(long id)
        {
            var rep = new Reporter();
            try
            {

                using (var db = new ImportPermitEntities())
                {

                    var notification = db.Notifications.Where(n => n.Id == id).ToList();

                    if (notification.Any())
                    {
                        var depotId = notification[0].DischargeDepotId;
                        //check if checklist is submitted
                        var doc = db.NotificationDocuments.Where(o => o.NotificationId == id).ToList();

                        if (doc.Any())
                        {

                            foreach (var item in doc)
                            {
                                var normalDoc = db.Documents.Where(m => m.DocumentId == item.DocumentId).ToList();
                                if (normalDoc.Any())
                                {

                                    if (normalDoc[0].IsValid == "False" || normalDoc[0].IsValid == null)
                                    {
                                        rep.IsChecklistSaved = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }


                                }

                            }
                            rep.IsChecklistSaved = true;

                            //check if vessel report is submitted

                            var notInspec = db.NotificationInspections.Where(o => o.NotificationId == id).ToList();

                            if (notInspec.Any())
                            {

                                var dischargedate = Convert.ToDateTime(notInspec[0].DischargeCompletionDate);

                                if (string.IsNullOrEmpty(notInspec[0].QuantityOnVessel.ToString()))
                                {
                                    rep.IsVesselReport = false;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }
                                if (string.IsNullOrEmpty(notInspec[0].QuantityOnBillOfLading.ToString()))
                                {
                                    rep.IsVesselReport = false;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }
                                if (string.IsNullOrEmpty(notInspec[0].InspectionDate.ToString()))
                                {
                                    rep.IsVesselReport = false;
                                    return Json(rep, JsonRequestBehavior.AllowGet);
                                }

                                rep.IsVesselReport = true;

                                var recert = db.RecertificationResults.Where(r => r.NotificationId == id).ToList();
                                if (recert.Any())
                                {
                                    if (string.IsNullOrEmpty(recert[0].Density.ToString()))
                                    {
                                        rep.IsRecertificationSaved = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }
                                    if (string.IsNullOrEmpty(recert[0].DischargeApproval))
                                    {
                                        rep.IsRecertificationSaved = false;
                                        return Json(rep, JsonRequestBehavior.AllowGet);
                                    }

                                    rep.IsRecertificationSaved = true;


                                        //move notification to the next level
                                        if (rep.IsChecklistSaved &&
                                            rep.IsDischargeDataSaved && rep.IsVesselReport)
                                        {
                                            var returnRep = AcceptNotification(id);

                                            //update the datas
                                            notInspec[0].SubmittionStatus = (int)EnumCheckListOutComeStatus.Submitted;
                                            db.NotificationInspections.Attach(notInspec[0]);
                                            db.Entry(notInspec[0]).State = EntityState.Modified;


                                            recert[0].SubmittionStatus = (int)EnumCheckListOutComeStatus.Submitted;
                                            db.RecertificationResults.Attach(recert[0]);
                                            db.Entry(recert[0]).State = EntityState.Modified;

                                            rep.IsAccepted = returnRep.IsAccepted;
                                            rep.IsNull = returnRep.IsNull;
                                            rep.IsError = returnRep.IsError;
                                            rep.IsCertificateGenerated = returnRep.IsCertificateGenerated;

                                            db.SaveChanges();
                                            return Json(rep, JsonRequestBehavior.AllowGet);
                                        }

                                        db.SaveChanges();
                                        return Json(rep, JsonRequestBehavior.AllowGet);

                                    


                                }
                                rep.IsRecertificationSaved = false;
                                return Json(rep, JsonRequestBehavior.AllowGet);
                            }
                            rep.IsVesselReport = false;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }

                        rep.IsChecklistSaved = false;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }

                    rep.IsError = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);






                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult SubmitFinalReport(long id)
        {
            var rep = new Reporter();
            try
            {

                using (var db = new ImportPermitEntities())
                {
                    //move notification to the next level

                    var returnRep = AcceptNotification(id);

                    if (returnRep.IsAccepted || returnRep.IsCertificateGenerated)
                    {
                        var notification = db.Notifications.Where(n => n.Id == id).ToList();

                        if (notification.Any())
                        {
                            var notInspec = db.NotificationInspections.Where(o => o.NotificationId == id).ToList();

                            if (notInspec.Any())
                            {

                                var dischargedate = Convert.ToDateTime(notInspec[0].DischargeCompletionDate);
                                double? total = 0;
                                var paramsBefore =
                                    db.DischargeParameterBefores.Where(
                                        d => d.NotificationId == id && d.StorageTank.Product.Code.Equals("PMS"))
                                        .ToList();

                                if (paramsBefore.Any())
                                {
                                    foreach (var item in paramsBefore)
                                    {
                                        var netVolOfOil = item.CrossVol_TkPcLTRS - item.VolOfWaterLTRS;
                                        var volBefore = netVolOfOil*item.VolCorrFactor;

                                        var paramAfter =
                                            db.DischargeParameterAfters.Where(
                                                p => p.NotificationId == id && p.TankId == item.TankId).ToList();

                                        if (paramAfter.Any())
                                        {
                                            var netVolofOilA = paramAfter[0].CrossVol_TkPcLTRS -
                                                               paramAfter[0].VolOfWaterLTRS;
                                            var volAfter = netVolofOilA*paramAfter[0].VolCorrFactor;
                                            var netVol = volAfter - volBefore;

                                            total = total + netVol;
                                        }
                                    }


                                    var calc = db.Calculators.Where(c => c.Id >= 1).ToList();
                                    if (calc.Any())
                                    {
                                        calc[0].QuantityInCountry = calc[0].QuantityInCountry + Convert.ToDouble(total);
                                        db.Calculators.Attach(calc[0]);
                                        db.Entry(calc[0]).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        var newcalc = new Calculator();
                                        newcalc.QuantityInCountry = Convert.ToDouble(total);
                                        newcalc.Counter = 0;
                                        db.Calculators.Add(newcalc);
                                        db.SaveChanges();
                                    }

                                    //update discharge depot 
                                    var depotDischarge = new DepotDischarge();
                                    depotDischarge.DepotId = notification[0].DischargeDepotId;
                                    depotDischarge.QuantityDischargedInDepot = Convert.ToDouble(total);
                                    depotDischarge.DischargedDate = dischargedate;
                                    db.DepotDischarges.Add(depotDischarge);
                                    db.SaveChanges();

                                }




                                //update vessel report
                                notInspec[0].SubmittionStatus = (int) EnumCheckListOutComeStatus.Submitted;
                                db.NotificationInspections.Attach(notInspec[0]);
                                db.Entry(notInspec[0]).State = EntityState.Modified;

                                //update recertification
                                var recert = db.RecertificationResults.Where(r => r.NotificationId == id).ToList();
                                if (recert.Any())
                                {
                                    recert[0].SubmittionStatus = (int) EnumCheckListOutComeStatus.Submitted;
                                    db.RecertificationResults.Attach(recert[0]);
                                    db.Entry(recert[0]).State = EntityState.Modified;
                                }

                                rep.IsAccepted = returnRep.IsAccepted;
                                rep.IsNull = returnRep.IsNull;
                                rep.IsError = returnRep.IsError;
                                rep.IsCertificateGenerated = returnRep.IsCertificateGenerated;

                                db.SaveChanges();
                                return Json(rep, JsonRequestBehavior.AllowGet);

                            }
                            rep.IsError = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }
                        rep.IsError = true;
                        return Json(rep, JsonRequestBehavior.AllowGet);
                    }
                    return Json(returnRep, JsonRequestBehavior.AllowGet);
                }

            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }

        private ImporterObject GetLoggedOnUserInfo()
        {
            try
            {
                if (Session["_importerInfo"] == null)
                {
                    return new ImporterObject();
                }

                var importerInfo = Session["_importerInfo"] as ImporterObject;
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return new ImporterObject();
                }

                return importerInfo;

            }
            catch (Exception)
            {
                return new ImporterObject();
            }
        }

        public ActionResult GetApplication(long id)
        {
            var gVal = new GenericValidator();
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out. Please refresh the page.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                };

                var app = GetApplicationInfo(id, importerInfo.UserProfileObject.Id);
                if (app == null || app.Id < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }


                return Json(app, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult GetApplicationFromHistory(long id)
        {
            var rep = new Reporter();
            try
            {
                var db = new ImportPermitEntities();
                //get the id of logged in user
                var aspnetId = User.Identity.GetUserId();

                //get the id of the userprofile table
                var registeredGuys = db.AspNetUsers.Find(aspnetId);
                var profileId = registeredGuys.UserProfile.Id;
                if (id < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetApplicationInfoFromHistory(id, profileId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetNotification(long id)
        {
            var rep = new Reporter();
            try
            {
                var db = new ImportPermitEntities();
                //get the id of logged in user
                var aspnetId = User.Identity.GetUserId();

                //get the id of the userprofile table
                var registeredGuys = db.AspNetUsers.Find(aspnetId);
                var profileId = registeredGuys.UserProfile.Id;

                if (id < 1)
                {
                    return Json(new Notification(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetNotificationInfo(id, profileId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetNotificationBack(long id)
        {
            var rep = new Reporter();
            try
            {
                var db = new ImportPermitEntities();
                //get the id of logged in user
                var aspnetId = User.Identity.GetUserId();

                //get the id of the userprofile table
                var registeredGuys = db.AspNetUsers.Find(aspnetId);
                var profileId = registeredGuys.UserProfile.Id;

                if (id < 1)
                {
                    return Json(new Notification(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetNotificationInfoBack(id, profileId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetNotificationFromDetail(long id)
        {
            var rep = new Reporter();
            try
            {
                var db = new ImportPermitEntities();
                //get the id of logged in user
                var aspnetId = User.Identity.GetUserId();

                //get the id of the userprofile table
                var registeredGuys = db.AspNetUsers.Find(aspnetId);
                var profileId = registeredGuys.UserProfile.Id;

                if (id < 1)
                {
                    return Json(new Notification(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetNotificationInfoFromDetail(id, profileId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetNotificationAdmin(long id)
        {
            var rep = new Reporter();
            try
            {

                if (id < 1)
                {
                    return Json(new Notification(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetNotificationAdminInfo(id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetRecertification(long id)
        {
            var rep = new Reporter();
            try
            {
                var db = new ImportPermitEntities();
                //get the id of logged in user
                var aspnetId = User.Identity.GetUserId();

                //get the id of the userprofile table
                var registeredGuys = db.AspNetUsers.Find(aspnetId);
                var profileId = registeredGuys.UserProfile.Id;
                if (id < 1)
                {
                    return Json(new RecertificationObject(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetRecertificationInfo(id, profileId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new RecertificationObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetHistory(long id)
        {

            var historyObjectlist = new List<NotificationHistoryObject>();
            try
            {
                using (var db = new ImportPermitEntities())
                {


                    if (id < 1)
                    {
                        return Json(new List<NotificationHistoryObject>(), JsonRequestBehavior.AllowGet);
                    }

                    var history = db.NotificationHistories.Where(h => h.NotificationId == id);

                    if (history.Any())
                    {


                        foreach (var item in history)
                        {
                            var historyObject = new NotificationHistoryObject();

                            historyObject.EmployeeName = item.EmployeeDesk.UserProfile.Person.FirstName + " " +
                                                         item.EmployeeDesk.UserProfile.Person.LastName;

                            historyObject.StepName = item.Step.Name;
                            historyObject.AssignedTimeStr = item.AssignedTime.Value.ToString("dd/MM/yy");
                            historyObject.ActualDeliveryDateTimeStr = item.FinishedTime.Value.ToString("dd/MM/yy");
                            historyObject.Remarks = item.Remarks;

                            historyObjectlist.Add(historyObject);
                        }

                        return Json(historyObjectlist, JsonRequestBehavior.AllowGet);

                    }
                    return Json(new List<NotificationHistoryObject>(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new List<NotificationHistoryObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        private ApplicationObject GetApplicationInfo(long trackId, long userId)
        {
            return new EmployeeProfileServices().GetApplication(trackId, userId);
        }

        private ResponseObject GetDashboardInfo(long userId)
        {
            return new EmployeeProfileServices().GetDashboard(userId);
        }

        private RecertificationObject GetRecertificationInfo(long trackId, long userId)
        {
            return new EmployeeProfileServices().GetRecertification(trackId, userId);
        }

        private ApplicationObject GetApplicationInfoFromHistory(long historyId, long userId)
        {
            return new EmployeeProfileServices().GetApplicationFromHistory(historyId, userId);
        }

        private NotificationObject GetNotificationInfo(long trackId, long userId)
        {
            return new EmployeeProfileServices().GetNotification(trackId, userId);
        }
        private NotificationObject GetNotificationInfoBack(long Id, long userId)
        {
            return new EmployeeProfileServices().GetNotificationBack(Id, userId);
        }

        private NotificationObject GetNotificationInfoFromDetail(long id, long userId)
        {
            return new EmployeeProfileServices().GetNotificationFromDetail(id, userId);
        }

        private NotificationObject GetNotificationAdminInfo(long id)
        {
            return new EmployeeProfileServices().GetNotificationAdmin(id);
        }

        [HttpPost]
        public ActionResult EditProcessTracking(ProcessTrackingObject processTracking)
        {
            var gVal = new GenericValidator();

            try
            {
               

                if (string.IsNullOrEmpty(processTracking.ReferenceCode.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide ProcessTracking.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_ProcessTracking"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldProcessTracking = Session["_ProcessTracking"] as ProcessTrackingObject;

                if (oldProcessTracking == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                oldProcessTracking.ReferenceCode = processTracking.ReferenceCode.Trim();

                var docStatus = new ProcessTrackingServices().UpdateProcessTracking(oldProcessTracking);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "ProcessTracking information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldProcessTracking.Id;
                gVal.Error = "ProcessTracking information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "ProcessTracking information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ProcessTrackingObject> GetEmployeeProfiles(int? itemsPerPage, int? pageNumber, out int countG,
            string userId)
        {
            try
            {

                return new EmployeeProfileServices().GetEmployeeProfiles(itemsPerPage, pageNumber, out countG, userId);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<ProcessTrackingObject>();
            }
        }

        private List<NotificationInspectionQueueObject> GetNotificationTrackProfiles(int? itemsPerPage, int? pageNumber,
            out int countG, string userId)
        {
            try
            {

                return new EmployeeProfileServices().GetNotificationTrackProfiles(itemsPerPage, pageNumber, out countG,
                    userId);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<NotificationInspectionQueueObject>();
            }
        }

        private List<RecertificationProcessObject> GetRecertificationTrackProfiles(int? itemsPerPage, int? pageNumber,
         out int countG, string userId)
        {
            try
            {

                return new EmployeeProfileServices().GetRecertificationTrackProfiles(itemsPerPage, pageNumber, out countG,
                    userId);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<RecertificationProcessObject>();
            }
        }

        private List<ProcessingHistoryObject> GetPreviousJobsProfiles(int? itemsPerPage, int? pageNumber, out int countG,
            string userId)
        {
            try
            {

                return new EmployeeProfileServices().GetPreviousJobsProfiles(itemsPerPage, pageNumber, out countG,
                    userId);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<ProcessingHistoryObject>();
            }
        }

        public ActionResult GetProcessTracking(long id)
        {
            try
            {


                var processTracking = new ProcessTrackingServices().GetProcessTracking(id);
                if (processTracking == null || processTracking.Id < 1)
                {
                    return Json(new ProcessTrackingObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_ProcessTracking"] = processTracking;

                return Json(processTracking, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new ProcessTrackingObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteProcessTracking(long id)
        {
            var gVal = new GenericValidator();
            try
            {
                if (id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Invalid selection";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                var delStatus = new ProcessTrackingServices().DeleteProcessTracking(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "ProcessTracking could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "ProcessTracking Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericEmplyeeProfileList()
                {

                    IssueTypes = getIssueTypes(),
                    Products = getProductTypes(),
                    Jettys = getJettyTypes(),
                    DischargeApprovals = getDischargeApprovals()
                    //Tanks = gettanks()


                };


                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetTankList(long id)
        {
            try
            {
                var newList = new GenericEmplyeeProfileList()
                {


                    Tanks = gettanks(id)


                };


                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetProductColourList()
        {
            try
            {
                var newList = new GenericEmplyeeProfileList()
                {


                    ProductColours = getColours()


                };


                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetDepotList()
        {
            try
            {
                var db = new ImportPermitEntities();
                //get the id of logged in user
                var aspnetId = User.Identity.GetUserId();

                //get the id of the userprofile table
                var registeredGuys = db.AspNetUsers.Find(aspnetId);
                var profileId = registeredGuys.UserProfile.Id;

                var newList = new GenericDepotList()
                {
                    Products = getProductTypes(),
                    Depots = getDepots(profileId)
                };

                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }
        
        private List<IssueTypeObject> getIssueTypes()
        {

            try
            {
                return new IssueTypeServices().GetIssueTypes();

            }
            catch (Exception)
            {
                return new List<IssueTypeObject>();
            }
        }


        private List<ProductColourObject> getColours()
        {

            try
            {
                return new ProductColourServices().GetProductColours();

            }
            catch (Exception)
            {
                return new List<ProductColourObject>();
            }
        }

        private List<DepotObject> getDepots(long userProfile)
        {

            try
            {
                return new DepotServices().GetDepots(userProfile);

            }
            catch (Exception)
            {
                return new List<DepotObject>();
            }
        }

     

        private List<ProductObject> getProductTypes()
        {

            try
            {
                return new ProductServices().GetProducts();

            }
            catch (Exception)
            {
                return new List<ProductObject>();
            }
        }

        private List<JettyObject> getJettyTypes()
        {

            try
            {
                return new JettyServices().GetJetties();

            }
            catch (Exception)
            {
                return new List<JettyObject>();
            }
        }



        private List<GenericObject> getDischargeApprovals()
        {

            try
            {
                var itemList = EnumToObjList.ConvertEnumToList(typeof(EnumDischargeApproval));
                return itemList;

            }
            catch (Exception)
            {
                return new List<GenericObject>();
            }
        }



        private List<StorageTankObject> gettanks(long id)
        {

            try
            {
                return new StorageTankServices().GetStorageTanks(id);

            }
            catch (Exception)
            {
                return new List<StorageTankObject>();
            }
        }
    }
}

