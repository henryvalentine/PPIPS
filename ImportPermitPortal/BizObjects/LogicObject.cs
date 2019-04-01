using ImportPermitPortal.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using ImportPermitPortal.EF.Model;
using System.IO;
using Application = ImportPermitPortal.Models.Application;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace ImportPermitPortal.BizObjects
{
    public class LogicObject
    {
        private ImportPermitEntities db = new ImportPermitEntities();

        public string NumberToWords(double number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (Convert.ToInt32(number) < 20)
                    words += unitsMap[Convert.ToInt32(number)];
                else
                {
                    words += tensMap[Convert.ToInt32(number) / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[Convert.ToInt32(number) % 10];
                }
            }

            return words;
        }

        public ICollection<AccountType> RetrieveAllAccountType()
        {
            //if (db.AccountTypes.Count() != 0)
            //{

            List<AccountType> l = new List<AccountType>();
            foreach (var item in db.AccountTypes)
                l.Add(item);

            return l;
            //}
        }

        public ICollection<Structure> RetrieveAllStructureType()
        {
            //if (db.AccountTypes.Count() != 0)
            //{

            List<Structure> l = new List<Structure>();
            foreach (var item in db.Structures)
                l.Add(item);

            return l;
            //}
        }

        public ICollection<DocumentType> RetrieveAllDocumentType()
        {
            //if (db.AccountTypes.Count() != 0)
            //{

            List<DocumentType> l = new List<DocumentType>();
            foreach (var item in db.DocumentTypes)
                l.Add(item);

            return l;
            //}
        }


        public ICollection<Step> RetrieveAllStepType()
        {
            //if (db.AccountTypes.Count() != 0)
            //{

            List<Step> l = new List<Step>();
            foreach (var item in db.Steps)
                l.Add(item);

            return l;
            //}
        }


        public ICollection<Group> RetrieveAllGroups()
        {
            //if (db.AccountTypes.Count() != 0)
            //{

            List<Group> l = new List<Group>();
            foreach (var item in db.Groups)
                l.Add(item);

            return l;
            //}
        }

        public bool CreateRole(string role)
        {
            try
            {
                if (string.IsNullOrEmpty(role))
                {
                    return false;
                }

                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
                if (!roleManager.RoleExists(role.Trim()))
                {
                    var newRole = new IdentityRole { Name = role };
                    var tdd = roleManager.Create(newRole);
                    return (tdd.Succeeded) ? true : false; ;

                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }


       


        //public bool AssignJob(int id)
        //{
        //    try
        //    {
        //        //get the step with the sequence of 1
        //        var step = db.Steps.FirstOrDefault(u => u.SequenceNumber.Equals(id));
             

        //        if (step == null)
        //        {
        //            return false;
        //        }

              

        //        var appstrack = from t in db.ProcessTrackings
        //                                               where t.StepCode == id
        //                                               orderby t.AssignedTime descending

        //                                               select new DatabaseObject
        //                                               {
        //                                                   Id = t.Id,
        //                                                   ProcessTrackingApplicationId = t.ApplicationId,
        //                                                   ApplicationCompanyName = t.Application.Importer.Name,
        //                                                   ApplicationReferenceCode = t.Application.Invoice.ReferenceCode,
        //                                                   ProcessTrackingAssignedTime = t.AssignedTime,
        //                                                   ProcessTrackingDueTime = t.DueTime



        //                                               };



        //        //retrieve the id of the role and get the role of users to assign job to
        //        var rol = db.AspNetRoles.FirstOrDefault(u => u.Name.Equals(step.Name));
             


        //        //use the retrieved role id to query the employeerole table for all the employees that has that role

        //        var records = from t in db.EmployeeRoles
        //                                             where t.RoleId == rol.Id

        //                                             select new DatabaseObject
        //                                             {

        //                                                 Id = t.Id,
        //                                                 EmploeeRolesEmployeeId = t.EmployeeId,
        //                                                 EmploeeRolesRoleId = t.RoleId

        //                                             };



        //        //check the records for the employee whose job count is lowest
        //        if (records.Any())
        //        {
        //            foreach (var item in records.ToList())
        //            {
        //                //retrieve the employeedesk object 

        //                var emp =
        //                    db.EmployeeDesks.FirstOrDefault(u => u.EmployeeId.Equals(item.EmploeeRolesEmployeeId));
        //                if (emp == null)
        //                {

        //                    //the employee has not been added to a group
        //                    return false;

        //                }
        //                //Check for availability
        //                //if (emp.AvailabilityCode == (int)EmployeeDeskEnum.Available)
        //                //{
        //                //check job count
        //                //if (emp.JobCount == 0)
        //                //{

        //                //select an application 
        //                var obj = appstrack.ToList().ElementAt(0);
        //                //Application impapp = db.Applications.Find(obj.ProcessTrackingApplicationId);
        //                //impapp.ApplicationStatusCode = impapp.ApplicationStatusCode + 1;

        //                ProcessTracking track = db.ProcessTrackings.Find(obj.Id);
        //                track.EmployeeId = emp.EmployeeId;
        //                track.AssignedTime = DateTime.Now;

        //                //to get the due time and stepactivitytype get the step object and get the step duration in hrs
        //                var duration = (int)step.ExpectedDeliveryDuration;


        //                var time = new TimeSpan(duration, 0, 0);
        //                var combined = DateTime.Now.Add(time);

        //                track.DueTime = combined;

        //                //track.StepActivityType = step.StepActivityType.Name;


        //                //track.ProcessStatus = EnumProcessStatus.Active.ToString();


        //                //track.StepStatus = EnumStepStatus.Active.ToString();

        //                appstrack.ToList().RemoveAt(0);

        //                db.Entry(track).State = EntityState.Modified;

        //                db.SaveChanges();



                        

        //            }
        //            return true;
        //        }

        //        else
        //        {
        //            return false;
        //        }

               
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        
        //public bool AssignJobFirst(int id)
        //{
        //    try
        //    {
        //        //get the step with the sequence of 1
        //        var step = db.Steps.FirstOrDefault(u => u.SequenceNumber.Equals(id));
               

        //        if (step == null)
        //        {
        //            return false;
        //        }

        //        //select all applications with a status code of 1
        //        var apps = from t in db.Applications where t.Id == id


        //                                          select new DatabaseObject
        //                                          {

        //                                              ApplicationId = t.Id,
        //                                              ApplicationDateApplied = t.DateApplied,
        //                                              ApplicationDerivedTotalQUantity = t.DerivedTotalQUantity,
        //                                              ApplicationReferenceCode = t.Invoice.ReferenceCode,
        //                                              ApplicationStatusCode = t.ApplicationStatusCode


        //                                          };


        //        //retrieve the id of the role and get the role of users to assign job to
        //        var rol = db.AspNetRoles.FirstOrDefault(u => u.Name.Equals(step.Name));
               
        //        //use the retrieved role id to query the employeerole table for all the employees that has that role

        //        IQueryable<DatabaseObject> records = from t in db.EmployeeRoles
        //                                             where t.RoleId == rol.Id

        //                                             select new DatabaseObject
        //                                             {

        //                                                 Id = t.Id,
        //                                                 EmploeeRolesEmployeeId = t.EmployeeId,
        //                                                 EmploeeRolesRoleId = t.RoleId

        //                                             };



        //        //check the records for the employee whose job count is lowest
        //        if (records.Any())
        //        {
        //            foreach (var item in records.ToList())
        //            {
        //                //retrieve the employeedesk object 

        //                var emp = db.EmployeeDesks.FirstOrDefault(u => u.EmployeeId.Equals(item.EmploeeRolesEmployeeId));
        //                if (emp == null)
        //                {

        //                    //the employee has not been added to a group
        //                    return false;

        //                }
        //                //Check for availability
        //                //if (emp.AvailabilityCode == (int)EmployeeDeskEnum.Available)
        //                //{
        //                //check job count
        //                //if (emp.JobCount == 0)
        //                //{

        //                //select an application 
        //                var obj = apps.ToList().ElementAt(0);
        //                var impapp = db.Applications.Find(obj.ApplicationId);
        //                impapp.ApplicationStatusCode = impapp.ApplicationStatusCode + 1;
        //                apps.ToList().RemoveAt(0);

        //                db.Entry(impapp).State = EntityState.Modified;

        //                db.SaveChanges();



        //                //assign job
        //                var track = new ProcessTracking();
        //                track.ApplicationId = impapp.Id;
        //                track.StepId = step.Id;
        //                track.StepCode = id;
        //                track.EmployeeId = emp.EmployeeId;
        //                track.AssignedTime = DateTime.Now;

        //                //to get the due time and stepactivitytype get the step object and get the step duration in hrs
        //                var duration = (int)step.ExpectedDeliveryDuration;


        //                var time = new TimeSpan(duration, 0, 0);
        //                var combined = DateTime.Now.Add(time);

        //                track.DueTime = combined;

        //                //track.StepActivityType = step.StepActivityType.Name;


        //                //track.ProcessStatus = EnumProcessStatus.Active.ToString();


        //                //track.StepStatus = EnumStepStatus.Active.ToString();



        //                db.ProcessTrackings.Add(track);
        //                db.SaveChanges();




        //                //}
        //                //}

        //            }
        //            return true;
        //        }



        //        else
        //        {
        //            return false;
        //        }



                
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}



        //public bool GeneratePermit(int appId)
        //{
        //    ImportPermitPortal.EF.Model.Application importapplication = db.Applications.Find(appId);

        //    IQueryable<DatabaseObject> items = from t in db.ApplicationItems
        //                                       where t.ApplicationId == appId
        //                                       orderby t.EstimatedValue descending

        //                                       select new DatabaseObject
        //                                       {
        //                                           ApplicationItemId = t.ApplicationItemId,
        //                                           ApplicationItemProductName = t.Product.Name,
        //                                           ApplicationItemEstimatedQuantity = t.EstimatedQuantity,
        //                                           ApplicationItemEstimatedValue = t.EstimatedValue,
        //                                           ApplicationItemPortOfOrigin = t.CountryOfOriginName,
        //                                           ApplicationItemPortOfDischarge = t.DischargeDepotName


        //                                       };


        //    Random rad = new Random();
        //    var radno = rad.Next();


        //    Permit perm = new Permit();
        //    perm.ApplicationId = appId;
        //    perm.IssueDate = DateTime.Now;
        //    perm.PermitNo = radno;

        //    TimeSpan time = new TimeSpan(90, 0, 0, 0);
        //    DateTime combined = DateTime.Now.Add(time);

        //    perm.ExpiryDate = combined;

        //    perm.Status = EnumPermitStatus.Active.ToString();


        //    //generate the permit
        //    string path;
        //    string path2;
        //    string path3;
        //    string pathsignature;

        //    PdfDocument pdf = new PdfDocument();

        //    //Next step is to create a an Empty page.

        //    PdfPage pp = pdf.AddPage();



        //    path = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "permitup.pdf");

        //    //Then create an XGraphics Object

        //    XGraphics gfx = XGraphics.FromPdfPage(pp);

        //    XImage image = XImage.FromFile(System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "permitup.pdf"));
        //    gfx.DrawImage(image, 0, 0);

           
        //    //find the last generated permit
        //    //   var maxValue = db.Permits.Max(x => x.Id);
        //    // var result = db.Permits.First(x => x.Id == maxValue);
        //    //int permno;

        //    //if(result != null)
        //    //{
        //    //    permno = result.Id;
        //    //}
        //    //else
        //    //{
        //    //    permno = 1;
        //    //}

        //    XFont font = new XFont("Calibri", 12, XFontStyle.Regular);


        //    gfx.DrawString("DPR/1", font, XBrushes.Black, new XRect(392, 70, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //    gfx.DrawString(DateTime.Now.ToString("dd/MM/yy"), font, XBrushes.Black, new XRect(390, 95, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //    gfx.DrawString("", font, XBrushes.Black, new XRect(70, 125, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //    gfx.DrawString("", font, XBrushes.Black, new XRect(70, 150, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //    gfx.DrawString("", font, XBrushes.Black, new XRect(60, 175, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


        //    gfx.DrawString(importapplication.Importer.Name, font, XBrushes.Black, new XRect(80, 213, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //    gfx.DrawString("", font, XBrushes.Black, new XRect(92, 240, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

           

        //    MigraDoc.DocumentObjectModel.Document doc = new MigraDoc.DocumentObjectModel.Document();

        //    Section section = doc.AddSection();

        //    var table = section.AddTable();

        //    //table = section.AddTable();
        //    table.Style = "Table";

        //    table.Borders.Width = 0.25;
        //    table.Borders.Left.Width = 0.5;
        //    table.Borders.Right.Width = 0.5;
        //    table.Rows.LeftIndent = 0;

        //    Column column = table.AddColumn("4cm");
        //    column.Format.Alignment = ParagraphAlignment.Center;


        //    column = table.AddColumn("4cm");
        //    column.Format.Alignment = ParagraphAlignment.Center;

        //    column = table.AddColumn("4cm");
        //    column.Format.Alignment = ParagraphAlignment.Center;

        //    column = table.AddColumn("4cm");
        //    column.Format.Alignment = ParagraphAlignment.Center;



        //    // Create the header of the table
        //    Row row = table.AddRow();
        //    //row = table.AddRow();
        //    row.HeadingFormat = true;
        //    row.Format.Alignment = ParagraphAlignment.Center;
        //    row.Format.Font.Bold = true;





        //    row.Cells[0].AddParagraph("Type of Petroleum Product:");
        //    row.Cells[0].Format.Alignment = ParagraphAlignment.Left;


        //    row.Cells[1].AddParagraph("Country of origin:");
        //    row.Cells[1].Format.Alignment = ParagraphAlignment.Left;

        //    row.Cells[2].AddParagraph("Quantity/Weight (Metric Tones):");
        //    row.Cells[2].Format.Alignment = ParagraphAlignment.Left;

        //    row.Cells[3].AddParagraph("Estimated Value($):");
        //    row.Cells[3].Format.Alignment = ParagraphAlignment.Left;

        //    double total = 0;

        //    if (items != null && items.Count() <= 7)
        //    {

        //        foreach (var item in items.ToList())
        //        {
        //            row = table.AddRow();
        //            row.Cells[0].AddParagraph(item.ApplicationItemProductName);
        //            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

        //            total = total + item.ApplicationItemEstimatedValue;

        //            row.Cells[1].AddParagraph(item.ApplicationItemPortOfOrigin);
        //            row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
        //            row.Cells[2].AddParagraph(item.ApplicationItemEstimatedQuantity.ToString());
        //            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
        //            row.Cells[3].AddParagraph(item.ApplicationItemEstimatedValue.ToString());
        //            row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
        //        }

        //    }
        //    else if (items != null && items.Count() > 7)
        //    {


        //        foreach (var item in items.ToList())
        //        {
        //            row = table.AddRow();
        //            row.Format.Font.Size = 8;
        //            row.Cells[0].AddParagraph(item.ApplicationItemProductName);
        //            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

        //            total = total + item.ApplicationItemEstimatedValue;

        //            row.Cells[1].AddParagraph(item.ApplicationItemPortOfOrigin);
        //            row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
        //            row.Cells[2].AddParagraph(item.ApplicationItemEstimatedQuantity.ToString());
        //            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
        //            row.Cells[3].AddParagraph(item.ApplicationItemEstimatedValue.ToString());
        //            row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
        //        }


        //    }


        //    const bool unicode = false;
        //    const PdfFontEmbedding embedding = PdfFontEmbedding.Always;

        //    PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);

        //    // Associate the MigraDoc document with a renderer

        //    pdfRenderer.Document = doc;



        //    // Layout and render document to PDF

        //    pdfRenderer.RenderDocument();


         

        //    var pathtable = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "table.pdf");

        //    pdfRenderer.PdfDocument.Save(pathtable);

        //    XImage imagetable = XImage.FromFile(System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "table.pdf"));
        //    gfx.DrawImage(imagetable, 0, 280);


        //    XImage image2 = XImage.FromFile(System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "permitdown.pdf"));
        //    gfx.DrawImage(image2, 0, 550);


        //    gfx.DrawString("total", font, XBrushes.Black, new XRect(135, 552, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //    gfx.DrawString("dpr fee", font, XBrushes.Black, new XRect(238, 593, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //    gfx.DrawString("app fee", font, XBrushes.Black, new XRect(220, 633, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //    gfx.DrawString("issue", font, XBrushes.Black, new XRect(100, 673, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

        //    gfx.DrawString("expiry", font, XBrushes.Black, new XRect(370, 673, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


        

        //    //path2 = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "perm2.pdf");

        //    path2 = System.IO.Path.Combine(Server.MapPath("~/PermDoc"), "perm2.pdf");





        //    var way = @"\PermDoc\" + "perm2.pdf";

        //    perm.file = way;


        //    pdf.Save(path2);

        //    db.Permits.Add(perm);
        //    db.SaveChanges();




        //    return true;

        //}




        public String ChangeToWords(String numb, bool isCurrency)
        {
            String val = "", wholeNo = numb, points = "", andStr = "", pointStr = "";
            String endStr = (isCurrency) ? ("naira only") : ("");
            try
            {
                int decimalPlace = numb.IndexOf(".");
                if (decimalPlace > 0)
                {
                    wholeNo = numb.Substring(0, decimalPlace);
                    points = numb.Substring(decimalPlace + 1);
                    if (Convert.ToInt32(points) > 0)
                    {
                        andStr = (isCurrency) ? ("and") : ("point");// just to separate whole numbers from points/cents
                        endStr = (isCurrency) ? ("Cents " + endStr) : ("");
                        pointStr = TranslateCents(points);
                    }
                }
                val = String.Format("{0} {1}{2} {3}", TranslateWholeNumber(wholeNo).Trim(), andStr, pointStr, endStr);
            }
            catch { ;}
            return val;
        }



        private String TranslateCents(String cents)
            {
                String cts = "", digit = "", engOne = "";
                for (int i = 0; i < cents.Length; i++)
                {
                    digit = cents[i].ToString();
                    if (digit.Equals("0"))
                        {
                            engOne = "Zero";
                        }
                    else
                        {
                            engOne = ones(digit);
                        }
                    cts += " " + engOne;
                }
                return cts;
            }



        private String TranslateWholeNumber(String number)
        {
            string word = "";
            try
            {
                bool beginsZero = false;//tests for 0XX
                bool isDone = false;//test if already translated
                double dblAmt = (Convert.ToDouble(number));
                //if ((dblAmt > 0) && number.StartsWith("0"))
                if (dblAmt > 0)
                {//test for zero or digit zero in a nuemric
                    beginsZero = number.StartsWith("0");
                    int numDigits = number.Length;
                    int pos = 0;//store digit grouping
                    String place = "";//digit grouping name:hundres,thousand,etc...
                    switch (numDigits)
                    {
                        case 1://ones' range
                            word = ones(number);
                            isDone = true;
                            break;
                        case 2://tens' range
                            word = tens(number);
                            isDone = true;
                            break;
                        case 3://hundreds' range
                            pos = (numDigits % 3) + 1;
                            place = " Hundred ";
                            break;
                        case 4://thousands' range
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            place = " Thousand ";
                            break;
                        case 7://millions' range
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            place = " Million ";
                            break;
                        case 10://Billions's range
                            pos = (numDigits % 10) + 1;
                            place = " Billion ";
                            break;
                        //add extra case options for anything above Billion...
                        default:
                            isDone = true;
                            break;
                    }
                    if (!isDone)
                    {//if transalation is not done, continue...(Recursion comes in now!!)
                        word = TranslateWholeNumber(number.Substring(0, pos)) + place + TranslateWholeNumber(number.Substring(pos));
                        //check for trailing zeros
                        if (beginsZero) word = " and " + word.Trim();
                    }
                    //ignore digit grouping names
                    if (word.Trim().Equals(place.Trim())) word = "";
                }
            }
            catch { ;}
            return word.Trim();
        }

        public String tens(String digit)
        {
            int digt = Convert.ToInt32(digit);
            String name = null;
            switch (digt)
            {
                case 10:
                    name = "Ten";
                    break;
                case 11:
                    name = "Eleven";
                    break;
                case 12:
                    name = "Twelve";
                    break;
                case 13:
                    name = "Thirteen";
                    break;
                case 14:
                    name = "Fourteen";
                    break;
                case 15:
                    name = "Fifteen";
                    break;
                case 16:
                    name = "Sixteen";
                    break;
                case 17:
                    name = "Seventeen";
                    break;
                case 18:
                    name = "Eighteen";
                    break;
                case 19:
                    name = "Nineteen";
                    break;
                case 20:
                    name = "Twenty";
                    break;
                case 30:
                    name = "Thirty";
                    break;
                case 40:
                    name = "Fourty";
                    break;
                case 50:
                    name = "Fifty";
                    break;
                case 60:
                    name = "Sixty";
                    break;
                case 70:
                    name = "Seventy";
                    break;
                case 80:
                    name = "Eighty";
                    break;
                case 90:
                    name = "Ninety";
                    break;
                default:
                    if (digt > 0)
                    {
                        name = tens(digit.Substring(0, 1) + "0") + " " + ones(digit.Substring(1));
                    }
                    break;
            }
            return name;
        }


        public String ones(String digit)
         {
            int digt = Convert.ToInt32(digit);
            String name = "";
            switch (digt)
                {
                    case 1:
                    name = "One";
                    break;
                    case 2:
                    name = "Two";
                    break;
                    case 3:
                    name = "Three";
                    break;
                    case 4:
                    name = "Four";
                    break;
                    case 5:
                    name = "Five";
                    break;
                    case 6:
                    name = "Six";
                    break;
                    case 7:
                    name = "Seven";
                    break;
                    case 8:
                    name = "Eight";
                    break;
                    case 9:
                    name = "Nine";
                    break;
                }
                    return name;
                }

        public void AddHistory(long appId, DateTime? assignedTime, DateTime dateLeft, int employeeId, int stepId, int outComeCode)
        {
            var prohis = new ProcessingHistory();
            prohis.ApplicationId = appId;
            prohis.AssignedTime = assignedTime;
            prohis.FinishedTime = dateLeft;
            prohis.EmployeeId = employeeId;
            prohis.StepId = stepId;
            prohis.OutComeCode = outComeCode;
            


            db.ProcessingHistories.Add(prohis);

            db.SaveChanges();
            
        }





    }
}