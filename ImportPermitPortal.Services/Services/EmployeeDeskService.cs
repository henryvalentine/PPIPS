using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class EmployeeDeskServices  
    {
        private readonly EmployeeDeskManager _employeeDeskManager;
        public EmployeeDeskServices()
        {
            _employeeDeskManager = new EmployeeDeskManager();
        }

        public long AddEmployeeDesk(EmployeeDeskObject employeeDesk)
        {
            try
            {
                return _employeeDeskManager.AddEmployeeDesk(employeeDesk);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteEmployeeDesk(long employeeDeskId)
        {
            try
            {
                return _employeeDeskManager.DeleteEmployeeDesk(employeeDeskId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateEmployeeDesk(EmployeeDeskObject employeeDesk)
        {
            try
            {
                return _employeeDeskManager.UpdateEmployeeDesk(employeeDesk);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<EmployeeDeskObject> GetEmployeeDesks()
        {
            try
            {
                var objList = _employeeDeskManager.GetEmployeeDesks();
                if (objList == null || !objList.Any())
                {
                    return new List<EmployeeDeskObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<EmployeeDeskObject>();
            }
        }


        public UserProfileObject GetAppUser(long id)
        {
            try
            {
                return _employeeDeskManager.GetAppUser(id);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new UserProfileObject();
            }
        }


        public EmployeeDeskObject GetEmployeeDesk(long employeeDeskId)
        {
            try
            {
                return _employeeDeskManager.GetEmployeeDesk(employeeDeskId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new EmployeeDeskObject();
            }
        }

        public List<EmployeeDeskObject> GetEmployeeDesks(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _employeeDeskManager.GetEmployeeDesks(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<EmployeeDeskObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<EmployeeDeskObject>();
            }
        }

        public List<UserProfileObject> GetAppUsers(int? itemsPerPage, int? pageNumber, out int countG, long id)
        {
            try
            {
                var objList = _employeeDeskManager.GetAppUsers(itemsPerPage, pageNumber, out countG, id);
                if (objList == null || !objList.Any())
                {
                    return new List<UserProfileObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<UserProfileObject>();
            }
        }

        public List<UserProfileObject> SearchAppUsers(string searchCriteria, long id)
        {
            try
            {
                var objList = _employeeDeskManager.SearchAppUsers(searchCriteria, id);
                if (objList == null || !objList.Any())
                {
                    return new List<UserProfileObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<UserProfileObject>();
            }
        }

        public List<EmployeeDeskObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _employeeDeskManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<EmployeeDeskObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<EmployeeDeskObject>();
            }
        }
    }

}
