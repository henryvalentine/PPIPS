using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;

namespace ImportPermitPortal.Controllers 
{
    [Authorize(Roles = "Applicant,Support,Super_Admin,Employee")]
    public class GeneralInformationController : Controller
    {
        public ActionResult GetGeneralInformation()
        {
            var info = new GeneralInformationObject();
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    info.CompanyId = -1;
                    info.Name = "Your session has timed out";
                    return Json(info, JsonRequestBehavior.AllowGet);
                }
                //var token = GetAccessToken();
                //if (string.IsNullOrEmpty(token))
                //{
                //    info.CompanyId = -1;
                //    info.Name = "Your session has timed out";
                //    return Json(info, JsonRequestBehavior.AllowGet);
                //}

                var response = new GeneralInformationServices().GetCompanyProfileAndAddresses(importerInfo.Id);
                
                if (response == null || response.CompanyId < 1)
                {
                    info.CompanyId = -1;
                    info.Name = "Process failed. Please try again later.";
                    return Json(info, JsonRequestBehavior.AllowGet);
                }

                Session["_genInfo"] = response;
                response.BusinessCommencementDateStr = response.BusinessCommencementDate.ToString("dd/MM/yyyy");
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            catch (Exception)
            {
                info.CompanyId = -1;
                info.Name = "Process failed. Please try again later.";
                return Json(info, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetImporterInformation(long id)
        {
            var info = new GeneralInformationObject();
            try
            {
                var response = new GeneralInformationServices().GetCompanyProfileAndAddresses(id);

                if (response == null || response.CompanyId < 1)
                {
                    info.CompanyId = -1;
                    info.Name = "Process failed. Please try again later.";
                    return Json(info, JsonRequestBehavior.AllowGet);
                }

                Session["_genInfo"] = response;
                response.BusinessCommencementDateStr = response.BusinessCommencementDate.ToString("dd/MM/yyyy");
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            catch (Exception)
            {
                info.CompanyId = -1;
                info.Name = "Process failed. Please try again later.";
                return Json(info, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult DeleteAddress(long id)
        {
            var gVal = new GenericValidator();
            try
            {
                if (id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Invalid Operation. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Invalid Operation. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                //var token = GetAccessToken();
                //if (string.IsNullOrEmpty(token))
                //{
                //    gVal.Code = -1;
                //    gVal.Error = "Your session has timed out";
                //    return Json(gVal, JsonRequestBehavior.AllowGet);
                //}

                var response = new GeneralInformationServices().DeleteCompanyAddressCheckReferences(importerInfo.Id, id);
                
                if (response)
                {
                    gVal.Code = -1;
                    gVal.Error = "Address could not be deleted.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                gVal.Code = 5;
                gVal.Error = "Address was successfully deleted.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
           
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "An unknown error was encountered. Request could not be serviced. Please try again later.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetCountries()
        {
            try
            {
                var response = new GeneralInformationServices().GetCountries();
                
                if (response == null || !response.Any())
                {
                    return Json(new List<CountryObject>(), JsonRequestBehavior.AllowGet);
                }

                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<CountryObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetStructures()
        {
            try
            {
                var response = new GeneralInformationServices().GetStructures();
                
                if (response == null || !response.Any())
                {
                    return Json(new List<StructureObject>(), JsonRequestBehavior.AllowGet);
                }

                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<StructureObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpPost]
        public ActionResult ProcessGeneralInformation(GeneralInformationObject model)
        {
           var gVal = new GenericValidator();
            try
            {
               
                var status = ValidateInput(model);
                if (status.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = status.Error;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var response = new GeneralInformationServices().ProcessGeneralInformation(model);
               
                if (response < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = response == -3 ? "A different Company with the same Name already exists.": "Process failed. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                gVal.Code = 5;
                gVal.Error = "Company Information was successfully updated.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "An unknown error was encountered. Please try again";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ProcessCompanyAddresses(List<CompanyAddressObject> addresses)
        {
            var gVal = new GenericValidator();
            try
            {
                if (!addresses.Any())
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide at least one Address.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Invalid Operation. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                var count = 0;
                addresses.ForEach(o =>
                {
                    if (o.CityId > 0 && !string.IsNullOrEmpty(o.AddressLine1))
                    {
                        count++;
                    }
                });

                if (count != addresses.Count())
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide all required input and try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var response = new GeneralInformationServices().ProcessCompanyAddresses(addresses, importerInfo.Id);

                if (response < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = response == -3 ? "Duplicate encountered." : "Process failed. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Address was successfully processed.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "An unknown error was encountered. Please try again";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CheckElligibility()
        {
            var gVal = new GenericValidator();
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Invalid Operation. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var response = new GeneralInformationServices().GetApplicantUnsuppliedDocumentTypes(importerInfo.Id);
                if (!response.Any())
                {
                    gVal.DocumentTypeObjects = new List<DocumentTypeObject>();
                }
                else
                {
                   gVal.DocumentTypeObjects = response;
                }
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "An unknown error was encountered. Request could not be serviced. Please try again later.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CheckImporterElligibility(long id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new List<StandardRequirementTypeObject>(), JsonRequestBehavior.AllowGet);
                }
                
                var response = new GeneralInformationServices().GetUStandardRequirements(id);
                if (!response.Any())
                {
                    return Json(new List<StandardRequirementTypeObject>(), JsonRequestBehavior.AllowGet);
                }
               
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<StandardRequirementTypeObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CheckAddressAvailability()
        {
            var gVal = new GenericValidator();
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Invalid Operation. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var response = new GeneralInformationServices().CheckAddressAvailability(importerInfo.Id);
                gVal.IsAddressProvided = response;
                gVal.Code = response? 5 : -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "An unknown error was encountered. Request could not be serviced. Please try again later.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
         
        private ImporterObject GetLoggedOnUserInfo()
        {
            try
            {
                if (Session["_importerInfo"] == null)
                {
                    return new ImporterObject();
                }

                var importerInfo = Session["_importerInfo"] as ImporterObject;
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return new ImporterObject();
                }

                return importerInfo;

            }
            catch (Exception)
            {
                return new ImporterObject();
            }
        }

        private string GetAccessToken()
        {
            try
            {
                if (Session["_token"] == null)
                {
                    return "";
                }

                var token = (string)Session["_token"];
                if (string.IsNullOrEmpty(token))
                {
                    return "";
                }

                return token;

            }
            catch (Exception)
            {
                return "";
            }
        }

        private GenericValidator ValidateInput(GeneralInformationObject model)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Name";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.RCNumber))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Registeration Code";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.TIN))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Tax Identification Number";
                    return gVal;
                }

                if (model.CompanyId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Error: An unknown error was encountered. Please try again";
                    return gVal;
                }

                if (model.StructureId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select business Category";
                    return gVal;
                }

                if (!model.CompanyAddressObjects.Any())
                {
                    gVal.Code = -1;
                    gVal.Error = "Please Please provide company Address.";
                    return gVal;
                }

                foreach (var add in model.CompanyAddressObjects)
                {
                    if (add.AddressTypeId < 1)
                    {
                        gVal.Code = -1;
                        gVal.Error = "Select Address Type.";
                        return gVal;
                    }

                    if (add.CityId < 1)
                    {
                        gVal.Code = -1;
                        gVal.Error = "Please Select a City.";
                        return gVal;
                    }

                    if (string.IsNullOrEmpty(add.AddressLine1))
                    {
                        gVal.Code = -1;
                        gVal.Error = "Please provide Address";
                        return gVal;
                    }
                }

                gVal.Code = 5;
                return gVal;
            }
            catch (Exception ex)
            {
                gVal.Code = -1;
                gVal.Error = "Validation failed. Please try again.";
                return gVal;
            }
        }
    }
}
