using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class CustomCodeServices
	{
        private readonly CustomCodeManager _customCodeManager;
        public CustomCodeServices()
		{
            _customCodeManager = new CustomCodeManager();
		}

        public long AddCustomCode(CustomCodeObject customCode)
		{
			try
			{
                return _customCodeManager.AddCustomCode(customCode);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteCustomCode(long customCodeId)
        {
            try
            {
                return _customCodeManager.DeleteCustomCode(customCodeId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateCustomCode(CustomCodeObject customCode)
        {
            try
            {
                return _customCodeManager.UpdateCustomCode(customCode);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<CustomCodeObject> GetCustomCodes()
		{
			try
			{
                var objList = _customCodeManager.GetCustomCodes();
                if (objList == null || !objList.Any())
			    {
                    return new List<CustomCodeObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<CustomCodeObject>();
			}
		}

        public CustomCodeObject GetCustomCode(long customCodeId)
        {
            try
            {
                return _customCodeManager.GetCustomCode(customCodeId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new CustomCodeObject();
            }
        }

        public List<CustomCodeObject> GetCustomCodes(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _customCodeManager.GetCustomCodes(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<CustomCodeObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<CustomCodeObject>();
            }
        }

        public List<CustomCodeObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _customCodeManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<CustomCodeObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<CustomCodeObject>();
            }
        }
	}

}
