using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class RefusalsController : Controller
{
    [HttpGet(RefusalPages.Index)]
    public IActionResult Index() => View("/Refusals/Index/Index.cshtml");
}
