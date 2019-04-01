using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class RegionServices
    {
        private readonly RegionManager _regionManager;
        public RegionServices()
        {
            _regionManager = new RegionManager();
        }

        public long AddRegion(RegionObject region)
        {
            try
            {
                return _regionManager.AddRegion(region);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public long DeleteRegion(long regionId)
        {
            try
            {
                return _regionManager.DeleteRegion(regionId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateRegion(RegionObject region)
        {
            try
            {
                return _regionManager.UpdateRegion(region);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<RegionObject> GetRegions()
        {
            try
            {
                var objList = _regionManager.GetRegions();
                if (objList == null || !objList.Any())
                {
                    return new List<RegionObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RegionObject>();
            }
        }

        public RegionObject GetRegion(long regionId)
        {
            try
            {
                return _regionManager.GetRegion(regionId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new RegionObject();
            }
        }

        public List<RegionObject> GetRegions(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _regionManager.GetRegions(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<RegionObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<RegionObject>();
            }
        }

        public List<RegionObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _regionManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<RegionObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RegionObject>();
            }
        }


       
    }

}
