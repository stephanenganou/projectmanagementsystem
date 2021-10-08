using PMSystem.DataAccess;
using PMSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using PMSystem.Utility;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Routing.Constraints;

namespace PMSystem.Controllers
{
    [Authorize] //it tells, checks for a valide user before allowing access to this action(s)
    public class SubTaskController : Controller
    {
        private static string ADMIN_FEATURE = "admin@ymail.com";
        private PMSystemDbContext context;

        public SubTaskController()
        {
            context = new PMSystemDbContext();
        }

        //um den angemeldeten User zu bekommen
        public User getCurrentUser()
        {
            return context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
        }

        // GET: SubTask
        public ActionResult Index()
        {
            User currentUser = getCurrentUser();
            ViewBag.Subtasks = context.SubTasks.ToList();
            ViewBag.Currentuser = currentUser.Email;
            //ViewBag.Subtasks = (from sts in context.SubTasks where sts.User.Id == getByID(User.Identity.Name).Id select sts).ToList();
            return View(from sts in context.SubTasks where sts.User.Id == currentUser.Id select sts);
        }

        // GET: SubTask/Details/subtask_ID
        public ActionResult Details(string p_ID, string s_ID)
        {
            ViewBag.Currentuser = User.Identity.Name;

            if (!TextUtil.checkIfEmpty(s_ID))
            {
                int id = int.Parse(s_ID);
                int pid = int.Parse(p_ID);
                ViewBag.ProjectID = p_ID;
                SubTask subtask = context.SubTasks.FirstOrDefault(u => u.Id == id);
                if(subtask != null)
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
                        allSubtasks.AddRange(task_iterator.SubTasks);
                    }
                    foreach (SubTask subtask_iterator in allSubtasks)
                    {
                        if (subtask_iterator.Id == id)
                        {
                            ViewBag.Subtask = subtask_iterator;
                        }
                    }
                    return View(subtask);
                }
            }
            else
            {
                ViewBag.Errormessagee = "Subtask mit der ID '" + s_ID + "' wurde nicht gefunden.";
            }

            return RedirectToAction("Details", "SubTask", new { p_ID = p_ID, s_ID = s_ID });
        }

        // GET: SubTask/Create/project_ID/task_ID
        public ActionResult Create(string p_ID, string t_ID)
        {
            ViewBag.Currentuser = User.Identity.Name;

            ViewBag.TaskID = t_ID;
            ViewBag.ProjectID = p_ID;
            ViewBag.Users = (from us in context.Users select us).ToList();
            List<SelectListItem> possibleUsersDropdown = new List<SelectListItem>();
            foreach (User user in ViewBag.Users)
            {
                possibleUsersDropdown.Add(new SelectListItem { Text = user.Vorname + " " + user.Nachname, Value = user.Id.ToString() });
            }
            ViewBag.UsersSelectList = possibleUsersDropdown;
            return View();
        }

        // POST: SubTask/Create/project_ID/task_ID
        [HttpPost]
        public ActionResult Create(string p_ID, string t_ID, FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here
                int taskID = int.Parse(t_ID);
                int projectID = int.Parse(p_ID);
                //Debug.WriteLine("TaskID is: " + taskID);
                if (taskID == 0 || projectID == 0)
                {
                    ViewBag.Errormessage = "Unkorrekt Task ID und/oder Projekt ID";
                    return RedirectToAction("Index", "Project");
                }

                if (ModelState.IsValid)
                {
                    User currentUser = getCurrentUser();
                    Task task = context.Tasks.FirstOrDefault(u => u.Id == taskID);
                    Project project = context.Projects.FirstOrDefault(p => p.Id == projectID);

                    if (currentUser.Email != ADMIN_FEATURE && currentUser.Id != project.Owner.Id)
                    {
                        ViewBag.Errormessage = "Sie haben keine Berechtigung";
                        return RedirectToAction("Index", "Project");
                    }

                    SubTask subtask = new SubTask();
                    subtask.Name = collection ["Name"];
                    subtask.Description = collection["Description"];
                    subtask.Duration = int.Parse(collection["Duration"]);
                    subtask.Start = DateTime.Parse(collection["Start"]);
                    subtask.End = DateTime.Parse(collection["End"]);
                    subtask.Status = float.Parse(collection["Status"]);
                    subtask.Task = task;
                    
                    if(TextUtil.checkIfEmpty(collection["User"]))
                    {
                        subtask.User = currentUser;
                    }
                    else
                    {
                        int assignedUser_ID = int.Parse(collection["User"]);

                        User user = context.Users.FirstOrDefault(u => u.Id == assignedUser_ID);
                        subtask.User = user;
                    }
                    
                    //Aktualisierung des 'Projekt' und 'Task' status.
                    float tSubtask_status = 0;
                    float tTask_Status = 0;
                    float total_task = project.Tasks.Count();
                    int total_subtask = task.SubTasks.Count() + 1;

                    foreach(SubTask s_subtask in task.SubTasks)
                    {
                        tSubtask_status = tSubtask_status + s_subtask.Status;
                    }

                    tSubtask_status = tSubtask_status + float.Parse(collection["Status"]);
                    task.Status = tSubtask_status / total_subtask;

                    foreach(Task t_task in project.Tasks)
                    {
                        if(task.Id != t_task.Id)
                        {
                            tTask_Status = tTask_Status + t_task.Status;
                        } 
                    }

                    tTask_Status = tTask_Status + (tSubtask_status / total_subtask);
                    project.Status = tTask_Status / total_task;

                    context.SubTasks.Add(subtask);
                    task.SubTasks.Add(subtask);
                    context.Tasks.AddOrUpdate(task);
                    context.Projects.AddOrUpdate(project);
                    
                    context.SaveChanges();

                    return RedirectToAction("Create", "Task", new { p_ID = p_ID });
                    //return RedirectToAction("Index");
                }

                ViewBag.Errormessage = "Invalid Input Data.";
                return RedirectToAction("Index", "Project");
            }
            catch
            {
                ViewBag.Errormessage = "Ein Fehler ist aufgetreten";
                return RedirectToAction("Index", "Project");
            }
        }

        // GET: SubTask/Edit/subtask_ID
        public ActionResult Edit(string p_ID, string s_ID)
        {
            ViewBag.Currentuser = User.Identity.Name;

            if (!TextUtil.checkIfEmpty(s_ID))
            {
                int id = int.Parse(s_ID);
                SubTask subtask = context.SubTasks.FirstOrDefault(u => u.Id == id);

                if (subtask != null)
                {
                    ViewBag.Users = (from us in context.Users select us).ToList();
                    List<SelectListItem> possibleUsersDropdown = new List<SelectListItem>();
                    foreach (User user in ViewBag.Users)
                    {
                        possibleUsersDropdown.Add(new SelectListItem { Text = user.Vorname + " " + user.Nachname, Value = user.Id.ToString() });
                    }
                    ViewBag.ProjectID = p_ID;
                    ViewBag.UsersSelectList = possibleUsersDropdown;
                    return View(subtask);
                }
            }
            else
            {
                ViewBag.Errormessage = "SubTask mit der ID '" + s_ID + "' wurde nicht gefunden";
            }

            return RedirectToAction("Details", "SubTask", new { p_ID = p_ID, s_ID = s_ID });
        }

        // POST: SubTask/Edit/subtask_ID
        [HttpPost]
        public ActionResult Edit(string p_ID, string s_ID, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
                int id = int.Parse(s_ID);
                if (id == 0 && collection["ID"] == "")
                {
                    ViewBag.Errormessage = "Unkorrekt ID ";
                    return RedirectToAction("Index", "Project");
                }
                if (ModelState.IsValid)
                {
                    SubTask subtask = context.SubTasks.FirstOrDefault(s => s.Id == id);
                    List<SelectListItem> possibleUsersDropdown = new List<SelectListItem>();
                    foreach (User user in context.Users)
                    {
                        possibleUsersDropdown.Add(new SelectListItem { Text = user.Vorname + " " + user.Nachname, Value = user.Id.ToString() });
                    }
                    ViewBag.UsersSelectList = possibleUsersDropdown;
                    User currentUser = getCurrentUser();

                    if (subtask != null)
                    {
                        int taskID = 0;
                        var taskquery =
                            from s in context.SubTasks
                            where s.Id == subtask.Id
                            select new { taskID = s.Task.Id };

                        foreach (var val in taskquery)
                        {
                            taskID = val.taskID;
                        }

                        Task task = context.Tasks.FirstOrDefault(t => t.Id == taskID);
                        float tSubtask_status = 0;
                        float tTask_Status = 0;

                        int projectID = 0;
                        var projectquery =
                            from t in context.Tasks
                            where t.Id == task.Id
                            select new { projectID = t.Project.Id };
                        foreach (var val in projectquery)
                        {
                            projectID = val.projectID;
                        }

                        Project project = context.Projects.FirstOrDefault(p => p.Id == projectID);

                        if (currentUser.Email == ADMIN_FEATURE || currentUser.Id == project.Owner.Id || currentUser.Id == subtask.User.Id)
                        {
                            subtask.Name = collection["Name"];
                            subtask.Description = collection["Description"];
                            subtask.Status = float.Parse(collection["Status"]);
                            subtask.Duration = int.Parse(collection["Duration"]);
                            subtask.Start = DateTime.Parse(collection["Start"]);
                            subtask.End = DateTime.Parse(collection["End"]);
                            //Assign Users
                            if (TextUtil.checkIfEmpty(collection["User"]))
                            {
                                //Wir machen nix
                            }
                            else
                            {
                                int uID = int.Parse(collection["User"]);
                                User user = context.Users.FirstOrDefault(u => u.Id == uID);
                                subtask.User = user;
                            }
                            context.SubTasks.AddOrUpdate(subtask);

                            //Aktualisierung des 'Projekt' und 'Subtask' status.
                            
                            //ViewBag.ProjectID = projectID;

                            ViewBag.ProjectID = project.Id;
                            float total_task = project.Tasks.Count();
                            int total_subtask = task.SubTasks.Count();
                            foreach (SubTask s_subtask in task.SubTasks)
                            {
                                if(s_subtask.Id != subtask.Id)
                                {
                                    tSubtask_status = tSubtask_status + s_subtask.Status;
                                }
                            }
                            tSubtask_status = tSubtask_status + float.Parse(collection["Status"]);
                            task.Status = tSubtask_status / total_subtask;

                            foreach (Task t_task in project.Tasks)
                            {
                                if (task.Id != t_task.Id)
                                {
                                    tTask_Status = tTask_Status + t_task.Status;
                                }
                            }
                            tTask_Status = tTask_Status + (tSubtask_status / total_subtask);
                            project.Status = tTask_Status / total_task;

                            context.SubTasks.AddOrUpdate(subtask);
                            context.Tasks.AddOrUpdate(task);
                            context.Projects.AddOrUpdate(project);

                            context.SaveChanges();

                            //how to return to the previous origin page
                            return RedirectToAction("Details", "SubTask", new { p_ID = project.Id, s_ID = id });
                        }
                        else
                        {
                            ViewBag.Errormessage = "Sie haben keine Berechtigung.";
                        }
                    }
                    else
                    {
                        ViewBag.Errormessage = "Keine Subtask mit der ID '" + s_ID + "' wurde gefunden.";
                    }
                    return RedirectToAction("Index", "Project");
                }
                return RedirectToAction("Index", "Project");
            }
            catch
            {
                return RedirectToAction("Index", "Project");
            }
        }

        // GET: SubTask/Delete/subtask_ID
        public ActionResult Delete(string s_ID)
        {
            if (!TextUtil.checkIfEmpty(s_ID))
            {
                int id = int.Parse(s_ID);

                User currentUser = getCurrentUser();

                SubTask subTask = (from s in context.SubTasks where s.Id == id select s).FirstOrDefault();

                if (subTask != null)
                {
                    int taskID = 0;
                    int projectID = 0;

                    var query =
                        from s in context.SubTasks
                        join t in context.Tasks on s.Task.Id equals t.Id
                        where s.Id == subTask.Id
                        select new { taskID = s.Task.Id, projectID = t.Project.Id };

                    foreach (var val in query)
                    {
                        taskID = val.taskID;
                        projectID = val.projectID;
                    }

                    Task task = context.Tasks.FirstOrDefault(t => t.Id == taskID);
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
                            foreach (SubTask subtask_iterator in task_iterator.SubTasks)
                            {
                                if (subtask_iterator.Id == id)
                                {
                                    project = project_iterator;
                                }
                            }
                        }
                    }

                    if (project.Owner.Id == currentUser.Id || currentUser.Email == ADMIN_FEATURE)
                    {

                        //Aktualisierung des 'Projekt' und 'Subtask' status.
                        float tSubtask_status = 0;
                        float tTask_Status = 0;

                        foreach (SubTask s_subtask in task.SubTasks)
                        {
                            if (s_subtask.Id != subTask.Id)
                            {
                                tSubtask_status = tSubtask_status + s_subtask.Status;
                            }
                        }

                        float total_task = project.Tasks.Count();
                        int total_subtask = task.SubTasks.Count() - 1;

                        foreach (Task t_task in project.Tasks)
                        {
                            if (task.Id != t_task.Id)
                            {
                                tTask_Status = tTask_Status + t_task.Status;
                            }
                        }

                        if (total_subtask > 0)
                        {
                            task.Status = tSubtask_status / total_subtask;
                            tTask_Status = tTask_Status + (tSubtask_status / total_subtask);
                        }
                        else
                        {
                            task.Status = 0;
                        }

                        if(total_task > 0)
                        {
                            project.Status = tTask_Status / total_task;
                        }
                        else
                        {
                            project.Status = 0;
                        }

                        task.SubTasks.Remove(subTask);
                        context.SubTasks.Remove(subTask);
                        context.Tasks.AddOrUpdate(task);
                        context.Projects.AddOrUpdate(project);

                        context.SaveChanges();
                        return RedirectToAction("Details", "Task", new { p_ID = project.Id, t_ID = task.Id });
                    }
                    else
                    {
                        ViewBag.Errormessage = "Sie haben keine Berechtigung, die Subtask zu löschen";
                    }
                }
                else
                {
                    ViewBag.Errormessage = "Keine Subtask mit der ID '" + s_ID + "' wurde gefunden.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Unkorrekt Subtask ID";
            }

            return RedirectToAction("Index", "Project");
        }

    }
}
