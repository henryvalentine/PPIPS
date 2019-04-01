using System;
using System.Collections.Generic;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class GeneralInformationServices
	{
        private readonly GeneralInformationManager _generalInformationManager;
        public GeneralInformationServices()
		{
            _generalInformationManager = new GeneralInformationManager();
		}

        public long ProcessGeneralInformation(GeneralInformationObject generalInformation)
		{
			try
			{
                return _generalInformationManager.ProcessCompanyProfileAndAddresses(generalInformation);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}


        public long ProcessCompanyAddresses(List<CompanyAddressObject> addresses, long importerId)
        {
            try
            {
                return _generalInformationManager.ProcessCompanyAddresses(addresses, importerId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }


        public List<CountryObject> GetCountries()  
        {
            try
            {
                return _generalInformationManager.GetCountries();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<CountryObject>();
            }
        }
        
        public List<StructureObject> GetStructures()
        {
            try
            {
                return _generalInformationManager.GetStructures();
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StructureObject>();
            }
        }
        
        public List<DocumentTypeObject> GetApplicantUnsuppliedDocumentTypes(long importerId)
        {
            try
            {
                return _generalInformationManager.GetApplicantUnsuppliedDocumentTypes(importerId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentTypeObject>();
            }
        }

        public List<StandardRequirementTypeObject> GetUStandardRequirements(long importerId)
        {
            try
            {
                return _generalInformationManager.GetUStandardRequirements(importerId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StandardRequirementTypeObject>();
            }
        }

        public bool CheckAddressAvailability(long importerId)
        {
            try
            {
                return _generalInformationManager.CheckAddressAvailability(importerId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return false;
            }
        }

        public bool DeleteCompanyAddressCheckReferences(long importerId, long importerAddressId)
        {
            try
            {
                return _generalInformationManager.DeleteCompanyAddressCheckReferences(importerId, importerAddressId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return false;
            }
        }

        public string GetImporterAddress(long importerId)
        {
            try
            {
                return _generalInformationManager.GetCompanyAddress(importerId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return "";
            }
        }

        public GeneralInformationObject GetCompanyProfileAndAddresses(long importerId)
		{
			try
			{
                return _generalInformationManager.GetCompanyProfileAndAddresses(importerId);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new GeneralInformationObject();
			}
		}

	}

}
