using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
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
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
//using ShopKeeper.GenericHelpers;
//using ImportApplication = ImportPermitPortal.EF.Model.ImportApplication;

namespace ImportPermitPortal.Controllers
{
     [Authorize(Roles = "Super_Admin,Support")]
    public class AdminController : Controller
    {
        public ActionResult GetNotificationAdmin(long id)
        {
            var rep = new Reporter();
            try
            {

                if (id < 1)
                {
                    return Json(new Notification(), JsonRequestBehavior.AllowGet);
                }

                return Json(GetNotificationAdminInfo(id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult GetRecertificationObjects(JQueryDataTableParamModel param)
        {
            try
            {
                IEnumerable<RecertificationObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = GetAdminRecertifications(param.iDisplayLength, param.iDisplayStart, out countG);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new RecertificationServices().Search(param.sSearch);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<RecertificationObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<RecertificationObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.ReferenceCode : c.DateApplied.ToString());

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "asc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.ReferenceCode, c.DateApplied.ToString(), c.StatusStr };
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
                return Json(new List<RecertificationObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        private List<RecertificationObject> GetAdminRecertifications(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new RecertificationServices().GetAdminRecertifications(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<RecertificationObject>();
            }
        }

        private NotificationObject GetNotificationAdminInfo(long id)
        {
            return new EmployeeProfileServices().GetNotificationAdmin(id);
        }


        public ActionResult GetRecertification(long id)
        {
            try
            {


                var recertification = new RecertificationServices().GetRecertification(id);
                if (recertification == null || recertification.Id < 1)
                {
                    return Json(new RecertificationObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_recertification"] = recertification;

                return Json(recertification, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new RecertificationObject(), JsonRequestBehavior.AllowGet);
            }
        }

       
        public ActionResult PrintDischargeDataNow(long id)
        {

            var way = new PObject();
            double? netVolTrack = 0;
            double vesselQuantity = 0;
            double? density = 0;

            try
            {
                Random rad = new Random();
                var radno = rad.Next();

                PdfDocument pdf = new PdfDocument();

                //Next step is to create a an Empty page.

                PdfPage pp = pdf.AddPage();

                pp.Size = PageSize.A4;
                pp.Orientation = PageOrientation.Landscape;

                string path = Path.Combine(Server.MapPath("~/InspectionDoc"), "discharge.pdf");

                //Then create an XGraphics Object

                XGraphics gfx = XGraphics.FromPdfPage(pp);

                XImage image = XImage.FromFile(path);
                gfx.DrawImage(image, 0, 0);




                XFont font = new XFont("Calibri", 10, XFontStyle.Regular);

                using (var db = new ImportPermitEntities())
                {
                    List<NotificationDischageData> dischargeList = new List<NotificationDischageData>();
                    List<DischargeParameterAfter> afterList = new List<DischargeParameterAfter>();
                    List<DischargeParameterBefore> beforeList = new List<DischargeParameterBefore>();

                    //get  the notification object
                    var notification = db.Notifications.Where(n => n.Id == id && n.Status == (int)NotificationStatusEnum.Approved).ToList();

                    var depotId = 0;

                    if (notification.Any())
                    {
                        depotId = notification[0].DischargeDepotId;


                        //get product
                        gfx.DrawString(notification[0].Product.Name, font, XBrushes.Black,
                            new XRect(280, 133, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //get arrival date
                        gfx.DrawString(notification[0].ArrivalDate.ToString("dd/MM/yy"), font, XBrushes.Black,
                            new XRect(280, 142, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //get vessel inspection
                        var vesselInspection =
                          db.NotificationInspections.Where(
                              r =>
                                  r.NotificationId == id &&
                                  r.SubmittionStatus == (int)EnumCheckListOutComeStatus.Saved).ToList();
                        if (vesselInspection.Any())
                        {
                            gfx.DrawString(vesselInspection[0].DischargeCommencementDate.ToString(), font, XBrushes.Black,
                         new XRect(280, 152, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                            gfx.DrawString(vesselInspection[0].DischargeCompletionDate.ToString(), font, XBrushes.Black,
                        new XRect(280, 162, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        }


                        //vessel arrival quantity
                        gfx.DrawString(notification[0].QuantityToDischarge.ToString(), font, XBrushes.Black,
                            new XRect(335, 483, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        vesselQuantity = notification[0].QuantityToDischarge;

                        //get the consignee
                        gfx.DrawString(notification[0].Importer.Name, font, XBrushes.Black,
                            new XRect(335, 524, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }

                    //get tanks in the depot
                    var tanks = db.StorageTanks.Where(t => t.DepotId == depotId).ToList();

                    if (tanks.Any())
                    {
                        //get all the dischargedatas for the tanks in the depot that was filled
                        foreach (var item in tanks)
                        {
                            var dischargeData =
                           db.NotificationDischageDatas.Where(
                               d => d.NotificationId == id &&
                                    d.TankId == item.Id)
                               .ToList();
                            if (dischargeData.Any())
                            {
                                dischargeList.Add(dischargeData[0]);
                            }

                            var parameterBefore =
                          db.DischargeParameterBefores.Where(
                              p =>
                                  p.NotificationId == id &&
                                  p.TankId == item.Id).ToList();
                            if (parameterBefore.Any())
                            {
                                beforeList.Add(parameterBefore[0]);
                            }

                            var parameterAfter =
                          db.DischargeParameterAfters.Where(
                              p =>
                                  p.NotificationId == id &&
                                  p.TankId == item.Id).ToList();

                            if (parameterAfter.Any())
                            {
                                afterList.Add(parameterAfter[0]);
                            }
                        }

                    }
                    else
                    {
                        way.NoCert = true;
                        return Json(way, JsonRequestBehavior.AllowGet);
                    }

                    var count = dischargeList.Count;

                    //get the density from recertification result

                    var cert =
                        db.RecertificationResults.Where(r => r.NotificationId == id)
                            .ToList();
                    if (cert.Any())
                    {
                        density = cert[0].Density;

                    }

                    //first tank
                    if (1 <= count)
                    {
                        gfx.DrawString(dischargeList[0].StorageTank.TankNo, font, XBrushes.Black,
                            new XRect(103, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(afterList[0].TankGuage.ToString(), font, XBrushes.Black,
                            new XRect(163, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].TankGuage.ToString(), font, XBrushes.Black,
                            new XRect(163, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(afterList[0].TankTC.ToString(), font, XBrushes.Black,
                            new XRect(200, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].TankTC.ToString(), font, XBrushes.Black,
                            new XRect(200, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);

                        gfx.DrawString(afterList[0].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black,
                            new XRect(230, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black,
                            new XRect(230, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(afterList[0].SGtC_Lab.ToString(), font, XBrushes.Black,
                            new XRect(280, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].SGtC_Lab.ToString(), font, XBrushes.Black,
                            new XRect(280, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(afterList[0].VolOfWaterLTRS.ToString(), font, XBrushes.Black,
                            new XRect(335, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].VolOfWaterLTRS.ToString(), font, XBrushes.Black,
                            new XRect(335, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[0].CrossVol_TkPcLTRS - afterList[0].VolOfWaterLTRS;
                        var netVolBefore = beforeList[0].CrossVol_TkPcLTRS - beforeList[0].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black,
                            new XRect(385, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black,
                            new XRect(385, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);

                        //net vol
                        var volAfter = netOilVolAfter * afterList[0].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[0].VolCorrFactor;
                        var netVol = volAfter - volBefore;


                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;

                        //Density
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black,
                            new XRect(450, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black,
                            new XRect(450, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol corr factor
                        gfx.DrawString(afterList[0].VolCorrFactor.ToString(), font, XBrushes.Black,
                            new XRect(505, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[0].VolCorrFactor.ToString(), font, XBrushes.Black,
                            new XRect(505, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);


                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black,
                            new XRect(575, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black,
                            new XRect(575, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black,
                          new XRect(575, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;


                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black,
                            new XRect(638, 210, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black,
                            new XRect(638, 223, pp.Width.Point, pp.Height.Point),
                            XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black,
                          new XRect(638, 238, pp.Width.Point, pp.Height.Point),
                          XStringFormats.TopLeft);

                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 210, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 223, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 238, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    //2nd tank

                    if (2 <= count)
                    {
                        gfx.DrawString(dischargeList[1].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 250, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[1].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[1].CrossVol_TkPcLTRS - afterList[1].VolOfWaterLTRS;
                        var netVolBefore = beforeList[1].CrossVol_TkPcLTRS - beforeList[1].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 278, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 277, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[1].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[1].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 278, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                        //net vol
                        var volAfter = netOilVolAfter * afterList[1].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[1].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;


                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 250, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 278, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 277, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 278, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 250, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 263, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 280, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                    }

                    //3rd tank
                    if (3 <= count)
                    {
                        gfx.DrawString(dischargeList[2].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 297, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[2].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        //vol of oil

                        var netOilVolAfter = afterList[2].CrossVol_TkPcLTRS - afterList[2].VolOfWaterLTRS;
                        var netVolBefore = beforeList[2].CrossVol_TkPcLTRS - beforeList[2].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[2].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[2].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);




                        //net vol
                        var volAfter = netOilVolAfter * afterList[2].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[2].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;


                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 297, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 297, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 310, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 325, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }



                    //4th tank
                    if (4 <= count)
                    {
                        gfx.DrawString(dischargeList[3].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 344, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[3].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[3].CrossVol_TkPcLTRS - afterList[3].VolOfWaterLTRS;
                        var netVolBefore = beforeList[3].CrossVol_TkPcLTRS - beforeList[3].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[3].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[3].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        //net vol
                        var volAfter = netOilVolAfter * afterList[3].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[3].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;

                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 344, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 344, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 357, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 370, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }


                    //5th tank
                    if (5 <= count)
                    {
                        gfx.DrawString(dischargeList[4].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 384, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[4].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[4].CrossVol_TkPcLTRS - afterList[4].VolOfWaterLTRS;
                        var netVolBefore = beforeList[4].CrossVol_TkPcLTRS - beforeList[4].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[4].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[4].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        //net vol
                        var volAfter = netOilVolAfter * afterList[4].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[4].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;

                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 384, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 384, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 412, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }


                    //6th tank
                    if (6 <= count)
                    {
                        gfx.DrawString(dischargeList[5].StorageTank.TankNo, font, XBrushes.Black, new XRect(103, 426, pp.Width.Point, pp.Height.Point),
                       XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].TankGuage.ToString(), font, XBrushes.Black, new XRect(163, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tgD", font, XBrushes.Black, new XRect(163, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString(afterList[5].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].TankTC.ToString(), font, XBrushes.Black, new XRect(200, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("tcD", font, XBrushes.Black, new XRect(200, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].CrossVol_TkPcLTRS.ToString(), font, XBrushes.Black, new XRect(230, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("cvD", font, XBrushes.Black, new XRect(240, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].SGtC_Lab.ToString(), font, XBrushes.Black, new XRect(280, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sgD", font, XBrushes.Black, new XRect(280, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].VolOfWaterLTRS.ToString(), font, XBrushes.Black, new XRect(335, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vwD", font, XBrushes.Black, new XRect(335, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        //vol of oil

                        var netOilVolAfter = afterList[5].CrossVol_TkPcLTRS - afterList[5].VolOfWaterLTRS;
                        var netVolBefore = beforeList[5].CrossVol_TkPcLTRS - beforeList[5].VolOfWaterLTRS;

                        gfx.DrawString(netOilVolAfter.ToString(), font, XBrushes.Black, new XRect(385, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolBefore.ToString(), font, XBrushes.Black, new XRect(385, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("nvD", font, XBrushes.Black, new XRect(385, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(cert[0].Density.ToString(), font, XBrushes.Black, new XRect(450, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("sg@D", font, XBrushes.Black, new XRect(450, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(afterList[5].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(beforeList[5].VolCorrFactor.ToString(), font, XBrushes.Black, new XRect(505, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        //gfx.DrawString("vcD", font, XBrushes.Black, new XRect(540, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        //net vol
                        var volAfter = netOilVolAfter * afterList[5].VolCorrFactor;
                        var volBefore = netVolBefore * beforeList[5].VolCorrFactor;
                        var netVol = volAfter - volBefore;

                        //Equivalent Vol
                        var volInTonnesAfter = volAfter * cert[0].Density;
                        var volInTonnesBefore = volBefore * cert[0].Density;

                        var netVolInTonnes = volInTonnesAfter - volInTonnesBefore;

                        //update the total shore value
                        netVolTrack = netVolTrack + netVol;


                        gfx.DrawString(volAfter.ToString(), font, XBrushes.Black, new XRect(575, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volBefore.ToString(), font, XBrushes.Black, new XRect(575, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVol.ToString(), font, XBrushes.Black, new XRect(575, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString(volInTonnesAfter.ToString(), font, XBrushes.Black, new XRect(638, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(volInTonnesBefore.ToString(), font, XBrushes.Black, new XRect(638, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString(netVolInTonnes.ToString(), font, XBrushes.Black, new XRect(638, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    }
                    else
                    {
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(103, 426, pp.Width.Point, pp.Height.Point),
                     XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(163, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(200, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(230, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(240, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(280, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(335, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 402, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(385, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(450, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(505, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(540, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(575, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 426, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 442, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        gfx.DrawString("X", font, XBrushes.Black, new XRect(638, 454, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                    }


                    //quantity received into shore tank
                    gfx.DrawString(netVolTrack.ToString(), font, XBrushes.Black,
                        new XRect(335, 496, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                    var shorepercent = (vesselQuantity - netVolTrack) / 100;
                    gfx.DrawString(shorepercent.ToString(), font, XBrushes.Black,
                        new XRect(335, 511, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);


                    //get the depot
                    var depot = db.Depots.Where(d => d.Id == depotId).ToList();

                    var portName = "";
                    if (depot.Any())
                    {
                        gfx.DrawString(depot[0].Name, font, XBrushes.Black,
                          new XRect(280, 110, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                        var jettyId = depot[0].JettyId;

                        var jetty = db.Jetties.Find(jettyId);
                        portName = jetty.Port.Name;

                        var jettyMapping = db.JettyMappings.Where(j => j.JettyId == jettyId).ToList();

                        if (jettyMapping.Any())
                        {
                            //get the zone
                            gfx.DrawString(jettyMapping[0].Zone.Name, font, XBrushes.Black, new XRect(575, 75, pp.Width.Point, pp.Height.Point),
                         XStringFormats.TopLeft);
                        }

                    }

                    var permNum = "";

                    var numbe = id;

                    var counting = numbe.ToString().Length;
                    if (counting == 1)
                    {
                        permNum = "00000" + numbe;
                    }
                    else if (counting == 2)
                    {
                        permNum = "0000" + numbe;
                    }
                    else if (counting == 3)
                    {
                        permNum = "000" + numbe;
                    }
                    else if (counting == 4)
                    {
                        permNum = "00" + numbe;
                    }
                    else if (counting == 5)
                    {
                        permNum = "0" + numbe;
                    }
                    else if (counting >= 6)
                    {
                        permNum = numbe.ToString();
                    }

                    var coq = "DPR/DS/PPIPS/" + portName + "/" + permNum;
                    //serial number
                    gfx.DrawString("CoQ Number:" + " " + coq, font, XBrushes.Black, new XRect(560, 50, pp.Width.Point, pp.Height.Point),
                        XStringFormats.TopLeft);

                    //get the vessel
                    var notificationVessel =
                        db.NotificationVessels.Where(v => v.NotificationId == id).ToList();

                    if (notificationVessel.Any())
                    {
                        var vesselId = notificationVessel[0].VesselId;

                        var vessel = db.Vessels.Where(v => v.VesselId == vesselId).ToList();

                        if (vessel.Any())
                        {
                            gfx.DrawString(vessel[0].Name, font, XBrushes.Black,
                           new XRect(280, 122, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);
                        }
                    }



                    string path2 = Path.Combine(Server.MapPath("~/TempDoc"), "discharge2" + radno + ".pdf");


                    pdf.Save(path2);

                    way.SmallPath = @"\TempDoc\\" + "discharge2" + radno + ".pdf";
                    way.BigPath = Path.Combine(Server.MapPath("~/TempDoc"), "discharge2" + radno + ".pdf");


                    return Json(way, JsonRequestBehavior.AllowGet);


                }
            }
            catch (Exception ex)
            {
                way.Error = true;
                return Json(way, JsonRequestBehavior.AllowGet);

            }
        }

        public string PrintRecertification(long id)
        {

           

            try
            {
                var r = new Random();
                var radno = r.Next();
                using (var db = new ImportPermitEntities())
                {

                    var recertifications = db.Recertifications.Where(c => c.Id == id).Include("Notification").ToList();

                    var recertification = recertifications[0];
                    //get the permit number
                   // var permitNo = recertification.Notification.Permit.PermitValue;
                    var notification = recertification.Notification;
                    //get the company name
                    var company = recertification.Notification.Importer.Name;
                    var mthVesslType = (int) VesselClassEnum.Mother_Vessel;
                    var shtlVessType = (int)VesselClassEnum.Shuttle_Vessel;
                    //get the vessel

                    var notId = recertification.NotificationId;

                    var notVessels = db.NotificationVessels.Where(n => n.NotificationId == notId).Include("Vessel").ToList();

                    if (notVessels.Any())
                    {
                        var motherVessel = notVessels.Find(v => v.VesselClassTypeId == mthVesslType);
                        var shuttleVessel = notVessels.Find(v => v.VesselClassTypeId == shtlVessType);

                        if (motherVessel != null && motherVessel.NotificationVesselId > 0 && shuttleVessel != null && shuttleVessel.NotificationVesselId > 0)
                        {
                            var motherVesselName = motherVessel.Vessel.Name;
                            var shuttleName = shuttleVessel.Vessel.Name;

                        var product = db.Products.Find(notification.ProductId);
                        var productName = product.Name;

                        var depotId = recertification.Notification.DischargeDepotId;
                        var depot = db.Depots.Find(depotId);
                        var jettyId = depot.JettyId;
                        var jetty = db.Jetties.Find(jettyId);
                        var jettyName = jetty.Name;


                        var inspection =
                            db.NotificationInspections.Where(n => n.NotificationId == notId).ToList();

                            if (inspection.Any())
                            {
                                var commenceDate = inspection[0].DischargeCommencementDate.ToString();
                                var finishDate = inspection[0].DischargeCompletionDate.ToString();





                                var items = from t in db.NotificationInspections
                                    where t.NotificationId == notId


                                    select new NotificationInspectionObject()
                                    {
                                        Id = t.Id,
                                        DepotName = t.Depot.Name,
                                        QuantityDischargedStr = t.QuantityDischarged.ToString(),
                                        QuantityDischarged = t.QuantityDischarged

                                    };


                                var permitId = notification.PermitId;

                                var permVal = "";
                                var permit = db.Permits.Where(p => p.Id == permitId).ToList();
                                if (permit.Any())
                                {
                                    permVal = permit[0].PermitValue;
                                }

                                PdfDocument pdf = new PdfDocument();

                                //Next step is to create a an Empty page.

                                PdfPage pp = pdf.AddPage();


                                pp.Size = PageSize.A4;




                                string path = Path.Combine(Server.MapPath("~/PermDoc"), "recert.pdf");

                                //Then create an XGraphics Object

                                XGraphics gfx = XGraphics.FromPdfPage(pp);

                                XImage image = XImage.FromFile(path);
                                gfx.DrawImage(image, 0, 0);




                                XFont font = new XFont("Calibri", 12, XFontStyle.Regular);
                                XFont font2 = new XFont("Calibri", 8, XFontStyle.Regular);

                                // ref number
                                gfx.DrawString(recertification.ReferenceCode, font, XBrushes.Black,
                                    new XRect(352, 182, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                                gfx.DrawString(DateTime.Now.ToString("dd/MM/yyyy"), font, XBrushes.Black,
                                    new XRect(352, 210, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);

                                gfx.DrawString(permVal, font, XBrushes.Black,
                                    new XRect(342, 410, pp.Width.Point, pp.Height.Point), XStringFormats.TopLeft);



                                MigraDoc.DocumentObjectModel.Document doc =
                                    new MigraDoc.DocumentObjectModel.Document();



                                Section section = doc.AddSection();

                                var table = section.AddTable();



                                //table = section.AddTable();
                                table.Style = "Table";

                                table.Borders.Width = 0;
                                table.Borders.Left.Width = 0;
                                table.Borders.Right.Width = 0;
                                table.Rows.LeftIndent = 0;

                                Column column = table.AddColumn("16cm");
                                column.Format.Alignment = ParagraphAlignment.Center;



                                // Create the header of the table
                                Row row = table.AddRow();


                                row.Cells[0].AddParagraph("We hereby certify that" + " " + company + " " +
                                                            "through" +
                                                            " " +
                                                            motherVesselName + "(" +
                                                            shuttleName + ")" + " " +
                                                            " delivered the following quantity of" + " " +
                                                            productName + " " +
                                                            "in the facility of the listed company at " + jettyName +
                                                            "," +
                                                            " " + " from " +
                                                            commenceDate +
                                                            "-" + finishDate);

                                row.Cells[0].Format.Alignment = ParagraphAlignment.Left;


                                row = table.AddRow();
                                row = table.AddRow();


                                row = table.AddRow();

                                //get bank name

                                var permApp = db.PermitApplications.Where(p => p.PermitId == permitId).ToList();

                                long appId = 0;
                                if (permApp.Any())
                                {
                                    appId = permApp[0].ApplicationId;

                                }

                                var application = db.Applications.Where(a => a.Id == appId).Include("ApplicationItems").ToList();

                                var bank = "";
                                if (application.Any())
                                {
                                    var aps = application[0].ApplicationItems.ToList();

                                    aps.ForEach(n =>
                                    {
                                        var bankers = db.ProductBankers.Where(s => s.ApplicationItemId == n.Id).Include("Bank").ToList();
                                        if (bankers.Any())
                                        {
                                            bankers.ForEach(q =>
                                            {
                                                if (string.IsNullOrEmpty(bank))
                                                {
                                                    bank = q.Bank.Name;
                                                }
                                                else
                                                {
                                                    bank += ", " + q.Bank.Name;
                                                }
                                            });
                                        }
                                    });

                                }


                                row.Cells[0].AddParagraph(
                                    bank + " " +
                                    "is/are responsible for the banking transaction in respect of this importation.");


                                row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

                                row = table.AddRow();
                                row = table.AddRow();

                                row = table.AddRow();

                                row.Cells[0].AddParagraph(
                                    "We also certify that the product delivered met specifications stipulated under the guidelines and was therefore accepted.");

                                row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

                                row = table.AddRow();
                                row = table.AddRow();

                                //add another table

                                var table2 = section.AddTable();

                                //table = section.AddTable();
                                table2.Style = "Table";

                                table2.Borders.Width = 0.25;
                                table2.Borders.Left.Width = 0.5;
                                table2.Borders.Right.Width = 0.5;
                                table2.Rows.LeftIndent = 0;

                                Column column2 = table2.AddColumn("2cm");
                                column2.Format.Alignment = ParagraphAlignment.Center;


                                column2 = table2.AddColumn("8cm");
                                column2.Format.Alignment = ParagraphAlignment.Center;

                                column2 = table2.AddColumn("6cm");
                                column2.Format.Alignment = ParagraphAlignment.Center;




                                // Create the header of the table
                                Row row2 = table2.AddRow();
                                //row = table.AddRow();
                                row2.HeadingFormat = true;
                                row2.Format.Alignment = ParagraphAlignment.Center;
                                row2.Format.Font.Bold = true;
                                row2.Format.Font.Size = 12;

                                row2.Cells[0].AddParagraph("S/A:");
                                row2.Cells[0].Format.Alignment = ParagraphAlignment.Left;


                                row2.Cells[1].AddParagraph("COMPANY'S FACILITY:");
                                row2.Cells[1].Format.Alignment = ParagraphAlignment.Left;

                                row2.Cells[2].AddParagraph("QTY. DISCHARGED IN M/T:");
                                row2.Cells[2].Format.Alignment = ParagraphAlignment.Left;




                                double? total = 0;

                                var i = 1;

                                foreach (var item in items)
                                {

                                    row2 = table2.AddRow();
                                    row2.Format.Font.Bold = true;

                                    row2.Cells[0].AddParagraph(i.ToString());
                                    row2.Cells[0].Format.Alignment = ParagraphAlignment.Left;
                                    row2.Cells[1].AddParagraph(item.DepotName);
                                    row2.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                                    row2.Cells[2].AddParagraph(item.QuantityDischargedStr);
                                    row2.Cells[2].Format.Alignment = ParagraphAlignment.Left;

                                    total = total + item.QuantityDischarged;

                                    i = i + 1;
                                }

                                row2 = table2.AddRow();
                                row2.Format.Font.Bold = true;
                                row2.Cells[1].AddParagraph("Total");
                                row2.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                                row2.Cells[2].AddParagraph(Convert.ToString(total));
                                row2.Cells[2].Format.Alignment = ParagraphAlignment.Left;



                                const bool unicode = false;
                                const PdfFontEmbedding embedding = PdfFontEmbedding.Always;

                                PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);

                                // Associate the MigraDoc document with a renderer

                                pdfRenderer.Document = doc;



                                // Layout and render document to PDF

                                pdfRenderer.RenderDocument();



                                var pathtable = Path.Combine(Server.MapPath("~/PermDoc"), "recertTable.pdf");

                                pdfRenderer.PdfDocument.Save(pathtable);


                                XImage imagetable =
                                    XImage.FromFile(Path.Combine(Server.MapPath("~/PermDoc"), "recertTable.pdf"));

                                //gfx.DrawImage(imagetable, 0, 210);
                                gfx.DrawImage(imagetable, 20, 400, 550, 500);


                                var sign =
                                    XImage.FromFile(Path.Combine(Server.MapPath("~/PermDoc"), "director.png"));

                                //gfx.DrawImage(imagetable, 0, 210);
                                gfx.DrawImage(sign, 30, 705, 35, 37);


                                string path2 = Path.Combine(Server.MapPath("~/PermDoc"),
                                    "recert" + radno + ".pdf");


                                pdf.Save(path2);

                                var way = @"\PermDoc\\" + "recert" + radno + ".pdf";

                                return way;
                                    
                                
                            }
                            return "No";
                        }
                        return "No";
                    }
                    return "No";
                }
               
            }
            

            catch (Exception ex)
            {
                return "No";
            }


        }



     

      
    }
}

