using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Trays;

public sealed class TraysController : Controller
{
    public IActionResult Presence() => View(new TraysPage());
}

public sealed record TraysPage;
