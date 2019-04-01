using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class WorkFlowServices
    {
        private readonly WorkFlowManager _workFlowManager;
        public WorkFlowServices()
        {
            _workFlowManager = new WorkFlowManager();
        }

        public bool AssignApplicationToEmployee(long applicationId)
        {
            try
            {
                return _workFlowManager.AssignApplicationToEmployee(applicationId);
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
                return _workFlowManager.AssignApplicationToEmployeeInTrack(applicationId, nextStepSequence, trackId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public bool AssignRecertificationToEmployeeInTrack(long applicationId, int nextStepSequence, long trackId)
        {
            try
            {
                return _workFlowManager.AssignRecertificationToEmployeeInTrack(applicationId, nextStepSequence, trackId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return false;
            }
        }


        public bool AssignNotificationToEmployee(long notificationId)
        {
            try
            {
                return _workFlowManager.AssignNotificationToEmployee(notificationId);
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
                return _workFlowManager.AssignNotificationToEmployeeInTrack(notificationId, nextStepSequence, trackId);
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
                return _workFlowManager.UpdateWorkFlow(workFlow);
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
                var objList = _workFlowManager.GetWorkFlows();
                if (objList == null || !objList.Any())
                {
                    return new List<WorkFlowObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<WorkFlowObject>();
            }
        }

        public WorkFlowObject GetWorkFlow(long workFlowId)
        {
            try
            {
                return _workFlowManager.GetWorkFlow(workFlowId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new WorkFlowObject();
            }
        }

        public List<WorkFlowObject> GetWorkFlows(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _workFlowManager.GetWorkFlows(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<WorkFlowObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<WorkFlowObject>();
            }
        }

        public List<WorkFlowObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _workFlowManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<WorkFlowObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<WorkFlowObject>();
            }
        }
    }

}
