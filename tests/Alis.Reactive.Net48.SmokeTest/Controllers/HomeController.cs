using System.Web.Mvc;

namespace Alis.Reactive.Net48.SmokeTest.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(new Models.SmokeTestModel());
        }

        public ActionResult DrawerContent()
        {
            return PartialView("_DrawerContent");
        }
    }
}
