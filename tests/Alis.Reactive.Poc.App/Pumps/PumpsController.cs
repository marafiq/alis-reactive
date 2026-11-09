using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Pumps;

public sealed class PumpsController : Controller
{
    public IActionResult Infusion() => View(new InfusionPage());

    public IActionResult PausedInALambda() => View(new InfusionPage());

    public IActionResult Checks() => View(new InfusionPage());

    public IActionResult SelfCheckedInALambda() => View(new InfusionPage());

    public IActionResult RecalibratedInALambda() => View(new InfusionPage());

    public IActionResult SummaryReadByAnotherPipeline() => View(new InfusionPage());

    public IActionResult ChannelReadByAnotherPipeline() => View(new InfusionPage());

    public IActionResult ChannelsReadByAnotherPipeline() => View(new InfusionPage());

    public IActionResult NoteKeptOutOfItsBranch() => View(new InfusionPage());

    public IActionResult ChannelKeptOutOfItsBranch() => View(new InfusionPage());

    public IActionResult ChannelKeptOutOfItsLambda() => View(new InfusionPage());

    public IActionResult LastAlarmNoted() => View(new InfusionPage());

    public IActionResult CaptionedBySummary() => View(new InfusionPage());

    public IActionResult PumpCard() => View(new InfusionPage());
}

public sealed record InfusionPage;
