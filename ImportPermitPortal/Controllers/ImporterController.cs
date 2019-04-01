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
    public class ImporterController : Controller    
    {

        [HttpGet]
        [Authorize(Roles = "Super_Admin,Support")]
        public ActionResult GetImporterObjects(JQueryDataTableParamModel param) 
        {
            try
            {
                IEnumerable<ImporterObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetImporters(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ImporterServices().Search(param.sSearch);
                    countG = filteredParentMenuObjects.Count();
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ImporterObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ImporterObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name : sortColumnIndex == 2 ? c.TIN : sortColumnIndex == 3 ? c.RCNumber : sortColumnIndex == 4? c.DateAdded : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.Name, c.TIN, c.RCNumber, c.DateAdded, c.StatusStr };
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
                return Json(new List<ImporterObject>(), JsonRequestBehavior.AllowGet);
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

      public ActionResult GetImporter(long id)
        {
            try
            {
                var importer = new ImporterServices().GetImporter(id);
                if (importer == null || importer.Id < 1)
                {
                    return Json(new ImporterObject(), JsonRequestBehavior.AllowGet);
                }

                return Json(importer, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new ImporterObject(), JsonRequestBehavior.AllowGet);
            }
        }

       private List<ImporterObject> GetImporters(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new ImporterServices().GetCompanies(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<ImporterObject>();
            }
        }

       public ActionResult SetImporterId(long id)
       {
           var gVal = new GenericValidator();
           if (id < 1)
           {
               gVal.Code = -1;
               gVal.Error = "Your session has timed out.";
               return Json(gVal, JsonRequestBehavior.AllowGet);
           }
           var importer = new ImporterServices().GetImporter(id);
           if (importer == null || importer.Id < 1)
           {
               gVal.Code = -1;
               gVal.Error = "An error was encountered.";
               return Json(gVal, JsonRequestBehavior.AllowGet);
           }

           Session["_importerId"] = id;
           gVal.Code = 5;
           gVal.Error = importer.Name;
           return Json(gVal, JsonRequestBehavior.AllowGet);
           
       }
      
    }
}
