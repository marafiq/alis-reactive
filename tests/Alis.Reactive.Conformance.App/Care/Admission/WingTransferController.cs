using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WingTransferController : Controller
{
    [HttpGet(CarePages.WingTransfer)]
    public IActionResult ForAResident() => View("/Care/Admission/WingTransfer.cshtml", new WingTransfer { Wing = "Rose" });
}

public sealed class WingTransfer
{
    public string? Wing { get; set; }
}

public static class WingTransfers
{
    public static readonly AppEvent TransferToOak = AppEvent.Declare();
}
