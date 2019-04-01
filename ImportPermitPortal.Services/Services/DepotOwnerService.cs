using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class DepotOwnerServices
    {
        private readonly DepotOwnerManager _depotOwnerManager;
        public DepotOwnerServices()
        {
            _depotOwnerManager = new DepotOwnerManager();
        }

        public long AddDepotOwner(DepotOwnerObject depotOwner)
        {
            try
            {
                return _depotOwnerManager.AddDepotOwner(depotOwner);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public long DeleteDepotOwner(long depotOwnerId)
        {
            try
            {
                return _depotOwnerManager.DeleteDepotOwner(depotOwnerId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateDepotOwner(DepotOwnerObject depotOwner)
        {
            try
            {
                return _depotOwnerManager.UpdateDepotOwner(depotOwner);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<DepotOwnerObject> GetDepotOwners()
        {
            try
            {
                var objList = _depotOwnerManager.GetDepotOwners();
                if (objList == null || !objList.Any())
                {
                    return new List<DepotOwnerObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DepotOwnerObject>();
            }
        }

        public DepotOwnerObject GetDepotOwner(long depotOwnerId)
        {
            try
            {
                return _depotOwnerManager.GetDepotOwner(depotOwnerId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new DepotOwnerObject();
            }
        }

        public List<DepotOwnerObject> GetDepotOwners(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _depotOwnerManager.GetDepotOwners(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<DepotOwnerObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<DepotOwnerObject>();
            }
        }

        public List<DepotOwnerObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _depotOwnerManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<DepotOwnerObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DepotOwnerObject>();
            }
        }


       
    }

}
