using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Controllers.Setups
{
    public class ApplicationIssuesController : Controller
    {
        private ImportPermitEntities db = new ImportPermitEntities();

        // GET: /ApplicationIssues/
        public ActionResult Index()
        {
            var applicationissues = db.ApplicationIssues.Include(a => a.Application).Include(a => a.IssueType);
            return View(applicationissues.ToList());
        }

        // GET: /ApplicationIssues/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationIssue applicationissue = db.ApplicationIssues.Find(id);
            if (applicationissue == null)
            {
                return HttpNotFound();
            }
            return View(applicationissue);
        }

        // GET: /ApplicationIssues/Create
        public ActionResult Create()
        {
            ViewBag.ApplicationId = new SelectList(db.Applications, "ApplicationId", "ReferenceCode");
            ViewBag.IssueTypeId = new SelectList(db.IssueTypes, "Id", "Description");
            return View();
        }

        // POST: /ApplicationIssues/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Id,IssueTypeId,ApplicationId,Description,Status,IssueDate,AmendedDate")] ApplicationIssue applicationissue)
        {
            if (ModelState.IsValid)
            {
                db.ApplicationIssues.Add(applicationissue);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ApplicationId = new SelectList(db.Applications, "ApplicationId", "ReferenceCode", applicationissue.ApplicationId);
            ViewBag.IssueTypeId = new SelectList(db.IssueTypes, "Id", "Description", applicationissue.IssueTypeId);
            return View(applicationissue);
        }

        // GET: /ApplicationIssues/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationIssue applicationissue = db.ApplicationIssues.Find(id);
            if (applicationissue == null)
            {
                return HttpNotFound();
            }
            ViewBag.ApplicationId = new SelectList(db.Applications, "ApplicationId", "ReferenceCode", applicationissue.ApplicationId);
            ViewBag.IssueTypeId = new SelectList(db.IssueTypes, "Id", "Description", applicationissue.IssueTypeId);
            return View(applicationissue);
        }

        // POST: /ApplicationIssues/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Id,IssueTypeId,ApplicationId,Description,Status,IssueDate,AmendedDate")] ApplicationIssue applicationissue)
        {
            if (ModelState.IsValid)
            {
                db.Entry(applicationissue).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ApplicationId = new SelectList(db.Applications, "ApplicationId", "ReferenceCode", applicationissue.ApplicationId);
            ViewBag.IssueTypeId = new SelectList(db.IssueTypes, "Id", "Description", applicationissue.IssueTypeId);
            return View(applicationissue);
        }

        // GET: /ApplicationIssues/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationIssue applicationissue = db.ApplicationIssues.Find(id);
            if (applicationissue == null)
            {
                return HttpNotFound();
            }
            return View(applicationissue);
        }

        // POST: /ApplicationIssues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ApplicationIssue applicationissue = db.ApplicationIssues.Find(id);
            db.ApplicationIssues.Remove(applicationissue);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
