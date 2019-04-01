using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ImportPermitPortal.BizObjects;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.EF.Model;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Models;
using ImportPermitPortal.Services.Services;
using Microsoft.AspNet.Identity;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

using Application = ImportPermitPortal.EF.Model.Application;

namespace ImportPermitPortal.Controllers
{
    //[Authorize(Roles = "Employee")]
    public class EmployeeHistoryController : Controller
    {
       

         [HttpGet]
        public ActionResult GetPreviousApplicationJobs(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<ProcessingHistoryObject> filteredParentMenuObjects;
                var countG = 0;

                //get the id of logged in user
                var userId = User.Identity.GetUserId();


                var pagedParentMenuObjects = GetPreviousJobsProfiles(param.iDisplayLength, param.iDisplayStart, out countG,
                    userId);


                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new ProcessingHistoryServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<ProcessingHistoryObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ProcessingHistoryObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : c.ActualDeliveryDateTimeStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), 
                                 c.ReferenceCode, c.CompanyName, c.AssignedTimeStr,
                                 c.ActualDeliveryDateTimeStr,c.Remarks};
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
                return Json(new List<ProcessingHistoryObject>(), JsonRequestBehavior.AllowGet);
            }
        }


         [HttpGet]
         public ActionResult GetPreviousNotificationJobs(JQueryDataTableParamModel param)
         {
             try
             {
                 IEnumerable<NotificationHistoryObject> filteredParentMenuObjects;
                 var countG = 0;

                 //get the id of logged in user
                 var userId = User.Identity.GetUserId();


                 var pagedParentMenuObjects = GetPreviousNotificationJobsProfiles(param.iDisplayLength, param.iDisplayStart, out countG,
                     userId);


                 if (!string.IsNullOrEmpty(param.sSearch))
                 {
                     filteredParentMenuObjects = new NotificationHistoryServices().Search(param.sSearch);
                 }
                 else
                 {
                     filteredParentMenuObjects = pagedParentMenuObjects;
                 }

                 if (!filteredParentMenuObjects.Any())
                 {
                     return Json(new List<NotificationHistoryObject>(), JsonRequestBehavior.AllowGet);
                 }

                 var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                 Func<NotificationHistoryObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : c.AssignedTimeStr);

                 var sortDirection = Request["sSortDir_0"]; // asc or desc
                 filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                 var displayedPersonnels = filteredParentMenuObjects;

                 var result = from c in displayedPersonnels
                              select new[] { Convert.ToString(c.Id), 
                                 c.ReferenceCode, c.CompanyName, c.AssignedTimeStr,
                                 c.ActualDeliveryDateTimeStr,c.Remarks};
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
                 return Json(new List<NotificationHistoryObject>(), JsonRequestBehavior.AllowGet);
             }
         }


         [HttpGet]
         public ActionResult GetPreviousRecertificationJobs(JQueryDataTableParamModel param)
         {
             try
             {
                 IEnumerable<RecertificationHistoryObject> filteredParentMenuObjects;
                 var countG = 0;

                 //get the id of logged in user
                 var userId = User.Identity.GetUserId();


                 var pagedParentMenuObjects = GetPreviousRecertificationJobsProfiles(param.iDisplayLength, param.iDisplayStart, out countG,
                     userId);


                 if (!string.IsNullOrEmpty(param.sSearch))
                 {
                     filteredParentMenuObjects = new RecertificationHistoryServices().Search(param.sSearch);
                 }
                 else
                 {
                     filteredParentMenuObjects = pagedParentMenuObjects;
                 }

                 if (!filteredParentMenuObjects.Any())
                 {
                     return Json(new List<RecertificationHistoryObject>(), JsonRequestBehavior.AllowGet);
                 }

                 var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                 Func<RecertificationHistoryObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : c.AssignedTimeStr);

                 var sortDirection = Request["sSortDir_0"]; // asc or desc
                 filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                 var displayedPersonnels = filteredParentMenuObjects;

                 var result = from c in displayedPersonnels
                              select new[] { Convert.ToString(c.Id), 
                                 c.ReferenceCode, c.CompanyName, c.AssignedTimeStr,
                                 c.ActualDeliveryDateTimeStr,c.Remarks};
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
                 return Json(new List<RecertificationHistoryObject>(), JsonRequestBehavior.AllowGet);
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


        public bool GetPdfNow(int id, int radno, string thenum, string company, DateTime combined)
        {
            try
            {

                using (var db = new ImportPermitEntities())
                {

                    var log = new LogicObject();

                    var appItems = (from t in db.ApplicationItems.Include("Application")
                                    where t.ApplicationId == id
                                    orderby t.EstimatedValue descending

                                    select t).ToList();

                    if (!appItems.Any())
                    {
                        return false;
                    }
                    var items = new List<DatabaseObject>();
                    appItems.ForEach(t =>
                    {
                        var im = new DatabaseObject()
                        {
                            Id = t.Id,
                            ApplicationItemId = t.Id,
                            ApplicationItemProductName = t.Product.Name,
                            ApplicationItemEstimatedQuantity = t.EstimatedQuantity,
                            ApplicationItemEstimatedValue = t.EstimatedValue,
                        };
                        var appCountries = db.ApplicationCountries.Where(a => a.ApplicationItemId == im.Id).Include("Country").ToList();
                        var depotList = db.ThroughPuts.Where(a => a.ApplicationItemId == im.Id).Include("Depot").ToList();
                        if (appCountries.Any() && depotList.Any())
                        {
                            im.ApplicationItemPortOfOrigin = "";
                            appCountries.ForEach(c =>
                            {
                                if (string.IsNullOrEmpty(im.ApplicationItemPortOfOrigin))
                                {
                                    im.ApplicationItemPortOfOrigin = c.Country.Name;
                                }
                                else
                                {
                                    im.ApplicationItemPortOfOrigin += ", " + c.Country.Name;
                                }
                            });

                            im.ApplicationItemPortOfDischarge = "";
                            depotList.ForEach(d =>
                            {
                                if (string.IsNullOrEmpty(im.ApplicationItemPortOfDischarge))
                                {
                                    im.ApplicationItemPortOfDischarge = d.Depot.Name;
                                }
                                else
                                {
                                    im.ApplicationItemPortOfDischarge += ", " + d.Depot.Name;
                                }
                            });

                            items.Add(im);
                        }
                    });


                    PdfDocument pdf = new PdfDocument();

                    //Next step is to create a an Empty page.

                    PdfPage pp = pdf.AddPage();



                    string path = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "permitup.pdf");

                    //Then create an XGraphics Object

                    XGraphics gfx = XGraphics.FromPdfPage(pp);

                    XImage image = XImage.FromFile(path);
                    gfx.DrawImage(image, 0, 0);




                    XFont font = new XFont("Calibri", 12, XFontStyle.Regular);
                    XFont font2 = new XFont("Calibri", 10, XFontStyle.Regular);


                    gfx.DrawString(thenum, font, XBrushes.Black, new XRect(392, 70, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);

                    gfx.DrawString(DateTime.Now.ToString("dd/MM/yy"), font, XBrushes.Black,
                        new XRect(390, 95, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    gfx.DrawString("", font, XBrushes.Black, new XRect(70, 125, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);

                    gfx.DrawString("", font, XBrushes.Black, new XRect(70, 150, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);

                    gfx.DrawString("", font, XBrushes.Black, new XRect(60, 175, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);


                    gfx.DrawString(company, font, XBrushes.Black, new XRect(80, 218, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);

                    gfx.DrawString("", font, XBrushes.Black, new XRect(92, 240, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);



                    MigraDoc.DocumentObjectModel.Document doc = new MigraDoc.DocumentObjectModel.Document();

                    Section section = doc.AddSection();

                    var table = section.AddTable();



                    //table = section.AddTable();
                    table.Style = "Table";

                    table.Borders.Width = 0.25;
                    table.Borders.Left.Width = 0.5;
                    table.Borders.Right.Width = 0.5;
                    table.Rows.LeftIndent = 0;

                    Column column = table.AddColumn("4cm");
                    column.Format.Alignment = ParagraphAlignment.Center;


                    column = table.AddColumn("4cm");
                    column.Format.Alignment = ParagraphAlignment.Center;

                    column = table.AddColumn("4cm");
                    column.Format.Alignment = ParagraphAlignment.Center;

                    column = table.AddColumn("4cm");
                    column.Format.Alignment = ParagraphAlignment.Center;



                    // Create the header of the table
                    Row row = table.AddRow();
                    //row = table.AddRow();
                    row.HeadingFormat = true;
                    row.Format.Alignment = ParagraphAlignment.Center;
                    row.Format.Font.Bold = true;


                    row.Cells[0].AddParagraph("Type of Petroleum Product:");
                    row.Cells[0].Format.Alignment = ParagraphAlignment.Left;


                    row.Cells[1].AddParagraph("Country of origin:");
                    row.Cells[1].Format.Alignment = ParagraphAlignment.Left;

                    row.Cells[2].AddParagraph("Quantity/Weight (Metric Tones):");
                    row.Cells[2].Format.Alignment = ParagraphAlignment.Left;

                    row.Cells[3].AddParagraph("Estimated Value($):");
                    row.Cells[3].Format.Alignment = ParagraphAlignment.Left;

                    double total = 0;

                    if (items.Any() && items.Count() <= 7)
                    {

                        foreach (var item in items.ToList())
                        {
                            row = table.AddRow();
                            row.Cells[0].AddParagraph(item.ApplicationItemProductName);
                            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

                            total = total + item.ApplicationItemEstimatedValue;

                            row.Cells[1].AddParagraph(item.ApplicationItemPortOfOrigin);
                            row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                            row.Cells[2].AddParagraph(item.ApplicationItemEstimatedQuantity.ToString());
                            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
                            row.Cells[3].AddParagraph(item.ApplicationItemEstimatedValue.ToString());
                            row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
                        }

                    }
                    else if (items.Any() && items.Count() > 7)
                    {


                        foreach (var item in items.ToList())
                        {
                            row = table.AddRow();
                            row.Format.Font.Size = 8;
                            row.Cells[0].AddParagraph(item.ApplicationItemProductName);
                            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

                            total = total + item.ApplicationItemEstimatedValue;

                            row.Cells[1].AddParagraph(item.ApplicationItemPortOfOrigin);
                            row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                            row.Cells[2].AddParagraph(item.ApplicationItemEstimatedQuantity.ToString());
                            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
                            row.Cells[3].AddParagraph(item.ApplicationItemEstimatedValue.ToString());
                            row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
                        }


                    }

                    //convert total amount to words



                    var amtWords = log.ChangeToWords(total.ToString(), true);


                    const bool unicode = false;
                    const PdfFontEmbedding embedding = PdfFontEmbedding.Always;

                    PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);

                    // Associate the MigraDoc document with a renderer

                    pdfRenderer.Document = doc;



                    // Layout and render document to PDF

                    pdfRenderer.RenderDocument();




                    var pathtable = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "table.pdf");

                    pdfRenderer.PdfDocument.Save(pathtable);

                    XImage imagetable = XImage.FromFile(pathtable);
                    gfx.DrawImage(imagetable, 0, 280);


                    //rigid style
                    XImage image2 = XImage.FromFile(System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "permitdown.pdf"));
                    gfx.DrawImage(image2, 0, 550);


                    gfx.DrawString(amtWords, font2, XBrushes.Black, new XRect(135, 556, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);

                    gfx.DrawString("", font, XBrushes.Black, new XRect(238, 593, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);

                    gfx.DrawString("", font, XBrushes.Black, new XRect(220, 633, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);

                    gfx.DrawString(DateTime.Now.ToString("dd/MM/yy"), font, XBrushes.Black,
                        new XRect(100, 673, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    gfx.DrawString(combined.ToString("dd/MM/yy"), font, XBrushes.Black,
                        new XRect(370, 673, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);





                    string path2 = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "permit" + radno + ".pdf");

                    

                    //string path2 = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "permit.pdf");

                    pdf.Save(path2);


                    return true;

                }
            }

            catch (Exception ex)
            {
                return false;
            }

        }

        //public ActionResult PreviousJobs(long id)
        //{
        //    try
        //    {
        //        //get the id of logged in user
        //        var userId = User.Identity.GetUserId();

        //        if (id < 1)
        //        {
        //            return Json(new ProcessingHistory(), JsonRequestBehavior.AllowGet);
        //        }

        //        return Json(PreviousTasks(id,userId), JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new ViewApplication(), JsonRequestBehavior.AllowGet);
        //    }
        //}

        private ApplicationObject GetPreviousApplicationTasks(long id, string userId)
        {
            return new EmployeeProfileServices().GetPreviousApplicationTasks(id, userId);
        }
 

        public ActionResult GetApplication(long id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new Application(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetApplicationInfo(id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ViewApplication(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetNotification(long id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new Notification(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetNotificationInfo(id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ViewApplication(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetRecertification(long id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new Recertification(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetRecertificationInfo(id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new ViewApplication(), JsonRequestBehavior.AllowGet);
            }
        }
        private NotificationObject GetNotificationInfo(long trackId)
        {
            return new EmployeeHistoryServices().GetNotification(trackId);
        }

        private ApplicationObject GetApplicationInfo(long trackId)
        {
            return new EmployeeHistoryServices().GetApplication(trackId);
        }

        private RecertificationObject GetRecertificationInfo(long trackId)
        {
            return new EmployeeHistoryServices().GetRecertification(trackId);
        }

        [HttpPost]
        public ActionResult EditProcessTracking(ProcessTrackingObject processTracking)
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

                if (string.IsNullOrEmpty(processTracking.ReferenceCode.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide ProcessTracking.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_ProcessTracking"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldProcessTracking = Session["_ProcessTracking"] as ProcessTrackingObject;

                if (oldProcessTracking == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                oldProcessTracking.ReferenceCode = processTracking.ReferenceCode.Trim();

                var docStatus = new ProcessTrackingServices().UpdateProcessTracking(oldProcessTracking);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "ProcessTracking information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldProcessTracking.Id;
                gVal.Error = "ProcessTracking information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "ProcessTracking information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ProcessTrackingObject> GetEmployeeProfiles(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {

                return new EmployeeProfileServices().GetEmployeeProfiles(itemsPerPage, pageNumber, out countG, userId);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<ProcessTrackingObject>();
            }
        }


        private List<ProcessingHistoryObject> GetPreviousJobsProfiles(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {

                return new EmployeeHistoryServices().GetPreviousJobsHistorys(itemsPerPage, pageNumber, out countG, userId);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<ProcessingHistoryObject>();
            }
        }

        private List<NotificationHistoryObject> GetPreviousNotificationJobsProfiles(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {

                return new EmployeeHistoryServices().GetPreviousNotificationJobsHistorys(itemsPerPage, pageNumber, out countG, userId);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<NotificationHistoryObject>();
            }
        }

        private List<RecertificationHistoryObject> GetPreviousRecertificationJobsProfiles(int? itemsPerPage, int? pageNumber, out int countG, string userId)
        {
            try
            {

                return new EmployeeHistoryServices().GetPreviousRecertificationJobsHistorys(itemsPerPage, pageNumber, out countG, userId);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<RecertificationHistoryObject>();
            }
        }

        public ActionResult GetProcessTracking(long id)
        {
            try
            {


                var processTracking = new ProcessTrackingServices().GetProcessTracking(id);
                if (processTracking == null || processTracking.Id < 1)
                {
                    return Json(new ProcessTrackingObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_ProcessTracking"] = processTracking;

                return Json(processTracking, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new ProcessTrackingObject(), JsonRequestBehavior.AllowGet);
            }
        }

       
      
       

   














    }
}
