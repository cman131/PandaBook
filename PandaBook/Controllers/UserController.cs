﻿

namespace PandaBook.Controllers
{using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PandaBook.Models;
    public class UserController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /User/

        public ActionResult Index()
        {
            return View(db.UserProfiles.ToList());
        }

        
        //
        // GET: /User/Details/5

        public ActionResult Details(int id = 0)
        {
            UserProfile userprofile = db.UserProfiles.Find(id);
            if (userprofile == null)
            {
                return HttpNotFound();
            }
            return View(userprofile);
        }

        /**
         * Should Eventually be swapped for getting the current user
         * 
         * @author Conor Wright
         * @return int - the current users id
         */
        public static int getCurrentUser()
        {
            return 0;
        }

        /**
         * Should eventually be swapped for getting the current user's favorite stocks
         * 
         * @author Conor Wright
         * @return String[] - the resulting array of favorite stock codes
         */
        public static String[] getFavoriteStocks(){
            return new String[]{ "GOOG", "AAPL", "MSFT", "A", "YHOO" };
        }

        /**
         * Should eventually be swapped for getting the current user's owned stocks
         * 
         * @author Conor Wright
         * @return String[] - the resulting array of owned stock codes
         */
        public static String[] getOwnedStocks()
        {
            return new String[] { "GOOG", "AAPL", "MSFT", "A", "YHOO" };
        }

        //
        // GET: /User/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /User/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserProfile userprofile)
        {
            if (ModelState.IsValid)
            {
                db.UserProfiles.Add(userprofile);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(userprofile);
        }

        //
        // GET: /User/Edit/5

        public ActionResult Edit(int id = 0)
        {
            UserProfile userprofile = db.UserProfiles.Find(id);
            if (userprofile == null)
            {
                return HttpNotFound();
            }
            return View(userprofile);
        }

        //
        // POST: /User/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserProfile userprofile)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userprofile).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(userprofile);
        }

        //
        // GET: /User/Delete/5

        public ActionResult Delete(int id = 0)
        {
            UserProfile userprofile = db.UserProfiles.Find(id);
            if (userprofile == null)
            {
                return HttpNotFound();
            }
            return View(userprofile);
        }

        //
        // POST: /User/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserProfile userprofile = db.UserProfiles.Find(id);
            db.UserProfiles.Remove(userprofile);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}