using System.Web.Mvc;
using Alis.Reactive.Consumer.Net48.Models;

namespace Alis.Reactive.Consumer.Net48.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(new Welcome());
        }
    }
}
