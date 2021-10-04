using PMSystem.DataAccess;
using PMSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace PMSystem.Controllers
{
    [Authorize] //it tells, checks for a valide user before allowing access to this action(s)
    public class TaskController : Controller
    {
        PMSystemDbContext context;

        public TaskController()
        {
            this.context = new PMSystemDbContext();
        }

        //um den angemeldeten User zu bekommen
        public User getCurrentUser()
        {
            return context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
        }
        // GET: Task
        public ActionResult Index()
        {
            User currentUser = getCurrentUser();
            ViewBag.Tasks = context.SubTasks.ToList();
            ViewBag.Currentuser = currentUser.Email;
            return View(currentUser.AssignedTasks);
        }

        // GET: Task/Details/5
        public ActionResult Details(string p_ID, string t_ID)
        {
            ViewBag.Currentuser = User.Identity.Name;

            if (!(String.IsNullOrEmpty(t_ID)))
            {
                int id = int.Parse(t_ID);
                Task task = context.Tasks.FirstOrDefault(t => t.Id == id);
                if (task != null)
                {
                    //ViewBag.Tasks = from projects in context.Projects select projects.Id == id;
                    //ViewBag.SubTasks = context.SubTasks.Where(s => s.Task.Id == task.Id).ToList();
                    List<Project> allProjects = new List<Project>();
                    foreach (User user_iterator in context.Users)
                    {
                        allProjects.AddRange(user_iterator.AssignedProjects);
                    }
                    List<Task> allTasks = new List<Task>();
                    foreach (Project project_iterator in allProjects)
                    {
                        allTasks.AddRange(project_iterator.Tasks);
                    }
                    List<SubTask> allSubtasks = new List<SubTask>();
                    foreach (Task task_iterator in allTasks)
                    {
                        if(task_iterator.Id == id)
                        {
                            foreach (SubTask subtask_iterator in task_iterator.SubTasks)
                            {
                                if (!(allSubtasks.Contains(subtask_iterator)))
                                {
                                    allSubtasks.Add(subtask_iterator);
                                }
                            }
                        }
                    }
                    ViewBag.Subtasks = allSubtasks;
                    ViewBag.ProjectID = p_ID;
                    return View(task);
                }
                else
                {
                    ViewBag.Errormessage = "Task mit der ID: '" + t_ID + "' wurde nicht gefunden";
                }
            }

            return View();
        }

        // GET: Task/Create
        public ActionResult Create(string p_ID)
        {
            if(p_ID != "") ViewBag.ProjectID = p_ID;
            ViewBag.Currentuser = User.Identity.Name;
            ViewBag.Users = (from users in context.Users select users).ToList();
            ViewBag.ProjectID = p_ID;
            return View();
        }

        // POST: Task/Create/p_ID
        [HttpPost]
        public ActionResult Create(string p_ID, FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here
                int projectID = int.Parse(p_ID);
                if(projectID == 0 && collection["ProjektID"] == "")
                {
                    ViewBag.Errormessage = "Unkorrekt Projekt ID";
                    return View(p_ID, collection);
                }

                if (ModelState.IsValid)
                {
                    Project project = new Project();
                    List<Project> allProjects = new List<Project>();
                    foreach (User user_iterator in context.Users)
                    {
                        allProjects.AddRange(user_iterator.AssignedProjects);
                    }
                    foreach (Project project_iterator in allProjects)
                    {
                        if(project_iterator.Id == projectID)
                        {
                            project = project_iterator;
                        }
                    }
                    User currentUser = getCurrentUser();

                    if(project.Owner.Id != currentUser.Id && currentUser.Id != 1)
                    {
                        ViewBag.Errormessage = "Sie haben keine Berechtigung!";
                        return View();
                    }

                    Task task = new Task();
                    task.Name = collection["Name"];
                    task.Description = collection["Description"];
                    task.Status = float.Parse(collection["Status"]);
                    task.Project = project;
                    //How do we do, to Assign new Users?

                    //Anzahl Tasks + 1(für die Neue Task)
                    int taskCounter = project.Tasks.Count() + 1;
                    Debug.WriteLine("number of Tasks in the Project= " + taskCounter);
                    if(project.Status != 0 || project.Status != 0.0)
                    {
                        float total_status = 0;
                        foreach(Task p_task in project.Tasks)
                        {
                            total_status = total_status + p_task.Status;
                        }

                        total_status = total_status + float.Parse(collection["Status"]);
                        project.Status = total_status / taskCounter;

                        context.Projects.AddOrUpdate(project);
                    }

                    context.Tasks.Add(task);
                    context.SaveChanges();

                    string t_ID = "" + task.Id;
                    //bevor weiterzugehen, fragen, ob anderen Tasks hinzugefügt werden

                    return RedirectToAction("Create", "SubTask", new { p_ID = p_ID, t_ID = t_ID });
                }

                return View(collection);

            }
            catch
            {
                return View();
            }
        }

        // GET: Task/Edit/5
        public ActionResult Edit(string p_ID, string t_ID)
        {
            ViewBag.Currentuser = User.Identity.Name;

            if (!(String.IsNullOrEmpty(t_ID)))
            {
                int id = int.Parse(t_ID);
                Task task = (from t in context.Tasks where t.Id == id select t).FirstOrDefault();
                ViewBag.ProjectID = p_ID;
                ViewBag.Users = (from us in context.Users select us).ToList();

                return View(task);
            }

            ViewBag.Errormessage = "Der Task mit der ID '" + t_ID + "' existiert nicht.";
            return View();
        }

        // POST: Task/Edit/5
        [HttpPost]
        public ActionResult Edit(string p_ID, string t_ID, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                int id = int.Parse(t_ID);
                if (id == 0) //collection["User"]
                {
                    ViewBag.Errormessage = "Unkorrekt Task ID";
                    return View(collection);
                }

                if (ModelState.IsValid)
                {
                    Task task = context.Tasks.FirstOrDefault(t => t.Id == id);
                    if(task != null)
                    {
                        User currentUser = getCurrentUser();
                        //Nur Admin oder Projekt Owner kann die Task bearbeiten
                        if (currentUser.Id == 1 || currentUser.Id == task.Project.Owner.Id)
                        {
                            task.Name = collection["Name"];
                            task.Description = collection["Description"];
                            task.Status = float.Parse(collection["Status"]);
                            //Assign Users
                            //task.Project = project;
                            //How do we do, to Assign new Users?

                            /*Aktualisierung des 'Projekt' und 'Subtask' status.*/
                            //Anzahl Tasks
                            int projectID = 0;
                            var query =
                                from t in context.Tasks
                                where t.Id == task.Id
                                select new { projectID = t.Project.Id };

                            foreach (var val in query)
                            {
                                projectID = val.projectID;
                            }
                            ViewBag.ProjectID = projectID;
                            Project project = context.Projects.FirstOrDefault(p => p.Id == projectID);

                            int taskCounter = project.Tasks.Count();
                            float total_status = 0;
                            foreach (Task p_task in task.Project.Tasks)
                            {
                                if(task.Id != p_task.Id)
                                {
                                    total_status = total_status + p_task.Status;
                                }
                            }

                            total_status = total_status + float.Parse(collection["Status"]);
                            if(taskCounter > 0)
                            {
                                project.Status = total_status / taskCounter;
                            }
                            else
                            {
                                project.Status = 0;
                            }

                            //UpdateModel(task);
                            context.Tasks.AddOrUpdate(task);
                            context.Projects.AddOrUpdate(project);
                            context.SaveChanges();

                            //how to return to the previous origin page
                            return RedirectToAction("Details", "Task", new { t_ID = id });
                        }


                        ViewBag.Errormessage = "Sind haben kein Berechtigung, um diese Aktion durchzuführen.";
                        return View(collection);
                    }
                    else
                    {
                        ViewBag.Errormessage = "Kein Subtask mit der ID '" + t_ID + "' wurde gefunden.";
                    }

                }
                return View(collection);
            }
            catch
            {
                return View();
            }
        }

        // GET: Task/Delete/5
        public ActionResult Delete(string t_ID)
        {
            if (!(String.IsNullOrEmpty(t_ID)))
            {
                int id = int.Parse(t_ID);
                User currentUser = getCurrentUser();
                Task task = (from t in context.Tasks where t.Id == id select t).FirstOrDefault();
                if(task != null)
                {
                    int projectID = 0;
                    var query =
                        from t in context.Tasks
                        where t.Id == task.Id
                        select new { projectID = t.Project.Id };

                    foreach (var val in query)
                    {
                        projectID = val.projectID;
                    }

                    List<Project> allProjects = new List<Project>();
                    Project project = new Project();
                    foreach (User user_iterator in context.Users)
                    {
                        allProjects.AddRange(user_iterator.AssignedProjects);
                    }
                    foreach (Project project_iterator in allProjects)
                    {
                        foreach (Task task_iterator in project_iterator.Tasks)
                        {
                            if (task_iterator.Id == id)
                            {
                                project = project_iterator;
                            }
                        }
                    }

                    if (project.Owner.Id == currentUser.Id || currentUser.Id == 1)
                    {
                        //deleting all realated SubTasks
                        foreach(SubTask subTask in task.SubTasks)
                        {
                            context.SubTasks.Remove(subTask);
                        }

                        //Aktualisierung des 'Projekt'
                        int taskCounter = project.Tasks.Count() - 1;

                        float total_status = 0;
                        foreach (Task p_task in task.Project.Tasks)
                        {
                            if (task.Id != p_task.Id)
                            {
                                total_status = total_status + p_task.Status;
                            }
                        }

                        if(taskCounter > 0)
                        {
                            project.Status = total_status / taskCounter;
                        }
                        else
                        {
                            project.Status = 0;
                        }

                        context.Tasks.Remove(task);
                        context.Projects.AddOrUpdate(project);
                        context.SaveChanges();
                        return RedirectToAction("Details", "Project", new { p_ID = project.Id });
                    }
                    else
                    {
                        ViewBag.Errormessage = "Sie haben keine Berechtigung, diese Task  zu löschen";
                    }
                }
                else
                {
                    ViewBag.Errormessage = "Keine Task mit der ID '" + t_ID + "' wurde gefunden.";
                }
            }
            else
            {
                ViewBag.Errormessage = "Unkorrekt Task ID";
            }

            //How to redirect to coming Link || it looks to work
            return RedirectToAction("Index", "Project");
        }

    }
}
