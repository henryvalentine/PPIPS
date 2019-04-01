using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class PortServices
    {
        private readonly PortManager _portManager;
        public PortServices()
        {
            _portManager = new PortManager();
        }

        public long AddPort(PortObject port)
        {
            try
            {
                return _portManager.AddPort(port);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeletePort(long portId)
        {
            try
            {
                return _portManager.DeletePort(portId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdatePort(PortObject port)
        {
            try
            {
                return _portManager.UpdatePort(port);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }




        public PortObject GetPort(long portId)
        {
            try
            {
                return _portManager.GetPort(portId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new PortObject();
            }
        }

        public List<PortObject> GetPorts()
        {
            try
            {
                return _portManager.GetPorts();

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<PortObject>();
            }
        }

        public List<PortObject> GetPorts(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _portManager.GetPorts(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<PortObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<PortObject>();
            }
        }

        public List<PortObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _portManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<PortObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<PortObject>();
            }
        }

    }

}
