using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class FeeServices
	{
        private readonly FeeManager _feeManager;
        public FeeServices()
		{
            _feeManager = new FeeManager();
		}

        public long AddFee(FeeObject fee)
		{
			try
			{
                return _feeManager.AddFee(fee);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteFee(long feeId)
        {
            try
            {
                return _feeManager.DeleteFee(feeId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateFee(FeeObject fee)
        {
            try
            {
                return _feeManager.UpdateFee(fee);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<FeeObject> GetFees()
		{
			try
			{
                var objList = _feeManager.GetFees();
                if (objList == null || !objList.Any())
			    {
                    return new List<FeeObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<FeeObject>();
			}
		}

        public List<FeeObject> GetAppliationStageFees(out CalculationFactor calculationFactor)
        {
            try
            {
                return _feeManager.GetAppliationStageFees(out calculationFactor) ?? new List<FeeObject>();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                calculationFactor = new CalculationFactor();
                return new List<FeeObject>();
            }
        }

        public List<FeeObject> GetNotificationFees(out CalculationFactor calculationFactor)
        {
            try
            {
                return _feeManager.GetNotificationFees(out calculationFactor) ?? new List<FeeObject>();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                calculationFactor = new CalculationFactor();
                return new List<FeeObject>();
            }
        }

        public FeeObject GetFee(long feeId)
        {
            try
            {
                return _feeManager.GetFee(feeId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new FeeObject();
            }
        }

        public List<FeeObject> GetFees(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _feeManager.GetFees(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<FeeObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<FeeObject>();
            }
        }
        public List<FeeObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _feeManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<FeeObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<FeeObject>();
            }
        }

        public long LogPaymentDetails(List<PaymentDistributionSummaryObject> fees)
        {
            try
            {
                return _feeManager.LogPaymentDetails(fees);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
	}

}
