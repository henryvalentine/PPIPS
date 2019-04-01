using System;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Services.Services;

namespace ImportPermitPortal.Controllers
{
    [Authorize(Roles = "Super_Admin")]
    public class ImportSettingController : Controller
    {
        public ActionResult AddApplicationSetting(ImportSettingObject applicationSetting)
        {
            var gVal = new GenericValidator();

            try
            {
                var validationResult = ValidateApplicationSetting(applicationSetting);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new ImportSettingServices().AddImportSetting(applicationSetting);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = "Import Setting could not be processed.";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Import Setting  was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Import Setting  processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditApplicationSetting(ImportSettingObject applicationSetting)
        {
            var gVal = new GenericValidator();

            try
            {
                if (!ModelState.IsValid)
                {
                    gVal.Code = -1;
                    gVal.Error = "Plese provide all required fields and try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var stat = ValidateApplicationSetting(applicationSetting);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_applicationSetting"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldapplicationSetting = Session["_applicationSetting"] as ImportSettingObject;

                if (oldapplicationSetting == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldapplicationSetting.ApplicationExpiry = applicationSetting.ApplicationExpiry;
                oldapplicationSetting.ApplicationLifeCycle = applicationSetting.ApplicationLifeCycle;
                oldapplicationSetting.PriceVolumeThreshold = applicationSetting.PriceVolumeThreshold;
                oldapplicationSetting.VesselArrivalLeadTime = applicationSetting.VesselArrivalLeadTime;
                oldapplicationSetting.VesselDischargeLeadTime = applicationSetting.VesselDischargeLeadTime;
                oldapplicationSetting.DischargeQuantityTolerance = oldapplicationSetting.DischargeQuantityTolerance;
                oldapplicationSetting.PermitExpiryTolerance = oldapplicationSetting.PermitExpiryTolerance;
                oldapplicationSetting.PermitValidity = oldapplicationSetting.PermitValidity;

                var docStatus = new ImportSettingServices().UpdateImportSetting(oldapplicationSetting);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Import Setting  could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldapplicationSetting.Id;
                gVal.Error = "Import Setting was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Import Setting could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
       
        public ActionResult GetApplicationSetting()
        {
            try
            {
                var applicationSetting = new ImportSettingServices().GetImportSetting();
                if (applicationSetting == null || applicationSetting.Id < 1)
                {
                    return Json(new ImportSettingObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_applicationSetting"] = applicationSetting;

                return Json(applicationSetting, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new ImportSettingObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteApplicationSetting(long id)
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
                var delStatus = new ImportSettingServices().DeleteImportSetting(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Import Setting not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Import Setting was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator ValidateApplicationSetting(ImportSettingObject applicationSetting)
        {
            var gVal = new GenericValidator();
            try
            {
                if (applicationSetting.ApplicationExpiry < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Application Expiry in days.";
                    return gVal;
                }

                if (applicationSetting.ApplicationLifeCycle < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Application Life Cycle in days.";
                    return gVal;
                }

                if (applicationSetting.PriceVolumeThreshold < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Price-Volume Threshold.";
                    return gVal;
                }

                if (applicationSetting.PermitValidity < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Permit Validity.";
                    return gVal;
                }


                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Import Setting  Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }


    }
}
