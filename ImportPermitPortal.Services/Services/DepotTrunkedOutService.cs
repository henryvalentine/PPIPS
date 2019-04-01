using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class DepotTrunkedOutServices
    {
        private readonly DepotTrunkedOutManager _depotTrunkedOutManager;
        public DepotTrunkedOutServices()
        {
            _depotTrunkedOutManager = new DepotTrunkedOutManager();
        }

        public long AddDepotTrunkedOut(DepotTrunkedOutObject depotTrunkedOut)
        {
            try
            {
                return _depotTrunkedOutManager.AddDepotTrunkedOut(depotTrunkedOut);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteDepotTrunkedOut(long depotTrunkedOutId)
        {
            try
            {
                return _depotTrunkedOutManager.DeleteDepotTrunkedOut(depotTrunkedOutId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateDepotTrunkedOut(DepotTrunkedOutObject depotTrunkedOut)
        {
            try
            {
                return _depotTrunkedOutManager.UpdateDepotTrunkedOut(depotTrunkedOut);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public DepotTrunkedOutObject GetDepotTrunkedOut(long depotTrunkedOutId)
        {
            try
            {
                return _depotTrunkedOutManager.GetDepotTrunkedOut(depotTrunkedOutId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new DepotTrunkedOutObject();
            }
        }
        public List<DepotTrunkedOutObject> GetDepotTrunkedOuts()
        {
            try
            {
                return _depotTrunkedOutManager.GetDepotTrunkedOuts();

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DepotTrunkedOutObject>();
            }
        }

        public CalculatorObject Calculator(DepotTrunkedOutObject depotTrunkedOut)
        {
            try
            {
                return _depotTrunkedOutManager.Calculator(depotTrunkedOut);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new CalculatorObject();
            }
        }

        public List<DepotObject> GetDeotList()
        {
            try
            {
                return _depotTrunkedOutManager.GetDepots();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DepotObject>();
            }
        }

        public List<DepotTrunkedOutObject> GetDepotTrunkedOuts(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _depotTrunkedOutManager.GetDepotTrunkedOuts(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<DepotTrunkedOutObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<DepotTrunkedOutObject>();
            }
        }

        public List<DepotTrunkedOutObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _depotTrunkedOutManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<DepotTrunkedOutObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DepotTrunkedOutObject>();
            }
        }

    }

}
