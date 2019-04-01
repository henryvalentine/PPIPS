using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class JettyServices
    {
        private readonly JettyManager _jettyManager;
        public JettyServices()
        {
            _jettyManager = new JettyManager();
        }

        public long AddJetty(JettyObject jetty)
        {
            try
            {
                return _jettyManager.AddJetty(jetty);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteJetty(long jettyId)
        {
            try
            {
                return _jettyManager.DeleteJetty(jettyId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateJetty(JettyObject jetty)
        {
            try
            {
                return _jettyManager.UpdateJetty(jetty);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public JettyObject GetJetty(long jettyId)
        {
            try
            {
                return _jettyManager.GetJetty(jettyId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new JettyObject();
            }
        }

        public int GetJettyIdByName(string jettyName)
        {
            try
            {
                return _jettyManager.GetJettyIdByName(jettyName);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<JettyObject> GetJetties()
        {
            try
            {
                return _jettyManager.GetJetties();

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<JettyObject>();
            }
        }

        public List<DepotObject> GetDeotList()
        {
            try
            {
                return _jettyManager.GetDepots();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DepotObject>();
            }
        }

        public List<JettyObject> GetJetties(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _jettyManager.GetJetties(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<JettyObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<JettyObject>();
            }
        }

        public List<JettyObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _jettyManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<JettyObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<JettyObject>();
            }
        }

    }

}
