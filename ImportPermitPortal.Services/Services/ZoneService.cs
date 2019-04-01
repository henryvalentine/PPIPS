using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class ZoneServices
    {
        private readonly ZoneManager _zoneManager;
        public ZoneServices()
        {
            _zoneManager = new ZoneManager();
        }

        public long AddZone(ZoneObject zone)
        {
            try
            {
                return _zoneManager.AddZone(zone);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public long DeleteZone(long zoneId)
        {
            try
            {
                return _zoneManager.DeleteZone(zoneId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateZone(ZoneObject zone)
        {
            try
            {
                return _zoneManager.UpdateZone(zone);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<ZoneObject> GetZones()
        {
            try
            {
                var objList = _zoneManager.GetZones();
                if (objList == null || !objList.Any())
                {
                    return new List<ZoneObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ZoneObject>();
            }
        }

        public ZoneObject GetZone(long zoneId)
        {
            try
            {
                return _zoneManager.GetZone(zoneId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ZoneObject();
            }
        }

        public List<ZoneObject> GetZones(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _zoneManager.GetZones(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ZoneObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ZoneObject>();
            }
        }

        public List<ZoneObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _zoneManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ZoneObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ZoneObject>();
            }
        }


       
    }

}
