using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class StepServices
    {
        private readonly StepManager _stepManager;
        public StepServices()
        {
            _stepManager = new StepManager();
        }

        public long AddStep(StepObject step)
        {
            try
            {
                return _stepManager.AddStep(step);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteStep(long stepId)
        {
            try
            {
                return _stepManager.DeleteStep(stepId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateStep(StepObject step)
        {
            try
            {
                return _stepManager.UpdateStep(step);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<StepObject> GetSteps()
        {
            try
            {
                var objList = _stepManager.GetSteps();
                if (objList == null || !objList.Any())
                {
                    return new List<StepObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StepObject>();
            }
        }


     

        public StepObject GetStep(long stepId)
        {
            try
            {
                return _stepManager.GetStep(stepId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new StepObject();
            }
        }

        public List<StepObject> GetSteps(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _stepManager.GetSteps(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<StepObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<StepObject>();
            }
        }




        public List<StepObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _stepManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<StepObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StepObject>();
            }
        }
    }

}
