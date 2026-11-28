using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class PluginDesksController : Controller
{
    [HttpGet(BenchPages.PluginsVisitorDesk)]
    public IActionResult VisitorDesk() => View("/Bench/Plugins/VisitorDesk.cshtml", new VisitorDeskBoard([DeskScripts.VisitorDesk]));

    [HttpGet(BenchPages.PluginsVisitorDeskOpenAtLoad)]
    public IActionResult VisitorDeskOpenAtLoad() => View("/Bench/Plugins/VisitorDesk.cshtml", new VisitorDeskBoard([DeskScripts.VisitorDeskOpenAtLoad]));

    [HttpGet(BenchPages.PluginsNightWard)]
    public IActionResult NightWard()
        => View("/Bench/Plugins/NightWard.cshtml", new NightWardBoard([DeskScripts.WardShiftLog, DeskScripts.AgencyShiftLog, DeskScripts.LanternAsANumber]));

    [HttpGet(BenchPages.PluginsNightWardOneLog)]
    public IActionResult NightWardOneLog() => View("/Bench/Plugins/NightWard.cshtml", new NightWardBoard([DeskScripts.WardShiftLog, DeskScripts.LanternOfAmber]));

    [HttpGet(BenchPages.PluginsNightDesk)]
    public IActionResult NightDesk() => View("/Bench/Plugins/NightDesk.cshtml", new NightDeskBoard([DeskScripts.NightDesk]));

    [HttpGet(BenchPages.PluginsNightDeskFaceFirst)]
    public IActionResult NightDeskFaceFirst() => View("/Bench/Plugins/NightDeskFaceFirst.cshtml", new NightDeskBoard([DeskScripts.NightDesk]));

    [HttpGet(BenchPages.PluginsShiftLog)]
    public IActionResult ShiftLogDesk()
        => View("/Bench/Plugins/ShiftLogDesk.cshtml", new ShiftLogDeskBoard([DeskScripts.ShiftLogDesk], [new ItemStart { Name = "Ada" }, new ItemStart { Name = "Bea" }]));
}

public interface IScriptedPage
{
    IReadOnlyList<string> Scripts { get; }
}

public sealed record VisitorDeskBoard(IReadOnlyList<string> Scripts) : IScriptedPage;

public sealed record NightWardBoard(IReadOnlyList<string> Scripts) : IScriptedPage;

public sealed record NightDeskBoard(IReadOnlyList<string> Scripts) : IScriptedPage;

public sealed record ShiftLogDeskBoard(IReadOnlyList<string> Scripts, IReadOnlyList<ItemStart> Residents) : IScriptedPage;

public static class DeskScripts
{
    private const string Root = "/js/bench/plugins/";

    public const string VisitorDesk = Root + "visitor-desk.js";

    public const string VisitorDeskOpenAtLoad = Root + "visitor-desk-open-at-load.js";

    public const string WardShiftLog = Root + "ward-shift-log.js";

    public const string AgencyShiftLog = Root + "agency-shift-log.js";

    public const string LanternAsANumber = Root + "lantern-as-a-number.js";

    public const string LanternOfAmber = Root + "lantern-of-amber.js";

    public const string NightDesk = Root + "night-desk.js";

    public const string ShiftLogDesk = Root + "shift-log-desk.js";
}

public static class VisitorDeskButtons
{
    public static readonly AppEvent CountTheVisits = AppEvent.Declare();

    public static readonly AppEvent GreetTheVisitor = AppEvent.Declare();
}

public static class NightWardButtons
{
    public static readonly AppEvent ReadTheLog = AppEvent.Declare();

    public static readonly AppEvent ReadTheLantern = AppEvent.Declare();

    public static readonly AppEvent StartTheRound = AppEvent.Declare();
}

public static class NightDeskButtons
{
    public static readonly AppEvent ReadTheHour = AppEvent.Declare();
}

public static class ShiftLogDeskButtons
{
    public static readonly AppEvent AddANote = AppEvent.Declare();

    public static readonly AppEvent SetTheEntriesToFive = AppEvent.Declare();

    public static readonly AppEvent CheckTheLog = AppEvent.Declare();

    public static readonly AppEvent AnswerTheItems = AppEvent.Declare();

    public static readonly AppEvent ReadTheInitials = AppEvent.Declare();
}
