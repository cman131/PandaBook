using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PandaBook.Models;

namespace PandaBook.Controllers
{
    public class CalendarController : Controller
    {
        private CalendarDBContext db = new CalendarDBContext();

        //
        // GET: /Calendar/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Calendar/Details/5

        public ActionResult Details(int id = 0)
        {
            Calendar calendar = db.Calendars.Find(id);
            if (calendar == null)
            {
                return HttpNotFound();
            }
            return View(calendar);
        }

        //
        // GET: /Calendar/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Calendar/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Calendar calendar)
        {
            if (ModelState.IsValid)
            {
                db.Calendars.Add(calendar);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(calendar);
        }

        //
        // GET: /Calendar/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Calendar calendar = db.Calendars.Find(id);
            if (calendar == null)
            {
                return HttpNotFound();
            }
            return View(calendar);
        }

        //
        // POST: /Calendar/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Calendar calendar)
        {
            if (ModelState.IsValid)
            {
                db.Entry(calendar).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(calendar);
        }

        //
        // GET: /Calendar/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Calendar calendar = db.Calendars.Find(id);
            if (calendar == null)
            {
                return HttpNotFound();
            }
            return View(calendar);
        }

        //
        // POST: /Calendar/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Calendar calendar = db.Calendars.Find(id);
            db.Calendars.Remove(calendar);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}