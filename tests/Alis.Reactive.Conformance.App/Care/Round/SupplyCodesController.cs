using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SupplyCodesController : Controller
{
    private const string PlainText = "text/plain";
    private const string StockedCode = "A12";
    private const string CodeUnknown = "Unknown code.";

    [HttpGet(CarePages.SupplyCodes)]
    public IActionResult OfTheCupboard() => View("/Care/Round/SupplyCodes.cshtml", new SupplyCodes());

    [HttpGet(CarePages.SupplyCodesWithAFrameworkInput)]
    public IActionResult OfTheCupboardWithAFrameworkInput() => View("/Care/Round/SupplyCodesBound.cshtml", new SupplyCodes());

    [HttpPost(SupplyCodesEndpoints.Codes)]
    public IActionResult Submit([FromForm] SupplyCodes supply)
    {
        if (supply.Code == StockedCode) return Content($"Code {supply.Code} received", PlainText);
        ModelState.AddModelError("Code", CodeUnknown);
        return ValidationProblem(ModelState);
    }
}

public sealed class SupplyCodes
{
    public string? Code { get; set; }
}

public static class SupplyCodesEndpoints
{
    public const string Codes = CarePages.SupplyCodes + "/codes";
}
