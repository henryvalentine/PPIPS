using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class StorageTankServices
    {
        private readonly StorageTankManager _storageTankManager;
        public StorageTankServices()
        {
            _storageTankManager = new StorageTankManager();
        }

        public long AddStorageTank(StorageTankObject storageTank)
        {
            try
            {
                return _storageTankManager.AddStorageTank(storageTank);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public long DeleteStorageTank(long storageTankId)
        {
            try
            {
                return _storageTankManager.DeleteStorageTank(storageTankId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateStorageTank(StorageTankObject storageTank)
        {
            try
            {
                return _storageTankManager.UpdateStorageTank(storageTank);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<StorageTankObject> GetStorageTanks(long id)
        {
            try
            {
                var objList = _storageTankManager.GetStorageTanks(id);
                if (objList == null || !objList.Any())
                {
                    return new List<StorageTankObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StorageTankObject>();
            }
        }

        public StorageTankObject GetStorageTank(long storageTankId)
        {
            try
            {
                return _storageTankManager.GetStorageTank(storageTankId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new StorageTankObject();
            }
        }

        public List<StorageTankObject> GetStorageTanks(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _storageTankManager.GetStorageTanks(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<StorageTankObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<StorageTankObject>();
            }
        }

        public List<StorageTankObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _storageTankManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<StorageTankObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StorageTankObject>();
            }
        }


       
    }

}
