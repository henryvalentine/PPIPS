using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class JettyMappingServices
    {
        private readonly JettyMappingManager _jettyMappingManager;
        public JettyMappingServices()
        {
            _jettyMappingManager = new JettyMappingManager();
        }

        public long AddJettyMapping(JettyMappingObject jettyMapping)
        {
            try
            {
                return _jettyMappingManager.AddJettyMapping(jettyMapping);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteJettyMapping(long jettyMappingId)
        {
            try
            {
                return _jettyMappingManager.DeleteJettyMapping(jettyMappingId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateJettyMapping(JettyMappingObject jettyMapping)
        {
            try
            {
                return _jettyMappingManager.UpdateJettyMapping(jettyMapping);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

     
      

        public JettyMappingObject GetJettyMapping(long jettyMappingId)
        {
            try
            {
                return _jettyMappingManager.GetJettyMapping(jettyMappingId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new JettyMappingObject();
            }
        }

        public List<JettyMappingObject> GetJettyMappings(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _jettyMappingManager.GetJettyMappings(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<JettyMappingObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<JettyMappingObject>();
            }
        }

        public List<JettyMappingObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _jettyMappingManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<JettyMappingObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<JettyMappingObject>();
            }
        }

    }

}
