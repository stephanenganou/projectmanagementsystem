using PMSystem.DataAccess;
using PMSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Mvc;
using PMSystem.Utility;

namespace PMSystem.Controllers
{
    //It make sure to check for a valide user before allowing access to this action(s)
    [Authorize]
    public class ProjectController : Controller
    {
        private static string ADMIN_FEATURE = "admin@ymail.com";
        private PMSystemDbContext context;

        //um den angemeldeten User zu bekommen
        private User getCurrentUser()
        {
            return context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
        }

        public ProjectController()
        {
            this.context = new PMSystemDbContext();
        }

        // GET: Project
        public ActionResult Index()
        {
            //List<Project> projects;
            User user = getCurrentUser();
            ViewBag.Currentuser = User.Identity.Name;
            ViewBag.UserLevel = user.Level.ToString();


            if (null != user)
                {
                    ViewBag.Projects = user.AssignedProjects;
                    List<Project> allProjects = new List<Project>();
                    foreach (Project project in this.context.Projects)
                    {
                        if (!(allProjects.Contains(project)))
                        {
                           allProjects.Add(project);
                        }
                    }
                    ViewBag.Active_Projects = allProjects;
                    ViewBag.Subtasks = this.context.SubTasks.ToList();
                    return View(user.AssignedProjects);
                }
            
            Console.WriteLine("No authentificated User found.");
            return View();
        }

        // GET: Project/Details/5
        public ActionResult Details(string p_ID)
        {
            
            ViewBag.Currentuser = User.Identity.Name;
            ViewBag.ProjectOwner = null;

            if(!TextUtil.checkIfEmpty(p_ID))
            {
                int id = int.Parse(p_ID);

                Project matchedProject = null;
                
                foreach (Project project in this.context.Projects)
                {
                    if(project.Id == id)
                    {
                        matchedProject = project;
                        ViewBag.ProjectOwner = project.Owner;
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

            ViewBag.Users = this.context.Users.ToList();
            User currentUser = getCurrentUser();
            List<SelectListItem> usersDropdown = new List<SelectListItem>();

            // Only Admin can select other users an Project owner
            if (ADMIN_FEATURE == currentUser.Email)
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
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                string p_ID = "";

                // TODO: Add insert logic here
                if (ModelState.IsValid)
                {
                    User currentUser = getCurrentUser();

                    //nur admin and project owners
                    if (currentUser.Email == ADMIN_FEATURE || currentUser.Level >= 2)
                    {
                        Project project = new Project();
                        project.Name = collection["Name"];
                        project.Description = collection["Description"];
                        
                        if (TextUtil.checkIfEmpty(collection["Owner"]))
                        {
                            project.Owner = currentUser;
                            project.AssignedUsers.Add(currentUser);
                            /*
                             * if(currentUser.Id < 2)
                            {
                                currentUser.Level = 2;
                                context.Users.AddOrUpdate(currentUser);
                            }
                            */
                        }
                        else
                        {
                            int temp_ID = int.Parse(collection["Owner"]);
                            User p_User = this.context.Users.FirstOrDefault(u => u.Id == temp_ID);
                            project.Owner = p_User;
                            project.AssignedUsers.Add(p_User);
                        }
                        project.Status = float.Parse(collection["Status"]);
                        this.context.Projects.Add(project);
                        this.context.SaveChanges();
                        p_ID = "" + project.Id;
                        return RedirectToAction("Create", "Task", new { p_ID = p_ID });
                    }
                    else
                    {
                        ViewBag.Errormessage = "You do not have permission to create this project";
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
                ViewBag.Errormessage = "An error has occurred";
                return View();
            }
        }

        // GET: Project/Edit/5
        public ActionResult Edit(string p_ID)
        {
            ViewBag.Currentuser = User.Identity.Name;
                        
            if (!TextUtil.checkIfEmpty(p_ID))
            {
                int id = int.Parse(p_ID);
                Project project = (from p in this.context.Projects where p.Id == id select p).FirstOrDefault();
                if(null != project)
                {
                    //ViewBag.Tasks = context.Tasks.Where(t => t.Project.Id == project.Id).ToList();
                    ViewBag.Users = (from us in this.context.Users select us).ToList();
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

            ViewBag.Errormessage = "No ID was entered";
            return View();
        }

        // POST: Project/Edit/5
        [HttpPost]
        public ActionResult Edit(string p_ID, FormCollection collection)
        {
            try
            {
               
                if(!TextUtil.checkIfEmpty(p_ID))
                {
                    if (ModelState.IsValid)
                    {
                        int id = int.Parse(p_ID);
                        User currentUser = getCurrentUser();
                        Project project = (from p in this.context.Projects where p.Id == id select p).FirstOrDefault();

                        // Only projects owner or admin can edit projects
                        if (null != project && (ADMIN_FEATURE == currentUser.Email || currentUser.Id == project.Owner.Id))
                        {
                            project.Name = collection["Name"];
                            project.Description = collection["Description"];

                            if (!TextUtil.checkIfEmpty(collection["Owner"]))
                            {
                                int temp_ID = int.Parse(collection["Owner"]);
                                User p_User = this.context.Users.FirstOrDefault(u => u.Id == temp_ID);
                                
                                // Reset Owner only if different
                                if(project.Owner.Id != p_User.Id)
                                {
                                    project.AssignedUsers.Remove(project.Owner);
                                    project.Owner = p_User;
                                    project.AssignedUsers.Add(p_User);

                                    if (ADMIN_FEATURE != p_User.Email && p_User.Level < 2)
                                    {
                                        p_User.Level = 2;
                                        this.context.Users.AddOrUpdate(p_User);
                                    }
                                }

                            }
                                                        
                            project.Status = float.Parse(collection["Status"]);

                            // We save all the changes in the Model Project;
                            this.context.Projects.AddOrUpdate(project);
                            this.context.SaveChanges();

                            return RedirectToAction("Index", "Project");
                        }
                        else
                        {
                            ViewBag.Errormessage = "You cannot edit this project!";
                            return View();
                        }
                        
                    }
                    
                    return View();
                }

                ViewBag.Errormessage = "The project ID has not been defined!";
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
            
            var referer = Request.UrlReferrer;
                        
            if (!TextUtil.checkIfEmpty(p_ID))
            {
                int id = int.Parse(p_ID);
                User currentUser = getCurrentUser();
                Project project = (from p in context.Projects where p.Id == id select p).FirstOrDefault();
                
                // Only Admin or Project Owner can delete a specific project.
                if(null != project && (ADMIN_FEATURE == currentUser.Email || currentUser.Id == project.Owner.Id))
                {
                    // Deleting all tasks (and subtasks) in Project
                    if (project.Tasks.Count() >= 1)
                    {
                        deleteProjectContain(project);
                    }
                    this.context.Projects.Remove(project);
                                       
                    this.context.SaveChanges();
                }
                else
                {
                    ViewBag.Errormessage = "You have no right to delete the project";
                }
            }
                        
            return RedirectToAction("index");
        }

        private void deleteProjectContain(Project project)
        {
            foreach (Task task in project.Tasks.ToList())
            {
                if (task.SubTasks.Count() >= 1)
                {
                    foreach (SubTask subTask in task.SubTasks.ToList())
                    {
                        this.context.SubTasks.Remove(context.SubTasks.FirstOrDefault(s => s.Id == subTask.Id));
                    }
                }

                this.context.Tasks.Remove(this.context.Tasks.FirstOrDefault(t => t.Id == task.Id));
            }
        }

    }
}
