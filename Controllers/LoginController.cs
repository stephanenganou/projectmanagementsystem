using PMSystem.DataAccess;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace PMSystem.Controllers
{
    public class LoginController : Controller
    {
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
            if (!ModelState.IsValid) return View(request);

            using (var ctx = new PMSystemDbContext())
            {
                if (!String.IsNullOrEmpty(request.Username) && !String.IsNullOrEmpty(request.Password))
                {
                    //First search in the DB
                    var user = ctx.Users.FirstOrDefault(u => u.Email == request.Username && u.Passwort == request.Password);
                    if(null != user)
                    {
                        // "false" to say we don't want the cookie to be persistent, and delete after a log out.
                        FormsAuthentication.SetAuthCookie(user.Email, false);
                        return Redirect(FormsAuthentication.GetRedirectUrl(user.Email, false));
                    }
                }

                ViewBag.Failed = true;
                return View(request);
            }
        }
    }
}