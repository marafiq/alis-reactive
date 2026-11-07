using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Discovery;

public sealed class DiscoveryController : Controller
{
    public IActionResult RuntimeFirst() => View(new WardChoice { Ward = "South" });
}

public sealed class WardChoice
{
    public string? Ward { get; set; }
}
