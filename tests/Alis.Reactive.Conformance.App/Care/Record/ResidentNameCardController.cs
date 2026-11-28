using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentNameCardController : Controller
{
    [HttpGet(CarePages.ResidentNameCardWithMarkupInTheName)]
    public IActionResult WithMarkupInTheName() => View("/Care/Record/ResidentNameCard.cshtml", new ResidentNameCard("</script><b>Ada</b>", "4B"));

    [HttpGet(CarePages.ResidentNameCardWithAPlainName)]
    public IActionResult WithAPlainName() => View("/Care/Record/ResidentNameCard.cshtml", new ResidentNameCard("Ada Lee", "4B"));
}

public sealed record ResidentNameCard(string Name, string Room);

public static class ResidentNameCards
{
    public static readonly AppEvent ShowName = AppEvent.Declare();
    public static readonly AppEvent ShowRoom = AppEvent.Declare();
}
