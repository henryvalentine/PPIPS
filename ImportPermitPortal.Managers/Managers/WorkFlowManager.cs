using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
    public class WorkFlowManager
    {

       public bool AssignApplicationToEmployee(long applicationId)
        {
            try
            {

                using (var db = new ImportPermitEntities())
                {
                    //get the application that has been paid for
                    var application = db.Applications.Find(applicationId);

                    //make sure it is paid for
                    if (application.ApplicationStatusCode != (int)AppStatus.Submitted)
                    {
                        return false;
                    }
                    const int appStage = (int)AppStage.Application;

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
                            var track = new ProcessTracking();
                            track.ApplicationId = applicationId;
                            track.EmployeeId = emp.EmployeeDeskId;
                            track.StepId = stepId;
                            track.AssignedTime = DateTime.Now;
                            track.StepCode = 1;
                            track.StatusId = (int)AppStatus.Processing;

                            db.ProcessTrackings.Add(track);

                            //update employee job count

                            var empId = emp.EmployeeDeskId;

                            var employeeDesk = db.EmployeeDesks.Find(empId);

                            employeeDesk.JobCount = employeeDesk.JobCount + 1;

                            employeeDesk.ApplicationCount = employeeDesk.ApplicationCount + 1;

                            db.EmployeeDesks.Attach(employeeDesk);
                            db.Entry(employeeDesk).State = EntityState.Modified;

                            //change the application status
                            application.ApplicationStatusCode = (int)AppStatus.Processing;
                            db.Entry(application).State = EntityState.Modified;
                            db.SaveChanges();


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



        public long AssignApplicationToEmployeeInTrack(long applicationId, int nextStepSequence, long trackId)
        {
            try
            {

                using (var db = new ImportPermitEntities())
                {
                   
                   

                    //get the step with a sequence of one in application stage
                    var currentStep = db.Steps.Where(s => s.SequenceNumber == nextStepSequence && s.Process.ImportStageId == (int)AppStage.Application).ToList();
                   
                    if (currentStep.Any())
                    {
                        //get the group and activity type the step belong to

                        var groupId = currentStep[0].GroupId;
                        var activityTypeId = currentStep[0].ActivityTypeId;
                        var stepId = currentStep[0].Id;
                        //get the employees in that group
                        var emps = (from e in db.EmployeeDesks

                                   where e.GroupId == groupId && e.ActivityTypeId == activityTypeId
                                   orderby e.JobCount ascending
                                   select new EmployeeStepObject
                                   {
                                       EmployeeDeskId = e.Id,

                                       JobCount = e.JobCount

                                   }).ToList();
                      



                        if (emps.Any())
                        {
                            var emp = emps.First();
                            //get the process track
                            var track = db.ProcessTrackings.Find(trackId);


                            track.EmployeeId = emp.EmployeeDeskId;
                            track.StepId = stepId;
                            track.AssignedTime = DateTime.Now;
                            track.StepCode = nextStepSequence;
                           

                            db.ProcessTrackings.Attach(track);
                            db.Entry(track).State = EntityState.Modified;


                            //update employee job count


                            var empId = emp.EmployeeDeskId;

                            var employeeDesk = db.EmployeeDesks.Find(empId);

                            employeeDesk.JobCount = employeeDesk.JobCount + 1;
                            employeeDesk.ApplicationCount = employeeDesk.ApplicationCount + 1;

                            db.EmployeeDesks.Attach(employeeDesk);
                            db.Entry(employeeDesk).State = EntityState.Modified;


                            db.SaveChanges();

                            return employeeDesk.EmployeeId;
                        }

                        return 0;
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


        public bool AssignNotificationToEmployee(long notificationId)
        {
            try
            {

                using (var db = new ImportPermitEntities())
                {
                    //get the application that has been paid for
                    var notification = db.Notifications.Find(notificationId);

                    //make sure it is paid for
                    //if (application.StatusCode != (int)AppStatus.Paid)
                    //{
                    //    return false;
                    //}

                    //get the step with a sequence of one in application stage
                    var firstStep = db.Steps.Where(s => s.SequenceNumber == 1 && s.Process.ImportStageId == (int)AppStage.Notification).ToList();

                    if (firstStep.Any())
                    {
                        //get the group the step belong to
                        //var group = db.Groups.Find(firstStep[0].GroupId);

                        var groupId = firstStep[0].GroupId;
                        var activityTypeId = firstStep[0].ActivityTypeId;
                        var stepId = firstStep[0].Id;

                         //check whether employee is an inspector or other location approver
                        if (firstStep[0].IsLocationRequired == true)
                        {

                            var zoneId = 0;

                            //get the jetty id from depot
                            var depot = db.Depots.Where(d => d.Id == notification.DischargeDepotId).ToList();

                            if (depot.Any())
                            {
                                var jettyId = depot[0].JettyId;

                                var jettyMapping =
                                    db.JettyMappings.Where(j => j.JettyId == jettyId).ToList();

                                if (jettyMapping.Any())
                                {
                                    zoneId = jettyMapping[0].ZoneId;


                                }
                                else
                                {
                                    return false;
                                }
                            }


                            //get the employees in that group
                            var emps = (from e in db.EmployeeDesks

                                where e.GroupId == groupId && e.ActivityTypeId == activityTypeId && e.ZoneId == zoneId
                                orderby e.JobCount ascending
                                select new EmployeeStepObject
                                {
                                    EmployeeDeskId = e.Id,

                                    JobCount = e.JobCount

                                }).ToList();






                            if (emps.Any())
                            {
                                var emp = emps.First();

                                var track = new NotificationInspectionQueue();

                                track.NotificationId = notificationId;
                                track.EmployeeId = emp.EmployeeDeskId;
                                track.StepId = stepId;
                                track.AssignedTime = DateTime.Now;
                                track.StepCode = 1;
                                track.StatusId = (int) NotificationStatusEnum.Processing;


                                db.NotificationInspectionQueues.Add(track);

                                //update employee job count


                                var empId = emp.EmployeeDeskId;

                                var employeeDesk = db.EmployeeDesks.Find(empId);

                                employeeDesk.JobCount = employeeDesk.JobCount + 1;
                                employeeDesk.NotificationCount = employeeDesk.NotificationCount + 1;

                                db.EmployeeDesks.Attach(employeeDesk);
                                db.Entry(employeeDesk).State = EntityState.Modified;

                                //change the notification status
                                notification.Status = (int) NotificationStatusEnum.Processing;
                                db.Notifications.Attach(notification);
                                db.Entry(notification).State = EntityState.Modified;
                                db.SaveChanges();
                                return true;
                            }
                            return false;
                        }
                        else
                        {
                            //get the employees in that group
                            var emps = (from e in db.EmployeeDesks

                                        where e.GroupId == groupId && e.ActivityTypeId == activityTypeId
                                        orderby e.JobCount ascending
                                        select new EmployeeStepObject
                                        {
                                            EmployeeDeskId = e.Id,

                                            JobCount = e.JobCount

                                        }).ToList();






                            if (emps.Any())
                            {
                                var emp = emps.First();

                                var track = new NotificationInspectionQueue();

                                track.NotificationId = notificationId;
                                track.EmployeeId = emp.EmployeeDeskId;
                                track.StepId = stepId;
                                track.AssignedTime = DateTime.Now;
                                track.StepCode = 1;
                                track.OutComeCode = (int)NotificationStatusEnum.Processing;


                                db.NotificationInspectionQueues.Add(track);

                                //update employee job count


                                var empId = emp.EmployeeDeskId;

                                var employeeDesk = db.EmployeeDesks.Find(empId);

                                employeeDesk.JobCount = employeeDesk.JobCount + 1;
                                employeeDesk.NotificationCount = employeeDesk.NotificationCount + 1;

                                db.EmployeeDesks.Attach(employeeDesk);
                                db.Entry(employeeDesk).State = EntityState.Modified;

                                //change the notification status
                                notification.Status = (int)NotificationStatusEnum.Processing;
                                db.Notifications.Attach(notification);
                                db.Entry(notification).State = EntityState.Modified;


                                db.SaveChanges();

                                return true;
                            }
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



        public bool AssignNotificationToEmployeeInTrack(long notificationId, int nextStepSequence, long trackId)
        {
            try
            {

                using (var db = new ImportPermitEntities())
                {
                    //get the application that has been paid for
                    var notification = db.Notifications.Find(notificationId);



                    //get the current step to be used in the assignment
                    var currentStep = db.Steps.Where(s => s.SequenceNumber == nextStepSequence && s.Process.ImportStageId == (int)AppStage.Notification).ToList();


                    if (currentStep.Any())
                    {
                        //get the group and activity type the step belong to

                        var groupId = currentStep[0].GroupId;
                        var activityTypeId = currentStep[0].ActivityTypeId;
                        var stepId = currentStep[0].Id;

                       
                      
                        //check whether employee is an inspector or other location approver
                        if (currentStep[0].IsLocationRequired == true)
                        {

                            var zoneId = 0;

                            //get the jetty id from depot
                            var depot = db.Depots.Where(d => d.Id == notification.DischargeDepotId).ToList();

                            if (depot.Any())
                            {
                                var jettyId = depot[0].JettyId;

                                var jettyMapping =
                               db.JettyMappings.Where(j => j.JettyId == jettyId).ToList();

                                if (jettyMapping.Any())
                                {
                                    zoneId = jettyMapping[0].ZoneId;


                                }
                                else
                                {
                                    return false;
                                }
                            }



                            var emps = (from e in db.EmployeeDesks

                                       where e.GroupId == groupId && e.ActivityTypeId == activityTypeId && e.ZoneId == zoneId
                                       orderby e.JobCount ascending
                                       select new EmployeeStepObject
                                       {
                                           EmployeeDeskId = e.Id,

                                           JobCount = e.JobCount

                                       }).ToList();

                            if (emps.Any())
                            {
                                var emp = emps.First();
                                //get the process track
                                var track = db.NotificationInspectionQueues.Find(trackId);


                                track.EmployeeId = emp.EmployeeDeskId;
                                track.StepId = stepId;
                                track.AssignedTime = DateTime.Now;
                                track.StepCode = nextStepSequence;
                              

                                db.NotificationInspectionQueues.Attach(track);
                                db.Entry(track).State = EntityState.Modified;


                                //update employee job count

                                var empId = emp.EmployeeDeskId;

                                var employeeDesk = db.EmployeeDesks.Find(empId);

                                employeeDesk.JobCount = employeeDesk.JobCount + 1;
                                employeeDesk.NotificationCount = employeeDesk.NotificationCount + 1;

                                db.EmployeeDesks.Attach(employeeDesk);
                                db.Entry(employeeDesk).State = EntityState.Modified;

                                var proId = employeeDesk.EmployeeId;

                                var pro = db.UserProfiles.Find(proId);


                                db.SaveChanges();

                                ////email for you
                                
                                return true;
                            }
                            return false;
                        }
                        else
                        {
                            //get the employees in that group
                            var emps = (from e in db.EmployeeDesks

                                       where e.GroupId == groupId && e.ActivityTypeId == activityTypeId
                                       orderby e.JobCount ascending
                                       select new EmployeeStepObject
                                       {
                                           EmployeeDeskId = e.Id,

                                           JobCount = e.JobCount

                                       }).ToList();

                            if (emps.Any())
                            {
                               var  emp = emps.First();

                                //get the process track
                                var track = db.NotificationInspectionQueues.Find(trackId);


                                track.EmployeeId = emp.EmployeeDeskId;
                                track.StepId = stepId;
                                track.AssignedTime = DateTime.Now;
                                track.StepCode = nextStepSequence;
                                

                                db.NotificationInspectionQueues.Attach(track);
                                db.Entry(track).State = EntityState.Modified;


                                //update employee job count


                                var empId = emp.EmployeeDeskId;

                                var employeeDesk = db.EmployeeDesks.Find(empId);

                                employeeDesk.JobCount = employeeDesk.JobCount + 1;
                                employeeDesk.NotificationCount = employeeDesk.NotificationCount + 1;

                                db.EmployeeDesks.Attach(employeeDesk);
                                db.Entry(employeeDesk).State = EntityState.Modified;

                                var proId = employeeDesk.EmployeeId;

                                var pro = db.UserProfiles.Find(proId);

                                db.SaveChanges();

                                ////email for you
                              


                                return true;
                                
                            }
                            return false;
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


   
        public bool AssignRecertificationToEmployeeInTrack(long applicationId, int nextStepSequence, long trackId)
        {
            try
            {

                using (var db = new ImportPermitEntities())
                {



                    //get the step with a sequence of one in application stage
                    var currentStep = db.Steps.Where(s => s.SequenceNumber == nextStepSequence && s.Process.ImportStageId == (int)AppStage.Recertification).ToList();

                    if (currentStep.Any())
                    {
                        //get the group and activity type the step belong to

                        var groupId = currentStep[0].GroupId;
                        var activityTypeId = currentStep[0].ActivityTypeId;
                        var stepId = currentStep[0].Id;
                        //get the employees in that group
                        var emps = (from e in db.EmployeeDesks

                                    where e.GroupId == groupId && e.ActivityTypeId == activityTypeId
                                    orderby e.JobCount ascending
                                    select new EmployeeStepObject
                                    {
                                        EmployeeDeskId = e.Id,

                                        JobCount = e.JobCount

                                    }).ToList();




                        if (emps.Any())
                        {
                            var emp = emps.First();
                            //get the process track
                            var track = db.RecertificationProcesses.Find(trackId);


                            track.EmployeeId = emp.EmployeeDeskId;
                            track.StepId = stepId;
                            track.AssignedTime = DateTime.Now;
                            track.StepCode = nextStepSequence;
                          

                            db.RecertificationProcesses.Attach(track);
                            db.Entry(track).State = EntityState.Modified;


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



        

        public long UpdateWorkFlow(WorkFlowObject workFlow)
        {
            try
            {
                if (workFlow == null)
                {
                    return -2;
                }

                var workFlowEntity = ModelMapper.Map<WorkFlowObject, WorkFlow>(workFlow);
                if (workFlowEntity == null || workFlowEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.WorkFlows.Attach(workFlowEntity);
                    db.Entry(workFlowEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return workFlow.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<WorkFlowObject> GetWorkFlows()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var WorkFlows = db.WorkFlows.ToList();
                    if (!WorkFlows.Any())
                    {
                        return new List<WorkFlowObject>();
                    }
                    var objList = new List<WorkFlowObject>();
                    WorkFlows.ForEach(app =>
                    {
                        var WorkFlowObject = ModelMapper.Map<WorkFlow, WorkFlowObject>(app);
                        if (WorkFlowObject != null && WorkFlowObject.Id > 0)
                        {
                            objList.Add(WorkFlowObject);
                        }
                    });

                    return !objList.Any() ? new List<WorkFlowObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<WorkFlowObject> GetWorkFlows(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var workFlows =
                            db.WorkFlows.OrderByDescending(m => m.Id)
                                .Skip(tpageNumber).Take(tsize)

                                .ToList();
                        if (workFlows.Any())
                        {
                            var newList = new List<WorkFlowObject>();
                            workFlows.ForEach(app =>
                            {
                                var workFlowObject = ModelMapper.Map<WorkFlow, WorkFlowObject>(app);
                                if (workFlowObject != null && workFlowObject.Id > 0)
                                {
                                    newList.Add(workFlowObject);
                                }
                            });
                            countG = db.WorkFlows.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<WorkFlowObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<WorkFlowObject>();
            }
        }


        public WorkFlowObject GetWorkFlow(long WorkFlowId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var WorkFlows =
                        db.WorkFlows.Where(m => m.Id == WorkFlowId)
                            .ToList();
                    if (!WorkFlows.Any())
                    {
                        return new WorkFlowObject();
                    }

                    var app = WorkFlows[0];
                    var WorkFlowObject = ModelMapper.Map<WorkFlow, WorkFlowObject>(app);
                    if (WorkFlowObject == null || WorkFlowObject.Id < 1)
                    {
                        return new WorkFlowObject();
                    }

                    return WorkFlowObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new WorkFlowObject();
            }
        }

        public List<WorkFlowObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var WorkFlows =
                        db.WorkFlows.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (WorkFlows.Any())
                    {
                        var newList = new List<WorkFlowObject>();
                        WorkFlows.ForEach(app =>
                        {
                            var WorkFlowObject = ModelMapper.Map<WorkFlow, WorkFlowObject>(app);
                            if (WorkFlowObject != null && WorkFlowObject.Id > 0)
                            {
                                newList.Add(WorkFlowObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<WorkFlowObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<WorkFlowObject>();
            }
        }

        public long DeleteWorkFlow(long WorkFlowId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.WorkFlows.Where(m => m.Id == WorkFlowId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.WorkFlows.Remove(item);
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
