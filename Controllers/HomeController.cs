using PMSystem.DataAccess;
using PMSystem.Models;
using System.Linq;
using System.Web.Mvc;

namespace PMSystem.Controllers
{
    public class HomeController : Controller
    {
        private PMSystemDbContext context;

        public HomeController()
        {
            this.context = new PMSystemDbContext();
        }
        public User getCurrentUser()
        {
            return context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
        }
        public ActionResult Index()
        {
            ViewBag.Currentuser = User.Identity.Name;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Currentuser = User.Identity.Name;
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Currentuser = User.Identity.Name;
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}