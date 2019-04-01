using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class StepActivityTypeServices
    {
        private readonly StepActivityTypeManager _stepActivityTypeManager;
        public StepActivityTypeServices()
        {
            _stepActivityTypeManager = new StepActivityTypeManager();
        }

        public long AddStepActivityType(StepActivityTypeObject stepActivityType)
        {
            try
            {
                return _stepActivityTypeManager.AddStepActivityType(stepActivityType);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public long DeleteStepActivityType(long stepActivityTypeId)
        {
            try
            {
                return _stepActivityTypeManager.DeleteStepActivityType(stepActivityTypeId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateStepActivityType(StepActivityTypeObject stepActivityType)
        {
            try
            {
                return _stepActivityTypeManager.UpdateStepActivityType(stepActivityType);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<StepActivityTypeObject> GetStepActivityTypes()
        {
            try
            {
                var objList = _stepActivityTypeManager.GetStepActivityTypes();
                if (objList == null || !objList.Any())
                {
                    return new List<StepActivityTypeObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StepActivityTypeObject>();
            }
        }

        public StepActivityTypeObject GetStepActivityType(long stepActivityTypeId)
        {
            try
            {
                return _stepActivityTypeManager.GetStepActivityType(stepActivityTypeId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new StepActivityTypeObject();
            }
        }

        public List<StepActivityTypeObject> GetStepActivityTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _stepActivityTypeManager.GetStepActivityTypes(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<StepActivityTypeObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<StepActivityTypeObject>();
            }
        }

        public List<StepActivityTypeObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _stepActivityTypeManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<StepActivityTypeObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StepActivityTypeObject>();
            }
        }


       
    }

}
