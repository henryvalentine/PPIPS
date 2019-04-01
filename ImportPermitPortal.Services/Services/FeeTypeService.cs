using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class FeeTypeServices
	{
        private readonly FeeTypeManager _feeTypeManager;
        public FeeTypeServices()
		{
            _feeTypeManager = new FeeTypeManager();
		}

        public long AddFeeType(FeeTypeObject feeType)
		{
			try
			{
                return _feeTypeManager.AddFeeType(feeType);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteFeeType(long feeTypeId)
        {
            try
            {
                return _feeTypeManager.DeleteFeeType(feeTypeId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateFeeType(FeeTypeObject feeType)
        {
            try
            {
                return _feeTypeManager.UpdateFeeType(feeType);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<FeeTypeObject> GetFeeTypes()
		{
			try
			{
                var objList = _feeTypeManager.GetFeeTypes();
                if (objList == null || !objList.Any())
			    {
                    return new List<FeeTypeObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<FeeTypeObject>();
			}
		}

        public FeeTypeObject GetFeeType(long feeTypeId)
        {
            try
            {
                return _feeTypeManager.GetFeeType(feeTypeId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new FeeTypeObject();
            }
        }

        public UserProfileObject GetUser(string id)
        {
            try
            {
                return _feeTypeManager.GetUser(id);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new UserProfileObject();
            }
        }

        public List<FeeTypeObject> GetFeeTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _feeTypeManager.GetFeeTypes(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<FeeTypeObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<FeeTypeObject>();
            }
        }

        public List<FeeTypeObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _feeTypeManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<FeeTypeObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<FeeTypeObject>();
            }
        }
	}

}
