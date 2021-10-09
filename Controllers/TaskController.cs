using PMSystem.DataAccess;
using PMSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using PMSystem.Utility;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace PMSystem.Controllers
{
    //it makes sure to check for a valide user before allowing access to this action(s)
    [Authorize]
    public class TaskController : Controller
    {
        private static string ADMIN_FEATURE = "admin@ymail.com";
        private PMSystemDbContext context;

        public TaskController()
        {
            context = new PMSystemDbContext();
        }

        //to get the registered user
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

            if (!TextUtil.checkIfEmpty(t_ID))
            {
                int id = int.Parse(t_ID);
                Task task = context.Tasks.FirstOrDefault(t => t.Id == id);
                if (task != null)
                {
                    
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
                    ViewBag.Errormessage = "Task with ID: '" + t_ID + "' was not found";
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
               
                int projectID = int.Parse(p_ID);
                if(projectID == 0 && TextUtil.checkIfEmpty(collection["ProjektID"]))
                {
                    ViewBag.Errormessage = "Incorrect Project ID;
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

                    if(project.Owner.Id != currentUser.Id && currentUser.Email != ADMIN_FEATURE)
                    {
                        ViewBag.Errormessage = "You have no authorization!";
                        return View();
                    }

                    Task task = new Task();
                    task.Name = collection["Name"];
                    task.Description = collection["Description"];
                    task.Status = float.Parse(collection["Status"]);
                    task.Project = project;

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

                    string t_ID = task.Id.ToString();

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

            if (!TextUtil.checkIfEmpty(t_ID))
            {
                int id = int.Parse(t_ID);
                Task task = (from t in context.Tasks where t.Id == id select t).FirstOrDefault();
                ViewBag.ProjectID = p_ID;
                ViewBag.Users = (from us in context.Users select us).ToList();

                return View(task);
            }

            ViewBag.Errormessage = "The task with ID '" + t_ID + "' does not exist.";
            return View();
        }

        // POST: Task/Edit/5
        [HttpPost]
        public ActionResult Edit(string p_ID, string t_ID, FormCollection collection)
        {
            try
            {
                
                int id = int.Parse(t_ID);
                if (id == 0)
                {
                    ViewBag.Errormessage = "Incorrect Task ID";
                    return View(collection);
                }

                if (ModelState.IsValid)
                {
                    Task task = context.Tasks.FirstOrDefault(t => t.Id == id);
                    if(task != null)
                    {
                        User currentUser = getCurrentUser();
                        //Only Admin or Project Owner can edit the Task
                        if (currentUser.Email == ADMIN_FEATURE || currentUser.Id == task.Project.Owner.Id)
                        {
                            task.Name = collection["Name"];
                            task.Description = collection["Description"];
                            task.Status = float.Parse(collection["Status"]);

                            // Update the 'Project' and 'Subtask' status.
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


                        ViewBag.Errormessage = "You do not have authorization to perform this action.";
                        return View(collection);
                    }
                    else
                    {
                        ViewBag.Errormessage = "No subtask with ID '" + t_ID + "' was found.";
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
            if (!TextUtil.checkIfEmpty(t_ID))
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

                    if (project.Owner.Id == currentUser.Id || currentUser.Email == ADMIN_FEATURE)
                    {
                        //deleting all realated SubTasks
                        foreach(SubTask subTask in task.SubTasks)
                        {
                            context.SubTasks.Remove(subTask);
                        }

                        //Update the 'Project'
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
                        ViewBag.Errormessage = "You do not have permission to delete this task";
                    }
                }
                else
                {
                    ViewBag.Errormessage = "No task with ID '" + t_ID + "' was found.";
                }
            }
            else
            {
                ViewBag.Errormessage = "Incorrect Task ID";
            }

            //How to redirect to coming Link || it looks to work
            return RedirectToAction("Index", "Project");
        }

    }
}
