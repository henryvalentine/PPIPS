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
    public class CalculatorController : Controller
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


        public CalculatorObject AddCalculator(CalculatorObject calculator)
        {
            try
            {


                var calc = new CalculatorServices().AddCalculator(calculator);


                Session["_depotTrunkedOut"] = calc;

                return new CalculatorObject();

            }
            catch (Exception)
            {
                return new CalculatorObject();
            }
        }

        //[HttpPost]
        //public ActionResult EditCalculator(CalculatorObject calculator)
        //{
        //    var gVal = new GenericValidator();

        //    try
        //    {

        //        var stat = ValidateCalculator(calculator);

        //        if (stat.Code < 1)
        //        {
        //            return Json(gVal, JsonRequestBehavior.AllowGet);
        //        }

        //        if (Session["_calculator"] == null)
        //        {
        //            gVal.Code = -1;
        //            gVal.Error = "Session has timed out.";
        //            return Json(gVal, JsonRequestBehavior.AllowGet);
        //        }

        //        var oldcalculator = Session["_calculator"] as CalculatorObject;

        //        if (oldcalculator == null)
        //        {
        //            gVal.Code = -1;
        //            gVal.Error = "Session has timed out.";
        //            return Json(gVal, JsonRequestBehavior.AllowGet);
        //        }

        //        oldcalculator.Name = calculator.Name;
        //        oldcalculator.JettyId = calculator.JettyId;

        //        var docStatus = new CalculatorServices().UpdateCalculator(oldcalculator);
        //        if (docStatus < 1)
        //        {
        //            gVal.Code = -1;
        //            gVal.Error = docStatus == -3 ? "Calculator already exists." : "Calculator information could not be updated. Please try again later";
        //            return Json(gVal, JsonRequestBehavior.AllowGet);
        //        }

        //        gVal.Code = oldcalculator.Id;
        //        gVal.Error = "Calculator information was successfully updated";
        //        return Json(gVal, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception)
        //    {
        //        gVal.Code = -1;
        //        gVal.Error = "Calculator information could not be updated. Please try again later";
        //        return Json(gVal, JsonRequestBehavior.AllowGet);
        //    }
        //}


        //public ActionResult GetJetties()
        //{
        //    try
        //    {
        //        return Json(getJettyObjects(), JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new List<JettyObject>(), JsonRequestBehavior.AllowGet);
        //    }
        //}

        //private List<JettyObject> getJettyObjects()
        //{

        //    try
        //    {
        //        return new JettyServices().GetJetties();

        //    }
        //    catch (Exception)
        //    {
        //        return new List<JettyObject>();
        //    }
        //}
        //public ActionResult GetCalculator(int id)
        //{
        //    try
        //    {
        //        var calculator = new CalculatorServices().GetCalculatorAdmin(id);
        //        if (calculator == null || calculator.Id < 1)
        //        {
        //            return Json(new UserProfileObject(), JsonRequestBehavior.AllowGet);
        //        }

        //        Session["_usProfObj"] = calculator;
        //        return Json(calculator, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception)
        //    {
        //        return Json(new UserProfileObject(), JsonRequestBehavior.AllowGet);
        //    }
        //}


        //[HttpPost]
        //public ActionResult DeleteCalculator(long id)
        //{
        //    var gVal = new GenericValidator();
        //    try
        //    {
        //        if (id < 1)
        //        {
        //            gVal.Code = -1;
        //            gVal.Error = "Invalid selection";
        //            return Json(gVal, JsonRequestBehavior.AllowGet);
        //        }
        //        var delStatus = new CalculatorServices().DeleteCalculator(id);
        //        if (delStatus < 1)
        //        {
        //            gVal.Code = -1;
        //            gVal.Error = "Calculator could not be deleted. Please try again later.";
        //            return Json(gVal, JsonRequestBehavior.AllowGet);
        //        }

        //        gVal.Code = 5;
        //        gVal.Error = "Calculator Information was successfully deleted";
        //        return Json(gVal, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception)
        //    {
        //        return Json(gVal, JsonRequestBehavior.AllowGet);
        //    }
        //}


        //private GenericValidator ValidateCalculator(CalculatorObject calculator)
        //{
        //    var gVal = new GenericValidator();
        //    try
        //    {
        //        if (calculator.JettyId < 1)
        //        {
        //            gVal.Code = -1;
        //            gVal.Error = "Please select a Calculator.";
        //            return gVal;
        //        }



        //        gVal.Code = 5;
        //        return gVal;

        //    }
        //    catch (Exception)
        //    {

        //        gVal.Code = -1;
        //        gVal.Error = "Calculator Validation failed. Please provide all required fields and try again.";
        //        return gVal;
        //    }
        //}

        //private List<CalculatorObject> GetCalculators(int? itemsPerPage, int? pageNumber, out int countG)
        //{
        //    try
        //    {
        //        return new CalculatorServices().GetCalculators(itemsPerPage, pageNumber, out countG);
        //    }
        //    catch (Exception)
        //    {
        //        countG = 0;
        //        return new List<CalculatorObject>();
        //    }
        //}

        //private List<PortObject> GetPorts()
        //{
        //    try
        //    {
        //        return new PortServices().GetPorts();

        //    }
        //    catch (Exception)
        //    {
        //        return new List<PortObject>();
        //    }
        //}



    }
}
