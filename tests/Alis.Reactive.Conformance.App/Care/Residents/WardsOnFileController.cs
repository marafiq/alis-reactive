using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardsOnFileController : Controller
{
    [HttpGet(CarePages.WardsOnFile)]
    public IActionResult OnFile() => View("/Care/Residents/WardsOnFile.cshtml");

    [HttpGet(CarePages.WardOnFile)]
    public IActionResult OfAWard(WardOnFile page) => View("/Care/Residents/WardOnFile.cshtml", page);
}

public sealed class WardOnFile
{
    public string Ward { get; set; } = "";
}
