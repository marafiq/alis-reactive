using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardTransferController : Controller
{
    [HttpGet(CarePages.WardTransfer)]
    public IActionResult OfAResident() => View("/Care/Record/WardTransfer.cshtml", new WardTransfer("", new TransferDestination("")));
}

public sealed record WardTransfer(string Name, TransferDestination Destination);

public sealed record TransferDestination(string Ward);

public sealed class TransferOrder
{
    public string Name { get; init; } = "";

    public OrderDestination Destination { get; init; } = new();

    public string Reason { get; init; } = "";
}

public sealed class OrderDestination
{
    public string Ward { get; init; } = "";
}

public static class WardTransfers
{
    public static readonly AppEvent Send = AppEvent.Declare();

    public static readonly AppEvent<TransferOrder> Transfer = AppEvent.Declare<TransferOrder>();
}
