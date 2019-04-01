using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class CountryServices
    {
        private readonly CountryManager _countryManager;
        public CountryServices()
        {
            _countryManager = new CountryManager();
        }

        public long AddCountry(CountryObject country)
        {
            try
            {
                return _countryManager.AddCountry(country);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteCountry(long countryId)
        {
            try
            {
                return _countryManager.DeleteCountry(countryId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateCountry(CountryObject country)
        {
            try
            {
                return _countryManager.UpdateCountry(country);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public CountryObject GetCountry(long countryId)
        {
            try
            {
                return _countryManager.GetCountry(countryId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new CountryObject();
            }
        }

        public List<CountryObject> GetCountries()
        {
            try
            {
                return _countryManager.GetCountries();

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<CountryObject>();
            }
        }

        public List<CountryObject> GetCountriesWithPorts()
        {
            try
            {
                return _countryManager.GetCountriesWithPorts();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<CountryObject>();
            }
        }
        public List<CountryObject> GetCountriesByRegion(int regionId)
        {
            try
            {
                return _countryManager.GetCountriesByRegion(regionId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<CountryObject>();
            }
        }

        public List<CountryObject> GetCountries(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _countryManager.GetCountries(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<CountryObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<CountryObject>();
            }
        }

        public List<CountryObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _countryManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<CountryObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<CountryObject>();
            }
        }

    }

}
