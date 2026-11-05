using System;
using System.Globalization;
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

        public ActionResult Day(DateTimeOffset id)
        {
            return Content(string.Format(CultureInfo.InvariantCulture, "{0:dddd} plan starts at {0:h:mm tt} (UTC{0:zzz}).", id));
        }
    }
}
