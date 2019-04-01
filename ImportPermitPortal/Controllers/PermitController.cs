using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ImportPermitPortal.BizObjects;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.EF.Model;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace ImportPermitPortal.Controllers
{
    [Authorize]
    public class PermitController : Controller
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
        [Authorize(Roles = "Super_Admin,Support")]
        public ActionResult GetPermitObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<PermitObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetPermits(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new PermitServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<PermitObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<PermitObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.PermitStatus.ToString() : c.ExpiryDate.ToString());

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.PermitValue,c.CompanyName, c.PermitStatusStr, c.IssueDateStr, c.ExpiryDateStr };
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
                return Json(new List<PermitObject>(), JsonRequestBehavior.AllowGet);
            }
        }


          [HttpGet]
          [Authorize(Roles = "Applicant")]
        public ActionResult GetApplicantPermitObjects(JQueryDataTableParamModel param)
          {
            var gVal = new GenericValidator();
            try
            {
                IEnumerable<PermitObject> filteredParentMenuObjects;
                var countG = 0;
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var pagedParentMenuObjects = GetApplicantPermits(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new PermitServices().SearchApplicantPermits(param.sSearch, importerInfo.Id);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<PermitObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<PermitObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.PermitStatus.ToString() : c.ExpiryDate.ToString());

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.PermitValue,c.CompanyName, c.PermitStatusStr, c.IssueDateStr, c.ExpiryDateStr };
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
                return Json(new List<PermitObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Super_Admin")]
        public ActionResult AddPermit(PermitObject permit)
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

                var validationResult = ValidatePermit(permit);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
              
                var appPermitStatus = new PermitServices().AddPermit(permit);
                if (appPermitStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appPermitStatus == -2 ? "Permit upload failed. Please try again." : "The Permit Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appPermitStatus;
                gVal.Error = "Permit was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Permit processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Super_Admin")]
        public ActionResult EditPermit(PermitObject permit)
        {
            var gVal = new GenericValidator();

            try
            {
               

                var stat = ValidatePermit(permit);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_permit"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldpermit = Session["_permit"] as PermitObject;

                if (oldpermit == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldpermit.Id = permit.Id;
                
                var docPermitStatus = new PermitServices().UpdatePermit(oldpermit);
                if (docPermitStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docPermitStatus == -3 ? "Permit already exists." : "Permit information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldpermit.Id;
                gVal.Error = "Permit information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Permit information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetImporterPermits(JQueryDataTableParamModel param)
        {
            var gVal = new GenericValidator();
            try
            {
                IEnumerable<PermitObject> filteredParentMenuObjects;
                var countG = 0;
                var id = GetImporterId();
                if (id < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }


                var pagedParentMenuObjects = GetApplicantPermits(param.iDisplayLength, param.iDisplayStart, out countG, id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new PermitServices().SearchApplicantPermits(param.sSearch, id);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<PermitObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<PermitObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.PermitStatus.ToString() : c.ExpiryDate.ToString());

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.PermitValue, c.CompanyName, c.PermitStatusStr, c.IssueDateStr, c.ExpiryDateStr };
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
                return Json(new List<PermitObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPermit(long id)
        {
            try
            {


                var permit = new PermitServices().GetPermit(id);
                if (permit == null || permit.Id < 1)
                {
                    return Json(new PermitObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_permit"] = permit;

                return Json(permit, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new PermitObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPermits(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<PermitObject> filteredParentMenuObjects;
                var countG = 0;

                var id = GetImporterId();
                if (id < 1)
                {
                    return Json(new List<ApplicationObject>(), JsonRequestBehavior.AllowGet);
                }

                var pagedParentMenuObjects = GetApplicantPermits(param.iDisplayLength, param.iDisplayStart, out countG, id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new PermitServices().SearchApplicantPermits(param.sSearch, id);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<PermitObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<PermitObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.PermitStatus.ToString() : c.ExpiryDate.ToString());

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.PermitValue, c.CompanyName, c.PermitStatusStr, c.IssueDateStr, c.ExpiryDateStr };
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
                return Json(new List<PermitObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        public List<PermitObject> GetPermits()
        {
            try
            {


                var permit = new PermitServices().GetPermits();
             

                Session["_permit"] = permit;

                return new List<PermitObject>();

            }
            catch (Exception)
            {
                return new List<PermitObject>();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Super_Admin")]
        public ActionResult DeletePermit(long id)
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
                var delPermitStatus = new PermitServices().DeletePermit(id);
                if (delPermitStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Permit could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Permit Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult VerifyPermit(string permNo)
        {
            var rep = new Reporter();
            try
            {


                if (String.IsNullOrEmpty(permNo))
                {
                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }



                using (var db = new ImportPermitEntities())
                {

                    var dat = DateTime.Now;
                    var check = db.Permits.Where(c => c.PermitValue.Equals(permNo)).ToList();
                    if (check.Any())
                    {
                        var permdate = check[0].ExpiryDate;
                        if (dat > permdate)
                        {
                            rep.IsExpired = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            rep.IsValid = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }

                    }


                    rep.NoEmployee = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);


                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }

       private GenericValidator ValidatePermit(PermitObject permit)
        {
            var gVal = new GenericValidator();
            try
            {
                if (permit.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a Permit.";
                    return gVal;
                }

               

                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "Permit Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<PermitObject> GetPermits(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new PermitServices().GetPermits(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<PermitObject>();
            }
        }

        private List<PermitObject> GetApplicantPermits(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
                return new PermitServices().GetApplicantPermits(itemsPerPage, pageNumber, out countG, importerId);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<PermitObject>();
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

        public ActionResult GetPermitView(long id)
        {
            try
            {


                var permit = new PermitServices().GetPermit(id);
                if (permit == null || permit.Id < 1)
                {
                    return Json(new PermitObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_permit"] = permit;

                return Json(permit, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new PermitObject(), JsonRequestBehavior.AllowGet);
            }
        }

         public ActionResult GetPermitInfo(long id)
        {
            try
            {
                var permitInfo = new PermitServices().GetPermitInfo(id);
                if (permitInfo == null || permitInfo.PermitId < 1)
                {
                    return Json(new ApplicationObject(), JsonRequestBehavior.AllowGet);
                }

                return Json(permitInfo, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new PermitObject(), JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult GetPdfNow1(int id)
        //{
        //    try
        //    {
                
        //         ImportPermitEntities db = new ImportPermitEntities();

        //        Permit perm = db.Permits.Find(id);

        //        var log = new LogicObject();
        //        IQueryable<DatabaseObject> items = from t in db.ApplicationItems
        //                                           where t.Id == id
        //                                           orderby t.EstimatedQuantity descending

        //                                           select new DatabaseObject
        //                                           {
        //                                               ApplicationItemId = t.Id,
        //                                               ApplicationItemProductName = t.Product.Name,
        //                                               ApplicationItemEstimatedQuantity = t.EstimatedQuantity,
        //                                               ApplicationItemEstimatedValue = t.EstimatedValue,
        //                                               ApplicationItemPortOfOrigin = t.CountryOfOriginName,
        //                                               ApplicationItemPortOfDischarge = t.DischargeDepotName


        //                                           };

        //        PdfDocument pdf = new PdfDocument();

        //        //Next step is to create a an Empty page.

        //        PdfPage pp = pdf.AddPage();



        //        string path = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "permitup.pdf");

        //        //Then create an XGraphics Object

        //        XGraphics gfx = XGraphics.FromPdfPage(pp);

        //        XImage image = XImage.FromFile(path);
        //        gfx.DrawImage(image, 0, 0);




        //        XFont font = new XFont("Calibri", 12, XFontStyle.Regular);
        //        XFont font2 = new XFont("Calibri", 10, XFontStyle.Regular);


        //        gfx.DrawString(perm.PermitValue, font, XBrushes.Black, new XRect(392, 70, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //        gfx.DrawString(DateTime.Now.ToString("dd/MM/yy"), font, XBrushes.Black, new XRect(390, 95, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //        gfx.DrawString("", font, XBrushes.Black, new XRect(70, 125, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //        gfx.DrawString("", font, XBrushes.Black, new XRect(70, 150, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //        gfx.DrawString("", font, XBrushes.Black, new XRect(60, 175, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //        var permitApps = (from ptApp in db.PermitApplications.Where(j => j.PermitId == perm.Id)
        //                             join app in db.Applications on ptApp.ApplicationId equals app.Id
        //                             join imp in db.Importers on app.ImporterId equals imp.Id select new {app, imp}).ToList();

        //        if (!permitApps.Any())
        //        {
        //            return Json("Error", JsonRequestBehavior.AllowGet);
        //        }
        //        var pApp = permitApps[0];

        //        gfx.DrawString(pApp.imp.Name, font, XBrushes.Black, new XRect(80, 218, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //        gfx.DrawString("", font, XBrushes.Black, new XRect(92, 240, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



        //        MigraDoc.DocumentObjectModel.Document doc = new MigraDoc.DocumentObjectModel.Document();

        //        Section section = doc.AddSection();

        //        var table = section.AddTable();



        //        //table = section.AddTable();
        //        table.Style = "Table";

        //        table.Borders.Width = 0.25;
        //        table.Borders.Left.Width = 0.5;
        //        table.Borders.Right.Width = 0.5;
        //        table.Rows.LeftIndent = 0;

        //        Column column = table.AddColumn("4cm");
        //        column.Format.Alignment = ParagraphAlignment.Center;


        //        column = table.AddColumn("4cm");
        //        column.Format.Alignment = ParagraphAlignment.Center;

        //        column = table.AddColumn("4cm");
        //        column.Format.Alignment = ParagraphAlignment.Center;

        //        column = table.AddColumn("4cm");
        //        column.Format.Alignment = ParagraphAlignment.Center;



        //        // Create the header of the table
        //        Row row = table.AddRow();
        //        //row = table.AddRow();
        //        row.HeadingFormat = true;
        //        row.Format.Alignment = ParagraphAlignment.Center;
        //        row.Format.Font.Bold = true;


        //        row.Cells[0].AddParagraph("Type of Petroleum Product:");
        //        row.Cells[0].Format.Alignment = ParagraphAlignment.Left;


        //        row.Cells[1].AddParagraph("Country of origin:");
        //        row.Cells[1].Format.Alignment = ParagraphAlignment.Left;

        //        row.Cells[2].AddParagraph("Quantity/Weight (Metric Tones):");
        //        row.Cells[2].Format.Alignment = ParagraphAlignment.Left;

        //        row.Cells[3].AddParagraph("Estimated Value($):");
        //        row.Cells[3].Format.Alignment = ParagraphAlignment.Left;

        //        double total = 0;

        //        if (items.Any() && items.Count() <= 7)
        //        {

        //            foreach (var item in items.ToList())
        //            {
        //                row = table.AddRow();
        //                row.Cells[0].AddParagraph(item.ApplicationItemProductName);
        //                row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

        //                total = total + item.ApplicationItemEstimatedValue;

        //                row.Cells[1].AddParagraph(item.ApplicationItemPortOfOrigin);
        //                row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
        //                row.Cells[2].AddParagraph(item.ApplicationItemEstimatedQuantity.ToString());
        //                row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
        //                row.Cells[3].AddParagraph(item.ApplicationItemEstimatedValue.ToString());
        //                row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
        //            }

        //        }
        //        else if (items.Any() && items.Count() > 7)
        //        {


        //            foreach (var item in items.ToList())
        //            {
        //                row = table.AddRow();
        //                row.Format.Font.Size = 8;
        //                row.Cells[0].AddParagraph(item.ApplicationItemProductName);
        //                row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

        //                total = total + item.ApplicationItemEstimatedValue;

        //                row.Cells[1].AddParagraph(item.ApplicationItemPortOfOrigin);
        //                row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
        //                row.Cells[2].AddParagraph(item.ApplicationItemEstimatedQuantity.ToString());
        //                row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
        //                row.Cells[3].AddParagraph(item.ApplicationItemEstimatedValue.ToString());
        //                row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
        //            }


        //        }

        //        //convert total amount to words



        //        var amtWords = log.ChangeToWords(total.ToString(), true);


        //        const bool unicode = false;
        //        const PdfFontEmbedding embedding = PdfFontEmbedding.Always;

        //        PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);

        //        // Associate the MigraDoc document with a renderer

        //        pdfRenderer.Document = doc;



        //        // Layout and render document to PDF

        //        pdfRenderer.RenderDocument();




        //        var pathtable = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "table.pdf");

        //        pdfRenderer.PdfDocument.Save(pathtable);

        //        XImage imagetable = XImage.FromFile(pathtable);
        //        gfx.DrawImage(imagetable, 0, 280);


        //        //rigid style
        //        XImage image2 = XImage.FromFile(System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "permitdown.pdf"));
        //        gfx.DrawImage(image2, 0, 550);


        //        gfx.DrawString(amtWords, font2, XBrushes.Black, new XRect(135, 556, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //        gfx.DrawString("", font, XBrushes.Black, new XRect(238, 593, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //        gfx.DrawString("", font, XBrushes.Black, new XRect(220, 633, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //        gfx.DrawString(perm.IssueDate.Value.ToString("dd/MM/yy"), font, XBrushes.Black, new XRect(100, 673, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //        gfx.DrawString(perm.ExpiryDate.Value.ToString("dd/MM/yy"), font, XBrushes.Black, new XRect(370, 673, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


        //        //string path2 = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "permit.pdf");

        //        //using (MemoryStream ms = new MemoryStream())
        //        //{
        //        //    pdf.Save(ms, false);
        //        //    byte[] buffer = new byte[ms.Length];
        //        //    ms.Seek(0, SeekOrigin.Begin);
        //        //    ms.Flush();
        //        //    ms.Read(buffer, 0, (int)ms.Length);

        //        //     return Json(ms, JsonRequestBehavior.AllowGet);
        //        //}
        //        //return new FileStreamResult(Response.OutputStream, "application/pdf"){FileDownloadName = "download.pdf"};
  
               
        //        using (MemoryStream stream = new MemoryStream())
        //        {
        //            byte[] fileContents = null;
        //            pdf.Save(stream, true);
        //            fileContents = stream.ToArray();

        //            var jsonResult = Json(fileContents, JsonRequestBehavior.AllowGet);
        //            jsonResult.MaxJsonLength = int.MaxValue;
        //            return jsonResult;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(ex.InnerException.Message, JsonRequestBehavior.AllowGet);
        //    }
        //}

        private long GetImporterId()
        {
            try
            {
                if (Session["_importerInfo"] == null)
                {
                    return 0;
                }

                var importerId = (long)Session["_importerId"];
                if (importerId < 1)
                {
                    return 0;
                }

                return importerId;

            }
            catch (Exception)
            {
                return 0;
            }
        }
        public string GetPdfNow(long id)
        {
            try
            {


                using (var db = new ImportPermitEntities())
                {

                    Permit perm = db.Permits.Find(id);






                    //return Json(path, JsonRequestBehavior.AllowGet);


                    return perm.file;
                }


            }
            catch (Exception ex)
            {
                //return Json(ex.Message, JsonRequestBehavior.AllowGet);
                return ex.Message;

            }
            

        }
        


    }
}
