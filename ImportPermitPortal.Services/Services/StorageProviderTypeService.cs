using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class StorageProviderTypeServices
    {
        private readonly StorageProviderTypeManager _storageProviderTypeManager;
        public StorageProviderTypeServices()
        {
            _storageProviderTypeManager = new StorageProviderTypeManager();
        }

        public long AddStorageProviderType(StorageProviderTypeObject storageProviderType)
        {
            try
            {
                return _storageProviderTypeManager.AddStorageProviderType(storageProviderType);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteStorageProviderType(long storageProviderTypeId)
        {
            try
            {
                return _storageProviderTypeManager.DeleteStorageProviderType(storageProviderTypeId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateStorageProviderType(StorageProviderTypeObject storageProviderType, List<StorageProviderRequirementObject> newRequirements)
        {
            try
            {
                return _storageProviderTypeManager.UpdateStorageProviderType(storageProviderType, newRequirements);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<StorageProviderTypeObject> GetStorageProviderTypes()
        {
            try
            {
                var objList = _storageProviderTypeManager.GetStorageProviderTypes();
                if (objList == null || !objList.Any())
                {
                    return new List<StorageProviderTypeObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StorageProviderTypeObject>();
            }
        }

        public StorageProviderTypeObject GetStorageProviderType(long storageProviderTypeId)
        {
            try
            {
                return _storageProviderTypeManager.GetStorageProviderType(storageProviderTypeId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new StorageProviderTypeObject();
            }
        }
        public List<StorageProviderTypeObject> GetStorageProviderTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _storageProviderTypeManager.GetStorageProviderTypes(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<StorageProviderTypeObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<StorageProviderTypeObject>();
            }
        }

        public List<StorageProviderTypeObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _storageProviderTypeManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<StorageProviderTypeObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StorageProviderTypeObject>();
            }
        }
    }

}
