using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class UnitOfMeasurementServices
    {
        private readonly UnitOfMeasurementManager _unitOfMeasurementManager;
        public UnitOfMeasurementServices()
        {
            _unitOfMeasurementManager = new UnitOfMeasurementManager();
        }

        public long AddUnitOfMeasurement(UnitOfMeasurementObject unitOfMeasurement)
        {
            try
            {
                return _unitOfMeasurementManager.AddUnitOfMeasurement(unitOfMeasurement);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public long DeleteUnitOfMeasurement(long unitOfMeasurementId)
        {
            try
            {
                return _unitOfMeasurementManager.DeleteUnitOfMeasurement(unitOfMeasurementId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateUnitOfMeasurement(UnitOfMeasurementObject unitOfMeasurement)
        {
            try
            {
                return _unitOfMeasurementManager.UpdateUnitOfMeasurement(unitOfMeasurement);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<UnitOfMeasurementObject> GetUnitOfMeasurements()
        {
            try
            {
                var objList = _unitOfMeasurementManager.GetUnitOfMeasurements();
                if (objList == null || !objList.Any())
                {
                    return new List<UnitOfMeasurementObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<UnitOfMeasurementObject>();
            }
        }

        public UnitOfMeasurementObject GetUnitOfMeasurement(long unitOfMeasurementId)
        {
            try
            {
                return _unitOfMeasurementManager.GetUnitOfMeasurement(unitOfMeasurementId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new UnitOfMeasurementObject();
            }
        }

        public List<UnitOfMeasurementObject> GetUnitOfMeasurements(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _unitOfMeasurementManager.GetUnitOfMeasurements(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<UnitOfMeasurementObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<UnitOfMeasurementObject>();
            }
        }

        public List<UnitOfMeasurementObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _unitOfMeasurementManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<UnitOfMeasurementObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<UnitOfMeasurementObject>();
            }
        }


       
    }

}
