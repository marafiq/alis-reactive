using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentAccountController : Controller
{
    [HttpGet(CarePages.ResidentAccount)]
    public IActionResult OfTheResident() => View("/Care/Record/ResidentAccount.cshtml", new ResidentAccount("Ada Hart", 1234.5m));

    [HttpGet(ResidentAccountEndpoints.Balance)]
    public IActionResult Balance() => Json(new AccountBalance(1234.5m));
}

public sealed record ResidentAccount(string Resident, decimal Balance);

public sealed record AccountBalance(decimal Balance);

public static class ResidentAccountEndpoints
{
    public const string Balance = CarePages.ResidentAccount + "/balance";
}

public static class ResidentAccountButtons
{
    public static readonly AppEvent ShowTheBalance = AppEvent.Declare();
}
