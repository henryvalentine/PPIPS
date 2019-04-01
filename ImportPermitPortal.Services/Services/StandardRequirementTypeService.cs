using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class StandardRequirementTypeServices
	{
        private readonly StandardRequirementTypeManager _sReqTypeManager;
        public StandardRequirementTypeServices()
		{
            _sReqTypeManager = new StandardRequirementTypeManager();
		}

        public long AddStandardRequirementType(StandardRequirementTypeObject sReqType)
		{
			try
			{
                return _sReqTypeManager.AddStandardRequirementType(sReqType);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteStandardRequirementType(long sReqTypeId)
        {
            try
            {
                return _sReqTypeManager.DeleteStandardRequirementType(sReqTypeId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateStandardRequirementType(StandardRequirementTypeObject sReqType)
        {
            try
            {
                return _sReqTypeManager.UpdateStandardRequirementType(sReqType);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<StandardRequirementTypeObject> GetStandardRequirementTypes()
		{
			try
			{
                var objList = _sReqTypeManager.GetStandardRequirementTypes();
                if (objList == null || !objList.Any())
			    {
                    return new List<StandardRequirementTypeObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StandardRequirementTypeObject>();
			}
		}

        public StandardRequirementTypeObject GetStandardRequirementType(long sReqTypeId)
        {
            try
            {
                return _sReqTypeManager.GetStandardRequirementType(sReqTypeId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new StandardRequirementTypeObject();
            }
        }

        public List<StandardRequirementTypeObject> GetStandardRequirementTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _sReqTypeManager.GetStandardRequirementTypes(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<StandardRequirementTypeObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<StandardRequirementTypeObject>();
            }
        }

        public List<StandardRequirementTypeObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _sReqTypeManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<StandardRequirementTypeObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StandardRequirementTypeObject>();
            }
        }
	}

}
