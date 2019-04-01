using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class StandardRequirementServices
	{
        private readonly StandardRequirementManager _sReqManager;
        public StandardRequirementServices()
		{
            _sReqManager = new StandardRequirementManager();
		}

        public long AddStandardRequirement(StandardRequirementObject sReq)
		{
			try
			{
                return _sReqManager.AddStandardRequirement(sReq);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public string DeleteStandardRequirement(long sReqId)
        {
            try
            {
                return _sReqManager.DeleteStandardRequirement(sReqId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return "";
            }
        }

        public long UpdateStandardRequirement(StandardRequirementObject sReq)
        {
            try
            {
                return _sReqManager.UpdateStandardRequirement(sReq);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<StandardRequirementObject> GetStandardRequirements(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
		{
			try
			{
                return _sReqManager.GetStandardRequirements(itemsPerPage, pageNumber, out countG, importerId);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
			    countG = 0;
                return new List<StandardRequirementObject>();
			}
		}

        public StandardRequirementObject GetStandardRequirement(long sReqId)
        {
            try
            {
                return _sReqManager.GetStandardRequirement(sReqId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new StandardRequirementObject();
            }
        }
      
        public List<StandardRequirementObject> Search(string searchCriteria, long importerId)
        {
            try
            {
                var objList = _sReqManager.Search(searchCriteria, importerId);
                if (objList == null || !objList.Any())
                {
                    return new List<StandardRequirementObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StandardRequirementObject>();
            }
        }
	}

}
