using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ImportPermitPortal.BizObjects;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.EF.Model;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;

namespace ImportPermitPortal.Controllers
{
    
    public class VerifyPermitController : Controller
    {
        [HttpGet]
        public ActionResult VerifyPermit(string permNo)
        {
            var perm = new PermitObject();
            try
            {


                if (String.IsNullOrEmpty(permNo))
                {
                    perm.IsNull = true;
                    return Json(perm, JsonRequestBehavior.AllowGet);
                }



                using (var db = new ImportPermitEntities())
                {

                    var dat = DateTime.Now;
                    var check = db.Permits.Where(c => c.PermitValue.Equals(permNo)).ToList();
                    if (check.Any())
                    {
                        var permdate = check[0].ExpiryDate;
                                               
                        var permId = check[0].Id;
                        var permApp = db.PermitApplications.Where(p => p.PermitId == permId).ToList();
                        if (permApp.Any())
                        {
                            var appId = permApp[0].ApplicationId;
                            var app = db.Applications.Where(a => a.Id == appId).ToList();
                            if (app.Any())
                            {
                                perm.CompanyName = app[0].Importer.Name;
                                var permit = db.Permits.Where(m => m.Id == permId).ToList();

                                if (permit.Any())
                                {
                                    perm.IssueDateStr = permit[0].IssueDate.ToString();
                                    perm.ExpiryDateStr = permit[0].ExpiryDate.ToString();
                                    perm.QuantityStr = permit[0].QuantityImported.ToString();
                                    if (dat > permdate)
                                    {
                                        perm.IsExpired = true;
                                        return Json(perm, JsonRequestBehavior.AllowGet);
                                    }
                                    perm.IsValid = true;
                                    return Json(perm, JsonRequestBehavior.AllowGet);
                                }

                            }
                            perm.NoEmployee = true;
                        }

                        perm.NoEmployee = true;

                    }


                    perm.NoEmployee = true;
                    return Json(perm, JsonRequestBehavior.AllowGet);


                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                perm.IsError = true;
                return Json(perm, JsonRequestBehavior.AllowGet);
            }
        }

    
    

    }
}
