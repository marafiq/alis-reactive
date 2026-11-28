using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class LinenCountController : Controller
{
    [HttpGet(CarePages.LinenCount)]
    public IActionResult OfTheWard() => View("/Care/BedBoard/LinenCount.cshtml", new LinenCount(8));
}

public sealed record LinenCount(int? Towels);

public static class LinenCounts
{
    public static readonly AppEvent AddPack = AppEvent.Declare();

    public static readonly AppEvent AddOne = AppEvent.Declare();

    public static readonly AppEvent TakePack = AppEvent.Declare();

    public static readonly AppEvent TakeOne = AppEvent.Declare();

    public static readonly AppEvent ThreePerBundle = AppEvent.Declare();

    public static readonly AppEvent FourPerBundle = AppEvent.Declare();

    public static readonly AppEvent ZeroDelivery = AppEvent.Declare();

    public static readonly AppEvent ZeroReturn = AppEvent.Declare();

    public static readonly AppEvent ProtectReserve = AppEvent.Declare();

    public static readonly AppEvent ChooseCart = AppEvent.Declare();

    public static readonly AppEvent EnforceCapacity = AppEvent.Declare();

    public static readonly AppEvent PermitDiscrepancy = AppEvent.Declare();
}
