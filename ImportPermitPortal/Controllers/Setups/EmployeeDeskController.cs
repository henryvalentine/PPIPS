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
using ImportPermitPortal.Services.Services;
using Microsoft.AspNet.Identity;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace ImportPermitPortal.Controllers
{
    [Authorize(Roles = "Super_Admin, ICT")]
    public class EmployeeDeskController : Controller
    {
        [HttpGet]
        public ActionResult GetEmployeeDeskObjects(JQueryDataTableParamModel param)
        {
            try
            {  
                IEnumerable<EmployeeDeskObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetEmployeeDesks(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new EmployeeDeskServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<EmployeeDeskObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<EmployeeDeskObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.EmployeeName : c.GroupName);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.EmployeeName, c.GroupName, c.ActivityTypeName, c.ZoneName,c.JobCountStr };
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
                return Json(new List<EmployeeDeskObject>(), JsonRequestBehavior.AllowGet);
            }
        }


      

        //[HttpPost]
        //public ActionResult AddEmployeeDesk(EmployeeDeskObject employeeDesk)
        //{
        //    var gVal = new GenericValidator();

        //    try
        //    {
        //        //if (!ModelState.IsValid)
        //        //{
        //        //    gVal.Code = -1;
        //        //    gVal.Error = "Plese provide all required fields and try again.";
        //        //    return Json(gVal, JsonRequestBehavior.AllowGet);
        //        //}

        //        var companyInfo = GetLoggedOnUserInfo();
        //        if (companyInfo.CompanyId < 1)
        //        {
        //            gVal.Error = "Your session has timed out";
        //            gVal.Code = -1;
        //            return Json(gVal, JsonRequestBehavior.AllowGet);
        //        }

        //        var validationResult = ValidateEmployeeDesk(employeeDesk);

        //        if (validationResult.Code == 1)
        //        {
        //            return Json(validationResult, JsonRequestBehavior.AllowGet);
        //        }

        //        var appStatus = new EmployeeDeskServices().AddEmployeeDesk(employeeDesk);
        //        if (appStatus < 1)
        //        {
        //            validationResult.Code = -1;
        //            validationResult.Error = appStatus == -2 ? "EmployeeDesk upload failed. Please try again." : "The EmployeeDesk Information already exists";
        //            return Json(validationResult, JsonRequestBehavior.AllowGet);
        //        }

        //        gVal.Code = appStatus;
        //        gVal.Error = "EmployeeDesk was successfully added.";
        //        return Json(gVal, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception)
        //    {
        //        gVal.Error = "EmployeeDesk employeeDesking failed. Please try again later";
        //        gVal.Code = -1;
        //        return Json(gVal, JsonRequestBehavior.AllowGet);
        //    }
        //}




        [HttpPost]
        public ActionResult EditEmployeeDesk(EmployeeDeskObject employeeDesk)
        {
            var gVal = new GenericValidator();

            try
            {
               

                if (string.IsNullOrEmpty(employeeDesk.GroupName.Trim()))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide EmployeeDesk.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_EmployeeDesk"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldEmployeeDesk = Session["_EmployeeDesk"] as EmployeeDeskObject;

                if (oldEmployeeDesk == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                oldEmployeeDesk.EmployeeName = employeeDesk.EmployeeName.Trim();

                var docStatus = new EmployeeDeskServices().UpdateEmployeeDesk(oldEmployeeDesk);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "EmployeeDesk information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldEmployeeDesk.Id;
                gVal.Error = "EmployeeDesk information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "EmployeeDesk information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private List<EmployeeDeskObject> GetEmployeeDesks(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {

                return new EmployeeDeskServices().GetEmployeeDesks(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception)
            {
                countG = 0;
                return new List<EmployeeDeskObject>();
            }
        }

        public ActionResult GetEmployeeDesk(long id)
        {
            try
            {


                var employeeDesk = new EmployeeDeskServices().GetEmployeeDesk(id);
                if (employeeDesk == null || employeeDesk.Id < 1)
                {
                    return Json(new EmployeeDeskObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_EmployeeDesk"] = employeeDesk;

                return Json(employeeDesk, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new EmployeeDeskObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteEmployeeDesk(long id)
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
                var delStatus = new EmployeeDeskServices().DeleteEmployeeDesk(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "EmployeeDesk could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "EmployeeDesk Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private GenericValidator ValidateEmployeeDesk(EmployeeDeskObject employeeDesk)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(employeeDesk.EmployeeName))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select provide EmployeeDesk.";
                    return gVal;
                }


                gVal.Code = 5;
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "EmployeeDesk Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericEmployeeDeskList()
                {

                    Groups = getGroups(),
                    Zones = getZones(),
                    ActivityTypes = getStepActivityTypes()

                };


                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }


        //public bool Accepted(int id)
        //{
        //    try
        //    {
        //        using (var db = new ImportPermitEntities())
        //        {
        //            var importapplication = db.Applications.Find(id);

        //            //update the application statuscode to enable us get the next step
        //            if (importapplication == null)
        //            {
        //                return false;
        //            }

        //            //get the id of logged in user
        //            var aspnetId = User.Identity.GetUserId();

        //            //get the id of the userprofile table
        //            var registeredGuys = db.AspNetUsers.Find(aspnetId);
        //            var profileId = registeredGuys.UserInfo_Id;

        //            //get the employee id on employee desk table
        //            var employeeDesk = db.EmployeeDesks.Where(e => e.EmployeeId.Equals(profileId)).ToList();



        //            long employeeId;

        //            if (employeeDesk.Any())
        //            {
        //                employeeId = employeeDesk[0].Id;

        //                //update the tracking stepcode to enable us get the next step
        //                var track =
        //                    db.ProcessTrackings.Where(p => p.ApplicationId.Equals(id) && p.EmployeeId.Equals(employeeId)).ToList();

        //                //get the sequence of the current step

        //                var stepnow = db.Steps.Find(track[0].StepId);

        //                var nextStepSequence = stepnow.SequenceNumber + 1;

        //                //find out if this is the last step
        //                var stepnext = db.Steps.Where(s => s.SequenceNumber.Equals(nextStepSequence)).ToList();



        //                if (stepnext.Any())
        //                {
        //                    //update the tracker actual delivery time
        //                    track[0].ActualDeliveryDateTime = DateTime.Now;


        //                    //if (stepnext[0].Process.Name != null)
        //                    //{
        //                    //    if (stepnow.Process.Name != stepnext[0].Process.Name)
        //                    //    {
        //                    //        track[0]. = EnumProcessStatus.Completed.ToString();
        //                    //    }
        //                    //    else
        //                    //    {
        //                    //        track.ProcessStatus = EnumProcessStatus.Active.ToString();
        //                    //    }
        //                    //}

        //                    track[0].StatusId = (int)EnumStepStatus.Active;

        //                    //stepcode of 0 means the application is assigned but not done with
        //                    //track.StepCode = nextstepcode;

        //                    db.Entry(track[0]).State = EntityState.Modified;
        //                    db.SaveChanges();

        //                    var fxg = (int)nextStepSequence;

        //                    //call the assign method
        //                    var res = new WorkFlowServices().AssignApplicationToEmployeeInTrack(id, fxg, track[0].Id);

        //                    if (res == true)
        //                    {
        //                        //add a history of work done 
        //                        var prohis = new ProcessingHistory();
        //                        prohis.ApplicationId = id;
        //                        prohis.AssignedTime = track[0].AssignedTime;
        //                        prohis.EmployeeId = employeeId;
        //                        prohis.FinishedTime = DateTime.Now;
        //                        prohis.StepId = track[0].StepId;
        //                        prohis.OutComeCode = (int)ProcessStatusEnum.Open;

        //                        db.ProcessingHistories.Add(prohis);
        //                        db.SaveChanges();


        //                        return true;

        //                    }
        //                }

        //                else
        //                {
        //                    //generate permit

        //                    track[0].ActualDeliveryDateTime = DateTime.Now;

        //                    track[0].StatusId = (int)EnumStepStatus.Completed;

        //                    //stepcode of 0 means the applicatication id finished
        //                    track[0].StepCode = 0;

        //                    db.Entry(track[0]).State = EntityState.Modified;

        //                    //add a history of work done 
        //                    var prohis = new ProcessingHistory();
        //                    prohis.ApplicationId = id;
        //                    prohis.AssignedTime = track[0].AssignedTime;
        //                    prohis.EmployeeId = employeeId;
        //                    prohis.FinishedTime = DateTime.Now;
        //                    prohis.StepId = track[0].StepId;
        //                    prohis.OutComeCode = (int)ProcessStatusEnum.Treated;

        //                    db.ProcessingHistories.Add(prohis);


        //                    Random rad = new Random();
        //                    var radno = rad.Next();

        //                    int permid;



        //                    var all = from l in db.Permits
        //                              orderby l.PermitNo descending
        //                              select new DatabaseObject
        //                              {
        //                                  Id = l.Id,
        //                                  PermitPermitNo = l.PermitNo
        //                              };


        //                    if (all.Any())
        //                    {
        //                        permid = all.First().PermitPermitNo + 1;
        //                    }
        //                    else
        //                    {
        //                        permid = 1;
        //                    }




        //                    var year = DateTime.Now.Year;

        //                    Permit perm = new Permit();
        //                    perm.ApplicationId = id;
        //                    perm.IssueDate = DateTime.Now;
        //                    perm.PermitNo = permid;
        //                    perm.PermitValue = "DPR/IMP/" + year + "/" + permid;



        //                    var company = importapplication.Importer.Name;
        //                    var thenum = "DPR/IMP/" + year + "/" + permid;

        //                    TimeSpan time = new TimeSpan(90, 0, 0, 0);
        //                    DateTime combined = DateTime.Now.Add(time);

        //                    perm.ExpiryDate = combined;

        //                    perm.PermitStatus = (int)EnumPermitStatus.Active;

        //                    perm.file = @"\PermDoc\\" + "permit" + radno + ".pdf";

        //                    db.Permits.Add(perm);
        //                    db.SaveChanges();

        //                    var pdfres = GetPdfNow(id, radno, thenum, company, combined);



        //                    if (pdfres)
        //                    {
        //                        return true;
        //                    }
                           
        //                        return false;

                            


        //                }
        //            }





        //            return false;

        //        }

        //    }


        //    catch (Exception ex)
        //    {


        //        return false;
        //    }

        //}


        //public bool GetPdfNow(int id, int radno, string thenum, string company, DateTime combined)
        //{
        //    try
        //    {

        //        using (var db = new ImportPermitEntities())
        //        {

        //            var log = new LogicObject();
        //            IQueryable<DatabaseObject> items = from t in db.ImportItems
        //                                               where t.ImportApplicationId == id
        //                                               orderby t.EstimatedValue descending

        //                                               select new DatabaseObject
        //                                               {
        //                                                   ImportItemId = t.ImportItemId,
        //                                                   ImportItemProductName = t.Product.Name,
        //                                                   ImportItemEstimatedQuantity = t.EstimatedQuantity,
        //                                                   ImportItemEstimatedValue = t.EstimatedValue,
        //                                                   ImportItemPortOfOrigin = t.CountryOfOriginName,
        //                                                   ImportItemPortOfDischarge = t.DischargeDepotName


        //                                               };

        //            PdfDocument pdf = new PdfDocument();

        //            //Next step is to create a an Empty page.

        //            PdfPage pp = pdf.AddPage();



        //            string path = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "permitup.pdf");

        //            //Then create an XGraphics Object

        //            XGraphics gfx = XGraphics.FromPdfPage(pp);

        //            XImage image = XImage.FromFile(path);
        //            gfx.DrawImage(image, 0, 0);




        //            XFont font = new XFont("Calibri", 12, XFontStyle.Regular);
        //            XFont font2 = new XFont("Calibri", 10, XFontStyle.Regular);


        //            gfx.DrawString(thenum, font, XBrushes.Black, new XRect(392, 70, pp.Width.Point, pp.Height.Point),
        //                XStringFormats.TopLeft);

        //            gfx.DrawString(DateTime.Now.ToString("dd/MM/yy"), font, XBrushes.Black,
        //                new XRect(390, 95, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //            gfx.DrawString("", font, XBrushes.Black, new XRect(70, 125, pp.Width.Point, pp.Height.Point),
        //                XStringFormats.TopLeft);

        //            gfx.DrawString("", font, XBrushes.Black, new XRect(70, 150, pp.Width.Point, pp.Height.Point),
        //                XStringFormats.TopLeft);

        //            gfx.DrawString("", font, XBrushes.Black, new XRect(60, 175, pp.Width.Point, pp.Height.Point),
        //                XStringFormats.TopLeft);


        //            gfx.DrawString(company, font, XBrushes.Black, new XRect(80, 218, pp.Width.Point, pp.Height.Point),
        //                XStringFormats.TopLeft);

        //            gfx.DrawString("", font, XBrushes.Black, new XRect(92, 240, pp.Width.Point, pp.Height.Point),
        //                XStringFormats.TopLeft);



        //            MigraDoc.DocumentObjectModel.Document doc = new MigraDoc.DocumentObjectModel.Document();

        //            Section section = doc.AddSection();

        //            var table = section.AddTable();



        //            //table = section.AddTable();
        //            table.Style = "Table";

        //            table.Borders.Width = 0.25;
        //            table.Borders.Left.Width = 0.5;
        //            table.Borders.Right.Width = 0.5;
        //            table.Rows.LeftIndent = 0;

        //            Column column = table.AddColumn("4cm");
        //            column.Format.Alignment = ParagraphAlignment.Center;


        //            column = table.AddColumn("4cm");
        //            column.Format.Alignment = ParagraphAlignment.Center;

        //            column = table.AddColumn("4cm");
        //            column.Format.Alignment = ParagraphAlignment.Center;

        //            column = table.AddColumn("4cm");
        //            column.Format.Alignment = ParagraphAlignment.Center;



        //            // Create the header of the table
        //            Row row = table.AddRow();
        //            //row = table.AddRow();
        //            row.HeadingFormat = true;
        //            row.Format.Alignment = ParagraphAlignment.Center;
        //            row.Format.Font.Bold = true;


        //            row.Cells[0].AddParagraph("Type of Petroleum Product:");
        //            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;


        //            row.Cells[1].AddParagraph("Country of origin:");
        //            row.Cells[1].Format.Alignment = ParagraphAlignment.Left;

        //            row.Cells[2].AddParagraph("Quantity/Weight (Metric Tones):");
        //            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;

        //            row.Cells[3].AddParagraph("Estimated Value($):");
        //            row.Cells[3].Format.Alignment = ParagraphAlignment.Left;

        //            double total = 0;

        //            if (items.Any() && items.Count() <= 7)
        //            {

        //                foreach (var item in items.ToList())
        //                {
        //                    row = table.AddRow();
        //                    row.Cells[0].AddParagraph(item.ImportItemProductName);
        //                    row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

        //                    total = total + item.ImportItemEstimatedValue;

        //                    row.Cells[1].AddParagraph(item.ImportItemPortOfOrigin);
        //                    row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
        //                    row.Cells[2].AddParagraph(item.ImportItemEstimatedQuantity.ToString());
        //                    row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
        //                    row.Cells[3].AddParagraph(item.ImportItemEstimatedValue.ToString());
        //                    row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
        //                }

        //            }
        //            else if (items.Any() && items.Count() > 7)
        //            {


        //                foreach (var item in items.ToList())
        //                {
        //                    row = table.AddRow();
        //                    row.Format.Font.Size = 8;
        //                    row.Cells[0].AddParagraph(item.ImportItemProductName);
        //                    row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

        //                    total = total + item.ImportItemEstimatedValue;

        //                    row.Cells[1].AddParagraph(item.ImportItemPortOfOrigin);
        //                    row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
        //                    row.Cells[2].AddParagraph(item.ImportItemEstimatedQuantity.ToString());
        //                    row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
        //                    row.Cells[3].AddParagraph(item.ImportItemEstimatedValue.ToString());
        //                    row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
        //                }


        //            }

        //            //convert total amount to words



        //            var amtWords = log.ChangeToWords(total.ToString(), true);


        //            const bool unicode = false;
        //            const PdfFontEmbedding embedding = PdfFontEmbedding.Always;

        //            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);

        //            // Associate the MigraDoc document with a renderer

        //            pdfRenderer.Document = doc;



        //            // Layout and render document to PDF

        //            pdfRenderer.RenderDocument();




        //            var pathtable = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "table.pdf");

        //            pdfRenderer.PdfDocument.Save(pathtable);

        //            XImage imagetable = XImage.FromFile(pathtable);
        //            gfx.DrawImage(imagetable, 0, 280);


        //            //rigid style
        //            XImage image2 = XImage.FromFile(System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "permitdown.pdf"));
        //            gfx.DrawImage(image2, 0, 550);


        //            gfx.DrawString(amtWords, font2, XBrushes.Black, new XRect(135, 556, pp.Width.Point, pp.Height.Point),
        //                XStringFormats.TopLeft);

        //            gfx.DrawString("", font, XBrushes.Black, new XRect(238, 593, pp.Width.Point, pp.Height.Point),
        //                XStringFormats.TopLeft);

        //            gfx.DrawString("", font, XBrushes.Black, new XRect(220, 633, pp.Width.Point, pp.Height.Point),
        //                XStringFormats.TopLeft);

        //            gfx.DrawString(DateTime.Now.ToString("dd/MM/yy"), font, XBrushes.Black,
        //                new XRect(100, 673, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //            gfx.DrawString(combined.ToString("dd/MM/yy"), font, XBrushes.Black,
        //                new XRect(370, 673, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);





        //            string path2 = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "permit" + radno + ".pdf");

        //            //string path2 = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "permit.pdf");

        //            pdf.Save(path2);


        //            return true;

        //        }
        //    }

        //    catch (Exception ex)
        //    {
        //        return false;
        //    }

        //    return false;
        //}


        private List<GroupObject> getGroups()
        {

            try
            {
                return new GroupServices().GetGroups();

            }
            catch (Exception)
            {
                return new List<GroupObject>();
            }
        }




        private List<StepActivityTypeObject> getStepActivityTypes()
        {

            try
            {
                return new StepActivityTypeServices().GetStepActivityTypes();

            }
            catch (Exception)
            {
                return new List<StepActivityTypeObject>();
            }
        }

        private List<ZoneObject> getZones()
        {
            try
            {
                return new ZoneServices().GetZones();

            }
            catch (Exception)
            {
                return new List<ZoneObject>();
            }
        }


    }
}
