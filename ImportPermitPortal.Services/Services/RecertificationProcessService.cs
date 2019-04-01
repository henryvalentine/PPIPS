using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class RecertificationProcessServices
    {
        private readonly RecertificationProcessManager _recertificationProcessManager;
        public RecertificationProcessServices()
        {
            _recertificationProcessManager = new RecertificationProcessManager();
        }

        public long AddRecertificationProcess(RecertificationProcessObject recertificationProcess)
        {
            try
            {
                return _recertificationProcessManager.AddRecertificationProcess(recertificationProcess);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteRecertificationProcess(long recertificationProcessId)
        {
            try
            {
                return _recertificationProcessManager.DeleteRecertificationProcess(recertificationProcessId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateRecertificationProcess(RecertificationProcessObject recertificationProcess)
        {
            try
            {
                return _recertificationProcessManager.UpdateRecertificationProcess(recertificationProcess);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<RecertificationProcessObject> GetRecertificationProcesss()
        {
            try
            {
                var objList = _recertificationProcessManager.GetRecertificationProcesss();
                if (objList == null || !objList.Any())
                {
                    return new List<RecertificationProcessObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RecertificationProcessObject>();
            }
        }

        public RecertificationProcessObject GetRecertificationProcess(long recertificationProcessId)
        {
            try
            {
                return _recertificationProcessManager.GetRecertificationProcess(recertificationProcessId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new RecertificationProcessObject();
            }
        }

        public List<RecertificationProcessObject> GetRecertificationProcesss(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _recertificationProcessManager.GetRecertificationProcesss(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<RecertificationProcessObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<RecertificationProcessObject>();
            }
        }

        public List<RecertificationProcessObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _recertificationProcessManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<RecertificationProcessObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RecertificationProcessObject>();
            }
        }
    }

}
