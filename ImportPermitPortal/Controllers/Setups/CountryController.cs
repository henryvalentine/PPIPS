using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;

namespace ImportPermitPortal.Controllers
{
    [Authorize]
    public class CountryController : Controller
    {
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
        [HttpGet]
        public ActionResult GetCountryObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<CountryObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetCountrys(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new CountryServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<CountryObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<CountryObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name : c.RegionName);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.Name, c.CountryCode, c.RegionName };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = countG,
                    iTotalDisplayRecords = countG,
                    aaData = result
                },
                   JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<CountryObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AddCountry(CountryObject country)
        {
            var gVal = new GenericValidator();

            try
            {
               

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var validationResult = ValidateCountry(country);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
              
                

                var appStatus = new CountryServices().AddCountry(country);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Country upload failed. Please try again." : "The Country Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Country was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Country processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditCountry(CountryObject country)
        {
            var gVal = new GenericValidator();

            try
            {
                //if (!ModelState.IsValid)
                //{
                //    gVal.Code = -1;
                //    gVal.Error = "Plese provide all required fields and try again.";
                //    return Json(gVal, JsonRequestBehavior.AllowGet);
                //}

                var stat = ValidateCountry(country);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_country"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldcountry = Session["_country"] as CountryObject;

                if (oldcountry == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldcountry.Name = country.Name;
                oldcountry.CountryCode = country.CountryCode;
                oldcountry.RegionId = country.RegionId;
                
                var docStatus = new CountryServices().UpdateCountry(oldcountry);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Country already exists." : "Country information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldcountry.Id;
                gVal.Error = "Country information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Country information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericCountryList()
                {
                   
                    Regions = GetRegions()
                };

                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetCountry(long id)
        {
            try
            {


                var country = new CountryServices().GetCountry(id);
                if (country == null || country.Id < 1)
                {
                    return Json(new CountryObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_country"] = country;

                return Json(country, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new CountryObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public List<CountryObject> GetCountrys()
        {
            try
            {


                var country = new CountryServices().GetCountries();
             

                Session["_country"] = country;

                return new List<CountryObject>();

            }
            catch (Exception)
            {
                return new List<CountryObject>();
            }
        }

        [HttpPost]
        public ActionResult DeleteCountry(long id)
        {
            var gVal = new GenericValidator();
            try
            {
                if (id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Invalid selection";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                var delStatus = new CountryServices().DeleteCountry(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Country could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Country Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

       

       

      
       
        private GenericValidator ValidateCountry(CountryObject country)
        {
            var gVal = new GenericValidator();
            try
            {
                if (country.RegionId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a Region.";
                    return gVal;
                }

               

                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "Country Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<CountryObject> GetCountrys(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new CountryServices().GetCountries(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<CountryObject>();
            }
        }

        private List<RegionObject> GetRegions()
        {
            try
            {
                return new RegionServices().GetRegions();

            }
            catch (Exception)
            {
                return new List<RegionObject>();
            }
        }

       

    }
}
