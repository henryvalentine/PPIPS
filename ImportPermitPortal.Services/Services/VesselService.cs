using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class VesselServices
    {
        private readonly VesselManager _vesselManager;
        public VesselServices()
        {
            _vesselManager = new VesselManager();
        }

        public long AddVessel(VesselObject vessel)
        {
            try
            {
                return _vesselManager.AddVessel(vessel);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddVessel(List<VesselObject> vessels)
        {
            try
            {
                return _vesselManager.AddVessel(vessels);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteVessel(long vesselId)
        {
            try
            {
                return _vesselManager.DeleteVessel(vesselId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateVessel(VesselObject vessel)
        {
            try
            {
                return _vesselManager.UpdateVessel(vessel);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<VesselObject> GetVessels()
        {
            try
            {
                var objList = _vesselManager.GetVessels();
                if (objList == null || !objList.Any())
                {
                    return new List<VesselObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<VesselObject>();
            }
        }

        public List<VesselObject> GetValidShuttleVessels()
        {
            try
            {
                return _vesselManager.GetValidShuttleVessels();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<VesselObject>();
            }
        }

        public VesselObject GetVessel(long vesselId)
        {
            try
            {
                return _vesselManager.GetVessel(vesselId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new VesselObject();
            }
        }

        public List<VesselObject> GetVessels(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _vesselManager.GetVessels(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<VesselObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<VesselObject>();
            }
        }

        public List<VesselObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _vesselManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<VesselObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<VesselObject>();
            }
        }
    }

}
