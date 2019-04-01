using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class DepotServices
    {
        private readonly DepotManager _depotManager;
        public DepotServices()
        {
            _depotManager = new DepotManager();
        }

        public long AddDepot(DepotObject depot)
        {
            try
            {
                return _depotManager.AddDepot(depot);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddDepot(List<DepotObject> depotList)
        {
            try
            {
                return _depotManager.AddDepot(depotList);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteDepot(long depotId)
        {
            try
            {
                return _depotManager.DeleteDepot(depotId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateDepot(DepotObject depot)
        {
            try
            {
                return _depotManager.UpdateDepot(depot);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public DepotObject GetDepot(long depotId)
        {
            try
            {
                return _depotManager.GetDepot(depotId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new DepotObject();
            }
        }

        public List<DepotObject> GetDepots()
        {
            try
            {
                return _depotManager.GetDepots();

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DepotObject>();
            }
        }
        public List<DepotObject> GetDepots(long userProfileId)
        {
            try
            {
                return _depotManager.GetDepots(userProfileId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DepotObject>();
            }
        }
        
        public UserProfileObject GetDepotAdmin(int depotId)
        {
            try
            {
                return _depotManager.GetDepotAdmin(depotId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new UserProfileObject();
            }
        }

        public List<DepotObject> GetLocalDepots()
        {
            try
            {
                return _depotManager.GetLocalDepots();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DepotObject>();
            }
        }

        public List<DepotObject> GetDepots(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _depotManager.GetDepots(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<DepotObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<DepotObject>();
            }
        }

        public List<DepotObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _depotManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<DepotObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DepotObject>();
            }
        }

    }

}
