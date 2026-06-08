using System.Web.Mvc;
using Net48MvcSmoke.Models;

namespace Net48MvcSmoke.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(new SmokeModel());
        }
    }
}
