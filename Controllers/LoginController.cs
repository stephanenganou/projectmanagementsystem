using PMSystem.DataAccess;
using System;
using PMSystem.Models;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using PMSystem.Utility;

namespace PMSystem.Controllers
{
    public class LoginController : Controller
    {
        private PMSystemDbContext context;
        public LoginController()
        {
            context = new PMSystemDbContext();
        }
        // GET: Login
        public ActionResult Index()
        {
            var user = User.Identity.Name;
            ViewBag.Currentuser = user;
            if (user.Contains("@"))
            {
                return RedirectToAction("Index", "Project");
            }
            else
            {
                return View();
            }
            
        }

        //This handle any post request from Index
        [HttpPost]
        public ActionResult Index(Models.Login request)
        {
            if (!ModelState.IsValid)
                return View(request);

            if (!TextUtil.checkIfEmpty(request.Username) && !TextUtil.checkIfEmpty(request.Password))
            {
                //First search in the DB
                var user = this.context.Users.FirstOrDefault(u => u.Email == request.Username && u.Passwort == request.Password);
                if (null != user)
                {
                    // "false" to say we don't want the cookie to be persistent, and delete after a log out.
                    FormsAuthentication.SetAuthCookie(user.Email, false);
                    return Redirect(FormsAuthentication.GetRedirectUrl(user.Email, false));
                }
            }
                  
            ViewBag.Errormessage = "Authentication Failure";
            return View(request);
        }
    }
}