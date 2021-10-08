﻿using PMSystem.DataAccess;
using PMSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PMSystem.Utility;
using WebGrease.Css.Ast.Selectors;

namespace PMSystem.Controllers
{
    [Authorize] //it tells, checks for a valide user before allowing access to this action(s)
    public class UserController : Controller
    {
        private static string ADMIN_FEATURE = "admin@ymail.com";
        private PMSystemDbContext context;
        public UserController()
        {
            context = new PMSystemDbContext();
        }
        public User getCurrentUser()
        {
            return context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
        }
        // GET: User/index page
        public ActionResult Index()
        {
            User currentuser = getCurrentUser();
            //ViewBag.Currentuser = currentuser;
            ViewBag.Currentuser = currentuser.Email;
            return View(currentuser);   
        }

        // GET: Admin
        public ActionResult Admin()
        {
            ViewBag.Currentuser = User.Identity.Name;

            if (getCurrentUser().Email != ADMIN_FEATURE)
                return RedirectToAction("Index", "User");

            var users = (from us in context.Users select us);
            //ViewBag.Projects = (from ps in context.Projects select ps).ToList();

            return View(users);
        }

        //GET: Logout
        public ActionResult Logout()
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect(FormsAuthentication.LoginUrl);
            else
            {
                FormsAuthentication.SignOut();
                Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
                Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
                HttpCookie cookie = HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (cookie != null)
                {
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(cookie);
                }

                return RedirectToAction("Index", "Home");
            }
            
        }

        // GET: User/Details/5
        public ActionResult Details(string u_ID)
        {
            ViewBag.Currentuser = User.Identity.Name;

            if (null != u_ID || u_ID != "")
            {
                var currentUser = getCurrentUser();
                int id = int.Parse(u_ID);
                if(currentUser.Id == id || currentUser.Email == ADMIN_FEATURE)
                {
                    if(currentUser.Email == ADMIN_FEATURE)
                    {
                        var user = context.Users.FirstOrDefault(u => u.Id == id);
                        if(null != user)
                        {
                            return View(currentUser);
                        }
                        else
                        {
                            ViewBag.Errormessage = "Kein User gefunden.";
                        }
                    }
                    return View(currentUser);
                }
                else
                {
                    ViewBag.Errormessage = "Sie haben keine Berechtigung";
                }

            }
            else
            {
                ViewBag.Errormessage = "Ungültige ID Nummer.";
            }

            return View();
        }

        // GET: User/Create
        public ActionResult Create()
        {
            ViewBag.Currentuser = User.Identity.Name;

            if (getCurrentUser().Email == ADMIN_FEATURE)
            {
                return View();
            }

            return RedirectToAction("Index", "User");
        }

        // POST: User/Create
        [HttpPost]
        public ActionResult Create(User user)
        {
            try
            {
                if(getCurrentUser().Email == ADMIN_FEATURE)
                {
                    if (ModelState.IsValid)
                    {
                        User oldUser = context.Users.FirstOrDefault(u => u.Email == user.Email);
                        if (oldUser != null)
                        {
                            ViewBag.ErrorMessage = user.Email + " exists already in the system";
                            return View(user);
                        }

                        context.Users.Add(user);
                        context.SaveChanges();

                        return RedirectToAction("Admin");
                    }

                    return View(user);
                }
                return RedirectToAction("Index", "User");
            }
            catch
            {
                return View();
            }
        }

        // GET: User/Edit/5
        public ActionResult Edit(string u_ID)
        {
            ViewBag.Currentuser = User.Identity.Name;
            User editingUser = getCurrentUser();

            if (u_ID != null || u_ID == "")
            {
                var user = getCurrentUser();

                if (user.Email == ADMIN_FEATURE || user.Id == int.Parse(u_ID))
                {
                    int id = int.Parse(u_ID);
                    var s_User = context.Users.FirstOrDefault(u => u.Id == id);
                    if (s_User != null) return View(s_User);
                    ViewBag.Errormessage = "Der Benutzer mit der ID '" + u_ID + "' wurde nicht gefunden.";
                    return View();
                }
                else
                {
                    User adminInfo = DataAccessUtil.getAdminInfoByEmail(context, ADMIN_FEATURE);
                    if (editingUser.Email == ADMIN_FEATURE && !(u_ID.Equals(adminInfo.Email)))
                    {
                        return RedirectToAction("Admin", "User");
                    }
                    else
                    {
                        return RedirectToAction("Index", "User");
                    }
                }

            }
            else
            {
                ViewBag.Errormessage = "Die ID der User ist ungültig";
                return View();
            }
        }

        // POST: User/Edit/5
        [HttpPost]
        public ActionResult Edit(string u_ID, FormCollection collection)
        {
            try
            {
                
                if (null != u_ID || TextUtil.checkIfEmpty(u_ID))
                {
                    var user = getCurrentUser();
                    bool isAdmin = false;
                    int id = int.Parse(u_ID);

                    if (user.Email != ADMIN_FEATURE && user.Id != id)
                    {
                        ViewBag.Errormessage = "Sie haben keine Berechtigung";
                        return RedirectToAction("Index", "User");
                    }

                    if (user.Email == ADMIN_FEATURE)
                    {
                        user = context.Users.FirstOrDefault(u => u.Id == id);
                        isAdmin = true;
                    }

                    if(isAdmin || user.Id == id)
                    {
                        user.Vorname = collection["Vorname"];
                        user.Nachname = collection["Nachname"];
                        user.Passwort = collection["Passwort"];
                        if (isAdmin) user.Level = int.Parse(collection["Level"]);
                        user.Email = collection["Email"];
                        user.Address = collection["Address"];
                        user.PLZ = collection["PLZ"];
                        user.Bundesland = collection["Bundesland"];
                        user.Land = collection["Land"];
                        user.Postfach = collection["Postfach"];
                        user.Telefonnummer = collection["Telefonnummer"];
                        user.Faxnummer = collection["Faxnummer"];

                        context.Users.AddOrUpdate(user);
                        context.SaveChanges();
                        if(isAdmin)
                        {
                            return RedirectToAction("Admin");
                        }
                        else
                        {
                            return RedirectToAction("Index");
                        }
                    }
                    else {
                        ViewBag.Errormessage = "Sie haben keine Berechtigung";
                        return View(collection);
                    }
                    
                }
                ViewBag.Errormessage = "Die ID der User ist ungültigt";
                return View(collection);
            }
            catch
            {
                ViewBag.Errormessage = "Ein Fehler ist aufgetreten";
                return View();
            }
        }

        // GET: User/Delete/5
        public ActionResult Delete(string u_ID)
        {
            if(null != u_ID || u_ID != "")
            {
                User currentUser = getCurrentUser();
                User adminInfo = DataAccessUtil.getAdminInfoByEmail(context, ADMIN_FEATURE);

                int id = int.Parse(u_ID);
                if(id != adminInfo.Id && currentUser.Id == adminInfo.Id)
                {
                    User userToRemove = (from u in context.Users where u.Id == id select u).FirstOrDefault();
                    if(null != userToRemove)
                    {
                        var projects = (from ps in context.Projects where ps.Owner.Id == userToRemove.Id select ps).ToList();

                        foreach(var data in projects)
                        {
                            Project pro = context.Projects.FirstOrDefault(p => p.Id == data.Id);
                            //Der neuer Owner wird der Admin sein
                            pro.Owner = currentUser;
                            context.Projects.AddOrUpdate(pro);
                        }

                        context.Users.Remove(userToRemove);
                        context.SaveChanges();
                    }
                }
                else
                {
                    ViewBag.Errormessage = "Der Admin can nicht manuell gelöscht oder Sie haben nicht die Berechtigung.";
                }
            }
            else
            {
                ViewBag.Errormessage = "Id '" + u_ID + "' wurde nicht angegeben.";
            }

            return RedirectToAction("Admin", "User");

        }

        // POST: User/Delete/5 || Now in use now
        /*
         [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        */
    }
}
