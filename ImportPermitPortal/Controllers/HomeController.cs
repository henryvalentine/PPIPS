using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImportPermitPortal.Services.Services;

namespace ImportPermitPortal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Disclaimer()
        {
            return View();
        }

        public ActionResult GeneralInformation()
        {
            return View();
        }

        public ActionResult Faq()
        {
            var faqs = new FaqServices().GetFaqs();
            return View(faqs);
        }

        public ActionResult PrivacyPolicy()
        {
            
            return View();
        }

        public ActionResult HowItWorks()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}