using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class ProcessServices
    {
        private readonly ProcessManager _processManager;
        public ProcessServices()
        {
            _processManager = new ProcessManager();
        }

        public long AddProcess(ProcessObject process)
        {
            try
            {
                return _processManager.AddProcess(process);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteProcess(long processId)
        {
            try
            {
                return _processManager.DeleteProcess(processId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateProcess(ProcessObject process)
        {
            try
            {
                return _processManager.UpdateProcess(process);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<ProcessObject> GetProcesss()
        {
            try
            {
                var objList = _processManager.GetProcesss();
                if (objList == null || !objList.Any())
                {
                    return new List<ProcessObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProcessObject>();
            }
        }

        public ProcessObject GetProcess(long processId)
        {
            try
            {
                return _processManager.GetProcess(processId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ProcessObject();
            }
        }

        public List<ProcessObject> GetProcesss(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _processManager.GetProcesss(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ProcessObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ProcessObject>();
            }
        }

        public List<ProcessObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _processManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ProcessObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProcessObject>();
            }
        }
    }

}
