using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class RecertificationServices
    {
        private readonly RecertificationManager _RecertificationManager;
        public RecertificationServices()
        {
            _RecertificationManager = new RecertificationManager();
        }

        public long AddRecertification(RecertificationObject Recertification)
        {
            try
            {
                return _RecertificationManager.AddRecertification(Recertification);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteRecertification(long RecertificationId)
        {
            try
            {
                return _RecertificationManager.DeleteRecertification(RecertificationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateRecertification(RecertificationObject Recertification)
        {
            try
            {
                return _RecertificationManager.UpdateRecertification(Recertification);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public RecertificationObject GetRecertification(long RecertificationId)
        {
            try
            {
                return _RecertificationManager.GetRecertification(RecertificationId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new RecertificationObject();
            }
        }

        public List<DocumentObject> GetRecertificationDocs(long RecertificationId)
        {
            try
            {
                return _RecertificationManager.GetRecertificationDocs(RecertificationId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentObject>();
            }
        }

        public List<RecertificationObject> GetRecertifications()
        {
            try
            {
                return _RecertificationManager.GetRecertifications();

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RecertificationObject>();
            }
        }
        public List<RecertificationObject> GetRecertifications(long userProfileId)
        {
            try
            {
                return _RecertificationManager.GetRecertifications(userProfileId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RecertificationObject>();
            }
        }

        public List<RecertificationObject> GetLocalRecertifications()
        {
            try
            {
                return _RecertificationManager.GetRecertifications();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RecertificationObject>();
            }
        }

        public List<RecertificationObject> GetAdminRecertifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _RecertificationManager.GetAdminRecertifications(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<RecertificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<RecertificationObject>();
            }
        }

        public List<RecertificationObject> GetUserRecertifications(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
                var objList = _RecertificationManager.GetUserRecertifications(itemsPerPage, pageNumber, out countG, importerId);
                if (objList == null || !objList.Any())
                {
                    return new List<RecertificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<RecertificationObject>();
            }
        }

        public List<RecertificationObject> Search(string searchCriteria, long id)
        {
            try
            {
                var objList = _RecertificationManager.Search(searchCriteria, id);
                if (objList == null || !objList.Any())
                {
                    return new List<RecertificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RecertificationObject>();
            }
        }

        public List<RecertificationObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _RecertificationManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<RecertificationObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<RecertificationObject>();
            }
        }

    }

}
