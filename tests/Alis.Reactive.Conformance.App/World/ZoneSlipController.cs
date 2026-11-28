using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.World;

public sealed class ZoneSlipController : Controller
{
    private const string Script = "/js/world/zone-slip.js";

    [HttpGet(WorldPages.ZoneSlip)]
    public ContentResult Slip()
        => Content(WorldArea.Page("The zone slip", $"<p id=\"local-clock\"></p><script src=\"{Script}\"></script>"), WorldArea.Html);
}
