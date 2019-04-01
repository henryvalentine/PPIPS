using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class PermitServices
    {
        private readonly PermitManager _permitManager;
        public PermitServices()
        {
            _permitManager = new PermitManager();
        }

        public long AddPermit(PermitObject permit)
        {
            try
            {
                return _permitManager.AddPermit(permit);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeletePermit(long permitId)
        {
            try
            {
                return _permitManager.DeletePermit(permitId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddBulkPermit(PermitObject permit)
        {
            try
            {
                return _permitManager.AddBulkPermit(permit);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdatePermit(PermitObject permit)
        {
            try
            {
                return _permitManager.UpdatePermit(permit);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public ApplicationObject GetPermitInfo(long id)
        {
            try
            {
                return _permitManager.GetPermitInfo(id);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public PermitObject GetPermit(long permitId)
        {
            try
            {
                return _permitManager.GetPermit(permitId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new PermitObject();
            }
        }

        public List<PermitObject> GetPermits()
        {
            try
            {
                return _permitManager.GetPermits();

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<PermitObject>();
            }
        }

        public List<PermitObject> GetPermits(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _permitManager.GetPermits(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<PermitObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<PermitObject>();
            }
        }

        public List<PermitObject> GetApplicantPermits(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
                var objList = _permitManager.GetApplicantPermits(itemsPerPage, pageNumber, out countG, importerId);
                if (objList == null || !objList.Any())
                {
                    return new List<PermitObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<PermitObject>();
            }
        }
        
        public List<PermitObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _permitManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<PermitObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<PermitObject>();
            }
        }

        public List<PermitObject> SearchApplicantPermits(string searchCriteria, long importerId)
        {
            try
            {
                var objList = _permitManager.SearchApplicantPermits(searchCriteria, importerId);
                if (objList == null || !objList.Any())
                {
                    return new List<PermitObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<PermitObject>();
            }
        }
    }

}
