using System.Web.Mvc;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NoticesController : Controller
{
    [HttpGet]
    [Address(CarePages.Notices)]
    public ActionResult OfTheWards() => View("/Care/Shell/Notices.Net48.cshtml");
}
