using System.Web.Mvc;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class TheWorldsNoticeController : Controller
{
    [HttpGet]
    [Address(CarePages.TheWorldsNotice)]
    public ActionResult OfThisWorld() => View("/Care/Shell/TheWorldsNotice.Net48.cshtml", new WorldsNotice(TwinWorlds.KeyOf(HttpContext)));
}

public sealed record WorldsNotice(string World);
