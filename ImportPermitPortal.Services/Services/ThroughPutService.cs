using System;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class ThroughPutServices
	{
        private readonly ThroughPutManager _throughPutManager;
        public ThroughPutServices()
		{
            _throughPutManager = new ThroughPutManager();
		}

        public long AddThroughPut(ThroughPutObject throughPut)
		{
			try
			{
                return _throughPutManager.AddThroughPut(throughPut);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public ThroughPutObject GetThroughPut(long applicationId)
        {
            try
            {
                return _throughPutManager.GetThroughPut(applicationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ThroughPutObject();
            }
        }

        public long DeleteThroughPut(long throughPutId)
        {
            try
            {
                return _throughPutManager.DeleteThroughPut(throughPutId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateThroughPut(ThroughPutObject throughPut)
        {
            try
            {
                return _throughPutManager.UpdateThroughPut(throughPut);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
       
	}

}
