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

        [HttpPost]
        public ActionResult Save(SmokeModel model)
        {
            var result = DependencyResolver.Current.GetService<SmokeModelRules>().Validate(model);
            if (!result.IsValid)
            {
                Response.StatusCode = 400;
                Response.TrySkipIisCustomErrors = true;
                return Json(new { errors = result.ErrorsByField });
            }
            return Json(new { saved = model.Name });
        }

        public ActionResult Day(DateTimeOffset startsAt)
        {
            return Content(string.Format(CultureInfo.InvariantCulture, "{0:dddd} plan starts at {0:h:mm tt} (UTC{0:zzz}).", startsAt));
        }
    }
}
