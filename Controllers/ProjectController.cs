using PMSystem.DataAccess;
using PMSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PMSystem.Controllers
{
    //It make sure to check for a valide user before allowing access to this action(s)
    [Authorize]
    public class ProjectController : Controller
    {
        PMSystemDbContext context;

        public ProjectController()
        {
            this.context = new PMSystemDbContext();
        }

        //um den angemeldeten User zu bekommen
        public User getCurrentUser()
        {
            return context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
        }
        // GET: Project
        public ActionResult Index()
        {
            //List<Project> projects;
            var user = getCurrentUser();
            ViewBag.Currentuser = User.Identity.Name;
            ViewBag.UserLevel = user.Level.ToString();


            if (null != user)
                {
                    ViewBag.Projects = user.AssignedProjects;
                    List<Project> allProjects = new List<Project>();
                    foreach (Project project in context.Projects)
                    {
                        if (!(allProjects.Contains(project)))
                        {
                           allProjects.Add(project);
                        }
                    }
                    ViewBag.Users = allProjects;
                    ViewBag.Subtasks = context.SubTasks.ToList();
                    return View(user.AssignedProjects);
                }
            
            Console.WriteLine("No authentificated User found.");
            return View();
        }

        // GET: Project/Details/5
        public ActionResult Details(string p_ID)
        {
            //List<Project> projects;
            ViewBag.Currentuser = User.Identity.Name;

            if (!(String.IsNullOrEmpty(p_ID)))
            {
                int id = int.Parse(p_ID);

                Project matchedProject = null;
                
                foreach (Project project in context.Projects)
                {
                    if(project.Id == id)
                    {
                        matchedProject = project;
                    }
                }
                if (null != matchedProject)
                {
                    return View(matchedProject);
                }
                else
                {
                    ViewBag.Errormessage = "The project with ID: " + p_ID + "was not found";
                    return View();
                }
            }

            ViewBag.Errormessage = "No ID was entered";
            return View();

        }

        // GET: Project/Create
        public ActionResult Create()
        {
            ViewBag.Currentuser = User.Identity.Name;

            ViewBag.Users = context.Users.ToList();
            User currentUser = getCurrentUser();
            List<SelectListItem> usersDropdown = new List<SelectListItem>();
            if (currentUser.Level == 3)
            {
                foreach (User user in ViewBag.Users)
                {
                    usersDropdown.Add(new SelectListItem { Text = user.Vorname + " " + user.Nachname, Value = user.Id.ToString() });
                }
            }
            else
            {
                usersDropdown.Add(new SelectListItem { Text = currentUser.Vorname + " " + currentUser.Nachname, Value = currentUser.Id.ToString() });
            }
            ViewBag.UsersSelectList = usersDropdown;
            return View();
        }

        // POST: Project/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection) //FormCollection collection
        {
            try
            {
                var p_ID = "";

                // TODO: Add insert logic here
                if (ModelState.IsValid)
                {
                    var currentUser = getCurrentUser(); //collection["UserId"]); "admin@ymail.com"

                    if(currentUser.Id == 1 || currentUser.Level >= 2) //nur admin
                    {
                        Project project = new Project();
                        project.Name = collection["Name"];
                        project.Description = collection["Description"];
                        if (String.IsNullOrEmpty(collection["Owner"]))
                        {
                            project.Owner = currentUser;
                            project.AssignedUsers.Add(currentUser);
                            if(currentUser.Id < 2)
                            {
                                currentUser.Level = 2;
                                context.Users.AddOrUpdate(currentUser);
                            }
                        }
                        else
                        {
                            var temp_ID = int.Parse(collection["Owner"]);
                            User p_User = context.Users.FirstOrDefault(u => u.Id == temp_ID);
                            project.Owner = p_User;
                            project.AssignedUsers.Add(p_User);
                            if(p_User.Id < 2)
                            {
                                p_User.Level = 2;
                                context.Users.AddOrUpdate(p_User);
                            }
                        }
                        project.Status = float.Parse(collection["Status"]);
                        context.Projects.Add(project);
                        context.SaveChanges();
                        p_ID = "" + project.Id;
                        return RedirectToAction("Create", "Task", new { p_ID = p_ID });
                    }
                    else
                    {
                        ViewBag.Errormessage = "Sie haben keine Berechtigung dieses Projektes zu erstellen";
                        return View();
                    }
                }
                else
                {
                    return View();
                }
            }
            catch
            {
                ViewBag.Errormessage = "Ein Fehler ist aufgetretten";
                return View();
            }
        }

        // GET: Project/Edit/5
        public ActionResult Edit(string p_ID)
        {
            ViewBag.Currentuser = User.Identity.Name;

            if (!(String.IsNullOrEmpty(p_ID)))
            {
                int id = int.Parse(p_ID);
                var project = (from p in context.Projects where p.Id == id select p).FirstOrDefault();
                if(project != null)
                {
                    //ViewBag.Tasks = context.Tasks.Where(t => t.Project.Id == project.Id).ToList();
                    ViewBag.Users = (from us in context.Users select us).ToList();
                    List<SelectListItem> usersDropdown = new List<SelectListItem>();
                    foreach (User user in ViewBag.Users)
                    {
                        usersDropdown.Add(new SelectListItem { Text = user.Vorname + " " + user.Nachname, Value = user.Id.ToString() });
                    }
                    ViewBag.UsersSelectList = usersDropdown;
                    return View(project);
                }

                return View();
            }

            ViewBag.Errormessage = "Kein ID wurde eingegeben";
            return View();
        }

        // POST: Project/Edit/5
        [HttpPost]
        public ActionResult Edit(string p_ID, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
                if(!(String.IsNullOrEmpty(p_ID)))
                {
                    if (ModelState.IsValid)
                    {
                        int id = int.Parse(p_ID);
                        var currentUser = getCurrentUser(); //collection["UserId"]); "admin@ymail.com"
                        var project = (from p in context.Projects where p.Id == id select p).FirstOrDefault();

                        //Nur Projekte Owner oder Admin kann projekte editieren
                        if(project != null && (currentUser.Id == 1 || currentUser.Id == project.Owner.Id))
                        {
                            project.Name = collection["Name"];
                            project.Description = collection["Description"];
                            //Should be tested later with changing Ownership
                            if (String.IsNullOrEmpty(collection["Owner"]))
                            {
                                //Do Nothing
                            }
                            else
                            {
                                var temp_ID = int.Parse(collection["Owner"]);
                                User p_User = context.Users.FirstOrDefault(u => u.Id == temp_ID);
                                
                                if(project.Owner.Id != p_User.Id)
                                {
                                    project.AssignedUsers.Remove(project.Owner);
                                    project.Owner = p_User;
                                    project.AssignedUsers.Add(p_User);
                                }

                                if (p_User.Id != 1 && project.Owner.Id != p_User.Id)
                                {
                                    p_User.Level = 2;
                                    context.Users.AddOrUpdate(p_User);
                                }


                            }

                            //This also
                            //project.AssignedUsers.Add(pOwner);
                            project.Status = float.Parse(collection["Status"]);

                            //UpdateModel(project);
                            context.Projects.AddOrUpdate(project);
                            context.SaveChanges();

                            return RedirectToAction("Index", "Project");
                        }
                        else
                        {
                            ViewBag.Errormessage = "Sie können dieses Projekt nicht bearbeiten.";
                            return View();
                        }
                        
                    }
                    
                    return View();
                }

                ViewBag.Errormessage = "Die project ID wurde nicht definiert";
                return View();

            }
            catch
            {
                return View();
            }
        }

        // GET: Project/Delete/5
        public ActionResult Delete(string p_ID)
        {
            // What to do with this shit?
            var referer = Request.UrlReferrer;

            if (!(String.IsNullOrEmpty(p_ID)))
            {
                int id = int.Parse(p_ID);
                User currentUser = getCurrentUser();
                var project = (from p in context.Projects where p.Id == id select p).FirstOrDefault();
                
                //Admin kann Projekte  löschen
                if(project != null && (currentUser.Id == 1 || currentUser.Id == project.Owner.Id))
                {
                    //Löschen von alle Tasks (und Subtasks) in Project
                    if(project.Tasks.Count() >= 1)
                    {
                        foreach (Task task in project.Tasks.ToList())
                        {
                            if(task.SubTasks.Count() >= 1)
                            {
                                foreach (SubTask subTask in task.SubTasks.ToList())
                                {
                                    context.SubTasks.Remove(context.SubTasks.FirstOrDefault(s => s.Id == subTask.Id));
                                }
                            }

                            context.Tasks.Remove(context.Tasks.FirstOrDefault(t => t.Id == task.Id));
                        }
                    }
                    context.Projects.Remove(project);

                    //Löschen von alle Tasks in Project
                    context.SaveChanges();
                }
                else
                {
                    ViewBag.Errormessage = "Sie haben kein Recht, das Projekt zu löschen";
                }
            }
            //How to redirect to coming Link || it looks to work
            return RedirectToAction("index");
        }

    }
}
